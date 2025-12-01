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
    
    
    public void OnPurchaseSuccess(Order order)
    {
        var cart = order.CartOrdered;
        var items = cart.Items();
        if (items == null || items.Count == 0)
        {
            Debug.LogError("[IAP] 구매 아이템 없음");
            return;
        }

        string purchasedId = items[0].Product.definition.id;
        
        if (purchasedId == productGemType.ToString())
        {
            Debug.Log($"[IAP] 구매 성공! productId = {purchasedId}, 지급 젬 = {rewardGem}");
            DataManager.Instance.GemChange(DataManager.Instance.PlayerData.gem += rewardGem);
        }
        else
        {
            Debug.LogWarning($"[IAP] 예상된 enum({productGemType})과 실제 상품({purchasedId})가 다름");
        }
    }
}
