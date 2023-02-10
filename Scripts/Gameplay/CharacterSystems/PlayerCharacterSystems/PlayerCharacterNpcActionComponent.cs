using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public partial class PlayerCharacterNpcActionComponent : BaseNetworkedGameEntityComponent<BasePlayerCharacterEntity>
    {
        public BaseNpcDialog CurrentNpcDialog { get; set; }
        public Quest CompletingQuest { get; set; }
        public BaseNpcDialog NpcDialogAfterSelectRewardItem { get; set; }

        /// <summary>
        /// Action: int questDataId
        /// </summary>
        public event System.Action<int> onShowQuestRewardItemSelection;
        /// <summary>
        /// Action: int npcDialogDataId
        /// </summary>
        public event System.Action<int> onShowNpcDialog;
        public event System.Action onShowNpcRefineItem;
        public event System.Action onShowNpcDismantleItem;
        public event System.Action onShowNpcRepairItem;

        public void ClearNpcDialogData()
        {
            CurrentNpcDialog = null;
            CompletingQuest = null;
            NpcDialogAfterSelectRewardItem = null;
        }

        public bool AccessingNpcShopDialog(out NpcDialog dialog)
        {
            dialog = null;

            if (Entity.IsDead())
                return false;

            if (CurrentNpcDialog == null)
                return false;

            // Dialog must be built-in shop dialog
            dialog = CurrentNpcDialog as NpcDialog;
            if (dialog == null || dialog.type != NpcDialogType.Shop)
                return false;

            return true;
        }

        #region Networking Functions
        public bool CallServerNpcActivate(uint objectId)
        {
            if (Entity.IsDead())
                return false;
            RPC(ServerNpcActivate, objectId);
            return true;
        }

        [ServerRpc]
        protected void ServerNpcActivate(uint objectId)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!Entity.CanDoActions())
                return;

            NpcEntity npcEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out npcEntity))
            {
                // Can't find the entity
                return;
            }

            if (!Entity.IsGameEntityInDistance(npcEntity, CurrentGameInstance.conversationDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            // Show start dialog
            CurrentNpcDialog = npcEntity.StartDialog;

            // Update task
            Quest quest;
            int taskIndex;
            BaseNpcDialog talkToNpcTaskDialog;
            bool completeAfterTalked;
            CharacterQuest characterQuest;
            for (int i = 0; i < Entity.Quests.Count; ++i)
            {
                characterQuest = Entity.Quests[i];
                if (characterQuest.isComplete)
                    continue;
                quest = characterQuest.GetQuest();
                if (quest == null || !quest.HaveToTalkToNpc(Entity, npcEntity, out taskIndex, out talkToNpcTaskDialog, out completeAfterTalked))
                    continue;
                CurrentNpcDialog = talkToNpcTaskDialog;
                if (!characterQuest.CompletedTasks.Contains(taskIndex))
                    characterQuest.CompletedTasks.Add(taskIndex);
                Entity.Quests[i] = characterQuest;
                if (completeAfterTalked && characterQuest.IsAllTasksDone(Entity))
                {
                    if (quest.selectableRewardItems != null &&
                        quest.selectableRewardItems.Length > 0)
                    {
                        // Show quest reward dialog at client
                        CallOwnerShowQuestRewardItemSelection(quest.DataId);
                        CompletingQuest = quest;
                        NpcDialogAfterSelectRewardItem = talkToNpcTaskDialog;
                        CurrentNpcDialog = null;
                    }
                    else
                    {
                        // No selectable reward items, complete the quest immediately
                        if (!Entity.CompleteQuest(quest.DataId, 0))
                            CurrentNpcDialog = null;
                    }
                    break;
                }
            }

            if (CurrentNpcDialog != null)
                CallOwnerShowNpcDialog(CurrentNpcDialog.DataId);
#endif
        }

        public bool CallOwnerShowQuestRewardItemSelection(int questDataId)
        {
            if (Entity.IsDead())
                return false;
            RPC(TargetShowQuestRewardItemSelection, ConnectionId, questDataId);
            return true;
        }

        [TargetRpc]
        protected void TargetShowQuestRewardItemSelection(int questDataId)
        {
            // Hide npc dialog
            if (onShowNpcDialog != null)
                onShowNpcDialog.Invoke(0);

            // Show quest reward dialog
            if (onShowQuestRewardItemSelection != null)
                onShowQuestRewardItemSelection.Invoke(questDataId);
        }

        public bool CallOwnerShowNpcDialog(int npcDialogDataId)
        {
            if (Entity.IsDead())
                return false;
            RPC(TargetShowNpcDialog, ConnectionId, npcDialogDataId);
            return true;
        }

        [TargetRpc]
        protected void TargetShowNpcDialog(int npcDialogDataId)
        {
            // Show npc dialog by dataId, if dataId = 0 it will hide
            if (onShowNpcDialog != null)
                onShowNpcDialog.Invoke(npcDialogDataId);
        }

        public bool CallOwnerShowNpcRefineItem()
        {
            if (Entity.IsDead())
                return false;
            RPC(TargetShowNpcRefineItem, ConnectionId);
            return true;
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

        public bool CallOwnerShowNpcDismantleItem()
        {
            if (Entity.IsDead())
                return false;
            RPC(TargetShowNpcDismantleItem, ConnectionId);
            return true;
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

        public bool CallOwnerShowNpcRepairItem()
        {
            if (Entity.IsDead())
                return false;
            RPC(TargetShowNpcRepairItem, ConnectionId);
            return true;
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

        public bool CallServerSelectNpcDialogMenu(byte menuIndex)
        {
            if (Entity.IsDead())
                return false;
            RPC(ServerSelectNpcDialogMenu, menuIndex);
            return true;
        }

        [ServerRpc]
        protected void ServerSelectNpcDialogMenu(byte menuIndex)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (CurrentNpcDialog == null)
                return;

            CurrentNpcDialog.GoToNextDialog(Entity, menuIndex);
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

        public bool CallServerHideNpcDialog()
        {
            RPC(ServerHideNpcDialog);
            return true;
        }

        [ServerRpc]
        protected void ServerHideNpcDialog()
        {
#if UNITY_EDITOR || UNITY_SERVER
            ClearNpcDialogData();
            CallOwnerShowNpcDialog(0);
#endif
        }

        public bool CallServerBuyNpcItem(int itemIndex, int amount)
        {
            if (amount <= 0 || Entity.IsDead())
                return false;
            RPC(ServerBuyNpcItem, itemIndex, amount);
            return true;
        }

        [ServerRpc]
        protected void ServerBuyNpcItem(int index, int amount)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (amount <= 0 || Entity.IsDead())
                return;

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
            if (!CurrentGameplayRule.CurrenciesEnoughToBuyItem(Entity, sellItem, amount))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD);
                return;
            }

            // Can carry or not?
            int dataId = sellItem.item.DataId;
            if (Entity.IncreasingItemsWillOverwhelming(dataId, amount))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_WILL_OVERWHELMING);
                return;
            }

            // Decrease currencies
            CurrentGameplayRule.DecreaseCurrenciesWhenBuyItem(Entity, sellItem, amount);

            // Add item to inventory
            Entity.IncreaseItems(CharacterItem.Create(dataId, 1, amount));
            Entity.FillEmptySlots();
            GameInstance.ServerGameMessageHandlers.NotifyRewardItem(ConnectionId, dataId, amount);
#endif
        }

        public bool CallServerSelectQuestRewardItem(byte itemIndex)
        {
            if (Entity.IsDead())
                return false;
            RPC(ServerSelectQuestRewardItem, itemIndex);
            return true;
        }

        [ServerRpc]
        protected void ServerSelectQuestRewardItem(byte index)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (CompletingQuest == null)
                return;

            if (!Entity.CompleteQuest(CompletingQuest.DataId, index))
                return;

            CurrentNpcDialog = NpcDialogAfterSelectRewardItem;
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

            // Clear data
            CompletingQuest = null;
            NpcDialogAfterSelectRewardItem = null;
#endif
        }
        #endregion
    }
}
