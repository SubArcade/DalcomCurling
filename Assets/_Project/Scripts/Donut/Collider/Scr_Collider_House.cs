using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scr_Collider_House : MonoBehaviour
{
    
    private List<StoneForceController_Firebase> inHouseDonutList =  new List<StoneForceController_Firebase>();
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InGameDonut"))
        {
            if (other.GetComponent<StoneForceController_Firebase>() != null 
                && inHouseDonutList.Contains(other.GetComponent<StoneForceController_Firebase>()) == false)
            {
                inHouseDonutList.Add(other.GetComponent<StoneForceController_Firebase>());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("InGameDonut"))
        {
            if (other.GetComponent<StoneForceController_Firebase>() != null 
                && inHouseDonutList.Contains(other.GetComponent<StoneForceController_Firebase>()))
            {
                inHouseDonutList.Remove(other.GetComponent<StoneForceController_Firebase>());
            }
        }
    }

    public List<StoneForceController_Firebase> GetInHouseDonutList()
    {
        return inHouseDonutList;
    }
}
