using System;
using System.Xml.Serialization;
using UnityEngine;

public class Scr_DonutParticleSystem : MonoBehaviour
{
    private EffectType selectedEffectType;
    //public enum TrailType { None, FireTail, LightTail, MagicTail, IceTail}

    [Header("팀 구별용 표시 오브젝트")]
    [SerializeField] private GameObject myTeamCircle;
    [SerializeField] private GameObject otherTeamCircle;
    private GameObject currentTeamCircleObject;
    
    //[Header("오라 파티클")]
    [SerializeField] private bool auraEnabled = false;
    private GameObject selectedAura = null;
    //[SerializeField] private AuraType selectedAura = AuraType.None;
    //[SerializeField] private GameObject auraFirePrefab;
   // [SerializeField] private GameObject auraLightPrefab;
    //[SerializeField] private GameObject auraMagicPrefab;
   // [SerializeField] private GameObject auraIcePrefab;
   // [SerializeField] private GameObject auraElectricPrefab;

    //[Header("꼬리 파티클")]
    [SerializeField] private bool trailEnabled = false;

    private GameObject selectedTrail = null;
    //[SerializeField] private TrailType selectedTrail = TrailType.None;
    //[SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private float minSpeedForTrail = 1.0f;
    //[SerializeField] private GameObject trailFirePrefab;
    //[SerializeField] private GameObject trailLightPrefab;
   // [SerializeField] private GameObject trailMagicPrefab;
    //[SerializeField] private GameObject trailIcePrefab;
    //[SerializeField] private GameObject trailElectricPrefab;

    //[Header("충돌 파티클")]
    private GameObject selectedCollision = null;
   // [SerializeField] private GameObject collosionRed;
   // [SerializeField] private GameObject collosionBlue;
   // [SerializeField] private GameObject collosionMagic;
   // [SerializeField] private GameObject collosionStar;
   // [SerializeField] private float CrashParticleDuation = 2.0f; // 충돌 파티클 지속시간 -> 필요없을시 삭제하기
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
    //public bool IsAuraEnabled => auraEnabled;
   // public AuraType CurrentAuraType => selectedAura;

    // 꼬리 파티클 프로퍼티
    //public bool IsTrailEnabled => trailEnabled;
    //public TrailType CurrentTrailType => selectedTrail;
    
    // 파티클 시스템 컴포넌트는 오브젝트의 자식으로 이미 생성되어 있어야 합니다.
    public ParticleSystem trailParticleSystem;

    private Rigidbody rb;
    private ParticleSystem.ShapeModule shapeModule;
    private float colliderRadius;
    private CapsuleCollider capsuleCollider;
    private Vector3 originalScale;
    private Vector3 newScale;
    private bool particleTrailFinished = false;
    private bool hasSelectedEffect = false;
    
    //fixedUpdate 내부에서 변수 생성을 하지 않도록 미리 생성한 변수
    private Vector3 velocity;
    private float sqrVelocity;
    private Vector3 direction;
    private Quaternion targetRotation;
    private float scaleValue_Y;
    private GameObject particleSystemParent;
    private ParticleSystem.MainModule mainModule;

    // 파티클 시스템의 로컬 X축 회전 보정을 위한 상수 (90도 회전 필요)
    private readonly Quaternion rotationOffset = Quaternion.Euler(0f, 0f, 0f);

    private void Awake()
    {
        donutRigidbody = GetComponent<Rigidbody>();
        //trailRenderer = GetComponent<TrailRenderer>();
        //trailRenderer.enabled = false;
        capsuleCollider = transform.GetComponent<CapsuleCollider>();
        colliderRadius = capsuleCollider.radius * transform.localScale.x;
    }
    
     
     void Update()
     {
         if (initialized && !particleTrailFinished && hasSelectedEffect)
         {
             // 1. 현재 속도 벡터를 가져옵니다.
             velocity = donutRigidbody.velocity;
             sqrVelocity = velocity.sqrMagnitude;
             //trailParticleSystem.transform.position = donutRigidbody.position;
             // 스톤이 멈췄는지 확인하는 임계값 (매우 작은 값)
             if (sqrVelocity < 0.0001f) // 0.01 의 제곱과 비교 (sqrMagnitude 이기 때문)
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
                 if (selectedEffectType == EffectType.Magic || selectedEffectType == EffectType.Star)
                 {
                     trailParticleSystem.transform.localScale = new Vector3(originalScale.x,
                         originalScale.y, originalScale.z + sqrVelocity * 0.02f);
                     mainModule = trailParticleSystem.main;
                     mainModule.simulationSpeed  = 1f + sqrVelocity * 0.02f;
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
                 particleTrailFinished = true;
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

    public void InitializeDonutParticles(bool isMyTeam, EffectType effectType) // StoneManager에서 도넛이 생성될때 미리 파티클들을 설정하는 함수
    {
        // InventoryData.curEffectType 값
        if (isMyTeam)
        {
            CreateTeamCircle(myTeamCircle);
        }
        else 
        {
            CreateTeamCircle(otherTeamCircle);
        }

        if (effectType == EffectType.None)
        {
            
        }
        else
        {
            var effectSo = FirebaseGameManager.Instance.EffectSoObject.GetEffectSO(effectType); //EffectSo에서 오브젝트 모음을 가져옴
            selectedEffectType = effectType; // 선택된 타입을 저장해둠
            selectedAura = effectSo.auraEffect; // 오라 이펙트 가져옴
            selectedCollision = effectSo.collisionEffect; // 충돌 이펙트 가져옴
            selectedTrail = effectSo.trailEffect; // 꼬리 이펙트 가져옴
            trailEnabled = true; // 꼬리 활성화
            initialized = true; // update문에서 꼬리 이펙트를 재생 시킬 준비를 마침
            hasSelectedEffect = true; //이펙트가 존재한다고 알림
            
            CreateCollisionParticleToChildren(); // 충돌 파티클 미리 만들어놓기
            Instantiate(selectedAura, transform.position, Quaternion.identity, transform); // 오라 생성
            //particleSystemParent = Instantiate(GetTrailPrefab(selectedTrail), transform.position, rotationOffset);
            particleSystemParent = Instantiate(selectedTrail, transform.position, rotationOffset);
            trailParticleSystem = particleSystemParent.transform.GetChild(0).GetComponent<ParticleSystem>();
            // Shape 모듈을 미리 캐시해둡니다.
            shapeModule = trailParticleSystem.shape;
        
            // 파티클을 오브젝트의 로컬 축 기준으로 날아가게 설정 (필수)
            var main = trailParticleSystem.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
        
            // 초기에는 파티클을 멈춰둡니다.
            trailParticleSystem.Stop();
            originalScale = trailParticleSystem.transform.localScale;
        }
        
    }

    private void CreateCollisionParticleToChildren() // 이 게임에 쓰일 충돌 파티클을 미리 생성해둠
    {
        //currentCollisionParticle = Instantiate(collosionBlue, transform, true);
        currentCollisionParticle = Instantiate(selectedCollision, transform, true);
    }

    private void CreateTeamCircle(GameObject teamCircle)
    {
        currentTeamCircleObject = Instantiate(teamCircle, transform.position, Quaternion.Euler(90f,0f,0f), transform);
    }

    public void PlayCollisionParticle(Collision collision) // 충돌시 StoneForceController_Firebase에서 호출할 함수. 파티클을 재생시킴
    {
        if (selectedEffectType == EffectType.None) return;
        ContactPoint contact = collision.contacts[0];
        Vector3 collisionPoint = contact.point;
        Quaternion collisionRotation = Quaternion.LookRotation(contact.normal);
        currentCollisionParticle.transform.position = collisionPoint;
        currentCollisionParticle.transform.rotation = collisionRotation;
        currentCollisionParticle.GetComponent<ParticleSystem>().Play();
    }
}