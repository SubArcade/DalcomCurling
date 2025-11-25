using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GiftBoxData
{
    [Header("기본 정보")]
    [Tooltip("고유ID")] public string id;
    [Range(1, 5), Tooltip("레벨")] public int level;
    [Tooltip("이미지")] public Sprite sprite;    // 스프라이트 참조
    [Tooltip("타입")] public DonutType donutType;
    [Tooltip("프리팹")] public GameObject prefab;

    [Header("보상 수치")]
    [Tooltip("골드")] public int minGold, maxGold;

    [Tooltip("에너지")] public int minEnergy, maxEnergy;

    [Tooltip("보석")] public int minGem, maxGem;
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

            switch (d.level)
            {
                case 1:
                    d.minGold = 100; d.maxGold = 199;
                    d.minEnergy = 10; d.maxEnergy = 19;
                    d.minGem = 0; d.maxGem = 0;
                    break;

                case 2:
                    d.minGold = 200; d.maxGold = 299;
                    d.minEnergy = 20; d.maxEnergy = 29;
                    d.minGem = 0; d.maxGem = 0;
                    break;

                case 3:
                    d.minGold = 300; d.maxGold = 399;
                    d.minEnergy = 30; d.maxEnergy = 39;
                    d.minGem = 10; d.maxGem= 20;

                    break;

                case 4:
                    d.minGold = 400; d.maxGold = 499;
                    d.minEnergy = 40; d.maxEnergy = 49;
                    d.minGem = 20; d.maxGem = 30;
                    break;

                case 5:
                    d.minGold = 500; d.maxGold = 599;
                    d.minEnergy = 50; d.maxEnergy = 59;
                    d.minGem = 30; d.maxGem = 40;
                    break;
            }

        }

    }

    public GiftBoxData GetLevelData(int level)
    {
        return levels.Find(d => d.level == level);
    }
}