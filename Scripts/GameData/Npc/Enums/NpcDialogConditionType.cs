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
        Custom = 254
    }
}
