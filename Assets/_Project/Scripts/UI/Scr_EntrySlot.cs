using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EntrySlot : MonoBehaviour, IDropHandler
{
    public Image slotImage;
    public MergeItemUI currentItem;

    void Awake()
    {
        if (slotImage == null)
            slotImage = GetComponent<Image>();
    }

    public bool IsEmpty
    {
        get
        {
            if (currentItem == null) return true;
            if (currentItem.gameObject == null) return true;
            return false;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag?.GetComponent<MergeItemUI>();
        if (dragged == null) return;

        // 드래그 시작 슬롯
        var originSlot = dragged.OriginalParent != null
            ? dragged.OriginalParent.GetComponent<EntrySlot>()
            : null;

        // 같은 슬롯으로 드롭한 경우 → 무시
        if (originSlot == this)
        {
            Debug.Log("같은 슬롯에 드롭 → 무시");
            dragged.ResetPosition();
            return;
        }

        // 이미 도넛이 있으면 리셋
        if (!IsEmpty)
        {
            Debug.Log("이 슬롯은 이미 도넛이 들어있어요!");
            dragged.ResetPosition();
            return;
        }

        // 정상 이동
        MoveIn(dragged);

        // 원래 슬롯 비우기
        if (originSlot != null)
            originSlot.currentItem = null;
    }

    private void MoveIn(MergeItemUI dragged)
    {
        dragged.transform.SetParent(transform, false);
        dragged.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        currentItem = dragged;
        Debug.Log("도넛이 엔트리 슬롯으로 이동됨");
    }
}
