using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Scr_DetailedSettingPopUp : MonoBehaviour
{
    //계정 옵션
    [Header("계정연동")]
    [SerializeField] private Button AccountLink;

    [Header("계정삭제")]
    [SerializeField] private Button AccountExit;

    [Header("유저 ID 텍스트")]
    [SerializeField] private TextMeshProUGUI accountIdText;
    
    //게임설정
    [Header("볼륨조절")]
    [SerializeField] private Scrollbar BGMsliderBar;
    [SerializeField] private Scrollbar SFXsliderBar;

    [Header("ON/OFF 기타설정")]
    [SerializeField] private Button OFFBGM;
    [SerializeField] private Button OFFSFX;
    [SerializeField] private Button OFFvibration;
    [SerializeField] private Button OFFquestion;
    [SerializeField] private Button ONBGM;
    [SerializeField] private Button ONSFX;
    [SerializeField] private Button ONvibration;
    [SerializeField] private Button ONquestion;

    [Header("언어설정")]
    [SerializeField] private TMP_Dropdown LanguageDropDown;

    [Header("알림설정")]
    [SerializeField] private Button energyONbutton;
    [SerializeField] private Button energyOFFbutton;
    [SerializeField] private Button eventOnbutton;
    [SerializeField] private Button eventOffbutton;
    [SerializeField] private Button nightOnbutton;
    [SerializeField] private Button nightOffbutton;

    [Header("닫기버튼")]
    [SerializeField] private Button CloseButton;

    void Awake()
    {
       // AccountLink = transform.Find("UIBackground/settingUI/account/AccountLink")?.GetComponent<Button>();
       // AccountExit = transform.Find("UIBackground/settingUI/account/AccountExit")?.GetComponent<Button>();
        accountIdText = transform.Find("UIBackground/settingUI/account/accountID_Text")?.GetComponent<TextMeshProUGUI>();

        BGMsliderBar = transform.Find("UIBackground/settingUI/setting/BGM/BGMsliderBar")?.GetComponent<Scrollbar>();
        SFXsliderBar = transform.Find("UIBackground/settingUI/setting/SFX/SFXsliderBar")?.GetComponent<Scrollbar>();

        OFFBGM = transform.Find("UIBackground/settingUI/setting/optionGroup/OFFBGM")?.GetComponent<Button>();
        OFFSFX = transform.Find("UIBackground/settingUI/setting/optionGroup/OFFSFX")?.GetComponent<Button>();
        OFFvibration = transform.Find("UIBackground/settingUI/setting/optionGroup/OFFvibration")?.GetComponent<Button>();
        OFFquestion = transform.Find("UIBackground/settingUI/setting/optionGroup/OFFquestion")?.GetComponent<Button>();
        ONBGM = transform.Find("UIBackground/settingUI/setting/optionGroup/ONBGM")?.GetComponent<Button>();
        ONSFX = transform.Find("UIBackground/settingUI/setting/optionGroup/ONSFX")?.GetComponent<Button>();
        ONvibration = transform.Find("UIBackground/settingUI/setting/optionGroup/ONvibration")?.GetComponent<Button>();
        ONquestion = transform.Find("UIBackground/settingUI/setting/optionGroup/ONquestion")?.GetComponent<Button>();

        LanguageDropDown = transform.Find("UIBackground/settingUI/setting/Language_Text/LanguageDropDown")?.GetComponent<TMP_Dropdown>();
        
        energyONbutton = transform.Find("UIBackground/settingUI/alarm/alarmGroup/energyAlarm_Text/EnergyOnButton")?.GetComponent<Button>();
        energyOFFbutton = transform.Find("UIBackground/settingUI/alarm/alarmGroup/energyAlarm_Text/EnergyOffButton")?.GetComponent<Button>();
        eventOnbutton = transform.Find("UIBackground/settingUI/alarm/alarmGroup/eventAlarm_Text/EventOnButton")?.GetComponent<Button>();
        eventOffbutton = transform.Find("UIBackground/settingUI/alarm/alarmGroup/eventAlarm_Text/EventOffButton")?.GetComponent<Button>();
        nightOnbutton = transform.Find("UIBackground/settingUI/alarm/alarmGroup/MidnightAlarm_Text/NightOnButton")?.GetComponent<Button>();
        nightOffbutton = transform.Find("UIBackground/settingUI/alarm/alarmGroup/MidnightAlarm_Text/NightOffButton")?.GetComponent<Button>();

        CloseButton = transform.Find("CloseButton")?.GetComponent<Button>();
    }
    void Start()
    {
        // BGM 설정
        ONBGM.onClick.AddListener(() =>
        {
            ONBGM.gameObject.SetActive(false);
            OFFBGM.gameObject.SetActive(true);
        });

        OFFBGM.onClick.AddListener(() =>
        {
            OFFBGM.gameObject.SetActive(false);
            ONBGM.gameObject.SetActive(true);
        });

        // SFX 설정
        ONSFX.onClick.AddListener(() =>
        {
            ONSFX.gameObject.SetActive(false);
            OFFSFX.gameObject.SetActive(true);
        });

        OFFSFX.onClick.AddListener(() =>
        {
            OFFSFX.gameObject.SetActive(false);
            ONSFX.gameObject.SetActive(true);
        });

        // 진동 설정
        ONvibration.onClick.AddListener(() =>
        {
            ONvibration.gameObject.SetActive(false);
            OFFvibration.gameObject.SetActive(true);
        });

        OFFvibration.onClick.AddListener(() =>
        {
            OFFvibration.gameObject.SetActive(false);
            ONvibration.gameObject.SetActive(true);
        });

        // 질문 도움말 설정
        ONquestion.onClick.AddListener(() =>
        {
            ONquestion.gameObject.SetActive(false);
            OFFquestion.gameObject.SetActive(true);
        });

        OFFquestion.onClick.AddListener(() =>
        {
            OFFquestion.gameObject.SetActive(false);
            ONquestion.gameObject.SetActive(true);
        });

        // 알림 설정 (에너지, 이벤트, 야간)
        energyONbutton.onClick.AddListener(() =>
        {
            energyONbutton.gameObject.SetActive(false);
            energyOFFbutton.gameObject.SetActive(true);
        });

        energyOFFbutton.onClick.AddListener(() =>
        {
            energyOFFbutton.gameObject.SetActive(false);
            energyONbutton.gameObject.SetActive(true);
        });

        eventOnbutton.onClick.AddListener(() =>
        {
            eventOnbutton.gameObject.SetActive(false);
            eventOffbutton.gameObject.SetActive(true);
        });

        eventOffbutton.onClick.AddListener(() =>
        {
            eventOffbutton.gameObject.SetActive(false);
            eventOnbutton.gameObject.SetActive(true);
        });

        nightOnbutton.onClick.AddListener(() =>
        {
            nightOnbutton.gameObject.SetActive(false);
            nightOffbutton.gameObject.SetActive(true);
        });

        nightOffbutton.onClick.AddListener(() =>
        {
            nightOffbutton.gameObject.SetActive(false);
            nightOnbutton.gameObject.SetActive(true);
        });

        CloseButton.onClick.AddListener(() =>
        {
            UIManager.Instance.Close(PanelId.DetailedSettingsPanel);
        });
    }
}
