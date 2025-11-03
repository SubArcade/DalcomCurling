using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public GameObject cellPrefab; // 칸 프리팹  
    public GameObject donutPrefab; // 
    public int boardSize = 7;
    private Cells[,] cells;

    [Header("테스트 아이템 스프라이트")]
    public Sprite startSpriteA;
    public Sprite startSpriteB;

    void Start()
    {
        GenerateBoard();
        UpdateBoardUnlock(1);

        SpawnItemAt(3, 3, startSpriteA);
        SpawnItemAt(3, 4, startSpriteA);
        SpawnItemAt(4, 4, startSpriteB);
    }

    void GenerateBoard()
    {
        cells = new Cells[boardSize, boardSize];  // 7x7칸 저장할 배열
        float offset = (boardSize - 1) / 2f;      // 중앙 맞추기용 위치 조정

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                GameObject obj = Instantiate(cellPrefab, transform);
                obj.transform.localPosition = new Vector3(x - offset, 0, y - offset);

                cells[x, y] = obj.GetComponent<Cells>();
                cells[x, y].Init(x, y);
            }
        }
    }

    // 레벨 활성영역 업데이트
    public void UpdateBoardUnlock(int level)
    {
        int activeSize = GetActiveSize(level);
        int center = boardSize / 2;

        int start = center - (activeSize - 1) / 2;
        int end = start + activeSize;

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                bool isActive = (x >= start && x < end) && (y >= start && y < end);
                cells[x, y].SetActive(isActive);
            }
        }
    }

    // 각 레벨별 보드 크기 반환
    private int GetActiveSize(int level)
    {
        if (level < 3) return 3;
        if (level < 5) return 4;
        if (level < 7) return 5;
        if (level < 10) return 6;
        return 7;
    }

    public void SpawnItemAt(int x, int y, Sprite sprite)
    {
        var cell = GetCell(x, y);
        if (cell == null || !cell.isActive || !cell.IsEmpty()) return;

        GameObject itemObj = Instantiate(donutPrefab, cell.transform);
        RectTransform rt = itemObj.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;

        var item = itemObj.GetComponent<MergeItemUI>();
        item.GetComponent<Image>().sprite = sprite;
        cell.SetItem(item);
    }

    public Cells GetCell(int x, int y)
    {
        if (x < 0 || y < 0 || x >= boardSize || y >= boardSize) return null;
        return cells[x, y];
    }
}
