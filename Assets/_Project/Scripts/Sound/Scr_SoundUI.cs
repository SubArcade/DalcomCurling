//using Google.Impl;
using UnityEngine;
using UnityEngine.UI;

public class Scr_SoundUI : MonoBehaviour
{
    [Header("UI Controls")]
    public Button lobbyBGMButton;
    public Button gameBGMButton;

    [Header("Volume Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;

 
    void Start()
    {
        // 버튼 이벤트 연결
        //lobbyBGMButton.onClick.AddListener(() => SoundManager.Instance.PlayLobbyBGM());
        //gameBGMButton.onClick.AddListener(() => SoundManager.Instance.PlayGameBGM());

        // 슬라이더 초기값 설정 (필요 시 저장된 값 불러오기)
        bgmSlider.value = 1f;
        sfxSlider.value = 1f;

        // 슬라이더 이벤트 연결
        bgmSlider.onValueChanged.AddListener((value) =>
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SetBGMVolume(value);
                Debug.Log($"BGM 볼륨 값: {value}");
            }
        });

        sfxSlider.onValueChanged.AddListener((value) =>
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SetSFXVolume(value);
                Debug.Log($"SFX 볼륨 값: {value}");
            }
        });


        Debug.Log("BGM 테스트 컨트롤러 준비 완료");
    }
}

