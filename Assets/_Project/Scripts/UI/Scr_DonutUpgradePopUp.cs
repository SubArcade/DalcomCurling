using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GeneratorType
{
    Hard,
    Soft,
    Moist
}

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

    [Header("업그레이드 팝업")]
    [SerializeField] private GameObject AskUpgradePopUp;
    [SerializeField] private Button agreeUpgradeButton;
    [SerializeField] private Button cancelUpgradeButton;
    private GeneratorType pendingUpgradeType;
    [SerializeField] private TextMeshProUGUI popupDescText;

    //TODO : 업그레이드 코스트 TextUI 연결
    //UpgradeCost_Text로 되어있는 금액 텍스트 UpgradeCost 만큼 증가하게
    //AskUpgradePopup에 있는 텍스트도 출력될때 UpgradeCost 노출

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

        HardUpgradeButton.onClick.AddListener(() => OpenAskUpgradePopup(GeneratorType.Hard));
        MoistUpgradeButton.onClick.AddListener(() => OpenAskUpgradePopup(GeneratorType.Moist));
        SoftUpgradeButton.onClick.AddListener(() => OpenAskUpgradePopup(GeneratorType.Soft));
        agreeUpgradeButton.onClick.AddListener(OnClickAgreeUpgrade);
        cancelUpgradeButton.onClick.AddListener(() => AskUpgradePopUp.gameObject.SetActive(false));

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

    private void OpenAskUpgradePopup(GeneratorType type)
    {
        pendingUpgradeType = type;

        // 현재 레벨 가져오기
        int currentLevel = GetCurrentGeneratorLevel(type);
        int nextLevel = currentLevel + 1;

        // 타입 한글 이름
        string typeName = type switch
        {
            GeneratorType.Hard => "단단 도넛",
            GeneratorType.Soft => "말랑 도넛",
            GeneratorType.Moist => "촉촉 도넛",
            _ => ""
        };

        // 팝업 텍스트 갱신
        popupDescText.text =
            $"[{typeName}] 생성기를 업그레이드 할까요?\n" +
            $"({currentLevel}단계 → {nextLevel}단계 업그레이드)\n" +
            $"도넛 생성 확률이 증가합니다.";

        // 팝업 열기
        AskUpgradePopUp.SetActive(true);
    }

    private void OnClickAgreeUpgrade()
    {
        UpgradeGenerator(pendingUpgradeType);

        // 업그레이드 후 텍스트 갱신
        updateAllText();

        // 팝업 닫기
        AskUpgradePopUp.SetActive(false);
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
            //비활성화로 만들어야함.
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
            //비활성화로 만들어야함.
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
            //비활성화로 만들어야함.
            return;
        }
        Data.PlayerData.gold -= cost;

        softLevel++;
        Data.MergeBoardData.generatorLevelSoft = softLevel;

        UpdateDonutText(softLevel, "말랑", SoftDonutCreatText, SoftDonutOptionText);
    }

    // 비활성화 확인용
    private void UpdateUpgradeButtonState()
    {
        int hardLevel = Data.MergeBoardData.generatorLevelHard;
        int moistLevel = Data.MergeBoardData.generatorLevelMoist;
        int softLevel = Data.MergeBoardData.generatorLevelSoft;

        int hardCost = GetUpgradeCost(hardLevel);
        int moistCost = GetUpgradeCost(moistLevel);
        int softCost = GetUpgradeCost(softLevel);

        int gold = Data.PlayerData.gold;
        int playerLevel = Data.PlayerData.level;

        bool hardEnabled =
        gold >= hardCost &&
        hardLevel < MaxLevel &&
        hardLevel < playerLevel;

        bool moistEnabled =
            gold >= moistCost &&
            moistLevel < MaxLevel &&
            moistLevel < playerLevel;

        bool softEnabled =
            gold >= softCost &&
            softLevel < MaxLevel &&
            softLevel < playerLevel;

        HardUpgradeButton.interactable = hardEnabled;
        MoistUpgradeButton.interactable = moistEnabled;
        SoftUpgradeButton.interactable = softEnabled;

        // 최대레벨일 때 Max 패널 ON
        HardUpgradeMax.SetActive(hardLevel >= MaxLevel);
        MoistUpgradeMax.SetActive(moistLevel >= MaxLevel);
        SoftUpgradeMax.SetActive(softLevel >= MaxLevel);
    }


    //생성기레벨 참조
    private int GetCurrentGeneratorLevel(GeneratorType type)
    {
        return type switch
        {
            GeneratorType.Hard => Data.MergeBoardData.generatorLevelHard,
            GeneratorType.Soft => Data.MergeBoardData.generatorLevelSoft,
            GeneratorType.Moist => Data.MergeBoardData.generatorLevelMoist,
            _ => 0
        };
    }

    //업그레이드 종류선택
    private void UpgradeGenerator(GeneratorType type)
    {
        switch (type)
        {
            case GeneratorType.Hard:
                OnClickHardUpgrade();
                break;

            case GeneratorType.Soft:
                OnClickSoftUpgrade();
                break;

            case GeneratorType.Moist:
                OnClickMoistUpgrade();
                break;
        }
    }

    //업그레이드 비용
    private int GetUpgradeCost(int level)
    {
        if (level <= 10)
        {
            return level * 1000;
        }

        return 10000 + ((level - 10) * 5000);
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

        UpdateUpgradeButtonState();
    }
    void OnClickClosePopUp() 
    {
        UIManager.Instance.Close(PanelId.DonutUpgradePopup);
    }
}
