using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Threading.Tasks;

public class MergeItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string donutID;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Image image;

    private Vector2 originalPos;
    private Transform originalParent;

    public Transform OriginalParent => originalParent;

    public Cells currentCell { get; private set; }
    public Cells originalCell { get; private set; } // 드래그 시작 시 셀 (EntrySlot용)

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
        
        // 셀과 도넛ID 연결
        cell.donutID = donutID;
        cell.occupant = this;
    }

    public void DetachFromCurrentCell()
    {
        if (currentCell != null)
        {
            currentCell.ClearItem();  // Cells 측에서 occupant = null 해주는 함수여야 함
            currentCell = null;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPos = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        originalCell = currentCell;

        // EntrySlot이라면 currentItem 비워두기
        var originSlot = originalParent.GetComponent<EntrySlot>();
        if (originSlot != null)
            originSlot.currentItem = null;

        transform.SetParent(canvas.transform, true);
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

    public async void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        GameObject targetObj = eventData.pointerEnter;

        if (targetObj != null && targetObj.CompareTag("TrashCan"))
        {
            Debug.Log($"{name} 휴지통으로 삭제됨");

            // 셀 참조 초기화
            if (currentCell != null)
                currentCell.ClearItem();

            Destroy(gameObject); // 오브젝트 삭제
            await AutoSaveAsync();
            return;
        }

        // EntrySlot 우선 체크
        var entrySlot = targetObj ? targetObj.GetComponentInParent<EntrySlot>() : null;
        if (entrySlot != null)
        {
            entrySlot.OnDrop(eventData);
            return;
        }

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

        if (targetCell == BoardManager.Instance.GeneratorCell)
        {
            // 생성기 칸 도넛 차단
            ResetPosition();
            return;
        }

        if (targetCell == null || !targetCell.isActive)
        {
            ResetPosition();
            return;
        }

        TryPlaceOrMerge(targetCell);

        BoardManager.Instance.SelectCell(targetCell);

        BoardManager.Instance.selectedCell = null;
    }
        
    private async void TryPlaceOrMerge(Cells targetCell)
    {
        if (targetCell == currentCell)
        {
            ResetPosition();
            BoardManager.Instance.SelectCell(targetCell);
            return;
        }

        if (targetCell.IsEmpty())
        {
            MoveToCell(targetCell);
            BoardManager.Instance.SelectCell(targetCell);
            await AutoSaveAsync();
            return;
        }

        // 엔트리 슬롯에는 머지 금지
        if (targetCell.CompareTag("EntrySlot"))
        {
            ResetPosition();
            return;
        }


        var otherItem = targetCell.occupant;
        // ID 같을 때만 머지 가능
        if (otherItem.donutID == donutID)
        {
            string nextID = DonutDatabase.GetNextID(donutID);
            if (string.IsNullOrEmpty(nextID))
            {
                Debug.LogWarning($"[MERGE] 다음 ID 없음 ({donutID})");
                ResetPosition();
                return;
            }

            // 머지 성공
            Sprite nextSprite = DonutDatabase.GetSpriteByID(nextID);
            otherItem.GetComponent<Image>().sprite = nextSprite;
            otherItem.donutID = nextID;

            // 현재 도넛 삭제
            currentCell.ClearItem();
            Destroy(gameObject);

            Debug.Log($"[MERGE] {donutID} → {nextID} 머지 성공");

            await AutoSaveAsync(); // 머지 후 저장
            return;
        }

        ResetPosition();
    }

    private void MoveToCell(Cells target)
    {
        currentCell?.ClearItem();
        target.SetItem(this);
        transform.SetParent(target.transform, false);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }

    public void ResetPosition()
    {
        transform.SetParent(originalParent, false);
        rectTransform.anchoredPosition = originalPos;

        if (originalCell != null)
            BoardManager.Instance.SelectCell(originalCell); //격자 원위치
    }

    // Firestore 자동 저장
    private async Task AutoSaveAsync()
    {
        try
        {
            string userId = FirebaseAuthManager.Instance.UserId;
            await BoardSaveManager.SaveToFirestore(BoardManager.Instance, userId);
            Debug.Log("[FS] 자동 저장 완료 (머지/이동/삭제)");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FS] 자동 저장 실패: {e.Message}");
        }
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
