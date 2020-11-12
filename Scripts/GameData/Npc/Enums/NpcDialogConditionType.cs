namespace MultiplayerARPG
{
    public enum NpcDialogConditionType : byte
    {
        LevelMoreThanOrEqual,
        LevelLessThanOrEqual,
        QuestNotStarted,
        QuestOngoing,
        QuestTasksCompleted,
        QuestCompleted,
        FactionIs,
        PlayerCharacterIs,
        Custom = 254
    }
}
