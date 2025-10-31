using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class StoneForceController : MonoBehaviour
{
    public float stoneForce;
    public int donutId { get; private set; }
    private bool isPassedEndHogLine;
    public StoneShoot.Team team { get; private set; }
    public float spinForce;
    private float velocityCalc; // 발사 파워와 현재 속도를 1로 노멀라이징한 변수
    private float spinAmountFactor = 1.5f; // 회전값을 얼마나 시각화 할지를 적용하는 변수 ( 높을수록 많이 회전 ) , 기본값 1.5
    private float sidewaysForceFactor = 0.07f; // 회전값을 통해 얼마나 옆으로 휘게 할지 적용하는 변수 ( 높을수록 많이 휨 ) , 기본값 0.07
    private float sidewaysForceSpeedLimit = 0.4f; // 속도가 몇%가 될때까지 옆으로 휘는 힘을 가할건지 ( 낮을수록 오래 휨 ), 기본값 0.4
    private float sweepSidewaysForceFactor = 0.3f; // 스위핑으로 양옆으로 얼마나 휘게 만들지 ( 높을수록 많이 휨 ) , 기본값 0.3
    private Rigidbody rigid;
    private PhysicMaterial physicMaterial;
    private StoneShoot shoot;
    private float initialFrictionValue;
    private bool isShooted = false;
    private bool attackMoveFinished = false;
    public float sweepSidewaysForce;

    public float sweepFrictionValue;

    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        physicMaterial = transform.GetComponent<MeshCollider>().material;
        initialFrictionValue = physicMaterial.dynamicFriction;
    }

    private void Update()
    {
        if (isShooted == true && attackMoveFinished == false)
        {
            if (rigid.velocity.magnitude < 0.01f && shoot.currentState == StoneShoot.LaunchState.Launched)
            {
                attackMoveFinished = true;
                physicMaterial.dynamicFriction = initialFrictionValue;
                shoot.DonutAttackFinished(isPassedEndHogLine);
            }
        }
    }

    private void FixedUpdate()
    {
        if (isShooted == true && attackMoveFinished == false)
        {
            velocityCalc =
                1f - (stoneForce - rigid.velocity.magnitude) / stoneForce; // 발사 파워와 현재 속도를 통해, 현재 속도의 비율을 0~1로 고정
            rigid.angularVelocity = Vector3.up * spinForce * spinAmountFactor * velocityCalc; // 물체의 물리적 회전속도를 직접 조정


            //현재 이동 속도가 발사속도의 설정한 퍼센트 이상일때까지만 옆으로 밀리는 힘을 추가함( 이후에는 남아있는 힘으로 인해 자연스럽게 휨 )
            rigid.AddForce(
                Vector3.right * spinForce * sidewaysForceFactor *
                (velocityCalc > sidewaysForceSpeedLimit ? velocityCalc * 0.5f : 0), ForceMode.Acceleration);

            // 스위핑으로 인한 꺾임 계산
            rigid.AddForce(
                Vector3.right * velocityCalc * 0.5f * sweepSidewaysForce * sweepSidewaysForceFactor, ForceMode.Acceleration);
        }
    }

    public void AddForceToStone(Vector3 launchDestination, float force, float spin, StoneShoot shoot)
    {
        this.shoot = shoot;
        stoneForce = force;

        spinForce = spin;
        rigid.AddForce(launchDestination, ForceMode.VelocityChange);
        DOVirtual.DelayedCall(0.5f, () =>
        {
            isShooted = true;
        });
        Debug.Log("");

        Debug.Log($"{force},  {launchDestination.magnitude},  {spin}");
        //rigid.AddForce(Vector3.forward * stoneForce, ForceMode.VelocityChange);
    }

    public void InitializeDonut(StoneShoot.Team team, int donutId) // 도넛의 팀과 id를 적용
    {
        this.team = team;
        this.donutId = donutId;
    }

    public void PassedEndHogLine() // 엔드 호그라인 콜라이더가 자신과 충돌한 적이 있음을 알림 ( 최소로 넘어가야 할 선을 넘김 )
    {
        isPassedEndHogLine = true;
    }
    
    

    public void SweepValueChanged(StoneShoot.SweepState sweepState, float value)
    {
        if (sweepState == StoneShoot.SweepState.FrontSweep)
        {
            sweepFrictionValue = value;
            physicMaterial.dynamicFriction = initialFrictionValue - (value * 0.1f);
            sweepSidewaysForce = 0;
        }
        else if (sweepState == StoneShoot.SweepState.LeftSweep)
        {
            sweepFrictionValue = 0;
            sweepSidewaysForce = -value;
            
        }
        else if (sweepState == StoneShoot.SweepState.RightSweep)
        {
            sweepFrictionValue = 0;
            sweepSidewaysForce = value;
            
        }
        else if (sweepState == StoneShoot.SweepState.None)
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
}