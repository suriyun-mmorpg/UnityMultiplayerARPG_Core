using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class LanRpgServerCashShopMessageHandlers : MonoBehaviour, IServerCashShopMessageHandlers
    {
        public async UniTaskVoid HandleRequestCashShopInfo(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseCashShopInfoMessage> result)
        {
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseCashShopInfoMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            result.Invoke(AckResponseCode.Success, new ResponseCashShopInfoMessage()
            {
                cash = playerCharacter.UserCash,
                cashShopItemIds = new List<int>(GameInstance.CashShopItems.Keys).ToArray(),
            });

            await UniTask.Yield();
        }

        public async UniTaskVoid HandleRequestCashShopBuy(
            RequestHandlerData requestHandler, RequestCashShopBuyMessage request,
            RequestProceedResultDelegate<ResponseCashShopBuyMessage> result)
        {
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseCashShopBuyMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            if (request.amount <= 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseCashShopBuyMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_DATA,
                });
                return;
            }

            CashShopItem cashShopItem;
            if (!GameInstance.CashShopItems.TryGetValue(request.dataId, out cashShopItem))
            {
                result.Invoke(AckResponseCode.Error, new ResponseCashShopBuyMessage()
                {
                    message = UITextKeys.UI_ERROR_ITEM_NOT_FOUND,
                });
                return;
            }

            if ((request.currencyType == CashShopItemCurrencyType.CASH && cashShopItem.SellPriceCash <= 0) ||
                (request.currencyType == CashShopItemCurrencyType.GOLD && cashShopItem.SellPriceGold <= 0))
            {
                result.Invoke(AckResponseCode.Error, new ResponseCashShopBuyMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_ITEM_DATA,
                });
                return;
            }

            int characterGold = playerCharacter.Gold;
            int userCash = playerCharacter.UserCash;
            int priceCash = 0;
            int priceGold = 0;

            if (request.currencyType == CashShopItemCurrencyType.CASH)
            {
                // Validate cash
                priceCash = cashShopItem.SellPriceCash * request.amount;
                if (userCash < priceCash)
                {
                    result.Invoke(AckResponseCode.Error, new ResponseCashShopBuyMessage()
                    {
                        message = UITextKeys.UI_ERROR_NOT_ENOUGH_CASH,
                    });
                    return;
                }
            }

            if (request.currencyType == CashShopItemCurrencyType.GOLD)
            {
                // Validate gold
                priceGold = cashShopItem.SellPriceGold * request.amount;
                if (characterGold < priceGold)
                {
                    result.Invoke(AckResponseCode.Error, new ResponseCashShopBuyMessage()
                    {
                        message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD,
                    });
                    return;
                }
            }

            // Increase items
            if (cashShopItem.ReceiveItems != null &&
                cashShopItem.ReceiveItems.Length > 0)
            {
                List<ItemAmount> receiveItems = new List<ItemAmount>();
                foreach (ItemAmount itemAmount in cashShopItem.ReceiveItems)
                {
                    receiveItems.Add(new ItemAmount()
                    {
                        item = itemAmount.item,
                        amount = (short)(itemAmount.amount * request.amount),
                    });
                }
                if (playerCharacter.IncreasingItemsWillOverwhelming(receiveItems))
                {
                    result.Invoke(AckResponseCode.Error, new ResponseCashShopBuyMessage()
                    {
                        message = UITextKeys.UI_ERROR_WILL_OVERWHELMING,
                    });
                    return;
                }
                playerCharacter.IncreaseItems(receiveItems);
                playerCharacter.FillEmptySlots();
            }

            if (request.currencyType == CashShopItemCurrencyType.CASH)
            {
                // Reduce cash
                userCash -= priceCash;
            }

            if (request.currencyType == CashShopItemCurrencyType.GOLD)
            {
                // Reduce gold
                characterGold -= priceGold;
            }

            if (cashShopItem.ReceiveGold > 0)
            {
                // Increase gold
                characterGold = characterGold.Increase(cashShopItem.ReceiveGold * request.amount);
            }

            playerCharacter.Gold = characterGold;
            playerCharacter.UserCash = userCash;

            // Response to client
            result.Invoke(AckResponseCode.Success, new ResponseCashShopBuyMessage()
            {
                dataId = request.dataId,
            });
        }

        public async UniTaskVoid HandleRequestCashPackageInfo(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseCashPackageInfoMessage> result)
        {
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseCashPackageInfoMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            result.Invoke(AckResponseCode.Success, new ResponseCashPackageInfoMessage()
            {
                cash = playerCharacter.UserCash,
                cashPackageIds = new List<int>(GameInstance.CashPackages.Keys).ToArray(),
            });

            await UniTask.Yield();
        }

        public async UniTaskVoid HandleRequestCashPackageBuyValidation(
            RequestHandlerData requestHandler, RequestCashPackageBuyValidationMessage request,
            RequestProceedResultDelegate<ResponseCashPackageBuyValidationMessage> result)
        {
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseCashPackageBuyValidationMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            CashPackage cashPackage;
            if (!GameInstance.CashPackages.TryGetValue(request.dataId, out cashPackage))
            {
                result.Invoke(AckResponseCode.Error, new ResponseCashPackageBuyValidationMessage()
                {
                    message = UITextKeys.UI_ERROR_CASH_PACKAGE_NOT_FOUND,
                });
                return;
            }
            playerCharacter.UserCash = playerCharacter.UserCash.Increase(cashPackage.CashAmount);

            result.Invoke(AckResponseCode.Success, new ResponseCashPackageBuyValidationMessage()
            {
                dataId = request.dataId,
                cash = playerCharacter.UserCash,
            });

            await UniTask.Yield();
        }
    }
}
