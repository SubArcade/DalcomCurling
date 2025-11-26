using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    public Button closebutton;
    public Button openbutton;
    private void Awake()
    {

    }
    void Start()
    {
        closebutton.onClick.AddListener(() => UIManager.Instance.Close(PanelId.DetailedSettingsPanel));
        openbutton.onClick.AddListener(() => UIManager.Instance.Open(PanelId.DetailedSettingsPanel));
    }
}
