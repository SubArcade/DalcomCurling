using System;
﻿using System.Collections;
﻿using System.Collections.Generic;
﻿using DG.Tweening;
﻿using EPOOutline;
﻿using UnityEngine;
﻿
﻿public class StoneForceController_Firebase : MonoBehaviour
﻿{
﻿    public float stoneForce { get; private set; }
﻿    public int donutId; // 발사한 인덱스를 기준으로 인게임에서 도넛을 찾을때 쓰는 ID
﻿    public int donutSettingAmount { get; private set; } = 1; // 현재 도넛의 엔트리에서의 설정 레벨
﻿    public string DonutTypeAndNumber { get; private set; } // 도넛의 종류를 식별하는 ID (예: "Soft_15")
﻿
﻿    // 도넛의 물리적 속성
﻿    public int DonutWeight { get; private set; }
﻿    public int DonutResilience { get; private set; }
﻿    public int DonutFriction { get; private set; }
﻿
﻿    private bool isPassedEndHogLine;
﻿
﻿    // StoneShoot.Team 대신 직접 Team enum을 정의하거나, FirebaseGameManager에서 팀 정보를 관리하도록 변경
﻿    public Team team { get; private set; }
﻿    public DonutType type { get; private set; } // 이 도넛의 타입 ( 단단, 말랑, 촉촉 )
﻿    public float spinForce;
﻿    private float velocityCalc = 0; // 발사 파워와 현재 속도를 1로 노멀라이징한 변수
﻿    private float spinAmountFactor = 600f; // 회전값을 얼마나 시각화 할지를 적용하는 변수 ( 높을수록 많이 회전 ) , 기본값 1.5
﻿    private float sidewaysForceFactor = 4f; // 회전값을 통해 얼마나 옆으로 휘게 할지 적용하는 변수 ( 높을수록 많이 휨 ) , 기본값 5, 0.07(기존로직)
﻿    //private float sidewaysForceSpeedLimit = 0f; // 속도가 몇%가 될때까지 옆으로 휘는 힘을 가할건지 ( 낮을수록 오래 휨 ), 기본값 0.4
﻿    private int sidewaysForceAddLimit = 1100; // 동일한 횟수만큼만 옆으로 밀리는 힘을 주어서 각 환경에서 싱크가 일치하도록 도움
﻿    private int sidewaysForceAddCount = 0; // 현재까지 옆으로 밀리는 힘을 준 횟수
﻿    //private float sweepSidewaysForceFactor = 0.3f; // 스위핑으로 양옆으로 얼마나 휘게 만들지 ( 높을수록 많이 휨 ) , 기본값 0.3
﻿    private Rigidbody rigid;
﻿
﻿    private PhysicMaterial physicMaterial;
﻿    private Coroutine spinVisualCoroutine = null;
﻿    private Scr_DonutParticleSystem donutParticleSystem;
﻿    private StoneManager stoneManager;
﻿
﻿    // private StoneShoot shoot; // 더 이상 StoneShoot을 직접 참조하지 않습니다.
﻿    private float initialFrictionValue;
﻿    private bool isShooted = false;
﻿    private bool attackMoveFinished = false;
﻿    private bool isCollided = false;
﻿    public float sweepSidewaysForce;
﻿
﻿    public float sweepFrictionValue;
﻿
﻿    // --- Reflection Fields ---
﻿    /// <summary>
﻿    /// 반사 효과를 표현할 게임 오브젝트입니다.
﻿    /// </summary>
﻿    private GameObject reflectionObject;
﻿    /// <summary>
﻿    /// 모든 스톤 인스턴스에서 공유할 반사 재질입니다. 한 번만 로드됩니다.
﻿    /// </summary>
﻿    private static Material reflectionMaterial;
﻿
﻿
﻿    //public bool isStartingTeam { get; private set; }
﻿
﻿    // StoneShoot.Team 대신 사용할 Team enum (또는 FirebaseGameManager에서 관리)
﻿    public enum Team
﻿    {
﻿        A,
﻿        B,
﻿        None
﻿    }
﻿
﻿    // Start is called before the first frame update
﻿    void Awake()
﻿    {
﻿        rigid = GetComponent<Rigidbody>();
﻿        stoneManager = FirebaseGameManager.Instance.StoneManagerInGM;
﻿        //CapsuleCollider capsuleCollider = transform.GetComponent<CapsuleCollider>();
﻿        
﻿        if (!transform.TryGetComponent<CapsuleCollider>(out  CapsuleCollider capsuleCollider))
﻿        {
﻿            Debug.Log("CapsuleCollider를 찾을 수 없음");
﻿        }
﻿        physicMaterial = capsuleCollider.material;
﻿        initialFrictionValue = physicMaterial.dynamicFriction;
﻿        donutParticleSystem = transform.GetComponent<Scr_DonutParticleSystem>();
﻿    }
﻿    
﻿    void Start()
﻿    {
﻿        // --- 반사 오브젝트 생성 ---
﻿        // 재질이 아직 로드되지 않았다면 로드합니다.
﻿        if (reflectionMaterial == null)
﻿        {
﻿            // M_Reflection 재질을 Assets/Resources/Materials 폴더 안에 위치시켜야 합니다.
﻿            reflectionMaterial = Resources.Load<Material>("Materials/Donut/M_Reflection");
﻿            if (reflectionMaterial == null)
﻿            {
﻿                Debug.LogError("반사 재질(M_Reflection)을 'Resources/Materials/Donut' 폴더에서 찾을 수 없습니다.");
﻿                return;
﻿            }
﻿        }
﻿
﻿        // 현재 게임 오브젝트를 복제하여 반사 오브젝트를 만듭니다.
﻿        reflectionObject = Instantiate(gameObject, transform.position, transform.rotation);
﻿        reflectionObject.name = $"{gameObject.name} (Reflection)";
﻿
﻿        // 반사 오브젝트에서 불필요한 컴포넌트를 제거합니다.
﻿        Destroy(reflectionObject.GetComponent<StoneForceController_Firebase>());
﻿        Destroy(reflectionObject.GetComponent<Rigidbody>());
﻿        Destroy(reflectionObject.GetComponent<CapsuleCollider>());
﻿        // Outlinable 스크립트가 있다면 제거합니다.
﻿        if (reflectionObject.TryGetComponent<Outlinable>(out var outlinable))
﻿        {
﻿            Destroy(outlinable);
﻿        }
﻿        
﻿        // 자식 오브젝트에 있는 파티클 시스템도 제거합니다.
﻿        foreach (var ps in reflectionObject.GetComponentsInChildren<ParticleSystem>())
﻿        {
﻿            Destroy(ps.gameObject);
﻿        }
﻿
﻿        // 렌더러에 반사 재질을 적용합니다.
﻿        var reflectionRenderer = reflectionObject.GetComponentInChildren<Renderer>();
﻿        if (reflectionRenderer != null)
﻿        {
﻿            reflectionRenderer.material = reflectionMaterial;
﻿        }
﻿
﻿        // Y축 스케일을 반전시켜 거울에 비친 것처럼 만듭니다.
﻿        reflectionObject.transform.localScale = new Vector3(transform.localScale.x, -transform.localScale.y, transform.localScale.z);
﻿    }
﻿    
﻿    void LateUpdate()
﻿    {
﻿        // --- 반사 오브젝트 위치 업데이트 ---
﻿        if (reflectionObject != null)
﻿        {
﻿            // 현재 스톤의 위치를 기준으로 Y축 대칭 위치에 반사 오브젝트를 놓습니다.
﻿            // 바닥이 Y=0 평면에 있다고 가정합니다.
﻿            reflectionObject.transform.position = new Vector3(transform.position.x, -transform.position.y, transform.position.z);
﻿            reflectionObject.transform.rotation = transform.rotation;
﻿        }
﻿    }
﻿
﻿        void OnDestroy()
﻿        {
﻿            // --- 반사 오브젝트 파괴 ---
﻿            // 원본 스톤이 파괴될 때 반사 오브젝트도 함께 파괴하여 씬에 남아있지 않도록 합니다.
﻿            if (reflectionObject != null)
﻿            {
﻿                Destroy(reflectionObject);
﻿            }
﻿        }
﻿    
﻿        /// <summary>
﻿        /// LateUpdate를 기다리지 않고 반사 오브젝트의 위치를 즉시 강제로 업데이트합니다.
﻿        /// StoneManager가 위치를 동기화할 때 호출됩니다.
﻿        /// </summary>
﻿        public void ForceUpdateReflectionPosition()
﻿        {
﻿            if (reflectionObject != null)
﻿            {
﻿                reflectionObject.transform.position = new Vector3(transform.position.x, -transform.position.y, transform.position.z);
﻿                reflectionObject.transform.rotation = transform.rotation;
﻿            }
﻿        }
﻿    
﻿    
﻿        private void FixedUpdate()
﻿        {
﻿            if (isShooted && attackMoveFinished == false && rigid.isKinematic == false 
﻿                && sidewaysForceAddLimit > sidewaysForceAddCount && isCollided == false)﻿        {
﻿            sidewaysForceAddCount++;
﻿            
﻿            
﻿            if (sidewaysForceAddCount % 2 == 0 && sidewaysForceAddCount > 100)
﻿            {
﻿                rigid.AddForce(
﻿                    Vector3.right * spinForce * sidewaysForceFactor * 0.01f,
﻿                    ForceMode.VelocityChange); // ForceMode를 VelocityChange로 변경 
﻿                ///////////
﻿            }
﻿        }
﻿    }
﻿
﻿    IEnumerator DonutSpinVisualizationCoroutine()
﻿    {
﻿        WaitForSeconds wait = new WaitForSeconds(0.2f);
﻿        yield return wait;
﻿        float sqrStartVelocityReciprocal = 1f / rigid.velocity.sqrMagnitude; // 시작속도의 제곱의 역수를 구함. 나눗셈이 연산속도를 낮추므로 
﻿        // 나중에 곱셈으로 쓸 수 있도록 미리 역수로 구해두는 것임
﻿        float sqrCurrentVelocity = rigid.velocity.sqrMagnitude; // 시작 속도의 제곱으로 초기값 설정
﻿        float currentSpeedRatio = 1f; // 시작 속도와 현재 속도의 비율
﻿        while (sqrCurrentVelocity > 0.01f * 0.01f)
﻿        {
﻿            if (sqrStartVelocityReciprocal != 0)
﻿            {
﻿                sqrCurrentVelocity = rigid.velocity.sqrMagnitude;
﻿                currentSpeedRatio = sqrCurrentVelocity * sqrStartVelocityReciprocal; // 현재속도와 시작속도의 역수를 곱하여 비율 계산
﻿                //Debug.Log($"현재속도 : {sqrCurrentVelocity}, 비율 : {currentSpeedRatio}");
﻿            }
﻿            //rigid.angularVelocity = Vector3.up * spinForce * spinAmountFactor * velocityCalc; // 물체의 물리적 회전속도를 직접 조정
﻿            rigid.angularVelocity = Vector3.up * spinForce * 2f * spinAmountFactor * currentSpeedRatio; // 회전속도는 속도 비율에 따라 조정
﻿            yield return wait;
﻿        }
﻿    }
﻿
﻿    // StoneShoot shoot 매개변수 제거
﻿    public void AddForceToStone(Vector3 launchDestination, float force, float spin)
﻿    {
﻿        stoneForce = force;
﻿
﻿        spinForce = spin * 0.01f; // fixedDaltaTime 삭제로 인한 계수 추가
﻿        rigid.velocity = launchDestination;
﻿       
﻿        isShooted = true;
﻿        spinVisualCoroutine = StartCoroutine(DonutSpinVisualizationCoroutine());
﻿    }
﻿
﻿    // StoneShoot.Team 대신 직접 정의한 Team enum 사용
﻿    public void InitializeDonut(Team team, DonutType type,int donutId, string donutTypeId, int amount) // 도넛의 팀과 id, 물리 속성을 적용
﻿    {
﻿        this.team = team;
﻿        this.type = type;
﻿        this.donutId = donutId;
﻿        this.DonutTypeAndNumber = donutTypeId;
﻿        donutSettingAmount = amount;
﻿        //donutLevel 받아와야 함
﻿
﻿    }
﻿
﻿
﻿    public void MoveFinishedInTurn()
﻿    {
﻿        if (spinVisualCoroutine != null)
﻿        {
﻿            StopCoroutine(spinVisualCoroutine);
﻿            spinVisualCoroutine = null;
﻿        }
﻿        attackMoveFinished = true;
﻿        rigid.angularVelocity = Vector3.zero;
﻿        this.enabled = false;
﻿        //Debug.Log($"sidewaysForceAddCount = {sidewaysForceAddCount}");
﻿    }
﻿
﻿    public void PassedEndHogLine() // 엔드 호그라인 콜라이더가 자신과 충돌한 적이 있음을 알림 ( 최소로 넘어가야 할 선을 넘김 )
﻿    {
﻿        isPassedEndHogLine = true;
﻿    }
﻿
﻿    public void ChangeMassByCompatibility(DonutType attackerType, int attackerAmount = 1) // 공격자와의 속성 상성관계에 따라 현재 도넛의 질량을 바꿈
﻿    {
﻿        int amountDiff = donutSettingAmount - attackerAmount + 29; //내 레벨에서 공격자 도넛의 레벨을 뺌. 결과값이 높을수록 수비자가 강함. 
﻿        // 0~58의 수치로 만들기 위해 29를 더함
﻿
﻿        float normalizedDiff = Mathf.Clamp01(amountDiff / 58.0f);
﻿        switch (attackerType)
﻿        {

﻿            case DonutType.Hard:
﻿                if (type == DonutType.Hard) // 단단
﻿                {
﻿                    //rigid.mass = 4f; // 단단이 단단한테 비김
﻿                    float rawVal = Mathf.Lerp(stoneManager.DrawMinMass, stoneManager.DrawMaxMass, normalizedDiff); // 비율로 최소값과 최대값 사이를 보간
﻿                    float resultVal = Mathf.Round(rawVal * 10) / 10f; // 둘째자리에서 반올림하여 첫째자리까지만 남김
﻿                    rigid.mass = resultVal; // 질량값에 대입
﻿                }
﻿                else if (type == DonutType.Moist) // 촉촉
﻿                {
﻿                    //rigid.mass = 2f; // 단단이 촉촉한테 이김
﻿                    float rawVal = Mathf.Lerp(stoneManager.WinMinMass, stoneManager.WinMaxMass, normalizedDiff); // 비율로 최소값과 최대값 사이를 보간
﻿                    float resultVal = Mathf.Round(rawVal * 10) / 10f; // 둘째자리에서 반올림하여 첫째자리까지만 남김
﻿                    rigid.mass = resultVal; // 질량값에 대입
﻿                }
﻿                else if (type == DonutType.Soft) // 말랑
﻿                {
﻿                    //rigid.mass = 8f; // 단단이 말랑한테 짐
﻿                    float rawVal = Mathf.Lerp(stoneManager.LoseMinMass, stoneManager.LoseMaxMass, normalizedDiff); // 비율로 최소값과 최대값 사이를 보간
﻿                    float resultVal = Mathf.Round(rawVal * 10) / 10f; // 둘째자리에서 반올림하여 첫째자리까지만 남김
﻿                    rigid.mass = resultVal; // 질량값에 대입
﻿                }
﻿               // Debug.Log($"상대 : {attackerType}, 나 : {type}, 설정된 Mass : {rigid.mass}");
﻿                break;
﻿            case DonutType.Moist:
﻿                if (type == DonutType.Hard)
﻿                {
﻿                    //rigid.mass = 8f; // 촉촉이 단단한테 짐
﻿                    float rawVal = Mathf.Lerp(stoneManager.LoseMinMass, stoneManager.LoseMaxMass, normalizedDiff); // 비율로 최소값과 최대값 사이를 보간
﻿                    float resultVal = Mathf.Round(rawVal * 10) / 10f; // 둘째자리에서 반올림하여 첫째자리까지만 남김
﻿                    rigid.mass = resultVal; // 질량값에 대입
﻿                }
﻿                else if (type == DonutType.Moist)
﻿                {
﻿                    //rigid.mass = 4f; // 촉촉이 촉촉한테 비김
﻿                    float rawVal = Mathf.Lerp(stoneManager.DrawMinMass, stoneManager.DrawMaxMass, normalizedDiff); // 비율로 최소값과 최대값 사이를 보간
﻿                    float resultVal = Mathf.Round(rawVal * 10) / 10f; // 둘째자리에서 반올림하여 첫째자리까지만 남김
﻿                    rigid.mass = resultVal; // 질량값에 대입
﻿                }
﻿                else if (type == DonutType.Soft)
﻿                {
﻿                    //rigid.mass = 2f; // 촉촉이 말랑을 이김
﻿                    float rawVal = Mathf.Lerp(stoneManager.WinMinMass, stoneManager.WinMaxMass, normalizedDiff); // 비율로 최소값과 최대값 사이를 보간
﻿                    float resultVal = Mathf.Round(rawVal * 10) / 10f; // 둘째자리에서 반올림하여 첫째자리까지만 남김
﻿                    rigid.mass = resultVal; // 질량값에 대입
﻿                }
﻿              //  Debug.Log($"상대 : {attackerType}, 나 : {type}, 설정된 Mass : {rigid.mass}");
﻿                
﻿
﻿                break;
﻿            case DonutType.Soft:
﻿                if (type == DonutType.Hard)
﻿                {
﻿                    //rigid.mass = 2f; // 말랑이 단단을 이김
﻿                    float rawVal = Mathf.Lerp(stoneManager.WinMinMass, stoneManager.WinMaxMass, normalizedDiff); // 비율로 최소값과 최대값 사이를 보간
﻿                    float resultVal = Mathf.Round(rawVal * 10) / 10f; // 둘째자리에서 반올림하여 첫째자리까지만 남김
﻿                    rigid.mass = resultVal; // 질량값에 대입
﻿                }
﻿                else if (type == DonutType.Moist) 
﻿                {
﻿                    //rigid.mass = 8f; // 말랑이 촉촉한테 짐
﻿                    float rawVal = Mathf.Lerp(stoneManager.LoseMinMass, stoneManager.LoseMaxMass, normalizedDiff); // 비율로 최소값과 최대값 사이를 보간
﻿                    float resultVal = Mathf.Round(rawVal * 10) / 10f; // 둘째자리에서 반올림하여 첫째자리까지만 남김
﻿                    rigid.mass = resultVal; // 질량값에 대입
﻿                }
﻿                else if (type == DonutType.Soft)
﻿                {
﻿                    //rigid.mass = 4f; // 말랑이 말랑한테 비김
﻿                    float rawVal = Mathf.Lerp(stoneManager.DrawMinMass, stoneManager.DrawMaxMass, normalizedDiff); // 비율로 최소값과 최대값 사이를 보간
﻿                    float resultVal = Mathf.Round(rawVal * 10) / 10f; // 둘째자리에서 반올림하여 첫째자리까지만 남김
﻿                    rigid.mass = resultVal; // 질량값에 대입
﻿                }
﻿//                Debug.Log($"상대 : {attackerType}, 나 : {type}, 설정된 Mass : {rigid.mass}");
﻿                
﻿
﻿                break;
﻿        }
﻿    }
﻿
﻿    private void OnCollisionEnter(Collision other)
﻿    {
﻿        if (other.gameObject.CompareTag("InGameDonut"))
﻿        {
﻿            isCollided = true;
﻿            donutParticleSystem.PlayCollisionParticle(other);
﻿            SoundManager.Instance.stoneCrash();
﻿        }
﻿        else if (other.gameObject.CompareTag("SideWall"))
﻿        {
﻿            Vector3 collisionNormal = other.contacts[0].normal;  // 충돌지점 수직 방향 벡터 계산
﻿        
﻿            // 현재 속도의 반대 방향으로 튕겨나가는 속도 벡터 계산
﻿            Vector3 newVelocity = Vector3.Reflect(rigid.velocity, collisionNormal);
﻿
﻿            rigid.velocity = newVelocity * 1.0f; // 여기 계수 바꾸면 튕기는 정도도 바뀜
﻿            isCollided = true;
﻿            donutParticleSystem.PlayCollisionParticle(other);
﻿            SoundManager.Instance.stoneCrash();
﻿        }
﻿        
﻿    }
﻿
﻿    
﻿}