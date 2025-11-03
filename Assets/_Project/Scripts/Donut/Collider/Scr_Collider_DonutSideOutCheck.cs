using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scr_Collider_DonutSideOutCheck : MonoBehaviour
{
    private StoneShoot stoneShoot;
    private void Awake()
    {
        stoneShoot = GameObject.FindGameObjectWithTag("GameController").GetComponent<StoneShoot>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InGameDonut"))
        {
            StoneForceController sfc = other.GetComponent<StoneForceController>();
            stoneShoot.DonutContactedSideWall(sfc.team, sfc.donutId);
        }
    }
}
