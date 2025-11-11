using UnityEngine;
using UnityEngine.Rendering;

public class DonutAppearance : MonoBehaviour
{
    [Header("현재 스탯")]
    public int Weight; // 무게
    public int Resilience; // 반탄력
    public int Friction; // 마찰력
    
    [Header("레벨 설정")]
    [Range(1, 30)] public int PlayerLevel = 1;

    private Transform fullDonut;
    private Transform holeDonut;
    private Transform starDonut;

    [Header("콜라이더 설정")]
    [SerializeField] private Collider fullDonutCollider;
    [SerializeField] private Collider holeDonutCollider;
    [SerializeField] private Collider starDonutCollider;

    private Collider currentCollider;
    private void Start()
    {
        InitializeDonutShapes();
        UpdateDonutAppearance();
    }
    private void InitializeDonutShapes()
    {
        if (transform.childCount >= 3)
        {
            fullDonut = transform.GetChild(0);
            holeDonut = transform.GetChild(1);
            starDonut = transform.GetChild(2);

            Debug.Log($"도넛 형태 초기화: Full={fullDonut.name}, Hole={holeDonut.name}, Star={starDonut.name}");

            SetAllDonutsActive(false);
        }
        else
        {
            Debug.LogError($"도넛 형태 오브젝트가 부족합니다. (필요: 3개, 현재: {transform.childCount}개)");
        }
    }

    public void UpdateDonutAppearance()
    {
        if (fullDonut == null || holeDonut == null || starDonut == null)
        {
            InitializeDonutShapes();
        }
        
        int maxState = Mathf.Max(Weight, Resilience, Friction);

        Debug.Log($"스탯 분석 - W:{Weight}, R:{Resilience}, F:{Friction}, 최대값:{maxState}, 레벨:{PlayerLevel}");

        // 1. 모든 도넛을 비활성화 하고
        SetAllDonutsActive(false);

        Transform selectedDonut = null;

        // 2. 가장 높은 스탯에 해당하는 도넛 타입 활성화
        if (Weight == maxState)
        {
            Debug.Log("FullDonut 활성화");
            selectedDonut = fullDonut;
        }
        else if (Resilience == maxState)
        {
            Debug.Log("HoleDonut 활성화");
            selectedDonut = holeDonut;
        }
        else if (Friction == maxState)
        {
            Debug.Log("StarDonut 활성화");
            selectedDonut = starDonut;
        }
        else
        {
            Debug.Log("기본값은 FullDonut 사용합니다");
            selectedDonut = fullDonut;
        }
        
        // 3. 선택된 도넛만 다시 활성화
        if (selectedDonut != null)
        {
            Debug.Log(selectedDonut.name + " 활성화 시작");
            selectedDonut.gameObject.SetActive(true); 
            Debug.Log(selectedDonut.name + " 활성화 완료");
            ActivateDonutWithPlayerLevel(selectedDonut);
        }

        UpdateCollider(selectedDonut);
    }

    // 모든 도넛 타입 비활성화
    private void SetAllDonutsActive(bool active) 
    {
        // 🔥 직접 변수 사용 (가장 안전한 방법)
        if (fullDonut != null)
        {
            fullDonut.gameObject.SetActive(active);
            Debug.Log("fullDonut " + (active ? "활성화" : "비활성화"));
        }
        if (holeDonut != null)
        {
            holeDonut.gameObject.SetActive(active);
            Debug.Log("holeDonut " + (active ? "활성화" : "비활성화"));
        }
        if (starDonut != null)
        {
            starDonut.gameObject.SetActive(active);
            Debug.Log("starDonut " + (active ? "활성화" : "비활성화"));
        }
    }

    // 조건에 할당된 레벨의 도넛만 활성화 시키기
    private void ActivateDonutWithPlayerLevel(Transform donutTypeParent)
    {
        if (donutTypeParent != null)
        {
            int childCount = donutTypeParent.childCount;

            if (childCount == 0)
            {
                Debug.LogError($"{donutTypeParent.name}에 자식 오브젝트가 없습니다");
                return;
            }

            // 모든 자식 오브젝트 비활성화
            for (int i = 0; i < childCount; i++)
            {
                donutTypeParent.GetChild(i).gameObject.SetActive(false);
            }

            // 플레이어 레벨에 해당하는 인덱스 계산 (1~30 → 0~29)
            int targetIndex = Mathf.Clamp(PlayerLevel - 1, 0, childCount - 1);

            Debug.Log($"활성화할 인덱스: {targetIndex} (레벨 {PlayerLevel} → 인덱스 {targetIndex})");

            // 해당 인덱스의 자식 활성화
            if (targetIndex >= 0 && targetIndex < childCount)
            {
                Transform targetChild = donutTypeParent.GetChild(targetIndex);
                targetChild.gameObject.SetActive(true);
                Debug.Log($"{donutTypeParent.name}의 {targetIndex}번째 자식 활성화: '{targetChild.name}'");
            }
            else
            {
                Debug.LogError($"유효하지 않은 인덱스: {targetIndex} (자식 수: {childCount})");
            }
        }
        else
        {
            Debug.LogError("도넛 타입 부모가 null입니다!");
        }
    }

    private void UpdateCollider(Transform selectedDonut)
    {
        // 기존 콜라이더 비활성화
        if (currentCollider != null)
            currentCollider.enabled = false;

        // 새 콜라이더 설정
        if (selectedDonut == fullDonut)
            currentCollider = fullDonutCollider;
        else if (selectedDonut == holeDonut)
            currentCollider = holeDonutCollider;
        else if (selectedDonut == starDonut)
            currentCollider = starDonutCollider;

        // 새 콜라이더 활성화
        if (currentCollider != null)
            currentCollider.enabled = true;
    }
}