using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        [ServerRpc]
        protected void ServerChangeQuestTracking(int questDataId, bool isTracking)
        {
            ChangeQuestTracking(questDataId, isTracking);
        }

        public virtual void ChangeQuestTracking(int questDataId, bool isTracking)
        {
            int indexOfQuest = this.IndexOfQuest(questDataId);
            if (indexOfQuest < 0 || !GameInstance.Quests.TryGetValue(questDataId, out Quest quest))
                return;
            CharacterQuest characterQuest = quests[indexOfQuest];
            characterQuest.isTracking = isTracking;
            quests[indexOfQuest] = characterQuest;
        }

        public virtual void AcceptQuest(int questDataId)
        {
            int indexOfQuest = this.IndexOfQuest(questDataId);
            if (indexOfQuest >= 0 || !GameInstance.Quests.TryGetValue(questDataId, out Quest quest))
                return;
            if (quest.abandonQuests != null && quest.abandonQuests.Length > 0)
            {
                for (int i = 0; i < quest.abandonQuests.Length; ++i)
                {
                    AbandonQuest(quest.abandonQuests[i].DataId);
                }
            }
            CharacterQuest characterQuest = CharacterQuest.Create(quest);
            quests.Add(characterQuest);
        }

        public virtual void AbandonQuest(int questDataId)
        {
            int indexOfQuest = this.IndexOfQuest(questDataId);
            if (indexOfQuest < 0 || !GameInstance.Quests.TryGetValue(questDataId, out Quest quest))
                return;
            CharacterQuest characterQuest = quests[indexOfQuest];
            if (characterQuest.isComplete)
                return;
            quests.RemoveAt(indexOfQuest);
        }

        public virtual bool CompleteQuest(int questDataId, byte selectedRewardIndex)
        {
            int indexOfQuest = this.IndexOfQuest(questDataId);
            if (indexOfQuest < 0 || !GameInstance.Quests.TryGetValue(questDataId, out Quest quest))
                return false;

            CharacterQuest characterQuest = quests[indexOfQuest];
            if (!characterQuest.IsAllTasksDone(this, out _))
                return false;

            if (characterQuest.isComplete)
                return false;

            Reward reward = CurrentGameplayRule.MakeQuestReward(quest);
            List<ItemAmount> rewardItems = new List<ItemAmount>();
            // Prepare reward items
            if (quest.selectableRewardItems != null &&
                quest.selectableRewardItems.Length > 0)
            {
                if (selectedRewardIndex >= quest.selectableRewardItems.Length)
                {
                    GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_INVALID_ITEM_INDEX);
                    return false;
                }
                rewardItems.Add(quest.selectableRewardItems[selectedRewardIndex]);
            }
            if (quest.randomRewardItems != null &&
                quest.randomRewardItems.Length > 0)
            {
                Dictionary<ItemRandomByWeight, int> randomItems = new Dictionary<ItemRandomByWeight, int>();
                foreach (ItemRandomByWeight item in quest.randomRewardItems)
                {
                    if (item.item == null || item.maxAmount <= 0 || item.randomWeight <= 0)
                        continue;
                    randomItems[item] = item.randomWeight;
                }
                ItemRandomByWeight randomedItem = WeightedRandomizer.From(randomItems).TakeOne();
                if (randomedItem.minAmount <= 0)
                {
                    rewardItems.Add(new ItemAmount()
                    {
                        item = randomedItem.item,
                        amount = randomedItem.maxAmount,
                    });
                }
                else
                {
                    rewardItems.Add(new ItemAmount()
                    {
                        item = randomedItem.item,
                        amount = Random.Range(randomedItem.minAmount, randomedItem.maxAmount),
                    });
                }
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
                return false;
            }
            // Decrease task items
            QuestTask[] tasks = quest.tasks;
            foreach (QuestTask task in tasks)
            {
                switch (task.taskType)
                {
                    case QuestTaskType.CollectItem:
                        if (!task.doNotDecreaseItemsOnQuestComplete)
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
                        GameInstance.ServerGameMessageHandlers.NotifyRewardItem(ConnectionId, RewardGivenType.Quest, rewardItem.item.DataId, rewardItem.amount);
                    }
                }
            }
            this.FillEmptySlots();
            // Reset attributes
            if (quest.resetAttributes)
                this.ResetAttributes();
            // Reset skills
            if (quest.resetSkills)
                this.ResetSkills();
            // Change character class
            if (quest.changeCharacterClass != null)
                DataId = quest.changeCharacterClass.DataId;
            // Change character faction
            if (quest.changeCharacterFaction != null)
                FactionId = quest.changeCharacterFaction.DataId;
            // Add exp
            RewardExp(reward, 1f, RewardGivenType.Quest);
            // Add currency
            RewardCurrencies(reward, 1f, RewardGivenType.Quest);
            // Add stat points
            checked
            {
                StatPoint += quest.rewardStatPoints;
            }
            // Add skill points
            checked
            {
                SkillPoint += quest.rewardSkillPoints;
            }
            // Set quest state
            characterQuest.isComplete = true;
            if (!quest.canRepeat)
                quests[indexOfQuest] = characterQuest;
            else
                quests.RemoveAt(indexOfQuest);
            return true;
        }
    }
}
