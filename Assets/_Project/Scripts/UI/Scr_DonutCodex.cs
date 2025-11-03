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
        AwakeGetInspector();//awake에서 추가로 넣을 코드 있으면 메서드 안에 작성
    }

    void Start()
    {
        startZip(); //start에서 추가로 넣을 코드 있으면 해당 메서드 안에 작성
    }

    void Update()
    {
        
    }
    void startZip() 
    {
        hardDonutToggle.onValueChanged.AddListener((isOn) => TogglePanel(hardPanel, isOn));
        softDonutToggle.onValueChanged.AddListener((isOn) => TogglePanel(softPanel, isOn));
        MoistDonutToggle.onValueChanged.AddListener((isOn) => TogglePanel(moistPanel, isOn));

        TogglePanel(hardPanel, hardDonutToggle.isOn);
        TogglePanel(softPanel, softDonutToggle.isOn);
        TogglePanel(moistPanel, MoistDonutToggle.isOn);

        closeButton.onClick.AddListener(CloseCollectionPopUp);
    }
    void AwakeGetInspector() 
    {
        Transform toggleGroup = transform.Find("DonutValueList");
        if (toggleGroup != null)
        {
            hardDonutToggle = toggleGroup.Find("HardTap")?.GetComponent<Toggle>();
            softDonutToggle = toggleGroup.Find("SoftTap")?.GetComponent<Toggle>();
            MoistDonutToggle = toggleGroup.Find("MoistTap")?.GetComponent<Toggle>();
        }

        hardPanel = transform.Find("HardPanel")?.gameObject;
        softPanel = transform.Find("SoftPanel")?.gameObject;
        moistPanel = transform.Find("MoistPanel")?.gameObject;
        closeButton = transform.Find("CloseButton")?.GetComponent<Button>();
    }
    void TogglePanel(GameObject panel, bool isOn)
    {
      panel.SetActive(isOn);
    }
    void CloseCollectionPopUp() 
    {
        this.gameObject.SetActive(false);
    }
}
