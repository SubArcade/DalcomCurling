using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Scr_DonutSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image donutImage;        // Slot 안의 DonutImage
    [SerializeField] private GameObject outlineImage; // 꼭지점 테두리 이미지 (GameObject)

    private int slotIndex;                      // 1~5
    private Scr_DonutStateEditor owner;

    // 드래그용
    private Canvas canvas;
    private RectTransform dragImageRT;
    private Image dragImage;

    public void Init(Scr_DonutStateEditor owner, int index)
    {
        this.owner = owner;
        this.slotIndex = index;

        // 캔버스 찾기
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindObjectOfType<Canvas>();

        // 드래그 팔로우용 이미지 오브젝트 생성 (각 슬롯마다 하나씩)
        GameObject dragObj = new GameObject($"DragImage_Slot{slotIndex}");
        dragObj.transform.SetParent(canvas.transform, false);
        dragObj.transform.SetAsLastSibling();

        dragImage = dragObj.AddComponent<Image>();
        dragImage.raycastTarget = false; // 레이캐스트 막지 않도록
        dragImage.enabled = false;

        dragImageRT = dragObj.GetComponent<RectTransform>();
        dragImageRT.sizeDelta = donutImage.rectTransform.sizeDelta;
    }

    public void SetSprite(Sprite sprite)
    {
        donutImage.sprite = sprite;
        donutImage.enabled = (sprite != null);
    }

    public void SetSelected(bool selected)
    {
        if (outlineImage != null)
            outlineImage.SetActive(selected);
    }

    // 클릭 → 단순 선택만
    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log($"OnPointerClick : {slotIndex}");
        if (owner != null)
            owner.OnClickSlot(slotIndex);
    }

    // ============================
    // 드래그 시작
    // ============================
    /*public void OnBeginDrag(PointerEventData eventData)
    {
        if (donutImage.sprite == null || owner == null)
            return;

        dragImage.sprite = donutImage.sprite;
        dragImage.enabled = true;

        // 드래그 시작 슬롯 전달
        owner.BeginDrag(slotIndex);

        // 시작 위치 바로 갱신
        UpdateDragImagePosition(eventData);
    }

    // ============================
    // 드래그 중 : 이미지 따라다님
    // ============================
    public void OnDrag(PointerEventData eventData)
    {
        if (!dragImage.enabled) return;
        UpdateDragImagePosition(eventData);
    }

    private void UpdateDragImagePosition(PointerEventData eventData)
    {
        if (canvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPos);

        dragImageRT.localPosition = localPos;
    }

    // ============================
    // 드래그 끝 : Slot 위에 드롭했는지 체크
    // ============================
    public void OnEndDrag(PointerEventData eventData)
    {
        if (owner == null)
            return;

        dragImage.enabled = false;  // 드래그 이미지 숨김

        GameObject dropObj = eventData.pointerEnter;
        if (dropObj == null)
        {
            owner.CancelDrag();
            return;
        }

        // 드롭 대상에서 Scr_DonutSlot 찾기 (자식/부모 포함)
        Scr_DonutSlot dropSlot = dropObj.GetComponentInParent<Scr_DonutSlot>();
        if (dropSlot == null)
        {
            owner.CancelDrag();
            return;
        }

        owner.EndDrag(dropSlot.slotIndex);
    }*/
}
