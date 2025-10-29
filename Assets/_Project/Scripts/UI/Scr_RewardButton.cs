/*using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scr_RewardButton : MonoBehaviour
{
    public TMP_Text statusText;
    
    void Awake()
    {
        Button button = gameObject.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick_WatchAd);
        }
    }
    private void OnEnable()
    {
        if (AdsRewarded.Instance != null)
            AdsRewarded.Instance.OnRewardEarned += OnReward;
    }
    private void OnDisable()
    {
        if (AdsRewarded.Instance != null)
            AdsRewarded.Instance.OnRewardEarned -= OnReward;
    }

    public void OnClick_WatchAd()
    {
        Debug.Log("광고 보기 버튼 클릭됨!");
        if (AdsRewarded.Instance.IsReady())
        {
            statusText.text = "광고 표시 중...";
            AdsRewarded.Instance.ShowRewarded();
        }
        else
        {
            statusText.text = "광고 준비 중...";
        }
    }

    private void OnReward(int amount, string type)
    {
        int gold = PlayerPrefs.GetInt("gold", 0) + amount;
        PlayerPrefs.SetInt("gold", gold);
        PlayerPrefs.Save();
        statusText.text = $"보상 획득: +{amount} {type} (총 {gold})";
    }
}*/