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
    
    [Header("야간 알림 차단 시간")]
    [SerializeField] private int quietStartHour = 22;
    [SerializeField] private int quietEndHour = 8;

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

    public void IsNotificationsEnabled(bool value)
    {
        DataManager.Instance.PlayerData.energyFullRecharged = value;
        //Debug.Log(DataManager.Instance.PlayerData.energyFullRecharged);
        
        if (!value)
            CancelAllEnergyNotifications();
        else
            RescheduleNotification();
    }
    
    public void IsUseQuietHours(bool value)
    {
        DataManager.Instance.PlayerData.nightNotif = value;
        //Debug.Log(DataManager.Instance.PlayerData.nightNotif);
    }
    
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
        //RescheduleNotification(); // 가득 차는 시점에 알림 재예약
        
        if (DataManager.Instance.PlayerData.energyFullRecharged)
            RescheduleNotification();
        else
            CancelAllEnergyNotifications();
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

        //Debug.Log($"[EnergyRegen] {before} → {playerData.energy} (+{gained}), elapsed={elapsed}s, carry={(elapsed - used)}s");

        // 가득이면 기준을 now로 스냅(선택, 보기 깔끔하게)
        if (playerData.energy >= playerData.maxEnergy)
            playerData.lastAt = now;
    }

    // 가득 차는 시간 계산 → 알림 예약
    private void RescheduleNotification()
    {
#if UNITY_ANDROID
        // 알림 끄기
        if (!DataManager.Instance.PlayerData.energyFullRecharged)
        {
            Debug.Log("[EnergyRegen] Notifications disabled, skip scheduling.");
            return;
        }
        
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
        
        // 야간 시간
        if (DataManager.Instance.PlayerData.nightNotif)
        {
            fireLocal = AdjustToAllowedTime(fireLocal);
        }

        var n = new AndroidNotification
        {
            Title    = messageTitle,
            Text     = messageText,
            FireTime = fireLocal,
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
    
    // localTime이 조용한 시간(22:00 ~ 08:00) 안인지 체크
    private bool IsInQuietHours(DateTime localTime)
    {
        int hour = localTime.Hour;

        // 22 ~ 08 처럼 날짜를 넘기는 구간
        if (quietStartHour > quietEndHour)
        {
            // 22~23 또는 0~7
            return (hour >= quietStartHour) || (hour < quietEndHour);
        }
        else
        {
            // 예: 1~5 같이 하루 안에서 끝나는 구간
            return (hour >= quietStartHour) && (hour < quietEndHour);
        }
    }

    // 조용한 시간이라면, 다음 허용 시간(여기서는 아침 8시)으로 밀어줌
    private DateTime AdjustToAllowedTime(DateTime localTime)
    {
        if (!IsInQuietHours(localTime))
            return localTime;

        // 22~08 형태(밤→다음날 아침)
        if (quietStartHour > quietEndHour)
        {
            if (localTime.Hour >= quietStartHour)
            {
                // 밤 22시 이후면 "다음날 08:00"로
                var nextDay = localTime.Date.AddDays(1);
                return new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, quietEndHour, 0, 0);
            }
            else
            {
                // 0~7 시면 "오늘 08:00"로
                var today = localTime.Date;
                return new DateTime(today.Year, today.Month, today.Day, quietEndHour, 0, 0);
            }
        }
        else
        {
            // 일반 구간(예: 1~5 등)을 지원하려면 여기를 쓰면 됨 (지금은 안 써도 됨)
            var today = localTime.Date;
            var allowed = new DateTime(today.Year, today.Month, today.Day, quietEndHour, 0, 0);
            if (allowed <= localTime)
                allowed = allowed.AddDays(1);
            return allowed;
        }
    }

    
}
