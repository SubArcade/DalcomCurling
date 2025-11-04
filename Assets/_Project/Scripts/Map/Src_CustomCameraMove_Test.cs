using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Src_CustomCameraMove_Test : MonoBehaviour
{
    public float forceAmount = 10f;
    private Rigidbody rb;

    private bool hasBeenLaunched = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // 씬에 있는 PunTurnManager를 찾아서 할당합니다.
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LaunchStone();
        }
    }

    private void LaunchStone()
    {
        hasBeenLaunched = true;
        rb.AddForce(transform.forward * forceAmount, ForceMode.Impulse);
    }

}
