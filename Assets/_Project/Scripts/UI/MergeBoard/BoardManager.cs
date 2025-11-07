using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [Header("보드 설정")]
    [SerializeField] private RectTransform boardParent;
    [SerializeField] private GameObject cellPrefab; // 칸 프리팹 
    public GameObject donutPrefab; // 도넛 프리팹
    [SerializeField] private GameObject donutSpawnerPrefab; //도넛 생성기 버튼
    [SerializeField] private int boardSize = 7;

    [Header("UI 요소")]
    public Image selectionHighlight;  // 선택격자 이미지
    [SerializeField] private GameObject infoPopup;      // 정보 팝업 패널
    [SerializeField] private TMP_Text infoText;             // 팝업 내부 텍스트

    private Cells[,] cells;
    private Cells generatorCell; // 도넛 생성기가 위치한 셀
    public Cells GeneratorCell => generatorCell;

    public Cells selectedCell; // 선택한 셀

    [Header("도넛 스프라이트")]
    public Sprite donutSprite;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        selectionHighlight = GameObject.Find("Canvas/Main_Panel/Mid/Merge/Background/SelectCursor_Image").GetComponent<Image>();
        infoPopup = GameObject.Find("Canvas/Main_Panel/Bottom/Description");
        infoText = GameObject.Find("Canvas/Main_Panel/Bottom/Description/Text").GetComponent<TMP_Text>();

        GenerateBoard();
        UpdateBoardUnlock(1);
        CreateDonutButtonAtCenter();

        if (selectionHighlight != null) selectionHighlight.gameObject.SetActive(false);
        if (infoPopup != null) infoPopup.SetActive(false);

        BoardSaveManager.LoadFromMemory(this);
    }

    //void OnApplicationQuit()
    //{
    //    BoardSaveManager.Save(this);
    //}

    public void OnCellClicked(Cells cell)
    {
        bool isSameCell = (selectedCell == cell);
        selectedCell = cell;

        // 격자 표시
        if (selectionHighlight != null)
        {
            selectionHighlight.gameObject.SetActive(true);
            selectionHighlight.transform.SetParent(cell.transform, false);
            selectionHighlight.rectTransform.anchoredPosition = Vector2.zero;
        }

        // 셀 상태별 동작
        if (cell == generatorCell)
        {
            // 도넛 생성기
            infoPopup.SetActive(true);
            infoText.text = "도넛 생성기에요!\n다시 터치 시 도넛을 생성합니다.";

            if (isSameCell)
            {
                SpawnDonutToEmptyCell();
                BoardSaveManager.SaveToMemory(this); // 저장
            }
        }
        else if (cell.occupant != null)
        {
            // 도넛 있는 셀
            infoPopup.SetActive(true);
            infoText.text = $"도넛: {cell.occupant.GetComponent<Image>().sprite.name}";
        }
        else
        {
            infoPopup.SetActive(false);  // 빈칸 팝업 끄기
        }
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

    void CreateDonutButtonAtCenter()
    {
        int center = boardSize / 2;
        generatorCell = cells[center, center]; // 중앙 칸 기억해두기

        GameObject buttonObj = Instantiate(donutSpawnerPrefab, generatorCell.transform);
        RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = Vector2.zero;

        //Button btn = buttonObj.GetComponent<Button>();
        //btn.onClick.AddListener(SpawnDonutToEmptyCell);

        // 중앙 칸은 항상 활성화하지만 아이템은 못 들어오게 잠금 표시 꺼둠
        generatorCell.SetActive(true);
    }

    Cells FindEmptyActiveCell()
    {
        List<Cells> available = new List<Cells>();

        foreach (var c in cells)
        {
            if (c == null) continue;
            if (!c.isActive) continue; 
            if (c == generatorCell) continue;   // 중앙(생성기 칸) 제외
            if (c.IsEmpty()) available.Add(c);
        }

        if (available.Count == 0) return null;
        return available[Random.Range(0, available.Count)];
    }

    public void SpawnDonutToEmptyCell()
    {
        Cells target = FindEmptyActiveCell();
        if (target == null)
        {
            Debug.Log("생성 가능한 빈 칸 없음");
            return;
        }

        GameObject itemObj = Instantiate(donutPrefab, target.transform);
        RectTransform rt = itemObj.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;

        var item = itemObj.GetComponent<MergeItemUI>();
        item.GetComponent<Image>().sprite = donutSprite;
        target.SetItem(item);

        Debug.Log($"도넛 생성 완료 at ({target.name})");
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

    public IEnumerable<Cells> GetAllCells()
    {
        foreach (var c in cells)
            if (c != null) yield return c;
    }

    public Cells GetCell(int x, int y)
    {
        if (x < 0 || y < 0 || x >= boardSize || y >= boardSize) return null;
        return cells[x, y];
    }
}
