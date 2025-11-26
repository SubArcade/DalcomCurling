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
    
    //TODO : 업그레이드 코스트 TextUI 연결
    //UpgradeCost_Text로 되어있는 금액 텍스트 UpgradeCost 만큼 증가하게
    //AskUpgradePopup에 있는 텍스트도 출력될때 UpgradeCost 노출
    //금액소모 시 Data값은 들어감 현재남은 재화 UI를 새로고침 해주는함수 호출해야함

    //도넛 업그레이드 레벨 체크용도
    private const int MaxLevel = 20;

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

        int playerLevel = Data.PlayerData.level;
        if (hardLevel >= playerLevel) return;

        int cost = GetUpgradeCost(hardLevel);
        if (Data.PlayerData.gold < cost)
        {
            FailUpgradePopUp.SetActive(true);
            return;
        }
        Data.PlayerData.gold -= cost;

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

        int playerLevel = Data.PlayerData.level;
        if (moistLevel >= playerLevel) return;

        int cost = GetUpgradeCost(moistLevel);
        if (Data.PlayerData.gold < cost)
        {
            FailUpgradePopUp.SetActive(true);
            return;
        }
        Data.PlayerData.gold -= cost;

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

        int playerLevel = Data.PlayerData.level;
        if (softLevel >= playerLevel) return;

        int cost = GetUpgradeCost(softLevel);
        if (Data.PlayerData.gold < cost)
        {
            FailUpgradePopUp.SetActive(true);
            return;
        }
        Data.PlayerData.gold -= cost;

        softLevel++;
        Data.MergeBoardData.generatorLevelSoft = softLevel;

        UpdateDonutText(softLevel, "말랑", SoftDonutCreatText, SoftDonutOptionText);
    }

    //업그레이드 비용
    private int GetUpgradeCost(int level)
    {
        if (level <= 10)
        {
            return level * 1000;
        }

        return 10000 + (level * 5000);
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
