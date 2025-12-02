using System;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

/// <summary>
/// 게임 내 플레이어의 프로필 정보를 담는 데이터 모델입니다.
/// Firebase Firestore에 저장될 수 있도록 FirestoreData 속성을 가집니다.
/// </summary>
[System.Serializable, FirestoreData]
public class PlayerProfile
{
    [field: SerializeField][FirestoreProperty] public string Nickname { get; set; }
    [field: SerializeField][FirestoreProperty] public string Email { get; set; }
    [field: SerializeField][FirestoreProperty] public NamePlateType curNamePlateType { get; set; }
    
    [field: SerializeField][FirestoreProperty] public InventoryData Inventory { get; set; } // DataManager.cs에 정의된 InventoryData 사용
}
