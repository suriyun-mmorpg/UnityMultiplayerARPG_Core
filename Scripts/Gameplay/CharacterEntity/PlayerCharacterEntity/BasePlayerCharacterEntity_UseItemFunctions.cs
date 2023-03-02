using LiteNetLibManager;
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
        protected void ServerUseItem(int itemIndex)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!CanUseItem())
                return;

            if (!this.ValidateUsableItemToUse(itemIndex, out IUsableItem usableItem, out UITextKeys gameMessage))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, gameMessage);
                return;
            }

            // Set cooldown
            CharacterSkillUsage newSkillUsage = CharacterSkillUsage.Create(SkillUsageType.UsableItem, nonEquipItems[itemIndex].dataId);
            newSkillUsage.Use(this, 1);
            SkillUsages.Add(newSkillUsage);
            // Use item
            usableItem.UseItem(this, itemIndex, nonEquipItems[itemIndex]);
            // Do something with buffs when use item
            SkillAndBuffComponent.OnUseItem();
#endif
        }
    }
}
