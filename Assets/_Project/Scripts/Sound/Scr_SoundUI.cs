using UnityEngine;
using UnityEngine.UI;

public class SoundUI : MonoBehaviour
{
    [Header("UI Controls")]
    public Button lobbyBGMButton;
    public Button gameBGMButton;
    public Button stopBGMButton;
    public Button pauseBGMButton;
    public Slider volumeSlider;

    void Start()
    {
        // 버튼 이벤트 연결
        lobbyBGMButton.onClick.AddListener(() => SoundManager.Instance.PlayLobbyBGM());
        gameBGMButton.onClick.AddListener(() => SoundManager.Instance.PlayGameBGM());
        stopBGMButton.onClick.AddListener(() => SoundManager.Instance.StopBGM());
        pauseBGMButton.onClick.AddListener(() => SoundManager.Instance.PauseBGM(true));

        volumeSlider.onValueChanged.AddListener((value) => SoundManager.Instance.SetBGMVolume(value));

        Debug.Log("BGM 테스트 컨트롤러 준비 완료");
    }
}