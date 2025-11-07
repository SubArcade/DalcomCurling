using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CellData
{
    public int x;
    public int y;
    public bool isActive;
    public string itemID; // Sprite 대신 고유 ID 사용
}

[System.Serializable]
public class BoardData
{
    public List<CellData> cells = new List<CellData>();
}

public static class BoardSaveManager
{
    // 메모리에 저장된 보드 데이터 (Firestore나 파일 대신)
    private static BoardData currentBoardData;

    // 저장 (현재 보드 상태 → BoardData)
    public static void SaveToMemory(BoardManager board)
    {
        if (board == null)
        {
            Debug.LogWarning("BoardSaveManager.SaveToMemory() : board가 null");
            return;
        }

        BoardData data = new BoardData();

        foreach (Cells cell in board.GetAllCells())
        {
            CellData cd = new CellData
            {
                x = cell.gridX,
                y = cell.gridY,
                isActive = cell.isActive,
                itemID = ""
            };

            if (cell.occupant != null)
            {
                Image img = cell.occupant.GetComponent<Image>();
                if (img != null && img.sprite != null)
                {
                    cd.itemID = DonutDatabase.GetIDBySprite(img.sprite);
                }
            }

            data.cells.Add(cd);
        }

        currentBoardData = data; // 메모리에 저장
        Debug.Log($"[MEMORY] 보드 저장 완료 (칸 수: {data.cells.Count})");
    }

    // 불러오기 (BoardData → 실제 보드에 적용)
    public static void LoadFromMemory(BoardManager board)
    {
        if (board == null)
        {
            Debug.LogWarning("BoardSaveManager.LoadFromMemory() : board가 null");
            return;
        }

        if (currentBoardData == null || currentBoardData.cells.Count == 0)
        {
            Debug.LogWarning("[MEMORY] 저장된 보드 데이터가 없습니다.");
            return;
        }

        foreach (CellData cd in currentBoardData.cells)
        {
            Cells cell = board.GetCell(cd.x, cd.y);
            if (cell == null) continue;

            cell.SetActive(cd.isActive);
            cell.ClearItem();

            if (!string.IsNullOrEmpty(cd.itemID))
            {
                Sprite sprite = DonutDatabase.GetSpriteByID(cd.itemID);
                if (sprite == null)
                {
                    Debug.LogWarning($"Sprite ID '{cd.itemID}'를 찾을 수 없습니다.");
                    continue;
                }

                GameObject obj = Object.Instantiate(board.donutPrefab, cell.transform);
                RectTransform rt = obj.GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;

                Image img = obj.GetComponent<Image>();
                img.sprite = sprite;

                MergeItemUI item = obj.GetComponent<MergeItemUI>();
                cell.SetItem(item);
            }
        }
        Debug.Log($"[MEMORY] 보드 불러오기 완료 (칸 수: {currentBoardData.cells.Count})");
    }

    // 메모리 데이터 비우기
    public static void ClearMemory()
    {
        currentBoardData = null;
        Debug.Log("[MEMORY] 보드 데이터 초기화 완료");
    }

    // 현재 보드 데이터 접근용 (예: UI나 Debug용)
    public static BoardData GetCurrentData() => currentBoardData;
}
