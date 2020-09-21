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
                return;

            if (GameplayUtils.BoundsDistance(WorldBounds, npcEntity.WorldBounds) > CurrentGameInstance.conversationDistance)
                return;

            CurrentNpcDialog = npcEntity.StartDialog;
            if (CurrentNpcDialog != null)
                RequestShowNpcDialog(CurrentNpcDialog.DataId);
#endif
        }

        protected void NetFuncShowNpcDialog(int dataId)
        {
            // Show npc dialog by dataId, if dataId = 0 it will hide
            if (onShowNpcDialog != null)
                onShowNpcDialog.Invoke(dataId);
        }

        protected void NetFuncShowNpcRefineItem()
        {
            // Hide npc dialog
            if (onShowNpcDialog != null)
                onShowNpcDialog.Invoke(0);

            // Show refine dialog
            if (onShowNpcRefineItem != null)
                onShowNpcRefineItem.Invoke();
        }

        protected void NetFuncShowNpcDismantleItem()
        {
            // Hide npc dialog
            if (onShowNpcDialog != null)
                onShowNpcDialog.Invoke(0);

            // Show dismantle dialog
            if (onShowNpcDismantleItem != null)
                onShowNpcDismantleItem.Invoke();
        }

        protected void NetFuncShowNpcRepairItem()
        {
            // Hide npc dialog
            if (onShowNpcDialog != null)
                onShowNpcDialog.Invoke(0);

            // Show dismantle dialog
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
                RequestShowNpcDialog(CurrentNpcDialog.DataId);
            }
            else
            {
                // Hide Npc dialog on client
                RequestShowNpcDialog(0);
            }
#endif
        }

        [ServerRpc]
        protected void ServerSellItem(short index, short amount)
        {
#if !CLIENT_BUILD
            if (this.IsDead() || index >= nonEquipItems.Count)
                return;

            if (CurrentNpcDialog == null)
                return;

            // Dialog must be built-in shop dialog
            NpcDialog builtInDialog = CurrentNpcDialog as NpcDialog;
            if (builtInDialog == null || builtInDialog.type != NpcDialogType.Shop)
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
        protected void ServerBuyNpcItem(short itemIndex, short amount)
        {
#if !CLIENT_BUILD
            if (CurrentNpcDialog == null)
                return;

            // Dialog must be built-in shop dialog
            NpcDialog builtInDialog = CurrentNpcDialog as NpcDialog;
            if (builtInDialog == null || builtInDialog.type != NpcDialogType.Shop)
                return;

            // Found buying item or not?
            NpcSellItem[] sellItems = builtInDialog.sellItems;
            if (sellItems == null || itemIndex >= sellItems.Length)
                return;

            // Currencies enough or not?
            NpcSellItem sellItem = sellItems[itemIndex];
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
