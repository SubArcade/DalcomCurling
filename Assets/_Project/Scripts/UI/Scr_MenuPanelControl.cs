using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scr_PanelControl : MonoBehaviour
{
    [Header("화면전환 판넬")]
    [SerializeField] private GameObject StartPanel;
    [SerializeField] private GameObject LoginPanel;
    [SerializeField] private GameObject MainPanel;
    [SerializeField] private GameObject DetailedSettigsPanel;
    [SerializeField] private GameObject GiftboxPopup;
    [SerializeField] private GameObject InfoPopup;
    [SerializeField] private GameObject DonutInfoPopup;
    
    
    void Awake()
    {
        UIManager.Instance.RegisterPanel(PanelId.Start,StartPanel);    
        UIManager.Instance.RegisterPanel(PanelId.Login,LoginPanel);    
        UIManager.Instance.RegisterPanel(PanelId.Main,MainPanel);    
        UIManager.Instance.RegisterPanel(PanelId.DetailedSettings,DetailedSettigsPanel);    
        UIManager.Instance.RegisterPanel(PanelId.Giftbox,GiftboxPopup);
        UIManager.Instance.RegisterPanel(PanelId.Info,InfoPopup);
        UIManager.Instance.RegisterPanel(PanelId.DonutInfo,DonutInfoPopup);
    }


}
