using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DonutDatabase", menuName = "Donut/Donut Database")]
public class DonutDatabase : ScriptableObject
{
    [System.Serializable]
    public class DonutInfo
    {
        [Header("기본 정보")]
        public string id;        // 고유 ID   donutType_Level
        public Sprite sprite;    // 스프라이트 참조
        public string displayName; // UI 표시명

        [Header("설명, 수치")]
        public string description; //설명
        public string donutType;              // 종류 (단단, 말랑, 촉촉)
        public int level;
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

    public static DonutInfo GetDonutByID(string id)
    {
        return Instance.donuts.Find(d => d.id == id);
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

    public static DonutInfo GetDonut(string type, int level)
    {
        return Instance.donuts.Find(d => d.donutType == type && d.level == level);
    }

    public static DonutInfo GetNextDonut(string currentID)
    {
        // 예: hard_1 → hard_2
        if (string.IsNullOrEmpty(currentID))
            return null;

        string[] parts = currentID.Split('_');
        if (parts.Length != 2) return null;

        string type = parts[0];
        if (!int.TryParse(parts[1], out int level)) return null;

        int nextLevel = level + 1;
        return GetDonut(type, nextLevel);
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

    public static List<DonutInfo> GetDonutsByLevel(int level)
    {
        return Instance.donuts.FindAll(d => d.level == level);
    }
}
