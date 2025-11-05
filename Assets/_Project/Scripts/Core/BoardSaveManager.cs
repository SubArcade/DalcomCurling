using System.Collections.Generic;
using System.IO;
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
    private static string SavePath => Path.Combine(Application.persistentDataPath, "board.json");

    // 저장
    public static void Save(BoardManager board)
    {
        if (board == null)
        {
            Debug.LogWarning("BoardSaveManager.Save() : BoardManager가 null입니다.");
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

            // 도넛이 있는 셀만 ID 저장
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

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"보드 저장 완료 ({SavePath})");
    }

    // 불러오기
    public static void Load(BoardManager board)
    {
        if (board == null)
        {
            Debug.LogWarning("BoardSaveManager.Load() : BoardManager가 null입니다.");
            return;
        }

        if (!File.Exists(SavePath))
        {
            Debug.Log("저장된 보드 파일이 없습니다.");
            return;
        }

        string json = File.ReadAllText(SavePath);
        BoardData data = JsonUtility.FromJson<BoardData>(json);

        foreach (CellData cd in data.cells)
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
                    Debug.LogWarning($"Sprite ID '{cd.itemID}'을(를) 찾을 수 없습니다.");
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

        Debug.Log("보드 불러오기 완료");
    }

    public static void DeleteSave() 
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("보드 저장파일 삭제 완료");
        }
    }
}
