using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class LanRpgServerCashShopMessageHandlers : MonoBehaviour, IServerCashShopMessageHandlers
    {
        public UniTaskVoid HandleRequestCashShopInfo(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseCashShopInfoMessage> result)
        {
            // Set response data
            UITextKeys message = UITextKeys.NONE;
            int cash = 0;
            List<int> cashShopItemIds = new List<int>();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                // Canot find user
                message = UITextKeys.UI_ERROR_NOT_LOGGED_IN;
            }
            else
            {
                // Get user cash amount
                cash = playerCharacter.UserCash;
                // Set cash shop item ids
                cashShopItemIds.AddRange(GameInstance.CashShopItems.Keys);
            }
            // Send response message
            result.Invoke(
                message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                 new ResponseCashShopInfoMessage()
                 {
                     message = message,
                     cash = cash,
                     cashShopItemIds = cashShopItemIds.ToArray(),
                 });
            return default;
        }

        public UniTaskVoid HandleRequestCashShopBuy(
            RequestHandlerData requestHandler, RequestCashShopBuyMessage request,
            RequestProceedResultDelegate<ResponseCashShopBuyMessage> result)
        {
            // Set response data
            UITextKeys message = UITextKeys.NONE;
            int dataId = request.dataId;
            int userCash = 0;
            int userGold = 0;
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                // Canot find user
                message = UITextKeys.UI_ERROR_NOT_LOGGED_IN;
            }
            else
            {
                // Get user cash amount
                userCash = playerCharacter.UserCash;
                userGold = playerCharacter.UserGold;
                CashShopItem cashShopItem;
                if (!GameInstance.CashShopItems.TryGetValue(dataId, out cashShopItem))
                {
                    // Cannot find item
                    message = UITextKeys.UI_ERROR_ITEM_NOT_FOUND;
                }
                else if (request.currencyType == CashShopItemCurrencyType.CASH && userCash < cashShopItem.SellPriceCash)
                {
                    // Not enough cash
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_CASH;
                }
                else if (request.currencyType == CashShopItemCurrencyType.GOLD && userGold < cashShopItem.SellPriceGold)
                {
                    // Not enough gold
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD;
                }
                else if (playerCharacter.IncreasingItemsWillOverwhelming(cashShopItem.ReceiveItems))
                {
                    // Cannot carry all rewards
                    message = UITextKeys.UI_ERROR_WILL_OVERWHELMING;
                }
                else
                {
                    // Decrease cash amount
                    if (request.currencyType == CashShopItemCurrencyType.CASH)
                    {
                        userCash -= cashShopItem.SellPriceCash;
                        playerCharacter.UserCash = userCash;
                    }
                    // Decrease gold amount
                    if (request.currencyType == CashShopItemCurrencyType.GOLD)
                    {
                        userGold -= cashShopItem.SellPriceGold;
                        playerCharacter.UserGold = userGold;
                    }
                    // Increase character gold
                    playerCharacter.Gold = playerCharacter.Gold.Increase(cashShopItem.ReceiveGold);
                    // Increase currencies
                    playerCharacter.IncreaseCurrencies(cashShopItem.ReceiveCurrencies);
                    // Increase character items
                    if (cashShopItem.ReceiveItems != null && cashShopItem.ReceiveItems.Length > 0)
                    {
                        playerCharacter.IncreaseItems(cashShopItem.ReceiveItems);
                        playerCharacter.FillEmptySlots();
                    }
                }
            }
            // Send response message
            result.Invoke(
                message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseCashShopBuyMessage()
                {
                    message = message,
                    dataId = dataId,
                });
            return default;
        }

        public UniTaskVoid HandleRequestCashPackageInfo(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseCashPackageInfoMessage> result)
        {
            // Set response data
            UITextKeys message = UITextKeys.NONE;
            int cash = 0;
            List<int> cashPackageIds = new List<int>();
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                // Canot find user
                message = UITextKeys.UI_ERROR_NOT_LOGGED_IN;
            }
            else
            {
                // Get user cash amount
                cash = playerCharacter.UserCash;
                // Set cash package ids
                cashPackageIds.AddRange(GameInstance.CashPackages.Keys);
            }
            // Send response message
            result.Invoke(
                message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseCashPackageInfoMessage()
                {
                    message = message,
                    cash = cash,
                    cashPackageIds = cashPackageIds.ToArray(),
                });
            return default;
        }

        public UniTaskVoid HandleRequestCashPackageBuyValidation(
            RequestHandlerData requestHandler, RequestCashPackageBuyValidationMessage request,
            RequestProceedResultDelegate<ResponseCashPackageBuyValidationMessage> result)
        {
            // TODO: Validate purchasing at server side
            // Set response data
            UITextKeys message = UITextKeys.NONE;
            int dataId = request.dataId;
            int cash = 0;
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                // Canot find user
                message = UITextKeys.UI_ERROR_NOT_LOGGED_IN;
            }
            else
            {
                // Get user cash amount
                cash = playerCharacter.UserCash;
                CashPackage cashPackage;
                if (!GameInstance.CashPackages.TryGetValue(dataId, out cashPackage))
                {
                    // Cannot find package
                    message = UITextKeys.UI_ERROR_CASH_PACKAGE_NOT_FOUND;
                }
                else
                {
                    // Increase cash amount
                    cash += cashPackage.CashAmount;
                    playerCharacter.UserCash = cash;
                }
            }
            // Send response message
            result.Invoke(
                message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseCashPackageBuyValidationMessage()
                {
                    message = message,
                    dataId = dataId,
                    cash = cash,
                });
            return default;
        }
    }
}
