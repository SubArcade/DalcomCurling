using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Serialization;

public class Scr_PlayerLevelPopUp : MonoBehaviour
{
    [Header("팝업 판넬")]    
    [SerializeField, Tooltip("칭호")] private GameObject namePlateListPopup;
    [SerializeField, Tooltip("닉네임 변경")] private GameObject nickNameChangePopup;
    
    [Header("기본창 전환")]
    [SerializeField, Tooltip("프로필 변경")] private GameObject profilePanel;
    [SerializeField, Tooltip("랭킹 변경")] private GameObject rankPanel;
    
    [SerializeField, Tooltip("프로필 토글")] private Toggle profileToggle;
    [SerializeField, Tooltip("랭킹 토글")] private Toggle rankToggle;
    
    [Header("기본창")]
    [SerializeField, Tooltip("레벨")] private TMP_Text levelText;
    [SerializeField, Tooltip("닉네임")] private TMP_Text nicknameText;
    [SerializeField, Tooltip("경험치")] private TMP_Text expText;
    [SerializeField, Tooltip("칭호")] private TMP_Text nameTitleText;
    [SerializeField, Tooltip("칭호 버튼")] private Button nameTitleButton;
    [SerializeField, Tooltip("환생 토글")] private Toggle reincarnationToggle;
    [SerializeField, Tooltip("환생 설명 버튼")] private Button explanationButton;
    [SerializeField, Tooltip("경험치 게이지")] private Image expFillImage;
    
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
    [SerializeField, Tooltip("확정 팝업")] private GameObject ReincarnationConfirmPopup;
    [SerializeField, Tooltip("확정 닫기 버튼")] private Button ReincarnationConfirmCloseButton;
    [SerializeField, Tooltip("환생 버튼")] private Button ReincarnationConfirmButton;
    [SerializeField, Tooltip("설명 팝업")] private GameObject ReincarnationInfoPopup;
    [SerializeField, Tooltip("설명 닫기 버튼")] private Button ReincarnationInfoCloseButton;
    
    
    private void OnEnable()
    {
        TextSetUp();
    }
    
    // 텍스트 셋업
    public void TextSetUp()
    {
        //Debug.Log("텍스트 셋업");
        levelText.text = DataManager.Instance.PlayerData.level.ToString();
        nicknameText.text = DataManager.Instance.PlayerData.nickname;
        expText.text = $"{DataManager.Instance.PlayerData.exp}/{DataManager.Instance.PlayerData.levelMax} EXP";
        nameTitleText.text = DataManager.Instance.PlayerData.curNamePlateType.ToString();
        
    }
    
    private void Awake()
    {
        PopupPanelClose();
        noneNameTiltleLabel.SetActive(false);
        changeNicknameButton.gameObject.SetActive(false);
        gemchangeNicknameButton.gameObject.SetActive(false);
        nickNameAnswerPopup.gameObject.SetActive(false);
        notEnoughGemPopup.SetActive(false);
        ReincarnationConfirmPopup.SetActive(false);
        ReincarnationInfoPopup.SetActive(false);
        
        nameTitleList.Clear();
        foreach (Transform child in inputTransform)
        {
            nameTitleList.Add(child.gameObject);
            child.gameObject.SetActive(false);
        }
    }
    
    private void Start()
    {
        UpdateBasePanels();
        
        profileToggle.onValueChanged.AddListener(_ => UpdateBasePanels());
        rankToggle.onValueChanged.AddListener(_ => UpdateBasePanels());
        reincarnationToggle.onValueChanged.AddListener(_ => IsreincarnationToggle());
        nameTitleButton.onClick.AddListener(() => NamaTitleOpen());
        platePopupCloseButton.onClick.AddListener(() => PopupPanelClose());
        nickPopupCloseButton.onClick.AddListener(() => PopupPanelClose());
        closeButton.onClick.AddListener(()=> UIManager.Instance.Open(PanelId.MainPanel));
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
            Debug.Log("상점이동");
            PopupPanelClose();
        });
        
        ReincarnationConfirmCloseButton.onClick.AddListener(() => ReincarnationConfirmPopup.SetActive(false));
        ReincarnationInfoCloseButton.onClick.AddListener(() => ReincarnationInfoPopup.SetActive(true));
        ReincarnationConfirmButton.onClick.AddListener(ReincarnationButton);
    }

    // 기본판넬 토글 on/off
    private void UpdateBasePanels()
    {
        profilePanel.SetActive(profileToggle.isOn);
        rankPanel.SetActive(rankToggle.isOn);
    }

    // 환생 토글버튼 on/off
    private void IsreincarnationToggle()
    {
        if (DataManager.Instance.PlayerData.level == DataManager.Instance.PlayerData.levelMax)
        {
            reincarnationToggle.isOn = true;
            ReincarnationButton();
        }
        else
            reincarnationToggle.isOn = false;
    }
    
    // 환생하기 
    private void ReincarnationButton()
    {
        DataManager.Instance.PlayerData.level = 0;
        DataManager.Instance.PlayerData.exp = 0;
        
        int num = DataManager.Instance.PlayerData.gainNamePlateType.Count;
        if (Enum.IsDefined(typeof(NamePlateType), num))
        {
            DataManager.Instance.PlayerData.gainNamePlateType.Add((NamePlateType)num);
        }
        else
        {
            Debug.LogWarning($"NamePlateType에 {num}번째 enum이 없습니다!");
        }
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
            if (type == NamePlateType.NONE)
                continue;

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
        DataManager.Instance.PlayerData.nickname = changeNicknameInputField.text;
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
            DataManager.Instance.PlayerData.nickname = changeNicknameInputField.text;
            DataManager.Instance.PlayerData.changeNicknameCount++;
            DataManager.Instance.PlayerData.gem -= int.Parse(gemText);
            nickNameAnswerPopup.SetActive(false);
            nickNameChangePopup.SetActive(false);
            
            nicknameText.text = DataManager.Instance.PlayerData.nickname;
        }
        
    }
    
}

