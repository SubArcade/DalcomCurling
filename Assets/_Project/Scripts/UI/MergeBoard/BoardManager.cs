using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

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

    private Cells[,] cells;
    private Cells generatorCell; // 도넛 생성기가 위치한 셀
    public Cells GeneratorCell => generatorCell;

    public Cells selectedCell; // 선택한 셀

    // 보드판 잠금이미지 용도
    private readonly int[,] requiredLevelMap =
    {   
        { 7, 7, 7, 7, 7, 7,10 },
        { 7, 3, 3, 3, 3, 5,10 },
        { 7, 3, 0, 0, 0, 5,10 },
        { 7, 3, 0, 0, 0, 5,10 },
        { 7, 3, 0, 0, 0, 5,10 },
        { 7, 5, 5, 5, 5, 5,10 },
        {10,10,10,10,10,10,10 }
    };

    [Tooltip("임시 보관칸")] public TempStorageSlot tempStorageSlot;

    void Awake()
    {
        Instance = this;
        tempStorageSlot = FindObjectOfType<TempStorageSlot>();
    }

    void Start()
    {
        selectionHighlight = GameObject.Find("Canvas/GameObject/Main_Panel/MainMenu/Mid/Merge/Background/SelectCursor_Image").GetComponent<Image>();
        if (selectionHighlight != null) selectionHighlight.gameObject.SetActive(false);
        GenerateBoard();
        UpdateBoardUnlock(1);
        CreateDonutButtonAtCenter();
        
        // 불러온 데이터로 보드 복원
        LoadBoardLocal();

        // 불러온 데이터로 보드 복원
        LoadBoardLocal();

    }

    public void OnCellClicked(Cells cell)
    {
        bool isSameCell = (selectedCell == cell);
        SelectCell(cell);

        // 셀 상태별 동작
        if (cell == generatorCell)
        {
            // 도넛 생성기
            if (isSameCell)
            {
                SpawnDonutToEmptyCell();
            }
        }

        // 기프트박스인지 확인
        var item = cell.occupant;
        if (item != null && item.donutId.StartsWith("Gift"))
        {
            // 첫 번째 터치 → 선택만
            if (!isSameCell)
            {
                SelectCell(cell); // 선택만 하고 리턴
                return;
            }
            // 두 번째 터치 → 보상 지급
            ClaimGiftReward(item.donutId, cell);
            return;

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
        cells = new Cells[boardSize, boardSize];

        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                GameObject obj = Instantiate(cellPrefab, transform);
                Cells cell = obj.GetComponent<Cells>();

                cell.gridX = x;
                cell.gridY = y;
                cells[x, y] = cell;

                int reqLevel = requiredLevelMap[y, x];
                cell.SetLockLevel(reqLevel);
                cell.UpdateLockState();
            }
        }
    }

    void CreateDonutButtonAtCenter() // 생성기 중앙에 생성
    { 
        int center = boardSize / 2;
        generatorCell = cells[center, center]; // 중앙 칸 기억해두기

        GameObject buttonObj = Instantiate(donutSpawnerPrefab, generatorCell.transform);
        RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = Vector2.zero;

        // 중앙 칸은 항상 활성화하지만 도넛은 못 들어오게 잠금 표시 꺼둠
        generatorCell.SetActive(true);
        SelectCell(generatorCell);
    }

    // 빈칸 찾는셀
    public Cells FindEmptyActiveCell()
    {
        List<Cells> available = new();
        foreach (var c in cells)
        {
            if (c == null || !c.isActive || c == generatorCell) continue;
            if (c.IsEmpty()) available.Add(c);
        }

        return available.Count > 0 ? available[Random.Range(0, available.Count)] : null;
    }

    //빈칸 도넛생성 함수
    private void SpawnDonutToEmptyCell()
    {
        Cells target = FindEmptyActiveCell();
        if (target == null)
        {
            Debug.Log("빈 칸이 없습니다.");
            return;
        }
        PlayerData playerData = DataManager.Instance.PlayerData;
        if (playerData.energy <= 0)
        {
            Debug.Log("에너지가 부족합니다");
            return;
        }

        //에너지 차감
        int useEnergy = playerData.energy -= 1;
        DataManager.Instance.EnergyChange(useEnergy);

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
        item.donutData = donutData;
        item.donutId = donutData.id;
        target.SetItem(item, donutData);



        //Debug.Log($"{donutData.displayName} 생성됨 (Level {donutData.level}, Type: {donutData.donutType})");
        AutoSaveBoardLocal();
    }

    public void SpawnGiftBox()
    {
        // 빈 활성 셀 찾기
        var cell = FindEmptyActiveCell();
        Debug.Log("CHECK cell : " + (cell != null));
        // GiftBox Level 1 데이터 가져오기
        var giftData = DataManager.Instance.GetGiftBoxData(1);
        if (giftData == null)
        {
            Debug.LogError("❌ GiftBoxData Level 1 NOT FOUND");
            return;
        }
        else
        {
            Debug.Log($"Gift Loaded: {giftData.id}, sprite:{giftData.sprite}");
        }

        // GiftBoxData → DonutData 로 변환
        DonutData fakeDonut = new DonutData()
        {
            id = giftData.id,
            sprite = giftData.sprite,
            donutType = DonutType.Gift,   // 반드시 Gift 타입
            level = giftData.level        // 기프트박스 레벨 = 도넛레벨
        };

        Debug.Log("CHECK donutPrefab : " + donutPrefab);

        if (cell == null)
        {
            tempStorageSlot.Add(giftData);
            Debug.Log("빈 칸 없음, 임시칸에 생성");
            return;
        }

        // 프리팹 생성
        GameObject obj = Instantiate(donutPrefab, cell.transform);
        Debug.Log("CHECK obj : " + obj);
        var item = obj.GetComponent<MergeItemUI>();
        Debug.Log("CHECK MergeItemUI : " + item);
        var img = obj.GetComponent<Image>();

        // GiftBox 아이콘/ID 설정
        img.sprite = fakeDonut.sprite;
        item.donutId = fakeDonut.id;
        item.donutData = fakeDonut;   // 반드시 넣기 (MergeItemUI가 Data 기반으로 동작)

        // 셀에 등록
        cell.SetItem(item, fakeDonut.id);

    }

    //기프트박스 보상 함수
    void ClaimGiftReward(string giftId, Cells cell)
    {
        // giftId가 GiftBox인지 확인
        int level = ParseGiftLevel(giftId);
        if (level <= 0)
        {
            return;
        }

        // GiftBox 데이터 가져오기
        var giftData = DataManager.Instance.GetGiftBoxData(level);
        if (giftData == null)
        {
            return;
        }
        //변수에 보상값+기존값 더해서 저장
        int gold = Random.Range(giftData.minGold, giftData.maxGold + 1);
        int energy = Random.Range(giftData.minEnergy, giftData.maxEnergy + 1);
        int gem = Random.Range(giftData.minGem, giftData.maxGem);


        int newGold = DataManager.Instance.PlayerData.gold + gold;
        int newEnergy = DataManager.Instance.PlayerData.energy + energy;
        int newGem = DataManager.Instance.PlayerData.gem + gem;
        //change함수로 갱신해줘야 UI즉각 반영됨
        DataManager.Instance.GoldChange(newGold);
        DataManager.Instance.EnergyChange(newEnergy);
        DataManager.Instance.GemChange(newGem);

        UIManager.Instance.Open(PanelId.UseGiftBoxPopUp);

        //팝업에 각 보상별로 텍스트 적용
        GameObject usegiftbox = GameObject.Find("UseGiftBoxPopUp");
        if (usegiftbox != null)
        {
            var popup = usegiftbox.GetComponent<Scr_UseGiftBoxPopUp>();
            if (popup != null)
                popup.SetRewardTexts(gold, energy,gem);
        }

        // 보상 후 기프트 박스 삭제
        if (cell.occupant != null)
        { 
            Destroy(cell.occupant.gameObject);
        }
        cell.ClearItem();
        BoardManager.Instance.AutoSaveBoardLocal();
    }

    // 도넛 찾기
    public Cells FindCellByDonutID(string targetID)
    {
        foreach (var cell in GetAllCells())
        {
            if (!cell.isActive) continue;
            if (cell.donutId == targetID)
                return cell;
        }
        return null;
    }
    
    public void RefreshBoardUnlock() // 플레이어 레벨로 보드 갱신
    {
        int playerLevel = DataManager.Instance.PlayerData.level;
        UpdateBoardUnlock(playerLevel);
    }

    // 레벨 활성영역 업데이트
    public void UpdateBoardUnlock(int level)
    {
        int activeSize = GetActiveSize(level);
        int center = boardSize / 2;

        // 짝수 크기일 때 좌상단으로 한 칸 치우치도록 계산
        int start = Mathf.Clamp(center - activeSize / 2, 0, boardSize - activeSize);
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
    private int GetActiveSize(int level) //TODO: PlayerData에서 가져와야함.
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

    public void AutoSaveBoardLocal()
    {
        var boardData = DataManager.Instance.MergeBoardData;
        if (boardData == null) return;

        boardData.cells.Clear();
        
        foreach (var cell in GetAllCells())
        {
            CellData data = new CellData
            {
                x = cell.gridX,
                y = cell.gridY,
                isCellActive = cell.isActive,

                // occupant 말고 cell.donutId 사용해야 함
                donutId = !string.IsNullOrEmpty(cell.donutId) ? cell.donutId : null
            };

            boardData.cells.Add(data);
        }

        //Debug.Log($"[AutoSaveBoardLocal] 저장 완료: 총 {boardData.cells.Count}칸 저장");
    }

    public void LoadBoardLocal()
    {
        var boardData = DataManager.Instance.MergeBoardData;
        if (boardData == null || boardData.cells == null || boardData.cells.Count == 0)
        {
            Debug.LogWarning("[LoadBoardLocal] 저장된 보드 데이터가 없습니다.");
            return;
        }

        foreach (var data in boardData.cells)
        {
            Cells cell = GetCell(data.x, data.y);
            if (cell == null) continue;

            // 저장된 활성상태 그대로 복구
            cell.isActive = data.isCellActive;

            // 락 UI 갱신 (잠금 또는 잠금 해제)
            if (cell.LockOverlay != null)
                cell.LockOverlay.gameObject.SetActive(!cell.isActive);

            // 기존 도넛 제거
            if (cell.occupant != null)
            {
                Destroy(cell.occupant.gameObject);
                cell.ClearItem();
            }

            // 도넛 없으면 빈칸으로 유지
            if (string.IsNullOrEmpty(data.donutId))
                continue;

            // 도넛 데이터 찾기
            DonutData donut = DataManager.Instance.GetDonutByID(data.donutId);
            if (donut == null)
            {
                Debug.LogWarning($"[LoadBoardLocal] 도넛 ID '{data.donutId}' 를 찾을 수 없습니다.");
                continue;
            }

            // 도넛 프리팹 생성
            GameObject obj = Instantiate(donutPrefab, cell.transform);
            var item = obj.GetComponent<MergeItemUI>();
            var img = obj.GetComponent<Image>();

            img.sprite = donut.sprite;
            item.donutId = donut.id;
            item.donutData = donut;

            cell.SetItem(item, donut);
        }

        //Debug.Log("[LoadBoardLocal] 보드 로드 완료");
    }

    // 임시보관칸에서 보드판에 생성
    public void SpawnFromTempStorage(GiftBoxData giftData)
    {
        Cells cell = FindEmptyActiveCell();
        if (cell == null)
        {
            Debug.Log("빈 칸이 없습니다.");
            return;
        }

        DonutData fakeDonut = new DonutData()
        {
            id = giftData.id,
            sprite = giftData.sprite,
            donutType = DonutType.Gift,
            level = giftData.level            // GiftBox 레벨과 일치
        };

        GameObject obj = Instantiate(donutPrefab, cell.transform);
        var item = obj.GetComponent<MergeItemUI>();

        item.donutId = fakeDonut.id;
        item.donutData = fakeDonut;

        obj.GetComponent<Image>().sprite = fakeDonut.sprite;
        cell.SetItem(item, fakeDonut);
    }

    private int ParseGiftLevel(string id) //기프트박스의 id문자열에서 레벨숫자만 추출하는 유틸함수
    {
        if (string.IsNullOrEmpty(id)) return -1; //null이거나 빈 문자열이면 잘못된 입력 -1 반환
        var parts = id.Split('_'); //gift와 1로 나눠서 배열로 저장 part[0] = gift, part[1] = 1
        if (parts.Length != 2) return -1; //gift_1 처럼 정확히 두 부분으로 나뉘지 않으면 잘못된 형식 -1 반환
        if (parts[0] != "Gift") return -1;  //첫번째 부분이 gift가 아니면 -1 반환
        if (int.TryParse(parts[1], out int level)) return level; //두번째 부분이 숫자면 level 저장하고 반환
        return -1;
    }

}
