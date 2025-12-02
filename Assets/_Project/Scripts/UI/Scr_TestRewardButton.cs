using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scr_TestRewardButton : MonoBehaviour
{
    [SerializeField, Tooltip("보상 선택")]private AdType rewardType;
    [SerializeField, Tooltip("보상수치")]private int reward;
    
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
        //Debug.Log("광고 보기 버튼 클릭됨!");
        if (AdsRewarded.Instance.IsReady())
        {
            //statusText.text = "광고 표시 중...";
            AdsRewarded.Instance.ShowRewarded(AdType.TEST);
        }
        else
        {
            //statusText.text = "광고 준비 중...";
        }
    }

    // 광고 수량 받는 곳
    private void OnReward(int amount, string type)
    {
        // int gold = PlayerPrefs.GetInt("gold", 0) + amount;
        // PlayerPrefs.SetInt("gold", gold);
        // PlayerPrefs.Save();

        switch (rewardType)
        {
            case AdType.TEST:
                break;
            case AdType.ENERGY:
                DataManager.Instance.PlayerData.energy += reward; 
                break;
            case AdType.REFRESH:
                DataManager.Instance.QuestData.refreshCount += reward;
                break;
            case AdType.GIFTBOX:
                GiveGiftBox();
                GiveGiftBox();
                UIManager.Instance.Open(PanelId.MainPanel);
                break;
            case AdType.Gem:
                DataManager.Instance.PlayerData.gem += reward;
                DataManager.Instance.GemChange(DataManager.Instance.PlayerData.gem);
                break;
        }
        Debug.Log($"보상 획득: +{amount} {type}");
    }
    
    private void GiveGiftBox()
    {
        // GiftBox Level 1 데이터 가져오기
        var giftData = DataManager.Instance.GetGiftBoxData(1);
        if (giftData == null)
        {
            return;
        }
        // 빈 칸 찾기
        var cell = BoardManager.Instance.FindEmptyActiveCell();
        if (cell == null)
        {
            // 빈 칸이 없으면 임시보관칸에 추가
            BoardManager.Instance.tempStorageSlot.Add(giftData);
            return;
        }
        // 빈 칸이 있으면 보드에 생성
        BoardManager.Instance.SpawnFromTempStorage(giftData);
    }
}