using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EntrySlot : MonoBehaviour, IDropHandler
{
    [Header("현재 슬롯에 들어있는 도넛")]
    public MergeItemUI currentItem;

    public int slotIndex = 0;
    
    private Image slotImage;

    void Awake()
    {
        slotImage = GetComponent<Image>();
    }

    // 슬롯이 비었는지 여부
    public bool IsEmpty => currentItem == null || currentItem.gameObject == null;

    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag?.GetComponent<MergeItemUI>();
        if (dragged == null) return;

        dragged.DetachFromCurrentCell();  // 보드에서온거 셀 해제
        dragged.isFromEntry = true;
      
        // 같은 슬롯이면 취소
        if (dragged.transform.parent == transform)
        {
            dragged.ResetPosition();
            return;
        }
     
        Debug.Log($"[OnDrop] {name} 드롭 시도 - 현재 currentItem: {currentItem?.name}");
     
        EntrySlot fromSlot = dragged.OriginalParent?.GetComponent<EntrySlot>();

        if (currentItem != null && fromSlot != null)
        {
            SwapItems(fromSlot, this, dragged);
            return;
        }

        MoveIn(dragged);
    }

    private void MoveIn(MergeItemUI dragged)
    {
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
        dragged.currentCell = null;  // 보드 참조 제거
        dragged.isFromEntry = true;  // 도넛 엔트리인지

        dragged.UpdateOriginalParent(transform);

        // ✅ 체크마크 비활성화
        var checkMark = dragged.transform.Find("CheckMark")?.gameObject;
        if (checkMark != null)
            checkMark.SetActive(false);

        // 해당 도넛 데이터 연동
        DataManager.Instance.SetDonutAt(slotIndex, false, donutData: currentItem.donutData);

        Debug.Log($"[MoveIn] {currentItem.donutData.id} 슬롯에 도넛 들어감");
        // Debug.Log($"[MoveIn] {name} 슬롯에 도넛 들어감");
        // Debug.Log($"currentItem: {currentItem?.name}");
        // Debug.Log($"IsEmpty: {IsEmpty}");
    }

    private void SwapItems(EntrySlot fromSlot, EntrySlot toSlot, MergeItemUI dragged)
    {
        MergeItemUI targetItem = toSlot.currentItem;  // 원래 있던 도넛

        // A → B 이동
        toSlot.currentItem = dragged;
        dragged.transform.SetParent(toSlot.transform, false);

        dragged.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        dragged.UpdateOriginalParent(toSlot.transform);

        // B → A 이동
        fromSlot.currentItem = targetItem;
        targetItem.transform.SetParent(fromSlot.transform, false);
        
        targetItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        targetItem.UpdateOriginalParent(fromSlot.transform);

        Debug.Log($"[Swap] {fromSlot.name} ↔ {toSlot.name} 스왑 완료");
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
}
