using UnityEngine;
using UnityEngine.UI;

public class Scr_SoundUI : MonoBehaviour
{
    // PlayerPrefs 키를 상수로 정의하여 관리합니다.
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const float DEFAULT_VOLUME = 1.0f; // 기본 볼륨 100%

    [Header("UI Controls")]
    public Button lobbyBGMButton;
    public Button gameBGMButton; // 이 버튼들은 필요에 따라 인스펙터에 연결해야 합니다.

    [Header("Volume Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;


    void Start()
    {
        // 1. 저장된 볼륨 값 로드 및 적용
        LoadAndApplyVolume();

        // 2. 슬라이더 이벤트 연결 (이전에 작성하신 로직이 맞습니다.)
        bgmSlider.onValueChanged.AddListener(SetBGMVolumeAndSave);
        sfxSlider.onValueChanged.AddListener(SetSFXVolumeAndSave);

        // 3. 버튼 이벤트 연결 (주석 해제 및 함수 이름 통일)
        if (lobbyBGMButton != null)
        {
            // SoundManager에 정의된 PlayBGMGroup("그룹 이름")을 호출합니다.
            lobbyBGMButton.onClick.AddListener(() => SoundManager.Instance.PlayBGMGroup("OutGameBGMs"));
        }
        if (gameBGMButton != null)
        {
            gameBGMButton.onClick.AddListener(() => SoundManager.Instance.PlayBGMGroup("InGameBGMs"));
        }

        Debug.Log("BGM 테스트 컨트롤러 준비 완료");
    }

    /// <summary>
    /// 저장된 볼륨 값을 불러와 슬라이더와 SoundManager에 적용합니다.
    /// </summary>
    private void LoadAndApplyVolume()
    {
        float savedBGM = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, DEFAULT_VOLUME);
        float savedSFX = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, DEFAULT_VOLUME);

        // 슬라이더 초기값 설정
        bgmSlider.value = savedBGM;
        sfxSlider.value = savedSFX;

        // SoundManager에 볼륨 적용 (슬라이더의 값이 변경될 때만 아니라, 시작 시에도 필요)
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetBGMVolume(savedBGM);
            SoundManager.Instance.SetSFXVolume(savedSFX);
        }
    }

    /// <summary>
    /// BGM 볼륨을 조절하고 저장합니다. (슬라이더 이벤트 리스너용)
    /// </summary>
    private void SetBGMVolumeAndSave(float volume)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetBGMVolume(volume);
            PlayerPrefs.SetFloat(BGM_VOLUME_KEY, volume); // 값 저장
            PlayerPrefs.Save();
        }
        // Debug.Log($"BGM 볼륨 값: {volume}"); // 최종 빌드 시 제거 권장
    }

    /// <summary>
    /// SFX 볼륨을 조절하고 저장합니다. (슬라이더 이벤트 리스너용)
    /// </summary>
    private void SetSFXVolumeAndSave(float volume)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(volume);
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume); // 값 저장
            PlayerPrefs.Save();
        }
        // Debug.Log($"SFX 볼륨 값: {volume}"); // 최종 빌드 시 제거 권장
    }
}