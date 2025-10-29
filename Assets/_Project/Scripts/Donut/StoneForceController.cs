using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneForceController : MonoBehaviour
{
    public float stoneForce;

    public float spinForce;
    private float velocityCalc; // 발사 파워와 현재 속도를 1로 노멀라이징한 변수
    private float spinAmountFactor = 1.5f; // 회전값을 얼마나 시각화 할지를 적용하는 변수 ( 높을수록 많이 회전 ) , 기본값 1.5
    private float sidewaysForceFactor = 0.07f; // 회전값을 통해 얼마나 옆으로 휘게 할지 적용하는 변수 ( 높을수록 많이 휨 ) , 기본값 0.07
    private float sidewaysForceSpeedLimit = 0.4f; // 속도가 몇%가 될때까지 옆으로 휘는 힘을 가할건지 ( 낮을수록 오래 휨 ), 기본값 0.4
    private Rigidbody rigid;
    private bool isShooted = false;
    // Start is called before the first frame update
    void Awake()
    {
        rigid =  GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            
        }
    }

    private void FixedUpdate()
    {
        if (isShooted == true)
        {


            velocityCalc =
                1f - (stoneForce - rigid.velocity.magnitude) / stoneForce; // 발사 파워와 현재 속도를 통해, 현재 속도의 비율을 0~1로 고정
            rigid.angularVelocity = Vector3.up * spinForce * spinAmountFactor * velocityCalc; // 물체의 물리적 회전속도를 직접 조정
            
            

            //현재 이동 속도가 발사속도의 60퍼센트 이상일때까지만 옆으로 밀리는 힘을 추가함( 이후에는 남아있는 힘으로 인해 자연스럽게 휨 )
            rigid.AddForce(
                Vector3.right * spinForce * sidewaysForceFactor * rigid.mass *
                (velocityCalc > sidewaysForceSpeedLimit ? velocityCalc * 0.5f : 0), ForceMode.Force);
        }
    }

    // Update is called once per frame
    public void AddForceToStone(Vector3 launchDestination,float force, float spin)
    {
        stoneForce = force;
        
        spinForce = spin;
        isShooted = true;
        rigid.AddForce(launchDestination, ForceMode.VelocityChange);
        
        Debug.Log("");
        
        Debug.Log($"{force},  {launchDestination.magnitude},  {spin}");
        //rigid.AddForce(Vector3.forward * stoneForce, ForceMode.VelocityChange);
    }
}
