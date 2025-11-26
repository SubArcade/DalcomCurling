using System.Collections.Generic;
using UnityEngine;

public class DonutManager : MonoBehaviour
{
    public static DonutManager Instance;

    [Header("도넛 프리팹")]
    [SerializeField] private GameObject donutMasterPrefab; // donutMasterPrefab = 생산되는 도넛의 변수
    
    [Header("도넛 선택하기")]
    [SerializeField] public int selectDonutIndex = 0;

    [Header("새 도넛 생성용 스탯")]
    [Range(1, 30)] public int Weight = 0;
    [Range(1, 30)] public int Resilience = 0;
    [Range(1, 30)] public int Friction = 0;

    [Header("레벨 설정")]
    [Range(1, 30)] public int DonutLevel = 1;

    private List<GameObject> spawnedDonuts = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnDonut()
    {
        // 항상 새 도넛 생성 (기존 도넛 유지)
        Vector3 spawnPosition = new Vector3(0, 0, spawnedDonuts.Count * 1.5f); // 도넛의 생성 위치를 조정함
        
        GameObject newDonut = Instantiate(donutMasterPrefab, spawnPosition, Quaternion.identity);
        DonutAppearance appearance = newDonut.GetComponent<DonutAppearance>();

        appearance.Weight = Weight;
        appearance.Resilience = Resilience;
        appearance.Friction = Friction;
        appearance.PlayerLevel = DonutLevel;
        appearance.UpdateDonutAppearance();
        spawnedDonuts.Add(newDonut);

        Debug.Log($"도넛 생성: 무게{Weight} 반탄력{Resilience} 마찰력{Friction} 도넛레벨{DonutLevel} (총 {spawnedDonuts.Count}개)" );
    }

    public void UpdateDonut() // 도넛을 현재 조정한 수치값으로 모두 변경하기
    {
        // 모든 도넛 업데이트
        foreach (GameObject donut in spawnedDonuts)
        {
            if (donut != null)
            {
                DonutAppearance appearance = donut.GetComponent<DonutAppearance>();
                appearance.Weight = Weight;
                appearance.Resilience = Resilience;
                appearance.Friction = Friction;
                appearance.PlayerLevel = DonutLevel;
                appearance.UpdateDonutAppearance();
            }
        }
        Debug.Log($"모든 도넛 업데이트: 무게{Weight} 반탄력{Resilience} 마찰력{Friction} ({spawnedDonuts.Count}개)");
    }

    public void UpdateAllDonutsLevel()
    {
        foreach (GameObject donut in spawnedDonuts)
        {
            if (donut != null)
            {
                DonutAppearance appearance = donut.GetComponent<DonutAppearance>();
                appearance.PlayerLevel = DonutLevel;
                appearance.UpdateDonutAppearance();
            }
        }
        Debug.Log($"모든 도넛 레벨 업데이트: {DonutLevel} ({spawnedDonuts.Count}개)");
    }

    public void RemoveDonut() // 생성된 도넛을 차례대로 제거하기
    {
        if (spawnedDonuts.Count > 0)
        {
            int lastIndex = spawnedDonuts.Count - 1;
            GameObject lastDonut = spawnedDonuts[lastIndex];

            if (lastDonut != null)
            {
                DestroyImmediate(lastDonut);
            }

            spawnedDonuts.RemoveAt(lastIndex);
            Debug.Log("마지막 도넛 제거됨");
        }
        else
        {
            Debug.LogWarning("제거할 도넛이 없습니다.");
        }
    }

    public void RemoveAllDonuts() // 모든 도넛 제거
    {
        // 역순으로 제거 (에러 방지)
        for (int i = spawnedDonuts.Count - 1; i >= 0; i--)
        {
            if (spawnedDonuts[i] != null)
            {
                DestroyImmediate(spawnedDonuts[i]);
            }
        }
        spawnedDonuts.Clear();
        Debug.Log("모든 도넛 제거됨");
    }

    // 파티클 Inspector 버튼 추가 
    public List<GameObject> GetSpawnedDonuts()
    {
        return spawnedDonuts;
    }

    public int GetSpawnedDonutCount()
    {
        return spawnedDonuts.Count;
    }
}