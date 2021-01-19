using LiteNetLibManager;
using System.Collections.Generic;

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
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            CurrentNpcDialog = npcEntity.StartDialog;

            // Update task
            Quest quest;
            int taskIndex;
            BaseNpcDialog talkToNpcTaskDialog;
            bool completeAfterTalked;
            CharacterQuest characterQuest;
            for (int i = 0; i < Quests.Count; ++i)
            {
                characterQuest = Quests[i];
                if (characterQuest.isComplete)
                    continue;
                quest = characterQuest.GetQuest();
                if (quest == null || !quest.HaveToTalkToNpc(this, npcEntity, out taskIndex, out talkToNpcTaskDialog, out completeAfterTalked))
                    continue;
                CurrentNpcDialog = talkToNpcTaskDialog;
                characterQuest.CompletedTasks.Add(taskIndex);
                Quests[i] = characterQuest;
                if (completeAfterTalked && characterQuest.IsAllTasksDone(this))
                    CompleteQuest(quest.DataId);
            }

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
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD);
                return;
            }

            // Can carry or not?
            int dataId = sellItem.item.DataId;
            if (this.IncreasingItemsWillOverwhelming(dataId, amount))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_WILL_OVERWHELMING);
                return;
            }

            // Decrease currencies
            CurrentGameplayRule.DecreaseCurrenciesWhenBuyItem(this, sellItem, amount);

            // Add item to inventory
            this.IncreaseItems(CharacterItem.Create(dataId, 1, amount));
            this.FillEmptySlots();
            GameInstance.ServerGameMessageHandlers.NotifyRewardItem(ConnectionId, dataId, amount);
#endif
        }

        public bool AccessingNpcShopDialog(out NpcDialog dialog)
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
    }
}
