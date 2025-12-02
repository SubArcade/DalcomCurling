using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CharacterSOList
{
    [Header("이펙트 기본 정보")]
    [Tooltip("타입")] public CharacterType characterType;
    [Tooltip("프리팹")] public GameObject characterPrefab;
    [Tooltip("이미지")] public Sprite characterSprite;
}

[CreateAssetMenu(fileName = "CharacterSO", menuName = "SO/Product/CharacterSO")]
public class CharacterSO : ScriptableObject
{
    public List<CharacterSOList> characterSoList;

    public Sprite GetCharacterSprite(CharacterType type) 
    {
        if (characterSoList == null) return null;

        var entry = characterSoList.Find(c => c.characterType == type);
        if (entry != null) return entry.characterSprite; 

        return null;
    }
    
}