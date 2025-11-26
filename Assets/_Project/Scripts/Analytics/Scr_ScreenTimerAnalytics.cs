using Firebase.Analytics;
using UnityEngine;

// 화면 체류 시간 측정
public class Scr_ScreenTimerAnalytics : MonoBehaviour
{
    private float _enterTime;
    [SerializeField] private string eventName;
    [SerializeField] private string categoryName;

    private void OnEnable()
    {
        _enterTime = Time.time;
    }

    private void OnDisable()
    {
        float stayTime = Time.time - _enterTime;
        FirebaseAnalytics.LogEvent(eventName, 
            new Parameter("category", categoryName),
            new Parameter("stay_sec", stayTime)
        );
    }
}
