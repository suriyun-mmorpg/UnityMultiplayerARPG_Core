using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        public virtual void RequestSwapOrMergeItem(int fromIndex, int toIndex)
        {
            if (IsDead())
                return;
            CallNetFunction("SwapOrMergeItem", FunctionReceivers.Server, fromIndex, toIndex);
        }

        public virtual void RequestAddAttribute(int attributeIndex, short amount)
        {
            if (IsDead())
                return;
            CallNetFunction("AddAttribute", FunctionReceivers.Server, attributeIndex, amount);
        }

        public virtual void RequestAddSkill(int skillIndex, short amount)
        {
            if (IsDead())
                return;
            CallNetFunction("AddSkill", FunctionReceivers.Server, skillIndex, amount);
        }

        public virtual void RequestRespawn()
        {
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

        public virtual void RequestShowNpcDialog(int npcDialogDataId)
        {
            if (IsDead())
                return;
            CallNetFunction("ShowNpcDialog", ConnectId, npcDialogDataId);
        }

        public virtual void RequestSelectNpcDialogMenu(int menuIndex)
        {
            if (IsDead())
                return;
            CallNetFunction("SelectNpcDialogMenu", FunctionReceivers.Server, menuIndex);
        }

        public virtual void RequestBuyNpcItem(int itemIndex, short amount)
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

        public virtual void RequestSellItem(int nonEquipIndex, short amount)
        {
            if (IsDead() ||
                nonEquipIndex < 0 ||
                nonEquipIndex >= NonEquipItems.Count)
                return;
            CallNetFunction("SellItem", FunctionReceivers.Server, nonEquipIndex, amount);
        }

        public virtual void RequestSendDealingRequest(uint objectId)
        {
            CallNetFunction("SendDealingRequest", FunctionReceivers.Server, objectId);
        }

        public virtual void RequestReceiveDealingRequest(uint objectId)
        {
            CallNetFunction("ReceiveDealingRequest", ConnectId, objectId);
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
            CallNetFunction("AcceptedDealingRequest", ConnectId, objectId);
        }

        public virtual void RequestSetDealingItem(int itemIndex, short amount)
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
            CallNetFunction("UpdateDealingState", ConnectId, (byte)state);
        }

        public virtual void RequestUpdateAnotherDealingState(DealingState state)
        {
            CallNetFunction("UpdateAnotherDealingState", ConnectId, (byte)state);
        }

        public virtual void RequestUpdateDealingGold(int gold)
        {
            CallNetFunction("UpdateDealingGold", ConnectId, gold);
        }

        public virtual void RequestUpdateAnotherDealingGold(int gold)
        {
            CallNetFunction("UpdateAnotherDealingGold", ConnectId, gold);
        }

        public virtual void RequestUpdateDealingItems(DealingCharacterItems dealingItems)
        {
            CallNetFunction("UpdateDealingItems", ConnectId, dealingItems);
        }

        public virtual void RequestUpdateAnotherDealingItems(DealingCharacterItems dealingItems)
        {
            CallNetFunction("UpdateAnotherDealingItems", ConnectId, dealingItems);
        }
    }
}
