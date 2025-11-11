using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; set; }

    [Header("보드 설정")]
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
    public Sprite[] donutSprites;

    void Awake()
    {
        Instance = this;
    }

    async void Start()
    {
        selectionHighlight = GameObject.Find("Canvas/Main_Panel/Mid/Merge/Background/SelectCursor_Image").GetComponent<Image>();
        infoPopup = GameObject.Find("Canvas/Main_Panel/Bottom/Description");
        infoText = GameObject.Find("Canvas/Main_Panel/Bottom/Description/Text").GetComponent<TMP_Text>();

        GenerateBoard();
        UpdateBoardUnlock(1);
        CreateDonutButtonAtCenter();

        if (selectionHighlight != null) selectionHighlight.gameObject.SetActive(false);
        if (infoPopup != null) infoPopup.SetActive(false);

        // Firestore에서 보드 불러오기
        
    }

    //void OnApplicationQuit()
    //{
    //    BoardSaveManager.Save(this);
    //}

    public async Task OnCellClicked(Cells cell)
    {
        bool isSameCell = (selectedCell == cell);
        SelectCell(cell);

        // 셀 상태별 동작
        if (cell == generatorCell)
        {
            // 도넛 생성기
            infoPopup.SetActive(true);
            infoText.text = "도넛 생성기에요!\n다시 터치 시 도넛을 생성합니다.";

            if (isSameCell)
            {
                SpawnDonutToEmptyCell();
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

    //격자 표시
    public void SelectCell(Cells cell)
    {
        selectedCell = cell;

        if (selectionHighlight == null) return;

        if (cell == null)
        {
            selectionHighlight.gameObject.SetActive(false);
            return;
        }

        selectionHighlight.gameObject.SetActive(true);
        selectionHighlight.transform.SetParent(cell.transform, false);
        selectionHighlight.rectTransform.anchoredPosition = Vector2.zero;
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

        // 중앙 칸은 항상 활성화하지만 아이템은 못 들어오게 잠금 표시 꺼둠
        generatorCell.SetActive(true);
    }

    //private MergeItemUI CreateDonutInCell(Cells cell, Sprite sprite)
    //{
    //    GameObject itemObj = Instantiate(donutPrefab, cell.transform);
    //    RectTransform rt = itemObj.GetComponent<RectTransform>();
    //    rt.anchoredPosition = Vector2.zero;
    //    rt.localScale = Vector3.one;

    //    var item = itemObj.GetComponent<MergeItemUI>();
    //    item.GetComponent<Image>().sprite = sprite;
    //    item.donutID = DonutDatabase.GetIDBySprite(sprite);
    //    cell.SetItem(item);
    //    return item;
    //}

    private Cells FindEmptyActiveCell()
    {
        List<Cells> available = new();
        foreach (var c in cells)
        {
            if (c == null || !c.isActive || c == generatorCell) continue;
            if (c.IsEmpty()) available.Add(c);
        }

        return available.Count > 0 ? available[Random.Range(0, available.Count)] : null;
    }

    private void SpawnDonutToEmptyCell()
    {
        Cells target = FindEmptyActiveCell();
        if (target == null)
        {
            Debug.Log("빈 칸이 없습니다.");
            return;
        }

        // 생성기에서 도넛 정보 랜덤 선택 (DonutGenerator 내부 확률 계산)
        var generator = generatorCell.GetComponentInChildren<DonutGenerator>();
        var donutData = generator.GetRandomDonut();
        if (donutData == null)
        {
            Debug.LogWarning("도넛 데이터가 없습니다.");
            return;
        }

        // 공용 도넛 프리팹으로 생성
        GameObject donutObj = Instantiate(donutPrefab, target.transform);
        RectTransform rt = donutObj.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;

        // 스프라이트, ID, 기타 정보 적용
        var item = donutObj.GetComponent<MergeItemUI>();
        var img = donutObj.GetComponent<Image>();
        img.sprite = donutData.sprite;
        item.donutID = donutData.id;

        // 셀에 등록
        target.SetItem(item);

        Debug.Log($"{donutData.displayName} 생성됨 (Level {donutData.level}, Type: {donutData.donutType})");
    }


    // 도넛 찾기
    public Cells FindCellByDonutID(string targetID)
    {
        foreach (var cell in GetAllCells())
        {
            if (!cell.isActive) continue;
            if (cell.donutID == targetID)
                return cell;
        }
        return null;
    }

    //도넛 삭제
    public void RemoveDonutByID(string targetID)
    {
        Cells target = FindCellByDonutID(targetID);
        if (target == null)
        {
            Debug.LogWarning($"도넛 ID '{targetID}'를 찾을 수 없습니다.");
            return;
        }

        if (target.occupant != null)
            Destroy(target.occupant.gameObject);

        target.ClearItem();
        Debug.Log($"도넛 '{targetID}' 삭제 완료");
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
