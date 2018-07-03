using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class PlayerCharacterEntity
    {
        public void RequestSwapOrMergeItem(int fromIndex, int toIndex)
        {
            if (IsDead())
                return;
            CallNetFunction("SwapOrMergeItem", FunctionReceivers.Server, fromIndex, toIndex);
        }

        public void RequestAddAttribute(int attributeIndex, short amount)
        {
            if (IsDead())
                return;
            CallNetFunction("AddAttribute", FunctionReceivers.Server, attributeIndex, amount);
        }

        public void RequestAddSkill(int skillIndex, short amount)
        {
            if (IsDead())
                return;
            CallNetFunction("AddSkill", FunctionReceivers.Server, skillIndex, amount);
        }

        public void RequestRespawn()
        {
            CallNetFunction("Respawn", FunctionReceivers.Server);
        }

        public void RequestAssignHotkey(string hotkeyId, HotkeyType type, int dataId)
        {
            CallNetFunction("AssignHotkey", FunctionReceivers.Server, hotkeyId, (byte)type, dataId);
        }

        public void RequestNpcActivate(uint objectId)
        {
            if (IsDead())
                return;
            CallNetFunction("NpcActivate", FunctionReceivers.Server, objectId);
        }

        public void RequestShowNpcDialog(int npcDialogDataId)
        {
            if (IsDead())
                return;
            CallNetFunction("ShowNpcDialog", ConnectId, npcDialogDataId);
        }

        public void RequestSelectNpcDialogMenu(int menuIndex)
        {
            if (IsDead())
                return;
            CallNetFunction("SelectNpcDialogMenu", FunctionReceivers.Server, menuIndex);
        }

        public void RequestBuyNpcItem(int itemIndex, short amount)
        {
            if (IsDead())
                return;
            CallNetFunction("BuyNpcItem", FunctionReceivers.Server, itemIndex, amount);
        }

        public void RequestEnterWarp()
        {
            if (IsDead() || IsPlayingActionAnimation() || warpingPortal == null)
                return;
            CallNetFunction("EnterWarp", FunctionReceivers.Server);
        }

        public void RequestBuild(int index, Vector3 position, Quaternion rotation, uint parentObjectId)
        {
            if (IsDead() || IsPlayingActionAnimation())
                return;
            CallNetFunction("Build", FunctionReceivers.Server, index, position, rotation, parentObjectId);
        }

        public void RequestDestroyBuilding(uint objectId)
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
    }
}
