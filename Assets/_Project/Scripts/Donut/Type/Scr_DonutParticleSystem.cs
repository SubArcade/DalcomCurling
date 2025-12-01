using System;
using System.Xml.Serialization;
using UnityEngine;

public class Scr_DonutParticleSystem : MonoBehaviour
{
    public enum AuraType { None, Fire, Light, Magic, Ice, Electric }
    public enum TrailType { None, FireTail, LightTail, MagicTail, IceTail, ElectricTail }

    [Header("오라 파티클")]
    [SerializeField] private bool auraEnabled = false;
    [SerializeField] private AuraType selectedAura = AuraType.None;
    [SerializeField] private GameObject auraFirePrefab;
    [SerializeField] private GameObject auraLightPrefab;
    [SerializeField] private GameObject auraMagicPrefab;
    [SerializeField] private GameObject auraIcePrefab;
    [SerializeField] private GameObject auraElectricPrefab;

    [Header("꼬리 파티클")]
    [SerializeField] private bool trailEnabled = false;
    [SerializeField] private TrailType selectedTrail = TrailType.None;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private float minSpeedForTrail = 1.0f;
    [SerializeField] private GameObject trailFirePrefab;
    [SerializeField] private GameObject trailLightPrefab;
    [SerializeField] private GameObject trailMagicPrefab;
    [SerializeField] private GameObject trailIcePrefab;
    [SerializeField] private GameObject trailElectricPrefab;

    [Header("충돌 파티클")]
    [SerializeField] private GameObject CrashParticlePrefab;
    [SerializeField] private float CrashParticleDuation = 2.0f; // 충돌 파티클 지속시간 -> 필요없을시 삭제하기
                                                                // 어차피 파티클 Looping 끈상태라서 필요없을것 같음
    [SerializeField] private string targetLayerName = "Donut";
    [SerializeField] private float minCrashForce = 1.0f; // 충돌에 필요한 최소값

    private GameObject currentCollisionParticle;
    private GameObject currentAuraParticle;
    private GameObject currentTrailParticle;
    private Rigidbody donutRigidbody;
    private bool wasMovingFast = false;
    private int targetLayer;
    private bool initialized = false;
    private bool attackFinished = false;
    
    // 오라 파티클 프로퍼티
    public bool IsAuraEnabled => auraEnabled;
    public AuraType CurrentAuraType => selectedAura;

    // 꼬리 파티클 프로퍼티
    public bool IsTrailEnabled => trailEnabled;
    public TrailType CurrentTrailType => selectedTrail;
    
    // 파티클 시스템 컴포넌트는 오브젝트의 자식으로 이미 생성되어 있어야 합니다.
    public ParticleSystem trailParticleSystem;

    private Rigidbody rb;
    private ParticleSystem.ShapeModule shapeModule;
    private float colliderRadius;
    private CapsuleCollider capsuleCollider;
    private Vector3 originalScale;
    private Vector3 newScale;
    
    //fixedUpdate 내부에서 변수 생성을 하지 않도록 미리 생성한 변수
    private Vector3 velocity;
    private float sqrVelocity;
    private Vector3 direction;
    private Quaternion targetRotation;
    private float scaleValue_Y;
    private GameObject particleSystemParent;

    // 파티클 시스템의 로컬 X축 회전 보정을 위한 상수 (90도 회전 필요)
    private readonly Quaternion rotationOffset = Quaternion.Euler(0f, 0f, 0f);

    private void Awake()
    {
        donutRigidbody = GetComponent<Rigidbody>();
        //trailRenderer = GetComponent<TrailRenderer>();
        //trailRenderer.enabled = false;
        capsuleCollider = transform.GetComponent<CapsuleCollider>();
        colliderRadius = capsuleCollider.radius * transform.localScale.x;
        


        //InitializeDonutParticles(StoneForceController_Firebase.Team.A);
    }
    // private void Start()
    // {
    //     donutRigidbody = GetComponent<Rigidbody>();
    //
    //     targetLayer = LayerMask.NameToLayer(targetLayerName);
    //     if (targetLayer == -1)
    //     {
    //         Debug.LogWarning($"Layer'{targetLayerName}'를 찾을 수 없습니다");
    //     }
    //     
    // }
    
     // private void Update()
     // {
     //     if (initialized && !attackFinished)
     //     {
     //         UpdateTrailBasedOnSpeed();
     //         UpdateTrailDirection();
     //     }
     // }
     
     void Update()
     {
         if (initialized)
         {
             // 1. 현재 속도 벡터를 가져옵니다.
             velocity = donutRigidbody.velocity;
             sqrVelocity = velocity.sqrMagnitude;
             //trailParticleSystem.transform.position = donutRigidbody.position;
             // 스톤이 멈췄는지 확인하는 임계값 (매우 작은 값)
             if (velocity.magnitude < 0.01f)
             {
                 if (trailParticleSystem.isPlaying)
                 {
                     trailParticleSystem.Stop();
                 }

                 return;
             }

             // 2. 파티클 재생 상태 확인 및 시작
             if (!trailParticleSystem.isPlaying)
             {
                 trailParticleSystem.Play();
             }

             // 3. 이동 방향의 반대 벡터를 구합니다.
             direction = -velocity.normalized;

             // 4. LookRotation을 사용하여 반대 방향을 바라보는 회전값을 계산합니다.
             //    (LookRotation은 기본적으로 Z축을 바라보게 합니다.)
             targetRotation = Quaternion.LookRotation(direction);

             // 5. 파티클 시스템의 로컬 X축 90도 오프셋을 적용합니다.
             //    (파티클 시스템의 로컬 축을 원하는 방향으로 정렬)
             targetRotation *= rotationOffset;

             // 6. Shape 모듈의 회전을 업데이트하여 입자가 정확히 반대 방향으로 방출되도록 합니다.

             
             if (sqrVelocity > 0.01)
             {
                 if (selectedTrail == TrailType.LightTail || selectedTrail == TrailType.MagicTail)
                 {
                     trailParticleSystem.transform.localScale = new Vector3(originalScale.x,
                         originalScale.y, originalScale.z + sqrVelocity * 0.05f);
                 }
                 else
                 {
                     trailParticleSystem.transform.localScale = new Vector3(originalScale.x,
                         originalScale.y + sqrVelocity * 0.05f, originalScale.z);
                 }
             }
             else
             {
                 //trailParticleSystem.transform.localScale = new Vector3(0, 0, 0);
                 trailParticleSystem.Stop();
             }
             
             //trailParticleSystem.transform.position = capsuleCollider.bounds.center + direction * colliderRadius;
             particleSystemParent.transform.position = capsuleCollider.bounds.center + direction * colliderRadius;
             // trailParticleSystem.transform.localScale = new Vector3(originalScale.x, 
             //     originalScale.y + donutRigidbody.velocity.sqrMagnitude * -0.05f, originalScale.z);
             //trailParticleSystem.transform.rotation = targetRotation;
             particleSystemParent.transform.rotation = targetRotation;
             //shapeModule.rotation = targetRotation.eulerAngles;
         }
     }

    public void InitializeDonutParticles(StoneForceController_Firebase.Team team) // StoneManager에서 도넛이 생성될때 미리 파티클들을 설정하는 함수
    {
        if (team == StoneForceController_Firebase.Team.A)
        {
            selectedAura = AuraType.Fire;
            //selectedTrail = TrailType.FireTail;
            selectedTrail = TrailType.MagicTail;
            trailEnabled = true;
            initialized = true;
        }
        else if (team == StoneForceController_Firebase.Team.B)
        {
            selectedAura = AuraType.Ice;
            //selectedTrail = TrailType.IceTail;
            selectedTrail = TrailType.LightTail;
            trailEnabled = true;
            initialized = true;
        }
        ApplyAuraSettings(); //오라 설정
        CreateCollisionParticleToChildren(); // 충돌 파티클 미리 만들어놓기
        particleSystemParent = Instantiate(GetTrailPrefab(selectedTrail), transform.position, rotationOffset);
        trailParticleSystem = particleSystemParent.transform.GetChild(0).GetComponent<ParticleSystem>();
        if (trailParticleSystem == null)
        {
            Debug.LogError("파티클 시스템을 인스펙터에 연결해주세요!");
            return;
        }

        // Shape 모듈을 미리 캐시해둡니다.
        shapeModule = trailParticleSystem.shape;
        
        // 파티클을 오브젝트의 로컬 축 기준으로 날아가게 설정 (필수)
        var main = trailParticleSystem.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        
        // 초기에는 파티클을 멈춰둡니다.
        trailParticleSystem.Stop();
        originalScale = trailParticleSystem.transform.localScale;
    }

    // 충돌 파티클에 관한 부분 스크립트 작성구간
    // private void OnCollisionEnter(Collision collision)
    // {
    //     // 지정된 레이어만 충돌하게 만들기
    //     if (targetLayer != -1 && collision.gameObject.layer != targetLayer) return;
    //
    //     // 충돌에 필요한 최소 힘 설정하기
    //     float collisionForce = collision.relativeVelocity.magnitude;
    //     if (collisionForce < minCrashForce) return;
    //
    //     // 충돌 지점에 파티클 생성하기 (2)
    //     CreateCollisionParticle(collision);
    //
    //     Debug.Log($"도넛이 {collision.gameObject.name}이랑 {collisionForce}의 힘으로 충돌했습니다");
    // }

    private void CreateCollisionParticleToChildren() // 이 게임에 쓰일 충돌 파티클을 미리 생성해둠
    {
        currentCollisionParticle = Instantiate(CrashParticlePrefab, transform, true);
    }

    public void PlayCollisionParticle(Collision collision) // 충돌시 StoneForceController_Firebase에서 호출할 함수. 파티클을 재생시킴
    {
        ContactPoint contact = collision.contacts[0];
        Vector3 collisionPoint = contact.point;
        Quaternion collisionRotation = Quaternion.LookRotation(contact.normal);
        currentCollisionParticle.transform.position = collisionPoint;
        currentCollisionParticle.transform.rotation = collisionRotation;
        currentCollisionParticle.GetComponent<ParticleSystem>().Play();
    }

    private void CreateCollisionParticle(Collision collision)
    {
        if (CrashParticlePrefab == null)
        {
            Debug.LogWarning("충돌 파티클이 설정되어있지않습니다"); 
            return;
        }

        // 충돌 지점에 파티클 생성하기 (1)
        ContactPoint contact = collision.contacts[0];
        Vector3 collisionPoint = contact.point;
        Quaternion collisionRotation = Quaternion.LookRotation(contact.normal);

        GameObject collisionParticle = Instantiate(
            CrashParticlePrefab,
            collisionPoint,
            collisionRotation
            );

        Destroy(collisionParticle, CrashParticleDuation);
    }

    // 충돌 파티클 메서드 설정하기

    public void SetCollisionParticlePrefab(GameObject newPrefab)
    {
        CrashParticlePrefab = newPrefab;
    }

    // 충돌 파티클 생성하기 테스트용으로 임시로 생성함 테스트후에 삭제하기
    public void TestCreateCollisionParticle(Vector3 position)
    {
        if (CrashParticlePrefab != null)
        {
            GameObject testParticle = Instantiate(
                CrashParticlePrefab,
                position,
                Quaternion.identity
                );
            Destroy(testParticle, CrashParticleDuation);
        }
    }

    private void UpdateTrailBasedOnSpeed()
    {
        float currentSpeed = GetDonutSpeed();
        bool isMovingFast = currentSpeed >= minSpeedForTrail;

        if (trailRenderer != null)
        {
            trailRenderer.emitting = isMovingFast && trailEnabled;
        }

        if (trailEnabled)
        {
            if(isMovingFast != wasMovingFast) //isMovingFast 현재 움직이고 있는 속도
            {
                if (isMovingFast)
                {
                    CreateTrailParticle(selectedTrail);
                }
                else
                {
                    DestroyCurrentTrailParticle();
                }
                wasMovingFast = isMovingFast;
            }
        }
        else
        {
            if (currentTrailParticle != null)
            {
                DestroyCurrentTrailParticle();
                wasMovingFast = false;
            }
        }
    }

    // 도넛 속도 계산
    private float GetDonutSpeed()
    {
        if (donutRigidbody != null)
        {
            //return donutRigidbody.velocity.magnitude;
            return donutRigidbody.velocity.sqrMagnitude;
        }
        return 0f;
    }

    // 꼬리 파티클 생성
    private void CreateTrailParticle(TrailType type)
    {
        if (type == TrailType.None) return;

        GameObject prefab = GetTrailPrefab(type);
        if (prefab != null)
        {
            currentTrailParticle = Instantiate(prefab, transform.position, Quaternion.identity, transform);

            // Particle System 설정
            ParticleSystem ps = currentTrailParticle.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startSpeed = 0.3f;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.startLifetime = 0.5f;
                
                var velocity = ps.velocityOverLifetime;
                velocity.enabled = true;
                velocity.space = ParticleSystemSimulationSpace.World;

                // 이동방향 반대로 입자 발사
                velocity.x = new ParticleSystem.MinMaxCurve(-2f, -5f);
                velocity.y = new ParticleSystem.MinMaxCurve(-1f, 1f);
                velocity.z = new ParticleSystem.MinMaxCurve(-1f, 1f);
            }

            Debug.Log("혜성 꼬리 파티클 경로에 따라서 생성하기");
        }
    }

    // 꼬리 파티클 제거
    private void DestroyCurrentTrailParticle()
    {
        if (currentTrailParticle != null)
        {
            DestroyImmediate(currentTrailParticle);
            currentTrailParticle = null;
            Debug.Log("꼬리 파티클 제거: 속도 부족");
        }
    }

    // 꼬리 방향 업데이트 - 이동 경로를 따라가는 혜성 꼬리 효과
    private void UpdateTrailDirection()
    {
        if (currentTrailParticle != null && donutRigidbody != null)
        {
            ParticleSystem ps = currentTrailParticle.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var velocity = ps.velocityOverLifetime;

                if (donutRigidbody.velocity != Vector3.zero)
                {
                    // 현재 이동 방향과 속도를 기반으로 동적으로 파티클 방향 설정
                    Vector3 currentVelocity = donutRigidbody.velocity;
                    Vector3 trailDirection = -currentVelocity.normalized;

                    // 파티클 시스템을 이동 방향에 따라 회전시켜 더 자연스러운 효과
                    currentTrailParticle.transform.rotation = Quaternion.LookRotation(trailDirection);
                }
                else
                {
                    // 정지 시 기본 방향 유지
                    velocity.x = new ParticleSystem.MinMaxCurve(-2f, -5f);
                    velocity.y = new ParticleSystem.MinMaxCurve(-1f, 1f);
                    velocity.z = new ParticleSystem.MinMaxCurve(-1f, 1f);
                }
            }
        }
    }


    // 꼬리 프리팹 가져오기
    private GameObject GetTrailPrefab(TrailType type)
    {
        switch (type)
        {
            case TrailType.FireTail: return trailFirePrefab;
            case TrailType.LightTail: return trailLightPrefab;
            case TrailType.MagicTail: return trailMagicPrefab;
            case TrailType.IceTail: return trailIcePrefab;
            case TrailType.ElectricTail: return trailElectricPrefab;
            default: return null;
        }
    }

    // 꼬리 파티클 수동 제어 메서드들
    public void ToggleTrail() => SetTrailEnabled(!trailEnabled);

    public void SetTrailEnabled(bool enabled)
    {
        trailEnabled = enabled;

        if (!trailEnabled)
        {
            DestroyCurrentTrailParticle();
            wasMovingFast = false;

            if (trailRenderer != null)
                trailRenderer.emitting = false;
        }
    }

    public void ChangeTrailType(TrailType newType)
    {
        if (selectedTrail == newType) return;
        selectedTrail = newType;

        if (trailEnabled && wasMovingFast)
        {
            DestroyCurrentTrailParticle();
            CreateTrailParticle(selectedTrail);
        }
    }

      public void ClearAllParticles()
    {
        DestroyCurrentAuraParticle();
        DestroyCurrentTrailParticle();
        auraEnabled = false;
        trailEnabled = false;
        wasMovingFast = false;
    }

    public void ToggleAura()
    {
        SetAuraEnabled(!auraEnabled);
    }

    public void SetAuraEnabled(bool enabled) // 파티클 On / Off 기능구현
    {
        if (auraEnabled == enabled) return;

        auraEnabled = enabled;

        if (auraEnabled)
        {
            CreateAuraParticle(selectedAura);
            Debug.Log($"강화 파티클 ON: {selectedAura}");
        }
        else
        {
            DestroyCurrentAuraParticle();
            Debug.Log("강화 파티클 OFF");
        }
    }

    public void ChangeAuraType(AuraType newType)
    {
        if (selectedAura == newType) return;

        selectedAura = newType;

        // 이미 켜져있으면 새 파티클 생성
        if (auraEnabled)
        {
            DestroyCurrentAuraParticle();
            CreateAuraParticle(selectedAura);
        }
    }

    public void ApplyAuraSettings()
    {
        // 현재 설정대로 즉시 적용
        DestroyCurrentAuraParticle();
        if (auraEnabled && selectedAura != AuraType.None)
        {
            CreateAuraParticle(selectedAura);
        }
    }

    private void CreateAuraParticle(AuraType type)
    {
        if (type == AuraType.None) return;

        GameObject prefab = GetAuraPrefab(type);
        if (prefab != null)
        {
            currentAuraParticle = Instantiate(prefab, transform.position, Quaternion.identity, transform);
        }
    }

    private void DestroyCurrentAuraParticle()
    {
        if (currentAuraParticle != null)
        {
            DestroyImmediate(currentAuraParticle);
            currentAuraParticle = null;
        }
    }

    private GameObject GetAuraPrefab(AuraType type)
    {
        switch (type)
        {
            case AuraType.Fire: return auraFirePrefab;
            case AuraType.Light: return auraLightPrefab;
            case AuraType.Magic: return auraMagicPrefab;
            case AuraType.Ice: return auraIcePrefab;
            case AuraType.Electric: return auraElectricPrefab;
            default: return null;
        }
    }
}