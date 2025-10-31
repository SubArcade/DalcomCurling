using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotUse_PhysicTest : MonoBehaviour
{
    public GameObject a;
    public Transform b;

    private Vector3 startAPos;
    private Vector3 startBPos;

    public Button resetBtn;
    public Button shootBtn;
    public Button x1;
    public Button x5;
    public Button x10;
    public float timeScale = 1;
    public TextMeshProUGUI bxVal;
    public TextMeshProUGUI bzVal;
    public TextMeshProUGUI axVal;
    public TextMeshProUGUI azVal;
    public bool isShooted = false;
    private float startFixed;
    private float velocityCalc;
    private Rigidbody rigid;

    private void Start()
    {
        Time.timeScale = timeScale;
        startFixed = Time.fixedDeltaTime;
        startAPos = a.transform.position;
        startBPos = b.transform.position;
        resetBtn.onClick.AddListener(ResetBtn);
        shootBtn.onClick.AddListener(ShootBtn);
        x1.onClick.AddListener(X1);
        x5.onClick.AddListener(X5);
        x10.onClick.AddListener(X10);
        //Time.fixedDeltaTime /= timeScale;
        rigid = a.GetComponent<Rigidbody>();
        
    }

    private void Update()
    {
        axVal.text = a.transform.position.x.ToString();
        azVal.text = a.transform.position.z.ToString();
        bxVal.text = b.position.x.ToString();
        bzVal.text = b.position.z.ToString();
    }

    private void FixedUpdate()
    {
        velocityCalc =
            1f - (10 - rigid.velocity.magnitude) / 10; // 발사 파워와 현재 속도를 통해, 현재 속도의 비율을 0~1로 고정
        rigid.AddForce(Vector3.right *  velocityCalc * 0.5f, ForceMode.Acceleration); // 물체의 물리적 회전속도를 직접 조정
    }


    public void ShootBtn()
    {
        isShooted = true;
        a.transform.GetComponent<Rigidbody>().AddForce(Vector3.forward * 10, ForceMode.VelocityChange);
    }

    public void ResetBtn()
    {
        isShooted = false;
        a.transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        b.transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        a.transform.position = startAPos;
        b.transform.position = startBPos;
    }

    public void X1()
    {
        timeScale = 1;
        Time.timeScale = timeScale;
        //Time.fixedDeltaTime  = startFixed / timeScale;
        Time.fixedDeltaTime  = startFixed / 10f;
    }

    public void X5()
    {
        timeScale = 5;
        Time.timeScale = timeScale;
        //Time.fixedDeltaTime  = startFixed / timeScale;
        Time.fixedDeltaTime  = startFixed / 10f;
        
    }

    public void X10()
    {
        timeScale = 10;
        Time.timeScale = timeScale;
        Time.fixedDeltaTime  = startFixed / timeScale;
        
    }
}
