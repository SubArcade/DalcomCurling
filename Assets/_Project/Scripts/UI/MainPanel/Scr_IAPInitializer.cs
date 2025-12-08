using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

public class Scr_IAPInitializer : MonoBehaviour, IStoreListener
{
    public static IStoreController StoreController;
    public static IExtensionProvider ExtensionProvider;

    private void Awake()
    {
        if (StoreController != null)
            return;

        Debug.Log("[IAP] Initializing...");

        var module = StandardPurchasingModule.Instance(AppStore.GooglePlay);
        var builder = ConfigurationBuilder.Instance(module);

        // catalog.json 자동 등록
        var catalog = ProductCatalog.LoadDefaultCatalog();

        foreach (var p in catalog.allValidProducts)
        {
            Debug.Log($"[IAP] Add product from catalog: {p.id}, type={p.type}");
            
            builder.AddProduct(p.id, p.type);
        }

        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("[IAP] OnInitialized SUCCESS");
        StoreController = controller;
        ExtensionProvider = extensions;

        foreach (var prod in controller.products.all)
        {
            Debug.Log($"[IAP] Store product: {prod.definition.id}, available={prod.availableToPurchase}");
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"[IAP] Init Failed: {error}");
    }

#if UNITY_2020_3_OR_NEWER
    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"[IAP] Init Failed: {error}, msg={message}");
    }
#endif

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        Debug.Log($"[IAP] Purchase: {e.purchasedProduct.definition.id}");
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"[IAP] PurchaseFailed: {product.definition.id}, reason={failureReason}");
    }
}
