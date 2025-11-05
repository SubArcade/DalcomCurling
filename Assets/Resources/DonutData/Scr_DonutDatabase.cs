using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DonutDatabase", menuName = "Donut/Donut Database")]
public class DonutDatabase : ScriptableObject
{
    [System.Serializable]
    public class donutInfo
    {
        public string id;        // 고유 ID
        public Sprite sprite;    // 스프라이트 참조
    }

    public List<donutInfo> donuts = new List<donutInfo>();

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
}
