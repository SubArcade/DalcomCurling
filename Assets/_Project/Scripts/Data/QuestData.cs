using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[System.Serializable, FirestoreData]
public class QuestList
{
    [Tooltip("도넛 고유 아이디")] [FirestoreProperty] public string donutId { get; set; }    
    [Tooltip("보상 골드")] [FirestoreProperty] public int rewardGold { get; set; }
}

[System.Serializable, FirestoreData]
public class QuestData
{
    [field: SerializeField, Tooltip("도넛 앤트리")][FirestoreProperty] public List<QuestList> questList { get; set; } = new();
    
    // 로컬 퀘스트 데이터 (디비에 들어가지 않는 변수)
    public int refreshCount { get; set; }
    public int baseGold { get; set; }

}
