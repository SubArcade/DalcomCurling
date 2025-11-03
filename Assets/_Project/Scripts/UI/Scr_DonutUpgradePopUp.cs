using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Scr_DonutUpgradePopUp : MonoBehaviour
{
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

    [Header("업그레이드 질문 팝업")]
    [SerializeField] private GameObject AskUpgradePopUp;
    [SerializeField] private GameObject FailUpgradePopUp;

    void Awake()
    {
        AwakeInspector();
    }

    void Start()
    {
        
    }

    void AwakeInspector() 
    {
        CloseButton = transform.Find("CloseButton")?.GetComponent<Button>();
        HardUpgradeButton = transform.Find("HardUpgradePanel/HardUpgradeButton")?.GetComponent<Button>();
        MoistUpgradeButton = transform.Find("MoistUpgradePanel/MoistUpgradeButton")?.GetComponent<Button>();
        SoftUpgradeButton = transform.Find("SoftUpgradePanel/SoftUpgradeButton")?.GetComponent<Button>();

        HardUpgradeMax = transform.Find("HardUpgradePanel/HardUpgradeMax")?.gameObject;
        MoistUpgradeMax = transform.Find("MoistUpgradePanel/MoistUpgradeMax")?.gameObject;
        SoftUpgradeMax = transform.Find("SoftUpgradePanel/SoftUpgradeMax")?.gameObject;

        HardDonutCreatText = transform.Find("HardUpgradePanel/HardDonutCreateText")?.GetComponent<TextMeshProUGUI>();
        MoistDonutCreatText = transform.Find("MoistUpgradePanel/MoistDonutCreateText")?.GetComponent<TextMeshProUGUI>();
        SoftDonutCreatText = transform.Find("SoftUpgradePanel/SoftDonutCreateText")?.GetComponent<TextMeshProUGUI>();

        HardDonutOptionText = transform.Find("HardUpgradePanel/UpgradeOptionLabel/HardDonutOptionText")?.GetComponent<TextMeshProUGUI>();
        MoistDonutOptionText = transform.Find("MoistUpgradePanel/UpgradeOptionLabel/MoistDonutOptionText")?.GetComponent<TextMeshProUGUI>();
        SoftDonutOptionText = transform.Find("SoftUpgradePanel/UpgradeOptionLabel/SoftDonutOptionText")?.GetComponent<TextMeshProUGUI>();
    }
    void Startzip() 
    {
        CloseButton.onClick.AddListener(OnClickClosePopUp);
    }

    void updateText() //업그레이드 누를때마다 레벨과 단계 텍스트 갱신
    {
    
    }
    void OnClickClosePopUp() 
    {
        this.gameObject.SetActive(false);
    }
}
