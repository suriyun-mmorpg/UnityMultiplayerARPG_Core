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

            if (itemIndex < 0 || itemIndex >= nonEquipItems.Count)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(Entity.ConnectionId, UITextKeys.UI_ERROR_INVALID_ITEM_INDEX);
                return;
            }

            if (this.IndexOfSkillUsage(nonEquipItems[itemIndex].dataId, SkillUsageType.UsableItem) >= 0)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(Entity.ConnectionId, UITextKeys.UI_ERROR_ITEM_IS_COOLING_DOWN);
                return;
            }

            if (nonEquipItems[itemIndex].IsLocked())
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(Entity.ConnectionId, UITextKeys.UI_ERROR_ITEM_IS_LOCKED);
                return;
            }

            IUsableItem usableItem = nonEquipItems[itemIndex].GetUsableItem();
            if (usableItem == null)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(Entity.ConnectionId, UITextKeys.UI_ERROR_INVALID_ITEM_DATA);
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
