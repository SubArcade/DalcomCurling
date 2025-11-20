using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scr_OrderRefresh : MonoBehaviour
{
    [Header("닫기버튼")]
    [SerializeField] private Button closeButton;

    [Header("충전버튼")]
    [SerializeField] private Button Btn;

    [Header("충전버튼의 텍스트")]
    [SerializeField] private TextMeshProUGUI adsText;
    [Header("카운트횟수 텍스트")]
    [SerializeField] private TextMeshProUGUI countText;


    private int maxChargeCount = 3;  // 최대 충전 가능 횟수
    private int currentChargeCount = 0; // 현재까지 충전한 횟수
    private Scr_OrderSystem orderSystem; //새로고침 참조를 위한 변수

    void Awake()
    {
        closeButton = transform.Find("Panel/CloseButton")?.GetComponent<Button>();
        Btn = transform.Find("Panel/Btn")?.GetComponent<Button>();
        adsText = transform.Find("Panel/Btn/Ads_Text")?.GetComponent<TextMeshProUGUI>();
        countText = transform.Find("Panel/Background/RefreshImage/Count_Text")?.GetComponent<TextMeshProUGUI>();
       
        var mainPanel = GameObject.Find("Canvas/GameObject/Main_Panel");
        if (mainPanel != null)
        {
            orderSystem = mainPanel.GetComponent<Scr_OrderSystem>();
        }

    }
    void Start()
    {
        closeButton.onClick.AddListener(() => UIManager.Instance.Close(PanelId.OrderRefreshPopUp));
        Btn.onClick.AddListener(OnClickChargeRefresh);

        UpdateUI();
    }

    void OnEnable()
    {
        UpdateUI();
    }

    //누르면 횟수 아무조건없이 차도록 해놨어요 광고보면 차는걸로 하시면될거같아용
    private void OnClickChargeRefresh()
    {
        if (currentChargeCount >= maxChargeCount) return;

        // 1번 주문서 기준으로 5회 충전
        orderSystem.AddRefreshCount(5);

        currentChargeCount++;
        UpdateUI();

        if (currentChargeCount >= maxChargeCount)
            Btn.interactable = false;
    }

    //버튼 텍스트에 횟수반영
    private void UpdateUI()
    {
        countText.text = $"{orderSystem.GetRefreshCount()}/{orderSystem.GetMaxRefreshCount()}";
        adsText.text = $"충전하기 ({maxChargeCount - currentChargeCount}/{maxChargeCount})";
    }

}
