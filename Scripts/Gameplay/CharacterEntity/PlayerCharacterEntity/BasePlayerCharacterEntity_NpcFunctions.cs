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
            if (!this.TryGetEntityByObjectId(objectId, out npcEntity))
                return;

            if (Vector3.Distance(CacheTransform.position, npcEntity.CacheTransform.position) > gameInstance.conversationDistance + 5f)
                return;

            currentNpcDialog = npcEntity.StartDialog;
            if (currentNpcDialog != null)
                RequestShowNpcDialog(currentNpcDialog.DataId);
        }

        protected void NetFuncShowNpcDialog(int dataId)
        {
            // Show npc dialog by dataId, if dataId = 0 it will hide
            if (onShowNpcDialog != null)
                onShowNpcDialog.Invoke(dataId);
        }

        protected void NetFuncShowNpcRefine()
        {
            // Hide npc dialog
            if (onShowNpcDialog != null)
                onShowNpcDialog.Invoke(0);

            // Show refine dialog
            if (onShowNpcRefine != null)
                onShowNpcRefine.Invoke();
        }

        protected void NetFuncSelectNpcDialogMenu(byte menuIndex)
        {
            if (currentNpcDialog == null)
                return;

            currentNpcDialog = currentNpcDialog.GetNextDialog(this, menuIndex);
            if (currentNpcDialog != null)
            {
                // Show Npc dialog on client
                RequestShowNpcDialog(currentNpcDialog.DataId);
            }
            else
            {
                // Hide Npc dialog on client
                RequestShowNpcDialog(0);
            }
        }

        protected void NetFuncBuyNpcItem(short itemIndex, short amount)
        {
            if (currentNpcDialog == null)
                return;
            NpcSellItem[] sellItems = currentNpcDialog.sellItems;
            if (sellItems == null || itemIndex >= sellItems.Length)
                return;
            NpcSellItem sellItem = sellItems[itemIndex];
            if (!gameplayRule.CurrenciesEnoughToBuyItem(this, sellItem, amount))
            {
                gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.NotEnoughGold);
                return;
            }
            int dataId = sellItem.item.DataId;
            if (this.IncreasingItemsWillOverwhelming(dataId, amount))
            {
                gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotCarryAnymore);
                return;
            }
            gameplayRule.DecreaseCurrenciesWhenBuyItem(this, sellItem, amount);
            this.IncreaseItems(CharacterItem.Create(dataId, 1, amount));
        }
    }
}
