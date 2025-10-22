using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : Singleton<IAPManager>
{
    // 가격 로드하는 메서드
    public string GetLocalizedPrice(string productId)
    {
        var listener = CodelessIAPStoreListener.Instance;
        if (listener == null)
        {
            Debug.Log($"{productId} : listener == null");
            return "error";
        }

        Product product = listener.GetProduct(productId);
        if (product != null && product.availableToPurchase)
        {
            Debug.Log($"{productId} : localizedPriceString == {product.metadata.localizedPriceString}");
            return product.metadata.localizedPriceString;
        }

        return "error";
    }

    // 상품명 로드하는 메서드
    public string GetLocalizedName(string productId)
    {
        Product product = CodelessIAPStoreListener.Instance.GetProduct(productId);
        return (product != null && product.availableToPurchase) ? product.metadata.localizedTitle : "";
    }

    // 구매 시 호출되는 메서드
    public void BuyProduct(string productId)
    {
        CodelessIAPStoreListener listener = CodelessIAPStoreListener.Instance;
        if (listener == null) return;

        Product product = listener.GetProduct(productId);
        if (product != null && product.availableToPurchase)
        {
            listener.InitiatePurchase(productId);
        }
        else
        {
            Debug.LogWarning($"IAP 상품 {productId} 구매 x");
        }
    }
}
