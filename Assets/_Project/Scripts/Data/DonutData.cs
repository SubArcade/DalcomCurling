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
    // 도넛 종류 넣기
    [field: SerializeField, Tooltip("무게")][FirestoreProperty] public int Weight { get; set; }
    [field: SerializeField, Tooltip("반발력")][FirestoreProperty] public int Resilience { get; set; }
    [field: SerializeField, Tooltip("마찰력")][FirestoreProperty] public int Friction { get; set; }
    //인게임에 진입할때 호출할 데이터

    [field: SerializeField, Tooltip("도넛레벨")][FirestoreProperty] private string Hard01 { get; set; }
    //메인메뉴에서 정보를 호출하기위한 키



    // 1 도넛 이름 -            2 도넛 종류                 
    // name= StarDonut 01~30 /  type = hard , soft, moist/
    //  ㄴ 스펙 마찰 무게 반발력


}
