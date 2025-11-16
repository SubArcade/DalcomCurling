using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

public enum DonutDexViewState
{
    Question,   // 물음표
    Donut,    // 도넛 슬롯
    Reward    // 보상 슬롯
}

[System.Serializable, FirestoreData]
public class DonutEntry
{
    [field: SerializeField, Tooltip("도넛 고유 아이디")] [FirestoreProperty] public string id { get; set; }
    [field: SerializeField, Tooltip("도넛 타입")] [FirestoreProperty] public DonutType type { get; set; }
    
    // 앤트리에서만 적용되는 값
    // 기존 SO에서 값 받고 수정한 값
    [field: SerializeField, Tooltip("무게")] [FirestoreProperty] public int weight { get; set; }
    [field: SerializeField, Tooltip("반발력")] [FirestoreProperty] public int resilience { get; set; }
    [field: SerializeField, Tooltip("마찰력")] [FirestoreProperty] public int friction { get; set; }
}

[System.Serializable, FirestoreData]
public class DonutCodexData
{
    [field: SerializeField, Tooltip("고유 아이디")] [FirestoreProperty] public string id { get; set; }
    [field: SerializeField, Tooltip("도넛 타입")] [FirestoreProperty] public DonutType donutType { get; set; }
    [field: SerializeField, Tooltip("프리팹 이미지 상태")] [FirestoreProperty] public DonutDexViewState donutDexViewState { get; set; }
}

[System.Serializable, FirestoreData]
public class InventoryData
{
    [field: SerializeField, Tooltip("도넛 앤트리")][FirestoreProperty] public List<DonutEntry> donutEntries { get; set; } = new();
    // 캐릭터
    // 이팩트
    
    // 도감 데이터
    [field: SerializeField, Tooltip("도넛 도감 상태")][FirestoreProperty] public List<DonutCodexData> hardDonutCodexDataList { get; set; } = new();
    [field: SerializeField, Tooltip("도넛 도감 상태")][FirestoreProperty] public List<DonutCodexData> softDonutCodexDataList { get; set; } = new();
    [field: SerializeField, Tooltip("도넛 도감 상태")][FirestoreProperty] public List<DonutCodexData> moistDnutCodexDataList { get; set; } = new();
}
