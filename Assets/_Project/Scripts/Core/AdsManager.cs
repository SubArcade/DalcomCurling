using System;
using System.Diagnostics;
using GoogleMobileAds.Api;
using UnityEngine;
using Debug = UnityEngine.Debug;

public enum AdType
{
    TEST,
    ENERGY,
    REFRESH,
    GIFTBOX
}

public class AdsRewarded : MonoBehaviour
{
    public static AdsRewarded Instance { get; private set; }

    // 보상형 광고단위 ID
    // ca-app-pub-4548432662417935/3098948422 안드로이드 배포용
    // ca-app-pub-3940256099942544/5224354917 안드로이드 테스트용
    private const string TestAD_UNIT_ID = "ca-app-pub-3940256099942544/5224354917"; // 안드로이드 테스트
    // 배포용
    private const string EnergyAD_UNIT_ID = "ca-app-pub-4548432662417935/3098948422"; // 에너지
    private const string RefreshAD_UNIT_ID = "ca-app-pub-4548432662417935/3823474752"; // 퀘스트 새로고침
    private const string GiftBoxAD_UNIT_ID = "ca-app-pub-4548432662417935/3098948422"; // 기프트박스
#if UNITY_ANDROID

#elif UNITY_IOS
    private const string AD_UNIT_ID = "apple"; // 아이폰
#else
    private const string AD_UNIT_ID = "other"; // 다른 플랫폼
#endif
    
    private RewardedAd rewardedAd;
    private bool sdkReady;

    public event Action<int, string> OnRewardEarned; // (amount, type)

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // SDK 초기화
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            if (initStatus == null)
            {
                Debug.LogError("[AdMob] Initialization failed.");
                return;
            }

            Debug.Log("[AdMob] Initialization complete.");

            // 어댑터 상태 로그 출력
            foreach (var adapter in initStatus.getAdapterStatusMap())
            {
                string name = adapter.Key;
                AdapterStatus status = adapter.Value;
                Debug.Log($"[AdMob] Adapter: {name}, State: {status.InitializationState}, Desc: {status.Description}");
            }

            // SDK 준비 완료 후 보상형 광고 로드
            sdkReady = true;
            LoadRewarded(AdType.TEST); 
        });
    }


    // 로드
    public void LoadRewarded(AdType adType)
    {
        if (!sdkReady) return;

        // 이전 광고 정리
        rewardedAd = null;
        var request = new AdRequest();
        string adUnitId = TestAD_UNIT_ID;

        switch (adType)
        {
            case AdType.TEST:
                adUnitId = TestAD_UNIT_ID;
                break;
            case AdType.ENERGY:
                adUnitId = EnergyAD_UNIT_ID;
                break;
            case AdType.REFRESH:
                adUnitId = RefreshAD_UNIT_ID;
                break;
            case AdType.GIFTBOX:
                adUnitId = GiftBoxAD_UNIT_ID;
                break;
        }
        
        RewardedAd.Load(adUnitId, request, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogWarning($"[AdMob] Rewarded Load Fail: {error}");
                Invoke(nameof(LoadRewarded), 2f); // 간단 재시도
                return;
            }

            rewardedAd = ad;
            Debug.Log("[AdMob] Rewarded Loaded");

            // 전체화면 표시 관련 콜백
            rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("[AdMob] Rewarded Closed → Preload next");
                LoadRewarded(adType); // 닫히면 다음 광고 미리 로드
            };
            rewardedAd.OnAdFullScreenContentFailed += (AdError err) =>
            {
                Debug.LogWarning($"[AdMob] Show Fail: {err}");
                LoadRewarded(adType);
            };
        });
    }

    public bool IsReady() => rewardedAd != null;

    // 표시
    public void ShowRewarded(AdType adType)
    {
        if (!IsReady())
        {
            Debug.Log("[AdMob] Not Ready. Reloading...");
            LoadRewarded(adType);
            return;
        }

        rewardedAd.Show((Reward reward) =>
        {
            int amount = Mathf.Max(1, (int)reward.Amount);
            string type = string.IsNullOrEmpty(reward.Type) ? "Reward" : reward.Type;

            Debug.Log($"[AdMob] Earned Reward: {amount} {type}");
            // 여기서 게임 보상 지급 로직 호출
            OnRewardEarned?.Invoke(amount, type);
        });

        rewardedAd = null; // 한 번 보여주면 참조 해제
    }
}
