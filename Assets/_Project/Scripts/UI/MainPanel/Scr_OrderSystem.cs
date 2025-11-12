using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scr_OrderSystem : MonoBehaviour
{
    //추가해야할 변수 : 도넛레벨마다 잡을 보상값 200~250
    //도넷 레벨, 도넛 이미지, 도넛 타입, 도넛 설명
    //
    [Header("1번 주문서")]
    [SerializeField] private GameObject orderGroup1; //주문서 그룹
    [SerializeField] private List<Image> orderDonuts1; //주문서 이미지
    [SerializeField] private Button refreshBtn1; //새로고침 버튼
    [SerializeField] private TextMeshProUGUI costText1; //주문서 도넛 합계가격텍스트
    [SerializeField] private Button orderClearBtn1; //완료 버튼(보상수령)
    [SerializeField] private GameObject completeObject1; //완료됨을 알려는 판떼기

    [Header("2번 주문서")]
    [SerializeField] private GameObject orderGroup2;
    [SerializeField] private List<Image> orderDonuts2;
    [SerializeField] private Button refreshBtn2;
    [SerializeField] private TextMeshProUGUI costText2;
    [SerializeField] private Button orderClearBtn2;
    [SerializeField] private GameObject completeObject2;

    [Header("3번 주문서")]
    [SerializeField] private GameObject orderGroup3;
    [SerializeField] private List<Image> orderDonuts3;
    [SerializeField] private Button refreshBtn3;
    [SerializeField] private TextMeshProUGUI costText3;
    [SerializeField] private Button orderClearBtn3;
    [SerializeField] private GameObject oompleteObject3;

    [Header("갱신된 새로고침 횟수 텍스트")]
    [SerializeField] private TextMeshProUGUI refreshCountText;


    private int refreshCount = 5; //새로고침 횟수
    private const int maxRefreshCount = 5; //최대 새로고침 횟수

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


    void Awake()
    {
        orderGroup1 = transform.Find("Top/QuestGroup/Order1/OrderGroup1")?.gameObject;
        orderGroup2 = transform.Find("Top/QuestGroup/Order2/OrderGroup2")?.gameObject;
        orderGroup3 = transform.Find("Top/QuestGroup/Order3/OrderGroup3")?.gameObject;

        orderDonuts1 = orderGroup1.GetComponentsInChildren<Image>().ToList();
        orderDonuts2 = orderGroup2.GetComponentsInChildren<Image>().ToList();
        orderDonuts3 = orderGroup3.GetComponentsInChildren<Image>().ToList();

        refreshBtn1 = transform.Find("Top/QuestGroup/Order1/RefreshButton")?.GetComponent<Button>();
        refreshBtn2 = transform.Find("Top/QuestGroup/Order2/RefreshButton")?.GetComponent<Button>();
        refreshBtn3 = transform.Find("Top/QuestGroup/Order3/RefreshButton")?.GetComponent<Button>();

        costText1 = transform.Find("Top/QuestGroup/Order1/OrderClearBtn/cost_Text")?.GetComponent<TextMeshProUGUI>();
        costText2 = transform.Find("Top/QuestGroup/Order2/OrderClearBtn/cost_Text")?.GetComponent<TextMeshProUGUI>();
        costText3 = transform.Find("Top/QuestGroup/Order3/OrderClearBtn/cost_Text")?.GetComponent<TextMeshProUGUI>();

        orderClearBtn1 = transform.Find("Top/QuestGroup/Order1/OrderClearBtn")?.GetComponent<Button>();
        orderClearBtn2 = transform.Find("Top/QuestGroup/Order2/OrderClearBtn")?.GetComponent<Button>();
        orderClearBtn3 = transform.Find("Top/QuestGroup/Order3/OrderClearBtn")?.GetComponent<Button>();

        refreshCountText = transform.Find("Top/ReroleGroup/RefreshCount/RefreshCount_text")?.GetComponent<TextMeshProUGUI>();
        refreshCountText.text = $"{refreshCount}/{maxRefreshCount}";

        completeObject1 = transform.Find("Top/QuestGroup/Order1/CompleteObject")?.gameObject;
        completeObject2 = transform.Find("Top/QuestGroup/Order2/CompleteObject")?.gameObject;
        oompleteObject3 = transform.Find("Top/QuestGroup/Order3/CompleteObject")?.gameObject;

        // orderdonuts1~3의 자식 텍스트들을 리스트에 저장
        donutTextInfos1 = FindLevelAndTypeText(orderDonuts1);
        donutTextInfos2 = FindLevelAndTypeText(orderDonuts2);
        donutTextInfos3 = FindLevelAndTypeText(orderDonuts3);

        //orderdonuts1~3의 자식 체크마크들을 리스트에 저장
        donutVisuals1 = FindCheckMark(orderDonuts1);
        donutVisuals2 = FindCheckMark(orderDonuts2);
        donutVisuals3 = FindCheckMark(orderDonuts3);

    }
    void Start()
    {     
        //새로고침버튼 연결
        refreshBtn1.onClick.AddListener(() => OnclickRefreshOrderButton(refreshBtn1));
        refreshBtn2.onClick.AddListener(() => OnclickRefreshOrderButton(refreshBtn2));
        refreshBtn3.onClick.AddListener(() => OnclickRefreshOrderButton(refreshBtn3));

        //주문서 완료 연결
        orderClearBtn1.onClick.AddListener(() => OnClickClearButton(completeObject1, orderDonutIDs1));
        orderClearBtn2.onClick.AddListener(() => OnClickClearButton(completeObject2, orderDonutIDs2));
        orderClearBtn3.onClick.AddListener(() => OnClickClearButton(oompleteObject3, orderDonutIDs3));

        //게임 시작 시 전체 주문서 자동 새로고침 (횟수 차감 없음)
        RefreshOrderDonut(orderDonuts1, donutTextInfos1, orderDonutIDs1,costText1);
        RefreshOrderDonut(orderDonuts2, donutTextInfos2, orderDonutIDs2,costText2);
        RefreshOrderDonut(orderDonuts3, donutTextInfos3, orderDonutIDs3,costText3);

        completeObject1.SetActive(false);
        completeObject2.SetActive(false);
        oompleteObject3.SetActive(false);
    }

    void Update()
    {
        IsOrderCompleted(orderDonutIDs1, donutVisuals1);
        IsOrderCompleted(orderDonutIDs2, donutVisuals2);
        IsOrderCompleted(orderDonutIDs3, donutVisuals3);

        FindCheckMarkBoard();
    }

    //새로고침 버튼 상호작용
    public void OnclickRefreshOrderButton(Button ClickedButton)
    {
        if (refreshCount <= 0)
        {
            print("남은 새로고침 횟수가 없습니다!");
            return;
        }

        refreshCount--;
        refreshCountText.text = $"{refreshCount}/{maxRefreshCount}";

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
            oompleteObject3.SetActive(false);
        }
    }

    //주문서 새로고침시 이미지, 이름.레벨,보상골드 갱신
    public void RefreshOrderDonut(List<Image> orderImages, List<DonutTextInfo> textInfos, List<string> idList, TextMeshProUGUI costText)
    {
        idList.Clear();
        int count = Mathf.Min(orderImages.Count, textInfos.Count, 3); //도넛을 최대 3개까지만 새로고침
        int totalReward = 0;

        for (int i = 0; i < count; i++)
        {
            //도넛 타입과 레벨을 랜덤으로 고름
            DonutType randomType = (DonutType)Random.Range(0, System.Enum.GetValues(typeof(DonutType)).Length);
            int randomLevel = Random.Range(1, 31);

            DonutData donut = DataManager.Instance.GetDonutData(randomType, randomLevel);
            
            //이미지, 타입, 단계 설명표시
            orderImages[i].sprite = donut.sprite;
            textInfos[i].typeText.text = donut.displayName;
            textInfos[i].levelText.text = donut.description;
            //저장
            idList.Add(donut.id);

            int reward = GetRewardByLevel(donut.level);
            totalReward += reward;
        }
        costText.text = $"{totalReward}";
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
    public void OnClickClearButton(GameObject completeObject, List<string> orderDonutIDs)
    {
        //해당하는 보상 수령 로직도 넣기      
        completeObject.SetActive(true);
    }

    //도넛레벨별 가격 책정하기
    private int GetRewardByLevel(int level)
    {
        if (level >= 1 && level <= 5)
            return Random.Range(300, 501);
        else if (level >= 6 && level <= 10)
            return Random.Range(600, 1001);
        else if (level >= 11 && level <= 15)
            return Random.Range(1100, 2001);
        else if (level >= 16 && level <= 20)
            return Random.Range(2100, 4001);
        else if (level >= 21 && level <= 30)
            return Random.Range(4100, 8001);
        else
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

    //머지보드판 체크마크 띄우기
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
            
            bool isInEntrySlot = item.transform.parent.GetComponent<EntrySlot>() != null;
            //엔트리슬롯이면 체크마크 끄기
            if (checkMark != null)
            { 
                checkMark.SetActive(allOrderIDs.Contains(id)&& !isInEntrySlot);
            }
        }
    }

    //사용하는 함수 아님니다 나중에 쓰던가 버리던가 할겁니댱
    public void SendQuest(List<int> rewardGolds, List<int> refreshCounts, List<string> donutIds)
    {
        for (int i = 0; i < 3; i++)
        {
            QuestList quest = new QuestList();
            quest.rewardGold = rewardGolds[i];
           
            quest.donutId = donutIds[i];

            DataManager.Instance.QuestData.questList.Add(quest);
        }
    }
}
