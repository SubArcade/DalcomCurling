using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NamePlate
{
    [Header("칭호 이미지")]
    public Sprite plateSprite;

    [Header("칭호 타입")]
    public NamePlateType plateType;
    
    [Header("칭호 타입")]
    public string koNamePlate;
    
    [Header("칭호 타입")]
    public string enNamePlate;
}

[CreateAssetMenu(fileName = "NamePlaateSO", menuName = "Game/Donut/NamePlaateSO")]
public class NamePlaateSO : ScriptableObject
{
    public List<NamePlate> namePlateList;
}
