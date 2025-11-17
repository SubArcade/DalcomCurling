using System.Collections.Generic;
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

        // 드래그된 오브젝트가 이미 내 자식이면 무시
        if (dragged.transform.parent == transform)
        {
            Debug.Log($"{name}: 이미 같은 슬롯에 있습니다. 드롭 무시");
            return;
        }

        Debug.Log($"[OnDrop] {name} 드롭 시도 - 현재 currentItem: {currentItem?.name}");

        // 이미 도넛이 있으면 거부
        if (!IsEmpty)
        {
            Debug.Log("이 슬롯은 이미 도넛이 들어있어요!");
            dragged.ResetPosition();
            return;
        }

        // 혹시 이전 슬롯이 자신이었다면 무시 (이중 드롭 방지)
        if (dragged.OriginalParent == transform)
        {
            Debug.Log($"{name}: 이전 슬롯 == 현재 슬롯, 중복 드롭 방지");
            return;
        }


        // 보드에서 왔다면 보드 점유 해제
        dragged.DetachFromCurrentCell();

        // 슬롯에 배치
        MoveIn(dragged);
    }

    private void MoveIn(MergeItemUI dragged)
    {
        // 기존 슬롯의 currentItem 초기화
        var oldSlot = dragged.OriginalParent?.GetComponent<EntrySlot>();
        if (oldSlot != null && oldSlot != this)
            oldSlot.Clear();

        dragged.transform.SetParent(transform, false);
        var rt = dragged.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        currentItem = dragged;

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

    // 슬롯을 비우는 함수
    public void Clear()
    {
        if (currentItem != null)
            currentItem = null;
    }
}
