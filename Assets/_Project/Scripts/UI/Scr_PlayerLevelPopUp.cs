using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Unity.VisualScripting;
using TMPro;

public class Scr_PlayerLevelPopUp : MonoBehaviour
{
    [Header("플레이어 레벨 텍스트")]
    [SerializeField] private TextMeshProUGUI LevelText;

    [Header("플레이어 경험치 이미지")]
    [SerializeField] private Image PlayerExp;
    //지금은 이미지타입을 Filled로 경험치 연결인데 슬라이더로 바꿔도됩니다

    [Header("플레이어 경험치 텍스트")]
    [SerializeField] private TextMeshProUGUI ExpView;

    [Header("창닫기 버튼")]
    [SerializeField] private Button CloseButton;

    [Header("수령확인 팝업")]
    [SerializeField] private GameObject RewardCheckPanel;

    [Header("수령확인 팝업 닫기버튼")]
    [SerializeField] private Button CloseReward;

    [Header("보상버튼")]
    [SerializeField] private Button LevelReward;

    [Header("단계보상 텍스트")]
    [SerializeField] private TextMeshProUGUI RewardLabel;

    [Header("기프트박스 프리팹")]
    [SerializeField] private GameObject GiftBoxShellPrefab;

    [Header("프리팹들이 들어갈 팝업창")]
    [SerializeField] private Scr_GiftBoxPopUp giftBoxPopUp;
    
    [Header("임시용 경험치주는버튼 삭제예정")]
    public Button getExpButton;

    void Awake()
    {
      awakeGetInspector();//awake에서 추가로 넣을 코드 있으면 메서드 안에 작성
        //인스펙터 자동연결 싹다 다시해야합니다
    }

    void Start()
    {
      startZip(); //start에서 추가로 넣을 코드 있으면 해당 메서드 안에 작성
                  ////스타트도 겸사겸사 연결보십시오
    }
    void Update()
    {
 
    }

    void awakeGetInspector() 
    {
        LevelText = transform.Find("LevelBox/LevelCount")?.GetComponent<TextMeshProUGUI>();
        PlayerExp = transform.Find("ExpBackground")?.GetComponent<Image>();
        ExpView = transform.Find("ExpBackground/ExpText")?.GetComponent<TextMeshProUGUI>();
        CloseButton = transform.Find("CloseButton")?.GetComponent<Button>();
        LevelReward = transform.Find("GiftBoxHouse")?.GetComponent<Button>();
        RewardLabel = transform.Find("RewardNameLabel/RewardText")?.GetComponent<TextMeshProUGUI>();
        
        giftBoxPopUp = null;

        foreach (var obj in Resources.FindObjectsOfTypeAll<Scr_GiftBoxPopUp>())
        {
            if (obj.name == "GiftBoxPopUp") // 이름으로 정확히 비교
            {
                giftBoxPopUp = obj;
                break;
            }
        }

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "RewardCheckPanel")
            {
                RewardCheckPanel = obj;
                Transform closeTransform = RewardCheckPanel.transform.Find("close");
                if (closeTransform != null)
                {
                    CloseReward = closeTransform.GetComponent<Button>();
                }
                break;
            }
        }
    }
    void startZip() 
    {      
        //버튼 이벤트 연결
        LevelReward.onClick.AddListener(OpenGiftBox);
        CloseButton.onClick.AddListener(OnClickCloseButton);
        CloseReward.onClick.AddListener(OnClickCloseRewardPopUp);

        //레벨과 경험치 초기값 설정
        LevelText.text = $"{currentLevel}";
        PlayerExp.fillAmount = (float)currentExp / maxExp;
        ExpView.text = $"{currentExp}/{maxExp}Exp";


        if (getExpButton != null) 
        {
            getExpButton.onClick.AddListener(() => LevelUp(50));
        }//테스트용 경험치 획득용
    }
    void PlayerProfile()
    {
       //유저의 프로필 사진.데이터 저장해서 다른사람에게 보이도록 해야함
    }

    private int currentExp = 0;
    private int maxExp = 100;
    private int currentLevel = 1;//레벨관련 변수들
    void LevelUp(int gainExp) 
    { 
        // 갱신된 레벨과 경험치 데이터 저장
        // 레벨업 했으면 레벨업 효과음이나 이펙트를 나오게해야합니다
        currentExp += gainExp;
        while (currentExp >= maxExp)
        {
            currentExp -= maxExp;
            currentLevel++;
            maxExp += maxExp / 10;
        }

        if (LevelText != null)
        {
            LevelText.text = $"{currentLevel}";
        }
        if (PlayerExp != null)
        {
            PlayerExp.fillAmount = (float)currentExp / maxExp;
            ExpView.text = $"{currentExp}/{maxExp}Exp";
        }
        LevelUpReward();
    }

    private HashSet<int> receivedLevel = new HashSet<int>(); //보상수령 기록
    void LevelUpReward() 
    {
        //레벨별 levelreward와 rewardtext 갱신
        //보상수령기록,보상 데이터 저장
        if (currentLevel == 1) return;
        if (currentLevel % 2 == 1 && !receivedLevel.Contains(currentLevel))
        {
            LevelReward.interactable = true;
            RewardLabel.text = $"Lv{currentLevel}수령가능!!";
        }
        else 
        {
            LevelReward.interactable = false;
        }
    }
    void OpenGiftBox()
    {
        if (currentLevel == 1) return;
        if (currentLevel % 2 ==1 && !receivedLevel.Contains(currentLevel)) 
        {
            RewardCheckPanel.SetActive(true);
            receivedLevel.Add(currentLevel);
            LevelReward.interactable = false;
        }

        // 프리팹 생성 및 리스트에 추가
        if (giftBoxPopUp != null && giftBoxPopUp.GiftBoxList != null && GiftBoxShellPrefab != null)
        {
            Transform parentTransform = giftBoxPopUp.GiftBoxList.transform;
            GameObject newGiftBox = Instantiate(GiftBoxShellPrefab, parentTransform);
            newGiftBox.transform.localScale = Vector3.one;
        }

        //획득한 보상은 기프트박스 or 머지화면의 합성쪽으로 넘겨야합니다
    }
    void OnClickCloseButton() 
    {
       this.gameObject.SetActive(false);
    }
    void OnClickCloseRewardPopUp()
    {
        RewardCheckPanel.SetActive(false);
    }
}

