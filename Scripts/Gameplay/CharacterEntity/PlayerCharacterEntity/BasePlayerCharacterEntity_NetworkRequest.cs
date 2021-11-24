using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        public bool ValidateRequestUseItem(short index)
        {
            if (!CanUseItem())
                return false;

            if (!UpdateLastActionTime())
                return false;

            if (Time.unscaledTime - lastUseItemTime < CurrentGameInstance.useItemDelay)
                return false;

            if (index >= nonEquipItems.Count)
                return false;

            if (nonEquipItems[index].IsLock())
                return false;

            IUsableItem item = nonEquipItems[index].GetUsableItem();
            if (item == null)
                return false;

            lastUseItemTime = Time.unscaledTime;
            return true;
        }

        public bool CallServerUseItem(short index)
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

        public bool CallServerNpcActivate(uint objectId)
        {
            if (this.IsDead())
                return false;
            RPC(ServerNpcActivate, objectId);
            return true;
        }

        public bool CallOwnerShowQuestRewardItemSelection(int questDataId)
        {
            if (this.IsDead())
                return false;
            RPC(TargetShowQuestRewardItemSelection, ConnectionId, questDataId);
            return true;
        }

        public bool CallOwnerShowNpcDialog(int npcDialogDataId)
        {
            if (this.IsDead())
                return false;
            RPC(TargetShowNpcDialog, ConnectionId, npcDialogDataId);
            return true;
        }

        public bool CallOwnerShowNpcRefineItem()
        {
            if (this.IsDead())
                return false;
            RPC(TargetShowNpcRefineItem, ConnectionId);
            return true;
        }

        public bool CallOwnerShowNpcDismantleItem()
        {
            if (this.IsDead())
                return false;
            RPC(TargetShowNpcDismantleItem, ConnectionId);
            return true;
        }

        public bool CallOwnerShowNpcRepairItem()
        {
            if (this.IsDead())
                return false;
            RPC(TargetShowNpcRepairItem, ConnectionId);
            return true;
        }

        public bool CallServerSelectNpcDialogMenu(byte menuIndex)
        {
            if (this.IsDead())
                return false;
            RPC(ServerSelectNpcDialogMenu, menuIndex);
            return true;
        }

        public bool CallServerBuyNpcItem(short itemIndex, short amount)
        {
            if (this.IsDead())
                return false;
            RPC(ServerBuyNpcItem, itemIndex, amount);
            return true;
        }

        public bool CallServerSelectQuestRewardItem(byte itemIndex)
        {
            if (this.IsDead())
                return false;
            RPC(ServerSelectQuestRewardItem, itemIndex);
            return true;
        }

        public bool CallServerEnterWarp(uint objectId)
        {
            if (!CanDoActions())
                return false;
            RPC(ServerEnterWarp, objectId);
            return true;
        }

        public bool CallServerConstructBuilding(short itemIndex, Vector3 position, Quaternion rotation, uint parentObjectId)
        {
            if (!CanDoActions())
                return false;
            RPC(ServerConstructBuilding, itemIndex, position, rotation, parentObjectId);
            return true;
        }

        public bool CallServerDestroyBuilding(uint objectId)
        {
            if (!CanDoActions())
                return false;
            RPC(ServerDestroyBuilding, objectId);
            return true;
        }

        public bool CallServerSendDealingRequest(uint objectId)
        {
            RPC(ServerSendDealingRequest, objectId);
            return true;
        }

        public bool CallOwnerReceiveDealingRequest(uint objectId)
        {
            RPC(TargetReceiveDealingRequest, ConnectionId, objectId);
            return true;
        }

        public bool CallServerAcceptDealingRequest()
        {
            RPC(ServerAcceptDealingRequest);
            return true;
        }

        public bool CallServerDeclineDealingRequest()
        {
            RPC(ServerDeclineDealingRequest);
            return true;
        }

        public bool CallOwnerAcceptedDealingRequest(uint objectId)
        {
            RPC(TargetAcceptedDealingRequest, ConnectionId, objectId);
            return true;
        }

        public bool CallServerSetDealingItem(short itemIndex, short amount)
        {
            RPC(ServerSetDealingItem, itemIndex, amount);
            return true;
        }

        public bool CallServerSetDealingGold(int dealingGold)
        {
            RPC(ServerSetDealingGold, dealingGold);
            return true;
        }

        public bool CallServerLockDealing()
        {
            RPC(ServerLockDealing);
            return true;
        }

        public bool CallServerConfirmDealing()
        {
            RPC(ServerConfirmDealing);
            return true;
        }

        public bool CallServerCancelDealing()
        {
            RPC(ServerCancelDealing);
            return true;
        }

        public bool CallOwnerUpdateDealingState(DealingState state)
        {
            RPC(TargetUpdateDealingState, ConnectionId, state);
            return true;
        }

        public bool CallOwnerUpdateAnotherDealingState(DealingState state)
        {
            RPC(TargetUpdateAnotherDealingState, ConnectionId, state);
            return true;
        }

        public bool CallOwnerUpdateDealingGold(int gold)
        {
            RPC(TargetUpdateDealingGold, ConnectionId, gold);
            return true;
        }

        public bool CallOwnerUpdateAnotherDealingGold(int gold)
        {
            RPC(TargetUpdateAnotherDealingGold, ConnectionId, gold);
            return true;
        }

        public bool CallOwnerUpdateDealingItems(DealingCharacterItems dealingItems)
        {
            RPC(TargetUpdateDealingItems, ConnectionId, dealingItems);
            return true;
        }

        public bool CallOwnerUpdateAnotherDealingItems(DealingCharacterItems dealingItems)
        {
            RPC(TargetUpdateAnotherDealingItems, ConnectionId, dealingItems);
            return true;
        }

        public bool CallServerOpenStorage(uint objectId, string password)
        {
            if (!CurrentGameplayRule.CanInteractEntity(this, objectId))
                return false;
            RPC(ServerOpenStorage, objectId, password);
            return true;
        }

        public bool CallServerOpenDoor(uint objectId, string password)
        {
            if (!CurrentGameplayRule.CanInteractEntity(this, objectId))
                return false;
            RPC(ServerOpenDoor, objectId, password);
            return true;
        }

        public bool CallServerCloseDoor(uint objectId)
        {
            if (!CurrentGameplayRule.CanInteractEntity(this, objectId))
                return false;
            RPC(ServerCloseDoor, objectId);
            return true;
        }

        public bool CallServerTurnOnCampFire(uint objectId)
        {
            if (!CurrentGameplayRule.CanInteractEntity(this, objectId))
                return false;
            RPC(ServerTurnOnCampFire, objectId);
            return true;
        }

        public bool CallServerTurnOffCampFire(uint objectId)
        {
            if (!CurrentGameplayRule.CanInteractEntity(this, objectId))
                return false;
            RPC(ServerTurnOffCampFire, objectId);
            return true;
        }

        public bool CallServerCraftItemByWorkbench(uint objectId, int dataId)
        {
            RPC(ServerCraftItemByWorkbench, objectId, dataId);
            return true;
        }

        public bool CallServerSetBuildingPassword(uint objectId, string password)
        {
            if (!CurrentGameplayRule.CanInteractEntity(this, objectId))
                return false;
            RPC(ServerSetBuildingPassword, objectId, password);
            return true;
        }

        public bool CallServerLockBuilding(uint objectId)
        {
            if (!CurrentGameplayRule.CanInteractEntity(this, objectId))
                return false;
            RPC(ServerLockBuilding, objectId);
            return true;
        }

        public bool CallServerUnlockBuilding(uint objectId)
        {
            if (!CurrentGameplayRule.CanInteractEntity(this, objectId))
                return false;
            RPC(ServerUnlockBuilding, objectId);
            return true;
        }

        public bool CallServerAppendCraftingQueueItem(uint sourceObjectId, int dataId, short amount)
        {
            if (!CurrentGameplayRule.CanInteractEntity(this, sourceObjectId))
                return false;
            RPC(ServerAppendCraftingQueueItem, sourceObjectId, dataId, amount);
            return true;
        }

        public bool CallServerChangeCraftingQueueItem(uint sourceObjectId, int indexOfData, short amount)
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
