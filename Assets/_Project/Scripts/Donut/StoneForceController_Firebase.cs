using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class StoneForceController_Firebase : MonoBehaviour
{
    public float stoneForce;
    public int donutId; // 발사한 인덱스를 기준으로 인게임에서 도넛을 찾을때 쓰는 ID
    public string DonutId { get; private set; } // 도넛의 종류를 식별하는 ID (예: "Soft_15")

    // 도넛의 물리적 속성
    public int DonutWeight { get; private set; }
    public int DonutResilience { get; private set; }
    public int DonutFriction { get; private set; }

    private bool isPassedEndHogLine;

    // StoneShoot.Team 대신 직접 Team enum을 정의하거나, FirebaseGameManager에서 팀 정보를 관리하도록 변경
    public Team team { get; private set; }
    public float spinForce;
    private float velocityCalc; // 발사 파워와 현재 속도를 1로 노멀라이징한 변수
    private float spinAmountFactor = 300f; // 회전값을 얼마나 시각화 할지를 적용하는 변수 ( 높을수록 많이 회전 ) , 기본값 1.5
    private float sidewaysForceFactor = 5f; // 회전값을 통해 얼마나 옆으로 휘게 할지 적용하는 변수 ( 높을수록 많이 휨 ) , 기본값 5, 0.07(기존로직)
    //private float sidewaysForceSpeedLimit = 0f; // 속도가 몇%가 될때까지 옆으로 휘는 힘을 가할건지 ( 낮을수록 오래 휨 ), 기본값 0.4
    private int sidewaysForceAddLimit = 500; // 동일한 횟수만큼만 옆으로 밀리는 힘을 주어서 각 환경에서 싱크가 일치하도록 도움
    private int sidewaysForceAddCount = 0; // 현재까지 옆으로 밀리는 힘을 준 횟수
    //private float sweepSidewaysForceFactor = 0.3f; // 스위핑으로 양옆으로 얼마나 휘게 만들지 ( 높을수록 많이 휨 ) , 기본값 0.3
    private Rigidbody rigid;

    private PhysicMaterial physicMaterial;

    // private StoneShoot shoot; // 더 이상 StoneShoot을 직접 참조하지 않습니다.
    private float initialFrictionValue;
    private bool isShooted = false;
    private bool attackMoveFinished = false;
    private bool isCollided = false;
    public float sweepSidewaysForce;

    public float sweepFrictionValue;

    //public bool isStartingTeam { get; private set; }

    // StoneShoot.Team 대신 사용할 Team enum (또는 FirebaseGameManager에서 관리)
    public enum Team
    {
        A,
        B,
        None
    }

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        //CapsuleCollider capsuleCollider = transform.GetComponent<CapsuleCollider>();
        
        if (!transform.TryGetComponent<CapsuleCollider>(out  CapsuleCollider capsuleCollider))
        {
            Debug.Log("CapsuleCollider를 찾을 수 없음");
        }
        physicMaterial = capsuleCollider.material;
        initialFrictionValue = physicMaterial.dynamicFriction;
    }


    private void FixedUpdate()
    {
        if (isShooted && attackMoveFinished == false && rigid.isKinematic == false 
            && sidewaysForceAddLimit > sidewaysForceAddCount && isCollided == false)
        {
            sidewaysForceAddCount++;
            
            velocityCalc =
                1f - (stoneForce - rigid.velocity.magnitude) / stoneForce; // 발사 파워와 현재 속도를 통해, 현재 속도의 비율을 0~1로 고정

            rigid.angularVelocity = Vector3.up * spinForce * spinAmountFactor * velocityCalc; // 물체의 물리적 회전속도를 직접 조정
            


            //현재 이동 속도가 발사속도의 설정한 퍼센트 이상일때까지만 옆으로 밀리는 힘을 추가함( 이후에는 남아있는 힘으로 인해 자연스럽게 휨 )
            // float forceAmount = spinForce * sidewaysForceFactor * (velocityCalc > sidewaysForceSpeedLimit ? velocityCalc * 0.5f : 0);
            //float forceAmount = spinForce * sidewaysForceFactor * velocityCalc * 0.5f;
            
            
            //////////
            // float forceAmount = spinForce * sidewaysForceFactor;
            // rigid.AddForce(
            //     Vector3.right * forceAmount * Time.fixedDeltaTime * 0.01f,
            //     ForceMode.VelocityChange); // ForceMode를 VelocityChange로 변경
            float forceAmount = spinForce * sidewaysForceFactor;
            rigid.AddForce(
                Vector3.right * forceAmount * 0.01f,
                ForceMode.VelocityChange); // ForceMode를 VelocityChange로 변경 
            ///////////
            

            // 스위핑으로 인한 꺾임 계산
            //rigid.AddForce(
            //    Vector3.right * velocityCalc * 0.5f * sweepSidewaysForce * sweepSidewaysForceFactor, ForceMode.Acceleration);
        }
        else if ((isShooted && attackMoveFinished == false && rigid.isKinematic == false 
                 && sidewaysForceAddLimit <= sidewaysForceAddCount)
                 || isShooted && attackMoveFinished == false && rigid.isKinematic == false
                 && isCollided) // 좌우로 가해지는 힘 횟수가 끝났거나 다른 도넛과 충돌했으면
        {
            velocityCalc =
                1f - (stoneForce - rigid.velocity.magnitude) / stoneForce; // 발사 파워와 현재 속도를 통해, 현재 속도의 비율을 0~1로 고정

            rigid.angularVelocity = Vector3.up * spinForce * spinAmountFactor * velocityCalc; // 물체의 물리적 회전속도를 직접 조정
        }
    }

    // StoneShoot shoot 매개변수 제거
    public void AddForceToStone(Vector3 launchDestination, float force, float spin)
    {
        stoneForce = force;

        spinForce = spin * 0.01f; // fixedDaltaTime 삭제로 인한 계수 추가
        rigid.AddForce(launchDestination, ForceMode.VelocityChange);
        // DOVirtual.DelayedCall(0.2f, () =>
        // {
        //     isShooted = true;
        // });
        isShooted = true;
        Debug.Log($"[StoneForceController_Firebase] Force: {force}, Spin: {spin}");
    }

    // StoneShoot.Team 대신 직접 정의한 Team enum 사용
    public void InitializeDonut(Team team, int donutId, string donutTypeId, int weight, int resilience, int friction) // 도넛의 팀과 id, 물리 속성을 적용
    {
        this.team = team;
        this.donutId = donutId;
        this.DonutId = donutTypeId;
        this.DonutWeight = weight;
        this.DonutResilience = resilience;
        this.DonutFriction = friction;

        // 무게를 Rigidbody의 질량으로 설정
        if (rigid != null)
        {
           // rigid.mass = DonutWeight;
        }

        // // 반발력과 마찰력은 PhysicMaterial을 통해 설정합니다.
        // if (physicMaterial == null)
        // {
        //     MeshCollider meshCollider = transform.GetComponent<MeshCollider>();
        //     if (meshCollider != null)
        //     {
        //         // 다른 돌에 영향을 주지 않도록 공유된 물리 재질의 인스턴스를 생성합니다.
        //         physicMaterial = new PhysicMaterial(gameObject.name + "_PhysicMaterial");
        //         meshCollider.material = physicMaterial;
        //     }
        //     else
        //     {
        //         Debug.LogError("InitializeDonut: MeshCollider가 없어 물리 속성을 적용할 수 없습니다.");
        //         return;
        //     }
        // }
        // 반발력과 마찰력은 PhysicMaterial을 통해 설정합니다.
        if (physicMaterial == null)
        {
            //CapsuleCollider capsuleCollider = transform.GetComponent<CapsuleCollider>();
            if (!transform.TryGetComponent<CapsuleCollider>(out CapsuleCollider capsuleCollider))
            {
                Debug.LogError("InitializeDonut: CapsuleCollider가 없어 물리 속성을 적용할 수 없습니다.");
                return;
            }
            // 다른 돌에 영향을 주지 않도록 공유된 물리 재질의 인스턴스를 생성합니다.
            physicMaterial = new PhysicMaterial(gameObject.name + "_PhysicMaterial");
            capsuleCollider.material = physicMaterial;
        }

        // int 값을 float (0~1) 범위로 변환하여 적용합니다. (예: 10 -> 1.0, 5 -> 0.5)
        // 이 변환 방식은 기획에 따라 달라질 수 있습니다.
        // physicMaterial.bounciness = Mathf.Clamp01(DonutResilience / 10.0f);
        // physicMaterial.dynamicFriction = Mathf.Clamp01(DonutFriction / 10.0f);
        initialFrictionValue = physicMaterial.dynamicFriction; // 스위핑 로직을 위해 초기 마찰값 업데이트
    }


    public void MoveFinishedInTurn()
    {
        attackMoveFinished = true;
        rigid.angularVelocity = Vector3.zero;
        Debug.Log($"sidewaysForceAddCount = {sidewaysForceAddCount}");
    }

    public void PassedEndHogLine() // 엔드 호그라인 콜라이더가 자신과 충돌한 적이 있음을 알림 ( 최소로 넘어가야 할 선을 넘김 )
    {
        isPassedEndHogLine = true;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("InGameDonut"))
        {
            isCollided = true;
        }
    }

    // SweepState enum도 StoneShoot에서 가져오거나 직접 정의해야 합니다.
    public void SweepValueChanged(SweepState sweepState, float value)
    {
        if (physicMaterial == null) return; // null 체크 추가

        if (sweepState == SweepState.FrontSweep)
        {
            sweepFrictionValue = value;
            physicMaterial.dynamicFriction = initialFrictionValue - (value * 0.1f);
            sweepSidewaysForce = 0;
        }
        else if (sweepState == SweepState.LeftSweep)
        {
            sweepFrictionValue = 0;
            sweepSidewaysForce = -value;
        }
        else if (sweepState == SweepState.RightSweep)
        {
            sweepFrictionValue = 0;
            sweepSidewaysForce = value;
        }
        else if (sweepState == SweepState.None)
        {
            if (sweepSidewaysForce != 0)
            {
                sweepSidewaysForce = value;
            }
            else if (sweepFrictionValue != 0)
            {
                sweepFrictionValue = value;
                sweepSidewaysForce = 0;
            }
        }

        physicMaterial.dynamicFriction = initialFrictionValue - (sweepFrictionValue * 0.6f * 0.1f);
    }

    // SweepState enum 정의 (StoneShoot에서 가져옴)
    public enum SweepState
    {
        FrontSweep,
        LeftSweep,
        RightSweep,
        None
    }
}