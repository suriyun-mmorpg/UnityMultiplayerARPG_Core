using UnityEngine;
using LiteNetLibManager;

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

        public bool RequestUseItem(short index)
        {
            if (!ValidateRequestUseItem(index))
                return false;
            CallNetFunction(ServerUseItem, FunctionReceivers.Server, index);
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

            int dataId = skill.DataId;
            float currentTime = Time.unscaledTime;

            GameMessage.Type gameMessageType;
            if (!skill.CanUse(this, skillLevel, isLeftHand, out gameMessageType, true))
            {
                QueueGameMessage(gameMessageType);
                return false;
            }

            return true;
        }

        public bool RequestUseSkillItem(short index, bool isLeftHand)
        {
            if (!ValidateRequestUseSkillItem(index, isLeftHand))
                return false;
            CallNetFunction(ServerUseSkillItem, FunctionReceivers.Server, index, isLeftHand);
            return true;
        }

        public bool RequestUseSkillItem(short index, bool isLeftHand, Vector3 aimPosition)
        {
            if (!ValidateRequestUseSkillItem(index, isLeftHand))
                return false;
            CallNetFunction(ServerUseSkillItemWithAimPosition, FunctionReceivers.Server, index, isLeftHand, aimPosition);
            return true;
        }

        public bool RequestSwapOrMergeItem(short fromIndex, short toIndex)
        {
            if (this.IsDead())
                return false;
            CallNetFunction(ServerSwapOrMergeItem, FunctionReceivers.Server, fromIndex, toIndex);
            return true;
        }

        public bool RequestAddAttribute(int dataId)
        {
            if (this.IsDead())
                return false;
            CallNetFunction(NetFuncAddAttribute, FunctionReceivers.Server, dataId);
            return true;
        }

        public bool RequestAddSkill(int dataId)
        {
            if (this.IsDead())
                return false;
            CallNetFunction(ServerAddSkill, FunctionReceivers.Server, dataId);
            return true;
        }

        public bool RequestAddGuildSkill(int dataId)
        {
            if (this.IsDead())
                return false;
            CallNetFunction(ServerAddGuildSkill, FunctionReceivers.Server, dataId);
            return true;
        }

        public bool RequestUseGuildSkill(int dataId)
        {
            if (this.IsDead())
                return false;
            CallNetFunction(ServerUseGuildSkill, FunctionReceivers.Server, dataId);
            return true;
        }

        public bool RequestRespawn()
        {
            if (!this.IsDead())
                return false;
            CallNetFunction(ServerRespawn, FunctionReceivers.Server);
            return true;
        }

        public bool RequestAssignHotkey(string hotkeyId, HotkeyType type, string id)
        {
            CallNetFunction(ServerAssignHotkey, FunctionReceivers.Server, hotkeyId, type, id);
            return true;
        }

        public bool RequestAssignItemHotkey(string hotkeyId, CharacterItem characterItem)
        {
            // Usable items will use item data id
            string relateId = characterItem.GetItem().Id;
            // For an equipments, it will use item unique id
            if (characterItem.GetEquipmentItem() != null)
            {
                relateId = characterItem.id;
            }
            return RequestAssignHotkey(hotkeyId, HotkeyType.Item, relateId);
        }

        public bool RequestAssignSkillHotkey(string hotkeyId, BaseSkill skill)
        {
            return RequestAssignHotkey(hotkeyId, HotkeyType.Skill, skill.Id);
        }

        public bool RequestUnAssignHotkey(string hotkeyId)
        {
            return RequestAssignHotkey(hotkeyId, HotkeyType.None, string.Empty);
        }

        public bool RequestNpcActivate(uint objectId)
        {
            if (this.IsDead())
                return false;
            CallNetFunction(ServerNpcActivate, FunctionReceivers.Server, new PackedUInt(objectId));
            return true;
        }

        public bool RequestShowNpcDialog(int dataId)
        {
            if (this.IsDead())
                return false;
            CallNetFunction(NetFuncShowNpcDialog, ConnectionId, dataId);
            return true;
        }

        public bool RequestShowNpcRefineItem()
        {
            if (this.IsDead())
                return false;
            CallNetFunction(NetFuncShowNpcRefineItem, ConnectionId);
            return true;
        }

        public bool RequestShowNpcDismantleItem()
        {
            if (this.IsDead())
                return false;
            CallNetFunction(NetFuncShowNpcDismantleItem, ConnectionId);
            return true;
        }

        public bool RequestShowNpcRepairItem()
        {
            if (this.IsDead())
                return false;
            CallNetFunction(NetFuncShowNpcRepairItem, ConnectionId);
            return true;
        }

        public bool RequestSelectNpcDialogMenu(byte menuIndex)
        {
            if (this.IsDead())
                return false;
            CallNetFunction(ServerSelectNpcDialogMenu, FunctionReceivers.Server, menuIndex);
            return true;
        }

        public bool RequestBuyNpcItem(short itemIndex, short amount)
        {
            if (this.IsDead())
                return false;
            CallNetFunction(ServerBuyNpcItem, FunctionReceivers.Server, itemIndex, amount);
            return true;
        }

        public bool RequestEnterWarp(uint objectId)
        {
            if (!CanDoActions())
                return false;
            CallNetFunction(ServerEnterWarp, FunctionReceivers.Server, new PackedUInt(objectId));
            return true;
        }

        public bool RequestBuild(short itemIndex, Vector3 position, Quaternion rotation, uint parentObjectId)
        {
            if (!CanDoActions())
                return false;
            CallNetFunction(ServerBuild, FunctionReceivers.Server, itemIndex, position, rotation, new PackedUInt(parentObjectId));
            return true;
        }

        public bool RequestDestroyBuilding(uint objectId)
        {
            if (!CanDoActions())
                return false;
            CallNetFunction(ServerDestroyBuilding, FunctionReceivers.Server, new PackedUInt(objectId));
            return true;
        }

        public bool RequestSellItem(short nonEquipIndex, short amount)
        {
            if (this.IsDead() || nonEquipIndex >= NonEquipItems.Count)
                return false;
            CallNetFunction(ServerSellItem, FunctionReceivers.Server, nonEquipIndex, amount);
            return true;
        }

        public bool RequestDismantleItem(short nonEquipIndex)
        {
            if (this.IsDead() || nonEquipIndex >= NonEquipItems.Count)
                return false;
            CallNetFunction(ServerDismantleItem, FunctionReceivers.Server, nonEquipIndex);
            return true;
        }

        public bool RequestRefineItem(InventoryType inventoryType, short index)
        {
            if (this.IsDead())
                return false;
            CallNetFunction(ServerRefineItem, FunctionReceivers.Server, inventoryType, index);
            return true;
        }

        public bool RequestEnhanceSocketItem(InventoryType inventoryType, short index, int enhancerId)
        {
            if (this.IsDead())
                return false;
            CallNetFunction(ServerEnhanceSocketItem, FunctionReceivers.Server, inventoryType, index, enhancerId);
            return true;
        }

        public bool RequestRepairItem(InventoryType inventoryType, short index)
        {
            if (this.IsDead())
                return false;
            CallNetFunction(ServerRepairItem, FunctionReceivers.Server, inventoryType, index);
            return true;
        }

        public bool RequestSendDealingRequest(uint objectId)
        {
            CallNetFunction(ServerSendDealingRequest, FunctionReceivers.Server, new PackedUInt(objectId));
            return true;
        }

        public bool RequestReceiveDealingRequest(uint objectId)
        {
            CallNetFunction(NetFuncReceiveDealingRequest, ConnectionId, new PackedUInt(objectId));
            return true;
        }

        public bool RequestAcceptDealingRequest()
        {
            CallNetFunction(ServerAcceptDealingRequest, FunctionReceivers.Server);
            return true;
        }

        public bool RequestDeclineDealingRequest()
        {
            CallNetFunction(ServerDeclineDealingRequest, FunctionReceivers.Server);
            return true;
        }

        public bool RequestAcceptedDealingRequest(uint objectId)
        {
            CallNetFunction(NetFuncAcceptedDealingRequest, ConnectionId, new PackedUInt(objectId));
            return true;
        }

        public bool RequestSetDealingItem(short itemIndex, short amount)
        {
            CallNetFunction(ServerSetDealingItem, FunctionReceivers.Server, itemIndex, amount);
            return true;
        }

        public bool RequestSetDealingGold(int dealingGold)
        {
            CallNetFunction(ServerSetDealingGold, FunctionReceivers.Server, dealingGold);
            return true;
        }

        public bool RequestLockDealing()
        {
            CallNetFunction(ServerLockDealing, FunctionReceivers.Server);
            return true;
        }

        public bool RequestConfirmDealing()
        {
            CallNetFunction(ServerConfirmDealing, FunctionReceivers.Server);
            return true;
        }

        public bool RequestCancelDealing()
        {
            CallNetFunction(ServerCancelDealing, FunctionReceivers.Server);
            return true;
        }

        public bool RequestUpdateDealingState(DealingState state)
        {
            CallNetFunction(NetFuncUpdateDealingState, ConnectionId, state);
            return true;
        }

        public bool RequestUpdateAnotherDealingState(DealingState state)
        {
            CallNetFunction(NetFuncUpdateAnotherDealingState, ConnectionId, state);
            return true;
        }

        public bool RequestUpdateDealingGold(int gold)
        {
            CallNetFunction(NetFuncUpdateDealingGold, ConnectionId, gold);
            return true;
        }

        public bool RequestUpdateAnotherDealingGold(int gold)
        {
            CallNetFunction(NetFuncUpdateAnotherDealingGold, ConnectionId, gold);
            return true;
        }

        public bool RequestUpdateDealingItems(DealingCharacterItems dealingItems)
        {
            CallNetFunction(NetFuncUpdateDealingItems, ConnectionId, dealingItems);
            return true;
        }

        public bool RequestUpdateAnotherDealingItems(DealingCharacterItems dealingItems)
        {
            CallNetFunction(NetFuncUpdateAnotherDealingItems, ConnectionId, dealingItems);
            return true;
        }

        public bool RequestCreateParty(bool shareExp, bool shareItem)
        {
            CallNetFunction(ServerCreateParty, FunctionReceivers.Server, shareExp, shareItem);
            return true;
        }

        public bool RequestChangePartyLeader(string characterId)
        {
            CallNetFunction(ServerChangePartyLeader, FunctionReceivers.Server, characterId);
            return true;
        }

        public bool RequestPartySetting(bool shareExp, bool shareItem)
        {
            CallNetFunction(ServerPartySetting, FunctionReceivers.Server, shareExp, shareItem);
            return true;
        }

        public bool RequestSendPartyInvitation(uint objectId)
        {
            CallNetFunction(ServerSendPartyInvitation, FunctionReceivers.Server, new PackedUInt(objectId));
            return true;
        }

        public bool RequestReceivePartyInvitation(uint objectId)
        {
            CallNetFunction(NetFuncReceivePartyInvitation, ConnectionId, new PackedUInt(objectId));
            return true;
        }

        public bool RequestAcceptPartyInvitation()
        {
            CallNetFunction(ServerAcceptPartyInvitation, FunctionReceivers.Server);
            return true;
        }

        public bool RequestDeclinePartyInvitation()
        {
            CallNetFunction(ServerDeclinePartyInvitation, FunctionReceivers.Server);
            return true;
        }

        public bool RequestKickFromParty(string characterId)
        {
            CallNetFunction(ServerKickFromParty, FunctionReceivers.Server, characterId);
            return true;
        }

        public bool RequestLeaveParty()
        {
            CallNetFunction(ServerLeaveParty, FunctionReceivers.Server);
            return true;
        }

        public bool RequestCreateGuild(string guildName)
        {
            CallNetFunction(ServerCreateGuild, FunctionReceivers.Server, guildName);
            return true;
        }

        public bool RequestChangeGuildLeader(string characterId)
        {
            CallNetFunction(ServerChangeGuildLeader, FunctionReceivers.Server, characterId);
            return true;
        }

        public bool RequestSetGuildMessage(string guildMessage)
        {
            CallNetFunction(ServerSetGuildMessage, FunctionReceivers.Server, guildMessage);
            return true;
        }

        public bool RequestSetGuildRole(byte guildRole, string name, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            CallNetFunction(ServerSetGuildRole, FunctionReceivers.Server, guildRole, name, canInvite, canKick, shareExpPercentage);
            return true;
        }

        public bool RequestSetGuildMemberRole(string characterId, byte guildRole)
        {
            CallNetFunction(ServerSetGuildMemberRole, FunctionReceivers.Server, characterId, guildRole);
            return true;
        }

        public bool RequestSendGuildInvitation(uint objectId)
        {
            CallNetFunction(ServerSendGuildInvitation, FunctionReceivers.Server, new PackedUInt(objectId));
            return true;
        }

        public bool RequestReceiveGuildInvitation(uint objectId)
        {
            CallNetFunction(NetFuncReceiveGuildInvitation, ConnectionId, new PackedUInt(objectId));
            return true;
        }

        public bool RequestAcceptGuildInvitation()
        {
            CallNetFunction(ServerAcceptGuildInvitation, FunctionReceivers.Server);
            return true;
        }

        public bool RequestDeclineGuildInvitation()
        {
            CallNetFunction(ServerDeclineGuildInvitation, FunctionReceivers.Server);
            return true;
        }

        public bool RequestKickFromGuild(string characterId)
        {
            CallNetFunction(ServerKickFromGuild, FunctionReceivers.Server, characterId);
            return true;
        }

        public bool RequestLeaveGuild()
        {
            CallNetFunction(ServerLeaveGuild, FunctionReceivers.Server);
            return true;
        }

        public bool RequestMoveItemToStorage(short nonEquipIndex, short amount, short storageItemIndex)
        {
            CallNetFunction(ServerMoveItemToStorage, FunctionReceivers.Server, nonEquipIndex, amount, storageItemIndex);
            return true;
        }

        public bool RequestMoveItemFromStorage(short storageItemIndex, short amount, short nonEquipIndex)
        {
            CallNetFunction(ServerMoveItemFromStorage, FunctionReceivers.Server, storageItemIndex, amount, nonEquipIndex);
            return true;
        }

        public bool RequestSwapOrMergeStorageItem(short fromIndex, short toIndex)
        {
            CallNetFunction(ServerSwapOrMergeStorageItem, FunctionReceivers.Server, fromIndex, toIndex);
            return true;
        }

        public bool RequestDepositGold(int amount)
        {
            CallNetFunction(ServerDepositGold, FunctionReceivers.Server, amount);
            return true;
        }

        public bool RequestWithdrawGold(int amount)
        {
            CallNetFunction(ServerWithdrawGold, FunctionReceivers.Server, amount);
            return true;
        }

        public bool RequestDepositGuildGold(int amount)
        {
            CallNetFunction(ServerDepositGuildGold, FunctionReceivers.Server, amount);
            return true;
        }

        public bool RequestWithdrawGuildGold(int amount)
        {
            CallNetFunction(ServerWithdrawGuildGold, FunctionReceivers.Server, amount);
            return true;
        }

        public bool RequestOpenStorage(uint objectId, string password)
        {
            CallNetFunction(ServerOpenStorage, FunctionReceivers.Server, new PackedUInt(objectId), password);
            return true;
        }

        public bool RequestCloseStorage()
        {
            CallNetFunction(ServerCloseStorage, FunctionReceivers.Server);
            return true;
        }

        public bool RequestOpenDoor(uint objectId, string password)
        {
            CallNetFunction(ServerOpenDoor, FunctionReceivers.Server, new PackedUInt(objectId), password);
            return true;
        }

        public bool RequestCloseDoor(uint objectId)
        {
            CallNetFunction(ServerCloseDoor, FunctionReceivers.Server, new PackedUInt(objectId));
            return true;
        }

        public bool RequestTurnOnCampFire(uint objectId)
        {
            CallNetFunction(ServerTurnOnCampFire, FunctionReceivers.Server, new PackedUInt(objectId));
            return true;
        }

        public bool RequestTurnOffCampFire(uint objectId)
        {
            CallNetFunction(ServerTurnOffCampFire, FunctionReceivers.Server, new PackedUInt(objectId));
            return true;
        }

        public bool RequestCraftItemByWorkbench(uint objectId, int dataId)
        {
            CallNetFunction(ServerCraftItemByWorkbench, FunctionReceivers.Server, new PackedUInt(objectId), dataId);
            return true;
        }

        public bool RequestFindCharacters(string characterName)
        {
            CallNetFunction(ServerFindCharacters, FunctionReceivers.Server, characterName);
            return true;
        }

        public bool RequestAddFriend(string friendCharacterId)
        {
            CallNetFunction(ServerAddFriend, FunctionReceivers.Server, friendCharacterId);
            return true;
        }

        public bool RequestRemoveFriend(string friendCharacterId)
        {
            CallNetFunction(ServerRemoveFriend, FunctionReceivers.Server, friendCharacterId);
            return true;
        }

        public bool RequestGetFriends()
        {
            CallNetFunction(ServerGetFriends, FunctionReceivers.Server);
            return true;
        }

        public bool RequestSetBuildingPassword(uint objectId, string password)
        {
            CallNetFunction(ServerSetBuildingPassword, FunctionReceivers.Server, new PackedUInt(objectId), password);
            return true;
        }

        public bool RequestLockBuilding(uint objectId)
        {
            CallNetFunction(ServerLockBuilding, FunctionReceivers.Server, new PackedUInt(objectId));
            return true;
        }

        public bool RequestUnlockBuilding(uint objectId)
        {
            CallNetFunction(ServerUnlockBuilding, FunctionReceivers.Server, new PackedUInt(objectId));
            return true;
        }
    }
}
