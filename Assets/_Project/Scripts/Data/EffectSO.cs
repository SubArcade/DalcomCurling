using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EffectSOList
{
    [Header("이펙트 기본 정보")]
    [Tooltip("타입")] public EffectType effectType;
    [Tooltip("프리팹")] public GameObject auraEffect;
    [Tooltip("프리팹")] public GameObject collisionEffect;
    [Tooltip("프리팹")] public GameObject trailEffect;
}

[CreateAssetMenu(fileName = "EffectSO", menuName = "SO/Product/EffectSO")]
public class EffectSO : ScriptableObject
{
    public List<EffectSOList> effectSoList;

    public EffectSOList GetEffectSO(EffectType effectType)
    {
        return effectSoList.Find(p => p.effectType == effectType);
    }
    
}