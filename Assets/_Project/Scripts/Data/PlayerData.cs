using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

public enum AuthProviderType
{
    Guest,  // 게스트
    GooglePlay, // 구글 플레이
}

public enum NamePlateType
{
    NONE = 0, // 없음
    NP1 = 1,
    NP2 = 2,
    NP3 = 3,
    NP4 = 4,
    NP5 = 5,
    NP6 = 6,
}

[System.Serializable, FirestoreData]
public class PlayerData
{
    [field: SerializeField, Tooltip("이메일")] [FirestoreProperty] public string email { get; set; }
    [field: SerializeField, Tooltip("닉네임")] [FirestoreProperty] public string nickname { get; set; }
    [field: SerializeField, Tooltip("골드")] [FirestoreProperty] public int gold { get; set; }
    [field: SerializeField, Tooltip("잼")] [FirestoreProperty] public int gem { get; set; }
    [field: SerializeField, Tooltip("에너지")] [FirestoreProperty] public int energy { get; set; }
    [field: SerializeField, Tooltip("레벨")] [FirestoreProperty] public int level { get; set; }
    [field: SerializeField, Tooltip("경험치")] [FirestoreProperty] public int exp { get; set; }
    [field: SerializeField, Tooltip("처음 접속 시간")] [FirestoreProperty] public Timestamp createAt { get; set; }
    [field: SerializeField, Tooltip("마지막 접속 시간")] [FirestoreProperty] public long lastAt { get; set; }
    
    [field: SerializeField, Tooltip("계정 연동")] [FirestoreProperty] public AuthProviderType authProviderType { get; set; }
    [field: SerializeField, Tooltip("현재 칭호")] [FirestoreProperty] public NamePlateType curNamePlateType { get; set; }
    [field: SerializeField, Tooltip("획득한 칭호")] [FirestoreProperty] public List<NamePlateType> gainNamePlateType { get; set; } = new List<NamePlateType>();
    [field: SerializeField, Tooltip("닉네임 변경 횟수")] [FirestoreProperty] public int changeNicknameCount { get; set; }
    

    // 랭크 데이터
    [field: SerializeField, Tooltip("솔로 점수")] [FirestoreProperty] public int soloScore { get; set; }
    [field: SerializeField, Tooltip("솔로 티어")] [FirestoreProperty] public GameTier soloTier { get; set; }

    // 로컬 플레이어 데이터 (디비에 들어가지 않는 변수)
    [Tooltip("최대 에너지")] public int maxEnergy { get; set; }
    [Tooltip("충전 시간(초)")] public int perSecEnergy { get; set; }
    [Tooltip("플레이어 만랩")] public int levelMax { get; set; }
}
