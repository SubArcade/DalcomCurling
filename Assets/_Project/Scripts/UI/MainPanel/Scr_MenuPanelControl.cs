using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scr_MenuPanelControl : MonoBehaviour
{
    [Header("화면 전환 캔버스(판넬)")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject detailedSettigsPanel;
    [SerializeField] private GameObject giftboxPopup;
    [SerializeField] private GameObject playerLevelInfoPopup;
    [SerializeField] private GameObject askUpgradePopup;
    [SerializeField] private GameObject failUpgradePopup;
    [SerializeField] private GameObject donutCodexPopup;
    [SerializeField] private GameObject donutUpgradePopup;
    [SerializeField] private GameObject donutCodexClickPopup;
    [SerializeField] private GameObject rewardCheckPopup;
    [SerializeField] private GameObject EntryPopUp;
    [SerializeField] private GameObject MatchingPopUp;
    [SerializeField] private GameObject readyMenuPanel;
    [SerializeField] private GameObject ShopPopUp;
    [SerializeField] private GameObject energyRechargePopUp;
    [SerializeField] private GameObject notEnoughGemPopup;
    [SerializeField] private GameObject orderRefreshPopup;
    [SerializeField] private GameObject useGiftBoxPopUp;
    [SerializeField] private GameObject deleteAccountPopUp;
    [SerializeField] private GameObject gameHelpPopUp;
    [SerializeField] private GameObject levelUpRewardPopUp;
    [SerializeField] private GameObject useGiftBoxPopup;
    [SerializeField] private GameObject guestPopup;

    [Header("화면 전환 버튼")]
    [SerializeField] private Button playerLevelInfoButton;
    [SerializeField] private Button donutCodexButton;   // 도감
    [SerializeField] private Button donutUpgradeButton; // 업그레이드
    [SerializeField] private Button entryPopUpButton;   // 엔트리 팝업
    [SerializeField] private Button detailedSettingsButton;
    [SerializeField] private Button readyButton;    // 레디
    [SerializeField] private Button startBattle;    // 매칭 시작
    [SerializeField] private Button goldShopButton; // 상점
    [SerializeField] private Button gemShopButton;  // 상점
    [SerializeField] private Button energyRechargeButton;
    [SerializeField] private Button orderRefreshButton;
    
    
    [Header("플레이어 데이터 값")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text gemText;
    
    public GameObject testLoginPanel;
    public Button testLevelUpButton;
    
    void Awake()
    {
        UIManager.Instance.RegisterPanel(PanelId.StartPanel,startPanel);    
        UIManager.Instance.RegisterPanel(PanelId.LoginPanel,loginPanel);    
        UIManager.Instance.RegisterPanel(PanelId.MainPanel,mainPanel);    
        UIManager.Instance.RegisterPanel(PanelId.DetailedSettingsPanel,detailedSettigsPanel);    
        UIManager.Instance.RegisterPanel(PanelId.GiftboxPopup,giftboxPopup);
        UIManager.Instance.RegisterPanel(PanelId.PlayerLevelInfoPopup,playerLevelInfoPopup);
        UIManager.Instance.RegisterPanel(PanelId.DonutCodexPopup,donutCodexPopup);
        UIManager.Instance.RegisterPanel(PanelId.DonutUpgradePopup,donutUpgradePopup);
        UIManager.Instance.RegisterPanel(PanelId.EntryPopUp, EntryPopUp);
        UIManager.Instance.RegisterPanel(PanelId.MatchingPopUp, MatchingPopUp);
        UIManager.Instance.RegisterPanel(PanelId.ReadyMenuPanel, readyMenuPanel);
        UIManager.Instance.RegisterPanel(PanelId.ShopPopUp, ShopPopUp);
        UIManager.Instance.RegisterPanel(PanelId.EnergyRechargePopUp, energyRechargePopUp);
        UIManager.Instance.RegisterPanel(PanelId.NotEnoughGemPopUp, notEnoughGemPopup);
        UIManager.Instance.RegisterPanel(PanelId.OrderRefreshPopUp, orderRefreshPopup);
        UIManager.Instance.RegisterPanel(PanelId.UseGiftBoxPopUp, useGiftBoxPopUp);
        UIManager.Instance.RegisterPanel(PanelId.DeleteAccountPopUp, deleteAccountPopUp);
        UIManager.Instance.RegisterPanel(PanelId.GameHelpPopup, gameHelpPopUp);
        UIManager.Instance.RegisterPanel(PanelId.LevelUpRewardPopUp, levelUpRewardPopUp);
        UIManager.Instance.RegisterPanel(PanelId.UseGiftBoxPopUp, useGiftBoxPopup);
        UIManager.Instance.RegisterPanel(PanelId.GuestPopup, guestPopup);

        UIManager.Instance.RegisterPanel(PanelId.TestLoginPanel, testLoginPanel);
        
        playerLevelInfoButton.onClick.AddListener(()=>UIManager.Instance.Open(PanelId.PlayerLevelInfoPopup));
        donutCodexButton.onClick.AddListener(() => UIManager.Instance.Open(PanelId.DonutCodexPopup));
        donutUpgradeButton.onClick.AddListener(()=>UIManager.Instance.Open(PanelId.DonutUpgradePopup));
        entryPopUpButton.onClick.AddListener(() => UIManager.Instance.Open(PanelId.EntryPopUp));
        detailedSettingsButton.onClick.AddListener(() => UIManager.Instance.Open(PanelId.DetailedSettingsPanel));
        //testBattle.onClick.AddListener(() => UIManager.Instance.Open(PanelId.MatchingPopUp));
        startBattle.onClick.AddListener(() => GameManager.Instance.StartGame());
        readyButton.onClick.AddListener(() => UIManager.Instance.Open(PanelId.ReadyMenuPanel));
        //goldShopButton.onClick.AddListener(() => UIManager.Instance.Open(PanelId.ShopPopUp)); //PM요청으로 주석처리
        gemShopButton.onClick.AddListener(() => UIManager.Instance.Open(PanelId.ShopPopUp));
        energyRechargeButton.onClick.AddListener(() => UIManager.Instance.Open(PanelId.EnergyRechargePopUp));
        orderRefreshButton.onClick.AddListener(() => UIManager.Instance.Open(PanelId.OrderRefreshPopUp));
        
        testLevelUpButton.onClick.AddListener(GameManager.Instance.LevelUp);
    }

    //private void OnEnable() =>  DataManager.Instance.OnUserDataChanged += SetPlayerText;
    private void OnEnable()
    {
        DataManager.Instance.OnUserDataChanged += SetPlayerText;
        GameManager.Instance.LevelUpdate += SetPlayerText;
    }
    
    public void SetPlayerText(PlayerData playerData)
    {
        levelText.text = $"{playerData.level}";
        energyText.text = $"{playerData.energy}";
        goldText.text = $"{playerData.gold}";
        gemText.text = $"{playerData.gem}";
    }

    private void Start()
    {
        SoundManager.Instance.PlayBGMGroup("OutGameBGM");
    }

}
