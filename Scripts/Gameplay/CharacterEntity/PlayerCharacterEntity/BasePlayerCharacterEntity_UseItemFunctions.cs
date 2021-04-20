using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        /// <summary>
        /// This function will be called at server to order character to use item
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="aimPosition"></param>
        [ServerRpc]
        protected void ServerUseItem(short itemIndex)
        {
#if !CLIENT_BUILD
            if (!CanUseItem())
                return;

            if (itemIndex >= nonEquipItems.Count)
                return;

            CharacterItem characterItem = nonEquipItems[itemIndex];
            if (characterItem.IsLock())
                return;

            IUsableItem usableItem = characterItem.GetUsableItem();
            if (usableItem == null)
                return;

            usableItem.UseItem(this, itemIndex, characterItem);
#endif
        }
    }
}
