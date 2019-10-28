using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
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

        public void CompleteQuest(int questDataId)
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

            Reward reward = gameplayRule.MakeQuestReward(quest);
            if (this.IncreasingItemsWillOverwhelming(quest.rewardItems))
            {
                // Overwhelming
                gameManager.SendServerGameMessage(ConnectionId, GameMessage.Type.CannotCarryAnymore);
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
            if (quest.rewardItems != null && quest.rewardItems.Length > 0)
            {
                foreach (ItemAmount rewardItem in quest.rewardItems)
                {
                    if (rewardItem.item != null && rewardItem.amount > 0)
                        this.IncreaseItems(CharacterItem.Create(rewardItem.item, 1, rewardItem.amount));
                }
            }
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
