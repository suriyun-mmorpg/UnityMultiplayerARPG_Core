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

            if (Time.unscaledTime - LastUseItemTime < CurrentGameInstance.useItemDelay)
                return false;

            if (itemIndex >= nonEquipItems.Count)
                return false;

            if (nonEquipItems[itemIndex].IsLock())
                return false;


            IUsableItem usableItem = nonEquipItems[itemIndex].GetUsableItem();
            if (usableItem == null)
                return false;

            float time = Time.unscaledTime;
            int itemDataId = nonEquipItems[itemIndex].dataId;
            if (usableItem.UseItemCooldown > 0f && LastUseItemTimes.ContainsKey(itemDataId) && time - LastUseItemTimes[itemDataId] < usableItem.UseItemCooldown)
                return false;

            LastUseItemTime = time;
            if (!IsServer)
                LastUseItemTimes[itemDataId] = time;
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
            {
                relateId = characterItem.id;
            }
            return CallServerAssignHotkey(hotkeyId, HotkeyType.Item, relateId);
        }

        public bool AssignSkillHotkey(string hotkeyId, BaseSkill skill)
        {
            return CallServerAssignHotkey(hotkeyId, HotkeyType.Skill, skill.Id);
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
