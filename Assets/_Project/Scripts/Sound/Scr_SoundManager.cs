using FMODUnity;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("BGM Events")]
    [SerializeField] private EventReference[] lobbyBGMs;
    [SerializeField] private EventReference[] gameBGMs;
    [SerializeField] private EventReference intenseMusic;

    [Header("SFX Events")]
    [SerializeField] private EventReference stoneImpact;
    [SerializeField] private EventReference particleSound;
    [SerializeField] private EventReference cheerSound;
    [SerializeField] private EventReference mergeSound;
    [SerializeField] private EventReference buttonSound;
    [SerializeField] private EventReference menuSound;
    [SerializeField] private EventReference crowdAmbience;

    private FMOD.Studio.EventInstance currentBGMInstance;
    private List<EventReference> currentPlaylist;
    private int currentBGMIndex = 0;
    private bool isInGame = false;
    private Coroutine bgmCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("SoundManager 초기화 완료 - BGM 시스템");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 시작 시 로비 BGM 재생
        PlayLobbyBGM();
    }

    #region BGM Management
    public void PlayLobbyBGM()
    {
        if (isInGame == false && currentPlaylist != null && currentPlaylist.Count == lobbyBGMs.Length)
        {
            Debug.Log("이미 로비 BGM 재생중");
            return;
        }

        Debug.Log("로비 BGM 시작");
        isInGame = false;
        SwitchBGMPlaylist(new List<EventReference>(lobbyBGMs), "로비");
    }

    public void PlayGameBGM()
    {
        if (isInGame == true && currentPlaylist != null && currentPlaylist.Count == gameBGMs.Length)
        {
            Debug.Log("이미 게임 BGM 재생중");
            return;
        }

        Debug.Log("게임 BGM 시작");
        isInGame = true;
        SwitchBGMPlaylist(new List<EventReference>(gameBGMs), "게임");
    }

    private void SwitchBGMPlaylist(List<EventReference> newPlaylist, string playlistName)
    {
        // 기존 BGM 정지
        if (bgmCoroutine != null)
            StopCoroutine(bgmCoroutine);

        if (currentBGMInstance.isValid())
        {
            currentBGMInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            currentBGMInstance.release();
            Debug.Log($"기존 BGM 정지 - {playlistName} 플레이리스트로 전환");
        }

        // 새로운 플레이리스트 설정
        currentPlaylist = newPlaylist;
        currentBGMIndex = 0;

        if (currentPlaylist != null && currentPlaylist.Count > 0)
        {
            bgmCoroutine = StartCoroutine(PlayBGMPlaylist(playlistName));
        }
        else
        {
            Debug.LogWarning($"{playlistName} BGM 플레이리스트가 비어있습니다!");
        }
    }

    private IEnumerator PlayBGMPlaylist(string playlistName)
    {
        Debug.Log($"{playlistName} BGM 플레이리스트 시작 (총 {currentPlaylist.Count}곡)");

        while (true)
        {
            if (currentPlaylist.Count == 0) yield break;

            // 현재 BGM 재생
            EventReference currentBGM = currentPlaylist[currentBGMIndex];

            if (!currentBGM.IsNull)
            {
                string bgmName = GetBGMName(currentBGM.Path);
                Debug.Log($"🎵 BGM 재생: {bgmName} (인덱스: {currentBGMIndex + 1}/{currentPlaylist.Count})");

                currentBGMInstance = RuntimeManager.CreateInstance(currentBGM);
                currentBGMInstance.start();

                // BGM 길이 대기 (테스트용 30초, 실제로는 음악 길이에 맞게 조정)
                yield return new WaitForSeconds(30f);

                // 현재 인스턴스 정리
                currentBGMInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                currentBGMInstance.release();

                // 다음 트랙으로
                currentBGMIndex = (currentBGMIndex + 1) % currentPlaylist.Count;
            }
            else
            {
                Debug.LogWarning($"빈 BGM 이벤트 참조 (인덱스: {currentBGMIndex})");
                yield break;
            }
        }
    }

    private string GetBGMName(string path)
    {
        if (string.IsNullOrEmpty(path)) return "Unknown";
        string[] parts = path.Split('/');
        return parts.Length > 0 ? parts[parts.Length - 1] : path;
    }
    #endregion

    #region Public Controls
    public void StopBGM()
    {
        if (bgmCoroutine != null)
        {
            StopCoroutine(bgmCoroutine);
            bgmCoroutine = null;
        }

        if (currentBGMInstance.isValid())
        {
            currentBGMInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            currentBGMInstance.release();
        }

        Debug.Log("BGM 정지");
    }

    public void SetBGMVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        // 현재 인스턴스 볼륨 설정
        if (currentBGMInstance.isValid())
        {
            currentBGMInstance.setVolume(volume);
        }

        // BGM 버스 볼륨 설정
        FMOD.Studio.Bus bgmBus = RuntimeManager.GetBus("bus:/BGM");
        bgmBus.setVolume(volume);

        Debug.Log($"BGM 볼륨 설정: {volume}");
    }

    public void PauseBGM(bool pause)
    {
        if (currentBGMInstance.isValid())
        {
            currentBGMInstance.setPaused(pause);
            Debug.Log($"BGM {(pause ? "일시정지" : "재생")}");
        }
    }
    #endregion

    void OnDestroy()
    {
        StopBGM();
    }
}
