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
}
