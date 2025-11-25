using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scr_NotEnoughGemPopUp : MonoBehaviour
{
    [Header("예,아니오 버튼")]
    [SerializeField] private Button noBtn;
    [SerializeField] private Button yesBtn;

    void Start()
    {
        noBtn.onClick.AddListener(() => UIManager.Instance.Close(PanelId.NotEnoughGemPopUp));
        yesBtn.onClick.AddListener(() => UIManager.Instance.Open(PanelId.ShopPopUp));
    }
}
