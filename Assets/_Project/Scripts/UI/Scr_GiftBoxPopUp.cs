using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Runtime.CompilerServices;
using UnityEngine.AI;
public class Scr_GiftBoxPopUp : MonoBehaviour
{
    [Header("사용 가능한 기프트박스 이미지")]
    [SerializeField] private GameObject GiftBoxImage;

    [Header("획득한 기프트박스 리스트")]
    [SerializeField] public Transform GiftBoxList;

    [Header("획득하기 버튼")]
    [SerializeField] private Button getRewardButton;

    [Header("획득 확인 팝업")]
    [SerializeField] private GameObject getCheckPopUp;

    [Header("획득 확인 팝업 닫기버튼")]
    [SerializeField] private Button CloseReward;

    [Header("창닫기 버튼")]
    [SerializeField] private Button closeButton;

    
    private Image giftBoxImageComponent;
    private Transform giftBoxContainer;

    void Awake()
    {
        AwakeGetInspector(); //awake에서 추가로 넣을 코드 있으면 메서드 안에 작성
        //인스펙터 자동연결 싹다 다시해야합니다
    }
    void Start()
    {
        startZip(); //start에서 추가로 넣을 코드 있으면 해당 메서드 안에 작성
        //스타트도 겸사겸사 연결보십시오
    }

    void Update()
    {     
        //지금은 기초적인것만 만들었고 기프트박스 별로 획득가능한자원이 정해지면
        //그것에 알맞게 획득 가능 보상이 출력되도록 다시 코드를 수정해야합니다.
        //그리고 보유한 기프트박스는 스택형식일지(ex>기프트박스X10)
        //개별로 저장될지 (ex>기프트박스, 기프트박스 .....)에 맞게 코드 추가
        //player Level PopUp에서 보상으로 받은 기프트박스나 보상이 이곳에서 저장됩니다.       
        //보상 수령 눌렀으면 도넛 합성기쪽으로 획득보상을 보내야합니다
    }
    void AwakeGetInspector() 
    {
        GiftBoxImage = transform.Find("GiftBoxList/GiftBoxListbackGround/GiftBoxImage")?.gameObject;

        getRewardButton = transform.Find("GiftBoxList/GiftBoxListbackGround/GiftBoxImage/GetRewardButton")?.GetComponent<Button>();

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "RewardCheckPanel")
            {
                getCheckPopUp = obj;
                Transform closeTransform = getCheckPopUp.transform.Find("close");
                if (closeTransform != null)
                {
                    CloseReward = closeTransform.GetComponent<Button>();
                }
                break;
            }
        }
        closeButton = transform.Find("CloseButton")?.GetComponent<Button>();
        GiftBoxList = transform.Find("GiftBoxList/GiftBoxListbackGround");
    }

    void startZip() 
    {
        giftBoxImageComponent = GiftBoxImage.GetComponent<Image>();

        Button giftBoxButton = GiftBoxImage.GetComponent<Button>();
  
        if (getRewardButton != null)
        {
            getRewardButton.onClick.AddListener(OnClickOpenRewardButton);
        }
        getCheckPopUp.SetActive(false);
        CloseReward.onClick.AddListener(OnClickCloseRewardPopUp);
        closeButton.onClick.AddListener(OnClickCloseGiftBoxPopUp);
    }

    public void OnClickOpenRewardButton() 
    {
        getCheckPopUp.SetActive(true);
    }

    void OnClickCloseGiftBoxPopUp()
    {
        this.gameObject.SetActive(false);
    }

    void OnClickCloseRewardPopUp()
    {
        getCheckPopUp.SetActive(false);
    }


}
