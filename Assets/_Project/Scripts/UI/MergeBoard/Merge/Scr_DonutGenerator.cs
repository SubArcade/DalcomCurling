using System.Collections.Generic;
using UnityEngine;

public class DonutGenerator : MonoBehaviour
{
    [Header("생성기 레벨 (1~20)")]
    [Range(1, 20)] public int generatorLevel = 1;

    [Header("도넛 데이터베이스 (자동 참조)")]
    public DataManager dataManager;

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
        if (dataManager == null)
            dataManager = DataManager.Instance;
    }

    // 생성기 레벨에 따라 도넛 확률 기반으로 랜덤 도넛 선택
    public DonutData GetRandomDonut()
    {
        if (dataManager == null)
        {
            Debug.LogError("DataManager가 초기화되지 않았습니다.");
            return null;
        }

        // 확률배열 가져오기
        float[] activeChances = GetChanceArrayForLevel(generatorLevel);
        if (activeChances == null || activeChances.Length == 0)
        {
            Debug.LogWarning($"{generatorLevel}레벨 확률 데이터를 찾을 수 없습니다.");
            return null;
        }
        
        

        // 레벨별 범위 계산
        // 1~5레벨은 1단계부터 시작 / 6레벨 이상은 낮은 단계 제외
        int startLevel = Mathf.Max(1, generatorLevel - (activeChances.Length - 1));
        int endLevel = startLevel + activeChances.Length - 1;

        // 확률 기반으로 레벨 선택
        float rand = Random.value;
        float cumulative = 0f;
        int chosenLevel = startLevel;

        // 해당 레벨의 모든 도넛 목록 가져오기
        List<DonutData> availableDonuts = dataManager.GetDonutsByLevel(chosenLevel);
        if (availableDonuts == null || availableDonuts.Count == 0)
        {
            Debug.LogWarning($"{chosenLevel}레벨 도넛이 DataManager에서 비어있습니다.");
            return null;
        }

        for (int i = 0; i < activeChances.Length; i++)
        {
            cumulative += activeChances[i];
            if (rand <= cumulative)
            {
                chosenLevel = startLevel + i;
                break;
            }
        }

        // 랜덤 타입 선택
        DonutData selected = availableDonuts[Random.Range(0, availableDonuts.Count)];
        Debug.Log($"생성기 Lv.{generatorLevel} → 도넛 Lv.{chosenLevel} ({selected.displayName}) 생성됨");

        return selected;
    }

    // 생성기 레벨에 맞는 확률배열 반환
    private float[] GetChanceArrayForLevel(int generatorLevel)
    {
        if (generatorLevel <= 5)
        {
            // 기본 확률표 그대로 사용
            return chanceTable[generatorLevel];
        }

        // 6레벨 이상일 경우 — 낮은 단계 확률이 점점 사라짐 (왼쪽 잘라내기)
        int shift = generatorLevel - 5; // 예: 6레벨이면 1칸, 7레벨이면 2칸 자름
        var baseArray = chanceTable[5];
        List<float> shifted = new();

        // 왼쪽 요소 제거 (낮은 단계 제거)
        for (int i = shift; i < baseArray.Length; i++)
            shifted.Add(baseArray[i]);

        // 정규화 (합이 1 되도록)
        float sum = 0f;
        foreach (float f in shifted) sum += f;
        for (int i = 0; i < shifted.Count; i++) shifted[i] /= sum;

        return shifted.ToArray();
    }
}
