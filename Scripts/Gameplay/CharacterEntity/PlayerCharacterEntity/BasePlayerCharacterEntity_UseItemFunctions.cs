using LiteNetLibManager;
using System.Reflection;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        /// <summary>
        /// This function will be called at server to order character to use item
        /// </summary>
        /// <param name="itemIndex"></param>
        [ServerRpc]
        protected void ServerUseItem(short itemIndex)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!CanUseItem())
                return;

            if (itemIndex >= nonEquipItems.Count)
                return;

            CharacterItem tempCharacterItem = nonEquipItems[itemIndex];
            if (tempCharacterItem.IsLock())
                return;

            IUsableItem usableItem = tempCharacterItem.GetUsableItem();
            if (usableItem == null)
                return;

            float time = Time.unscaledTime;
            if (usableItem.UseItemCooldown > 0f && lastUseItemTimes.ContainsKey(itemIndex) && time - lastUseItemTimes[nonEquipItems[itemIndex].dataId] < usableItem.UseItemCooldown)
                return;

            usableItem.UseItem(this, itemIndex, tempCharacterItem);
            lastUseItemTimes[nonEquipItems[itemIndex].dataId] = time;
#endif
        }
    }
}
