namespace MultiplayerARPG
{
    [System.Serializable]
    public struct QuestRequirement
    {
        public PlayerCharacter character;
        public short level;
        public Quest[] completedQuests;
    }
}
