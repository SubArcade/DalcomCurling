using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;


[System.Serializable, FirestoreData]
public class RankData
{
    [field: SerializeField, Tooltip("닉네임")] [FirestoreProperty] public string nickname { get; set; }
    [field: SerializeField, Tooltip("점수")] [FirestoreProperty] public int score { get; set; }
    [field: SerializeField, Tooltip("티어")] [FirestoreProperty] public GameTier tier { get; set; }
    [field: SerializeField, Tooltip("고유 아이디")] [FirestoreProperty] public string uid { get; set; }
}
