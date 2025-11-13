using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable, FirestoreData]
public class CellData
{
    [Tooltip("보드판 X 좌표")] [FirestoreProperty] public int x { get; set; }
    [Tooltip("보드판 Y 좌표")] [FirestoreProperty] public int y { get; set; }
    [Tooltip("활성화 여부")] [FirestoreProperty] public bool isCellActive { get; set; }
    [Tooltip("도넛 고유 아이디")] [FirestoreProperty] public string donutId { get; set; } // 고유 ID 사용
    
    [Tooltip("퀘스트 활성화 여부")] [FirestoreProperty] public bool isQuestActive { get; set; }
}

[System.Serializable, FirestoreData]
public class MergeBoardData
{
    [field: SerializeField, Tooltip("보드칸 정보")][FirestoreProperty] public List<CellData> cells { get; set; } = new();
    
    // 로컬 머지 보드 데이터 (디비에 들어가지 않는 변수)
    [Tooltip("셀 총 칸수")] public int cellMax;
    [Tooltip("셀 가로 칸수")] public int cellWidth;
    [Tooltip("셀 세로 칸수")] public int cellLength;
}
