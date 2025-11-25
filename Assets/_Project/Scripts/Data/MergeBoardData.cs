using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable, FirestoreData]
public class CellData
{
    [field: SerializeField, Tooltip("보드판 X 좌표")] [FirestoreProperty] public int x { get; set; }
    [field: SerializeField, Tooltip("보드판 Y 좌표")] [FirestoreProperty] public int y { get; set; }
    [field: SerializeField, Tooltip("활성화 여부")] [FirestoreProperty] public bool isCellActive { get; set; }
    [field: SerializeField, Tooltip("도넛 고유 아이디")] [FirestoreProperty] public string donutId { get; set; } // 고유 ID 사용
    [field: SerializeField, Tooltip("퀘스트 활성화 여부")] [FirestoreProperty] public bool isQuestActive { get; set; }
}

[System.Serializable, FirestoreData]
public class MergeBoardData
{
    [field: SerializeField, Tooltip("보드칸 정보")][FirestoreProperty] public List<CellData> cells { get; set; } = new();
    [field: SerializeField, Tooltip("임시 보관칸")][FirestoreProperty] public List<string> tempGiftIds { get; set; } = new();
    [field: SerializeField, Tooltip("생성기 단단 레벨")][FirestoreProperty] public int generatorLevelHard { get; set; }
    [field: SerializeField, Tooltip("생성기 말랑 레벨")][FirestoreProperty] public int generatorLevelSoft { get; set; }
    [field: SerializeField, Tooltip("생성기 촉촉 레벨")][FirestoreProperty] public int generatorLevelMoist { get; set; }

    // 로컬 머지 보드 데이터 (디비에 들어가지 않는 변수)
    [Tooltip("셀 총 칸수")] public int cellMax;
    [Tooltip("셀 가로 칸수")] public int cellWidth;
    [Tooltip("셀 세로 칸수")] public int cellLength;
}
