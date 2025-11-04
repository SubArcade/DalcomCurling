using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MergeItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Image image;

    private Vector2 originalPos;
    private Transform originalParent;

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

        transform.SetParent(canvas.transform, true); // 최상단으로 올리기
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pos);

        rectTransform.anchoredPosition = pos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        GameObject targetObj = eventData.pointerEnter;
        Cells targetCell = targetObj ? targetObj.GetComponentInParent<Cells>() : null;

        // 드롭 위치에 격자 이동
        if (BoardManager.Instance.selectionHighlight != null)
        {
            var highlight = BoardManager.Instance.selectionHighlight;

            if (targetCell != null && targetCell.isActive)
            {
                // 격자를 타겟 셀로 이동
                highlight.gameObject.SetActive(true);
                highlight.transform.SetParent(targetCell.transform, false);
                highlight.rectTransform.anchoredPosition = Vector2.zero;
            }
            else
            {
                // 잘못된 위치면 격자 숨김
                highlight.gameObject.SetActive(false);
            }
        }

        if (targetCell == null || !targetCell.isActive)
        {
            ResetPosition();
            return;
        }

        TryPlaceOrMerge(targetCell);
        BoardManager.Instance.selectedCell = null;
    }

    private void TryPlaceOrMerge(Cells targetCell)
    {
        if (targetCell == currentCell)
        {
            ResetPosition();
            return;
        }

        if (targetCell.IsEmpty())
        {
            MoveToCell(targetCell);
            return;
        }

        var otherItem = targetCell.occupant;
        if (otherItem != null)
        {
            Sprite mySprite = image.sprite;
            Sprite otherSprite = otherItem.GetComponent<Image>().sprite;
            Sprite result = RecipeManager.Instance.GetMergeResult(mySprite, otherSprite);

            if (result != null)
            {
                otherItem.GetComponent<Image>().sprite = result;
                currentCell.ClearItem();
                Destroy(gameObject);
                return;
            }
        }

        ResetPosition();
    }

    private void MoveToCell(Cells target)
    {
        currentCell?.ClearItem();
        target.SetItem(this);
        transform.SetParent(target.transform, false);
        rectTransform.anchoredPosition = Vector2.zero;
    }

    private void ResetPosition()
    {
        transform.SetParent(originalParent, false);
        rectTransform.anchoredPosition = originalPos;
    }

    // 나중에 이펙트 추가할거
    /*
    private IEnumerator MergeEffect(Vector3 pos)
    {
        // 간단한 이펙트 예시 (빛나는 이미지 생성)
        GameObject flash = new GameObject("MergeFX", typeof(Image));
        flash.transform.SetParent(canvas.transform, false);
        var img = flash.GetComponent<Image>();
        img.color = new Color(1,1,1,0.8f);
        flash.GetComponent<RectTransform>().anchoredPosition = pos;
        flash.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 150);
        Destroy(flash, 0.3f);
        yield return null;
    }
    */
}
