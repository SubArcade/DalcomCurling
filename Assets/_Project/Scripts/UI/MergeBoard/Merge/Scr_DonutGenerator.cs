using System.Collections.Generic;
using UnityEngine;

public class DonutGenerator : MonoBehaviour
{
    [Header("생성기 레벨 (1~20)")]
    [Range(1, 20)] public int generatorLevel = 1;

    private DataManager Data => DataManager.Instance;

    [Header("이 생성기가 생성할 도넛 타입")]
    public DonutType generatorType;

    // 1~5레벨 기준 확률표
    // 배열: [1단계, 2단계, 3단계, 4단계, 5단계]
    private readonly Dictionary<int, float[]> chanceTable = new()
    {
        { 1, new[] {1.0f} },
        { 2, new[] {0.9f, 0.1f} },
        { 3, new[] {0.8f, 0.15f, 0.05f} },
        { 4, new[] {0.7f, 0.2f, 0.08f, 0.02f} },
        { 5, new[] {0.6f, 0.25f, 0.1f, 0.04f, 0.01f} }
    };

    private void Awake()
    {
        if (Data == null)
        {
            Debug.LogError("❌ DataManager가 아직 초기화되지 않았습니다.");
        }
    }

    private DonutType PickDonutType()
    {
        // 3개를 동일 확률로 선택하는 기본 구조
        int rand = Random.Range(0, 3);

        return rand switch
        {
            0 => DonutType.Hard,
            1 => DonutType.Soft,
            _ => DonutType.Moist
        };
    }

    public DonutData GetRandomDonut()
    {
        if (DataManager.Instance == null)
            return null;

        // 1. Hard/Soft/Moist 중 어떤 타입을 생성할지 선택
        DonutType selectedType = PickDonutType();

        // 2. 타입에 따른 생성기 레벨 얻기
        int generatorLevel = DataManager.Instance.GetGeneratorLevel(selectedType);

        // 3. 선택된 타입의 확률표 가져오기
        float[] chances = GetChanceArray(generatorLevel);

        // 4. 실제 생성될 도넛 단계 선택
        int chosenLevel = PickLevel(chances, generatorLevel);

        // 5. 해당 타입 + 단계의 도넛 목록 가져오기
        List<DonutData> list = DataManager.Instance.GetDonutsByTypeAndLevel(selectedType, chosenLevel);

        if (list == null || list.Count == 0)
        {
            Debug.LogError($"❌ {selectedType} {chosenLevel}레벨 도넛이 존재하지 않습니다!");
            return null;
        }

        // 6. 최종 생성
        DonutData result = list[Random.Range(0, list.Count)];

        Debug.Log($"생성된 도넛: Type={selectedType}, Level={chosenLevel}, Name={result.id}");
        DataManager.Instance.AddCodexEntry(selectedType, result.id, chosenLevel);
        
        return result;
    }


    /// <summary>
    /// 확률표를 이용해서 실제 도넛 '단계 레벨'을 고름
    /// </summary>
    private int PickLevel(float[] chances, int generatorLevel)
    {
        // 예: generatorLevel=3, chances.Length=3 → start = 1
        int start = Mathf.Max(1, generatorLevel - (chances.Length - 1));

        float rand = Random.value;
        float cumulative = 0f;

        for (int i = 0; i < chances.Length; i++)
        {
            cumulative += chances[i];
            if (rand <= cumulative)
            {
                return start + i; // 실제 도넛 레벨
            }
        }

        return start; // 혹시 분기 안 타면 최소 레벨 반환
    }

    /// <summary>
    /// 생성기 레벨에 맞는 확률배열 반환
    /// </summary>
    private float[] GetChanceArray(int level)
    {
        if (level <= 5)
            return chanceTable[level];

        // 6레벨 이상: 낮은 단계 제거하면서 고단계만 나오게
        int removeCount = level - 5;
        var baseArray = chanceTable[5];

        List<float> temp = new();

        for (int i = removeCount; i < baseArray.Length; i++)
            temp.Add(baseArray[i]);

        // 정규화
        float sum = 0f;
        foreach (var v in temp) sum += v;
        for (int i = 0; i < temp.Count; i++)
            temp[i] /= sum;

        return temp.ToArray();
    }
}
