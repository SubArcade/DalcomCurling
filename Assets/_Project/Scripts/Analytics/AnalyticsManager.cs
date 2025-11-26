using Firebase.Analytics;
using UnityEngine;

public enum AnalyticsTimerType
{
    app_launch,
    main_enter,
    donutbook_open,
    upgrade_open,
    profile_open,
    entry_open,
    shop_open,
    ready_open,
    match_search_start,
    ingame_enter
}

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

    [SerializeField] private GameObject appLaunch;
    [SerializeField] private GameObject mainEnter;
    [SerializeField] private GameObject donutbookOpen;
    [SerializeField] private GameObject upgradeOpen;
    [SerializeField] private GameObject profileOpen;
    [SerializeField] private GameObject entryOpen;
    [SerializeField] private GameObject shopOpen;
    [SerializeField] private GameObject readyOpen;
    [SerializeField] private GameObject matchSearchStart;
    [SerializeField] private GameObject ingameEnter;
    
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

    // 화면 체류 시간 기록
    public void LogScreenTime(string eventName, string categoryName, float staySec)
    {
        FirebaseAnalytics.LogEvent(eventName,
            new Parameter("category", categoryName),
            new Parameter("stay_sec", staySec)
        );
    }

    // count 기록
    public void LogCount(string eventName, string categoryName)
    {
        FirebaseAnalytics.LogEvent(eventName,
            new Parameter("category", categoryName),
            new Parameter("count", 1));
    }
    
    // 인트로 Intro
    public void LoginSelect(string loginType) =>
        FirebaseAnalytics.LogEvent("login_select",
            new Parameter("category", "Intro"),
            new Parameter("login_type", loginType));

    public void LoginResult(string loginType, bool success) =>
        FirebaseAnalytics.LogEvent("login_result",
            new Parameter("category", "Intro"),
            new Parameter("result", success ? "Success" : "Fail"));
    
    public void IntroSelect(string type) => 
        FirebaseAnalytics.LogEvent("intro_select",
            new Parameter("category", "Intro"),
            new Parameter("intro_type", type));
    

    // 아웃게임 OutGame
    public void MergeAction() => LogCount("merge_action", "OutGame");
    public void MergeSuccess() => LogCount("merge_success", "OutGame");
    public void TrashUse() => LogCount("trash_use", "OutGame");
    public void MergeBoardFull() => LogCount("mergeBoard_full", "OutGame");
    public void QuestComplete() => LogCount("quest_complete", "OutGame");
    public void QuestRewardComplete() => LogCount("questReward_complete", "OutGame");

    // 상점 Shop
    public void ShopPurchaseTry() => LogCount("shop_purchase_try", "Shop");
    public void ShopPurchaseResult() => LogCount("shop_purchase_result", "Shop");

    // 엔트리/레디 Entry/Ready
    public void ReadyEquip(string donutType) => LogCount("ready_equip", "Entry/Ready");
    public void ReadyDonutAdjust() => LogCount("ready_donut_adjust", "Entry/Ready");
    
    // 매칭 Match
    public void MatchSearchCancel() => LogCount("match_search_cancel", "Match");

    // 인게임 InGame
    // public void ShotRelease(int power) =>
    //     FirebaseAnalytics.LogEvent("shot_release",
    //         new Parameter("power", power));
    //
    // public void MatchResult(bool win) =>
    //     FirebaseAnalytics.LogEvent("match_result",
    //         new Parameter("result", win ? "Win" : "Lose"));
    
    
    // 시간초 재는 타입들 
    public void LogTimer(AnalyticsTimerType type, float staySec)
    {
        string eventName = "";
        string category = "";

        switch (type)
        {
            case AnalyticsTimerType.app_launch:
                eventName = "app_launch";
                category = "Intro";
                break;

            case AnalyticsTimerType.main_enter:
                eventName = "main_enter";
                category = "Intro";
                break;

            case AnalyticsTimerType.donutbook_open:
                eventName = "donutbook_open";
                category = "OutGame";
                break;

            case AnalyticsTimerType.upgrade_open:
                eventName = "upgrade_open";
                category = "OutGame";
                break;

            case AnalyticsTimerType.profile_open:
                eventName = "profile_open";
                category = "OutGame";
                break;

            case AnalyticsTimerType.entry_open:
                eventName = "entry_open";
                category = "OutGame";
                break;

            case AnalyticsTimerType.shop_open:
                eventName = "shop_open";
                category = "Shop";
                break;

            case AnalyticsTimerType.ready_open:
                eventName = "ready_open";
                category = "Entry/Ready";
                break;

            case AnalyticsTimerType.match_search_start:
                eventName = "match_search_start";
                category = "Match";
                break;

            case AnalyticsTimerType.ingame_enter:
                eventName = "ingame_enter";
                category = "InGame";
                break;
        }

        LogScreenTime(eventName, category, staySec);
    }
    
    // 게임 오브젝트로 시간초 껏다 키는거 작동 시작
    // 이 함수만 호출하면 위에 LogTimer알아서 호출댐
    public void SetActivetLogTimer(AnalyticsTimerType type, bool isActive)
    {
        switch (type)
        {
            case AnalyticsTimerType.app_launch:
                appLaunch.SetActive(isActive);
                break;

            case AnalyticsTimerType.main_enter:
                mainEnter.SetActive(isActive);
                break;

            case AnalyticsTimerType.donutbook_open:
                donutbookOpen.SetActive(isActive);
                break;

            case AnalyticsTimerType.upgrade_open:
                upgradeOpen.SetActive(isActive);
                break;

            case AnalyticsTimerType.profile_open:
                profileOpen.SetActive(isActive);
                break;

            case AnalyticsTimerType.entry_open:
                entryOpen.SetActive(isActive);
                break;

            case AnalyticsTimerType.shop_open:
                shopOpen.SetActive(isActive);
                break;

            case AnalyticsTimerType.ready_open:
                readyOpen.SetActive(isActive);
                break;

            case AnalyticsTimerType.match_search_start:
                matchSearchStart.SetActive(isActive);
                break;

            case AnalyticsTimerType.ingame_enter:
                ingameEnter.SetActive(isActive);
                break;
        }
        
    }
}
