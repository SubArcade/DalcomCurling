using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scr_OrderSystem : MonoBehaviour
{ 
    [Header("1번 주문서")]
    [SerializeField, Tooltip("주문서 그룹")] private GameObject orderGroup1;
    [SerializeField, Tooltip("주문서 안의 이미지들")] private List<Image> orderDonuts1;
    [SerializeField, Tooltip("새로고침 버튼")] private Button refreshBtn1;
    [SerializeField, Tooltip("주문도넛 가격합계")] private TextMeshProUGUI costText1;
    [SerializeField, Tooltip("주문완료 버튼")] private GameObject orderClearBtn1;
    [SerializeField, Tooltip("주문서 완료확인용오브젝트")] private GameObject completeObject1; 

    [Header("2번 주문서")]
    [SerializeField] private GameObject orderGroup2;
    [SerializeField] private List<Image> orderDonuts2;
    [SerializeField] private Button refreshBtn2;
    [SerializeField] private TextMeshProUGUI costText2;
    [SerializeField] private GameObject orderClearBtn2;
    [SerializeField] private GameObject completeObject2;

    [Header("3번 주문서")]
    [SerializeField] private GameObject orderGroup3;
    [SerializeField] private List<Image> orderDonuts3;
    [SerializeField] private Button refreshBtn3;
    [SerializeField] private TextMeshProUGUI costText3;
    [SerializeField] private GameObject orderClearBtn3;
    [SerializeField] private GameObject completeObject3;

    [Header("갱신된 새로고침 횟수 텍스트")]
    [SerializeField] private TextMeshProUGUI refreshCountText;

    private const string RefreshCountKey = "Quest_RefreshCount";

    //도넛들의 체크이미지 활성/비활성용
    public class DonutVisualInfo
    {
        public Image donutImage;
        public GameObject checkMark;
    }

    // 도넛이미지와 같이 갱신될 도넛타입TextMeshPro, 도넛단계 TextMeshPro
    public class DonutTextInfo
    {
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI typeText;
    }
    //주문서별로 도넛 텍스트 정보를 담을 리스트
    private List<DonutTextInfo> donutTextInfos1 = new List<DonutTextInfo>();
    private List<DonutTextInfo> donutTextInfos2 = new List<DonutTextInfo>();
    private List<DonutTextInfo> donutTextInfos3 = new List<DonutTextInfo>();
    
    // 주문서별 도넛 ID 저장 리스트
    private List<string> orderDonutIDs1 = new List<string>();
    private List<string> orderDonutIDs2 = new List<string>();
    private List<string> orderDonutIDs3 = new List<string>();

    //주문서별로 체크마크를 담을 리스트
    private List<DonutVisualInfo> donutVisuals1 = new();
    private List<DonutVisualInfo> donutVisuals2 = new();
    private List<DonutVisualInfo> donutVisuals3 = new();

    //주문서별로 계산되는 최종 보상골드
    private int rewardGold1 = 0;
    private int rewardGold2 = 0;
    private int rewardGold3 = 0;

    //주문서 완료시 자동새로고침 할동안만 컴플리트버튼을 유지하기위한 판단변수
    private bool isRewarding1 = false;
    private bool isRewarding2 = false;
    private bool isRewarding3 = false;

    //주문서가 중복으로 완료처리하지 않도록 막는 플래그
    private bool isOrderClearing = false;

    //저장된퀘스트가 있는지 없는지 판단용도 변수
    private bool isRestored = false;
    
    //도넛당 가격 범위 최소값,최대값
    public class DonutRewardRange
    {
        public int minReward;
        public int maxReward;
    }

    //도넛당 레벨별 보상테이블 생성
    private Dictionary<int, DonutRewardRange> levelRewardTable = new();
    //가격들....
    private void DonutRewardTable()
    {
        levelRewardTable = new Dictionary<int, DonutRewardRange>
    {
        { 1,  new DonutRewardRange { minReward = 100,  maxReward = 150 } },
        { 2,  new DonutRewardRange { minReward = 200,  maxReward = 250 } },
        { 3,  new DonutRewardRange { minReward = 300,  maxReward = 350 } },
        { 4,  new DonutRewardRange { minReward = 400,  maxReward = 450 } },
        { 5,  new DonutRewardRange { minReward = 500,  maxReward = 550 } },
        { 6,  new DonutRewardRange { minReward = 600,  maxReward = 650 } },
        { 7,  new DonutRewardRange { minReward = 700,  maxReward = 750 } },
        { 8,  new DonutRewardRange { minReward = 800,  maxReward = 850 } },
        { 9,  new DonutRewardRange { minReward = 900,  maxReward = 950 } },
        { 10, new DonutRewardRange { minReward = 1000, maxReward = 1100 } },
        { 11, new DonutRewardRange { minReward = 1200, maxReward = 1300 } },
        { 12, new DonutRewardRange { minReward = 1400, maxReward = 1500 } },
        { 13, new DonutRewardRange { minReward = 1600, maxReward = 1700 } },
        { 14, new DonutRewardRange { minReward = 1800, maxReward = 1900 } },
        { 15, new DonutRewardRange { minReward = 2000, maxReward = 2100 } },
        { 16, new DonutRewardRange { minReward = 2200, maxReward = 2300 } },
        { 17, new DonutRewardRange { minReward = 2400, maxReward = 2500 } },
        { 18, new DonutRewardRange { minReward = 2600, maxReward = 2700 } },
        { 19, new DonutRewardRange { minReward = 2800, maxReward = 2900 } },
        { 20, new DonutRewardRange { minReward = 3000, maxReward = 3100 } },
        { 21, new DonutRewardRange { minReward = 3500, maxReward = 3800 } },
        { 22, new DonutRewardRange { minReward = 4000, maxReward = 4300 } },
        { 23, new DonutRewardRange { minReward = 4500, maxReward = 4800 } },
        { 24, new DonutRewardRange { minReward = 5000, maxReward = 5300 } },
        { 25, new DonutRewardRange { minReward = 5500, maxReward = 5800 } },
        { 26, new DonutRewardRange { minReward = 6000, maxReward = 6300 } },
        { 27, new DonutRewardRange { minReward = 6500, maxReward = 6800 } },
        { 28, new DonutRewardRange { minReward = 7000, maxReward = 7300 } },
        { 29, new DonutRewardRange { minReward = 7500, maxReward = 7800 } },
        { 30, new DonutRewardRange { minReward = 8000, maxReward = 8300 } },
    };
    }

    private readonly DonutType[] orderDonutTypes = { DonutType.Hard, DonutType.Soft, DonutType.Moist };

    void Awake()
    {
        LoadRefreshCount();
        refreshCountText.text = $"{DataManager.Instance.QuestData.refreshCount}/{DataManager.Instance.QuestData.maxCount}";

        // orderdonuts1~3의 자식 텍스트들을 리스트에 저장
        donutTextInfos1 = FindLevelAndTypeText(orderDonuts1);
        donutTextInfos2 = FindLevelAndTypeText(orderDonuts2);
        donutTextInfos3 = FindLevelAndTypeText(orderDonuts3);

        //orderdonuts1~3의 자식 체크마크들을 리스트에 저장
        donutVisuals1 = FindCheckMark(orderDonuts1);
        donutVisuals2 = FindCheckMark(orderDonuts2);
        donutVisuals3 = FindCheckMark(orderDonuts3);

        //도넛별 보상골드 갱신
        DonutRewardTable();
    }
    void Start()
    {

        //새로고침버튼 연결
        refreshBtn1.onClick.AddListener(() => OnclickRefreshOrderBtn(refreshBtn1));
        refreshBtn2.onClick.AddListener(() => OnclickRefreshOrderBtn(refreshBtn2));
        refreshBtn3.onClick.AddListener(() => OnclickRefreshOrderBtn(refreshBtn3));

        //주문서 완료 연결
        completeObject1.GetComponent<Button>().onClick.AddListener(() => OnClickCompleteObject(completeObject1, orderDonutIDs1));
        completeObject2.GetComponent<Button>().onClick.AddListener(() => OnClickCompleteObject(completeObject2, orderDonutIDs2));
        completeObject3.GetComponent<Button>().onClick.AddListener(() => OnClickCompleteObject(completeObject3, orderDonutIDs3));
        

        //시작 시 퀘스트 완료 판떼기 비활성화
        completeObject1.SetActive(false);
        completeObject2.SetActive(false);
        completeObject3.SetActive(false);
    
        // 새로고침 회복 코루틴
        //refreshCoroutine = StartCoroutine(RecoveryRefreshCount());

        // 저장된 주문서 복원 시도
        bool isRestored = TryRestoreQuest();
        // 저장된게 없다면 새로고침으로 초기화
        if (!isRestored)
        {
            RefreshOrderDonut(orderDonuts1, donutTextInfos1, orderDonutIDs1, costText1);
            RefreshOrderDonut(orderDonuts2, donutTextInfos2, orderDonutIDs2, costText2);
            RefreshOrderDonut(orderDonuts3, donutTextInfos3, orderDonutIDs3, costText3);
        }

    }

    void Update()
    {
        bool isOrder1Complete = IsOrderCompleted(orderDonutIDs1, donutVisuals1);
        bool isOrder2Complete = IsOrderCompleted(orderDonutIDs2, donutVisuals2);
        bool isOrder3Complete = IsOrderCompleted(orderDonutIDs3, donutVisuals3);

        FindCheckMarkBoard();

        // 보상 UI 표시 조건
        completeObject1.SetActive(isOrder1Complete || isRewarding1);
        completeObject2.SetActive(isOrder2Complete || isRewarding2);
        completeObject3.SetActive(isOrder3Complete || isRewarding3);


        // ClearBtn은 항상 비활성화 상태 유지
        orderClearBtn1.SetActive(!isOrder1Complete);
        orderClearBtn2.SetActive(!isOrder2Complete);
        orderClearBtn3.SetActive(!isOrder3Complete);
    }

    void OnEnable()
    {
        LocalizationManager.Instance.OnLanguageChanged += RefreshAllDonutTexts;
        DataManager.Instance.UpdateUI += AddRefreshCount;
    }

    void OnDisable()
    {
        LocalizationManager.Instance.OnLanguageChanged -= RefreshAllDonutTexts;
    }

    //도넛레벨별 가격 책정하기
    private int GetRewardByDonutLevel(int level)
    {
        if (levelRewardTable.TryGetValue(level, out var range))
        {
            return UnityEngine.Random.Range(range.minReward, range.maxReward + 1);
        }
        return 0;
    }

    //orderdonuts1~3에서 자식체크마크를 찾아주는 함수
    private List<DonutVisualInfo> FindCheckMark(List<Image> donutImages)
    {
        var result = new List<DonutVisualInfo>();
        foreach (var img in donutImages)
        {
            var check = img.transform.Find("CheckMark")?.gameObject;
            result.Add(new DonutVisualInfo
            {
                donutImage = img,
                checkMark = check
            });
        }
        return result;
    }

    //orderdonuts1~3에서 자식텍스트를 찾아주는 함수
    private List<DonutTextInfo> FindLevelAndTypeText(List<Image> donutImages)
    {
        var result = new List<DonutTextInfo>();

        foreach (var img in donutImages)
        {
            var info = new DonutTextInfo
            {
                typeText = img.transform.Find("Type_Text")?.GetComponent<TextMeshProUGUI>(),
                levelText = img.transform.Find("Level_Text")?.GetComponent<TextMeshProUGUI>()
            };

            result.Add(info);
        }

        return result;
    }

    //머지보드판 체크마크 띄우기, 체크마크 삭제는 entryslot 스크립트에 추가함.
    private void FindCheckMarkBoard()
    {
        // 주문서에 있는 모든 도넛 ID를 하나의 집합으로 통합
        HashSet<string> allOrderIDs = new();
        allOrderIDs.UnionWith(orderDonutIDs1); //각각의 주문서 1,2,3에 있는 도넛id리스트 합치기
        allOrderIDs.UnionWith(orderDonutIDs2); //unionwith는 중복없이 합쳐수는 메서드라고함.
        allOrderIDs.UnionWith(orderDonutIDs3); //모든 주문서에 존재하는 도넛을 하나의 집합으로 저장

        foreach (var cell in BoardManager.Instance.GetAllCells()) //모든 셀 검사
        {
            if (!cell.isActive || cell.occupant == null) continue; //도넛이 존재하는 셀만 검사

            var item = cell.occupant;  //셀에 들어잇는 도넛을 변수에 저장
            string id = item.donutData?.id; //donutdata가 존재하면 id를 변수에 저장

            var checkMark = item.transform.Find("CheckMark")?.gameObject;
            //checkmark찾기
            

            if (checkMark != null)
            {
                checkMark.SetActive(allOrderIDs.Contains(id));
            }
        }
    }

    //새로고침 버튼 상호작용
    public void OnclickRefreshOrderBtn(Button ClickedButton)
    {
        if (DataManager.Instance.QuestData.refreshCount <= 0)
        {
            UIManager.Instance.Open(PanelId.OrderRefreshPopUp);
            return;
        }

        DataManager.Instance.QuestData.refreshCount--;
        SaveRefreshCount();
        refreshCountText.text = $"{DataManager.Instance.QuestData.refreshCount}/{DataManager.Instance.QuestData.maxCount}";
       
        // 해당 주문서의 CompleteObject 비활성화
        if (ClickedButton == refreshBtn1)
        {
            RefreshOrderDonut(orderDonuts1, donutTextInfos1, orderDonutIDs1,costText1);
            completeObject1.SetActive(false);
        }
        else if (ClickedButton == refreshBtn2)
        {
            RefreshOrderDonut(orderDonuts2, donutTextInfos2, orderDonutIDs2,costText2);

            completeObject2.SetActive(false);
        }
        else if (ClickedButton == refreshBtn3)
        {
            RefreshOrderDonut(orderDonuts3, donutTextInfos3, orderDonutIDs3,costText3);
            completeObject3.SetActive(false);
        }
    }

    public void ResetQuest()
    {
        OnclickRefreshOrderBtn(refreshBtn1);
        OnclickRefreshOrderBtn(refreshBtn2);
        OnclickRefreshOrderBtn(refreshBtn3);
    }
    
    //주문서 새로고침시 이미지, 이름.레벨,보상골드 갱신
    public void RefreshOrderDonut(List<Image> orderImages, List<DonutTextInfo> textInfos, List<string> idList, TextMeshProUGUI costText)
    {
        idList.Clear(); // id 초기화

        // 이번에 갱신할 도넛 개수를 랜덤으로 결정 (1~3)
        int refreshCount = Random.Range(1, 4);

        int totalReward = 0; //보상골드 초기화
        var questList = new List<QuestList>(); //도넛들의 퀘스트 정보를 저장할 리스트
        HashSet<string> usedDonuts = new(); //중복방지를 위한 도넛조합을 저장할 집합

        // 플레이어 레벨 기반 도넛 레벨 범위 계산
        int playerLevel = DataManager.Instance.PlayerData.level;
        var (minLevel, maxLevel) = DonutLevelRange(playerLevel); 

        // 먼저 모든 슬롯 비활성화
        for (int i = 0; i < orderImages.Count; i++)
        {
            orderImages[i].gameObject.SetActive(false);
            if (i < textInfos.Count)
            {
                textInfos[i].levelText.text = "";
                textInfos[i].typeText.text = "";
            }
        }

        // refreshCount 만큼만 채워 넣기
        for (int i = 0; i < refreshCount; i++)
        {
            DonutData donut = null; //도넛데이터를 찾기위해 변수 초기화
            int safety = 100; //무한루프 방지용 100회 제한
            while (safety-- > 0)
            {
                DonutType[] types = { DonutType.Hard, DonutType.Soft, DonutType.Moist };//타입랜덤으로 선택
                DonutType randomType = types[Random.Range(0, types.Length)]; 
                int randomLevel = Random.Range(minLevel, maxLevel + 1);//레벨을 랜덤으로 선택
                string comboKey = $"{randomType}_{randomLevel}"; //타입과 레벨을 조합키로 문자열 생성

                if (usedDonuts.Contains(comboKey)) continue; //중복이면 다시 시도

                //위의 랜덤 타입과 레벨을 통해 데이터매니저에서 도넛을 찾음
                var candidate = DataManager.Instance.GetDonutData(randomType, randomLevel);
                if (candidate != null)
                {
                    donut = candidate; //변수에 저장
                    usedDonuts.Add(comboKey); //위의 조합키 문자열을 중복방지용 리스트에 저장
                    break;
                }
            }

            if (donut == null) continue; //도넛이 없으면 건너뜀

            // 슬롯 활성화
            orderImages[i].gameObject.SetActive(true);
            orderImages[i].sprite = donut.sprite;

            idList.Add(donut.id);

            int reward = GetRewardByDonutLevel(donut.level);
            totalReward += reward;

            questList.Add(new QuestList
            {
                donutId = donut.id,
                rewardGold = reward
            });

            // 텍스트 갱신
            if (i < textInfos.Count)
            {
                textInfos[i].levelText.text = $"{donut.level}단계";
                textInfos[i].typeText.text = donut.donutType.ToString();
            }
        }

        costText.text = $"{totalReward}";

        // 주문서별 보상 저장
        if (orderImages == orderDonuts1) rewardGold1 = totalReward;
        else if (orderImages == orderDonuts2) rewardGold2 = totalReward;
        else if (orderImages == orderDonuts3) rewardGold3 = totalReward;
    }


    //머지보드판에서 도넛 검색후 주문서 클리어 여부 결정
    public bool IsOrderCompleted(List<string> orderDonutIDs, List<DonutVisualInfo> visuals)
    {
        var boardCells = BoardManager.Instance.GetAllCells();
        HashSet<string> boardDonutIDs = new();

        foreach (var cell in boardCells)
        {
            if (!cell.isActive || string.IsNullOrEmpty(cell.donutId)) continue;
            boardDonutIDs.Add(cell.donutId);
        }

        bool allMatched = true;

        for (int i = 0; i < orderDonutIDs.Count && i < visuals.Count; i++)
        {
            bool matched = boardDonutIDs.Contains(orderDonutIDs[i]);

            if (visuals[i].checkMark != null)
                visuals[i].checkMark.SetActive(matched);

            if (!matched)
                allMatched = false;
        }
        
        return allMatched;
    }

    //주문서 완료시 버튼 상호작용
    public void OnClickCompleteObject(GameObject completeObject, List<string> orderDonutIDs)
    {
        //다른주문서 처리중에는 리턴
        if (isOrderClearing) return;

        //윗줄 넘어왔으면처리중으로 전환
        isOrderClearing = true;

        // 모든 완료 버튼 잠시 비활성화 (시각적으로도 눌리지 않게)
        SetCompleteImageRed(completeObject1);
        SetCompleteImageRed(completeObject2);
        SetCompleteImageRed(completeObject3);
        // ⭐ 눌린 버튼은 조금 더 짙은 원래색으로 표시
        SetPressedButtonColor(completeObject);


        //퀘스트 클리어 시 같은 도넛을 전부 없애버리는것을 방지하기위한 체크변수
        var idsToRemove = new List<string>(orderDonutIDs);
        //셀 전체검사
        foreach (var cell in BoardManager.Instance.GetAllCells())
        {   //셀이 비활성화거나 비어있으면 건너뜀
            if (!cell.isActive || string.IsNullOrEmpty(cell.donutId)) continue;

            if (idsToRemove.Contains(cell.donutId))
            {   //제거대상이 있으면 파괴
                if (cell.occupant != null)
                    Destroy(cell.occupant.gameObject);
                //셀을 비움
                cell.ClearItem();
                //1개만 삭제
                idsToRemove.Remove(cell.donutId);

                // ⭐ 같은 ID 하나 제거했으면 루프 종료
                break;
            }
        }

        //보상골드 지급
        int reward = 0;
        if (completeObject == completeObject1) reward = rewardGold1;
        else if (completeObject == completeObject2) reward = rewardGold2;
        else if (completeObject == completeObject3) reward = rewardGold3;

        // 보상 지급 후 버튼 비활성화
        //var button = completeObject.GetComponent<Button>();
        //if (button != null)
        //    button.interactable = false;

        //컴플리트버튼 유지를 위해 true 적용
        if (completeObject == completeObject1) isRewarding1 = true;
        else if (completeObject == completeObject2) isRewarding2 = true;
        else if (completeObject == completeObject3) isRewarding3 = true;

        //클릭직후 완료버튼 즉시숨김
        completeObject.SetActive(false);

        //보상골드 데이터 저장
        int newGold = DataManager.Instance.PlayerData.gold + reward;
        DataManager.Instance.GoldChange(newGold);
        
        // 퀘스트 완료 보상 애널리틱스
        AnalyticsManager.Instance.QuestRewardComplete();
        // 새로운 주문서 등장
        SoundManager.Instance.receiptReward();
        
        //  주문서별 자동 새로고침 코루틴 시작
        if (completeObject == completeObject1)
            StartCoroutine(RefreshAfterOrderClear(orderDonuts1, donutTextInfos1, orderDonutIDs1, costText1, completeObject1, 1.5f));
        else if (completeObject == completeObject2)
            StartCoroutine(RefreshAfterOrderClear(orderDonuts2, donutTextInfos2, orderDonutIDs2, costText2, completeObject2, 1.5f));
        else if (completeObject == completeObject3)
            StartCoroutine(RefreshAfterOrderClear(orderDonuts3, donutTextInfos3, orderDonutIDs3, costText3, completeObject3, 1.5f));
    }

    //주문서 완료 후 퀘스트 알아서 새로고침
    private IEnumerator RefreshAfterOrderClear(List<Image> orderImages, List<DonutTextInfo> textInfos, List<string> idList, TextMeshProUGUI costText, GameObject completeObject, float delaySeconds) 
    {
        yield return new WaitForSeconds(delaySeconds);
        RefreshOrderDonut(orderImages, textInfos, idList, costText); // 새로고침
        //새로고침이 되었으니 컴플리트 버튼 비활성화
        if (completeObject == completeObject1) isRewarding1 = false;
        else if (completeObject == completeObject2) isRewarding2 = false;
        else if (completeObject == completeObject3) isRewarding3 = false;

        completeObject.SetActive(false); // 완료 UI 숨김

        // 버튼 다시 활성화
        var button = completeObject.GetComponent<Button>();
        if (button != null)
            button.interactable = true;

        // 처리중이 다끝나면 모든 완료 버튼 다시 활성화
        ResetCompleteImageColor(completeObject1);
        ResetCompleteImageColor(completeObject2);
        ResetCompleteImageColor(completeObject3);
        // ⭐ 눌린 버튼만 원래 색(#66E3AD)으로 복원
        ResetCompleteImageColor(completeObject);


        //이제 다른 주문서 완료가능
        isOrderClearing = false;
    }

    //게임 시작할때 이전 퀘스트정보 불러오기
    private void CallFromSavedQuest(List<QuestList> savedQuests, List<Image> orderImages, List<DonutTextInfo> textInfos, List<string> idList, TextMeshProUGUI costText)
    {
        
        idList.Clear();
        int totalReward = 0;

        for (int i = 0; i < savedQuests.Count && i < orderImages.Count && i < textInfos.Count; i++)
        {
            var quest = savedQuests[i];
            DonutData donut = DataManager.Instance.GetDonutByID(quest.donutId);
            if (donut == null) continue;

            orderImages[i].sprite = donut.sprite;

            idList.Add(donut.id);
            totalReward += quest.rewardGold;
        }
        SetDonutTexts(textInfos, idList);
        costText.text = $"{totalReward}";
    }

    //주문서1~3 각각 퀘스트 불러오기
    private bool TryRestoreQuest()
    {
        bool restored = false;

        var q1 = DataManager.Instance.QuestData.questList1;
        var q2 = DataManager.Instance.QuestData.questList2;
        var q3 = DataManager.Instance.QuestData.questList3;

        if (q1 != null && q1.Count > 0)
        {
            CallFromSavedQuest(q1, orderDonuts1, donutTextInfos1, orderDonutIDs1, costText1);
            restored = true;
        }
        if (q2 != null && q2.Count > 0)
        {
            CallFromSavedQuest(q2, orderDonuts2, donutTextInfos2, orderDonutIDs2, costText2);
            restored = true;
        }
        if (q3 != null && q3.Count > 0)
        {
            CallFromSavedQuest(q3, orderDonuts3, donutTextInfos3, orderDonutIDs3, costText3);
            restored = true;
        }
        return restored;
    }

    //플레이어 레벨에 따라 주문도넛 레벨 결정  
    private (int minLevel, int maxLevel) DonutLevelRange(int playerLevel)
    {
        int min = playerLevel;
        int max;

        switch (playerLevel)
        {
            case int n when n <= 9:
                max = playerLevel + 3;
                break;
            case int n when n <= 19:
                max = playerLevel + 5;
                break;
            default:
                max = playerLevel + 10;
                break;
        }
        return (min, Mathf.Clamp(max, min, 30));
    }
    //로컬라이제이션을 위한 언어변경용 함수
    private void RefreshAllDonutTexts()
    {
        SetDonutTexts(donutTextInfos1, orderDonutIDs1);
        SetDonutTexts(donutTextInfos2, orderDonutIDs2);
        SetDonutTexts(donutTextInfos3, orderDonutIDs3);
    }
    //도넛 텍스트 로컬라이징 설정 함수
    private void SetDonutTexts(List<DonutTextInfo> textInfos, List<string> idList)
    {
        for (int i = 0; i < textInfos.Count && i < idList.Count; i++)
        {//텍스트와 아이디 리스트 둘중 짧은쪽까지만 범위지정         
            var donut = DataManager.Instance.GetDonutByID(idList[i]); //현재 인덱스의 도넛 ID로 DonutData를 가져옴

            if (donut == null || donut.donutType == DonutType.Gift) continue;
            //도넛이 없거나 Gift 타입이면 무시

            LocalizationKey typeKey = donut.donutType switch
            { //로컬라이제이션키 설정
                DonutType.Hard => LocalizationKey.Label_HardDonut,
                DonutType.Soft => LocalizationKey.Label_SoftDonut,
                DonutType.Moist => LocalizationKey.Label_MoistDonut,
                _ => LocalizationKey.None
            };

            string localizedType = LocalizationManager.Instance.GetText(typeKey); //설정된 키 값 저장
            string typeText = $"{localizedType}"; //문자열로 변환후 저장

            textInfos[i].typeText.text = typeText; //UI에 표시될 텍스트

            string levelLabel = LocalizationManager.Instance.GetText(LocalizationKey.Order_Level);
            string lang = LocalizationManager.Instance.CurrentLanguage; //현재 언어상태 가져옴

            string levelText = lang == "ko" 
                ? $"{donut.level}{levelLabel}"
                : $"{levelLabel}-{donut.level}";
            //언어상태에따라 텍스트를 변경 ex)한글이면 1단계 영어면 Level-1

            textInfos[i].levelText.text = levelText;//UI에 표시될 텍스트
        }
    }


    //외부 접근용 함수 
    public int GetRefreshCount() => DataManager.Instance.QuestData.refreshCount; //- 외부에서 Scr_OrderSystem의 refreshCount 값을 읽을 수 있게 해주는 역할

    public int GetMaxRefreshCount() => DataManager.Instance.QuestData.maxCount;


    public void AddRefreshCount()
    {
        refreshCountText.text = $"{DataManager.Instance.QuestData.refreshCount}/{DataManager.Instance.QuestData.maxCount}";
    }

    private void SaveRefreshCount()
    {
        PlayerPrefs.SetInt(RefreshCountKey, DataManager.Instance.QuestData.refreshCount);
        PlayerPrefs.Save();
    }
    private void LoadRefreshCount()
    {
        if (PlayerPrefs.HasKey(RefreshCountKey))
        {
            DataManager.Instance.QuestData.refreshCount = PlayerPrefs.GetInt(RefreshCountKey);
        }
        else
        {
            DataManager.Instance.QuestData.refreshCount = DataManager.Instance.QuestData.maxCount;
        }
    }
    // HEX → Color 변환 함수
    private Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out var color))
            return color;
        return Color.white;
    }

    // Complete 버튼의 Image 색상을 옅은 붉은색으로 바꾸기
    private void SetCompleteImageRed(GameObject completeObject)
    {
        var img = completeObject.GetComponent<Image>();
        if (img != null)
        {
           // img.color = new Color(1f, 0.5f, 0.5f, 1f); // 옅은 붉은색
        }
    }

    // Complete 버튼의 Image 색상을 원래 색(#66E3AD)으로 복원하기
    private void ResetCompleteImageColor(GameObject completeObject)
    {
        var img = completeObject.GetComponent<Image>();
        if (img != null)
        {
            //img.color = HexToColor("#66E3AD"); // 원래 색상
        }
    }

    // 눌린 버튼을 원래 색(#66E3AD)보다 조금 더 짙게 표시
    private void SetPressedButtonColor(GameObject completeObject)
    {
        var img = completeObject.GetComponent<Image>();
        if (img != null)
        {
            // 원래 색보다 조금 더 짙은 톤 (예: #4CCB95)
            //img.color = HexToColor("#4CCB95");
        }
    }

}
