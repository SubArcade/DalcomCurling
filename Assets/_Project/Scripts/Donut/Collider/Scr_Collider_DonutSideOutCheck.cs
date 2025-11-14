using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scr_Collider_DonutSideOutCheck : MonoBehaviour
{
    private StoneManager stoneManager;
    private void Awake()
    {
        stoneManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<StoneManager>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("InGameDonut"))
        {
            StoneForceController_Firebase sfc = other.transform.GetComponent<StoneForceController_Firebase>();
            stoneManager.DonutOut(sfc);
            Debug.Log("옆쪽 벽과 충돌함");
        }
    }
}
