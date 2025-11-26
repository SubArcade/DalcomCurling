using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scr_GameHelpPopup : MonoBehaviour
{

    public Button closeButton;
    void Start()
    {
        closeButton.onClick.AddListener(() =>
        {
            UIManager.Instance.Close(PanelId.GameHelpPopup);
            UIManager.Instance.Open(PanelId.DetailedSettingsPanel);
        });
    }
}
