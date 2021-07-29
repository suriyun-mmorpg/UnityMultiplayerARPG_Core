using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class BaseItem
    {
        public static List<ItemAmount> GetDismantleReturnItems(CharacterItem dismantlingItem, short amount)
        {
            if (dismantlingItem.IsEmptySlot() || amount == 0)
                return new List<ItemAmount>();

            if (amount < 0 || amount > dismantlingItem.amount)
                amount = dismantlingItem.amount;

            List<ItemAmount> result = new List<ItemAmount>();
            ItemAmount[] dismantleReturnItems = dismantlingItem.GetItem().dismantleReturnItems;
            for (int i = 0; i < dismantleReturnItems.Length; ++i)
            {
                result.Add(new ItemAmount()
                {
                    item = dismantleReturnItems[i].item,
                    amount = (short)(dismantleReturnItems[i].amount * amount)
                });
            }
            if (dismantlingItem.Sockets.Count > 0)
            {
                BaseItem socketItem;
                for (int i = 0; i < dismantlingItem.Sockets.Count; ++i)
                {
                    if (!GameInstance.Items.TryGetValue(dismantlingItem.Sockets[i], out socketItem))
                        continue;
                    result.Add(new ItemAmount()
                    {
                        item = socketItem,
                        amount = 1,
                    });
                }
            }
            return result;
        }
    }
}
