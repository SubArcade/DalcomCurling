using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scr_ShopPopUp : MonoBehaviour
{
    [Header("캐시상점 토글")]
    [SerializeField] private Toggle cashShopBtn;

    [Header("골드상점 토글")]
    [SerializeField] private Toggle goldShopBtn;

    [Header("로비로 돌아가기 버튼")]
    [SerializeField] private Button robbyButton;

    [Header("에너지 골드 보석")]
    [SerializeField] private Button eneryBtn;
    [SerializeField] private Button goldBtn;
    [SerializeField] private Button gemBtn;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI gemText;

    [Header("광고제거 아이템&패키지 아이템")]
    [SerializeField] private Button deleteAdsBtn1;
    [SerializeField] private Button deleteAdsBtn2;
    [SerializeField] private Button deleteAdsBtn3;
    [SerializeField] private Button packageBtn1;
    [SerializeField] private Button packageBtn2;

    [Header("이펙트&캐릭터 아이템")]
    [SerializeField] private Button effectItem1;
    [SerializeField] private Button effectItem2;
    [SerializeField] private Button effectItem3;
    [SerializeField] private Button characterItem1;
    [SerializeField] private Button characterItem2;
    [SerializeField] private Button characterItem3;
    [SerializeField] private Button characterItem4;

    //미완성된 보석 현질 버튼 추가해야함

    void Awake()
    {
        cashShopBtn = transform.Find("BottomUI/CashShopBtn")?.GetComponent<Toggle>();
        goldShopBtn = transform.Find("BottomUI/GoldShopBtn")?.GetComponent<Toggle>();
        
        robbyButton = transform.Find("TopUI/PlayerMoneyGroup/RobbyButton")?.GetComponent<Button>();
        eneryBtn = transform.Find("TopUI/PlayerMoneyGroup/Energy")?.GetComponent<Button>();
        goldBtn = transform.Find("TopUI/PlayerMoneyGroup/Gold")?.GetComponent<Button>();
        gemBtn = transform.Find("TopUI/PlayerMoneyGroup/Gem")?.GetComponent<Button>();

        energyText = transform.Find("TopUI/PlayerMoneyGroup/Energy/Energy_Text")?.GetComponent<TextMeshProUGUI>();
        goldText = transform.Find("TopUI/PlayerMoneyGroup/Gold/Gold_Text")?.GetComponent<TextMeshProUGUI>();
        gemText = transform.Find("TopUI/PlayerMoneyGroup/Gem/Gem_Text")?.GetComponent<TextMeshProUGUI>();

        deleteAdsBtn1 = transform.Find("CashUIBackground/CashShopUI/MonthlyPlanBanner/DeleteAdsItem1")?.GetComponent<Button>();
        deleteAdsBtn2 = transform.Find("CashUIBackground/CashShopUI/MonthlyPlanBanner/DeleteAdsItem2")?.GetComponent<Button>();
        deleteAdsBtn3 = transform.Find("CashUIBackground/CashShopUI/MonthlyPlanBanner/DeleteAdsItem3")?.GetComponent<Button>();
        packageBtn1 = transform.Find("CashUIBackground/CashShopUI/MonthlyPlanBanner/PackageItem1")?.GetComponent<Button>();
        packageBtn2 = transform.Find("CashUIBackground/CashShopUI/MonthlyPlanBanner/PackageItem2")?.GetComponent<Button>();

        effectItem1 = transform.Find("GoldUIBackground/GoldShopUI/DonutBanner/EffectItem1")?.GetComponent<Button>();
        effectItem2 = transform.Find("GoldUIBackground/GoldShopUI/DonutBanner/EffectItem2")?.GetComponent<Button>();
        effectItem3 = transform.Find("GoldUIBackground/GoldShopUI/DonutBanner/EffectItem3")?.GetComponent<Button>();

        characterItem1 = transform.Find("GoldUIBackground/GoldShopUI/CharacterBanner/CharacterItem1")?.GetComponent<Button>();
        characterItem2 = transform.Find("GoldUIBackground/GoldShopUI/CharacterBanner/CharacterItem2")?.GetComponent<Button>();
        characterItem3 = transform.Find("GoldUIBackground/GoldShopUI/CharacterBanner/CharacterItem3")?.GetComponent<Button>();
        characterItem4 = transform.Find("GoldUIBackground/GoldShopUI/CharacterBanner/CharacterItem4")?.GetComponent<Button>();

    }
    void Start()
    {
        //캐시상점 열기
        cashShopBtn.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                goldShopBtn.isOn = false;
                ShowCashShopUI();
            }
        });

        //골드상점열기
        goldShopBtn.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                cashShopBtn.isOn = false;
                ShowGoldShopUI();
            }
        });
        //로비로 돌아가버리는 버튼
        robbyButton.onClick.AddListener(() =>
        {
            UIManager.Instance.Close(PanelId.ShopPopUp);
        });

        SetPlayerText(DataManager.Instance.PlayerData);
        eneryBtn.onClick.AddListener(() => UIManager.Instance.Open(PanelId.EnergyRechargePopUp));
    }
    void OnEnable()
    {
        DataManager.Instance.OnUserDataChanged += HandleUserDataChanged;
        SetPlayerText(DataManager.Instance.PlayerData);
    }

    void OnDisable()
    {
        DataManager.Instance.OnUserDataChanged -= HandleUserDataChanged;
    }
    private void HandleUserDataChanged(PlayerData playerData)
    {
        SetPlayerText(playerData);
    }

    //캐시샵열기용
    private void ShowCashShopUI()
    {
        transform.Find("CashUIBackground").gameObject.SetActive(true);
        transform.Find("GoldUIBackground").gameObject.SetActive(false);
    }
    //골드샵열기용
    private void ShowGoldShopUI()
    {
        transform.Find("CashUIBackground").gameObject.SetActive(false);
        transform.Find("GoldUIBackground").gameObject.SetActive(true);
    }

    public void SetPlayerText(PlayerData playerData)
    {
        energyText.text = $"{playerData.energy}/{playerData.maxEnergy}";
        goldText.text = $"{playerData.gold}";
        gemText.text = $"{playerData.gem}";
    }

}
