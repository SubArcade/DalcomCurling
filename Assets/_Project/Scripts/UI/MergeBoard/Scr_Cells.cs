using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Cells : MonoBehaviour, IPointerClickHandler
{
    [Header("잠금 상태 표시용 UI")]
    public Image lockOverlay; // 이 이미지는 CellPrefab 안에 자식 오브젝트로 둔다

    public bool isActive { get; private set; }
    public MergeItemUI occupant { get; set; }
    public int gridX, gridY;
    public string donutID;

    public void Init(int x, int y)
    {
        gridX = x;
        gridY = y;
        SetActive(false);
    }

    public void SetActive(bool active)
    {
        isActive = active;

        // 활성화되지 않았으면 lockOverlay를 보이게
        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(!active);
    }
    public bool IsEmpty() => occupant == null;
    public void SetItem(MergeItemUI item)
    {
        occupant = item;
        donutID = item.donutID;
        if (item) item.BindToCell(this);
    }

    public void ClearItem()
    {
        occupant = null;
        donutID = null; // 연결해제
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isActive) return;
        BoardManager.Instance.OnCellClicked(this);
    }
}
