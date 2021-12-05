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

        private void OnEnable()
        {
            Setup();
        }

        public void Setup()
        {
            comparingQuests.Clear();
            foreach (CharacterQuest quest in GameInstance.PlayingCharacterEntity.Quests)
            {
                comparingQuests.Add(quest.Clone());
            }
            GameInstance.PlayingCharacterEntity.onQuestsOperation += OnQuestsOperation;
        }

        public void Desetup()
        {
            GameInstance.PlayingCharacterEntity.onQuestsOperation -= OnQuestsOperation;
        }

        private void OnQuestsOperation(LiteNetLibSyncList.Operation op, int index)
        {
            BasePlayerCharacterEntity character = GameInstance.PlayingCharacterEntity;
            Quest quest = character.Quests[index].GetQuest();
            TextWrapper newMessage;
            switch (op)
            {
                case LiteNetLibSyncList.Operation.Clear:
                    comparingQuests.Clear();
                    break;
                case LiteNetLibSyncList.Operation.Add:
                case LiteNetLibSyncList.Operation.Insert:
                    newMessage = messageHandler.AddMessage(questAcceptMessagePrefab);
                    newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestAccept.ToString()), quest.Title);
                    comparingQuests.Add(character.Quests[index]);
                    break;
                case LiteNetLibSyncList.Operation.Set:
                case LiteNetLibSyncList.Operation.Dirty:
                    if (comparingQuests[index].isComplete && !character.Quests[index].isComplete)
                    {
                        newMessage = messageHandler.AddMessage(questAcceptMessagePrefab);
                        newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestAccept.ToString()), quest.Title);
                    }
                    else if (!comparingQuests[index].isComplete && character.Quests[index].isComplete)
                    {
                        newMessage = messageHandler.AddMessage(questCompleteMessagePrefab);
                        newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestAccept.ToString()), quest.Title);
                    }
                    else if (!comparingQuests[index].isComplete)
                    {
                        QuestTask[] tasks = quest.tasks;
                        for (int i = 0; i < tasks.Length; ++i)
                        {
                            string taskTitle;
                            int updatingMaxProgress;
                            bool updatingIsComplete;
                            int updatingProgress = character.Quests[index].GetProgress(character, i, out taskTitle, out updatingMaxProgress, out updatingIsComplete);

                            int comparingMaxProgress;
                            bool comparingIsComplete;
                            int comparingProgress = comparingQuests[index].GetProgress(character, i, out _, out comparingMaxProgress, out comparingIsComplete);

                            if (comparingProgress != updatingProgress)
                            {
                                switch (tasks[i].taskType)
                                {
                                    case QuestTaskType.KillMonster:
                                        newMessage = messageHandler.AddMessage(questTaskUpdateMessagePrefab);
                                        if (updatingProgress >= updatingMaxProgress)
                                            newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestTaskUpdateKillMonsterComplete.ToString()), taskTitle, updatingProgress, updatingMaxProgress);
                                        else
                                            newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestTaskUpdateKillMonster.ToString()), taskTitle, updatingProgress, updatingMaxProgress);
                                        break;
                                    case QuestTaskType.CollectItem:
                                        newMessage = messageHandler.AddMessage(questTaskUpdateMessagePrefab);
                                        if (updatingProgress >= updatingMaxProgress)
                                            newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestTaskUpdateCollectItemComplete.ToString()), taskTitle, updatingProgress, updatingMaxProgress);
                                        else
                                            newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestTaskUpdateCollectItem.ToString()), taskTitle, updatingProgress, updatingMaxProgress);
                                        break;
                                    case QuestTaskType.TalkToNpc:
                                        newMessage = messageHandler.AddMessage(questTaskUpdateMessagePrefab);
                                        if (updatingProgress >= updatingMaxProgress)
                                            newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestTaskUpdateTalkToNpcComplete.ToString()), taskTitle, updatingProgress, updatingMaxProgress);
                                        else
                                            newMessage.text = string.Format(LanguageManager.GetText(formatKeyQuestTaskUpdateTalkToNpc.ToString()), taskTitle, updatingProgress, updatingMaxProgress);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    comparingQuests[index] = character.Quests[index].Clone();
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
