using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    public static RecipeManager Instance { get; private set; }

    [Header("합성 레시피 목록")]
    public List<MergeRecipe> recipes = new List<MergeRecipe>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 두 스프라이트로 결과물 찾기
    public Sprite GetMergeResult(Sprite sprite1, Sprite sprite2)
    {
        // 스프라이트 → ID 변환
        string id1 = DonutDatabase.GetIDBySprite(sprite1);
        string id2 = DonutDatabase.GetIDBySprite(sprite2);

        // 두 스프라이트가 같지 않으면 합성 불가
        if (id1 != id2) return null;

        // 레시피에 해당 ID가 있으면 결과 Sprite 반환
        foreach (var recipe in recipes)
        {
            if (recipe.inputID == id1)
            {
                return DonutDatabase.GetSpriteByID(recipe.outputID);
            }
        }

        return null; // 합성 불가
    }

    // 합성 가능한지 확인
    public bool CanMerge(Sprite sprite1, Sprite sprite2)
    {
        return GetMergeResult(sprite1, sprite2) != null;
    }

    [System.Serializable]
    public class MergeRecipe
    {
        public string inputID;    // 합칠 도넛
        public string outputID;   // 결과물 도넛
    }

}