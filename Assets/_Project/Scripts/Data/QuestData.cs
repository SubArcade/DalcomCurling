using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

[System.Serializable, FirestoreData]
public class QuestList
{
    [field: SerializeField, Tooltip("도넛 고유 아이디")] [FirestoreProperty] public string donutId { get; set; }    
    [field: SerializeField, Tooltip("보상 골드")] [FirestoreProperty] public int rewardGold { get; set; }
}

[System.Serializable, FirestoreData]
public class QuestData
{
    [field: SerializeField, Tooltip("주문서1번")][FirestoreProperty] public List<QuestList> questList1 { get; set; } = new();
    [field: SerializeField, Tooltip("주문서2번")][FirestoreProperty] public List<QuestList> questList2 { get; set; } = new();
    [field: SerializeField, Tooltip("주문서3번")][FirestoreProperty] public List<QuestList> questList3 { get; set; } = new();

    // 로컬 퀘스트 데이터 (디비에 들어가지 않는 변수)
    [field: SerializeField, Tooltip("새로고침 횟수")][FirestoreProperty] public int refreshCount { get; set; }
    [field: SerializeField, Tooltip("새로고침 채우기 횟수")][FirestoreProperty] public int maxChargeCount { get; set; }
    [field: SerializeField, Tooltip("새로고침 채우기 횟수")][FirestoreProperty] public int currentChargeCount { get; set; }

    public int maxCount { get; set; }
    public int baseGold { get; set; }

}
