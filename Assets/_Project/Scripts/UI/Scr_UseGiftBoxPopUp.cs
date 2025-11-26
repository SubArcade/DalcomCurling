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
    [SerializeField] private GameObject panel;

    [Header("보상으로 받은 재화 텍스트")]
    [SerializeField] private TextMeshProUGUI gainEnergyText;
    [SerializeField] private TextMeshProUGUI gainGoldText;
    [SerializeField] private TextMeshProUGUI gainGemText;

    [Header("보상 이미지들")]
    [SerializeField] private Image eneryImg;
    [SerializeField] private Image goldImg;
    [SerializeField] private Image gemImg;

    //초기화용
    private void ResetImage() 
    {
        if (eneryImg != null) eneryImg.gameObject.SetActive(true);
        if (goldImg != null) goldImg.gameObject.SetActive(true);
        if (gemImg != null) gemImg.gameObject.SetActive(true);
    }
    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        // 클릭한 위치가 Panel 내부라면 닫지 않음
        if (panel != null && RectTransformUtility.RectangleContainsScreenPoint(
            panel.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera))
        {
            return;
        }

        // ✅ Panel이 아닌 UseGiftBoxPopUp(배경)을 클릭했을 때만 닫기
        UIManager.Instance.Close(PanelId.UseGiftBoxPopUp);

        // 닫힐 때 이미지 초기화
        ResetImage();
    }



    public void SetRewardTexts(int gold, int energy, int gem)
    {
        gainGoldText.text = $"+{gold}";
        gainEnergyText.text = $"+{energy} ";
        gainGemText.text = gem > 0 ? $"+{gem}" : "0";

        if (gemImg != null)
            gemImg.gameObject.SetActive(gem > 0);
    }

}
