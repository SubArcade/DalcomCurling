using System;
using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

public class EnergyRegenNotifier : MonoBehaviour
{
    // 알림 채널
    [Header("안드로이드 설정창에서 볼수있는 설정")]
    [SerializeField] private const string CHANNEL_ID   = "energy_regen_channel";
    [SerializeField] private const string CHANNEL_NAME = "Energy Regen";
    [SerializeField] private const string CHANNEL_DESC = "Notify when energy is full";

    [Header("알림창에 보일 메시지")]
    [SerializeField] private string messageTitle = "에너지 가득!";
    [SerializeField] private string messageText = $"에너지가 회복되었어요.";

    // 알림 중복 방지용 (마지막 예약 ID 기억)
#if UNITY_ANDROID
    private static int? _lastScheduledId;
    private static bool _channelRegistered;
#endif

    // 프로젝트의 싱글톤/데이터 접근은 여기에 맞춰주세요
    private PlayerData playerData => DataManager.Instance.PlayerData;

    private void Awake()
    {
#if UNITY_ANDROID
        EnsureChannel();
#endif
    }
    
    void OnEnable()  => DataManager.Instance.PauseChanged += OnPauseChanged;
    void OnDisable() => DataManager.Instance.PauseChanged -= OnPauseChanged;

    private void OnPauseChanged(bool isPaused)
    {
        if (isPaused)
        {
            // 앱이 백그라운드로 감 → 종료 흐름
            ApplyLazyRegenAndReschedule();
        }
        else
        {
            // 앱 복귀 → 오프라인 누적 정산만
            ApplyLazyRegenOnly();
        }
    }
    
    public void ApplyLazyRegenOnly()
    {
        if (!DataManager.Instance.isLogin) return;
        LazyRegen();
    }
    
    public void ApplyLazyRegenAndReschedule()
    {
        
        if (!DataManager.Instance.isLogin)
        {
            CancelAllEnergyNotifications();
            return;
        }
        
        LazyRegen();            // 누적 시간만큼 회복
        RescheduleNotification(); // 가득 차는 시점에 알림 재예약
    }

    // 경과 시간만큼 회복(5분=300초당 +1), 잔여초 보존
    public void LazyRegen()
    {
        if (playerData.energy >= playerData.maxEnergy)
        {
            playerData.lastAt = NowUtcSeconds();
            return;
        }

        long now = NowUtcSeconds();

        // 초기 진입 보호
        if (playerData.lastAt <= 0 || playerData.lastAt > now)
            playerData.lastAt = now;

        long elapsed = now - playerData.lastAt; // 경과 초
        if (elapsed < playerData.perSecEnergy) return;

        // 몇 칸 회복 가능한지
        int gained = (int)(elapsed / playerData.perSecEnergy);
        if (gained <= 0) return;

        int before = playerData.energy;
        playerData.energy = Mathf.Min(playerData.maxEnergy, before + gained);

        // 사용된 초 만큼 lastAt 전진(잔여 초는 보존)
        long used = (long)gained * playerData.perSecEnergy;
        playerData.lastAt += used;

        Debug.Log($"[EnergyRegen] {before} → {playerData.energy} (+{gained}), elapsed={elapsed}s, carry={(elapsed - used)}s");

        // 가득이면 기준을 now로 스냅(선택, 보기 깔끔하게)
        if (playerData.energy >= playerData.maxEnergy)
            playerData.lastAt = now;
    }

    // 가득 차는 시간 계산 → 알림 예약
    private void RescheduleNotification()
    {
#if UNITY_ANDROID
        // 기존 예약 취소(이 채널에서 보낸 마지막 알림만 취소)
        if (_lastScheduledId.HasValue)
        {
            AndroidNotificationCenter.CancelScheduledNotification(_lastScheduledId.Value);
            _lastScheduledId = null;
        }

        var fullUtc = CalcFullTimeUtc();
        if (!fullUtc.HasValue)
        {
            // 이미 가득(20) → 알림 불필요
            return;
        }

        // FireTime은 LocalTime으로 넣기
        var fireLocal = fullUtc.Value.ToLocalTime();

        var n = new AndroidNotification
        {
            Title    = messageTitle,
            Text     = messageText,
            FireTime = fireLocal,
            // (선택) 고급 옵션
            // SmallIcon = "icon_32", LargeIcon = "icon_128",
            // ShouldAutoCancel = true,
        };

        int id = AndroidNotificationCenter.SendNotification(n, CHANNEL_ID);
        _lastScheduledId = id;

        Debug.Log($"[EnergyRegen] Full at {fireLocal} (local) / {fullUtc.Value} (UTC). NotiId={id}");
#endif
    }

    // 가득 도달 시각(UTC) 계산
    private DateTime? CalcFullTimeUtc()
    {
        if (playerData.energy >= playerData.maxEnergy) return null;

        // lastAt이 미래거나 미설정이면 지금으로 보정
        long now = NowUtcSeconds();
        if (playerData.lastAt <= 0 || playerData.lastAt > now)
            playerData.lastAt = now;

        int need = playerData.maxEnergy - playerData.energy;     // 남은 칸 수
        long secToFull = need * playerData.perSecEnergy;    // 남은 시간(초)
        long fullUtcSec = playerData.lastAt + secToFull;

        // 만약 기준시각이 너무 과거로 밀려있으면(이론상 이미 가득), now 기준으로 보정
        if (fullUtcSec < now) fullUtcSec = now;

        return FromUnixSeconds(fullUtcSec);
    }

    private void EnsureChannel()
    {
#if UNITY_ANDROID
        if (_channelRegistered) return;

        var ch = new AndroidNotificationChannel(
            CHANNEL_ID, CHANNEL_NAME, CHANNEL_DESC, Importance.Default
        );
        AndroidNotificationCenter.RegisterNotificationChannel(ch);
        _channelRegistered = true;
#endif
    }

    private long NowUtcSeconds() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    private DateTime FromUnixSeconds(long s) => DateTimeOffset.FromUnixTimeSeconds(s).UtcDateTime;
    
    public void CancelAllEnergyNotifications()
    {
#if UNITY_ANDROID
        if (_lastScheduledId.HasValue)
        {
            AndroidNotificationCenter.CancelScheduledNotification(_lastScheduledId.Value);
            AndroidNotificationCenter.CancelDisplayedNotification(_lastScheduledId.Value);
            _lastScheduledId = null;
        }
#endif
    }
    
    /// <summary>
    /// 다음 에너지 +1 까지 남은 초 (가득 차 있으면 0)
    /// </summary>
    public int GetSecondsToNextEnergy()
    {
        if (playerData == null) return 0;
        if (playerData.energy >= playerData.maxEnergy) return 0;

        long now = NowUtcSeconds();
        if (playerData.lastAt <= 0 || playerData.lastAt > now)
            playerData.lastAt = now;

        long elapsed = now - playerData.lastAt;
        long cycle = playerData.perSecEnergy;   // 1칸 차는 데 걸리는 초

        if (cycle <= 0) return 0;

        long remainInCycle = cycle - (elapsed % cycle);
        if (remainInCycle == cycle) remainInCycle = 0; // 막 갱신된 직후

        return (int)remainInCycle;
    }
    
}
