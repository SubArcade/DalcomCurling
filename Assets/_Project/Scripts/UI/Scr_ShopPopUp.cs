using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scr_ShopPopUp : MonoBehaviour
{
    [Header("캐시상점 토글")]
    [SerializeField] private Toggle cashShopBtn;
    [SerializeField] private Outline cashShopOutline;

    [Header("골드상점 토글")]
    [SerializeField] private Toggle goldShopBtn;
    [SerializeField] private Outline goldShopOutline;

    [Header("로비로 돌아가기 버튼")]
    [SerializeField] private Button robbyButton;

    [Header("에너지 골드 보석")]
    [SerializeField] private Button energyBtn;
    [SerializeField] private Button goldBtn;
    [SerializeField] private Button gemBtn;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI gemText;

    [Header("광고제거 아이템&패키지 아이템")]
    [SerializeField] private Button deleteAdsBtn1;
    [SerializeField] private Button deleteAdsBtn2;
    [SerializeField] private Button deleteAdsBtn3;
    [SerializeField] private Button packageBtn1;
    [SerializeField] private Button packageBtn2;

    [Header("이펙트&캐릭터 아이템")]
    [SerializeField] private Button effectItem1;
    [SerializeField] private Button effectItem2;
    [SerializeField] private Button effectItem3;
    [SerializeField] private Button characterItem1;
    [SerializeField] private Button characterItem2;
    [SerializeField] private Button characterItem3;
    [SerializeField] private Button characterItem4;

    private readonly Color activeColor = new Color(1f, 1f, 0f, 1f); // 노란색
    private readonly Color inactiveColor = new Color32(0xAD, 0x2E, 0x78, 0xFF);
    private readonly Vector2 activeThickness = new Vector2(5f, 5f);
    private readonly Vector2 inactiveThickness = new Vector2(2f, 2f);
    //상점 토글의 아웃라인 관리용 변수들

    void Awake()
    {
 
    }
    void Start()
    {
        bool isCashOn = cashShopBtn.isOn;

        cashShopOutline.effectColor = isCashOn ? activeColor : inactiveColor;
        cashShopOutline.effectDistance = isCashOn ? activeThickness : inactiveThickness;

        goldShopOutline.effectColor = isCashOn ? inactiveColor : activeColor;
        goldShopOutline.effectDistance = isCashOn ? inactiveThickness : activeThickness;

        //캐시상점 열기
        cashShopBtn.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                goldShopBtn.isOn = false;
                ShowCashShopUI();

                cashShopOutline.effectColor = activeColor;
                cashShopOutline.effectDistance = activeThickness;

                goldShopOutline.effectColor = inactiveColor;
                goldShopOutline.effectDistance = inactiveThickness;
            }
        });
        //골드상점열기
        goldShopBtn.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                cashShopBtn.isOn = false;
                ShowGoldShopUI();

                goldShopOutline.effectColor = activeColor;
                goldShopOutline.effectDistance = activeThickness;

                cashShopOutline.effectColor = inactiveColor;
                cashShopOutline.effectDistance = inactiveThickness;
            }
        });
        //로비로 돌아가버리는 버튼
        robbyButton.onClick.AddListener(() =>
        {
            UIManager.Instance.Close(PanelId.ShopPopUp);
        });

        SetPlayerText(DataManager.Instance.PlayerData);
        energyBtn.onClick.AddListener(() => UIManager.Instance.Open(PanelId.EnergyRechargePopUp));
    }
    void OnEnable()
    {
        DataManager.Instance.OnUserDataChanged += HandleUserDataChanged;
        SetPlayerText(DataManager.Instance.PlayerData);
    }

    void OnDisable()
    {
        DataManager.Instance.OnUserDataChanged -= HandleUserDataChanged;
    }
    private void HandleUserDataChanged(PlayerData playerData)
    {
        SetPlayerText(playerData);
    }

    //캐시샵열기용
    private void ShowCashShopUI()
    {
        transform.Find("CashUIBackground").gameObject.SetActive(true);
        transform.Find("GoldUIBackground").gameObject.SetActive(false);
    }
    //골드샵열기용
    private void ShowGoldShopUI()
    {
        transform.Find("CashUIBackground").gameObject.SetActive(false);
        transform.Find("GoldUIBackground").gameObject.SetActive(true);
    }

    public void SetPlayerText(PlayerData playerData)
    {
        energyText.text = $"{playerData.energy}/{playerData.maxEnergy}";
        goldText.text = $"{playerData.gold}";
        gemText.text = $"{playerData.gem}";
    }

}
