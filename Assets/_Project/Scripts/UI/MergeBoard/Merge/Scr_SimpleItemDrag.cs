using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SimpleItemDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private GameObject dragObject;
    private Image originalImage;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        originalImage = GetComponent<Image>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;

        // 드래그 이미지 생성
        dragObject = new GameObject("DragIcon");
        dragObject.transform.SetParent(canvas.transform);
        dragObject.transform.SetAsLastSibling();

        Image dragImage = dragObject.AddComponent<Image>();
        dragImage.sprite = originalImage.sprite;
        dragImage.color = originalImage.color;
        dragImage.raycastTarget = false;

        RectTransform dragRect = dragObject.GetComponent<RectTransform>();
        dragRect.anchorMin = new Vector2(0.5f, 0.5f);
        dragRect.anchorMax = new Vector2(0.5f, 0.5f);
        dragRect.pivot = new Vector2(0.5f, 0.5f);
        dragRect.sizeDelta = rectTransform.sizeDelta;

        CanvasGroup dragCanvasGroup = dragObject.AddComponent<CanvasGroup>();
        dragCanvasGroup.alpha = 0.8f;

        UpdateDragPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragObject != null)
        {
            UpdateDragPosition(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // 드래그 이미지 제거 먼저
        if (dragObject != null)
        {
            Destroy(dragObject);
            dragObject = null;
        }

        GameObject targetSlot = FindSlotUnderCursor();

        if (targetSlot != null && targetSlot != originalParent.gameObject)
        {
            SimpleItemDrag targetDrag = targetSlot.GetComponentInChildren<SimpleItemDrag>();
            bool hasValidItem = targetDrag != null && targetDrag != this;

            if (hasValidItem)
            {
                // 같은 스프라이트인지 확인
                if (originalImage.sprite == targetDrag.originalImage.sprite)
                {
                    // 합성 가능한지 레시피 매니저에 확인
                    if (RecipeManager.Instance.CanMerge(originalImage.sprite, targetDrag.originalImage.sprite))
                    {
                        // 합성 실행
                        MergeItems(targetSlot, targetDrag);
                    }
                    else
                    {
                        // 합성 불가 - 교환
                        SwapItems(targetSlot, targetDrag);
                    }
                }
                else
                {
                    // 다른 스프라이트 - 교환
                    SwapItems(targetSlot, targetDrag);
                }
            }
            else
            {
                // 빈 슬롯 - 이동
                MoveToEmptySlot(targetSlot);
            }
        }
        else
        {
            ReturnToOriginalSlot();
        }

        Debug.Log("드래그 끝");
    }

    // 드래그 위치 업데이트 메서드
    private void UpdateDragPosition(PointerEventData eventData)
    {
        if (dragObject == null) return;

        RectTransform dragRect = dragObject.GetComponent<RectTransform>();
        Vector3 worldPoint;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            eventData.position,
            canvas.worldCamera,
            out worldPoint
        );
        dragRect.position = worldPoint;
    }

    // 슬롯 찾기 메서드
    private GameObject FindSlotUnderCursor()
    {
        Vector2 mousePosition = Input.mousePosition;
        Transform slotContainer = originalParent.parent;

        for (int i = 0; i < slotContainer.childCount; i++)
        {
            GameObject slot = slotContainer.GetChild(i).gameObject;
            RectTransform slotRect = slot.GetComponent<RectTransform>();

            if (IsMouseOverSlot(slotRect, mousePosition))
            {
                return slot;
            }
        }
        return null;
    }

    private bool IsMouseOverSlot(RectTransform slotRect, Vector2 mousePosition)
    {
        Vector2 localMousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            slotRect, mousePosition, null, out localMousePosition);

        return slotRect.rect.Contains(localMousePosition);
    }

    // 합성 메서드
    private void MergeItems(GameObject targetSlot, SimpleItemDrag targetDrag)
    {
        // 결과물 스프라이트 가져오기
        Sprite resultSprite = RecipeManager.Instance.GetMergeResult(originalImage.sprite, targetDrag.originalImage.sprite);

        if (resultSprite != null)
        {
            // 대상 슬롯에 결과물 설정
            targetDrag.originalImage.sprite = resultSprite;

            // 원본 슬롯 비우기
            originalImage.enabled = false;
            canvasGroup.alpha = 0f;

            Debug.Log($"합성 성공: {originalImage.sprite.name} + {originalImage.sprite.name} = {resultSprite.name}");
        }
    }

    private void SwapItems(GameObject targetSlot, SimpleItemDrag targetDrag)
    {
        Transform targetItem = targetDrag.transform;

        // 부모 교환
        transform.SetParent(targetSlot.transform);
        rectTransform.anchoredPosition = Vector2.zero;

        targetItem.SetParent(originalParent);
        targetItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        Debug.Log($"아이템 교환 성공!");
    }

    private void MoveToEmptySlot(GameObject targetSlot)
    {
        transform.SetParent(targetSlot.transform);
        rectTransform.anchoredPosition = Vector2.zero;
        Debug.Log("빈 슬롯으로 이동");
    }

    private void ReturnToOriginalSlot()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = Vector2.zero;
        Debug.Log("원래 위치로 복귀");
    }
}