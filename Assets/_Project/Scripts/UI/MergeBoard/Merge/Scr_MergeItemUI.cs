using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MergeItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPos;
    private Transform originalParent;
    private Image image;

    public Cells currentCell { get; private set; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        image = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void BindToCell(Cells cell)
    {
        currentCell = cell;
        transform.SetParent(cell.transform, false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPos = rectTransform.anchoredPosition;
        originalParent = transform.parent;

        transform.SetParent(canvas.transform, true); // 최상단으로
        canvasGroup.blocksRaycasts = false; // 드래그 중 자기 자신에 대한 클릭 방지
        canvasGroup.alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 pos);
        rectTransform.anchoredPosition = pos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // Pointer가 올라간 대상 Cell 찾기
        GameObject targetObj = eventData.pointerEnter;
        Cells targetCell = targetObj ? targetObj.GetComponent<Cells>() : null;

        if (targetCell != null && targetCell.isActive)
        {
            TryPlaceOrMerge(targetCell);
        }
        else
        {
            // 실패 → 원위치
            rectTransform.anchoredPosition = originalPos;
            transform.SetParent(originalParent, false);
        }
    }

    private void TryPlaceOrMerge(Cells target)
    {
        if (target == currentCell) return;

        if (target.IsEmpty())
        {
            MoveToCell(target);
            return;
        }

        var other = target.occupant;
        if (other != null)
        {
            Sprite result = RecipeManager.Instance.GetMergeResult(image.sprite, other.GetComponent<Image>().sprite);
            if (result != null)
            {
                other.GetComponent<Image>().sprite = result;
                currentCell.ClearItem();
                Destroy(gameObject);
                return;
            }
        }

        rectTransform.anchoredPosition = originalPos;
        transform.SetParent(originalParent, false);
    }

    private void MoveToCell(Cells target)
    {
        currentCell?.ClearItem();
        target.SetItem(this);
        transform.SetParent(target.transform, false);
        rectTransform.anchoredPosition = Vector2.zero;
    }
}
