using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static partial class CharacterInventoryExtensions
    {
        public static bool VerifyDismantleItem(this IPlayerCharacterData character, int index, int amount, List<CharacterItem> simulatingNonEquipItems, out UITextKeys gameMessage, out ItemAmount dismentleItem, out int returningGold, out List<ItemAmount> returningItems, out List<CurrencyAmount> returningCurrencies)
        {
            gameMessage = UITextKeys.NONE;
            dismentleItem = new ItemAmount();
            returningGold = 0;
            returningItems = null;
            returningCurrencies = null;

            if (index < 0 || index >= character.NonEquipItems.Count)
            {
                gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX;
                return false;
            }

            // Found item or not?
            CharacterItem nonEquipItem = character.NonEquipItems[index];
            if (nonEquipItem.IsEmptySlot() || amount > nonEquipItem.amount)
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_ITEMS;
                return false;
            }

            if (!GameInstance.Singleton.dismantleFilter.Filter(nonEquipItem))
            {
                gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_DATA;
                return false;
            }

            // Simulate data before applies
            if (!simulatingNonEquipItems.DecreaseItemsByIndex(index, amount, GameInstance.Singleton.IsLimitInventorySlot, false))
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_ITEMS;
                return false;
            }

            // Character can receives all items or not?
            nonEquipItem.GetDismantleReturnItems(amount, out returningItems, out returningCurrencies);
            if (simulatingNonEquipItems.IncreasingItemsWillOverwhelming(
                returningItems,
                GameInstance.Singleton.IsLimitInventoryWeight,
                character.GetCaches().LimitItemWeight,
                character.GetCaches().TotalItemWeight,
                GameInstance.Singleton.IsLimitInventorySlot,
                character.GetCaches().LimitItemSlot))
            {
                returningItems.Clear();
                gameMessage = UITextKeys.UI_ERROR_WILL_OVERWHELMING;
                return false;
            }
            BaseItem item = nonEquipItem.GetItem();
            dismentleItem = new ItemAmount()
            {
                item = item,
                amount = amount,
            };
            simulatingNonEquipItems.IncreaseItems(returningItems);
            returningGold = item.DismantleReturnGold * amount;
            return true;
        }

        public static bool DismantleItem(this IPlayerCharacterData character, int index, int amount, out UITextKeys gameMessage)
        {
#if UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES
            ItemAmount dismentleItem;
            int returningGold;
            List<ItemAmount> returningItems;
            List<CurrencyAmount> returningCurrencies;
            List<CharacterItem> simulatingNonEquipItems = character.NonEquipItems.Clone();
            if (!character.VerifyDismantleItem(index, amount, simulatingNonEquipItems, out gameMessage, out dismentleItem, out returningGold, out returningItems, out returningCurrencies))
                return false;
            List<ItemAmount> dismentleItems = new List<ItemAmount>() { dismentleItem };
            List<CharacterItem> increasedItems = new List<CharacterItem>();
            List<CharacterItem> droppedItems = new List<CharacterItem>();
            character.Gold = character.Gold.Increase(returningGold);
            character.DecreaseItemsByIndex(index, amount, true);
            character.IncreaseItems(returningItems);
            character.IncreaseCurrencies(returningCurrencies);
            character.FillEmptySlots();
            GameInstance.ServerLogHandlers.LogDismentleItems(character, dismentleItems);
            return true;
#else
            gameMessage = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE;
            return false;
#endif
        }

        public static bool DismantleItems(this IPlayerCharacterData character, int[] selectedIndexes, out UITextKeys gameMessage)
        {
#if UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES
            gameMessage = UITextKeys.NONE;
            List<int> indexes = new List<int>(selectedIndexes);
            indexes.Sort();
            Dictionary<int, int> indexAmountPairs = new Dictionary<int, int>();
            List<CharacterItem> simulatingNonEquipItems = character.NonEquipItems.Clone();
            List<ItemAmount> dismentleItems = new List<ItemAmount>();
            int returningGold = 0;
            List<ItemAmount> returningItems = new List<ItemAmount>();
            List<CurrencyAmount> returningCurrencies = new List<CurrencyAmount>();
            int tempIndex;
            int tempAmount;
            ItemAmount tempDismentleItem;
            int tempReturningGold;
            List<ItemAmount> tempReturningItems;
            List<CurrencyAmount> tempReturningCurrencies;
            for (int i = indexes.Count - 1; i >= 0; --i)
            {
                tempIndex = indexes[i];
                if (indexAmountPairs.ContainsKey(tempIndex))
                    continue;
                if (tempIndex >= character.NonEquipItems.Count)
                    continue;
                tempAmount = character.NonEquipItems[tempIndex].amount;
                if (!character.VerifyDismantleItem(tempIndex, tempAmount, simulatingNonEquipItems, out gameMessage, out tempDismentleItem, out tempReturningGold, out tempReturningItems, out tempReturningCurrencies))
                    return false;
                dismentleItems.Add(tempDismentleItem);
                returningGold += tempReturningGold;
                returningItems.AddRange(tempReturningItems);
                returningCurrencies.AddRange(tempReturningCurrencies);
                indexAmountPairs.Add(tempIndex, tempAmount);
            }
            character.Gold = character.Gold.Increase(returningGold);
            indexes.Clear();
            indexes.AddRange(indexAmountPairs.Keys);
            indexes.Sort();
            for (int i = indexes.Count - 1; i >= 0; --i)
            {
                character.DecreaseItemsByIndex(indexes[i], indexAmountPairs[indexes[i]], true);
            }
            character.IncreaseItems(returningItems);
            character.IncreaseCurrencies(returningCurrencies);
            character.FillEmptySlots();
            GameInstance.ServerLogHandlers.LogDismentleItems(character, dismentleItems);
            return true;
#else
            gameMessage = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE;
            return false;
#endif
        }

        public static void GetDismantleReturnItems(this CharacterItem dismantlingItem, int amount, out List<ItemAmount> items, out List<CurrencyAmount> currencies)
        {
            items = new List<ItemAmount>();
            currencies = new List<CurrencyAmount>();
            if (dismantlingItem.IsEmptySlot() || amount == 0)
                return;

            if (amount < 0 || amount > dismantlingItem.amount)
                amount = dismantlingItem.amount;

            // Returning items
            ItemAmount[] dismantleReturnItems = dismantlingItem.GetItem().DismantleReturnItems;
            for (int i = 0; i < dismantleReturnItems.Length; ++i)
            {
                items.Add(new ItemAmount()
                {
                    item = dismantleReturnItems[i].item,
                    amount = dismantleReturnItems[i].amount * amount,
                });
            }
            if (dismantlingItem.sockets.Count > 0)
            {
                BaseItem socketItem;
                for (int i = 0; i < dismantlingItem.sockets.Count; ++i)
                {
                    if (!GameInstance.Items.TryGetValue(dismantlingItem.sockets[i], out socketItem))
                        continue;
                    items.Add(new ItemAmount()
                    {
                        item = socketItem,
                        amount = 1,
                    });
                }
            }

            // Returning currencies
            CurrencyAmount[] dismantleReturnCurrencies = dismantlingItem.GetItem().DismantleReturnCurrencies;
            for (int i = 0; i < dismantleReturnCurrencies.Length; ++i)
            {
                currencies.Add(new CurrencyAmount()
                {
                    currency = dismantleReturnCurrencies[i].currency,
                    amount = dismantleReturnCurrencies[i].amount * amount,
                });
            }
        }
    }
}
