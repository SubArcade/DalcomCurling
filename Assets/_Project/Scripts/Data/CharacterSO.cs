using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterSOList
{
    [Header("이펙트 기본 정보")]
    [Tooltip("타입")] public CharacterType characterType;
    [Tooltip("프리팹")] public GameObject characterPrefab;
}

[CreateAssetMenu(fileName = "CharacterSO", menuName = "SO/Product/CharacterSO")]
public class CharacterSO : ScriptableObject
{
    public List<CharacterSOList> characterSoList;
    
    
}