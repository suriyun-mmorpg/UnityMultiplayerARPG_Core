using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class LanRpgServerCashShopMessageHandlers : MonoBehaviour, IServerCashShopMessageHandlers
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
                CashShopItem cashShopItem;
                if (!GameInstance.CashShopItems.TryGetValue(dataId, out cashShopItem))
                {
                    // Cannot find item
                    message = UITextKeys.UI_ERROR_ITEM_NOT_FOUND;
                }
                else if (cash < cashShopItem.sellPrice)
                {
                    // Not enough cash
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_CASH;
                }
                else if (playerCharacter.IncreasingItemsWillOverwhelming(cashShopItem.receiveItems))
                {
                    // Cannot carry all rewards
                    message = UITextKeys.UI_ERROR_WILL_OVERWHELMING;
                }
                else
                {
                    // Decrease cash amount
                    cash -= cashShopItem.sellPrice;
                    playerCharacter.UserCash = cash;
                    // Increase character gold
                    playerCharacter.Gold = playerCharacter.Gold.Increase(cashShopItem.receiveGold);
                    // Increase character item
                    foreach (ItemAmount receiveItem in cashShopItem.receiveItems)
                    {
                        if (receiveItem.item == null || receiveItem.amount <= 0) continue;
                        playerCharacter.AddOrSetNonEquipItems(CharacterItem.Create(receiveItem.item, 1, receiveItem.amount));
                    }
                    playerCharacter.FillEmptySlots();
                }
            }
            // Send response message
            result.Invoke(
                message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseCashShopBuyMessage()
                {
                    message = message,
                    dataId = dataId,
                    cash = cash,
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
                    cash += cashPackage.cashAmount;
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
