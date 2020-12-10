using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class LanRpgCashShopMessageHandlers : MonoBehaviour, ICashShopMessageHandlers
    {
        public IServerPlayerCharacterHandlers ServerPlayerCharacterHandlers { get; set; }

        public UniTaskVoid HandleRequestCashShopInfo(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseCashShopInfoMessage> result)
        {
            // Set response data
            ResponseCashShopInfoMessage.Error error = ResponseCashShopInfoMessage.Error.None;
            int cash = 0;
            List<int> cashShopItemIds = new List<int>();
            BasePlayerCharacterEntity playerCharacter;
            if (!BaseGameNetworkManager.PlayerCharacters.TryGetValue(requestHandler.ConnectionId, out playerCharacter))
            {
                // Canot find user
                error = ResponseCashShopInfoMessage.Error.UserNotFound;
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
                error == ResponseCashShopInfoMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error,
                 new ResponseCashShopInfoMessage()
                 {
                     error = error,
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
            ResponseCashShopBuyMessage.Error error = ResponseCashShopBuyMessage.Error.None;
            int dataId = request.dataId;
            int cash = 0;
            BasePlayerCharacterEntity playerCharacter;
            if (!BaseGameNetworkManager.PlayerCharacters.TryGetValue(requestHandler.ConnectionId, out playerCharacter))
            {
                // Canot find user
                error = ResponseCashShopBuyMessage.Error.UserNotFound;
            }
            else
            {
                // Get user cash amount
                cash = playerCharacter.UserCash;
                CashShopItem cashShopItem;
                if (!GameInstance.CashShopItems.TryGetValue(dataId, out cashShopItem))
                {
                    // Cannot find item
                    error = ResponseCashShopBuyMessage.Error.ItemNotFound;
                }
                else if (cash < cashShopItem.sellPrice)
                {
                    // Not enough cash
                    error = ResponseCashShopBuyMessage.Error.NotEnoughCash;
                }
                else if (playerCharacter.IncreasingItemsWillOverwhelming(cashShopItem.receiveItems))
                {
                    // Cannot carry all rewards
                    error = ResponseCashShopBuyMessage.Error.CannotCarryAllRewards;
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
                error == ResponseCashShopBuyMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseCashShopBuyMessage()
                {
                    error = error,
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
            ResponseCashPackageInfoMessage.Error error = ResponseCashPackageInfoMessage.Error.None;
            int cash = 0;
            List<int> cashPackageIds = new List<int>();
            BasePlayerCharacterEntity playerCharacter;
            if (!BaseGameNetworkManager.PlayerCharacters.TryGetValue(requestHandler.ConnectionId, out playerCharacter))
            {
                // Canot find user
                error = ResponseCashPackageInfoMessage.Error.UserNotFound;
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
                error == ResponseCashPackageInfoMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseCashPackageInfoMessage()
                {
                    error = error,
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
            ResponseCashPackageBuyValidationMessage.Error error = ResponseCashPackageBuyValidationMessage.Error.None;
            int dataId = request.dataId;
            int cash = 0;
            BasePlayerCharacterEntity playerCharacter;
            if (!BaseGameNetworkManager.PlayerCharacters.TryGetValue(requestHandler.ConnectionId, out playerCharacter))
            {
                // Canot find user
                error = ResponseCashPackageBuyValidationMessage.Error.UserNotFound;
            }
            else
            {
                // Get user cash amount
                cash = playerCharacter.UserCash;
                CashPackage cashPackage;
                if (!GameInstance.CashPackages.TryGetValue(dataId, out cashPackage))
                {
                    // Cannot find package
                    error = ResponseCashPackageBuyValidationMessage.Error.PackageNotFound;
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
                error == ResponseCashPackageBuyValidationMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseCashPackageBuyValidationMessage()
                {
                    error = error,
                    dataId = dataId,
                    cash = cash,
                });
            return default;
        }
    }
}
