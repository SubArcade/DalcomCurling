/*
using System;
using UnityEngine;
using Unity.Notifications.Android;

public class Scr_EnergyRegenNotifier : MonoBehaviour
{
    public static Scr_EnergyRegenNotifier Instance { get; private set; }

    private const string ChannelId = "energy_regen_channel";

    private PlayerData playerData => DataManager.Instance.PlayerData;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ApplyLazyRegenAndReschedule();
    }

    // 백그라운드 or 복귀 시 회복량 계산 및 재등록
    /*private void OnApplicationPause(bool paused)
    {
        DataManager.Instance.SaveAllUserDataAsync();
        if (paused)
        {
            ApplyLazyRegenAndReschedule();
        }
        else ApplyLazyRegenAndReschedule();
    }

    // 앱 종료 시 
    private void OnApplicationQuit()
    {
        DataManager.Instance.SaveAllUserDataAsync();
        ApplyLazyRegenAndReschedule();
    }#1#
    
    // 회복 계산 후 알림 재등록
    private void ApplyLazyRegenAndReschedule()
    {
        LazyRegen();
        RescheduleNotification();
    }

    // 에너지 회복 계산
    private void LazyRegen()
    {
        if (playerData.energy >= playerData.maxEnergy) return;

        long now = NowUtcSeconds();
        long elapsedSec = Math.Max(0, now - playerData.lastAt);
        if (elapsedSec <= 0) return;

        float perMin = Mathf.Max(0.0001f, playerData.perminuteEnergy);
        float gainedFloat = (elapsedSec / 60f) * perMin;
        int gained = Mathf.FloorToInt(gainedFloat);
        if (gained <= 0) return;

        int before = playerData.energy;
        playerData.energy = Mathf.Min(playerData.maxEnergy, playerData.energy + gained);

        double usedSecForGained = (gained / perMin) * 60.0;
        playerData.lastAt = (long)Math.Min(now, playerData.lastAt + Math.Floor(usedSecForGained));

        Debug.Log($"[EnergyRegen] {before} → {playerData.energy} (elapsed {elapsedSec}s)");
    }

    // 알림 재예약
    public void RescheduleNotification()
    {
        AndroidNotificationCenter.CancelAllScheduledNotifications();

        var fullUtc = CalcFullTimeUtc();
        if (!fullUtc.HasValue) return;

        var fireLocal = fullUtc.Value.ToLocalTime();

        var ch = new AndroidNotificationChannel(
            ChannelId,
            "Energy Regen",
            "Notify when energy is full",
            Importance.Default
        );
        AndroidNotificationCenter.RegisterNotificationChannel(ch);

        var n = new AndroidNotification
        {
            Title = "풀 에너지",
            Text = "에너지 가득 참",
            FireTime = fireLocal
        };

        AndroidNotificationCenter.SendNotification(n, ChannelId);
    }

    // 에너지 도달 시간 계산 로직
    private DateTime? CalcFullTimeUtc()
    {
        if (playerData.energy >= playerData.maxEnergy) return null;

        float perMin = Mathf.Max(0.0001f, playerData.perminuteEnergy);
        int need = playerData.maxEnergy - playerData.energy;
        double minutesToFull = Math.Ceiling(need / perMin);
        long fullUtcSec = playerData.lastAt + (long)(minutesToFull * 60.0);
        return FromUnixSeconds(fullUtcSec);
    }
    
    // Util
    private static long NowUtcSeconds() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    private static DateTime FromUnixSeconds(long s) => DateTimeOffset.FromUnixTimeSeconds(s).UtcDateTime;
}
*/
