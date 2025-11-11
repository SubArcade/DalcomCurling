using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class Scr_DonutCodex : MonoBehaviour
{
    [Header("도넛 토글 연결")]
    [SerializeField] private Toggle hardDonutToggle;
    [SerializeField] private Toggle softDonutToggle;
    [SerializeField] private Toggle MoistDonutToggle;

    [Header("도넛 도감 연결")]
    [SerializeField] private GameObject hardPanel;
    [SerializeField] private GameObject softPanel;
    [SerializeField] private GameObject moistPanel;
   
    [Header("창닫기 버튼")]
    [SerializeField] private Button closeButton;

    [Header("도감에 등록될 도넛 프리팹들")]
    [SerializeField] private List<GameObject> donutPreafab;

    void Awake()
    {
        Transform toggleGroup = transform.Find("rectangle626/DonutValueList");
        if (toggleGroup != null)
        {
            hardDonutToggle = toggleGroup.Find("HardTap_Text")?.GetComponent<Toggle>();
            softDonutToggle = toggleGroup.Find("SoftTap_Text")?.GetComponent<Toggle>();
            MoistDonutToggle = toggleGroup.Find("MoistTap_Text")?.GetComponent<Toggle>();
        }

        hardPanel = transform.Find("HardPanel")?.gameObject;
        softPanel = transform.Find("SoftPanel")?.gameObject;
        moistPanel = transform.Find("MoistPanel")?.gameObject;
        closeButton = transform.Find("rectangle625/CloseButton")?.GetComponent<Button>();
    }

    void Start()
    {
        hardDonutToggle.onValueChanged.AddListener((isOn) => TogglePanel(hardPanel, isOn));
        softDonutToggle.onValueChanged.AddListener((isOn) => TogglePanel(softPanel, isOn));
        MoistDonutToggle.onValueChanged.AddListener((isOn) => TogglePanel(moistPanel, isOn));

        TogglePanel(hardPanel, hardDonutToggle.isOn);
        TogglePanel(softPanel, softDonutToggle.isOn);
        TogglePanel(moistPanel, MoistDonutToggle.isOn);

        closeButton.onClick.AddListener(CloseCollectionPopUp);
    }

    void TogglePanel(GameObject panel, bool isOn)
    {
      panel.SetActive(isOn);
    }
    void CloseCollectionPopUp() 
    {
        UIManager.Instance.Close(PanelId.DonutCodexPopup);
    }
}
