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
    [SerializeField] private ToastMessage toastMessage;

    public bool isActive { get; set; }
    public MergeItemUI occupant { get; set; }
    public int gridX, gridY;
    public string donutId;
    public bool isTempOccupied = false; // 임시점유 확인용

    private Sprite lockSprite;

    private void Awake()
    {
        toastMessage = FindObjectOfType<ToastMessage>();
    }

    public void Init(int x, int y)
    {
        gridX = x;
        gridY = y;
        SetActive(false);
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (lockOverlay != null) lockOverlay.gameObject.SetActive(!active);
    }

    public void SetLockLevel(int level)
    {
        requiredLevel = level;
        string path = $"DonutSprite/LockImage/lock_lv{level}";
        lockSprite = Resources.Load<Sprite>(path);
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
            if (!unlocked && lockSprite != null) lockOverlay.sprite = lockSprite;
        }
    }

    public bool IsEmpty() => occupant == null;

    public void SetItem(MergeItemUI item, DonutData data)
    {
        occupant = item;
        donutId = data != null ? data.id : null;
        if (item) item.BindToCell(this);
    }

    public void SetItem(MergeItemUI item, string id) //기프트박스 전용 오버로드 함수
    {
        occupant = item;
        donutId = id;
        if (item) item.BindToCell(this);
    }

    public void ClearItem()
    {
        occupant = null;
        donutId = null; // 연결해제
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isActive)
        {
            string msg = string.Format(
            LocalizationManager.Instance.GetText(LocalizationKey.toast_unlockRequirement),requiredLevel);
            toastMessage.Show(msg);
            return;
        }
        BoardManager.Instance.OnCellClicked(this);
    }
}
