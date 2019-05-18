using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIQuestTask : UISelectionEntry<QuestTaskProgressTuple>
    {
        public QuestTask QuestTask { get { return Data.questTask; } }
        public int Progress { get { return Data.progress; } }

        [Header("UI Elements")]
        public TextWrapper uiTextTaskDescription;

        protected override void UpdateData()
        {
            string tempFormat;
            switch (QuestTask.taskType)
            {
                case QuestTaskType.KillMonster:
                    MonsterCharacterAmount monsterCharacterAmount = QuestTask.monsterCharacterAmount;
                    string monsterTitle = monsterCharacterAmount.monster == null ? LanguageManager.GetUnknowTitle() : monsterCharacterAmount.monster.Title;
                    short monsterKillAmount = monsterCharacterAmount.amount;
                    tempFormat = Progress >= monsterCharacterAmount.amount ?
                        LanguageManager.GetText(UILocaleKeys.UI_QUEST_TASK_FORMAT_KILL_MONSTER_COMPLETE.ToString()) :
                        LanguageManager.GetText(UILocaleKeys.UI_QUEST_TASK_FORMAT_KILL_MONSTER.ToString());
                    if (uiTextTaskDescription != null)
                        uiTextTaskDescription.text = string.Format(tempFormat, monsterTitle, Progress.ToString("N0"), monsterKillAmount.ToString("N0"));
                    break;
                case QuestTaskType.CollectItem:
                    ItemAmount itemAmount = QuestTask.itemAmount;
                    string itemTitle = itemAmount.item == null ? LanguageManager.GetUnknowTitle() : itemAmount.item.Title;
                    short itemCollectAmount = itemAmount.amount;
                    tempFormat = Progress >= itemAmount.amount ?
                        LanguageManager.GetText(UILocaleKeys.UI_QUEST_TASK_FORMAT_COLLECT_ITEM_COMPLETE.ToString()) :
                        LanguageManager.GetText(UILocaleKeys.UI_QUEST_TASK_FORMAT_COLLECT_ITEM.ToString());
                    if (uiTextTaskDescription != null)
                        uiTextTaskDescription.text = string.Format(tempFormat, itemTitle, Progress.ToString("N0"), itemCollectAmount.ToString("N0"));
                    break;
            }
        }
    }
}
