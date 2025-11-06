using System.Collections.Generic;
using UnityEngine;
using static DonutDatabase;

[CreateAssetMenu(fileName = "DonutDatabase", menuName = "Donut/Donut Database")]
public class DonutDatabase : ScriptableObject
{
    [System.Serializable]
    public class DonutInfo
    {
        [Header("기본 정보")]
        public string id;        // 고유 ID
        public Sprite sprite;    // 스프라이트 참조
        public string displayName; // UI 표시명

        [Header("설명, 수치")]
        [TextArea(2, 3)] public string description; //설명
        [Tooltip("")]public int returnCoin; // 휴지통 반환 금액
    }

    public List<DonutInfo> donuts = new List<DonutInfo>();

    private static DonutDatabase _instance;
    public static DonutDatabase Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<DonutDatabase>("DonutData/SO_DonutDatabase");
            return _instance;
        }
    }

    public static Sprite GetSpriteByID(string id)
    {
        foreach (var item in Instance.donuts)
        {
            if (item.id == id)
                return item.sprite;
        }
        return null;
    }

    public static string GetIDBySprite(Sprite sprite)
    {
        foreach (var item in Instance.donuts)
        {
            if (item.sprite == sprite)
                return item.id;
        }
        return "";
    }

    public static DonutInfo GetDonutBySprite(Sprite sprite)
    {
        foreach (var item in Instance.donuts)
        {
            if (item.sprite == sprite)
                return item;
        }
        return null;
    }
}
