using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scr_NameTitle : MonoBehaviour
{
    public  TMP_Text namePlateTitleText;
    public Toggle toggleButton;
    public NamePlateType namePlateType; 

    void Awake()
    {
        namePlateTitleText.text = namePlateType.ToString();
    }
}
