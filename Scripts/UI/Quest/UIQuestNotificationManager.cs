using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIQuestNotificationManager : MonoBehaviour
    {
        [Tooltip("Format => {0} = {Exp Amount}")]
        public UILocaleKeySetting formatKeyQuestAccept = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_QUEST_TITLE_ON_GOING);
        public UILocaleKeySetting formatKeyQuestTaskUpdateKillMonster = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_QUEST_TASK_KILL_MONSTER);
        public UILocaleKeySetting formatKeyQuestTaskUpdateKillMonsterComplete = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_QUEST_TASK_KILL_MONSTER_COMPLETE);
        public UILocaleKeySetting formatKeyQuestTaskUpdateCollectItem = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_QUEST_TASK_COLLECT_ITEM);
        public UILocaleKeySetting formatKeyQuestTaskUpdateCollectItemComplete = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_QUEST_TASK_COLLECT_ITEM_COMPLETE);
        public UILocaleKeySetting formatKeyQuestTaskUpdateTalkToNpc = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_QUEST_TASK_TALK_TO_NPC);
        public UILocaleKeySetting formatKeyQuestTaskUpdateTalkToNpcComplete = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_QUEST_TASK_TALK_TO_NPC_COMPLETE);
        public UILocaleKeySetting formatKeyQuestComplete = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_QUEST_TITLE_COMPLETE);
        public TextWrapper questAcceptMessagePrefab;
        public TextWrapper questTaskUpdateMessagePrefab;
        public TextWrapper questCompleteMessagePrefab;
        public UIGameMessageHandler messageHandler;

        private List<CharacterQuest> comparingQuests = new List<CharacterQuest>();
        private HashSet<int> notifyingItemDataIds = new HashSet<int>();

        private void OnEnable()
        {
            Setup();
        }

        public void Setup()
        {
            comparingQuests.Clear();
            notifyingItemDataIds.Clear();
            CharacterQuest tempQuest;
            foreach (CharacterQuest quest in GameInstance.PlayingCharacterEntity.Quests)
            {
                tempQuest = quest.Clone();
                comparingQuests.Add(tempQuest);
                AddItemDataIdsForNotification(tempQuest);
            }
            GameInstance.PlayingCharacterEntity.onQuestsOperation += OnQuestsOperation;
            GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation += OnNonEquipItemsOperation;
        }

        public void Desetup()
        {
            GameInstance.PlayingCharacterEntity.onQuestsOperation -= OnQuestsOperation;
            GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation += OnNonEquipItemsOperation;
        }

        private void AddItemDataIdsForNotification(CharacterQuest characterQuest)
        {
            Quest quest = characterQuest.GetQuest();
            if (quest == null)
                return;
            foreach (QuestTask task in quest.tasks)
            {
                if (task.taskType == QuestTaskType.CollectItem && task.itemAmount.item)
                {
                    notifyingItemDataIds.Add(task.itemAmount.item.DataId);
                }
            }
        }

        private void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation op, int index)
        {
            BasePlayerCharacterEntity character = GameInstance.PlayingCharacterEntity;
            switch (op)
            {
                case LiteNetLibSyncList.Operation.Add:
                case LiteNetLibSyncList.Operation.Insert:
                case LiteNetLibSyncList.Operation.Set:
                case LiteNetLibSyncList.Operation.Dirty:
                    if (notifyingItemDataIds.Contains(character.NonEquipItems[index].dataId))
                    {
                        for (int i = 0; i < character.Quests.Count; ++i)
                        {
                            OnQuestsOperation(LiteNetLibSyncList.Operation.Dirty, i);
                        }
                        return;
                    }
                    break;
            }
        }

        private void OnQuestsOperation(LiteNetLibSyncList.Operation op, int index)
        {
            BasePlayerCharacterEntity character = GameInstance.PlayingCharacterEntity;
            Quest tempQuestData;
            TextWrapper newMessage;
            CharacterQuest tempCharacterQuest;
            switch (op)
            {
                case LiteNetLibSyncList.Operation.Clear:
                    comparingQuests.Clear();
                    break;
                case LiteNetLibSyncList.Operation.Add:
                case LiteNetLibSyncList.Operation.Insert:
                    tempCharacterQuest = character.Quests[index];
                    tempQuestData = tempCharacterQuest.GetQuest();
                    newMessage = messageHandler.AddMessage(questAcceptMessagePrefab);
                    newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestAccept.ToString()), tempQuestData.Title);
                    comparingQuests.Add(tempCharacterQuest);
                    AddItemDataIdsForNotification(tempCharacterQuest);
                    break;
                case LiteNetLibSyncList.Operation.Set:
                case LiteNetLibSyncList.Operation.Dirty:
                    tempCharacterQuest = character.Quests[index];
                    tempQuestData = tempCharacterQuest.GetQuest();
                    if (comparingQuests[index].isComplete && !tempCharacterQuest.isComplete)
                    {
                        // **Repeatable quests can be accepted again after completed**
                        newMessage = messageHandler.AddMessage(questAcceptMessagePrefab);
                        newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestAccept.ToString()), tempQuestData.Title);
                    }
                    else if (!comparingQuests[index].isComplete && tempCharacterQuest.isComplete)
                    {
                        newMessage = messageHandler.AddMessage(questCompleteMessagePrefab);
                        newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestComplete.ToString()), tempQuestData.Title);
                    }
                    else if (!comparingQuests[index].isComplete)
                    {
                        QuestTask[] tasks = tempQuestData.tasks;
                        for (int i = 0; i < tasks.Length; ++i)
                        {
                            string taskTitle;
                            int maxProgress;
                            bool updatingIsComplete;
                            int updatingProgress = tempCharacterQuest.GetProgress(character, i, out taskTitle, out maxProgress, out updatingIsComplete);

                            bool comparingIsComplete;
                            int comparingProgress = comparingQuests[index].GetProgress(character, i, out _, out _, out comparingIsComplete);

                            if ((comparingIsComplete != updatingIsComplete || comparingProgress != updatingProgress) && !comparingIsComplete)
                            {
                                switch (tasks[i].taskType)
                                {
                                    case QuestTaskType.KillMonster:
                                        newMessage = messageHandler.AddMessage(questTaskUpdateMessagePrefab);
                                        if (updatingProgress >= maxProgress)
                                            newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestTaskUpdateKillMonsterComplete.ToString()), taskTitle, updatingProgress, maxProgress);
                                        else
                                            newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestTaskUpdateKillMonster.ToString()), taskTitle, updatingProgress, maxProgress);
                                        break;
                                    case QuestTaskType.CollectItem:
                                        newMessage = messageHandler.AddMessage(questTaskUpdateMessagePrefab);
                                        if (updatingProgress >= maxProgress)
                                            newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestTaskUpdateCollectItemComplete.ToString()), taskTitle, updatingProgress, maxProgress);
                                        else
                                            newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestTaskUpdateCollectItem.ToString()), taskTitle, updatingProgress, maxProgress);
                                        break;
                                    case QuestTaskType.TalkToNpc:
                                        newMessage = messageHandler.AddMessage(questTaskUpdateMessagePrefab);
                                        if (updatingProgress >= maxProgress)
                                            newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestTaskUpdateTalkToNpcComplete.ToString()), taskTitle, updatingProgress, maxProgress);
                                        else
                                            newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestTaskUpdateTalkToNpc.ToString()), taskTitle, updatingProgress, maxProgress);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    comparingQuests[index] = tempCharacterQuest.Clone();
                    break;
                case LiteNetLibSyncList.Operation.RemoveAt:
                    comparingQuests.RemoveAt(index);
                    break;
                case LiteNetLibSyncList.Operation.RemoveFirst:
                    comparingQuests.RemoveAt(0);
                    break;
                case LiteNetLibSyncList.Operation.RemoveLast:
                    comparingQuests.RemoveAt(comparingQuests.Count - 1);
                    break;
            }
        }
    }
}
