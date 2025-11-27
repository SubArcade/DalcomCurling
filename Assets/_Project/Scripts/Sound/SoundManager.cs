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
                        Debug.LogWarning($"[SoundManager] 딕셔너리에 이미 '{eventName}' 키가 존재합니다. 이벤트: {path}");
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

    // BGM 재생 (그룹/플레이리스트 관리)

    /// SoundGroup 이름으로 BGM 플레이리스트를 시작합니다
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
                string path;
                FMODUnity.RuntimeManager.StudioSystem.lookupPath(currentBGM.Guid, out path);
                string bgmName = GetEventName(path);
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
        Debug.Log("[SoundManager] BGM 정지됨.");
    }

    /// <summary>
    /// 현재 재생 중인 BGM 인스턴스의 FMOD 파라미터 값을 설정합니다.
    /// UI_LaunchIndicator_Firebase.cs와 같은 외부 스크립트에서 호출됩니다.
    /// </summary>
    /// <param name="parameterName">FMOD Studio에서 설정한 파라미터 이름입니다 (예: "MusicState").</param>
    /// <param name="value">파라미터에 설정할 값입니다 (0f 또는 1f).</param>
    public void SetBGMParameter(string parameterName, float value)
    {
        // 1. 현재 BGM 인스턴스가 유효한지 확인합니다.
        if (currentBGMInstance.isValid())
        {
            // 2. FMOD Studio API를 사용하여 파라미터 값을 설정합니다.
            FMOD.RESULT result = currentBGMInstance.setParameterByName(parameterName, value);

            if (result == FMOD.RESULT.OK)
            {
                Debug.Log($"[SoundManager] BGM 파라미터 '{parameterName}'가 {value}로 설정되었습니다.");
            }
            else
            {
                Debug.LogWarning($"[SoundManager] 파라미터 설정 오류 ({parameterName}:{value}). FMOD Result: {result}");
            }
        }
        else
        {
            // BGM이 재생되고 있지 않을 경우 경고 메시지 출력
            Debug.LogWarning($"[SoundManager] BGM 인스턴스가 유효하지 않아 파라미터 '{parameterName}'를 설정할 수 없습니다.");
        }
    }

    // SFX 재생 (세분화된 호출)

    /// 딕셔너리에 등록된 이벤트 이름으로 SFX를 재생합니다.
    /// 모든 인게임/아웃게임 SFX는 이 메서드를 통해 이름으로 호출됩니다.

    /// <param name="eventName">InitializeSoundMap에서 등록된 이벤트의 이름입니다.</param>
    public void PlaySFX(string eventName, Vector3 position = default(Vector3))
    {
        EventReference sfxRef = GetEventReference(eventName);

        if (!sfxRef.IsNull)
        {
            RuntimeManager.PlayOneShot(sfxRef, position);
        }
    }

    #region 아웃게임 SFX 사운드 모음

    // 일반 버튼 누를 시
    public void buttonClick(Vector3 position = default(Vector3))
    {
        PlaySFX("01_ui_menu_button_beep_19", position);
    }

    // 칸 선택 시 / 새로운 주문서 등장 시 / 유닛 이동 시(유닛끼리 자리 바뀔 시)
    public void selectSlotScroll(Vector3 position = default(Vector3))
    {
        PlaySFX("02_item_pickup_swipe_01", position);
    }

    // 도넛 생성 시
    public void createDonut(Vector3 position = default(Vector3))
    {
        PlaySFX("03_collect_item_13", position);
    }

    // 도넛 머지 시 
    public void mergeDonut(Vector3 position = default(Vector3))
    {
        PlaySFX("04_happy_collect_item_01", position);
    }

    // 기프트박스 머지 시
    public void mergeGiftBox(Vector3 position = default(Vector3))
    {
        PlaySFX("05_collect_item_11", position);
    }

    // 포화상태 시 더 생성하려고 시도할 때
    public void saturation(Vector3 position)
    {
        PlaySFX("06_jingle_chime_16_negative", position);
    }

    // 유닛 이동 시 (유닛끼리 자리 바뀔 시)
    public void moveUnit(Vector3 position)
    {
        PlaySFX("02_item_pickup_swipe_01", position);
    }

    // 도넛 판매 시
    public void sellDonut(Vector3 position)
    {
        PlaySFX("07_ui_menu_button_beep_23", position);
    }

    // 보상 수령 창 노출 시
    public void receiptReward(Vector3 position)
    {
        PlaySFX("08_collectable_item_bonus_03", position);
    }

    // 주문서 complete 버튼 터치 시
    public void completeScroll(Vector3 position)
    {
        PlaySFX("09_collect_item_15", position);
    }

    // complete 버튼 터치 후 골드 획득 시
    public void getGold(Vector3 position)
    {
        PlaySFX("10_coin_bag_ring_gemstone_item_15", position);
    }

    #endregion

    #region 인게임 SFX 모음

    // 캐릭터/도넛 엔트리 등장 사운드
    public void appearEntry(Vector3 position)
    {
        PlaySFX("01_explosion_small_02", position);
    }

    // VS 연출 사운드
    public void appearVS(Vector3 position)
    {
        PlaySFX("02_punch_grit_wet_impact_03", position);
    }

    // Round Start / Turn Start 사운드
    public void roundturnStart(Vector3 position)
    {
        PlaySFX("03_sci-fi_power_up_03", position);
    }

    // 도넛 선택 사운드
    public void selectDonut(Vector3 position)
    {
        PlaySFX("04_ui_menu_button_beep_11", position);
    }

    // 파워 드래그 사운드
    public void powerDrag(Vector3 position)
    {
        PlaySFX("05_powerup_whiz_nightvision_goggles_on_01", position);
    }

    // 스핀 조작 (좌/우 드래그) 사운드
    public void spinControl(Vector3 position)
    {
        PlaySFX("06_whistle_slide_up_06", position);
    }

    // 10초 타이머 사운드
    public void tenTimer(Vector3 position)
    {
        PlaySFX("07_ui_menu_button_beep_19", position);
    }

    // TIME OVER 사운드
    public void timeOver(Vector3 position)
    {
        PlaySFX("08_chime_bell_02", position);
    }

    // 타이밍 퍼펙트 터치 사운드
    public void timingPerfectTouch(Vector3 position)
    {
        PlaySFX("09_ui_menu_button_confirm_01", position);
    }

    // 타이밍 얼리 터치 사운드
    public void timingEarlyTouch(Vector3 position)
    {
        PlaySFX("10_ui_menu_button_confirm_06", position);
    }

    // 5배속 프리뷰 시작 사운드
    public void timerFast(Vector3 position)
    {
        PlaySFX("11_cartoon_electronic_computer_code_07", position);
    }

    // 얼음 위 활주 기본 사운드
    public void slideSound(Vector3 position)
    {
        PlaySFX("12_ice_cracking_melting_02", position);
    }

    // 도넛 - 도넛 충돌 약하게 사운드
    public void stoneCrashweak(Vector3 position)
    {
        PlaySFX("13_02_Synth_Boing_4", position);
    }
    
    // 도넛 - 도넛 충돌 강하게 사운드
    public void stoneCrashstrong(Vector3 position)
    {
        PlaySFX("14_cartoon_boing_jump_15", position);
    }

    // OUT 판정 사운드
    public void outDecide(Vector3 position)
    {
        PlaySFX("15_comedy_bite_chew_05", position);
    }

    // 턴 하이라이트 (가장 가까운 도넛 표시) 사운드
    public void highlightTurn(Vector3 position)
    {
        PlaySFX("16_ui_menu_button_beep_13", position);
    }

    // 라운드 점수 집계 사운드 
    public void resultSoundscore(Vector3 position)
    {
        PlaySFX("17_ui_menu_popup_message_07", position);
    }

    // DRAW 사운드 
    public void drawDecide(Vector3 position)
    {
        PlaySFX("18_music_sting_short_groovy_flute_03", position);
    }

    // YOU WIN 사운드 
    public void winDecide(Vector3 position)
    {
        PlaySFX("19_Positive_07", position);
    }

    // YOU LOSE 사운드 
    public void loseDecide(Vector3 position)
    {
        PlaySFX("20_Negative_09", position);
    }

    #endregion

    #region 사운드 볼륨 조절

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

    #endregion
}
