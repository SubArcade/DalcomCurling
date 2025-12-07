using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;
using static UnityEngine.EventSystems.EventTrigger;
using DG.Tweening;
using Random = UnityEngine.Random;

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
    public bool isCompleted = false;
    
    [SerializeField] private GameObject entryPopupObj;
    
    void Awake()
    {
        isCompleted = false;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        tempStorageSlot = FindObjectOfType<TempStorageSlot>();
    }

    private void OnEnable()
    {
        SetBoard();
    }

    void Start()
    {
        selectionHighlight = GameObject.Find("Canvas/GameObject/Main_Panel/MainMenu/Mid/Merge/Background/SelectCursor_Image").GetComponent<Image>();
        if (selectionHighlight != null) selectionHighlight.gameObject.SetActive(false);
        //GenerateBoard();
        //UpdateBoardUnlock(1);
        //SetBoard();
    }

    public void SetBoard()
    {
        if (!isCompleted)
        {
            Debug.Log("[BoardManager] SetBoard 실행");
            isCompleted = true;
            if (cells == null || cells.Length == 0) GenerateBoard();

            CreateDonutButtonAtCenter();
            RefreshBoardUnlock();
            LoadBoardLocal();
        }
    }
    
    public void ResetBoard()
    {
        foreach (Transform child in transform)
        {
            var item = child.GetComponentInChildren<MergeItemUI>();

            if (item != null)
            {
                Debug.Log("[BoardManager] ResetBoard → " + item.name);
                Destroy(item.gameObject);
            }
        }
    }
    public void ResetEntry(int index = 100)
    {
        if (entryPopupObj.GetComponent<Scr_EntryPopUP>() == null)
            return;
        
        Debug.Log("[BoardManager] ResetBoard 호출");
        var entryObj = entryPopupObj.GetComponent<Scr_EntryPopUP>();

        if (index != 100)
        {
            var item = entryObj.entrySlots[index].gameObject.GetComponentInChildren<MergeItemUI>();
            if (item != null)
            {
                //Debug.Log("[BoardManager] ResetBoard → " + item.name);
                entryObj.entrySlots[index].currentItem = null;
                Destroy(item.gameObject);
            }
        }
        else
        {
            foreach (var child in entryObj.entrySlots)
            {
                var item = child.gameObject.GetComponentInChildren<MergeItemUI>();
                
                if (item != null)
                {
                    //Debug.Log("[BoardManager] ResetBoard → " + item.name);
                    child.currentItem = null;
                    Destroy(item.gameObject);
                }
            }
        }
    }

    public void ResetTempGiftIds()
    {
        tempStorageSlot.storage.Clear();
        tempStorageSlot.RefreshUI();
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

        // 칸 선택 사운드
        if (generatorCell) return;
        SoundManager.Instance.selectSlotScroll();
    }

    void GenerateBoard()
    {
        Debug.Log("GenerateBoard 완료");
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
        //Debug.Log("FindEmptyActiveCell1111111111111111111111111111111");
        if (cells == null)
        {
            Debug.Log("[BoardManager] cells 배열이 NULL 입니다!!", this);
            SetBoard();
        }
        
        List<Cells> available = new();
        foreach (var c in cells)
        {
            if (c == null || !c.isActive || c == generatorCell) continue;
            if (c.IsEmpty() && c.isTempOccupied == false)
                available.Add(c);
        }
        
        return available.Count > 0 ? available[Random.Range(0, available.Count)] : null;
    }

    //빈칸 도넛생성 함수
    private void SpawnDonutToEmptyCell()
    {
        Cells target = FindEmptyActiveCell();
        if (target == null)
        {
            //Debug.Log("빈 칸이 없습니다.");
            // 가득착 애널리틱스
            AnalyticsManager.Instance.MergeBoardFull();
            Debug.Log("빈 칸이 없습니다.");
            
            // + 도넛의 포화상태때 생성 시도를 할때 나는 사운드 ---
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.saturation();
            }
           
            return;
        }

        PlayerData playerData = DataManager.Instance.PlayerData;
        if (playerData.energy <= 0)
        {
            Debug.Log("에너지가 부족합니다");
            UIManager.Instance.Open(PanelId.EnergyRechargePopUp);
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
        GameObject donutObj = Instantiate(donutPrefab, generatorCell.transform);
        RectTransform rt = donutObj.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.zero;

        // 스프라이트, ID, 기타 정보 적용
        var item = donutObj.GetComponent<MergeItemUI>();
        var img = donutObj.GetComponent<Image>();
        img.sprite = donutData.sprite;
        item.donutData = donutData;
        item.donutId = donutData.id;

        //이동 시 캔버스 부모로
        Canvas rootCanvas = generatorCell.GetComponentInParent<Canvas>();
        donutObj.transform.SetParent(rootCanvas.transform, true);

        target.isTempOccupied = true;

        rt.DOScale(1.1f, 0.15f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            // target UI 위치 계산
            RectTransform canvasRect = rootCanvas.transform as RectTransform;

            Vector2 uiTargetPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                RectTransformUtility.WorldToScreenPoint(null, target.transform.position),
                null,
                out uiTargetPos
            );

            // 이동
            rt.DOAnchorPos(uiTargetPos, 0.125f).SetEase(Ease.InOutQuad).OnComplete(() =>
            {
                // 도착 후 셀로 되돌리기
                donutObj.transform.SetParent(target.transform, false);
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;

                target.isTempOccupied = false;
                target.SetItem(item, donutData);

                if (SoundManager.Instance != null)
                    SoundManager.Instance.createDonut();

                AutoSaveBoardLocal();
            });
        });
    }

    //인게임 도넛 획득
    public void AddRewardDonut(DonutData rewardData)
    {
        if (rewardData == null)
        {
            Debug.LogError("Reward DonutData is null");
            return;
        }

        // 빈 활성 칸 찾기
        Cells target = FindEmptyActiveCell();
        
         if (target == null)
         {
             Debug.Log("보드가 꽉 찼습니다 → 임시보관칸 이동");
        
             bool added = tempStorageSlot.Add(rewardData);
        
             if (!added)
                 Debug.LogWarning("임시보관칸도 가득 찼습니다!");
        
             return; // 도넛 생성하지 않고 종료
         }

        // var cellList = DataManager.Instance.MergeBoardData.cells;
        //
        // List<CellData> target = new();
        //
        // foreach (var cell in cellList)
        // {
        //     if (cell.isCellActive && cell.donutId == "" && cell.donutId == null)
        //     {
        //         target.Add(cell);
        //     }
        // }
        
        // 보드에 도넛 생성
        GameObject obj = Instantiate(donutPrefab, target.transform);
        var item = obj.GetComponent<MergeItemUI>();
        var img = obj.GetComponent<Image>();

        img.sprite = rewardData.sprite;
        item.donutData = rewardData;
        item.donutId = rewardData.id;

        target.SetItem(item, rewardData);

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
            Debug.LogError("GiftBoxData Level 1 NOT FOUND");
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

        if (cell == null)
        {
            tempStorageSlot.Add(giftData);
            Debug.Log("빈 칸 없음, 임시칸에 생성");
            return;
        }

        // 프리팹 생성
        GameObject obj = Instantiate(donutPrefab, cell.transform);
        var item = obj.GetComponent<MergeItemUI>();
        var img = obj.GetComponent<Image>();

        // GiftBox 아이콘/ID 설정
        img.sprite = fakeDonut.sprite;
        item.donutId = fakeDonut.id;
        item.donutData = fakeDonut;   // 반드시 넣기 (MergeItemUI가 Data 기반으로 동작)

        // 셀에 등록
        cell.SetItem(item, fakeDonut.id);
        AutoSaveBoardLocal(); // 보드 저장
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
        AutoSaveBoardLocal();
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

            //// 1) 셀 활성/비활성 복구
            //cell.isActive = data.isCellActive;

            //if (cell.LockOverlay != null)
            //    cell.LockOverlay.gameObject.SetActive(!cell.isActive);

            // 2) 기존 도넛 제거
            if (cell.occupant != null)
            {
                Destroy(cell.occupant.gameObject);
                cell.ClearItem();
            }

            // 3) 도넛 없음 → 빈칸 유지
            if (string.IsNullOrEmpty(data.donutId))
                continue;

            // 4) 도넛 데이터 불러오기 (일반 도넛 / Gift 모두 포함)
            DonutData donut = DataManager.Instance.GetDonutByID(data.donutId);

            // 💥 GiftBox였다면 DonutData가 null일 수 있음
            // → GiftBoxData → DonutData 변환
            if (donut == null)
            {
                GiftBoxData gift = DataManager.Instance.GetGiftBoxDataByID(data.donutId);
                if (gift != null)
                {
                    donut = new DonutData()
                    {
                        id = gift.id,
                        sprite = gift.sprite,
                        donutType = DonutType.Gift,
                        level = gift.level
                    };
                }
                else
                {
                    Debug.LogWarning($"[LoadBoardLocal] 저장된 도넛ID '{data.donutId}' 를 찾을 수 없습니다.");
                    continue;
                }
            }

            // 5) 도넛 프리팹 생성 및 설정
            GameObject obj = Instantiate(donutPrefab, cell.transform);
            var item = obj.GetComponent<MergeItemUI>();
            var img = obj.GetComponent<Image>();

            img.sprite = donut.sprite;

            item.donutId = donut.id;
            item.donutData = donut;

            // 6) 셀 등록
            cell.SetItem(item, donut);
        }

        Debug.Log("[LoadBoardLocal] 보드 로드 완료");
        isCompleted = true;
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
