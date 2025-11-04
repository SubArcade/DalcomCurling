using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scr_PanelControl : MonoBehaviour
{
    [Header("화면전환 판넬")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject detailedSettigsPanel;
    [SerializeField] private GameObject giftboxPopup;
    [SerializeField] private GameObject infoPopup;
    [SerializeField] private GameObject donutInfoPopup;
    [SerializeField] private GameObject playerLevelInfoPopup;
    
    [SerializeField] private Button playerLevelInfoButton;
    
    void Awake()
    {
        UIManager.Instance.RegisterPanel(PanelId.Start,startPanel);    
        UIManager.Instance.RegisterPanel(PanelId.Login,loginPanel);    
        UIManager.Instance.RegisterPanel(PanelId.Main,mainPanel);    
        UIManager.Instance.RegisterPanel(PanelId.DetailedSettings,detailedSettigsPanel);    
        UIManager.Instance.RegisterPanel(PanelId.Giftbox,giftboxPopup);
        UIManager.Instance.RegisterPanel(PanelId.Info,infoPopup);
        UIManager.Instance.RegisterPanel(PanelId.DonutInfo,donutInfoPopup);
        UIManager.Instance.RegisterPanel(PanelId.PlayerLevelInfo,playerLevelInfoPopup);
        
        playerLevelInfoButton.onClick.AddListener(()=>UIManager.Instance.Open(PanelId.PlayerLevelInfo));
        
    }


}
