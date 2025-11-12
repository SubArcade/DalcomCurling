using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[System.Serializable, FirestoreData]
public class DonutEntry
{
    [Tooltip("도넛 고유 아이디")] [FirestoreProperty] public string id { get; set; }
    [Tooltip("도넛 타입")] [FirestoreProperty] public DonutType type { get; set; }
    
    // 앤트리에서만 적용되는 값
    // 기존 SO에서 값 받고 수정한 값
    [Tooltip("무게")] [FirestoreProperty] public int weight { get; set; }
    [Tooltip("반발력")] [FirestoreProperty] public int resilience { get; set; }
    [Tooltip("마찰력")] [FirestoreProperty] public int friction { get; set; }
}

[System.Serializable, FirestoreData]
public class InventoryData
{
    [field: SerializeField, Tooltip("도넛 앤트리")][FirestoreProperty] public List<DonutEntry> donutEntries { get; set; } = new();
    // 캐릭터
    // 이팩트
}
