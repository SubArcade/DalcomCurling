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
        foreach (MergeRecipe recipe in recipes)
        {
            // 같은 스프라이트이고 레시피에 등록된 경우
            if (sprite1 == sprite2 && recipe.inputSprite == sprite1)
            {
                return recipe.outputSprite;
            }
        }
        return null; // 합성 불가
    }

    // 합성 가능한지 확인
    public bool CanMerge(Sprite sprite1, Sprite sprite2)
    {
        return GetMergeResult(sprite1, sprite2) != null;
    }
}