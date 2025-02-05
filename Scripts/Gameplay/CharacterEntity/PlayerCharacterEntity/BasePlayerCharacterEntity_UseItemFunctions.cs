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
        protected void CmdUseItem(int itemIndex)
        {
#if UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES
            if (!CanUseItem())
                return;

            if (!this.ValidateUsableItemToUse(itemIndex, out IUsableItem usableItem, out UITextKeys gameMessage))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, gameMessage);
                return;
            }

            // Set cooldown to user
            this.AddOrUpdateSkillUsage(SkillUsageType.UsableItem, nonEquipItems[itemIndex].dataId, 1);

            // Use item
            usableItem.UseItem(this, itemIndex, nonEquipItems[itemIndex]);
            // Do something with buffs when use item
            SkillAndBuffComponent.OnUseItem();
#endif
        }

        [ServerRpc]
        protected async void CmdUseCharacterNameChangeItem(int itemIndex, string newCharacterName)
        {
#if UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES
            if (!CanUseItem())
                return;

            if (!this.ValidateUsableItemToUse(itemIndex, out IUsableItem usableItem, out UITextKeys gameMessage))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, gameMessage);
                return;
            }

            // Validate item
            if (usableItem is not CharacterNameChangeItem)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_INVALID_ITEM_DATA);
                return;
            }

            // Validate new character name
            UITextKeys detectNameExistance = await GameInstance.ServerUserHandlers.DetectCharacterNameExistance(newCharacterName);
            if (detectNameExistance != UITextKeys.NONE)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, detectNameExistance);
                return;
            }

            // Use item
            usableItem.UseItem(this, itemIndex, nonEquipItems[itemIndex]);
#endif
        }
    }
}
