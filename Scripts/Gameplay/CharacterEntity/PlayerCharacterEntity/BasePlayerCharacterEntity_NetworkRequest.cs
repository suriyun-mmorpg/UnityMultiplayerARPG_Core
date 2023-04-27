using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        public bool ValidateRequestUseItem(int itemIndex)
        {
            if (!CanUseItem())
                return false;

            if (!UpdateLastActionTime())
                return false;

            float time = Time.unscaledTime;
            if (time - LastUseItemTime < CurrentGameInstance.useItemDelay)
                return false;

            if (!this.ValidateUsableItemToUse(itemIndex, out _, out UITextKeys gameMessage))
            {
                ClientGenericActions.ClientReceiveGameMessage(gameMessage);
                return false;
            }

            LastUseItemTime = time;
            return true;
        }

        public bool CallServerUseItem(int index)
        {
            if (!ValidateRequestUseItem(index))
                return false;
            RPC(ServerUseItem, index);
            return true;
        }

        public bool CallServerUseGuildSkill(int dataId)
        {
            if (this.IsDead())
                return false;
            RPC(ServerUseGuildSkill, dataId);
            return true;
        }

        public bool CallServerAssignHotkey(string hotkeyId, HotkeyType type, string id)
        {
            RPC(ServerAssignHotkey, hotkeyId, type, id);
            return true;
        }

        public bool AssignItemHotkey(string hotkeyId, CharacterItem characterItem)
        {
            // Usable items will use item data id
            string relateId = characterItem.GetItem().Id;
            // For an equipments, it will use item unique id
            if (characterItem.GetEquipmentItem() != null)
                relateId = characterItem.id;
            return CallServerAssignHotkey(hotkeyId, HotkeyType.Item, relateId);
        }

        public bool AssignSkillHotkey(string hotkeyId, CharacterSkill characterSkill)
        {
            // Use skil data id
            string relateId = characterSkill.GetSkill().Id;
            return CallServerAssignHotkey(hotkeyId, HotkeyType.Skill, relateId);
        }

        public bool UnAssignHotkey(string hotkeyId)
        {
            return CallServerAssignHotkey(hotkeyId, HotkeyType.None, string.Empty);
        }

        public bool CallServerEnterWarp(uint objectId)
        {
            if (!CanDoActions())
                return false;
            RPC(ServerEnterWarp, objectId);
            return true;
        }

        public bool CallServerAppendCraftingQueueItem(uint sourceObjectId, int dataId, int amount)
        {
            if (!CurrentGameplayRule.CanInteractEntity(this, sourceObjectId))
                return false;
            RPC(ServerAppendCraftingQueueItem, sourceObjectId, dataId, amount);
            return true;
        }

        public bool CallServerChangeCraftingQueueItem(uint sourceObjectId, int indexOfData, int amount)
        {
            if (!CurrentGameplayRule.CanInteractEntity(this, sourceObjectId))
                return false;
            RPC(ServerChangeCraftingQueueItem, sourceObjectId, indexOfData, amount);
            return true;
        }

        public bool CallServerCancelCraftingQueueItem(uint sourceObjectId, int indexOfData)
        {
            if (!CurrentGameplayRule.CanInteractEntity(this, sourceObjectId))
                return false;
            RPC(ServerCancelCraftingQueueItem, sourceObjectId, indexOfData);
            return true;
        }

        public bool CallServerChangeQuestTracking(int questDataId, bool isTracking)
        {
            RPC(ServerChangeQuestTracking, questDataId, isTracking);
            return true;
        }
    }
}
