using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Scr_UseGiftBoxPopUp : MonoBehaviour, IPointerClickHandler
{
    [Header("팝업창")]
    [SerializeField] private GameObject usegiftBoxPopup;

    [Header("보상으로 받은 재화 텍스트")]
    [SerializeField] private TextMeshProUGUI gainEnergyText;
    [SerializeField] private TextMeshProUGUI gainGoldText;
    [SerializeField] private TextMeshProUGUI gainGemText;


    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        UIManager.Instance.Close(PanelId.UseGiftBoxPopUp);
    }

    public void SetRewardTexts(int gold, int energy, int gem)
    {
        gainGoldText.text = $"+{gold}";
        gainEnergyText.text = $"+{energy} ";
        gainGemText.text = gem > 0 ? $"+{gem}" : "0"; 
    }

}
