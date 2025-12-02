 using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scr_ShopPopUp : MonoBehaviour
{
    [Header("캐시상점 토글")]
    [SerializeField] private Toggle cashShopBtn;
    [SerializeField] private Image cashShopImg;
    [SerializeField] private Outline cashShopOutline;

    [Header("골드상점 토글")]
    [SerializeField] private Toggle goldShopBtn;
    [SerializeField] private Image goldShopImg;
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
    
    [Header("이펙트&캐릭터 아이템")]
    [SerializeField] private Button effectItem1;
    [SerializeField] private Button effectItem2;
    [SerializeField] private Button effectItem3;
    [SerializeField] private Button characterItem1;
    [SerializeField] private Button characterItem2;
    
    [Header("젬 부족 팝업창")]
    [SerializeField] private GameObject enoughPopup;
    [SerializeField] private Button enoughCloseButton;

    [Header("구매한 이력이 있는 팝업창")]
    [SerializeField] private GameObject purchasedItemsPopup;
    [SerializeField] private Button purchasedCloseButton;
    
    [Header("구매 완료")]
    [SerializeField] private GameObject completePopup;
    [SerializeField] private Button completeCloseButton;
    
    private readonly Color activeColor = new Color(1f, 1f, 0f, 1f); // 노란색
    private readonly Color inactiveColor = new Color32(0xAD, 0x2E, 0x78, 0xFF);
    private readonly Vector2 activeThickness = new Vector2(5f, 5f);
    private readonly Vector2 inactiveThickness = new Vector2(2f, 2f);
    //상점 토글의 아웃라인 관리용 변수들

    // ✅ 이미지 색상 관리용
    private readonly Color selectedImgColor = new Color32(0xFE, 0xD3, 0x7E, 0xFF); // 선택시FED37E
    private readonly Color defaultImgColor = new Color32(0x2B, 0x3F, 0x5D, 0xFF);   // 선택이 아닐시2B3F5D
    
    void Start()
    {
        bool isCashOn = cashShopBtn.isOn;

        cashShopOutline.effectColor = isCashOn ? activeColor : inactiveColor;
        cashShopImg.color = isCashOn ? selectedImgColor : defaultImgColor;
        cashShopOutline.effectDistance = isCashOn ? activeThickness : inactiveThickness;

        goldShopOutline.effectColor = isCashOn ? inactiveColor : activeColor;
        goldShopImg.color = isCashOn ? defaultImgColor : selectedImgColor;
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

                cashShopImg.color = selectedImgColor;
                goldShopImg.color = defaultImgColor;
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

                goldShopImg.color = selectedImgColor;
                cashShopImg.color = defaultImgColor;

            }
        });

        //로비로 돌아가버리는 버튼
        robbyButton.onClick.AddListener(() =>
        {
            UIManager.Instance.Close(PanelId.ShopPopUp);
        });

        SetPlayerText(DataManager.Instance.PlayerData);
        energyBtn.onClick.AddListener(() => UIManager.Instance.Open(PanelId.EnergyRechargePopUp));
        
        // 팝업창 닫기
        enoughCloseButton.onClick.AddListener(() => enoughPopup.SetActive(false));
        purchasedCloseButton.onClick.AddListener(() => purchasedItemsPopup.SetActive(false));
        completeCloseButton.onClick.AddListener(() => completePopup.SetActive(false));
        
        // 구매 버튼 이벤트
        effectItem1.onClick.AddListener(() => EffectBuy(EffectType.Blue, 50));
        effectItem2.onClick.AddListener(() => EffectBuy(EffectType.Magic, 100));
        effectItem3.onClick.AddListener(() => EffectBuy(EffectType.Star, 100));
        
        characterItem1.onClick.AddListener(() => CharacterBuy(CharacterType.a, 500));
        characterItem2.onClick.AddListener(() => CharacterBuy(CharacterType.b, 500));
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

    private void EffectBuy(EffectType effectType, int amount)
    {
        if (DataManager.Instance.InventoryData.effectList.Contains(effectType))
        {
            // 리스트 안에 있을 경우
            purchasedItemsPopup.SetActive(true);
            return;
        }
        
        if (amount > DataManager.Instance.PlayerData.gem)
        {
            // 부족한 팝업창 노출
            enoughPopup.SetActive(true);
            return;
        }
        
        DataManager.Instance.InventoryData.effectList.Add(effectType);
        DataManager.Instance.PlayerData.gem -= amount;
        DataManager.Instance.GemChange(DataManager.Instance.PlayerData.gem);
        SetPlayerText(DataManager.Instance.PlayerData);
        completePopup.SetActive(true);
    }
    
    private void CharacterBuy(CharacterType characterType, int amount)
    {
        if (DataManager.Instance.InventoryData.characterList.Contains(characterType))
        {
            // 리스트 안에 있을 경우
            purchasedItemsPopup.SetActive(true);
            return;
        }
        
        if (amount > DataManager.Instance.PlayerData.gem)
        {
            // 부족한 팝업창 노출
            enoughPopup.SetActive(true);
            return;
        }

       
        DataManager.Instance.InventoryData.characterList.Add(characterType);
        DataManager.Instance.PlayerData.gem -= amount;
        DataManager.Instance.GemChange(DataManager.Instance.PlayerData.gem);
        SetPlayerText(DataManager.Instance.PlayerData);
        completePopup.SetActive(true);
    }
}
