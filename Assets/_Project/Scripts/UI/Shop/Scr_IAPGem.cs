using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

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
    
    public string ProductId => productGemType.ToString();
    
    public void TryPurchase()
    {
        Debug.Log($"[IAP] 구매 시도! product = {productGemType}");
        AnalyticsManager.Instance.ShopPurchaseTry();
        
        // if (!GameManager.Instance.isEasterEgg)
        // {
        //     Debug.Log("[IAP] 구매 조건 미충족");
        //     return;
        // }
        
        if (GameManager.Instance.isEasterEgg)
        {
            if (Scr_IAPInitializer.Instance.StoreController == null)
            {
                Debug.LogError("[IAP] StoreController 없음. 초기화 안 됨.");
                return;
            }

            string productId = productGemType.ToString(); // enum 이름이 상품 ID와 동일하다고 가정
            var product = Scr_IAPInitializer.Instance.StoreController.products.WithID(productId);
            if (product == null || !product.availableToPurchase)
            {
                Debug.LogError($"[IAP] 구매 불가 상태: {productId}");
                return;
            }

            Debug.Log($"[IAP] 실제 결제 시작: {productId}");
            Scr_IAPInitializer.Instance.StoreController.InitiatePurchase(product);
        }
        else
        {
            Debug.Log("[IAP] 테스트/이벤트 모드 – 결제 없이 보상만 지급");
            GrantReward();
        }
    }
    
    // 성공
    // public void OnPurchaseSuccess(Order order)
    // {
    //     Debug.Log($"[IAP] OnPurchaseSuccess 호출 from {name}, instanceID={GetInstanceID()}");
    //     var cart = order.CartOrdered;
    //     var items = cart.Items();
    //     if (items == null || items.Count == 0)
    //     {
    //         Debug.LogError("[IAP] 구매 아이템 없음");
    //         return;
    //     }
    //
    //     string purchasedId = items[0].Product.definition.id;
    //     Debug.Log($"[IAP] OnPurchaseSuccess from {name} (instanceID={GetInstanceID()}), product={purchasedId}");
    //     if (purchasedId == productGemType.ToString())
    //     {
    //         Debug.Log($"[IAP] 구매 성공! productId = {purchasedId}, 지급 젬 = {rewardGem}");
    //         DataManager.Instance.GemChange(DataManager.Instance.PlayerData.gem += rewardGem);
    //         // 애널리틱스 구매 성공
    //         AnalyticsManager.Instance.ShopPurchaseResult(true);
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"[IAP] 예상된 enum({productGemType})과 실제 상품({purchasedId})가 다름");
    //     }
    // }
    
    // 실패
    public void OnPurchaseFailed()
    {
        Debug.LogError("[IAP] 구매 실패!");
        // 애널리틱스 구매 실패
        AnalyticsManager.Instance.ShopPurchaseResult(false);
    }
    
    public void GrantReward()
    {
        Debug.Log($"[IAP] 보상 지급! productId = {ProductId}, gem = {rewardGem}");
        DataManager.Instance.GemChange(DataManager.Instance.PlayerData.gem + rewardGem);
        AnalyticsManager.Instance.ShopPurchaseResult(true);
    }
}
