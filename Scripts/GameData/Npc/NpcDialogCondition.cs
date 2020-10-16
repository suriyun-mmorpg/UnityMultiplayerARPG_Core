namespace MultiplayerARPG
{
    [System.Serializable]
    public struct NpcDialogCondition
    {
        public NpcDialogConditionType conditionType;
        [StringShowConditional(nameof(conditionType), new string[] { nameof(NpcDialogConditionType.FactionIs) })]
        public Faction faction;
        [StringShowConditional(nameof(conditionType), new string[] { nameof(NpcDialogConditionType.QuestNotStarted), nameof(NpcDialogConditionType.QuestOngoing), nameof(NpcDialogConditionType.QuestTasksCompleted), nameof(NpcDialogConditionType.QuestCompleted) })]
        public Quest quest;
        [StringShowConditional(nameof(conditionType), new string[] { nameof(NpcDialogConditionType.LevelMoreThanOrEqual), nameof(NpcDialogConditionType.LevelLessThanOrEqual) })]
        public int conditionalLevel;
        [NpcDialogConditionData]
        public NpcDialogConditionData conditionData;

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
                    return indexOfQuest >= 0 && !questTasksCompleted;
                case NpcDialogConditionType.QuestTasksCompleted:
                    return indexOfQuest >= 0 && questTasksCompleted;
                case NpcDialogConditionType.QuestCompleted:
                    return indexOfQuest >= 0 && questCompleted;
                case NpcDialogConditionType.FactionIs:
                    return character.FactionId == faction.DataId;
                case NpcDialogConditionType.Custom:
                    return conditionData.Invoke(character);
            }
            return true;
        }
    }
}
