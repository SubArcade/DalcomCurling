
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

public class Scr_IAPInitializer : MonoBehaviour, IStoreListener
{
    public static Scr_IAPInitializer Instance;
    public IStoreController StoreController;
    public IExtensionProvider ExtensionProvider;

    private bool isInitializing = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);

        // 2) 이미 초기화됐거나, 초기화 중이면 또 안 들어가게
        if (StoreController != null || isInitializing)
            return;

        isInitializing = true;

        Debug.Log("[IAP] Initializing...");

        var module = StandardPurchasingModule.Instance(AppStore.GooglePlay);
        var builder = ConfigurationBuilder.Instance(module);

        var catalog = ProductCatalog.LoadDefaultCatalog();
        foreach (var p in catalog.allValidProducts)
        {
            //Debug.Log($"[IAP] Add product from catalog: {p.id}, type={p.type}");
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
        string productId = e.purchasedProduct.definition.id;
        Debug.Log($"[IAP] Purchase: {productId}");
        
        var allGems = FindObjectsOfType<Scr_IAPGem>();
        foreach (var gem in allGems)
        {
            if (gem.ProductId == productId)
            {
                gem.GrantReward();
                break;
            }
        }
        
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"[IAP] PurchaseFailed: {product.definition.id}, reason={failureReason}");
    }
}
