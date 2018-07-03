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
            if (IsDead() || IsPlayingActionAnimation() || warpingPortal == null)
                return;
            CallNetFunction("EnterWarp", FunctionReceivers.Server);
        }

        public virtual void RequestBuild(int index, Vector3 position, Quaternion rotation, uint parentObjectId)
        {
            if (IsDead() || IsPlayingActionAnimation())
                return;
            CallNetFunction("Build", FunctionReceivers.Server, index, position, rotation, parentObjectId);
        }

        public virtual void RequestDestroyBuilding(uint objectId)
        {
            if (IsDead() || IsPlayingActionAnimation())
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

        public virtual void RequestSendDealingOffer(uint objectId)
        {
            CallNetFunction("SendDealingOffer", FunctionReceivers.Server, objectId);
        }
    }
}
