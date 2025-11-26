using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MergeItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string donutId;

    public DonutData donutData;

    public RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Image icon;
    public bool isFromEntry = false; //엔트리 확인용
    public bool isFromTemp = false;  //임시 보관칸 확인용

    private Vector2 originalPos;
    private Transform originalParent;

    public Transform OriginalParent => originalParent;

    public Cells currentCell { get; set; }
    public Cells originalCell { get; private set; } // 드래그 시작 시 셀 (EntrySlot용)

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        icon = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void BindToCell(Cells cell)
    {
        currentCell = cell;
        transform.SetParent(cell.transform, false);

        // 셀과 도넛ID 연결
        //cell.donutId = donutData != null ? donutData.id : null;
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
        UpdateOriginalParent(originalParent);

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

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        GameObject targetObj = eventData.pointerEnter;
        
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        EntrySlot entrySlot = null;
        Cells targetCell = null;

        foreach (var hit in results)
        {
            // EntrySlot 찾기
            if (entrySlot == null)
                entrySlot = hit.gameObject.GetComponentInParent<EntrySlot>();

            // Cells 찾기
            if (targetCell == null)
                targetCell = hit.gameObject.GetComponentInParent<Cells>();
        }

        // 휴지통
        if (targetObj != null && targetObj.CompareTag("TrashCan"))
        {
            if (donutData != null && donutData.donutType == DonutType.Gift)
            {
                ResetPosition();
                return;
            }

            Debug.Log($"{name} 휴지통으로 삭제됨");

            // 셀 참조 초기화
            if (currentCell != null)
                currentCell.ClearItem();

            // 엔트리 슬롯에서 온 경우 슬롯도 비워야 함
            var originSlot = originalParent.GetComponent<EntrySlot>();
            if (originSlot != null)
            {
                originSlot.currentItem = null; 
            }

            Destroy(gameObject); // 오브젝트 삭제
            return;
        }

        // 임시보관칸에 드래그 금지
        var tempSlot = targetObj?.GetComponentInParent<TempStorageSlot>();
        if (tempSlot != null)
        {
            ResetPosition();
            return;
        }

        // EntrySlot 우선 처리
        if (entrySlot != null)
        {
            // 엔트리 슬롯에도 격자 표시
            var highlight = BoardManager.Instance.selectionHighlight;
            if (highlight != null)
            {
                highlight.gameObject.SetActive(true);
                highlight.transform.SetParent(entrySlot.transform, false);
                highlight.rectTransform.anchoredPosition = Vector2.zero;
                highlight.rectTransform.localScale = Vector3.one;
            }

            entrySlot.OnDrop(eventData);
            return;
        }

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

    private void TryPlaceOrMerge(Cells targetCell)
    {
        EntrySlot fromEntrySlot = null;
        if (originalParent != null)
            fromEntrySlot = originalParent.GetComponent<EntrySlot>();

        // 같은 칸이면 리셋
        if (targetCell == currentCell)
        {
            ResetPosition();
            BoardManager.Instance.SelectCell(targetCell);
            return;
        }

        if (targetCell.IsEmpty())
        {
            // 엔트리에서 온 도넛이면 → 보드로 꺼내기
            if (fromEntrySlot != null || isFromEntry)
            {
                // 엔트리 슬롯 비우기
                if (fromEntrySlot != null && fromEntrySlot.currentItem == this)
                {
                    fromEntrySlot.currentItem = null;
                    fromEntrySlot.SaveToInventory();
                }

                // 이 도넛은 이제 보드 소속
                isFromEntry = false;

                MoveToCell(targetCell);
                BoardManager.Instance.SelectCell(targetCell);
                return;
            }

            // 보드 도넛이면 기존 이동
            MoveToCell(targetCell);
            BoardManager.Instance.SelectCell(targetCell);
            return;
        }

        if (isFromEntry)
        {
            // 엔트리 → 보드 스왑만 허용, 머지는 절대 안 됨
            var entrySlot = originalParent.GetComponent<EntrySlot>();
            var targetItem = targetCell.occupant;
            
            if (targetItem != null && targetItem.donutId.StartsWith("Gift"))
            {
                ResetPosition();
                return;
            }

            if (entrySlot != null && targetItem != null)
            {
                SwapEntryAndCell(entrySlot, targetCell, this, targetItem);
                BoardManager.Instance.AutoSaveBoardLocal();
            }
            else
            {
                ResetPosition();
            }

            return;
        }

        // 머지 대상 도넛 가져오기
        var otherItem = targetCell.occupant;
        if (otherItem == null)
        {
            ResetPosition();
            return;
        }

        // 스왑로직
        if (fromEntrySlot != null)   // 엔트리에서 옴
        {
            var targetItem = targetCell.occupant;

            // 머지 불가 조건
            bool canMerge = false;
            var myEntryData = donutData;
            var myBoardData = targetItem.donutData;

            if (myEntryData != null && myBoardData != null)
            {
                canMerge = myEntryData.donutType == myBoardData.donutType &&
                           myEntryData.level == myBoardData.level;
            }

            if (!canMerge)
            {
                SwapEntryAndCell(fromEntrySlot, targetCell, this, targetItem);
                BoardManager.Instance.AutoSaveBoardLocal();
                return;
            }
        }

        // 서로 같은 타입·레벨인지 확인
        var myData = DataManager.Instance.GetDonutByID(donutId);
        var otherData = DataManager.Instance.GetDonutByID(otherItem.donutId);

        if (myData == null || otherData == null)
        {
            int myLevel = ParseGiftLevel(donutId);
            int otherLevel = ParseGiftLevel(otherItem.donutId);

            if (myLevel > 0 && otherLevel > 0 && myLevel == otherLevel)
            {
                var nextGift = DataManager.Instance.GetGiftBoxData(myLevel + 1);
                if (nextGift == null)
                {
                    Debug.LogWarning($"[MERGE] 다음 단계 GiftBox 없음 ({donutId})");
                    ResetPosition();
                    return;
                }

                // 머지 성공 → 상위 GiftBox로 교체
                otherItem.GetComponent<Image>().sprite = nextGift.sprite;
                otherItem.donutId = nextGift.id;
                otherItem.donutData = null; // GiftBox는 DonutData 아님

                if (otherItem.currentCell != null)
                    otherItem.currentCell.donutId = nextGift.id;

                currentCell.ClearItem();
                Destroy(gameObject);

                Debug.Log($"[MERGE] GiftBox {donutId} → {nextGift.id} 머지 성공");
                BoardManager.Instance.AutoSaveBoardLocal();
                return;
            }


            Debug.LogWarning($"[MERGE] 도넛 데이터가 존재하지 않습니다. ({donutId})");
            ResetPosition();
            return;
        }

        // 같은 종류(type) + 같은 레벨(level)일 때만 머지 가능
        if (myData.donutType == otherData.donutType && myData.level == otherData.level)
        {
            // 다음 단계 도넛 찾기 (자동 계산)
            var nextDonut = DataManager.Instance.GetNextDonut(myData);

            if (nextDonut == null)
            {
                Debug.LogWarning($"[MERGE] 다음 단계 도넛 없음 ({donutId})");
                ResetPosition();
                return;
            }

            // 머지 성공 → 상위 스프라이트로 교체
            Sprite nextSprite = nextDonut.sprite;
            otherItem.GetComponent<Image>().sprite = nextSprite;
            otherItem.donutId = nextDonut.id;

            // 셀의 donutID도 갱신
            if (otherItem.currentCell != null)
            { 
                otherItem.currentCell.donutId = nextDonut.id;

                otherItem.donutData = nextDonut; // ✅ 머지된 도넛 데이터 저장
                otherItem.donutId = nextDonut.id; // ✅ 유지 가능
                otherItem.currentCell.donutId = nextDonut.id; // ✅ 셀에도 반영

            }

            // 현재 도넛 제거
            currentCell.ClearItem();
            Destroy(gameObject);

            Debug.Log($"[MERGE] {donutId} → {nextDonut.id} 머지 성공");

            BoardManager.Instance.AutoSaveBoardLocal(); //로컬 저장
            return;
        }

        // 머지안되면 스왑
        TrySwap(targetCell);
        return;
    }


    private void MoveToCell(Cells target)
    {
        // 엔트리에서 온거 비워주기
        var originSlot = originalParent.GetComponent<EntrySlot>();
        if (originSlot != null)
            originSlot.currentItem = null;

        isFromEntry = false; //엔트리 끄기

        currentCell?.ClearItem();
        target.SetItem(this, donutData); 
        transform.SetParent(target.transform, false);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;

        UpdateOriginalParent(target.transform);
        BoardManager.Instance.AutoSaveBoardLocal();
    }

    public void ResetPosition()
    {
        transform.SetParent(originalParent, false);
        rectTransform.anchoredPosition = originalPos;

        // 1) 엔트리슬롯이었다면 그냥 복구
        var slot = originalParent.GetComponent<EntrySlot>();
        if (slot != null)
        {
            slot.currentItem = this;
            isFromEntry = true;
            currentCell = null;
            return;
        }

        // 2) 원래 셀이 있었다면 셀 점유 복구
        if (originalCell != null)
        {
            currentCell = originalCell;
            originalCell.occupant = this;
            originalCell.donutId = donutData != null ? donutData.id : donutId;
        }

        BoardManager.Instance.SelectCell(originalCell);
    }

    //엔트리 슬롯 참조용
    public void UpdateOriginalParent(Transform newParent) 
    {
        originalParent = newParent;
    }

    // 보드에서 엔트리쪽으로 스왑
    private void SwapEntryAndCell(EntrySlot fromSlot, Cells targetCell, MergeItemUI entryItem, MergeItemUI boardItem)
    {
        // 1) Entry 슬롯에 보드 도넛 넣기
        fromSlot.currentItem = boardItem;
        boardItem.transform.SetParent(fromSlot.transform, false);
        boardItem.rectTransform.anchoredPosition = Vector2.zero;
        boardItem.isFromEntry = true;

        // EntrySlot originalParent 갱신
        boardItem.UpdateOriginalParent(fromSlot.transform);
        boardItem.currentCell = null;

        // 2) 보드 셀에 엔트리 도넛 넣기
        targetCell.SetItem(entryItem, entryItem.donutData);
        entryItem.transform.SetParent(targetCell.transform, false);
        entryItem.rectTransform.anchoredPosition = Vector2.zero;
        entryItem.isFromEntry = false;

        entryItem.UpdateOriginalParent(targetCell.transform);
        entryItem.currentCell = targetCell;

        // 3) 선택 하이라이트 갱신
        BoardManager.Instance.SelectCell(targetCell);

        fromSlot.CheckMarkOff(boardItem); // 체크박스 끄기

        Debug.Log($"[SwapEntryAndCell] 엔트리 ↔ 보드 스왑 완료");
    }

    // 보드 <> 보드 스왑
    private void TrySwap(Cells targetCell)
    {
        MergeItemUI other = targetCell.occupant;

        if (other == null)
        {
            ResetPosition();
            return;
        }

        Cells cellA = currentCell;
        Cells cellB = targetCell;

        // 서로 아이템 교환
        cellA.occupant = other;
        cellB.occupant = this;

        // currentCell 변경
        other.currentCell = cellA;
        this.currentCell = cellB;

        // donutId 변경
        cellA.donutId = other.donutId;
        cellB.donutId = this.donutId;

        // 부모 변경
        other.transform.SetParent(cellA.transform, false);
        this.transform.SetParent(cellB.transform, false);

        // 위치 중앙으로
        other.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        this.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        BoardManager.Instance.AutoSaveBoardLocal();
        Debug.Log($"[SWAP] {cellA.gridX},{cellA.gridY} <-> {cellB.gridX},{cellB.gridY}");
    }


    public void Init(DonutData data)
    {
        donutData = data;
        donutId = data.id; // 도넛아이디 넣기

        if (icon != null) icon.sprite = data.sprite;
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
   public int ParseGiftLevel(string id) //유틸 함수
    {
        if (string.IsNullOrEmpty(id)) return -1;
        var parts = id.Split('_');
        if (parts.Length != 2) return -1;
        if (parts[0] != "Gift") return -1;
        if (int.TryParse(parts[1], out int level)) return level;
        return -1;
    }

}
