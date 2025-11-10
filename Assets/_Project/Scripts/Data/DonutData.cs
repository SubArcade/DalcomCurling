using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable, FirestoreData]

public enum DonutType 
{
    Hard, //단단
    Soft, //말랑
    Moist //촉촉
}
public class DonutData
{
    
    [field: SerializeField, Tooltip("무게")][FirestoreProperty] private int Weight { get; set; }
    [field: SerializeField, Tooltip("반발력")][FirestoreProperty] private int Resilience { get; set; }
    [field: SerializeField, Tooltip("마찰력")][FirestoreProperty] private int Friction { get; set; }
    //인게임에 진입할때 호출할 데이터

    [field: SerializeField, Tooltip("도넛종류")][FirestoreProperty] private DonutType DonutType { get; set; }
    [field: SerializeField, Tooltip("단단한 도넛 레벨")][FirestoreProperty] public Dictionary<int, string> HardLevels { get; private set; } = new();

    [field: SerializeField, Tooltip("말랑한 도넛 레벨")][FirestoreProperty] public Dictionary<int, string> SoftLevels { get; private set; } = new();

    [field: SerializeField, Tooltip("촉촉한 도넛 레벨")][FirestoreProperty] public Dictionary<int, string> MoistLevels { get; private set; } = new();
}

    


