using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Scr_DetailedSettingPopUp : MonoBehaviour
{
    //계정 옵션
    [Header("계정연동")]
    [SerializeField] private Button accountLink;

    [Header("계정삭제")]
    [SerializeField] private Button accountExit;

    [Header("유저 ID 텍스트")]
    [SerializeField] private TextMeshProUGUI accountIdText;
    
    //게임설정
    [Header("볼륨조절")]
    [SerializeField] private Scrollbar BGMsliderBar;
    [SerializeField] private Scrollbar SFXsliderBar;
    [SerializeField] private TextMeshProUGUI BGMvolumeText;
    [SerializeField] private TextMeshProUGUI SFXvolumeText;
    [SerializeField] private Image BGMGage;
    [SerializeField] private Image SFXGage;


    [Header("ON/OFF 기타설정")]
    [SerializeField] private Button OFFBGM;
    [SerializeField] private Button OFFSFX;
    [SerializeField] private Button OFFvibration;
    [SerializeField] private Button ONBGM;
    [SerializeField] private Button ONSFX;
    [SerializeField] private Button ONvibration;
    [SerializeField] private Button gameHelpBtn;

    [Header("언어설정")]
    [SerializeField] private TMP_Dropdown languageDropDown;

    [Header("알림설정")]
    [SerializeField] private Button energyONbutton;
    [SerializeField] private Button energyOFFbutton;
    [SerializeField] private Button eventOnbutton;
    [SerializeField] private Button eventOffbutton;
    [SerializeField] private Button nightOnbutton;
    [SerializeField] private Button nightOffbutton;

    [Header("닫기버튼")]
    [SerializeField] private Button closeButton;

    void Awake()
    {
        accountLink.onClick.AddListener(FirebaseAuthManager.Instance.ConnectGpgsAccount);
        accountExit.onClick.AddListener(() => UIManager.Instance.Open(PanelId.DeleteAccountPopUp));
        gameHelpBtn.onClick.AddListener(() =>UIManager.Instance.Open(PanelId.GameHelpPopup));          
    }
    void Start()
    {
        // BGM 설정
        ONBGM.onClick.AddListener(() =>
        {
            ONBGM.gameObject.SetActive(false);
            OFFBGM.gameObject.SetActive(true);

            // BGM 끄기  
            RestoreVolume("BGMVolume", BGMsliderBar, BGMvolumeText, BGMGage, SoundManager.Instance.SetBGMVolume);
        });

        OFFBGM.onClick.AddListener(() =>
        {
            OFFBGM.gameObject.SetActive(false);
            ONBGM.gameObject.SetActive(true);

            // BGM 켜기 
            SetVolumeSilent("BGMVolume", BGMsliderBar, BGMvolumeText, BGMGage);
        });

        // SFX 설정
        ONSFX.onClick.AddListener(() =>
        {
            ONSFX.gameObject.SetActive(false);
            OFFSFX.gameObject.SetActive(true);

            // SFX 끄기
            RestoreVolume("SFXVolume", SFXsliderBar, SFXvolumeText, SFXGage, SoundManager.Instance.SetSFXVolume);
        });

        OFFSFX.onClick.AddListener(() =>
        {
            OFFSFX.gameObject.SetActive(false);
            ONSFX.gameObject.SetActive(true);

            // SFX 켜기
            SetVolumeSilent("SFXVolume", SFXsliderBar, SFXvolumeText, SFXGage);
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

        closeButton.onClick.AddListener(() =>
        {
            UIManager.Instance.Close(PanelId.DetailedSettingsPanel);
        });

        InitVolumeSettings();

        languageDropDown.onValueChanged.AddListener(OnLanguageChanged);

        // 현재 언어에 따라 드롭다운 초기화
        if (LocalizationManager.Instance != null)
        {
            string lang = LocalizationManager.Instance.CurrentLanguage;
            int index = lang == "ko" ? 0 : 1;
            languageDropDown.value = index;
        }

    }

    //사운드 조절 슬라이더와 텍스트 연결 및 저장 불러오기
    private void InitVolumeSettings()
    {
        //저장된 값 불러오기
        float savedBGM = PlayerPrefs.GetFloat("BGMVolume", 1f); // 기본값 100%
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // FMOD 연결 추가 +
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetBGMVolume(savedBGM);
            SoundManager.Instance.SetSFXVolume(savedSFX);
        }

        //슬라이더에 적용
        BGMsliderBar.value = savedBGM;
        SFXsliderBar.value = savedSFX;

        BGMGage.fillAmount = savedBGM;
        SFXGage.fillAmount = savedSFX;

        //텍스트 초기화
        BGMvolumeText.text = Mathf.RoundToInt(savedBGM * 100f).ToString();
        SFXvolumeText.text = Mathf.RoundToInt(savedSFX * 100f).ToString();

        //슬라이더 값 변경 시 저장 및 텍스트 갱신
        BGMsliderBar.onValueChanged.AddListener((value) =>
        {
            int volume = Mathf.RoundToInt(value * 100f);
            BGMvolumeText.text = volume.ToString();
            BGMGage.fillAmount = value; // 게이지 채우기

            // FMOD 연결 추가 +
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SetBGMVolume(value);
            }

            PlayerPrefs.SetFloat("BGMVolume", value);
            PlayerPrefs.Save();
        });

        SFXsliderBar.onValueChanged.AddListener((value) =>
        {
            int volume = Mathf.RoundToInt(value * 100f);
            SFXvolumeText.text = volume.ToString();
            SFXGage.fillAmount = value; // 게이지 채우기

            // FMOD 연결 추가 +
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SetSFXVolume(value);
            }

            PlayerPrefs.SetFloat("SFXVolume", value);
            PlayerPrefs.Save();
        });
    }

    private void OnLanguageChanged(int index)
    {
        string selectedLang = "ko"; // 기본값

        switch (index)
        {
            case 0: selectedLang = "ko"; break;
            case 1: selectedLang = "en"; break;
                // 필요 시 다른 언어 추가
        }

        LocalizationManager.Instance.SetLanguage(selectedLang);
    }

    // + 볼륨 끄기 버튼 스크립트
    private void SetVolumeSilent(string key, Scrollbar slider, TextMeshProUGUI text, Image gage)
    {
        if (SoundManager.Instance == null) return;

        // 사운드 끄기 명령 및 UI 업데이트
        float value = 0f;
        if (key == "BGMVolume")
        {
            SoundManager.Instance.SetBGMVolume(value);
        }
        else
        {
            SoundManager.Instance.SetSFXVolume(value);
        }

        slider.value = value;
        text.text = "0";
        gage.fillAmount = value;

        // 0 볼륨 값을 저장합니다. (켜기 버튼이 다시 눌리기 전까지 0 유지)
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    // + 다시 100의 값으로 볼륨으로 키는 버튼
    private void RestoreVolume(string key, Scrollbar slider, TextMeshProUGUI text, Image gage, System.Action<float> setVolumeAction)
    {
        if (SoundManager.Instance == null || setVolumeAction == null) return;

        float restoredValue = PlayerPrefs.GetFloat(key, 1f);

        if (restoredValue == 0f) restoredValue = 1f;

        setVolumeAction(restoredValue);

        slider.value = restoredValue;
        text.text = Mathf.RoundToInt(restoredValue * 100f).ToString();
        gage.fillAmount = restoredValue;

        // 복구된 값으로 저장합니다.
        PlayerPrefs.SetFloat(key, restoredValue);
        PlayerPrefs.Save();
    }
}
