using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cells : MonoBehaviour
{
    [Header("잠금 상태 표시용 UI")]
    public Image lockOverlay; // 이 이미지는 CellPrefab 안에 자식 오브젝트로 둔다

    public bool isActive { get; private set; }
    public MergeItemUI occupant { get; private set; }

    public void Init(int x, int y)
    {
        // 처음엔 전부 잠긴 상태로 시작
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
        if (item) item.BindToCell(this);
    }

    public void ClearItem() => occupant = null;
}
