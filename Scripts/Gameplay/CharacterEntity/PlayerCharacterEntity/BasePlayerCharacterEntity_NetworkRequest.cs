using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        public virtual void RequestSwapOrMergeItem(short fromIndex, short toIndex)
        {
            if (IsDead())
                return;
            CallNetFunction(NetFuncSwapOrMergeItem, FunctionReceivers.Server, fromIndex, toIndex);
        }

        public virtual void RequestAddAttribute(int dataId)
        {
            if (IsDead())
                return;
            CallNetFunction(NetFuncAddAttribute, FunctionReceivers.Server, dataId);
        }

        public virtual void RequestAddSkill(int dataId)
        {
            if (IsDead())
                return;
            CallNetFunction(NetFuncAddSkill, FunctionReceivers.Server, dataId);
        }

        public virtual void RequestAddGuildSkill(int dataId)
        {
            if (IsDead())
                return;
            CallNetFunction(NetFuncAddGuildSkill, FunctionReceivers.Server, dataId);
        }

        public virtual void RequestUseGuildSkill(int dataId)
        {
            if (IsDead())
                return;
            CallNetFunction(NetFuncUseGuildSkill, FunctionReceivers.Server, dataId);
        }

        public virtual void RequestRespawn()
        {
            if (!IsDead())
                return;
            CallNetFunction(NetFuncRespawn, FunctionReceivers.Server);
        }

        public virtual void RequestAssignHotkey(string hotkeyId, HotkeyType type, int dataId)
        {
            CallNetFunction(NetFuncAssignHotkey, FunctionReceivers.Server, hotkeyId, (byte)type, dataId);
        }

        public virtual void RequestNpcActivate(uint objectId)
        {
            if (IsDead())
                return;
            CallNetFunction(NetFuncNpcActivate, FunctionReceivers.Server, new PackedUInt(objectId));
        }

        public virtual void RequestShowNpcDialog(int dataId)
        {
            if (IsDead())
                return;
            CallNetFunction(NetFuncShowNpcDialog, ConnectionId, dataId);
        }

        public virtual void RequestShowNpcRefine()
        {
            if (IsDead())
                return;
            CallNetFunction(NetFuncShowNpcRefine, ConnectionId);
        }

        public virtual void RequestSelectNpcDialogMenu(byte menuIndex)
        {
            if (IsDead())
                return;
            CallNetFunction(NetFuncSelectNpcDialogMenu, FunctionReceivers.Server, menuIndex);
        }

        public virtual void RequestBuyNpcItem(short itemIndex, short amount)
        {
            if (IsDead())
                return;
            CallNetFunction(NetFuncBuyNpcItem, FunctionReceivers.Server, itemIndex, amount);
        }

        public virtual void RequestEnterWarp()
        {
            if (!CanDoActions() || warpingPortal == null)
                return;
            CallNetFunction(NetFuncEnterWarp, FunctionReceivers.Server);
        }

        public virtual void RequestBuild(short itemIndex, Vector3 position, Quaternion rotation, uint parentObjectId)
        {
            if (!CanDoActions())
                return;
            CallNetFunction(NetFuncBuild, FunctionReceivers.Server, itemIndex, position, rotation, new PackedUInt(parentObjectId));
        }

        public virtual void RequestDestroyBuilding(uint objectId)
        {
            if (!CanDoActions())
                return;
            CallNetFunction(NetFuncDestroyBuilding, FunctionReceivers.Server, new PackedUInt(objectId));
        }

        public virtual void RequestSellItem(short nonEquipIndex, short amount)
        {
            if (IsDead() ||
                nonEquipIndex >= NonEquipItems.Count)
                return;
            CallNetFunction(NetFuncSellItem, FunctionReceivers.Server, nonEquipIndex, amount);
        }

        public virtual void RequestRefineItem(byte byteInventoryType, short index)
        {
            if (IsDead() ||
                index >= NonEquipItems.Count)
                return;
            CallNetFunction(NetFuncRefineItem, FunctionReceivers.Server, byteInventoryType, index);
        }

        public virtual void RequestRepairItem(byte byteInventoryType, short index)
        {
            if (IsDead() ||
                index >= NonEquipItems.Count)
                return;
            CallNetFunction(NetFuncRepairItem, FunctionReceivers.Server, byteInventoryType, index);
        }

        public virtual void RequestSendDealingRequest(uint objectId)
        {
            CallNetFunction(NetFuncSendDealingRequest, FunctionReceivers.Server, new PackedUInt(objectId));
        }

        public virtual void RequestReceiveDealingRequest(uint objectId)
        {
            CallNetFunction(NetFuncReceiveDealingRequest, ConnectionId, new PackedUInt(objectId));
        }

        public virtual void RequestAcceptDealingRequest()
        {
            CallNetFunction(NetFuncAcceptDealingRequest, FunctionReceivers.Server);
        }

        public virtual void RequestDeclineDealingRequest()
        {
            CallNetFunction(NetFuncDeclineDealingRequest, FunctionReceivers.Server);
        }

        public virtual void RequestAcceptedDealingRequest(uint objectId)
        {
            CallNetFunction(NetFuncAcceptedDealingRequest, ConnectionId, new PackedUInt(objectId));
        }

        public virtual void RequestSetDealingItem(short itemIndex, short amount)
        {
            CallNetFunction(NetFuncSetDealingItem, FunctionReceivers.Server, itemIndex, amount);
        }

        public virtual void RequestSetDealingGold(int dealingGold)
        {
            CallNetFunction(NetFuncSetDealingGold, FunctionReceivers.Server, dealingGold);
        }

        public virtual void RequestLockDealing()
        {
            CallNetFunction(NetFuncLockDealing, FunctionReceivers.Server);
        }

        public virtual void RequestConfirmDealing()
        {
            CallNetFunction(NetFuncConfirmDealing, FunctionReceivers.Server);
        }

        public virtual void RequestCancelDealing()
        {
            CallNetFunction(NetFuncCancelDealing, FunctionReceivers.Server);
        }

        public virtual void RequestUpdateDealingState(DealingState state)
        {
            CallNetFunction(NetFuncUpdateDealingState, ConnectionId, (byte)state);
        }

        public virtual void RequestUpdateAnotherDealingState(DealingState state)
        {
            CallNetFunction(NetFuncUpdateAnotherDealingState, ConnectionId, (byte)state);
        }

        public virtual void RequestUpdateDealingGold(int gold)
        {
            CallNetFunction(NetFuncUpdateDealingGold, ConnectionId, gold);
        }

        public virtual void RequestUpdateAnotherDealingGold(int gold)
        {
            CallNetFunction(NetFuncUpdateAnotherDealingGold, ConnectionId, gold);
        }

        public virtual void RequestUpdateDealingItems(DealingCharacterItems dealingItems)
        {
            CallNetFunction(NetFuncUpdateDealingItems, ConnectionId, dealingItems);
        }

        public virtual void RequestUpdateAnotherDealingItems(DealingCharacterItems dealingItems)
        {
            CallNetFunction(NetFuncUpdateAnotherDealingItems, ConnectionId, dealingItems);
        }

        public virtual void RequestCreateParty(bool shareExp, bool shareItem)
        {
            CallNetFunction(NetFuncCreateParty, FunctionReceivers.Server, shareExp, shareItem);
        }

        public virtual void RequestChangePartyLeader(string characterId)
        {
            CallNetFunction(NetFuncChangePartyLeader, FunctionReceivers.Server, characterId);
        }

        public virtual void RequestPartySetting(bool shareExp, bool shareItem)
        {
            CallNetFunction(NetFuncPartySetting, FunctionReceivers.Server, shareExp, shareItem);
        }

        public virtual void RequestSendPartyInvitation(uint objectId)
        {
            CallNetFunction(NetFuncSendPartyInvitation, FunctionReceivers.Server, new PackedUInt(objectId));
        }

        public virtual void RequestReceivePartyInvitation(uint objectId)
        {
            CallNetFunction(NetFuncReceivePartyInvitation, ConnectionId, new PackedUInt(objectId));
        }

        public virtual void RequestAcceptPartyInvitation()
        {
            CallNetFunction(NetFuncAcceptPartyInvitation, FunctionReceivers.Server);
        }

        public virtual void RequestDeclinePartyInvitation()
        {
            CallNetFunction(NetFuncDeclinePartyInvitation, FunctionReceivers.Server);
        }

        public virtual void RequestKickFromParty(string characterId)
        {
            CallNetFunction(NetFuncKickFromParty, FunctionReceivers.Server, characterId);
        }

        public virtual void RequestLeaveParty()
        {
            CallNetFunction(NetFuncLeaveParty, FunctionReceivers.Server);
        }

        public virtual void RequestCreateGuild(string guildName)
        {
            CallNetFunction(NetFuncCreateGuild, FunctionReceivers.Server, guildName);
        }

        public virtual void RequestChangeGuildLeader(string characterId)
        {
            CallNetFunction(NetFuncChangeGuildLeader, FunctionReceivers.Server, characterId);
        }

        public virtual void RequestSetGuildMessage(string guildMessage)
        {
            CallNetFunction(NetFuncSetGuildMessage, FunctionReceivers.Server, guildMessage);
        }

        public virtual void RequestSetGuildRole(byte guildRole, string name, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            CallNetFunction(NetFuncSetGuildRole, FunctionReceivers.Server, guildRole, name, canInvite, canKick, shareExpPercentage);
        }

        public virtual void RequestSetGuildMemberRole(string characterId, byte guildRole)
        {
            CallNetFunction(NetFuncSetGuildMemberRole, FunctionReceivers.Server, characterId, guildRole);
        }

        public virtual void RequestSendGuildInvitation(uint objectId)
        {
            CallNetFunction(NetFuncSendGuildInvitation, FunctionReceivers.Server, new PackedUInt(objectId));
        }

        public virtual void RequestReceiveGuildInvitation(uint objectId)
        {
            CallNetFunction(NetFuncReceiveGuildInvitation, ConnectionId, new PackedUInt(objectId));
        }

        public virtual void RequestAcceptGuildInvitation()
        {
            CallNetFunction(NetFuncAcceptGuildInvitation, FunctionReceivers.Server);
        }

        public virtual void RequestDeclineGuildInvitation()
        {
            CallNetFunction(NetFuncDeclineGuildInvitation, FunctionReceivers.Server);
        }

        public virtual void RequestKickFromGuild(string characterId)
        {
            CallNetFunction(NetFuncKickFromGuild, FunctionReceivers.Server, characterId);
        }

        public virtual void RequestLeaveGuild()
        {
            CallNetFunction(NetFuncLeaveGuild, FunctionReceivers.Server);
        }
    }
}
