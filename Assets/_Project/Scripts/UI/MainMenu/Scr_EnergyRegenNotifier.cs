using System;
using UnityEngine;
using Unity.Notifications.Android;

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
    private static int? _lastScheduledId;
    private static bool _channelRegistered;

    // 프로젝트의 싱글톤/데이터 접근은 여기에 맞춰주세요
    private PlayerData playerData => DataManager.Instance.PlayerData;

    // playerData 가정:
    // int    energy;        // 현재 에너지
    // long   lastAt;        // 마지막 회복 '틱' 기준 시간(UTC seconds)
    // ※ lastAt가 0이면 초기화가 필요합니다. (아래에서 처리)

    private void Awake()
    {
        EnsureChannel();
    }

    private void Start()
    {
        // 앱 진입 시 한 번 정산 + 예약
        ApplyLazyRegenAndReschedule();
    }

    void OnEnable()  => DataManager.Instance.PauseChanged += ApplyLazyRegenAndReschedule;
    void OnDisable() => DataManager.Instance.PauseChanged -= ApplyLazyRegenAndReschedule;

    // 외부에서 에너지를 소비했을 때 호출하면 즉시 재예약
    public void OnEnergySpent(int amount)
    {
        if (amount <= 0) return;
        playerData.energy = Mathf.Max(0, playerData.energy - amount);

        // 소비한 시점이 새로운 기준이 되도록 lastAt도 조정(이전 잔여 누적은 유지됨)
        if (playerData.energy < playerData.maxEnergy && playerData.lastAt <= 0)
            playerData.lastAt = NowUtcSeconds();

        // 소비 즉시 가득 알림 다시 예약
        RescheduleNotification();
    }

    public void ApplyLazyRegenAndReschedule()
    {
        LazyRegen();            // 누적 시간만큼 회복
        RescheduleNotification(); // 가득 차는 시점에 알림 재예약
    }

    // 경과 시간만큼 회복(5분=300초당 +1), 잔여초 보존
    private void LazyRegen()
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
        if (_channelRegistered) return;

        var ch = new AndroidNotificationChannel(
            CHANNEL_ID, CHANNEL_NAME, CHANNEL_DESC, Importance.Default
        );
        AndroidNotificationCenter.RegisterNotificationChannel(ch);
        _channelRegistered = true;
    }

    private long NowUtcSeconds() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    private DateTime FromUnixSeconds(long s) => DateTimeOffset.FromUnixTimeSeconds(s).UtcDateTime;
}
