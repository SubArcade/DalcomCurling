using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Scr_DonutUpgradePopUp : MonoBehaviour
{
    [Header("창 닫기 버튼")]
    [SerializeField] private Button CloseButton;

    [Header("업그레이드 버튼")]
    [SerializeField] private Button HardUpgradeButton;
    [SerializeField] private Button MoistUpgradeButton;
    [SerializeField] private Button SoftUpgradeButton;

    [Header("최대 업글 도달 패널")]
    [SerializeField] private GameObject HardUpgradeMax;
    [SerializeField] private GameObject MoistUpgradeMax;
    [SerializeField] private GameObject SoftUpgradeMax;

    [Header("생성기 텍스트")]
    [SerializeField] private TextMeshProUGUI HardDonutCreatText;
    [SerializeField] private TextMeshProUGUI MoistDonutCreatText;
    [SerializeField] private TextMeshProUGUI SoftDonutCreatText;

    [Header("생성기 설명텍스트")]
    [SerializeField] private TextMeshProUGUI HardDonutOptionText;
    [SerializeField] private TextMeshProUGUI MoistDonutOptionText;
    [SerializeField] private TextMeshProUGUI SoftDonutOptionText;

    [Header("업그레이드 질문 팝업")]
    [SerializeField] private GameObject AskUpgradePopUp;
    [SerializeField] private GameObject FailUpgradePopUp;

    
    //도넛 업그레이드 레벨 체크용도
    private int hardDonutLevel = 1;
    private int moistDonutLevel = 1;
    private int softDonutLevel = 1;
    private const int MaxLevel = 20;
    void Awake()
    {
        //업글버튼 누르면 바로 업글 되게 해놓음
        //데이터베이스에서 돈이랑 연결해서 업글버튼누르면
        //돈이 있다면 askupgrade팝업
        //돈이없다면 failupgrade팝업 뜨도록 코드 추가
        //각 팝업에 존재하는 버튼들과 텍스트 인스펙터에 자동으로 잡히고 애드리스너등록
        AwakeInspector();
    }

    void Start()
    {
        Startzip();
    }

    void AwakeInspector() 
    {
        Transform canvas = GameObject.Find("Canvas")?.transform;
        CloseButton = transform.Find("CloseButton")?.GetComponent<Button>();
        HardUpgradeButton = transform.Find("HardUpgradePanel/HardUpgradeButton")?.GetComponent<Button>();
        MoistUpgradeButton = transform.Find("MoistUpgradePanel/MoistUpgradeButton")?.GetComponent<Button>();
        SoftUpgradeButton = transform.Find("SoftUpgradePanel/SoftUpgradeButton")?.GetComponent<Button>();

        HardUpgradeMax = transform.Find("HardUpgradePanel/HardUpgradeMax")?.gameObject;
        MoistUpgradeMax = transform.Find("MoistUpgradePanel/MoistUpgradeMax")?.gameObject;
        SoftUpgradeMax = transform.Find("SoftUpgradePanel/SoftUpgradeMax")?.gameObject;

        HardDonutCreatText = transform.Find("HardUpgradePanel/HardDonutCreateText")?.GetComponent<TextMeshProUGUI>();
        MoistDonutCreatText = transform.Find("MoistUpgradePanel/MoistDonutCreateText")?.GetComponent<TextMeshProUGUI>();
        SoftDonutCreatText = transform.Find("SoftUpgradePanel/SoftDonutCreateText")?.GetComponent<TextMeshProUGUI>();

        HardDonutOptionText = transform.Find("HardUpgradePanel/UpgradeOptionLabel/HardDonutOptionText")?.GetComponent<TextMeshProUGUI>();
        MoistDonutOptionText = transform.Find("MoistUpgradePanel/UpgradeOptionLabel/MoistDonutOptionText")?.GetComponent<TextMeshProUGUI>();
        SoftDonutOptionText = transform.Find("SoftUpgradePanel/UpgradeOptionLabel/SoftDonutOptionText")?.GetComponent<TextMeshProUGUI>();

        AskUpgradePopUp = canvas.Find("AskUpgradePopUp")?.gameObject;
        FailUpgradePopUp = canvas.Find("FailUpgradePopUp")?.gameObject;     
    }
    void Startzip() 
    {
        CloseButton.onClick.AddListener(OnClickClosePopUp);
        HardUpgradeButton.onClick.AddListener(() => 
        UpgradeDonut(ref hardDonutLevel,"단단", HardDonutCreatText, HardDonutOptionText, HardUpgradeButton, HardUpgradeMax));
        
        MoistUpgradeButton.onClick.AddListener(() => 
        UpgradeDonut(ref moistDonutLevel,"촉촉" ,MoistDonutCreatText, MoistDonutOptionText, MoistUpgradeButton, MoistUpgradeMax));
        
        SoftUpgradeButton.onClick.AddListener(() => 
        UpgradeDonut(ref softDonutLevel, "말랑",SoftDonutCreatText, SoftDonutOptionText, SoftUpgradeButton, SoftUpgradeMax));
        
        updateAllText();
    }

    //도넛 업그레이드 시 레벨업 & 만렙 도달시 업그레이드 비활성화
    void UpgradeDonut(ref int level,string type, TextMeshProUGUI createText, TextMeshProUGUI optionText, Button upgradeButton, GameObject maxlevelPanel)
    {
        if (level >= MaxLevel)
        {
            upgradeButton.interactable = false;
            maxlevelPanel.SetActive(true);
            return;
        }
        level++;
        UpdateDonutText(level,type, createText, optionText);

        if (level >= MaxLevel)
        {
            upgradeButton.interactable = false;
            maxlevelPanel.SetActive(true);
        }
    }

    //도넛 레벨에 따라 텍스트 갱신
    void UpdateDonutText(int level,string type, TextMeshProUGUI createText, TextMeshProUGUI optionText) 
    {
        createText.text = $"Lv{level} {type}도넛 생성기";
        optionText.text = $"{level}단계 {type}도넛 생성확률 증가";
    }

    void updateAllText() 
    {
        UpdateDonutText(hardDonutLevel,"단단", HardDonutCreatText, HardDonutOptionText);
        UpdateDonutText(moistDonutLevel,"촉촉", MoistDonutCreatText, MoistDonutOptionText);
        UpdateDonutText(softDonutLevel,"말랑" ,SoftDonutCreatText, SoftDonutOptionText);

    }
    void OnClickClosePopUp() 
    {
        UIManager.Instance.Open(PanelId.MainPanel);
    }
}
