using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;
#if ENABLE_PURCHASING && (UNITY_IOS || UNITY_ANDROID)
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

#if ENABLE_PURCHASING && (UNITY_IOS || UNITY_ANDROID)
        public static StoreController StoreController { get; private set; }
        public static CatalogProvider CatalogProvider { get; private set; }
#endif

        public static System.Action<bool, string> PurchaseCallback;
        public static System.Action<bool, string> RestoreCallback;

        [Header("Purchasing")]
        [Tooltip("You can add cash packages / cash shop item here")]
        public CashShopDatabase cashShopDatabase;
        public static readonly Dictionary<int, CashShopItem> CashShopItems = new Dictionary<int, CashShopItem>();
        public static readonly Dictionary<int, CashPackage> CashPackages = new Dictionary<int, CashPackage>();
#if ENABLE_PURCHASING && (UNITY_IOS || UNITY_ANDROID)
        public static readonly Dictionary<string, PendingOrder> PendingOrders = new Dictionary<string, PendingOrder>();
        public static List<ProductDefinition> ProductDefinitions { get; private set; } = null;
        public static List<Product> FetchedProducts { get; private set; } = null;
        public static Orders FetchedOrders { get; private set; } = null;
#endif
        private bool _productFetched = false;
        private bool _purchaseFetched = false;


        private async void InitializePurchasing()
        {
            CashShopItems.Clear();
            CashPackages.Clear();

            if (cashShopDatabase != null)
            {
                foreach (CashShopItem cashShopItem in cashShopDatabase.cashShopItems)
                {
                    if (cashShopItem == null || CashShopItems.ContainsKey(cashShopItem.DataId))
                        continue;
                    CashShopItems[cashShopItem.DataId] = cashShopItem;
                }
            }
            // Generate and add cash shop items by items data
            foreach (BaseItem item in Items.Values)
            {
                item.GenerateCashShopItems();
            }

#if ENABLE_PURCHASING && (UNITY_ANDROID || UNITY_IOS)
            StoreController = new StoreController();
            CatalogProvider = new CatalogProvider();
#endif

            if (cashShopDatabase != null)
            {
                foreach (CashPackage cashPackage in cashShopDatabase.cashPackages)
                {
                    if (cashPackage == null || CashPackages.ContainsKey(cashPackage.DataId))
                        continue;

#if ENABLE_PURCHASING && (UNITY_ANDROID || UNITY_IOS)
                    // Setup IAP package for clients
                    var productCatalogItem = cashPackage.ProductCatalogItem;
                    if (productCatalogItem == null)
                        continue;

                    Logging.Log(LogTag, "[" + TAG_INIT + "]: Adding product " + productCatalogItem.id + " type " + productCatalogItem.type.ToString());
                    if (productCatalogItem.allStoreIDs.Count > 0)
                    {
                        var ids = new StoreSpecificIds();
                        foreach (var storeID in productCatalogItem.allStoreIDs)
                        {
                            ids.Add(storeID.id, storeID.store);
                        }
                        CatalogProvider.AddProduct(productCatalogItem.id, productCatalogItem.type, ids);
                    }
                    else
                    {
                        CatalogProvider.AddProduct(productCatalogItem.id, productCatalogItem.type);
                    }
#endif
                    CashPackages[cashPackage.DataId] = cashPackage;
                }
            }

#if ENABLE_PURCHASING && (UNITY_ANDROID || UNITY_IOS)
            Logging.Log(LogTag, "[" + TAG_INIT + "]: Initializing Purchasing...");
            try
            {
                CatalogProvider.FetchProducts(list => ProductDefinitions = list);
                // Prepare store events
                StoreController.OnStoreConnected += StoreController_OnStoreConnected;
                StoreController.OnStoreDisconnected += StoreController_OnStoreDisconnected;

                StoreController.OnProductsFetched += StoreController_OnProductsFetched;
                StoreController.OnProductsFetchFailed += StoreController_OnProductsFetchFailed;

                StoreController.OnPurchasesFetched += StoreController_OnPurchasesFetched;
                StoreController.OnPurchasesFetchFailed += StoreController_OnPurchasesFetchFailed;

                StoreController.OnPurchasePending += StoreController_OnPurchasePending;
                await StoreController.Connect();
            }
            catch (System.InvalidOperationException ex)
            {
                var errorMessage = "[" + TAG_INIT + "]: Cannot initialize purchasing, the platform may not supports.";
                Logging.LogError(LogTag, errorMessage);
                Logging.LogException(LogTag, ex);
            }
#else
            Logging.Log(LogTag, "[" + TAG_INIT + "]: Initialized without purchasing");
#endif
        }

        public static bool IsPurchasingInitialized()
        {
#if ENABLE_PURCHASING && (UNITY_IOS || UNITY_ANDROID)
            // Only say we are initialized if both the Purchasing references are set.
            return StoreController != null && CatalogProvider != null && _productFetched && _purchaseFetched;
#else
            return false;
#endif
        }

        #region IStoreListener
#if ENABLE_PURCHASING && (UNITY_IOS || UNITY_ANDROID)
        private void StoreController_OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
        {
            var errorMessage = "[" + TAG_INIT + "]: OnPurchasesFetchFailed: " + failure.FailureReason + ", " + failure.Message;
            Logging.LogError(LogTag, errorMessage);
            UISceneGlobal.Singleton.ShowMessageDialog("IAP Purchases Fetch Failed", failure.Message);
        }

        private void StoreController_OnPurchasesFetched(Orders orders)
        {
            _purchaseFetched = true;
            FetchedOrders = orders;
        }

        private void StoreController_OnProductsFetchFailed(ProductFetchFailed failure)
        {
            var errorMessage = "[" + TAG_INIT + "]: OnProductsFetchFailed: " + failure.FailureReason + ", " + failure.FailureReason;
            Logging.LogError(LogTag, errorMessage);
            UISceneGlobal.Singleton.ShowMessageDialog("IAP Products Fetch Failed", failure.FailureReason);
        }

        private void StoreController_OnProductsFetched(List<Product> products)
        {
            _productFetched = true;
            FetchedProducts = products;
            StoreController.FetchPurchases();
        }

        private void StoreController_OnStoreConnected()
        {
            _productFetched = false;
            _purchaseFetched = false;
            StoreController.FetchProducts(ProductDefinitions);
        }

        private void StoreController_OnStoreDisconnected(StoreConnectionFailureDescription desc)
        {
            var errorMessage = "[" + TAG_INIT + "]: OnStoreDisconnected: " + desc.Message;
            Logging.LogError(LogTag, errorMessage);
            bool isRetryable = desc.IsRetryable;
            UISceneGlobal.Singleton.ShowMessageDialog("IAP Store Disconnected", desc.Message, onClickYes: async () => {
                if (isRetryable)
                    await StoreController.Connect();
            });
        }

        private async void StoreController_OnPurchasePending(PendingOrder pendingOrder)
        {
            PendingOrders[pendingOrder.Info.TransactionID] = pendingOrder;
            List<CashPackageItemInfo> items = new List<CashPackageItemInfo>();
            foreach (var item in pendingOrder.CartOrdered.Items())
            {
                items.Add(new CashPackageItemInfo()
                {
                    dataId = item.Product.definition.id.GenerateHashId(),
                    quantity = item.Quantity,
                });
            }
            string appleJwsRepresentation = string.Empty;
            if (pendingOrder.Info.Apple != null)
            {
                appleJwsRepresentation = pendingOrder.Info.Apple.jwsRepresentation;
            }
            ClientCashShopHandlers.RequestCashPackageBuyValidation(new RequestCashPackageBuyValidationMessage()
            {
                items = items,
                platform = Application.platform,
                transactionID = pendingOrder.Info.TransactionID,
                receipt = pendingOrder.Info.Receipt,
                appleJwsRepresentation = appleJwsRepresentation,
            }, ResponseCashPackageBuyValidation);
        }

        private void ResponseCashPackageBuyValidation(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseCashPackageBuyValidationMessage response)
        {
            ClientCashShopActions.ResponseCashPackageBuyValidation(requestHandler, responseCode, response);
            if (responseCode == AckResponseCode.Unimplemented)
            {
                PurchaseResult(false, LanguageManager.GetText(UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE.ToString()));
                return;
            }
            if (responseCode == AckResponseCode.Timeout)
            {
                PurchaseResult(false, LanguageManager.GetText(UITextKeys.UI_ERROR_CONNECTION_TIMEOUT.ToString()));
                return;
            }
            switch (responseCode)
            {
                case AckResponseCode.Error:
                    PurchaseResult(false, LanguageManager.GetText(response.message.ToString()));
                    break;
                default:
                    if (PendingOrders.TryGetValue(response.transactionID, out PendingOrder pendingOrder))
                    {
                        StoreController.ConfirmPurchase(pendingOrder);
                        PurchaseResult(true);
                    }
                    else
                    {
                        PurchaseResult(false, LanguageManager.GetText(UITextKeys.UI_ERROR_CASH_PACKAGE_NOT_FOUND.ToString()));
                    }
                    break;
            }
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
            // this reason with the user to guide their troubleshooting actions.
            Logging.LogError(LogTag, "[" + TAG_PURCHASE + "]: FAIL. Product: " + product.definition.storeSpecificId + ", PurchaseFailureReason: " + failureReason);
            string errorMessage = string.Empty;
            switch (failureReason)
            {
                case PurchaseFailureReason.PurchasingUnavailable:
                    errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_IAP_PURCHASING_UNAVAILABLE.ToString());
                    break;
                case PurchaseFailureReason.ExistingPurchasePending:
                    errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_IAP_EXISTING_PURCHASE_PENDING.ToString());
                    break;
                case PurchaseFailureReason.ProductUnavailable:
                    errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_IAP_PRODUCT_UNAVAILABLE.ToString());
                    break;
                case PurchaseFailureReason.SignatureInvalid:
                    errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_IAP_SIGNATURE_INVALID.ToString());
                    break;
                case PurchaseFailureReason.UserCancelled:
                    errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_IAP_USER_CANCELLED.ToString());
                    break;
                case PurchaseFailureReason.PaymentDeclined:
                    errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_IAP_PAYMENT_DECLINED.ToString());
                    break;
                case PurchaseFailureReason.DuplicateTransaction:
                    errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_IAP_DUPLICATE_TRANSACTION.ToString());
                    break;
                default:
                    errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_IAP_UNKNOW.ToString());
                    break;
            }
            PurchaseResult(false, errorMessage);
        }
#endif
		#endregion

        #region IAP Actions
        public void Purchase(string productId)
        {
#if ENABLE_PURCHASING && (UNITY_IOS || UNITY_ANDROID)
            // If Purchasing has not yet been set up ...
            if (!IsPurchasingInitialized())
            {
                // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
                Logging.LogError(LogTag, "[" + TAG_PURCHASE + "]: FAIL. Not initialized.");
                PurchaseResult(false, LanguageManager.GetText(UITextKeys.UI_ERROR_IAP_NOT_INITIALIZED.ToString()));
                return;
            }

            var product = StoreController.GetProductById(productId);
            if (product != null && product.availableToPurchase)
            {
                Logging.Log(LogTag, string.Format("[" + TAG_PURCHASE + "] Purchasing product asychronously: '{0}'", product.definition.id));
                StoreController.PurchaseProduct(product);
            }
            else
            {
                Logging.LogError(LogTag, "[" + TAG_PURCHASE + "]: FAIL. Not purchasing product, either is not found or is not available for purchase.");
                PurchaseResult(false, LanguageManager.GetText(UITextKeys.UI_ERROR_IAP_PRODUCT_UNAVAILABLE.ToString()));
            }
#else
            Logging.LogWarning(LogTag, "Cannot purchase product, Unity Purchasing is not enabled.");
#endif
        }

        public void Restore()
        {
#if ENABLE_PURCHASING && (UNITY_IOS || UNITY_ANDROID)
            foreach (var order in FetchedOrders.PendingOrders)
            {
                StoreController_OnPurchasePending(order);
            }
#endif
        }
#endregion

        #region Callback Events
        private static void PurchaseResult(bool success, string errorMessage = "")
        {
            if (PurchaseCallback != null)
            {
                PurchaseCallback(success, errorMessage);
                PurchaseCallback = null;
            }
        }
        #endregion
    }
}
