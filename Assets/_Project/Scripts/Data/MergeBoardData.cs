using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CellData
{
    public int x;
    public int y;
    public bool isActive;
    public string donutID; // 고유 ID 사용
}

[System.Serializable, FirestoreData]
public class MergeBoardData : MonoBehaviour
{
    [field: SerializeField, Tooltip("보드칸 정보")][FirestoreProperty] public List<CellData> cells { get; set; } = new();
}
