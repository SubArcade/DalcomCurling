using Firebase.Analytics;
using UnityEngine;

// 화면 체류 시간 측정
public class Scr_ScreenTimerAnalytics : MonoBehaviour
{
    private float _enterTime;
    [SerializeField] private AnalyticsTimerType analyticsTimerType;

    private void OnEnable()
    {
        _enterTime = Time.time;
    }

    private void OnDisable()
    {
        float stayTime = Time.time - _enterTime;
        AnalyticsManager.Instance.LogTimer(analyticsTimerType, stayTime);
    }
}
