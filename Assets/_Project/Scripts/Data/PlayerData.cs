using Firebase.Firestore;
using UnityEngine;

[System.Serializable, FirestoreData]
public class PlayerData
{
    [field: SerializeField, Tooltip("이메일")] [FirestoreProperty] public string email { get; set; }
    [field: SerializeField, Tooltip("이메일")] [FirestoreProperty] public string nickname { get; set; }
    [field: SerializeField, Tooltip("골드")] [FirestoreProperty] public int gold { get; set; }
    [field: SerializeField, Tooltip("잼")] [FirestoreProperty] public int gem { get; set; }
    [field: SerializeField, Tooltip("에너지")] [FirestoreProperty] public int energy { get; set; }
    [field: SerializeField, Tooltip("레벨")] [FirestoreProperty] public int level { get; set; }
    [field: SerializeField, Tooltip("경험치")] [FirestoreProperty] public int exp { get; set; }
    [field: SerializeField, Tooltip("처음 접속 시간")] [FirestoreProperty] public Timestamp createAt { get; set; }
    [field: SerializeField, Tooltip("마지막 접속 시간")] [FirestoreProperty] public long lastAt { get; set; }

    // 랭크 데이터
    [field: SerializeField, Tooltip("솔로 점수")] [FirestoreProperty] public int soloScore { get; set; }
    [field: SerializeField, Tooltip("솔로 티어")] [FirestoreProperty] public GameTier soloTier { get; set; }

    // 로컬 플레이어 데이터 (디비에 들어가지 않는 변수)
    [field: SerializeField, Tooltip("최대 에너지")] public int maxEnergy { get; set; }
    [field: SerializeField, Tooltip("충전 시간(초)")] public int perSecEnergy { get; set; }
    
    // public PlayerData()
    // {
    //     email = "test@test.com";
    //     nickname = "test";
    //     gold = 250;
    //     gem = 7;
    //     energy = 10;
    //     level = 1;
    //     exp = 0;
    //     lastAt = 0;
    //     maxEnergy = 20;
    //     perSecEnergy = 10;
    //     soloScore = 0;
    //     soloTier = GameTier.Bronze;
    // }
}
