using System.Xml.Serialization;
using UnityEngine;

public class DonutParticleSystem : MonoBehaviour
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

    private GameObject currentAuraParticle;
    private GameObject currentTrailParticle;
    private Rigidbody donutRigidbody;
    private bool wasMovingFast = false;
    private int targetLayer;
    
    // 오라 파티클 프로퍼티
    public bool IsAuraEnabled => auraEnabled;
    public AuraType CurrentAuraType => selectedAura;

    // 꼬리 파티클 프로퍼티
    public bool IsTrailEnabled => trailEnabled;
    public TrailType CurrentTrailType => selectedTrail;

    private void Start()
    {
        donutRigidbody = GetComponent<Rigidbody>();

        targetLayer = LayerMask.NameToLayer(targetLayerName);
        if (targetLayer == -1)
        {
            Debug.LogWarning($"Layer'{targetLayerName}'를 찾을 수 없습니다");
        }
    }
    private void Update()
    {
        UpdateTrailBasedOnSpeed();
        UpdateTrailDirection();
    }

    // 충돌 파티클에 관한 부분 스크립트 작성구간
    private void OnCollisionEnter(Collision collision)
    {
        // 지정된 레이어만 충돌하게 만들기
        if (targetLayer != -1 && collision.gameObject.layer != targetLayer) return;

        // 충돌에 필요한 최소 힘 설정하기
        float collisionForce = collision.relativeVelocity.magnitude;
        if (collisionForce < minCrashForce) return;

        // 충돌 지점에 파티클 생성하기 (2)
        CreateCollisionParticle(collision);

        Debug.Log($"도넛이 {collision.gameObject.name}이랑 {collisionForce}의 힘으로 충돌했습니다");
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
            return donutRigidbody.velocity.magnitude;
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
            case TrailType.FireTail: return trailLightPrefab;
            case TrailType.LightTail: return trailIcePrefab;
            case TrailType.MagicTail: return trailMagicPrefab;
            case TrailType.IceTail: return trailFirePrefab;
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