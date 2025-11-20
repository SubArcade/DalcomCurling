using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GiftBoxData
{
    [Header("기본 정보")]
    [Tooltip("고유ID")] public string id;
    [Range(1, 4), Tooltip("레벨")] public int level;
    [Tooltip("이미지")] public Sprite sprite;    // 스프라이트 참조
    [Tooltip("도넛 타입")] public DonutType donutType;
    [Tooltip("도넛 프리팹")] public GameObject prefab;

    [Header("보상 수치")]
    [Tooltip("무게")] public int rewardGold;
    [Tooltip("반발력")] public int rewardEnergy;
    [Tooltip("마찰력")] public int rewardGem;
}

[CreateAssetMenu(fileName = "GiftBoxSO", menuName = "Game/Donut/GiftBoxSO")]
public class GiftBoxSO : ScriptableObject
{
    public DonutType type = DonutType.Gift;
    public List<GiftBoxData> levels;
    
    public GiftBoxData giftBoxData = new GiftBoxData();
    
    private void OnValidate()
    {
        if (levels == null) return;

        for (int i = 0; i < levels.Count; i++)
        {
            var d = levels[i];
            if (d == null) continue;

            d.level = i + 1;
            d.donutType = type;
            d.id = $"{type}_{d.level}";
        }
    }

    public GiftBoxData GetLevelData(int level)
    {
        return levels.Find(d => d.level == level);
    }
}