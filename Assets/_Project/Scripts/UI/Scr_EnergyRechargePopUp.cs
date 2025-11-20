using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scr_EnergyRechargePopUp : MonoBehaviour
{
    [Header("현재 에너지 반영할 이미지와 텍스트")]
    [SerializeField] private Image gage;
    [SerializeField] private TextMeshProUGUI energyCountText;

    [Header("보석 사용 버튼")]
    [SerializeField] private Button gemButton;

    [Header("광고 충전 버튼")]
    [SerializeField] private Button adsButton;
    [SerializeField] private TextMeshProUGUI adsText;

    [Header("닫기 버튼")]
    [SerializeField] private Button closeButton;

    void Awake()
    {
        gage = transform.Find("Panel/Background/EnergyBar/Gage")?.GetComponent<Image>();
        energyCountText = transform.Find("Panel/Background/EnergyBar/EnergyCount_Text")?.GetComponent<TextMeshProUGUI>();

        gemButton = transform.Find("Panel/GemRecharge/GemBtn")?.GetComponent<Button>();

        adsButton = transform.Find("Panel/AdsRecharge/AdsBtn")?.GetComponent<Button>();
        adsText = transform.Find("Panel/AdsRecharge/AdsBtn/Ads_Text")?.GetComponent<TextMeshProUGUI>();

        closeButton = transform.Find("Panel/CloseButton")?.GetComponent<Button>();
    }    
    void Start()
    {
        closeButton.onClick.AddListener(() => OnClickCloseBtn());
        gemButton.onClick.AddListener(() => OnClickGemBtn());
    }
    void Update()
{
    int currentEnergy = DataManager.Instance.PlayerData.energy;
    int maxEnergy = DataManager.Instance.PlayerData.maxEnergy;
    UpdateEnergyUI(currentEnergy, maxEnergy);
}

    void OnEnable()
    {
        DataManager.Instance.OnUserDataChanged += DataSetUI;
    }

    void OnDisable()
    {
        DataManager.Instance.OnUserDataChanged -= DataSetUI;
    }
    void DataSetUI(PlayerData data)
    {
        UpdateEnergyUI(data.energy, data.maxEnergy);
    }

    //닫기 버튼
    void OnClickCloseBtn() 
    {
        UIManager.Instance.Close(PanelId.EnergyRechargePopUp);
    }

    //에너지 텍스트와 이미지 연결
    public void UpdateEnergyUI(int currentEnergy, int maxEnergy)
    {
        // 텍스트 갱신
        energyCountText.text = $"{currentEnergy}/{maxEnergy}";

        // 이미지 fillAmount 갱신 (0.0 ~ 1.0)
        float ratio = Mathf.Clamp01((float)currentEnergy / maxEnergy);
        gage.fillAmount = ratio;
    }
 
    //보석버튼 에너지충전
    public void OnClickGemBtn()
    {
        int currentGem = DataManager.Instance.PlayerData.gem;
        int Energy = DataManager.Instance.PlayerData.energy;

        // 보석 부족 시 처리
        if (currentGem < 10)
        {
            UIManager.Instance.Open(PanelId.NotEnoughGemPopUp);
            return;
        }

        // 보석 차감 + 에너지 회복
        int newGem = currentGem - 10;
        int newEnergy = Energy + 50;
       
        DataManager.Instance.PlayerData.gem = newGem;
        DataManager.Instance.PlayerData.energy = newEnergy;
    }
   
    //광고버튼 에너지충전
    public void OnClickAdsBtn() 
    {
        //ㅋㅋ
    }
}
