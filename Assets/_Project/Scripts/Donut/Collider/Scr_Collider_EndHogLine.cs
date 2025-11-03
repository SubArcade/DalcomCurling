using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scr_Collider_EndHogLine : MonoBehaviour
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
            other.GetComponent<StoneForceController>().PassedEndHogLine();
        }
    }
}
