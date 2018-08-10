using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_PURCHASING && (UNITY_IOS || UNITY_ANDROID)
using UnityEngine.Purchasing;
#endif
using LiteNetLibManager;

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

        [Header("Purchasing")]
        [Tooltip("You can add cash packages / cash shop item here")]
        public CashShopDatabase cashShopDatabase;
        public static readonly Dictionary<int, CashShopItem> CashShopItems = new Dictionary<int, CashShopItem>();
        public static readonly Dictionary<int, CashPackage> CashPackages = new Dictionary<int, CashPackage>();

        private void InitializePurchasing()
        {
            CashShopItems.Clear();
            CashPackages.Clear();

            if (cashShopDatabase != null)
            {
                foreach (var cashShopItem in cashShopDatabase.cashStopItems)
                {
                    if (cashShopItem == null || CashShopItems.ContainsKey(cashShopItem.DataId))
                        continue;
                    CashShopItems[cashShopItem.DataId] = cashShopItem;
                }
            }

#if UNITY_PURCHASING && (UNITY_ANDROID || UNITY_IOS)
            // If we have already connected to Purchasing ...
            if (IsPurchasingInitialized())
                return;

            // Create a builder, first passing in a suite of Unity provided stores.
            var module = StandardPurchasingModule.Instance();
            var builder = ConfigurationBuilder.Instance(module);
#endif

            if (cashShopDatabase != null)
            {
                foreach (var cashPackage in cashShopDatabase.cashPackages)
                {
                    if (cashPackage == null || CashPackages.ContainsKey(cashPackage.DataId))
                        continue;

#if UNITY_PURCHASING && (UNITY_ANDROID || UNITY_IOS)
                    // Setup IAP package for clients
                    var productCatalogItem = cashPackage.ProductCatalogItem;
                    if (productCatalogItem == null)
                        continue;

                    var logMessage = "[" + TAG_INIT + "]: Adding product " + productCatalogItem.id + " type " + productCatalogItem.type.ToString();
                    Debug.Log(logMessage);
                    if (productCatalogItem.allStoreIDs.Count > 0)
                    {
                        var ids = new IDs();
                        foreach (var storeID in productCatalogItem.allStoreIDs)
                        {
                            ids.Add(storeID.id, storeID.store);
                        }
                        builder.AddProduct(productCatalogItem.id, productCatalogItem.type, ids);
                    }
                    else
                    {
                        builder.AddProduct(productCatalogItem.id, productCatalogItem.type);
                    }
#endif
                    CashPackages[cashPackage.DataId] = cashPackage;
                }
            }

#if UNITY_PURCHASING && (UNITY_ANDROID || UNITY_IOS)
            // Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
            // and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
            try
            {
                UnityPurchasing.Initialize(this, builder);
            }
            catch (System.InvalidOperationException ex)
            {
                var errorMessage = "[" + TAG_INIT + "]: Cannot initialize purchasing, the platform may not supports.";
                Debug.LogError(errorMessage);
                Debug.LogException(ex);
            }
#endif
        }

        public static bool IsPurchasingInitialized()
        {
#if UNITY_PURCHASING && (UNITY_IOS || UNITY_ANDROID)
            // Only say we are initialized if both the Purchasing references are set.
            return StoreController != null && StoreExtensionProvider != null;
#else
        return false;
#endif
        }
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
            var dataId = args.purchasedProduct.definition.id.GenerateHashId();

            CashPackage package;
            if (CashPackages.TryGetValue(dataId, out package))
            {
                // Connect to server to precess purchasing
                var mapNetworkManager = FindObjectOfType<BaseGameNetworkManager>();
                mapNetworkManager.RequestCashPackageBuyValidation(dataId, ResponseCashPackageBuyValidation);
            }
            else
                PurchaseResult(false, "Package not found");

            // Return a flag indicating whether this product has completely been received, or if the application needs 
            // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
            // saving purchased products to the cloud, and when that save is delayed.
            return PurchaseProcessingResult.Pending;
        }

        private void ResponseCashPackageBuyValidation(AckResponseCode responseCode, BaseAckMessage message)
        {
            var castedMessage = (ResponseCashPackageBuyValidationMessage)message;
            switch (responseCode)
            {
                case AckResponseCode.Error:
                    var errorMessage = string.Empty;
                    switch (castedMessage.error)
                    {
                        case ResponseCashPackageBuyValidationMessage.Error.UserNotFound:
                            errorMessage = "User not found";
                            break;
                        case ResponseCashPackageBuyValidationMessage.Error.CharacterNotFound:
                            errorMessage = "Character not found";
                            break;
                        case ResponseCashPackageBuyValidationMessage.Error.PackageNotFound:
                            errorMessage = "Package not found";
                            break;
                        case ResponseCashPackageBuyValidationMessage.Error.Invalid:
                            errorMessage = "Invalid";
                            break;
                    }
                    PurchaseResult(false, errorMessage);
                    break;
                case AckResponseCode.Timeout:
                    UISceneGlobal.Singleton.ShowMessageDialog("Error", "Connection timeout");
                    PurchaseResult(false, "Connection timeout");
                    break;
                default:
                    CashPackage package;
                    if (CashPackages.TryGetValue(castedMessage.dataId, out package))
                    {
                        StoreController.ConfirmPendingPurchase(package.ProductData);
                        PurchaseResult(true);
                    }
                    else
                        PurchaseResult(false, "Package not found");
                    break;
            }
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
