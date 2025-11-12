using System.Collections.Generic;
using UnityEngine;

public enum DonutType
{
    Hard, //단단
    Soft, //말랑
    Moist //촉촉
}

[System.Serializable]
public class DonutData
{
    [Header("기본 정보")]
    [Tooltip("고유ID")] public string id;        // 고유 ID   donutType_Level
    [Range(1, 30), Tooltip("레벨")] public int level;
    [Tooltip("이미지")] public Sprite sprite;    // 스프라이트 참조
    [Tooltip("자세한 도넛 이름")] public string displayName; // UI 표시명
    
    [Header("설명, 수치")]
    [Tooltip("설명")] public string description; //설명
    [Tooltip("도넛 타입")] public DonutType donutType;
    [Tooltip("도넛 프리팹")] public GameObject prefab;

    [Tooltip("무게")] public int weight;
    [Tooltip("반발력")] public int resilience;
    [Tooltip("마찰력")] public int friction;

    [Header("보상")] [Tooltip("젬 보상")] public int rewardGem = 1;
}

[CreateAssetMenu(fileName = "DonutTypeSO", menuName = "Game/Donut/DonutTypeSO")]
public class DonutTypeSO : ScriptableObject
{
    public DonutType type;
    public List<DonutData> levels;
    
    public DonutData donutData = new DonutData();
    
    private void OnValidate()
    {
        if (levels == null) return;

        for (int i = 0; i < levels.Count; i++)
        {
            var d = levels[i];
            if (d == null) continue;

            d.level = i + 1; // 인덱스 기반으로 자동 설정 (1~30)
            d.donutType = type;
            d.id = $"{type}_{d.level}";
        }
    }

    public DonutData GetLevelData(int level)
    {
        return levels.Find(d => d.level == level);
    }
}
    