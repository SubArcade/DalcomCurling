using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Cells : MonoBehaviour, IPointerClickHandler
{
    [Header("잠금 상태 표시용 UI")]
    [SerializeField] private Image lockOverlay; // Cell 프리팹 안 자식 오브젝트
    public Image LockOverlay => lockOverlay;

    [SerializeField] private int requiredLevel; // 이 칸을 해금하기 위한 최소 플레이어 레벨

    public bool isActive { get; set; }
    public MergeItemUI occupant { get; set; }
    public int gridX, gridY;
    public string donutId;

    private Sprite lockSprite;

    public void Init(int x, int y)
    {
        gridX = x;
        gridY = y;
        SetActive(false);
    }

    public void SetActive(bool active)
    {
        isActive = active;

        // 활성화되지 않았으면 lockOverlay를 보이게
        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(!active);
    }

    public void SetLockLevel(int level)
    {
        requiredLevel = level;

        // lock_lv3, lock_lv5 등 이름 규칙으로 Resources 폴더에서 로드
        string path = $"DonutSprite/LockImage/lock_lv{level}";
        lockSprite = Resources.Load<Sprite>(path);

        //if (lockSprite == null)
        //    Debug.LogWarning($"잠금 이미지가 없습니다: {path}");
    }

    // 플레이어 레벨에 따라 활성/비활성 갱신
    public void UpdateLockState()
    {
        int playerLevel = DataManager.Instance.PlayerData.level;
        bool unlocked = playerLevel >= requiredLevel;

        isActive = unlocked;

        if (lockOverlay != null)
        {
            lockOverlay.gameObject.SetActive(!unlocked);
            if (!unlocked && lockSprite != null)
                lockOverlay.sprite = lockSprite;
        }
    }

    public bool IsEmpty() => occupant == null;

    public void SetItem(MergeItemUI item, DonutData data)
    {
        occupant = item;
        donutId = data.id;
        if (item) item.BindToCell(this);
    }


    public void ClearItem()
    {
        occupant = null;
        donutId = null; // 연결해제
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isActive) return;
        BoardManager.Instance.OnCellClicked(this);
    }
}
