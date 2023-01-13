using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class BaseItem
    {
        public static void GetDismantleReturnItems(CharacterItem dismantlingItem, int amount, out List<ItemAmount> items, out List<CurrencyAmount> currencies)
        {
            items = new List<ItemAmount>();
            currencies = new List<CurrencyAmount>();
            if (dismantlingItem.IsEmptySlot() || amount == 0)
                return;

            if (amount < 0 || amount > dismantlingItem.amount)
                amount = dismantlingItem.amount;

            // Returning items
            ItemAmount[] dismantleReturnItems = dismantlingItem.GetItem().dismantleReturnItems;
            for (int i = 0; i < dismantleReturnItems.Length; ++i)
            {
                items.Add(new ItemAmount()
                {
                    item = dismantleReturnItems[i].item,
                    amount = dismantleReturnItems[i].amount * amount,
                });
            }
            if (dismantlingItem.Sockets.Count > 0)
            {
                BaseItem socketItem;
                for (int i = 0; i < dismantlingItem.Sockets.Count; ++i)
                {
                    if (!GameInstance.Items.TryGetValue(dismantlingItem.Sockets[i], out socketItem))
                        continue;
                    items.Add(new ItemAmount()
                    {
                        item = socketItem,
                        amount = 1,
                    });
                }
            }

            // Returning currencies
            CurrencyAmount[] dismantleReturnCurrencies = dismantlingItem.GetItem().dismantleReturnCurrencies;
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
