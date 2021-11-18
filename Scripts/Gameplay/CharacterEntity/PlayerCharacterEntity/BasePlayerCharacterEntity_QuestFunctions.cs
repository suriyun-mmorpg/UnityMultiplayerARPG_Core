using LiteNetLibManager;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        [ServerRpc]
        protected void ServerChangeQuestTracking(int questDataId, bool isTracking)
        {
            ChangeQuestTracking(questDataId, isTracking);
        }

        public void ChangeQuestTracking(int questDataId, bool isTracking)
        {
            int indexOfQuest = this.IndexOfQuest(questDataId);
            Quest quest;
            if (indexOfQuest < 0 || !GameInstance.Quests.TryGetValue(questDataId, out quest))
                return;
            CharacterQuest characterQuest = quests[indexOfQuest];
            characterQuest.isTracking = isTracking;
            quests[indexOfQuest] = characterQuest;
        }

        public void AcceptQuest(int questDataId)
        {
            int indexOfQuest = this.IndexOfQuest(questDataId);
            Quest quest;
            if (indexOfQuest >= 0 || !GameInstance.Quests.TryGetValue(questDataId, out quest))
                return;
            CharacterQuest characterQuest = CharacterQuest.Create(quest);
            quests.Add(characterQuest);
        }

        public void AbandonQuest(int questDataId)
        {
            int indexOfQuest = this.IndexOfQuest(questDataId);
            Quest quest;
            if (indexOfQuest < 0 || !GameInstance.Quests.TryGetValue(questDataId, out quest))
                return;
            CharacterQuest characterQuest = quests[indexOfQuest];
            if (characterQuest.isComplete)
                return;
            quests.RemoveAt(indexOfQuest);
        }

        public void CompleteQuest(int questDataId, byte selectedRewardIndex)
        {
            int indexOfQuest = this.IndexOfQuest(questDataId);
            Quest quest;
            if (indexOfQuest < 0 || !GameInstance.Quests.TryGetValue(questDataId, out quest))
                return;

            CharacterQuest characterQuest = quests[indexOfQuest];
            if (!characterQuest.IsAllTasksDone(this))
                return;

            if (characterQuest.isComplete)
                return;

            Reward reward = CurrentGameplayRule.MakeQuestReward(quest);
            List<ItemAmount> rewardItems = new List<ItemAmount>();
            // Prepare reward items
            if (quest.selectableRewardItems != null &&
                quest.selectableRewardItems.Length > 0)
            {
                if (selectedRewardIndex >= quest.selectableRewardItems.Length)
                {
                    GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_INVALID_ITEM_INDEX);
                    return;
                }
                rewardItems.Add(quest.selectableRewardItems[selectedRewardIndex]);
            }
            if (quest.randomRewardItems != null &&
                quest.randomRewardItems.Length > 0)
            {
                Dictionary<ItemRandomByWeight, int> randomItems = new Dictionary<ItemRandomByWeight, int>();
                foreach (ItemRandomByWeight item in quest.randomRewardItems)
                {
                    if (item.item == null || item.randomWeight <= 0)
                        continue;
                    randomItems[item] = item.randomWeight;
                }
                ItemRandomByWeight randomedItem = WeightedRandomizer.From(randomItems).TakeOne();
                rewardItems.Add(new ItemAmount()
                {
                    item = randomedItem.item,
                    amount = randomedItem.amount,
                });
            }
            if (quest.rewardItems != null &&
                quest.rewardItems.Length > 0)
            {
                rewardItems.AddRange(quest.rewardItems);
            }
            // Check that the character can carry all items or not
            if (rewardItems.Count > 0 && this.IncreasingItemsWillOverwhelming(rewardItems))
            {
                // Overwhelming
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_WILL_OVERWHELMING);
                return;
            }
            // Decrease task items
            QuestTask[] tasks = quest.tasks;
            foreach (QuestTask task in tasks)
            {
                switch (task.taskType)
                {
                    case QuestTaskType.CollectItem:
                        this.DecreaseItems(task.itemAmount.item.DataId, task.itemAmount.amount);
                        break;
                }
            }
            // Add reward items
            if (rewardItems.Count > 0)
            {
                foreach (ItemAmount rewardItem in rewardItems)
                {
                    if (rewardItem.item != null && rewardItem.amount > 0)
                    {
                        this.IncreaseItems(CharacterItem.Create(rewardItem.item, 1, rewardItem.amount));
                        GameInstance.ServerGameMessageHandlers.NotifyRewardItem(ConnectionId, rewardItem.item.DataId, rewardItem.amount);
                    }
                }
            }
            this.FillEmptySlots();
            // Add exp
            RewardExp(reward, 1f, RewardGivenType.Quest);
            // Add currency
            RewardCurrencies(reward, 1f, RewardGivenType.Quest);
            // Set quest state
            characterQuest.isComplete = true;
            if (!quest.canRepeat)
                quests[indexOfQuest] = characterQuest;
            else
                quests.RemoveAt(indexOfQuest);
        }
    }
}
