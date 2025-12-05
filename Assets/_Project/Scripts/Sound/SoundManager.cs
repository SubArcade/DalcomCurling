using FMODUnity;
using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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

    [Header("모든 사운드 그룹 설정")]
    [Tooltip("모든 FMOD EventReference를 그룹별로 분류하여 설정합니다.")]
    [SerializeField]
    private SoundGroup[] allSoundGroups;
    private Dictionary<string, EventReference> soundEventMap = new Dictionary<string, EventReference>();

    [Header("발사시 재생되는 랜덤 사운드 재생")]
    [Tooltip("발사 시 재생할 5가지 SFX 이벤트를 여기에 할당합니다.")]
    [SerializeField]
    private EventReference[] launchSFXEvents;

    [Header("도넛 충돌시 재생되는 랜덤 사운드 재생")]
    [Tooltip("충돌 사운드를 여기에 할당하기")]
    [SerializeField]
    private EventReference[] stoneCrashEvents;

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
            //DontDestroyOnLoad(gameObject);
            InitializeSoundMap();
        }
        else
        {
            Destroy(gameObject);
        }

        float savedBGM = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 1f);

        SetBGMVolume(savedBGM);
        SetSFXVolume(savedSFX);

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
                    string path;
                    FMODUnity.RuntimeManager.StudioSystem.lookupPath(eventRef.Guid, out path);
                    string eventName = GetEventName(path);

                    if (soundEventMap.ContainsKey(eventName))
                    {
                        //Debug.LogWarning($"[SoundManager] 딕셔너리에 이미 '{eventName}' 키가 존재합니다. 이벤트: {path}");
                        continue;
                    }

                    // 모든 개별 사운드(SFX, 단일 BGM, 인텐스 뮤직 등)는 딕셔너리로 관리
                    soundEventMap.Add(eventName, eventRef);
                }
            }
        }
        //Debug.Log($"[SoundManager] 사운드 맵 로드 완료. 총 {soundEventMap.Count}개 이벤트.");
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
        //Debug.LogWarning($"[SoundManager] 딕셔너리에서 '{eventName}' 이벤트 참조를 찾을 수 없습니다. 호출에 실패했습니다.");
        return new EventReference(); // Null EventReference 반환
    }

    #endregion

    #region BGM SFX 재생 관련 구성
    // BGM 재생 (그룹/플레이리스트 관리)

    /// SoundGroup 이름으로 BGM 플레이리스트를 시작합니다
    /// <param name="groupName">설정에서 정의된 BGM SoundGroup의 이름입니다 (예: OutGameBGMs, InGameBGMs).</param>
    public void PlayBGMGroup(string groupName)
    {
        SoundGroup group = System.Array.Find(allSoundGroups, g => g.Name == groupName);

        if (group == null || group.Events == null || group.Events.Length == 0)
        {
            //Debug.LogWarning($"[SoundManager] '{groupName}' BGM 그룹을 찾을 수 없거나 이벤트가 비어 있습니다. BGM을 정지합니다.");
            StopBGM();
            return;
        }

        List<EventReference> newPlaylist = new List<EventReference>(group.Events);

        // 현재 재생 목록과 새 목록이 동일하면 중복 재생 방지
        if (IsSamePlaylist(newPlaylist, currentBGMPlaylist))
        {
            //Debug.Log($"[SoundManager] 이미 '{groupName}' BGM 재생 목록이 재생 중입니다.");
            return;
        }

        //Debug.Log($"[SoundManager] BGM 플레이리스트 전환 시작: '{groupName}'");
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
            //Debug.LogWarning($"[SoundManager] {playlistName} BGM 플레이리스트가 비어있습니다!");
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
        //Debug.Log($"[SoundManager] {playlistName} BGM 플레이리스트 시작 (총 {currentBGMPlaylist.Count}곡)");

        // 일반 BGM 플레이리스트 루프 로직
        while (true)
        {
            if (currentBGMPlaylist.Count == 0) yield break;

            EventReference currentBGM = currentBGMPlaylist[currentBGMIndex];

            if (!currentBGM.IsNull)
            {
                string path;
                FMODUnity.RuntimeManager.StudioSystem.lookupPath(currentBGM.Guid, out path);
                string bgmName = GetEventName(path);
                //Debug.Log($"[SoundManager] BGM 재생: {bgmName} (인덱스: {currentBGMIndex + 1}/{currentBGMPlaylist.Count})");

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
                //Debug.LogWarning($"[SoundManager] 빈 BGM 이벤트 참조 (인덱스: {currentBGMIndex})");
                yield break;
            }
        }
    }

    /// 현재 재생 중인 BGM을 정지합니다.
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
        //Debug.Log("[SoundManager] BGM 정지됨.");
    }

    // SFX 재생 (세분화된 호출)

    /// 딕셔너리에 등록된 이벤트 이름으로 SFX를 재생합니다.
    /// 모든 인게임/아웃게임 SFX는 이 메서드를 통해 이름으로 호출됩니다.
    /// <param name="eventName">InitializeSoundMap에서 등록된 이벤트의 이름입니다.</param>
    public void PlaySFX(string eventName)
    {
        EventReference sfxRef = GetEventReference(eventName);

        if (!sfxRef.IsNull)
        {
            // 2D Playback: RuntimeManager.PlayOneShot(EventReference)만 사용
            FMODUnity.RuntimeManager.PlayOneShot(sfxRef);
        }
    }
    #endregion

    #region 아웃게임 SFX 사운드 모음

    // 일반 버튼 누를 시
    public void buttonClick()
    {
        PlaySFX("01_ui_menu_button_beep_19");
    }

    // 칸 선택 시 / 새로운 주문서 등장 시 / 유닛 이동 시(유닛끼리 자리 바뀔 시)
    public void selectSlotScroll()
    {
        PlaySFX("02_item_pickup_swipe_01");
    }

    // 도넛 생성 시
    public void createDonut()
    {
        PlaySFX("03_collect_item_13");
    }

    // 도넛 머지 시 
    public void mergeDonut()
    {
        PlaySFX("04_happy_collect_item_01");
    }

    // 기프트박스 머지 시
    public void mergeGiftBox()
    {
        PlaySFX("05_collect_item_11");
    }

    // 포화상태 시 더 생성하려고 시도할 때
    public void saturation()
    {
        PlaySFX("06_jingle_chime_16_negative");
    }

    // 유닛 이동 시 (유닛끼리 자리 바뀔 시)
    public void moveUnit()
    {
        PlaySFX("02_item_pickup_swipe_01");
    }

    // 도넛 판매 시
    public void sellDonut()
    {
        PlaySFX("07_ui_menu_button_beep_23");
    }

    // 보상 수령 창 노출 시
    public void receiptReward()
    {
        PlaySFX("08_collectable_item_bonus_03");
    }

    // 주문서 complete 버튼 터치 시
    public void completeScroll()
    {
        PlaySFX("09_collect_item_15");
    }

    // complete 버튼 터치 후 골드 획득 시
    public void getGold()
    {
        PlaySFX("10_coin_bag_ring_gemstone_item_15");
    }

    #endregion

    #region 인게임 SFX 모음

    // 캐릭터/도넛 엔트리 등장 사운드
    public void appearEntry()
    {
        PlaySFX("01_explosion_small_02");
    }

    // VS 연출 사운드
    public void appearVS()
    {
        PlaySFX("02_punch_grit_wet_impact_03");
    }

    // Round Start / Turn Start 사운드
    public void roundturnStart()
    {
        PlaySFX("03_sci-fi_power_up_03");
    }

    // 도넛 선택 사운드
    public void selectDonut()
    {
        PlaySFX("04_ui_menu_button_beep_19");
    }

    // 파워 드래그 사운드
    public void powerDrag()
    {
        PlaySFX("05_powerup_whiz_nightvision_goggles_on_01");
    }

    // 스핀 조작 (좌/우 드래그) 사운드
    public void spinControl()
    {
        PlaySFX("06_whistle_slide_up_06");
    }

    // 10초 타이머 사운드
    public void tenTimer()
    {
        
        PlaySFX("07_ui_menu_button_beep_11");
    }

    // TIME OVER 사운드
    public void timeOver()
    {
        PlaySFX("08_chime_bell_02");
    }

    // 타이밍 퍼펙트 터치 사운드
    public void timingPerfectTouch()
    {
        PlaySFX("09_ui_menu_button_confirm_01");
    }

    // 타이밍 얼리 터치 사운드
    public void timingEarlyTouch()
    {
        PlaySFX("10_ui_menu_button_confirm_06");
    }

    // 5배속 프리뷰 시작 사운드
    public void timerFast()
    {
        PlaySFX("11_cartoon_electronic_computer_code_07");
    }

    // 얼음 위 활주 기본 사운드
    public void slideSound()
    {
        PlaySFX("12_ice_cracking_melting_02");
    }

    // 도넛 - 도넛 충돌 약하게 사운드
    public void stoneCrash()
    {
        // 배열이 비어있는지 확인
        if (stoneCrashEvents == null || stoneCrashEvents.Length == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, stoneCrashEvents.Length);

        EventReference sfxRef = stoneCrashEvents[randomIndex];

        if (!sfxRef.IsNull)
        {
            FMODUnity.RuntimeManager.PlayOneShot(sfxRef);
            string path;
            FMODUnity.RuntimeManager.StudioSystem.lookupPath(sfxRef.Guid, out path);
        }
        else
        {
            //Debug.LogWarning($"[SoundManager] shotSound에서 인덱스 {randomIndex}에 유효한 이벤트가 없습니다.");
        }
    }


    // 도넛 - 도넛 충돌 강하게 사운드
    public void stoneCrashstrong()
    {
        PlaySFX("14_cartoon_boing_jump_15");
    }

    // OUT 판정 사운드
    public void outDecide()
    {
        PlaySFX("15_comedy_bite_chew_05");
    }

    // 턴 하이라이트 (가장 가까운 도넛 표시) 사운드
    public void highlightTurn()
    {
        PlaySFX("16_ui_menu_button_beep_13");
    }

    // 라운드 점수 집계 사운드 
    public void resultSoundscore()
    {
        PlaySFX("17_ui_menu_popup_message_07");
    }

    // DRAW 사운드 
    public void drawDecide()
    {
        PlaySFX("18_music_sting_short_groovy_flute_03");
    }

    // YOU WIN 사운드 
    public void winDecide()
    {
        PlaySFX("19_Positive_07");
    }

    // YOU LOSE 사운드 
    public void loseDecide()
    {
        PlaySFX("20_Negative_09");
    }


    // 발사시 나오는 사운드를 지정한 사운드중 하나가 랜덤으로 재생되게 하는 코드 추가
    public void shotSound()
    {
        // 배열이 비어있는지 확인
        if (launchSFXEvents == null || launchSFXEvents.Length == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, launchSFXEvents.Length);

        EventReference sfxRef = launchSFXEvents[randomIndex];

        if (!sfxRef.IsNull)
        {
            FMODUnity.RuntimeManager.PlayOneShot(sfxRef);
            string path;
            FMODUnity.RuntimeManager.StudioSystem.lookupPath(sfxRef.Guid, out path);
        }
        else
        {
            //Debug.LogWarning($"[SoundManager] shotSound에서 인덱스 {randomIndex}에 유효한 이벤트가 없습니다.");
        }
    }

    #endregion

    #region 사운드 볼륨 조절

    public void SetBGMVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        FMOD.Studio.Bus bgmBus = RuntimeManager.GetBus("bus:/BGM");
        bgmBus.setVolume(volume);
        //Debug.Log($"[SoundManager] BGM Bus 볼륨 설정됨: {volume}");
    }

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        FMOD.Studio.Bus sfxBus = RuntimeManager.GetBus("bus:/SFX");
        sfxBus.setVolume(volume);
        //Debug.Log($"[SoundManager] SFX Bus 볼륨 설정됨: {volume}");
    }

    #endregion
}
