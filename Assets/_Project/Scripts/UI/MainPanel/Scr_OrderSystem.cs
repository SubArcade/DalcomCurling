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
    [SerializeField] private GameObject OrderGroup1; //주문서 그룹
    [SerializeField] private List<Image> OrderDonuts1; //주문서 이미지
    [SerializeField] private Button RefreshBtn1; //새로고침 버튼
    [SerializeField] private Button OrderClearBtn1; //완료 버튼(보상수령)
    [SerializeField] private GameObject CompleteObject1; //완료됨을 알려는 판떼기

    [Header("2번 주문서")]
    [SerializeField] private GameObject OrderGroup2;
    [SerializeField] private List<Image> OrderDonuts2;
    [SerializeField] private Button RefreshBtn2;
    [SerializeField] private Button OrderClearBtn2;
    [SerializeField] private GameObject CompleteObject2;

    [Header("3번 주문서")]
    [SerializeField] private GameObject OrderGroup3;
    [SerializeField] private List<Image> OrderDonuts3;
    [SerializeField] private Button RefreshBtn3;
    [SerializeField] private Button OrderClearBtn3;
    [SerializeField] private GameObject CompleteObject3;

    [Header("갱신된 새로고침 횟수 텍스트")]
    [SerializeField] private TextMeshProUGUI RefreshCountText;


    private int RefreshCount = 5; //새로고침 횟수
    private const int MaxRefreshCount = 5; //최대 새로고침 횟수

    // 도넛이미지와 같이 갱신될 도넛타입TextMeshPro, 도넛단계 TextMeshPro
    public class DonutTextInfo
    {
        public TextMeshProUGUI LevelText;
        public TextMeshProUGUI TypeText;
    }
    //주문서별로 도넛 텍스트 정보를 담을 리스트
    private List<DonutTextInfo> DonutTextInfos1 = new List<DonutTextInfo>();
    private List<DonutTextInfo> DonutTextInfos2 = new List<DonutTextInfo>();
    private List<DonutTextInfo> DonutTextInfos3 = new List<DonutTextInfo>();
    
    // 주문서별 도넛 ID 저장 리스트 추가
    private List<string> OrderDonutIDs1 = new List<string>();
    private List<string> OrderDonutIDs2 = new List<string>();
    private List<string> OrderDonutIDs3 = new List<string>();

    void Awake()
    {

        OrderGroup1 = transform.Find("Top/QuestGroup/Order1/OrderGroup1")?.gameObject;
        OrderGroup2 = transform.Find("Top/QuestGroup/Order2/OrderGroup2")?.gameObject;
        OrderGroup3 = transform.Find("Top/QuestGroup/Order3/OrderGroup3")?.gameObject;

        OrderDonuts1 = OrderGroup1.GetComponentsInChildren<Image>().ToList();
        OrderDonuts2 = OrderGroup2.GetComponentsInChildren<Image>().ToList();
        OrderDonuts3 = OrderGroup3.GetComponentsInChildren<Image>().ToList();

        RefreshBtn1 = transform.Find("Top/QuestGroup/Order1/RefreshButton")?.GetComponent<Button>();
        RefreshBtn2 = transform.Find("Top/QuestGroup/Order2/RefreshButton")?.GetComponent<Button>();
        RefreshBtn3 = transform.Find("Top/QuestGroup/Order3/RefreshButton")?.GetComponent<Button>();

        OrderClearBtn1 = transform.Find("Top/QuestGroup/Order1/OrderClearBtn")?.GetComponent<Button>();
        OrderClearBtn2 = transform.Find("Top/QuestGroup/Order2/OrderClearBtn")?.GetComponent<Button>();
        OrderClearBtn3 = transform.Find("Top/QuestGroup/Order3/OrderClearBtn")?.GetComponent<Button>();

        RefreshCountText = transform.Find("Top/ReroleGroup/RefreshCount/RefreshCount_text")?.GetComponent<TextMeshProUGUI>();
        RefreshCountText.text = $"{RefreshCount}/{MaxRefreshCount}";

        CompleteObject1 = transform.Find("Top/QuestGroup/Order1/CompleteObject")?.gameObject;
        CompleteObject2 = transform.Find("Top/QuestGroup/Order2/CompleteObject")?.gameObject;
        CompleteObject3 = transform.Find("Top/QuestGroup/Order3/CompleteObject")?.gameObject;

        // orderdonuts1~3의 자식 텍스트들을 리스트에 저장
        DonutTextInfos1 = ExtractTextInfos(OrderDonuts1);
        DonutTextInfos2 = ExtractTextInfos(OrderDonuts2);
        DonutTextInfos3 = ExtractTextInfos(OrderDonuts3);

    }
    void Start()
    {     
        //새로고침버튼 연결
        RefreshBtn1.onClick.AddListener(() => OnclickRefreshOrderButton(RefreshBtn1));
        RefreshBtn2.onClick.AddListener(() => OnclickRefreshOrderButton(RefreshBtn2));
        RefreshBtn3.onClick.AddListener(() => OnclickRefreshOrderButton(RefreshBtn3));

        //주문서 완료 연결
        OrderClearBtn1.onClick.AddListener(() => OnClickClearButton(CompleteObject1, OrderDonutIDs1));
        OrderClearBtn2.onClick.AddListener(() => OnClickClearButton(CompleteObject2, OrderDonutIDs2));
        OrderClearBtn3.onClick.AddListener(() => OnClickClearButton(CompleteObject3, OrderDonutIDs3));

    }

    //새로고침 버튼 상호작용
    //새로고침 버튼 상호작용
    public void OnclickRefreshOrderButton(Button ClickedButton)
    {
        if (RefreshCount <= 0)
        {
            print("남은 새로고침 횟수가 없습니다!");
            return;
        }

        RefreshCount--;
        RefreshCountText.text = $"{RefreshCount}/{MaxRefreshCount}";

        // 해당 주문서의 CompleteObject 비활성화
        if (ClickedButton == RefreshBtn1)
        {
            RefreshOrderDonut(OrderDonuts1, DonutTextInfos1, OrderDonutIDs1);
            CompleteObject1.SetActive(false);
        }
        else if (ClickedButton == RefreshBtn2)
        {
            RefreshOrderDonut(OrderDonuts2, DonutTextInfos2, OrderDonutIDs2);

            CompleteObject2.SetActive(false);
        }
        else if (ClickedButton == RefreshBtn3)
        {
            RefreshOrderDonut(OrderDonuts3, DonutTextInfos3, OrderDonutIDs3);
            CompleteObject3.SetActive(false);
        }
    }

    //orderdonuts1~3에서 자식텍스트를 찾아주는 함수
    private List<DonutTextInfo> ExtractTextInfos(List<Image> donutImages)
    {
        var result = new List<DonutTextInfo>();

        foreach (var img in donutImages)
        {
            var info = new DonutTextInfo
            {
                TypeText = img.transform.Find("Type_Text")?.GetComponent<TextMeshProUGUI>(),
                LevelText = img.transform.Find("Level_Text")?.GetComponent<TextMeshProUGUI>()
            };

            result.Add(info);
        }

        return result;
    }

    //주문서 새로고침시 이미지, 이름.레벨 갱신
    public void RefreshOrderDonut(List<Image> orderImages, List<DonutTextInfo> textInfos, List<string> idList)
    {
        idList.Clear();
        int count = Mathf.Min(orderImages.Count, textInfos.Count, 3);

        for (int i = 0; i < count; i++)
        {
            DonutType randomType = (DonutType)Random.Range(0, System.Enum.GetValues(typeof(DonutType)).Length);
            int randomLevel = Random.Range(1, 31);

            DonutData donut = DataManager.Instance.GetDonutData(randomType, randomLevel);
            
            orderImages[i].sprite = donut.sprite;
            textInfos[i].TypeText.text = donut.displayName;
            textInfos[i].LevelText.text = donut.description;

            idList.Add(donut.id);
        }
    }
    //머지보드판에서 도넛 검색후 주문서 클리어 여부 결정
    public bool IsOrderCompleted(List<string> orderDonutIDs)
    {
        var boardCells = BoardManager.Instance.GetAllCells();

        HashSet<string> boardDonutIDs = new();
        foreach (var cell in boardCells)
        {
            if (!cell.isActive || string.IsNullOrEmpty(cell.donutID)) continue;
            boardDonutIDs.Add(cell.donutID);
        }

        foreach (var id in orderDonutIDs)
        {
            if (!boardDonutIDs.Contains(id))
                return false;
        }
        return true;
    }
    //주문서 완료시 버튼 상호작용
    public void OnClickClearButton(GameObject completeObject, List<string> orderDonutIDs)
    {
        //해당하는 보상 수령 로직도 넣기
        //ex)단단1단계 200~250 + 말랑2단계 225~275 + 촉촉 3단계 250~275 = 보상골드
        //데이터도 보내줘야하나
        completeObject.SetActive(true);
    }
}
