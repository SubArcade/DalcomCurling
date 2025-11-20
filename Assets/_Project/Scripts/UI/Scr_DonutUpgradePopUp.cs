using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Scr_DonutUpgradePopUp : MonoBehaviour
{
    private DataManager Data => DataManager.Instance;

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
    
    private const int MaxLevel = 20;
    void Awake()
    {
        //업글버튼 누르면 바로 업글 되게 해놓음
        //데이터베이스에서 돈이랑 연결해서 업글버튼누르면
        //돈이 있다면 askupgrade팝업
        //돈이없다면 failupgrade팝업 뜨도록 코드 추가
        //각 팝업에 존재하는 버튼들과 텍스트 인스펙터에 자동으로 잡히고 애드리스너등록
        Transform canvas = GameObject.Find("Canvas")?.transform;
        CloseButton = transform.Find("Panel/CloseButton")?.GetComponent<Button>();
        HardUpgradeButton = transform.Find("Panel/ButtonPanel/HardUpgradePanel/HardUpgradeButton")?.GetComponent<Button>();
        MoistUpgradeButton = transform.Find("Panel/ButtonPanel/MoistUpgradePanel/MoistUpgradeButton")?.GetComponent<Button>();
        SoftUpgradeButton = transform.Find("Panel/ButtonPanel/SoftUpgradePanel/SoftUpgradeButton")?.GetComponent<Button>();

        HardUpgradeMax = transform.Find("Panel/ButtonPanel/HardUpgradePanel/HardUpgradeMax")?.gameObject;
        MoistUpgradeMax = transform.Find("Panel/ButtonPanel/MoistUpgradePanel/MoistUpgradeMax")?.gameObject;
        SoftUpgradeMax = transform.Find("Panel/ButtonPanel/SoftUpgradePanel/SoftUpgradeMax")?.gameObject;

        HardDonutCreatText = transform.Find("Panel/ButtonPanel/HardUpgradePanel/HardDonutCreate_Text")?.GetComponent<TextMeshProUGUI>();
        MoistDonutCreatText = transform.Find("Panel/ButtonPanel/MoistUpgradePanel/MoistDonutCreate_Text")?.GetComponent<TextMeshProUGUI>();
        SoftDonutCreatText = transform.Find("Panel/ButtonPanel/SoftUpgradePanel/SoftDonutCreate_Text")?.GetComponent<TextMeshProUGUI>();

        HardDonutOptionText = transform.Find("Panel/ButtonPanel/HardUpgradePanel/UpgradeOptionLabel/HardDonutOption_Text")?.GetComponent<TextMeshProUGUI>();
        MoistDonutOptionText = transform.Find("Panel/ButtonPanel/MoistUpgradePanel/UpgradeOptionLabel/MoistDonutOption_Text")?.GetComponent<TextMeshProUGUI>();
        SoftDonutOptionText = transform.Find("Panel/ButtonPanel/SoftUpgradePanel/UpgradeOptionLabel/SoftDonutOption_Text")?.GetComponent<TextMeshProUGUI>();

        AskUpgradePopUp = canvas.Find("AskUpgradePopup")?.gameObject;
        FailUpgradePopUp = canvas.Find("FailUpgradePopup")?.gameObject;
    }

    void Start()
    {
        CloseButton.onClick.AddListener(OnClickClosePopUp);
        //HardUpgradeButton.onClick.AddListener(() =>
        //UpgradeDonut(ref hardDonutLevel, "단단", HardDonutCreatText, HardDonutOptionText, HardUpgradeButton, HardUpgradeMax));

        //MoistUpgradeButton.onClick.AddListener(() =>
        //UpgradeDonut(ref moistDonutLevel, "촉촉", MoistDonutCreatText, MoistDonutOptionText, MoistUpgradeButton, MoistUpgradeMax));

        //SoftUpgradeButton.onClick.AddListener(() =>
        //UpgradeDonut(ref softDonutLevel, "말랑", SoftDonutCreatText, SoftDonutOptionText, SoftUpgradeButton, SoftUpgradeMax));

        HardUpgradeButton.onClick.AddListener(OnClickHardUpgrade);
        MoistUpgradeButton.onClick.AddListener(OnClickMoistUpgrade);
        SoftUpgradeButton.onClick.AddListener(OnClickSoftUpgrade);

        updateAllText();
    }
    void OnEnable()
    {
        LocalizationManager.Instance.OnLanguageChanged += updateAllText;
        updateAllText(); // 팝업이 열릴 때도 즉시 반영
    }

    void OnDisable()
    {
        LocalizationManager.Instance.OnLanguageChanged -= updateAllText;
    }

    //단단 업글
    void OnClickHardUpgrade()
    {
        int hardLevel = Data.MergeBoardData.generatorLevelHard;

        if (hardLevel >= MaxLevel)
        {
            HardUpgradeButton.interactable = false;
            HardUpgradeMax.SetActive(true);
            return;
        }

        hardLevel++;
        Data.MergeBoardData.generatorLevelHard = hardLevel;

        UpdateDonutText(hardLevel, "단단", HardDonutCreatText, HardDonutOptionText);
    }

    //촉촉 업글
    void OnClickMoistUpgrade()
    {
        int moistLevel = Data.MergeBoardData.generatorLevelMoist;

        if (moistLevel >= MaxLevel)
        {
            MoistUpgradeButton.interactable = false;
            MoistUpgradeMax.SetActive(true);
            return;
        }

        moistLevel++;
        Data.MergeBoardData.generatorLevelMoist = moistLevel;

        UpdateDonutText(moistLevel, "촉촉", MoistDonutCreatText, MoistDonutOptionText);
    }

    //말랑 업글
    void OnClickSoftUpgrade()
    {
        int softLevel = Data.MergeBoardData.generatorLevelSoft;

        if (softLevel >= MaxLevel)
        {
            SoftUpgradeButton.interactable = false;
            SoftUpgradeMax.SetActive(true);
            return;
        }

        softLevel++;
        Data.MergeBoardData.generatorLevelSoft = softLevel;

        UpdateDonutText(softLevel, "말랑", SoftDonutCreatText, SoftDonutOptionText);
    }

    ////도넛 업그레이드 시 레벨업 & 만렙 도달시 업그레이드 비활성화
    //void UpgradeDonut(ref int level,string type, TextMeshProUGUI createText, TextMeshProUGUI optionText, Button upgradeButton, GameObject maxlevelPanel)
    //{
    //    if (level >= MaxLevel)
    //    {
    //        upgradeButton.interactable = false;
    //        maxlevelPanel.SetActive(true);
    //        return;
    //    }
    //    level++;
    //    UpdateDonutText(level,type, createText, optionText);

    //    if (level >= MaxLevel)
    //    {
    //        upgradeButton.interactable = false;
    //        maxlevelPanel.SetActive(true);
    //    }
    //}

    //도넛 레벨에 따라 텍스트 갱신
    void UpdateDonutText(int level,string type, TextMeshProUGUI createText, TextMeshProUGUI optionText) 
    {
        LocalizationKey typeKey;

        switch (type)
        {
            case "단단":
                typeKey = LocalizationKey.Label_HardDonut;
                break;
            case "촉촉":
                typeKey = LocalizationKey.Label_MoistDonut;
                break;
            case "말랑":
                typeKey = LocalizationKey.Label_SoftDonut;
                break;
            default:            
                typeKey = LocalizationKey.Label_HardDonut;
                break;
        }

        string localizedType = LocalizationManager.Instance.GetText(typeKey);
        string generatorText = LocalizationManager.Instance.GetText(LocalizationKey.Up_generator);
        string chanceText = LocalizationManager.Instance.GetText(LocalizationKey.Up_generatorUP);

        createText.text = $"Lv{level} {localizedType} {generatorText}";
        optionText.text = $"Lv{level} {localizedType} {chanceText}";
    }

    void updateAllText() 
    {
        //UpdateDonutText(hardDonutLevel,"단단", HardDonutCreatText, HardDonutOptionText);
        //UpdateDonutText(moistDonutLevel,"촉촉", MoistDonutCreatText, MoistDonutOptionText);
        //UpdateDonutText(softDonutLevel,"말랑" ,SoftDonutCreatText, SoftDonutOptionText);
        UpdateDonutText(Data.MergeBoardData.generatorLevelHard, "단단", HardDonutCreatText, HardDonutOptionText);
        UpdateDonutText(Data.MergeBoardData.generatorLevelMoist, "촉촉", MoistDonutCreatText, MoistDonutOptionText);
        UpdateDonutText(Data.MergeBoardData.generatorLevelSoft, "말랑", SoftDonutCreatText, SoftDonutOptionText);
    }
    void OnClickClosePopUp() 
    {
        UIManager.Instance.Close(PanelId.DonutUpgradePopup);
    }
}
