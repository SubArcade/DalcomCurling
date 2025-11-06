using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scr_PanelControl : MonoBehaviour
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
    [SerializeField] private GameObject nickNameChangePopup;
    
    [Header("화면 전환 버튼")]
    [SerializeField] private Button playerLevelInfoButton;
    [SerializeField] private Button donutCodexButton;
    [SerializeField] private Button donutUpgradeButton;
    
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
        
        
        playerLevelInfoButton.onClick.AddListener(()=>UIManager.Instance.Open(PanelId.PlayerLevelInfoPopup));
        donutCodexButton.onClick.AddListener(()=>UIManager.Instance.Open(PanelId.DonutCodexPopup));
        donutUpgradeButton.onClick.AddListener(()=>UIManager.Instance.Open(PanelId.DonutUpgradePopup));
    }


}
