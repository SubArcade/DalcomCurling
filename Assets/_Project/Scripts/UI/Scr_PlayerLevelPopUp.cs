using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

public class Scr_PlayerLevelPopUp : MonoBehaviour
{
    [Header("로비화면에서의 플레이어 레벨 텍스트")]
    [SerializeField] private TextMeshProUGUI Level_Text;

    [Header("플레이어 레벨 텍스트")]
    [SerializeField] private TextMeshProUGUI LevelText;
    
    [SerializeField] private TextMeshProUGUI nicknameText;

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
   
    [Header("임시용 경험치주는버튼 삭제예정")]
    public Button getExpButton;

    private void OnEnable()
    {
        TextSetUp();
    }
    
    void Awake()
    {
        Transform canvas = GameObject.FindGameObjectWithTag("MainCanvas")?.transform;
        Level_Text = canvas.Find("Main_Panel/Top/Level/Level_Text")?.GetComponent<TextMeshProUGUI>();
        LevelText = transform.Find("LevelBox/LevelCount_Text")?.GetComponent<TextMeshProUGUI>();
        PlayerExp = transform.Find("ExpBackground")?.GetComponent<Image>();
        ExpView = transform.Find("ExpBackground/Exp_Text")?.GetComponent<TextMeshProUGUI>();
        CloseButton = transform.Find("CloseButton")?.GetComponent<Button>();
        LevelReward = transform.Find("GiftBoxHouse")?.GetComponent<Button>();
        RewardLabel = transform.Find("RewardNameLabel/Reward_Text")?.GetComponent<TextMeshProUGUI>();

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "RewardCheckPanel")
            {
                RewardCheckPanel = obj;
                Transform closeTransform = RewardCheckPanel.transform.Find("Close");
                if (closeTransform != null)
                {
                    CloseReward = closeTransform.GetComponent<Button>();
                }
                break;
            }
        }
    }

    void Start()
    {
        //버튼 이벤트 연결
        LevelReward.onClick.AddListener(LevelUpRewardPickUp);
        CloseButton.onClick.AddListener(OnClickCloseButton);
        //CloseReward.onClick.AddListener(OnClickCloseRewardPopUp);

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

    //레벨 및 경험치
    private int currentExp = 0;
    private int maxExp = 100;
    private int currentLevel = 1;
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

        if (Level_Text != null)
        {
            Level_Text.text = $"{currentLevel}";
        }

        if (PlayerExp != null)
        {
            PlayerExp.fillAmount = (float)currentExp / maxExp;
            ExpView.text = $"{currentExp}/{maxExp}Exp";
        }
        LevelUpReward();
        SaveLevelData();
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
    void LevelUpRewardPickUp()
    {
        if (currentLevel == 1) return;
        if (currentLevel % 2 ==1 && !receivedLevel.Contains(currentLevel)) 
        {
            RewardCheckPanel.SetActive(true);
            receivedLevel.Add(currentLevel);
            LevelReward.interactable = false;
        }
        //기프트박스 수령시 머지칸으로 보내도록 코드 추가하셔야합니다
    }

    //레벨 및 경험치 파이어베이스에 저장하기 위한 메서드
    async void SaveLevelData() 
    {        
        await DataManager.Instance.UpdateUserDataAsync(level: currentLevel, exp: currentExp);
    }
    void OnClickCloseButton() 
    {
       //this.gameObject.SetActive(false);
       UIManager.Instance.Close(PanelId.PlayerLevelInfoPopup);
    }
    void OnClickCloseRewardPopUp()
    {
        RewardCheckPanel.SetActive(false);
    }

    public void TextSetUp()
    {
        //Debug.Log("텍스트 셋업");
        nicknameText.text = DataManager.Instance.PlayerData.nickname;
    }
}

