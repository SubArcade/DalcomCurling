using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Scr_PlayerLevelPopUp : MonoBehaviour
{
    [Header("팝업 판넬")]    
    [SerializeField, Tooltip("칭호")] private GameObject namePlateListPopup;
    [SerializeField, Tooltip("닉네임 변경")] private GameObject nickNameChangePopup;
    
    [Header("기본창 전환")]
    [SerializeField, Tooltip("프로필 변경")] private GameObject profilePanel;
    [SerializeField, Tooltip("랭킹 변경")] private GameObject rankPanel;
    
    [SerializeField, Tooltip("프로필 버튼")] private Button profileButton;
    [SerializeField, Tooltip("랭킹 버튼")] private Button rankButton;
    
    [Header("기본창")]
    [SerializeField, Tooltip("레벨")] private TMP_Text levelText;
    [SerializeField, Tooltip("닉네임")] private TMP_Text nicknameText;
    [SerializeField, Tooltip("경험치")] private TMP_Text expText;
    [SerializeField, Tooltip("경험치 게이지")] private Image expFillImage;
    [SerializeField, Tooltip("칭호")] private TMP_Text nameTitleText;
    [SerializeField, Tooltip("칭호 버튼")] private Button nameTitleButton;
    [SerializeField, Tooltip("환생 설명 버튼")] private Button explanationButton;
    [SerializeField, Tooltip("랭킹 스크롤바")] private ScrollRect scrollRect;
    
    [Header("닉네임 변경 기능")]
    [SerializeField, Tooltip("닉네임 변경 창 버튼")] private Button renameButton;
    [SerializeField, Tooltip("인풋 필드")] private TMP_InputField changeNicknameInputField;
    [SerializeField, Tooltip("닉네임 변경 버튼")] private Button changeNicknameButton;
    [SerializeField, Tooltip("닉네임 변경 버튼")] private Button gemchangeNicknameButton;
    [SerializeField, Tooltip("텍스트 숫자")] private TMP_Text changeNicknameText;
    [SerializeField, Tooltip("잼 개수")] private string gemText = "100";
    
    [Header("닉네임 변경 확정 팝업 기능")]
    [SerializeField, Tooltip("확인 팝업")] private GameObject nickNameAnswerPopup;
    [SerializeField, Tooltip("무료")] private Button freeAnswerButton;
    [SerializeField, Tooltip("젬 사용")] private Button gemAnswerButton;
    [SerializeField, Tooltip("닫기")] private Button closeAnswerButton;
    [SerializeField, Tooltip("닉네임 텍스트")] private TMP_Text nameAnswerText;
    [SerializeField, Tooltip("닉네임 색깔")] private Color nicknameColor = Color.magenta;
    
    [Header("젬 부족")]
    [SerializeField, Tooltip("젬 부족 팝업")] private GameObject notEnoughGemPopup;
    [SerializeField, Tooltip("아니요")] private Button noButton;
    [SerializeField, Tooltip("상점 가기")] private Button yesButton;
    
    [Header("칭호 on/off 기능")]
    [SerializeField, Tooltip("프리팹 들어갈 곳")] private Transform inputTransform;
    [SerializeField, Tooltip("칭호 라벨 텍스트")] private GameObject noneNameTiltleLabel;
    [SerializeField] private List<GameObject> nameTitleList = new List<GameObject>();
    
    [Header("닫기")] 
    [SerializeField, Tooltip("닫기 버튼")] private Button closeButton;
    [SerializeField, Tooltip("팝업창 닫기 버튼")] private Button platePopupCloseButton;
    [SerializeField, Tooltip("팝업창 닫기 버튼")] private Button nickPopupCloseButton;
    
    [Header("환생하기")]
    [SerializeField, Tooltip("환생 On 버튼")] private Button reincarnationOnButton;
    [SerializeField, Tooltip("환생 Off 버튼")] private Button reincarnationOffButton;
    [SerializeField, Tooltip("확정 팝업")] private GameObject reincarnationConfirmPopup;
    [SerializeField, Tooltip("확정 닫기 버튼")] private Button reincarnationConfirmCloseButton;
    [SerializeField, Tooltip("환생 버튼")] private Button reincarnationConfirmButton;
    [SerializeField, Tooltip("설명 팝업")] private GameObject reincarnationInfoPopup;
    [SerializeField, Tooltip("설명 닫기 버튼")] private Button reincarnationInfoCloseButton;
    
    [Header("랭크 티어")]
    [SerializeField, Tooltip("팝업")] private GameObject rankTierPopUp;
    [SerializeField, Tooltip("닫기 버튼")] private Button rankTierCloseButton;
    [SerializeField, Tooltip("열기 버튼")] private Button rankTierOpenButton;
    
    [SerializeField, Tooltip("칭호 SO")] private NamePlaateSO namePlateSO;
    [SerializeField, Tooltip("티어 SO")] private Scr_TierSpriteSO rankTierSO;
    
    [SerializeField, Tooltip("닉네임")] private TMP_Text rankNicknameText;
    [SerializeField, Tooltip("창호 텍스트")] private TMP_Text namePlateText;
    [SerializeField, Tooltip("칭호 스프라이트")] private Image namePlateImage;
    [SerializeField, Tooltip("점수")] private TMP_Text rankScoreText;
    [SerializeField, Tooltip("랭크 이미지")] private Image rankSprite;
    
    private void OnEnable()
    {
        TextSetUp();
        noneNameTiltleLabel.SetActive(false);
        changeNicknameButton.gameObject.SetActive(false);
        gemchangeNicknameButton.gameObject.SetActive(false);
        nickNameAnswerPopup.gameObject.SetActive(false);
        notEnoughGemPopup.SetActive(false);
        reincarnationConfirmPopup.SetActive(false);
        reincarnationInfoPopup.SetActive(false);
        if (DataManager.Instance.PlayerData.level == DataManager.Instance.PlayerData.levelMax)
        {
            reincarnationOnButton.gameObject.SetActive(true);
            reincarnationOffButton.gameObject.SetActive(false);
        }
        else
        {
            reincarnationOnButton.gameObject.SetActive(false);
            reincarnationOffButton.gameObject.SetActive(true);
        }
        
        // 본인 랭크 티어 적용
        SetRankTier();
    }
    
    // 텍스트 셋업
    public void TextSetUp()
    {
        //Debug.Log("텍스트 셋업");
        levelText.text = DataManager.Instance.PlayerData.level.ToString();
        nicknameText.text = DataManager.Instance.PlayerData.nickname;
        expText.text = $"{DataManager.Instance.PlayerData.exp}/{DataManager.Instance.PlayerData.levelMax} EXP";
        nameTitleText.text = DataManager.Instance.PlayerData.curNamePlateType.ToString();
        expFillImage.fillAmount = DataManager.Instance.PlayerData.exp / 100f;
    }
    
    private void Awake()
    {
        PopupPanelClose();
        noneNameTiltleLabel.SetActive(false);
        changeNicknameButton.gameObject.SetActive(false);
        gemchangeNicknameButton.gameObject.SetActive(false);
        nickNameAnswerPopup.gameObject.SetActive(false);
        notEnoughGemPopup.SetActive(false);
        reincarnationConfirmPopup.SetActive(false);
        reincarnationInfoPopup.SetActive(false);
        
        nameTitleList.Clear();
        foreach (Transform child in inputTransform)
        {
            nameTitleList.Add(child.gameObject);
            child.gameObject.SetActive(false);
        }
        
    }
    
    private void Start()
    {
        UpdateBasePanels(true);
        profileButton.onClick.AddListener(() => UpdateBasePanels(true));
        rankButton.onClick.AddListener(() => UpdateBasePanels(false));
        IsreincarnationToggle();
        reincarnationOnButton.onClick.AddListener(() => reincarnationConfirmPopup.SetActive(true));
        nameTitleButton.onClick.AddListener(() => NamaTitleOpen());
        platePopupCloseButton.onClick.AddListener(() => PopupPanelClose());
        nickPopupCloseButton.onClick.AddListener(() => PopupPanelClose());
        closeButton.onClick.AddListener(()=>
        {
            scrollRect.verticalNormalizedPosition = 1f; 
            UpdateBasePanels(true);
            UIManager.Instance.Open(PanelId.MainPanel);
        });
        renameButton.onClick.AddListener(IsChangeNickname);
        changeNicknameButton.onClick.AddListener(OnAnswerPopup);
        gemchangeNicknameButton.onClick.AddListener(OnAnswerPopup);
        freeAnswerButton.onClick.AddListener(FreeChangeNickname);
        gemAnswerButton.onClick.AddListener(GemChangeNickname);
        closeAnswerButton.onClick.AddListener(() => nickNameAnswerPopup.SetActive(false));
        noButton.onClick.AddListener(() =>
        {
            PopupPanelClose();
        });
        yesButton.onClick.AddListener(() =>
        {
            // 상정으로 이동해야함
            //Debug.Log("상점이동");
            UIManager.Instance.Open(PanelId.ShopPopUp);
            PopupPanelClose();
        });
        
        reincarnationConfirmCloseButton.onClick.AddListener(() => reincarnationConfirmPopup.SetActive(false));
        reincarnationInfoCloseButton.onClick.AddListener(() => reincarnationInfoPopup.SetActive(false));
        reincarnationConfirmButton.onClick.AddListener(ReincarnationButton);
        explanationButton.onClick.AddListener(() => reincarnationInfoPopup.SetActive(true));
        
        // 랭크 티어 팝업
        rankTierCloseButton.onClick.AddListener(() => rankTierPopUp.SetActive(false));
        rankTierOpenButton.onClick.AddListener(() => rankTierPopUp.SetActive(true));
    }

    // 기본판넬 토글 on/off
    private void UpdateBasePanels(bool isopen)
    {
        profilePanel.SetActive(isopen);
        rankPanel.SetActive(!isopen);
        
        
    }

    // 환생 토글버튼 on/off
    private void IsreincarnationToggle()
    {
        if (DataManager.Instance.PlayerData.level == DataManager.Instance.PlayerData.levelMax)
        {
            reincarnationOnButton.gameObject.SetActive(true);
            reincarnationOffButton.gameObject.SetActive(false);
        }
        else
        {
            reincarnationOnButton.gameObject.SetActive(false);
            reincarnationOffButton.gameObject.SetActive(true);
        }
    }

    
    // 환생하기 
    private void ReincarnationButton()
    {
        var data = DataManager.Instance.PlayerData;
        var invenData = DataManager.Instance.InventoryData;
        data.level = 1;
        data.exp = 0;
        data.energy = 50;
        data.gold = 0;
        DataManager.Instance.FirstBaseInventoryData();  // 엔트리 초기화
        DataManager.Instance.BaseInventoryData();   // 도감 초기화
        DataManager.Instance.FirstBaseMergeBoardData(); // 보드판 초기화
        DataManager.Instance.MergeBoardData.tempGiftIds.Clear();    // 임시 보관칸 초기화
        DataManager.Instance.MergeBoardData.generatorLevelHard = 1; 
        DataManager.Instance.MergeBoardData.generatorLevelSoft = 1;
        DataManager.Instance.MergeBoardData.generatorLevelMoist = 1;
        DataManager.Instance.QuestData.questList1.Clear();
        DataManager.Instance.QuestData.questList2.Clear();
        DataManager.Instance.QuestData.questList3.Clear();
        DataManager.Instance.QuestData.refreshCount = 5;
        
        levelText.text = data.level.ToString();
        expText.text = $"{data.exp}/{data.levelMax} EXP";
        expFillImage.fillAmount = 0;
        reincarnationOnButton.gameObject.SetActive(false);
        reincarnationOffButton.gameObject.SetActive(true);
        
        // 칭호 획득
        int num = data.gainNamePlateType.Count;
        if (Enum.IsDefined(typeof(NamePlateType), num))
        {
            data.gainNamePlateType.Add((NamePlateType)num);
        }
        else
        {
            Debug.LogWarning($"NamePlateType에 {num}번째 enum이 없습니다!");
        }
        
        reincarnationConfirmPopup.SetActive(false);
        
        // 메뉴판 업데이트
        DataManager.Instance.GemChange(data.gem);
        BoardManager.Instance.RefreshBoardUnlock();
    }
    
    // 환생하기 설명창
    
    // 팝업창 전체 닫기
    private void PopupPanelClose()
    {
        namePlateListPopup.SetActive(false);
        nickNameChangePopup.SetActive(false);
        nickNameAnswerPopup.SetActive(false);
        notEnoughGemPopup.SetActive(false);
    }

    private void NamaTitleOpen()
    {
        namePlateListPopup.SetActive(true);
        NameTitleOnList();
    }
    
    // 칭호 있는지 여부와 프리팹 활성화
    private void NameTitleOnList()
    {
        int stack = 0;

        foreach (var type in DataManager.Instance.PlayerData.gainNamePlateType)
        {
            if (type != NamePlateType.NONE)
                stack++;

            foreach (var obj in nameTitleList)
            {
                if (obj == null) continue;

                var ui = obj.GetComponent<Scr_NameTitle>();
                if (ui == null) continue;

                if (ui.namePlateType == type)
                {
                    obj.SetActive(true);
                    
                    var toggle = ui.toggleButton;
                    if (toggle == null) continue;
                    
                    toggle.onValueChanged.RemoveAllListeners();
                    
                    var capturedType = type;

                    // isOn 이 true 될 때만 실행
                    toggle.onValueChanged.AddListener(isOn =>
                    {
                        if (isOn)
                            OnSelectNameTitle(obj, capturedType);
                    });

                    // 현재 선택된 칭호면 체크만 해두기
                    if (type == DataManager.Instance.PlayerData.curNamePlateType)
                        toggle.SetIsOnWithoutNotify(true);
                }
            }
        }

        if (stack == 0)
            noneNameTiltleLabel.SetActive(true);
        else
            noneNameTiltleLabel.SetActive(false);
    }
    
    // 선택된 토글만 활성화
    public void OnSelectNameTitle(GameObject selectedObj, NamePlateType type)
    {
        var ui = selectedObj.GetComponent<Scr_NameTitle>();
        if (ui == null || ui.toggleButton == null) return;
        var selectedToggle = ui.toggleButton;
        
        if (DataManager.Instance.PlayerData.curNamePlateType == type)
        {
            selectedToggle.isOn = false;
            DataManager.Instance.PlayerData.curNamePlateType = NamePlateType.NONE;
            return;
        }
        
        foreach (var obj in nameTitleList)
        {
            var otherUi = obj.GetComponent<Scr_NameTitle>();
            if (otherUi == null || otherUi.toggleButton == null) continue;

            bool isTarget = (obj == selectedObj);
            otherUi.toggleButton.SetIsOnWithoutNotify(isTarget);
        }

        DataManager.Instance.PlayerData.curNamePlateType = type;
        nameTitleText.text = type.ToString();
        Debug.Log($"선택된 칭호 변경됨: {type}");
    }


    
    // 추후 기획후에 개발 진행
    // 경험치 계산식
    private void ExpCalcu()
    {
        
    }


    // 닉네임 버튼 활성화
    private void IsChangeNickname()
    {
        nickNameChangePopup.SetActive(true);
        if (DataManager.Instance.PlayerData.changeNicknameCount == 0)
        {
            changeNicknameButton.gameObject.SetActive(true);
            gemchangeNicknameButton.gameObject.SetActive(false);
        }
        else
        {
            changeNicknameButton.gameObject.SetActive(false);
            gemchangeNicknameButton.gameObject.SetActive(true);
        }
    }

    // 닉네임 변경 확정창 띄우기
    private void OnAnswerPopup()
    {
        if (changeNicknameInputField.text.Length < 2)
        {
            return;
        }
        nickNameAnswerPopup.SetActive(true);
        string hexColor = ColorUtility.ToHtmlStringRGB(nicknameColor);
        nameAnswerText.text = $"<color=#{hexColor}>‘{changeNicknameInputField.text}’</color>로\n 변경하시겠습니까?";
        
        if (DataManager.Instance.PlayerData.changeNicknameCount == 0)
        {
            freeAnswerButton.gameObject.SetActive(true);
            gemAnswerButton.gameObject.SetActive(false);
        }
        else
        {
            freeAnswerButton.gameObject.SetActive(false);
            gemAnswerButton.gameObject.SetActive(true);
        }
    }
    
    // 닉네임 변경 (무료)
    private void FreeChangeNickname()
    {
        //DataManager.Instance.PlayerData.nickname = changeNicknameInputField.text;
        DataManager.Instance.NicknameChange(changeNicknameInputField.text);
        DataManager.Instance.PlayerData.changeNicknameCount++;
        nickNameAnswerPopup.SetActive(false);
        nickNameChangePopup.SetActive(false);
        
        nicknameText.text = DataManager.Instance.PlayerData.nickname;
    }

    // 닉네임 변경 (젬)
    private void GemChangeNickname()
    {
        int gem = int.Parse(gemText);
        
        // 젬이 모자란 경우
        if (DataManager.Instance.PlayerData.gem < gem)
        {
            notEnoughGemPopup.SetActive(true);
        }
        else
        {
            // 닉네임 변경
            //DataManager.Instance.PlayerData.nickname = changeNicknameInputField.text;
            DataManager.Instance.NicknameChange(changeNicknameInputField.text);
            DataManager.Instance.PlayerData.changeNicknameCount++;
            DataManager.Instance.PlayerData.gem -= int.Parse(gemText);
            DataManager.Instance.GemChange(DataManager.Instance.PlayerData.gem);
            nickNameAnswerPopup.SetActive(false);
            nickNameChangePopup.SetActive(false);
            
            nicknameText.text = DataManager.Instance.PlayerData.nickname;
        }
        
    }
    
    private void SetRankTier()
    {
        var data = DataManager.Instance.PlayerData;
        
        rankNicknameText.text = data.nickname;
        var namePlate = namePlateSO.GetByType(data.curNamePlateType);
        namePlateText.text = namePlate.plateType.ToString();
        namePlateImage.sprite = namePlate.plateSprite;
        rankScoreText.text = data.soloScore.ToString();
        rankSprite.sprite = rankTierSO.GetSprite(data.soloTier);
        
    }
}

