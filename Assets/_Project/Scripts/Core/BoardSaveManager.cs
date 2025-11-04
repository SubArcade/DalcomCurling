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
    public string spriteName; // 도넛이 있으면 sprite 이름, 없으면 ""
}

[System.Serializable]
public class BoardData
{
    public List<CellData> cells = new List<CellData>();
}

public static class BoardSaveManager
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "board.json");

    // 🔹 저장
    public static void Save(BoardManager board)
    {
        if (board == null)
        {
            Debug.LogWarning("BoardSaveManager.Save() : BoardManager가 null입니다.");
            return;
        }

        BoardData data = new BoardData();

        // 모든 셀 데이터 저장
        foreach (Cells cell in board.GetAllCells())
        {
            CellData cd = new CellData
            {
                x = cell.gridX,
                y = cell.gridY,
                isActive = cell.isActive,
                spriteName = ""
            };

            if (cell.occupant != null)
            {
                Image img = cell.occupant.GetComponent<Image>();
                if (img != null && img.sprite != null)
                    cd.spriteName = img.sprite.name;
            }

            data.cells.Add(cd);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"보드 저장 완료 ({SavePath})");
    }

    // 🔹 불러오기
    public static void Load(BoardManager board)
    {
        if (board == null)
        {
            Debug.LogWarning("BoardSaveManager.Load() : BoardManager가 null입니다.");
            return;
        }

        if (!File.Exists(SavePath))
        {
            Debug.Log("저장된 보드 파일이 없습니다. 새로 시작합니다.");
            return;
        }

        string json = File.ReadAllText(SavePath);
        BoardData data = JsonUtility.FromJson<BoardData>(json);

        foreach (CellData cd in data.cells)
        {
            Cells cell = board.GetCell(cd.x, cd.y);
            if (cell == null) continue;

            cell.SetActive(cd.isActive);

            // 기존 도넛 제거
            cell.ClearItem();

            // 스프라이트 이름이 있으면 재생성
            if (!string.IsNullOrEmpty(cd.spriteName))
            {
                Sprite s = LoadSpriteByName(cd.spriteName);
                if (s != null)
                {
                    GameObject obj = Object.Instantiate(board.donutPrefab, cell.transform);
                    RectTransform rt = obj.GetComponent<RectTransform>();
                    rt.anchoredPosition = Vector2.zero;
                    rt.localScale = Vector3.one;

                    Image img = obj.GetComponent<Image>();
                    img.sprite = s;

                    MergeItemUI item = obj.GetComponent<MergeItemUI>();
                    cell.SetItem(item);
                }
            }
        }

        Debug.Log("보드 불러오기 완료");
    }

    // Sprite 이름으로 로드
    private static Sprite LoadSpriteByName(string name)
    {
        // Resources/Sprites/Donuts/ 폴더 안의 파일명과 같아야 함
        Sprite s = Resources.Load<Sprite>("Sprites/Donuts/" + name);
        if (s == null)
            Debug.LogWarning($"Sprite를 찾을 수 없음: {name}");
        return s;
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

