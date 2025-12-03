using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EntrySlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("현재 슬롯에 들어있는 도넛")]
    public MergeItemUI currentItem;

    [Header("엔트리에서 생성할 도넛 프리팹")]
    [SerializeField] private MergeItemUI itemPrefab;

    public int slotIndex = 0;
    
    private Image slotImage;

    void Awake()
    {
        slotImage = GetComponent<Image>();
    }
    
    void OnEnable()
    {
        LoadFromInventory();
    }

    // 슬롯이 비었는지 여부
    public bool IsEmpty => currentItem == null || currentItem.gameObject == null;

    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag?.GetComponent<MergeItemUI>();
        // Debug.Log($"{dragged.currentCell.gridX}");
        // Debug.Log($"{dragged.currentCell.gridY}");
        if (dragged == null) return;

        //dragged.DetachFromCurrentCell();  // 보드에서온거 셀 해제
        //dragged.isFromEntry = true;
      
        // 같은 슬롯이면 취소
        if (dragged.transform.parent == transform)
        {
            dragged.ResetPosition();
            return;
        }

        if (dragged.isFromTemp) //임시보관칸에서 온거 차단
        {
            dragged.ResetPosition();
            return;
        }

        Debug.Log($"[OnDrop] {name} 드롭 시도 - 현재 currentItem: {currentItem?.name}");

        EntrySlot fromSlot = dragged.OriginalParent?.GetComponent<EntrySlot>();

        EntrySlot toSlot = this;

        Cells fromCell = dragged.originalCell;

        if (fromSlot != null && fromCell != null) // 엔트리에서 보드로 이동
        {
            fromSlot.currentItem = null;
            fromSlot.SaveToInventory();   // ← null 저장
        }

        // 엔트리에서 보드 스왑
        if (fromCell != null && currentItem != null)
        {
            SwapWithCell(fromCell, this, dragged);
            return;
        }

        // 엔트리 안에서 스왑
        if (fromSlot != null && toSlot != null && toSlot.currentItem != null)
        {
            SwapItems(fromSlot, toSlot, dragged);
            return;
        }
        //Debug.Log($"{dragged.currentCell.gridX}");
        MoveIn(dragged);
    }

    private void MoveIn(MergeItemUI dragged)
    {
        //Debug.Log($"{dragged.currentCell.gridX}");
        // Gift 타입은 차단
        if (dragged.donutData != null && dragged.donutData.donutType == DonutType.Gift)
        {
            dragged.ResetPosition();
            return;
        }

        // 기존 슬롯의 currentItem 초기화
        var oldSlot = dragged.OriginalParent?.GetComponent<EntrySlot>();
        if (oldSlot != null && oldSlot != this)
            oldSlot.currentItem = null;

        dragged.transform.SetParent(transform, false);
        var rt = dragged.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;

        currentItem = dragged;
        dragged.isFromEntry = true;  // 도넛 엔트리인지

        dragged.UpdateOriginalParent(transform);

        CheckMarkOff(dragged);

        SaveToInventory(); //도넛 값 넣기

        if (dragged.currentCell != null) dragged.currentCell.ClearItem();
        BoardManager.Instance.AutoSaveBoardLocal();

        // 해당 도넛 데이터 연동
        DataManager.Instance.SetDonutAt(slotIndex, false, donutData: currentItem.donutData);
        //Debug.Log($"[MoveIn] {currentItem.donutData.id} 슬롯에 도넛 들어감");
        // Debug.Log($"[MoveIn] {name} 슬롯에 도넛 들어감");
        // Debug.Log($"currentItem: {currentItem?.name}");
        // Debug.Log($"IsEmpty: {IsEmpty}");
    }

    // 엔트리에서 보드 스왑
    private void SwapWithCell(Cells fromCell, EntrySlot toSlot, MergeItemUI dragged)
    {
        // Gift 타입은 차단
        if (dragged.donutData != null && dragged.donutData.donutType == DonutType.Gift)
        {
            dragged.ResetPosition();
            return;
        }

        MergeItemUI entryItem = toSlot.currentItem;     // 엔트리에 있던 아이템

        // 1) dragged(A) → 엔트리로 이동
        toSlot.currentItem = dragged;
        dragged.transform.SetParent(toSlot.transform, false);
        dragged.rectTransform.anchoredPosition = Vector2.zero;
        dragged.isFromEntry = true;

        // EntrySlot 기준 originalParent 갱신
        dragged.UpdateOriginalParent(toSlot.transform);
        dragged.currentCell = null; // 보드와의 연결 끊기

        // 2) 엔트리에 있던 entryItem(B) → 보드로 이동
        entryItem.transform.SetParent(fromCell.transform, false);
        entryItem.rectTransform.anchoredPosition = Vector2.zero;
        entryItem.isFromEntry = false;

        CheckMarkOff (dragged);

        entryItem.currentCell = fromCell;
        fromCell.occupant = entryItem;
        fromCell.donutId = entryItem.donutData.id;

        entryItem.UpdateOriginalParent(fromCell.transform);

        // 3) 엔트리 currentItem 갱신
        toSlot.currentItem = dragged;

        toSlot.SaveToInventory();

        // 해당 도넛 데이터 연동
        DataManager.Instance.SetDonutAt(slotIndex, false, donutData: currentItem.donutData);
    }

    // 엔트리 <> 엔트리 스왑
    private void SwapItems(EntrySlot fromSlot, EntrySlot toSlot, MergeItemUI dragged)
    {
        // B 슬롯에 원래 있던 아이템
        MergeItemUI targetItem = toSlot.currentItem;

        // 1. A → B 이동
        toSlot.currentItem = dragged;
        dragged.transform.SetParent(toSlot.transform, false);
        dragged.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        // 원래 parent 갱신
        dragged.UpdateOriginalParent(toSlot.transform);
        dragged.isFromEntry = true;

        // 2. B → A 이동
        fromSlot.currentItem = targetItem;
        if (targetItem != null)
        {
            targetItem.transform.SetParent(fromSlot.transform, false);
            targetItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            targetItem.UpdateOriginalParent(fromSlot.transform);
            targetItem.isFromEntry = true;
        }

        // === 2. InventoryData 스왑 (두 칸 모두 저장해야 함) ===
        // toSlot(=dragged가 들어간 자리)
        if (toSlot.currentItem != null)
            toSlot.SaveToInventory();

        // fromSlot(=targetItem이 들어간 자리)
        if (fromSlot.currentItem != null)
            fromSlot.SaveToInventory();
        
        // 해당 도넛 데이터 연동
        DataManager.Instance.SetDonutAt(slotIndex, false, donutData: currentItem.donutData);

        Debug.Log($"[Swap] {fromSlot.name} ↔ {toSlot.name} 스왑 완료");
    }

    // 체크마크 비활성화 함수
    public void CheckMarkOff(MergeItemUI dragged)
    {
        var checkMark = dragged.transform.Find("CheckMark")?.gameObject;
        if (checkMark != null)
            checkMark.SetActive(false);
    }

    // 엔트리 저장
    public void SaveToInventory()
    {
        if (currentItem == null || currentItem.donutData == null)
        {
            // 슬롯이 비었으면 삭제 처리
            DataManager.Instance.InventoryData.donutEntries[slotIndex] = null;
            return;
        }

        var d = currentItem.donutData;

        // DonutEntry로 변환
        DonutEntry entry = new DonutEntry()
        {
            id = d.id,
            type = d.donutType,
            weight = d.weight,
            resilience = d.resilience,
            friction = d.friction
        };

        // 저장
        DataManager.Instance.InventoryData.donutEntries[slotIndex] = entry;

        //Debug.Log($"[Inventory Save] 슬롯 {slotIndex} → {entry.id} 저장완료");
    }

    //엔트리 도넛 불러오기
    public void LoadFromInventory()
    {
        BoardManager.Instance.ResetEntry();
        var inv = DataManager.Instance.InventoryData.donutEntries;

        if (slotIndex < 0 || slotIndex >= inv.Count)
            return;

        DonutEntry saved = inv[slotIndex];
        if (saved == null || string.IsNullOrEmpty(saved.id))
            return;

        // 1) ID → DonutData
        DonutData data = DataManager.Instance.GetDonutByID(saved.id);
        if (data == null)
        {
            Debug.LogError($"도넛 ID {saved.id} 로 Data 못찾음");
            return;
        }

        // 2) 프리팹 생성
        MergeItemUI newItem = Instantiate(itemPrefab, transform);

        // 3) DonutData 기반 이미지 설정
        newItem.Init(data);

        //// 4) Entry 전용 스탯 반영
        //newItem.ApplyEntryStats(saved); 
        // 보류 사용하는값인지 모르겠음

        // 5) UI 정렬
        RectTransform rt = newItem.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;

        // 6) 슬롯에 반영
        currentItem = newItem;
        newItem.isFromEntry = true;
        newItem.UpdateOriginalParent(transform);
    }

    // 슬롯을 비우는 함수
    public void Clear()
    {
        if (currentItem != null)
        {
            if (currentItem.transform.parent == transform)
                Destroy(currentItem.gameObject);

            currentItem = null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var highlight = BoardManager.Instance.selectionHighlight;
        if (highlight == null) return;

        highlight.gameObject.SetActive(true);
        highlight.transform.SetParent(transform, false);
        highlight.rectTransform.anchoredPosition = Vector2.zero;
    }
}
