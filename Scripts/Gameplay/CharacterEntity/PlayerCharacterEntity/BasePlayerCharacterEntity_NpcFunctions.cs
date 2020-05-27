using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        protected void NetFuncNpcActivate(PackedUInt objectId)
        {
            if (!CanDoActions())
                return;

            NpcEntity npcEntity = null;
            if (!Manager.TryGetEntityByObjectId(objectId, out npcEntity))
                return;

            if (GameplayUtils.BoundsDistance(WorldBounds, npcEntity.WorldBounds) > CurrentGameInstance.conversationDistance)
                return;

            CurrentNpcDialog = npcEntity.StartDialog;
            if (CurrentNpcDialog != null)
                RequestShowNpcDialog(CurrentNpcDialog.DataId);
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

        protected void NetFuncSelectNpcDialogMenu(byte menuIndex)
        {
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
        }

        protected void NetFuncBuyNpcItem(short itemIndex, short amount)
        {
            if (CurrentNpcDialog == null)
                return;
            NpcSellItem[] sellItems = CurrentNpcDialog.sellItems;
            if (sellItems == null || itemIndex >= sellItems.Length)
                return;
            NpcSellItem sellItem = sellItems[itemIndex];
            if (!CurrentGameplayRule.CurrenciesEnoughToBuyItem(this, sellItem, amount))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.NotEnoughGold);
                return;
            }
            int dataId = sellItem.item.DataId;
            if (this.IncreasingItemsWillOverwhelming(dataId, amount))
            {
                CurrentGameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotCarryAnymore);
                return;
            }
            CurrentGameplayRule.DecreaseCurrenciesWhenBuyItem(this, sellItem, amount);
            this.IncreaseItems(CharacterItem.Create(dataId, 1, amount));
            this.FillEmptySlots();
            CurrentGameManager.SendNotifyRewardItem(ConnectionId, dataId, amount);
        }
    }
}
