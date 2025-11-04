using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scr_GiftBoxShell : MonoBehaviour
{
    [Header("기프트박스 버튼")]
    [SerializeField] private Button giftBoxButton;

    [Header("보상 수령 버튼")]
    [SerializeField] private Button getRewardButton;

    [Header("보상 이미지")]
    [SerializeField] private Image giftBoxImage;
    
    private bool isActive = false;
    private Scr_GiftBoxPopUp giftBoxPopUp;

    void Awake()
    {        
        giftBoxButton = transform.Find("Giftbox")?.GetComponent<Button>();
        getRewardButton = transform.Find("GetRewardButton")?.GetComponentInChildren<Button>();
        giftBoxImage = transform.Find("Giftbox")?.GetComponent<Image>();

        giftBoxPopUp = FindObjectOfType<Scr_GiftBoxPopUp>();
    }
    void Start()
    {
        giftBoxPopUp = FindObjectOfType<Scr_GiftBoxPopUp>();
        giftBoxButton.onClick.AddListener(OnClickGiftBox);
        getRewardButton.onClick.AddListener(OnClickRewardButton);
    }

    void OnClickGiftBox()
    {
        isActive = !isActive;
        giftBoxImage.color = isActive ? new Color(0, 0, 1, 0.3f) : Color.white;
    }

    void OnClickRewardButton()
    {
        giftBoxPopUp.OnClickOpenRewardButton(); // 중앙 팝업 호출
        Destroy(this.gameObject);  // 셀 자체 제거
    }
}
