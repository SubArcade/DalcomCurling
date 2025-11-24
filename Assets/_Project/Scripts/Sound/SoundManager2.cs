using FMODUnity;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// FMOD 이벤트를 이름으로 관리하고, BGM/SFX 및 인게임/아웃게임 사운드를 구분하여 재생하는 싱글톤 SoundManager.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [System.Serializable]
    public class SoundGroup
    {
        [Tooltip("그룹의 고유 이름입니다 (예: LobbyBGMs, InGameSFXs).")]
        public string Name;
        [Tooltip("이 그룹에 속하는 FMOD 이벤트들입니다.")]
        public EventReference[] Events;
    }

    // 이 배열에 OutGameBGM, InGameBGM, OutGameSFX, InGameSFX 그룹을 모두 설정합니다.
    [Header("모든 사운드 그룹 설정")]
    [Tooltip("모든 FMOD EventReference를 그룹별로 분류하여 설정합니다.")]
    [SerializeField]
    private SoundGroup[] allSoundGroups;

    // 모든 SFX 및 개별 BGM 이벤트를 이름(키)으로 저장하는 딕셔너리
    private Dictionary<string, EventReference> soundEventMap = new Dictionary<string, EventReference>();

    // BGM 재생 관리 필드
    private FMOD.Studio.EventInstance currentBGMInstance;
    private List<EventReference> currentBGMPlaylist;
    private int currentBGMIndex = 0;
    private Coroutine bgmCoroutine;

    #region 초기화 및 딕셔너리 구성

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSoundMap();
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
        StopAllCoroutines();
        StopBGMInternal();
    }

    void Start()
    {
        // 초기 시작 BGM 설정 (Inspector에서 정의한 'OutGameBGMs' 그룹이 있다고 가정)
        PlayBGMGroup("OutGameBGMs");
    }

    /// <summary>
    /// Inspector에 설정된 모든 EventReference를 이름 기반 딕셔너리에 로드합니다.
    /// </summary>
    private void InitializeSoundMap()
    {
        soundEventMap.Clear();
        foreach (var group in allSoundGroups)
        {
            foreach (var eventRef in group.Events)
            {
                if (!eventRef.IsNull)
                {
                    // FMOD 경로에서 이벤트 이름 추출 (예: "event:/SFX/Merge_Donut" -> "Merge_Donut")
                    string eventName = GetEventName(eventRef.Path);

                    if (soundEventMap.ContainsKey(eventName))
                    {
                        Debug.LogWarning($"[SoundManager] 딕셔너리에 이미 '{eventName}' 키가 존재합니다. 이벤트: {eventRef.Path}");
                        continue;
                    }

                    // 모든 개별 사운드(SFX, 단일 BGM, 인텐스 뮤직 등)는 딕셔너리로 관리
                    soundEventMap.Add(eventName, eventRef);
                }
            }
        }
        Debug.Log($"[SoundManager] 사운드 맵 로드 완료. 총 {soundEventMap.Count}개 이벤트.");
    }

    // FMOD 경로에서 이벤트 이름 추출 헬퍼
    private string GetEventName(string path)
    {
        if (string.IsNullOrEmpty(path)) return "Unknown";
        string[] parts = path.Split('/');
        return parts.Length > 0 ? parts[parts.Length - 1] : path;
    }

    // 사운드 맵에서 이벤트 참조를 가져오는 헬퍼
    private EventReference GetEventReference(string eventName)
    {
        if (soundEventMap.TryGetValue(eventName, out EventReference eventRef))
        {
            return eventRef;
        }
        Debug.LogWarning($"[SoundManager] 딕셔너리에서 '{eventName}' 이벤트 참조를 찾을 수 없습니다. 호출에 실패했습니다.");
        return new EventReference(); // Null EventReference 반환
    }

    #endregion



    // 🎶 BGM 재생 (그룹/플레이리스트 관리)

    /// <summary>
    /// SoundGroup 이름으로 BGM 플레이리스트를 시작합니다.
    /// </summary>
    /// <param name="groupName">설정에서 정의된 BGM SoundGroup의 이름입니다 (예: OutGameBGMs, InGameBGMs).</param>
    public void PlayBGMGroup(string groupName)
    {
        SoundGroup group = System.Array.Find(allSoundGroups, g => g.Name == groupName);

        if (group == null || group.Events == null || group.Events.Length == 0)
        {
            Debug.LogWarning($"[SoundManager] '{groupName}' BGM 그룹을 찾을 수 없거나 이벤트가 비어 있습니다. BGM을 정지합니다.");
            StopBGM();
            return;
        }

        List<EventReference> newPlaylist = new List<EventReference>(group.Events);

        // 현재 재생 목록과 새 목록이 동일하면 중복 재생 방지
        if (IsSamePlaylist(newPlaylist, currentBGMPlaylist))
        {
            Debug.Log($"[SoundManager] 이미 '{groupName}' BGM 재생 목록이 재생 중입니다.");
            return;
        }

        Debug.Log($"[SoundManager] BGM 플레이리스트 전환 시작: '{groupName}'");
        SwitchBGMPlaylist(newPlaylist, groupName);
    }

    // 플레이리스트가 동일한지 확인
    private bool IsSamePlaylist(List<EventReference> a, List<EventReference> b)
    {
        if (a == null || b == null) return false;
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
        {
            if (!a[i].Equals(b[i])) return false;
        }
        return true;
    }

    private void SwitchBGMPlaylist(List<EventReference> newPlaylist, string playlistName)
    {
        if (bgmCoroutine != null)
            StopCoroutine(bgmCoroutine);

        StopBGMInternal();

        currentBGMPlaylist = newPlaylist;
        currentBGMIndex = 0;

        if (currentBGMPlaylist.Count > 0)
        {
            bgmCoroutine = StartCoroutine(PlayBGMPlaylist(playlistName));
        }
        else
        {
            Debug.LogWarning($"[SoundManager] {playlistName} BGM 플레이리스트가 비어있습니다!");
        }
    }

    private void StopBGMInternal()
    {
        if (currentBGMInstance.isValid())
        {
            currentBGMInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            currentBGMInstance.release();
        }
    }

    private IEnumerator PlayBGMPlaylist(string playlistName)
    {
        Debug.Log($"[SoundManager] {playlistName} BGM 플레이리스트 시작 (총 {currentBGMPlaylist.Count}곡)");

        while (true)
        {
            if (currentBGMPlaylist.Count == 0) yield break;

            EventReference currentBGM = currentBGMPlaylist[currentBGMIndex];

            if (!currentBGM.IsNull)
            {
                string bgmName = GetEventName(currentBGM.Path);
                Debug.Log($"[SoundManager] BGM 재생: {bgmName} (인덱스: {currentBGMIndex + 1}/{currentBGMPlaylist.Count})");

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

                // 다음 트랙으로 진행하기 (순환 재생)
                currentBGMIndex = (currentBGMIndex + 1) % currentBGMPlaylist.Count;
            }
            else
            {
                Debug.LogWarning($"[SoundManager] 빈 BGM 이벤트 참조 (인덱스: {currentBGMIndex})");
                yield break;
            }
        }
    }

    /// <summary>
    /// 현재 재생 중인 BGM을 정지합니다.
    /// </summary>
    public void StopBGM()
    {
        if (bgmCoroutine != null)
        {
            StopCoroutine(bgmCoroutine);
            bgmCoroutine = null;
        }
        StopBGMInternal();
        currentBGMPlaylist = null;
        currentBGMIndex = 0;
        Debug.Log("[SoundManager] BGM 정지됨.");
    }

    

    // 🔊 SFX 재생 (세분화된 호출)

    /// <summary>
    /// 딕셔너리에 등록된 이벤트 이름으로 SFX를 재생합니다.
    /// 모든 인게임/아웃게임 SFX는 이 메서드를 통해 이름으로 호출됩니다.
    /// </summary>
    /// <param name="eventName">InitializeSoundMap에서 등록된 이벤트의 이름입니다.</param>
    /// <param name="position">SFX 재생 위치 (3D 사운드용).</param>
    public void PlaySFX(string eventName, Vector3 position = default(Vector3))
    {
        EventReference sfxRef = GetEventReference(eventName);

        if (!sfxRef.IsNull)
        {
            RuntimeManager.PlayOneShot(sfxRef, position);
        }
    }

    // --- 외부 호출 API (예시: 세분화된 SFX 호출) ---

    // 아웃게임 SFX 예시 1: 버튼 클릭
    public void PlayButtonClickSound(Vector3 position = default(Vector3))
    {
        // "ButtonClick"은 Inspector의 FMOD 이벤트 이름과 일치해야 합니다.
        PlaySFX("ButtonClick", position);
    }

    // 아웃게임 SFX 예시 2: 물체 생성 (도넛 생산)
    public void PlayProductionSound(Vector3 position = default(Vector3))
    {
        PlaySFX("ProductionDonutSound", position);
    }

    // 아웃게임 SFX 예시 3: 물체 합쳐짐 (도넛 병합)
    public void PlayMergeSound(Vector3 position = default(Vector3))
    {
        PlaySFX("MergeDonutSound", position);
    }

    // 인게임 SFX 예시: 물체 충돌
    public void PlayStoneImpact(Vector3 position)
    {
        PlaySFX("StoneImpact", position);
    }

    // 인게임 BGM 예시: 인텐스 음악 재생 (단일 이벤트)
    public FMOD.Studio.EventInstance PlayIntenseMusic()
    {
        EventReference intenseRef = GetEventReference("IntenseMusic");
        if (!intenseRef.IsNull)
        {
            FMOD.Studio.EventInstance instance = RuntimeManager.CreateInstance(intenseRef);
            instance.start();
            return instance;
        }
        return new FMOD.Studio.EventInstance();
    }


    #region Volume and Scene Controls

    public void SetBGMVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        FMOD.Studio.Bus bgmBus = RuntimeManager.GetBus("bus:/BGM");
        bgmBus.setVolume(volume);
        Debug.Log($"[SoundManager] BGM Bus 볼륨 설정됨: {volume}");
    }

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        FMOD.Studio.Bus sfxBus = RuntimeManager.GetBus("bus:/SFX");
        sfxBus.setVolume(volume);
        Debug.Log($"[SoundManager] SFX Bus 볼륨 설정됨: {volume}");
    }

    // --- 씬 로드 처리 ---
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 이름에 따라 BGM 그룹을 재생하도록 수정 (OutGame/InGame 구분)
        if (scene.name == "Sce_MainMenu")
        {
            PlayBGMGroup("OutGameBGMs"); // 아웃게임 BGM 그룹 이름
        }
        else if (scene.name == "LSJ_Test")
        {
            PlayBGMGroup("InGameBGMs"); // 인게임 BGM 그룹 이름
        }
        else
        {
            StopBGM();
        }
    }

    #endregion
}