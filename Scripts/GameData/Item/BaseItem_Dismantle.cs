using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseItem
    {
        public static List<ItemAmount> GetDismantleReturnItems(CharacterItem dismantlingItem)
        {
            if (dismantlingItem.IsEmptySlot())
                return new List<ItemAmount>();
            List<ItemAmount> result = new List<ItemAmount>(dismantlingItem.GetItem().dismantleReturnItems);
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
