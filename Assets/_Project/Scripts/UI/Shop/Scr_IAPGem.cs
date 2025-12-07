using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public enum ProductGemType
{
    gem20,
    gem110,
    gem350,
    gem800,
    gem1500,
    gem4000,
}

public class Scr_IAPGem : MonoBehaviour
{
    [SerializeField] private ProductGemType productGemType;
    [SerializeField] private int rewardGem;
    [SerializeField] private IAPButton iapButton;
    [SerializeField] private Button button;
    
    void Awake()
    {
        button.onClick.AddListener(OnClickHandler);
    }
    
    private void OnClickHandler()
    {
        // 여기에 조건 넣으면 됨
        // if (!GameManager.Instance.isEasterEgg)
        // {
        //     Debug.Log("[IAP] IAP 비활성 → 바로 보상 지급");
        //     DataManager.Instance.GemChange(DataManager.Instance.PlayerData.gem += rewardGem);
        //     return;
        // }
        // else
        // {
        //     Debug.Log("[IAP] IAP 활성 → 실제 결제 진행");
        //     iapButton.purch
        // }
    }
    
    
    // 구매 시도
    public void TryPurchase()
    {
        Debug.Log($"[IAP] 구매 시도! product = {productGemType}");
        // 애널리틱스 구매 시도
        AnalyticsManager.Instance.ShopPurchaseTry();
    }
    
    // 성공
    public void OnPurchaseSuccess(Order order)
    {
        Debug.Log($"[IAP] OnPurchaseSuccess 호출 from {name}, instanceID={GetInstanceID()}");
        var cart = order.CartOrdered;
        var items = cart.Items();
        if (items == null || items.Count == 0)
        {
            Debug.LogError("[IAP] 구매 아이템 없음");
            return;
        }

        string purchasedId = items[0].Product.definition.id;
        Debug.Log($"[IAP] OnPurchaseSuccess from {name} (instanceID={GetInstanceID()}), product={purchasedId}");
        if (purchasedId == productGemType.ToString())
        {
            Debug.Log($"[IAP] 구매 성공! productId = {purchasedId}, 지급 젬 = {rewardGem}");
            DataManager.Instance.GemChange(DataManager.Instance.PlayerData.gem += rewardGem);
            // 애널리틱스 구매 성공
            AnalyticsManager.Instance.ShopPurchaseResult(true);
        }
        else
        {
            Debug.LogWarning($"[IAP] 예상된 enum({productGemType})과 실제 상품({purchasedId})가 다름");
        }
    }
    
    // 실패
    public void OnPurchaseFailed()
    {
        Debug.LogError("[IAP] 구매 실패!");
        // 애널리틱스 구매 실패
        AnalyticsManager.Instance.ShopPurchaseResult(false);
    }
}
