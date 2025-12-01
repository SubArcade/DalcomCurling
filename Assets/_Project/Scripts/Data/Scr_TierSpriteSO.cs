using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TierSpritePair
{
    public GameTier tier;     // enum 값
    public Sprite sprite;     // 매칭되는 이미지
}

[CreateAssetMenu(fileName = "TierSpriteSO", menuName = "SO/TierSpriteSO")]
public class Scr_TierSpriteSO : ScriptableObject
{
    [SerializeField] private List<TierSpritePair> tierSprites = new List<TierSpritePair>();

    private Dictionary<GameTier, Sprite> tierSpriteDict;

    private void OnEnable()
    {
        tierSpriteDict = new Dictionary<GameTier, Sprite>();
        foreach (var pair in tierSprites)
        {
            if (!tierSpriteDict.ContainsKey(pair.tier))
                tierSpriteDict.Add(pair.tier, pair.sprite);
        }
    }

    /// <summary>
    /// 해당 티어에 맞는 스프라이트 반환
    /// </summary>
    public Sprite GetSprite(GameTier tier)
    {
        if (tierSpriteDict != null && tierSpriteDict.TryGetValue(tier, out var sprite))
            return sprite;

        Debug.LogWarning($"[TierSpriteSO] 스프라이트 없음: {tier}");
        return null;
    }
}