using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        [ServerRpc]
        protected void ServerNpcActivate(PackedUInt objectId)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            NpcEntity npcEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out npcEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(npcEntity, CurrentGameInstance.conversationDistance))
            {
                // Too far from the entity
                return;
            }

            CurrentNpcDialog = npcEntity.StartDialog;
            if (CurrentNpcDialog != null)
                CallOwnerShowNpcDialog(CurrentNpcDialog.DataId);
#endif
        }

        [TargetRpc]
        protected void TargetShowNpcDialog(int dataId)
        {
            // Show npc dialog by dataId, if dataId = 0 it will hide
            if (onShowNpcDialog != null)
                onShowNpcDialog.Invoke(dataId);
        }

        [TargetRpc]
        protected void TargetShowNpcRefineItem()
        {
            // Hide npc dialog
            if (onShowNpcDialog != null)
                onShowNpcDialog.Invoke(0);

            // Show refine dialog
            if (onShowNpcRefineItem != null)
                onShowNpcRefineItem.Invoke();
        }

        [TargetRpc]
        protected void TargetShowNpcDismantleItem()
        {
            // Hide npc dialog
            if (onShowNpcDialog != null)
                onShowNpcDialog.Invoke(0);

            // Show dismantle dialog
            if (onShowNpcDismantleItem != null)
                onShowNpcDismantleItem.Invoke();
        }

        [TargetRpc]
        protected void TargetShowNpcRepairItem()
        {
            // Hide npc dialog
            if (onShowNpcDialog != null)
                onShowNpcDialog.Invoke(0);

            // Show repair dialog
            if (onShowNpcRepairItem != null)
                onShowNpcRepairItem.Invoke();
        }

        [ServerRpc]
        protected void ServerSelectNpcDialogMenu(byte menuIndex)
        {
#if !CLIENT_BUILD
            if (CurrentNpcDialog == null)
                return;

            CurrentNpcDialog = CurrentNpcDialog.GetNextDialog(this, menuIndex);
            if (CurrentNpcDialog != null)
            {
                // Show Npc dialog on client
                CallOwnerShowNpcDialog(CurrentNpcDialog.DataId);
            }
            else
            {
                // Hide Npc dialog on client
                CallOwnerShowNpcDialog(0);
            }
#endif
        }

        protected bool AccessingNpcShopDialog(out NpcDialog dialog)
        {
            dialog = null;

            if (this.IsDead())
                return false;

            if (CurrentNpcDialog == null)
                return false;

            // Dialog must be built-in shop dialog
            dialog = CurrentNpcDialog as NpcDialog;
            if (dialog == null || dialog.type != NpcDialogType.Shop)
                return false;

            return true;
        }

        [ServerRpc]
        protected void ServerSellItem(short index, short amount)
        {
#if !CLIENT_BUILD
            if (!AccessingNpcShopDialog(out _))
                return;

            if (index >= nonEquipItems.Count)
                return;

            // Found selling item or not?
            CharacterItem nonEquipItem = nonEquipItems[index];
            if (nonEquipItem.IsEmptySlot() || amount > nonEquipItem.amount)
                return;

            // Remove item from inventory
            BaseItem item = nonEquipItem.GetItem();
            if (!this.DecreaseItemsByIndex(index, amount))
                return;
            this.FillEmptySlots();

            // Increase currencies
            CurrentGameplayRule.IncreaseCurrenciesWhenSellItem(this, item, amount);
#endif
        }

        [ServerRpc]
        protected void ServerSellItems(List<short> indexes)
        {
#if !CLIENT_BUILD
            if (!AccessingNpcShopDialog(out _))
                return;
            indexes.Sort();
            short index;
            for (int i = indexes.Count - 1; i >= 0; --i)
            {
                index = indexes[i];
                if (index >= nonEquipItems.Count)
                    continue;
                ServerSellItem(index, nonEquipItems[index].amount);
            }
#endif
        }

        [ServerRpc]
        protected void ServerBuyNpcItem(short index, short amount)
        {
#if !CLIENT_BUILD
            // Dialog must be built-in shop dialog
            NpcDialog dialog;
            if (!AccessingNpcShopDialog(out dialog))
                return;

            // Found buying item or not?
            NpcSellItem[] sellItems = dialog.sellItems;
            if (sellItems == null || index >= sellItems.Length)
                return;

            // Currencies enough or not?
            NpcSellItem sellItem = sellItems[index];
            if (!CurrentGameplayRule.CurrenciesEnoughToBuyItem(this, sellItem, amount))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.NotEnoughGold);
                return;
            }

            // Can carry or not?
            int dataId = sellItem.item.DataId;
            if (this.IncreasingItemsWillOverwhelming(dataId, amount))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotCarryAnymore);
                return;
            }

            // Decrease currencies
            CurrentGameplayRule.DecreaseCurrenciesWhenBuyItem(this, sellItem, amount);

            // Add item to inventory
            this.IncreaseItems(CharacterItem.Create(dataId, 1, amount));
            this.FillEmptySlots();
            CurrentGameManager.SendNotifyRewardItem(ConnectionId, dataId, amount);
#endif
        }
    }
}
