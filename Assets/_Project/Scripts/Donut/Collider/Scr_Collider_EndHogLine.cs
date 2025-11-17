using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scr_Collider_EndHogLine : MonoBehaviour
{
    private StoneShoot_Firebase stoneShoot;

    private void Awake()
    {
        stoneShoot = GameObject.FindGameObjectWithTag("GameManager").GetComponent<StoneShoot_Firebase>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InGameDonut"))
        {
            other.GetComponent<StoneForceController_Firebase>().PassedEndHogLine();
        }
    }
}
