namespace MultiplayerARPG
{
    [System.Serializable]
    public struct NpcDialogCondition
    {
        public NpcDialogConditionType conditionType;
        [StringShowConditional(conditionFieldName: "conditionType", conditionValues: new string[] { "FactionIs" })]
        public Faction faction;
        [StringShowConditional(conditionFieldName: "conditionType", conditionValues: new string[] { "QuestNotStarted", "QuestOngoing", "QuestTasksCompleted", "QuestCompleted" })]
        public Quest quest;
        [StringShowConditional(conditionFieldName: "conditionType", conditionValues: new string[] { "LevelMoreThanOrEqual", "LevelLessThanOrEqual" })]
        public int conditionalLevel;
        public bool IsPass(IPlayerCharacterData character)
        {
            int indexOfQuest = -1;
            bool questTasksCompleted = false;
            bool questCompleted = false;
            if (quest != null)
            {
                indexOfQuest = character.IndexOfQuest(quest.DataId);
                if (indexOfQuest >= 0)
                {
                    CharacterQuest characterQuest = character.Quests[indexOfQuest];
                    questTasksCompleted = characterQuest.IsAllTasksDone(character);
                    questCompleted = characterQuest.isComplete;
                }
            }
            switch (conditionType)
            {
                case NpcDialogConditionType.LevelMoreThanOrEqual:
                    return character.Level >= conditionalLevel;
                case NpcDialogConditionType.LevelLessThanOrEqual:
                    return character.Level <= conditionalLevel;
                case NpcDialogConditionType.QuestNotStarted:
                    return indexOfQuest < 0;
                case NpcDialogConditionType.QuestOngoing:
                    return !questTasksCompleted;
                case NpcDialogConditionType.QuestTasksCompleted:
                    return questTasksCompleted;
                case NpcDialogConditionType.QuestCompleted:
                    return questCompleted;
                case NpcDialogConditionType.FactionIs:
                    return character.FactionId == faction.DataId;
            }
            return true;
        }
    }
}
