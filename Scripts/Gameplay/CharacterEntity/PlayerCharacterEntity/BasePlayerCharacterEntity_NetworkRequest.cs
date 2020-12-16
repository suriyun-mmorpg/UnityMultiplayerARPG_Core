using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        public bool ValidateRequestUseItem(short index)
        {
            if (!CanUseItem())
                return false;

            float time = Time.unscaledTime;
            if (time - lastActionTime < ACTION_DELAY)
                return false;
            lastActionTime = time;

            if (index >= nonEquipItems.Count)
                return false;

            if (nonEquipItems[index].IsLock())
                return false;

            IUsableItem item = nonEquipItems[index].GetUsableItem();
            if (item == null)
                return false;

            return true;
        }

        public bool CallServerUseItem(short index)
        {
            if (!ValidateRequestUseItem(index))
                return false;
            RPC(ServerUseItem, index);
            return true;
        }

        public bool ValidateRequestUseSkillItem(short index, bool isLeftHand)
        {
            if (!CanUseItem())
                return false;

            float time = Time.unscaledTime;
            if (time - lastActionTime < ACTION_DELAY)
                return false;
            lastActionTime = time;

            if (index >= nonEquipItems.Count)
                return false;

            if (nonEquipItems[index].IsLock())
                return false;

            ISkillItem item = nonEquipItems[index].GetSkillItem();
            if (item == null)
                return false;

            BaseSkill skill = item.UsingSkill;
            short skillLevel = item.UsingSkillLevel;
            if (skill == null)
                return false;

            GameMessage.Type gameMessageType;
            if (!skill.CanUse(this, skillLevel, isLeftHand, out gameMessageType, true))
            {
                QueueGameMessage(gameMessageType);
                return false;
            }

            return true;
        }

        public bool CallServerUseSkillItem(short index, bool isLeftHand)
        {
            if (!ValidateRequestUseSkillItem(index, isLeftHand))
                return false;
            RPC(ServerUseSkillItem, index, isLeftHand);
            return true;
        }

        public bool CallServerUseSkillItem(short index, bool isLeftHand, Vector3 aimPosition)
        {
            if (!ValidateRequestUseSkillItem(index, isLeftHand))
                return false;
            RPC(ServerUseSkillItemWithAimPosition, index, isLeftHand, aimPosition);
            return true;
        }

        public bool CallServerAddAttribute(int dataId)
        {
            if (this.IsDead())
                return false;
            RPC(ServerAddAttribute, dataId);
            return true;
        }

        public bool CallServerAddSkill(int dataId)
        {
            if (this.IsDead())
                return false;
            RPC(ServerAddSkill, dataId);
            return true;
        }

        public bool CallServerUseGuildSkill(int dataId)
        {
            if (this.IsDead())
                return false;
            RPC(ServerUseGuildSkill, dataId);
            return true;
        }

        public bool CallServerRespawn()
        {
            if (!this.IsDead())
                return false;
            RPC(ServerRespawn);
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
            RPC(ServerNpcActivate, new PackedUInt(objectId));
            return true;
        }

        public bool CallOwnerShowNpcDialog(int dataId)
        {
            if (this.IsDead())
                return false;
            RPC(TargetShowNpcDialog, ConnectionId, dataId);
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

        public bool CallServerSellItem(short nonEquipIndex, short amount)
        {
            if (this.IsDead() || nonEquipIndex >= NonEquipItems.Count)
                return false;
            RPC(ServerSellItem, nonEquipIndex, amount);
            return true;
        }

        public bool CallServerSellItems(short[] selectedIndexes)
        {
            if (this.IsDead() || selectedIndexes.Length == 0)
                return false;
            RPC(ServerSellItems, selectedIndexes);
            return true;
        }

        public bool CallServerDismantleItem(short nonEquipIndex, short amount)
        {
            if (this.IsDead() || nonEquipIndex >= NonEquipItems.Count)
                return false;
            RPC(ServerDismantleItem, nonEquipIndex, amount);
            return true;
        }

        public bool CallServerDismantleItems(short[] selectedIndexes)
        {
            if (this.IsDead() || selectedIndexes.Length == 0)
                return false;
            RPC(ServerDismantleItems, selectedIndexes);
            return true;
        }

        public bool CallServerRefineItem(InventoryType inventoryType, short index)
        {
            if (this.IsDead())
                return false;
            RPC(ServerRefineItem, inventoryType, index);
            return true;
        }

        public bool CallServerEnhanceSocketItem(InventoryType inventoryType, short index, int enhancerId, short socketIndex)
        {
            if (this.IsDead())
                return false;
            RPC(ServerEnhanceSocketItem, inventoryType, index, enhancerId, socketIndex);
            return true;
        }

        public bool CallServerRemoveEnhancerFromItem(InventoryType inventoryType, short index, short socketIndex)
        {
            if (this.IsDead())
                return false;
            RPC(ServerRemoveEnhancerFromItem, inventoryType, index, socketIndex);
            return true;
        }

        public bool CallServerRepairItem(InventoryType inventoryType, short index)
        {
            if (this.IsDead())
                return false;
            RPC(ServerRepairItem, inventoryType, index);
            return true;
        }

        public bool CallServerRepairEquipItems()
        {
            if (this.IsDead())
                return false;
            RPC(ServerRepairEquipItems);
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

        public bool CallServerDepositGold(int amount)
        {
            RPC(ServerDepositGold, amount);
            return true;
        }

        public bool CallServerWithdrawGold(int amount)
        {
            RPC(ServerWithdrawGold, amount);
            return true;
        }

        public bool CallServerDepositGuildGold(int amount)
        {
            RPC(ServerDepositGuildGold, amount);
            return true;
        }

        public bool CallServerWithdrawGuildGold(int amount)
        {
            RPC(ServerWithdrawGuildGold, amount);
            return true;
        }

        public bool CallServerOpenStorage(uint objectId, string password)
        {
            RPC(ServerOpenStorage, objectId, password);
            return true;
        }

        public bool CallServerCloseStorage()
        {
            RPC(ServerCloseStorage);
            return true;
        }

        public bool CallServerOpenDoor(uint objectId, string password)
        {
            RPC(ServerOpenDoor, objectId, password);
            return true;
        }

        public bool CallServerCloseDoor(uint objectId)
        {
            RPC(ServerCloseDoor, objectId);
            return true;
        }

        public bool CallServerTurnOnCampFire(uint objectId)
        {
            RPC(ServerTurnOnCampFire, objectId);
            return true;
        }

        public bool CallServerTurnOffCampFire(uint objectId)
        {
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
            RPC(ServerSetBuildingPassword, objectId, password);
            return true;
        }

        public bool CallServerLockBuilding(uint objectId)
        {
            RPC(ServerLockBuilding, objectId);
            return true;
        }

        public bool CallServerUnlockBuilding(uint objectId)
        {
            RPC(ServerUnlockBuilding, objectId);
            return true;
        }
    }
}
