using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject cellPrefab; // 칸 프리팹  
    public int boardSize = 7;
    private Cells[,] cells;

    void Start()
    {
        GenerateBoard();
        UpdateBoardUnlock(1);
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
}
