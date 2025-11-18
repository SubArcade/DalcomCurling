using FMODUnity;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("BGM Events")]
    [SerializeField] private EventReference[] lobbyBGMs;
    [SerializeField] private EventReference[] gameBGMs;
    [SerializeField] private EventReference intenseMusic;

    [Header("SFX Events")]
    [SerializeField] private EventReference stoneImpact;
    [SerializeField] private EventReference buttonSound;
    [SerializeField] private EventReference spectatorSound;

    private FMOD.Studio.EventInstance currentBGMInstance;
    private FMOD.Studio.EventInstance spectatorInstance;
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
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
                Debug.Log($"BGM 재생: {bgmName} (인덱스: {currentBGMIndex + 1}/{currentPlaylist.Count})");

                currentBGMInstance = RuntimeManager.CreateInstance(currentBGM);
                currentBGMInstance.start();

                FMOD.Studio.PLAYBACK_STATE playbackState;
                do
                {
                    yield return null;
                    currentBGMInstance.getPlaybackState(out playbackState);
                }
                while (playbackState != FMOD.Studio.PLAYBACK_STATE.STOPPED);

                currentBGMInstance.release();

                // 다음 트랙으로 진행하기
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

    //-----------씬 이름 넣는 구간-----------
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LobbyScene")
        {
            PlayLobbyBGM();
        }
        else if (scene.name == "GameScene")
        {
            PlayGameBGM();
        }
    }

    public void PlaySFX(EventReference sfx, Vector3 position)
    {
        if (!sfx.IsNull)
        {
            RuntimeManager.PlayOneShot(sfx, position);
            Debug.Log($"SFX 재생: {GetBGMName(sfx.Path)}");
        }
        else
        {
            Debug.LogWarning("SFX 이벤트가 비어 있습니다.");
        }
    }
    public void PlayButtonClickSound(Vector3 position)
    {
        PlaySFX(buttonSound, position);
    }


    public void HandleDonutCollision(GameObject source)
    {
        PlaySFX(stoneImpact, source.transform.position);
    }

    public void PlaySpectatorCheer(Vector3 position)
    {
        if (!spectatorSound.IsNull)
        {
            spectatorInstance = RuntimeManager.CreateInstance(spectatorSound);
            spectatorInstance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
            spectatorInstance.start();
            Debug.Log("관중 응원 사운드 시작");
        }
        else
        {
            Debug.LogWarning("관중 사운드 이벤트가 비어 있습니다.");
        }
    }

    public void StopSpectatorCheer()
    {
        if (spectatorInstance.isValid())
        {
            spectatorInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            spectatorInstance.release();
            Debug.Log("관중 응원 사운드 정지");
        }
    }

    public void SetBGMVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (currentBGMInstance.isValid())
            currentBGMInstance.setVolume(volume);

        FMOD.Studio.Bus bgmBus = RuntimeManager.GetBus("bus:/BGM");
        bgmBus.setVolume(volume);
        Debug.Log($"[SoundManager] BGM 볼륨 설정됨: {volume}");
    }

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        FMOD.Studio.Bus sfxBus = RuntimeManager.GetBus("bus:/SFX");
        sfxBus.setVolume(volume);
        Debug.Log($"[SoundManager] SFX 볼륨 설정됨: {volume}");
    }

    #endregion
}
