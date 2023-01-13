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
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseCashShopInfoMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            result.InvokeSuccess(new ResponseCashShopInfoMessage()
            {
                cash = playerCharacter.UserCash,
                cashShopItemIds = new List<int>(GameInstance.CashShopItems.Keys),
            });
        }

        public async UniTaskVoid HandleRequestCashShopBuy(
            RequestHandlerData requestHandler, RequestCashShopBuyMessage request,
            RequestProceedResultDelegate<ResponseCashShopBuyMessage> result)
        {
            await UniTask.Yield();

            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseCashShopBuyMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            if (request.amount <= 0)
            {
                result.InvokeError(new ResponseCashShopBuyMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_DATA,
                });
                return;
            }

            if (!GameInstance.CashShopItems.TryGetValue(request.dataId, out CashShopItem cashShopItem))
            {
                result.InvokeError(new ResponseCashShopBuyMessage()
                {
                    message = UITextKeys.UI_ERROR_ITEM_NOT_FOUND,
                });
                return;
            }

            if ((request.currencyType == CashShopItemCurrencyType.CASH && cashShopItem.SellPriceCash <= 0) ||
                (request.currencyType == CashShopItemCurrencyType.GOLD && cashShopItem.SellPriceGold <= 0))
            {
                result.InvokeError(new ResponseCashShopBuyMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_ITEM_DATA,
                });
                return;
            }

            int characterGold = playerCharacter.Gold;
            int userCash = playerCharacter.UserCash;
            int priceGold = 0;
            int priceCash = 0;
            int changeCharacterGold = 0;
            int changeUserCash = 0;

            // Validate cash
            if (request.currencyType == CashShopItemCurrencyType.CASH)
            {
                priceCash = cashShopItem.SellPriceCash * request.amount;
                if (userCash < priceCash)
                {
                    result.InvokeError(new ResponseCashShopBuyMessage()
                    {
                        message = UITextKeys.UI_ERROR_NOT_ENOUGH_CASH,
                    });
                    return;
                }
                changeUserCash -= priceCash;
            }

            // Validate gold
            if (request.currencyType == CashShopItemCurrencyType.GOLD)
            {
                priceGold = cashShopItem.SellPriceGold * request.amount;
                if (characterGold < priceGold)
                {
                    result.InvokeError(new ResponseCashShopBuyMessage()
                    {
                        message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD,
                    });
                    return;
                }
                changeCharacterGold -= priceGold;
            }

            // Increase gold
            if (cashShopItem.ReceiveGold > 0)
                changeCharacterGold = changeCharacterGold.Increase(cashShopItem.ReceiveGold * request.amount);

            // Increase items
            List<RewardedItem> rewardItems = new List<RewardedItem>();
            if (cashShopItem.ReceiveItems != null &&
                cashShopItem.ReceiveItems.Length > 0)
            {
                foreach (ItemAmount itemAmount in cashShopItem.ReceiveItems)
                {
                    for (int i = 0; i < request.amount; ++i)
                    {
                        rewardItems.Add(new RewardedItem()
                        {
                            item = itemAmount.item,
                            level = 1,
                            amount = itemAmount.amount,
                            randomSeed = Random.Range(int.MinValue, int.MaxValue),
                        });
                    }
                }
                if (playerCharacter.IncreasingItemsWillOverwhelming(rewardItems))
                {
                    result.InvokeError(new ResponseCashShopBuyMessage()
                    {
                        message = UITextKeys.UI_ERROR_WILL_OVERWHELMING,
                    });
                    return;
                }
            }

            // Increase custom currencies
            List<CharacterCurrency> customCurrencies = new List<CharacterCurrency>();
            if (cashShopItem.ReceiveCurrencies != null &&
                cashShopItem.ReceiveCurrencies.Length > 0)
            {
                foreach (CurrencyAmount currencyAmount in cashShopItem.ReceiveCurrencies)
                {
                    for (int i = 0; i < request.amount; ++i)
                    {
                        customCurrencies.Add(CharacterCurrency.Create(currencyAmount.currency, currencyAmount.amount));
                    }
                }
            }

            // Update currency
            characterGold += changeCharacterGold;
            userCash += changeUserCash;
            playerCharacter.Gold = characterGold;
            playerCharacter.UserCash = userCash;
            playerCharacter.IncreaseItems(rewardItems);
            playerCharacter.IncreaseCurrencies(customCurrencies);
            playerCharacter.FillEmptySlots();

            // Response to client
            result.InvokeSuccess(new ResponseCashShopBuyMessage()
            {
                dataId = request.dataId,
                rewardGold = cashShopItem.ReceiveGold,
                rewardItems = rewardItems,
            });
        }

        public async UniTaskVoid HandleRequestCashPackageInfo(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseCashPackageInfoMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseCashPackageInfoMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            result.InvokeSuccess(new ResponseCashPackageInfoMessage()
            {
                cash = playerCharacter.UserCash,
                cashPackageIds = new List<int>(GameInstance.CashPackages.Keys),
            });
        }

        public async UniTaskVoid HandleRequestCashPackageBuyValidation(
            RequestHandlerData requestHandler, RequestCashPackageBuyValidationMessage request,
            RequestProceedResultDelegate<ResponseCashPackageBuyValidationMessage> result)
        {
            await UniTask.Yield();
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseCashPackageBuyValidationMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (!GameInstance.CashPackages.TryGetValue(request.dataId, out CashPackage cashPackage))
            {
                result.InvokeError(new ResponseCashPackageBuyValidationMessage()
                {
                    message = UITextKeys.UI_ERROR_CASH_PACKAGE_NOT_FOUND,
                });
                return;
            }
            playerCharacter.UserCash = playerCharacter.UserCash.Increase(cashPackage.CashAmount);

            result.InvokeSuccess(new ResponseCashPackageBuyValidationMessage()
            {
                dataId = request.dataId,
                cash = playerCharacter.UserCash,
            });
        }
    }
}
