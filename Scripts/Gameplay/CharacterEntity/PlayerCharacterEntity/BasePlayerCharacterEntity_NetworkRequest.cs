using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        public virtual void RequestSwapOrMergeItem(ushort fromIndex, ushort toIndex)
        {
            if (IsDead())
                return;
            CallNetFunction("SwapOrMergeItem", FunctionReceivers.Server, fromIndex, toIndex);
        }

        public virtual void RequestAddAttribute(int dataId)
        {
            if (IsDead())
                return;
            CallNetFunction("AddAttribute", FunctionReceivers.Server, dataId);
        }

        public virtual void RequestAddSkill(int dataId)
        {
            if (IsDead())
                return;
            CallNetFunction("AddSkill", FunctionReceivers.Server, dataId);
        }

        public virtual void RequestAddGuildSkill(int dataId)
        {
            if (IsDead())
                return;
            CallNetFunction("AddGuildSkill", FunctionReceivers.Server, dataId);
        }

        public virtual void RequestUseGuildSkill(int dataId)
        {
            if (IsDead())
                return;
            CallNetFunction("UseGuildSkill", FunctionReceivers.Server, dataId);
        }

        public virtual void RequestRespawn()
        {
            if (!IsDead())
                return;
            CallNetFunction("Respawn", FunctionReceivers.Server);
        }

        public virtual void RequestAssignHotkey(string hotkeyId, HotkeyType type, int dataId)
        {
            CallNetFunction("AssignHotkey", FunctionReceivers.Server, hotkeyId, (byte)type, dataId);
        }

        public virtual void RequestNpcActivate(uint objectId)
        {
            if (IsDead())
                return;
            CallNetFunction("NpcActivate", FunctionReceivers.Server, objectId);
        }

        public virtual void RequestShowNpcDialog(int dataId)
        {
            if (IsDead())
                return;
            CallNetFunction("ShowNpcDialog", ConnectionId, dataId);
        }

        public virtual void RequestSelectNpcDialogMenu(byte menuIndex)
        {
            if (IsDead())
                return;
            CallNetFunction("SelectNpcDialogMenu", FunctionReceivers.Server, menuIndex);
        }

        public virtual void RequestBuyNpcItem(ushort itemIndex, short amount)
        {
            if (IsDead())
                return;
            CallNetFunction("BuyNpcItem", FunctionReceivers.Server, itemIndex, amount);
        }

        public virtual void RequestEnterWarp()
        {
            if (!CanMoveOrDoActions() || warpingPortal == null)
                return;
            CallNetFunction("EnterWarp", FunctionReceivers.Server);
        }

        public virtual void RequestBuild(int index, Vector3 position, Quaternion rotation, uint parentObjectId)
        {
            if (!CanMoveOrDoActions())
                return;
            CallNetFunction("Build", FunctionReceivers.Server, index, position, rotation, parentObjectId);
        }

        public virtual void RequestDestroyBuilding(uint objectId)
        {
            if (!CanMoveOrDoActions())
                return;
            CallNetFunction("DestroyBuild", FunctionReceivers.Server, objectId);
        }

        public virtual void RequestSellItem(ushort nonEquipIndex, short amount)
        {
            if (IsDead() ||
                nonEquipIndex >= NonEquipItems.Count)
                return;
            CallNetFunction("SellItem", FunctionReceivers.Server, nonEquipIndex, amount);
        }

        public virtual void RequestRefineItem(ushort nonEquipIndex)
        {
            if (IsDead() ||
                nonEquipIndex >= NonEquipItems.Count)
                return;
            CallNetFunction("RefineItem", FunctionReceivers.Server, nonEquipIndex);
        }

        public virtual void RequestSendDealingRequest(uint objectId)
        {
            CallNetFunction("SendDealingRequest", FunctionReceivers.Server, objectId);
        }

        public virtual void RequestReceiveDealingRequest(uint objectId)
        {
            CallNetFunction("ReceiveDealingRequest", ConnectionId, objectId);
        }

        public virtual void RequestAcceptDealingRequest()
        {
            CallNetFunction("AcceptDealingRequest", FunctionReceivers.Server);
        }

        public virtual void RequestDeclineDealingRequest()
        {
            CallNetFunction("DeclineDealingRequest", FunctionReceivers.Server);
        }

        public virtual void RequestAcceptedDealingRequest(uint objectId)
        {
            CallNetFunction("AcceptedDealingRequest", ConnectionId, objectId);
        }

        public virtual void RequestSetDealingItem(ushort itemIndex, short amount)
        {
            CallNetFunction("SetDealingItem", FunctionReceivers.Server, itemIndex, amount);
        }

        public virtual void RequestSetDealingGold(int dealingGold)
        {
            CallNetFunction("SetDealingGold", FunctionReceivers.Server, dealingGold);
        }

        public virtual void RequestLockDealing()
        {
            CallNetFunction("LockDealing", FunctionReceivers.Server);
        }

        public virtual void RequestConfirmDealing()
        {
            CallNetFunction("ConfirmDealing", FunctionReceivers.Server);
        }

        public virtual void RequestCancelDealing()
        {
            CallNetFunction("CancelDealing", FunctionReceivers.Server);
        }

        public virtual void RequestUpdateDealingState(DealingState state)
        {
            CallNetFunction("UpdateDealingState", ConnectionId, (byte)state);
        }

        public virtual void RequestUpdateAnotherDealingState(DealingState state)
        {
            CallNetFunction("UpdateAnotherDealingState", ConnectionId, (byte)state);
        }

        public virtual void RequestUpdateDealingGold(int gold)
        {
            CallNetFunction("UpdateDealingGold", ConnectionId, gold);
        }

        public virtual void RequestUpdateAnotherDealingGold(int gold)
        {
            CallNetFunction("UpdateAnotherDealingGold", ConnectionId, gold);
        }

        public virtual void RequestUpdateDealingItems(DealingCharacterItems dealingItems)
        {
            CallNetFunction("UpdateDealingItems", ConnectionId, dealingItems);
        }

        public virtual void RequestUpdateAnotherDealingItems(DealingCharacterItems dealingItems)
        {
            CallNetFunction("UpdateAnotherDealingItems", ConnectionId, dealingItems);
        }

        public virtual void RequestCreateParty(bool shareExp, bool shareItem)
        {
            CallNetFunction("CreateParty", FunctionReceivers.Server, shareExp, shareItem);
        }

        public virtual void RequestChangePartyLeader(string characterId)
        {
            CallNetFunction("ChangePartyLeader", FunctionReceivers.Server, characterId);
        }

        public virtual void RequestPartySetting(bool shareExp, bool shareItem)
        {
            CallNetFunction("PartySetting", FunctionReceivers.Server, shareExp, shareItem);
        }

        public virtual void RequestSendPartyInvitation(uint objectId)
        {
            CallNetFunction("SendPartyInvitation", FunctionReceivers.Server, objectId);
        }

        public virtual void RequestReceivePartyInvitation(uint objectId)
        {
            CallNetFunction("ReceivePartyInvitation", ConnectionId, objectId);
        }

        public virtual void RequestAcceptPartyInvitation()
        {
            CallNetFunction("AcceptPartyInvitation", FunctionReceivers.Server);
        }

        public virtual void RequestDeclinePartyInvitation()
        {
            CallNetFunction("DeclinePartyInvitation", FunctionReceivers.Server);
        }

        public virtual void RequestKickFromParty(string characterId)
        {
            CallNetFunction("KickFromParty", FunctionReceivers.Server, characterId);
        }

        public virtual void RequestLeaveParty()
        {
            CallNetFunction("LeaveParty", FunctionReceivers.Server);
        }

        public virtual void RequestCreateGuild(string guildName)
        {
            CallNetFunction("CreateGuild", FunctionReceivers.Server, guildName);
        }

        public virtual void RequestChangeGuildLeader(string characterId)
        {
            CallNetFunction("ChangeGuildLeader", FunctionReceivers.Server, characterId);
        }

        public virtual void RequestSetGuildMessage(string guildMessage)
        {
            CallNetFunction("SetGuildMessage", FunctionReceivers.Server, guildMessage);
        }

        public virtual void RequestSetGuildRole(byte guildRole, string name, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            CallNetFunction("SetGuildRole", FunctionReceivers.Server, guildRole, name, canInvite, canKick, shareExpPercentage);
        }

        public virtual void RequestSetGuildMemberRole(string characterId, byte guildRole)
        {
            CallNetFunction("SetGuildMemberRole", FunctionReceivers.Server, characterId, guildRole);
        }

        public virtual void RequestSendGuildInvitation(uint objectId)
        {
            CallNetFunction("SendGuildInvitation", FunctionReceivers.Server, objectId);
        }

        public virtual void RequestReceiveGuildInvitation(uint objectId)
        {
            CallNetFunction("ReceiveGuildInvitation", ConnectionId, objectId);
        }

        public virtual void RequestAcceptGuildInvitation()
        {
            CallNetFunction("AcceptGuildInvitation", FunctionReceivers.Server);
        }

        public virtual void RequestDeclineGuildInvitation()
        {
            CallNetFunction("DeclineGuildInvitation", FunctionReceivers.Server);
        }

        public virtual void RequestKickFromGuild(string characterId)
        {
            CallNetFunction("KickFromGuild", FunctionReceivers.Server, characterId);
        }

        public virtual void RequestLeaveGuild()
        {
            CallNetFunction("LeaveGuild", FunctionReceivers.Server);
        }
    }
}
