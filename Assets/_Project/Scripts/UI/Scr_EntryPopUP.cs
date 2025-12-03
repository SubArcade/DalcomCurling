using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scr_EntryPopUP : MonoBehaviour
{
    [Header("닫기 버튼")]
    [SerializeField] private Button CloseButton;

    void Start()
    {
        CloseButton.onClick.AddListener (() => 
        {
            UIManager.Instance.Close(PanelId.EntryPopUp);
        });
    }
}
