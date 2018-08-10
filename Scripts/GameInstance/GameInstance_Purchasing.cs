using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_PURCHASING && (UNITY_IOS || UNITY_ANDROID)
using UnityEngine.Purchasing;
#endif

namespace MultiplayerARPG
{
    public partial class GameInstance
    {
        // NOTE: something about product type
        // -- Consumable product is product such as gold, gem that can be consumed
        // -- Non-Consumable product is product such as special characters/items
        // that player will buy it to unlock ability to use and will not buy it later
        // -- Subscription product is product such as weekly/monthly promotion
        public const string TAG_INIT = "IAP_INIT";
        public const string TAG_PURCHASE = "IAP_PURCHASE";
        public const string TAG_RESTORE = "IAP_RESTORE";

#if UNITY_PURCHASING && (UNITY_IOS || UNITY_ANDROID)
        public static IStoreController StoreController { get; private set; }
        public static IExtensionProvider StoreExtensionProvider { get; private set; }
#endif

        public static System.Action<bool, string> PurchaseCallback;
        public static System.Action<bool, string> RestoreCallback;

        #region IStoreListener
#if UNITY_PURCHASING && (UNITY_IOS || UNITY_ANDROID)
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            // Purchasing has succeeded initializing. Collect our Purchasing references.
            // Overall Purchasing system, configured with products for this application.
            StoreController = controller;
            // Store specific subsystem, for accessing device-specific store features.
            StoreExtensionProvider = extensions;
            var productCount = StoreController.products.all.Length;
            var logMessage = "[" + TAG_INIT + "]: OnInitialized with " + productCount + " products";
            Debug.Log(logMessage);
        }
        
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
            var errorMessage = "[" + TAG_INIT + "]: Fail. InitializationFailureReason:" + error;
            Debug.LogError(errorMessage);
        }
        
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var productId = args.purchasedProduct.definition.id.GenerateHashId();

            CashPackage package;
            if (CashPackages.TryGetValue(productId, out package))
            {
                // Connect to server to precess purchasing
            }

            // Return a flag indicating whether this product has completely been received, or if the application needs 
            // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
            // saving purchased products to the cloud, and when that save is delayed.
            PurchaseResult(true);
            return PurchaseProcessingResult.Pending;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
            // this reason with the user to guide their troubleshooting actions.
            var errorMessage = "[" + TAG_PURCHASE + "]: FAIL. Product: " + product.definition.storeSpecificId + ", PurchaseFailureReason: " + failureReason;
            PurchaseResult(false, errorMessage);
        }
#endif
        #endregion

        #region Callback Events
        private static void PurchaseResult(bool success, string errorMessage = "")
        {
            if (!success)
                Debug.LogError(errorMessage);
            if (PurchaseCallback != null)
            {
                PurchaseCallback(success, errorMessage);
                PurchaseCallback = null;
            }
        }

        private static void RestoreResult(bool success, string errorMessage = "")
        {
            if (!success)
                Debug.LogError(errorMessage);
            if (RestoreCallback != null)
            {
                RestoreCallback(success, errorMessage);
                RestoreCallback = null;
            }
        }
        #endregion
    }
}
