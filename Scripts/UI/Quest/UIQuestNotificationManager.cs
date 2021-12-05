using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIQuestNotificationManager : MonoBehaviour
    {
        private class QuestRecord
        {
            public int dataId;
            public bool isComplete;
            public List<QuestTaskRecord> tasks = new List<QuestTaskRecord>();
        }

        private class QuestTaskRecord
        {
            public int progress;
            public bool isComplete;
        }

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

        private List<QuestRecord> comparingQuests = new List<QuestRecord>();

        private void OnEnable()
        {
            Setup();
        }

        public void Setup()
        {
            MakeComparingQuests();
            GameInstance.PlayingCharacterEntity.onQuestsOperation += OnQuestsOperation;
            GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation += OnNonEquipItemsOperation;
        }

        public void Desetup()
        {
            GameInstance.PlayingCharacterEntity.onQuestsOperation -= OnQuestsOperation;
            GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
        }

        private QuestRecord MakeRecord(CharacterQuest characterQuest)
        {
            QuestRecord record = new QuestRecord();
            record.dataId = characterQuest.dataId;
            record.isComplete = characterQuest.isComplete;
            Quest questData = characterQuest.GetQuest();
            QuestTask[] tasks = questData.tasks;
            for (int i = 0; i < tasks.Length; ++i)
            {
                bool isComplete;
                int progress = characterQuest.GetProgress(GameInstance.PlayingCharacterEntity, i, out _, out _, out isComplete);
                record.tasks.Add(new QuestTaskRecord()
                {
                    progress = progress,
                    isComplete = isComplete,
                });
            }
            return record;
        }

        private void MakeComparingQuests()
        {
            comparingQuests.Clear();
            foreach (CharacterQuest characterQuest in GameInstance.PlayingCharacterEntity.Quests)
            {
                comparingQuests.Add(MakeRecord(characterQuest));
            }
        }

        private void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation op, int index)
        {
            TextWrapper newMessage;
            BasePlayerCharacterEntity character = GameInstance.PlayingCharacterEntity;
            CharacterQuest tempCharacterQuest;
            Quest tempQuestData;
            QuestTask[] tempTasks;
            switch (op)
            {
                case LiteNetLibSyncList.Operation.Add:
                case LiteNetLibSyncList.Operation.Insert:
                case LiteNetLibSyncList.Operation.Set:
                case LiteNetLibSyncList.Operation.Dirty:
                    for (int i = 0; i < character.Quests.Count; ++i)
                    {
                        tempCharacterQuest = character.Quests[i];
                        if (tempCharacterQuest.isComplete)
                            continue;
                        tempQuestData = tempCharacterQuest.GetQuest();
                        tempTasks = tempQuestData.tasks;
                        for (int j = 0; j < tempTasks.Length; ++j)
                        {
                            if (tempTasks[j].taskType != QuestTaskType.CollectItem || !tempTasks[j].itemAmount.item || tempTasks[j].itemAmount.amount <= 0)
                                continue;
                            string taskTitle;
                            int maxProgress;
                            bool updatingIsComplete;
                            int updatingProgress = tempCharacterQuest.GetProgress(character, j, out taskTitle, out maxProgress, out updatingIsComplete);
                            if (comparingQuests[i].tasks[j].progress != updatingProgress)
                            {
                                newMessage = messageHandler.AddMessage(questCompleteMessagePrefab);
                                if (updatingProgress >= maxProgress)
                                    newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestTaskUpdateCollectItemComplete.ToString()), taskTitle, updatingProgress, maxProgress);
                                else
                                    newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestTaskUpdateCollectItem.ToString()), taskTitle, updatingProgress, maxProgress);
                                comparingQuests[i].tasks[j].progress = updatingProgress;
                                comparingQuests[i].tasks[j].isComplete = updatingIsComplete;
                            }
                            break;
                        }
                    }
                    break;
                case LiteNetLibSyncList.Operation.RemoveAt:
                case LiteNetLibSyncList.Operation.RemoveFirst:
                case LiteNetLibSyncList.Operation.RemoveLast:
                    for (int i = 0; i < character.Quests.Count; ++i)
                    {
                        tempCharacterQuest = character.Quests[i];
                        if (tempCharacterQuest.isComplete)
                            continue;
                        tempQuestData = tempCharacterQuest.GetQuest();
                        tempTasks = tempQuestData.tasks;
                        for (int j = 0; j < tempTasks.Length; ++j)
                        {
                            if (tempTasks[j].taskType != QuestTaskType.CollectItem || !tempTasks[j].itemAmount.item || tempTasks[j].itemAmount.amount <= 0)
                                continue;
                            comparingQuests[i].tasks[j].progress = character.CountNonEquipItems(character.NonEquipItems[index].dataId);
                            break;
                        }
                    }
                    break;
            }
        }

        private void OnQuestsOperation(LiteNetLibSyncList.Operation op, int index)
        {
            TextWrapper newMessage;
            BasePlayerCharacterEntity character = GameInstance.PlayingCharacterEntity;
            CharacterQuest tempCharacterQuest;
            Quest tempQuestData;
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
                    comparingQuests.Add(MakeRecord(tempCharacterQuest));
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
                        comparingQuests[index] = MakeRecord(tempCharacterQuest);
                    }
                    else if (!comparingQuests[index].isComplete && tempCharacterQuest.isComplete)
                    {
                        newMessage = messageHandler.AddMessage(questCompleteMessagePrefab);
                        newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestComplete.ToString()), tempQuestData.Title);
                        comparingQuests[index] = MakeRecord(tempCharacterQuest);
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

                            bool comparingIsComplete = comparingQuests[index].tasks[i].isComplete;
                            int comparingProgress = comparingQuests[index].tasks[i].progress;

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
                                    case QuestTaskType.TalkToNpc:
                                        newMessage = messageHandler.AddMessage(questTaskUpdateMessagePrefab);
                                        if (updatingProgress >= maxProgress)
                                            newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestTaskUpdateTalkToNpcComplete.ToString()), taskTitle, updatingProgress, maxProgress);
                                        else
                                            newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestTaskUpdateTalkToNpc.ToString()), taskTitle, updatingProgress, maxProgress);
                                        break;
                                    default:
                                        break;
                                } // End switch
                            } // End if
                        } // End for
                        comparingQuests[index] = MakeRecord(tempCharacterQuest);
                    }
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
