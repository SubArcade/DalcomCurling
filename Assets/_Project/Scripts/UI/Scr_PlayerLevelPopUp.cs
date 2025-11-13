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
    
    [Header("칭호 on/off 기능")]
    [SerializeField, Tooltip("프리팹 들어갈 곳")] private Transform inputTransform;
    [SerializeField, Tooltip("칭호 라벨 텍스트")] private GameObject noneNameTiltleLabel;
    [SerializeField] private List<GameObject> nameTitleList = new List<GameObject>();
    
    [Header("닫기")] 
    [SerializeField, Tooltip("닫기 버튼")] private Button closeButton;
    [SerializeField, Tooltip("팝업창 닫기 버튼")] private Button platePopupCloseButton;
    [SerializeField, Tooltip("팝업창 닫기 버튼")] private Button nickPopupCloseButton;
    
    
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
        noneNameTiltleLabel.SetActive(false);
        changeNicknameButton.gameObject.SetActive(false);
        gemchangeNicknameButton.gameObject.SetActive(false);
        
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
        changeNicknameButton.onClick.AddListener(()=>DataManager.Instance.PlayerData.nickname = changeNicknameInputField.text);
        changeNicknameButton.onClick.AddListener(()=>DataManager.Instance.PlayerData.nickname = changeNicknameInputField.text);
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
        if(DataManager.Instance.PlayerData.level == DataManager.Instance.PlayerData.levelMax)
            reincarnationToggle.isOn = true;
        else
            reincarnationToggle.isOn = false;
    }

    // 팝업창 전체 닫기
    private void PopupPanelClose()
    {
        namePlateListPopup.SetActive(false);
        nickNameChangePopup.SetActive(false);
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
            // NONE 스킵
            if (type == NamePlateType.NONE)
                continue;

            stack++;

            // 리스트에서 해당 칭호를 가진 프리팹 찾아서 활성화
            foreach (var obj in nameTitleList)
            {
                var ui = obj.GetComponent<Scr_NameTitle>();
                if (ui.namePlateType == type)
                {
                    obj.SetActive(true);

                    // 토글 이벤트 추가
                    obj.GetComponentInChildren<Toggle>().onValueChanged.AddListener(_ => OnSelectNameTitle(obj, type));
                    
                    if (type == DataManager.Instance.PlayerData.curNamePlateType)
                        obj.GetComponentInChildren<Toggle>().isOn = true;
                }
            }
        }
        
        if(stack == 0)
            noneNameTiltleLabel.SetActive(true);
    }
    
    // 선택된 토글만 활성화
    public void OnSelectNameTitle(GameObject selectedObj, NamePlateType type)
    {
        var selectedToggle = selectedObj.GetComponentInChildren<Toggle>();
        
        if (DataManager.Instance.PlayerData.curNamePlateType == type)
        {
            selectedToggle.isOn = false;
            DataManager.Instance.PlayerData.curNamePlateType = NamePlateType.NONE;
            return;
        }
        
        foreach (var obj in nameTitleList)
        {
            var toggle = obj.GetComponentInChildren<Toggle>();
            if (toggle == null) continue;

            // 선택된 토글은 ON
            toggle.isOn = (obj == selectedObj);
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
}

