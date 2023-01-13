namespace MultiplayerARPG
{
    [System.Serializable]
    public struct QuestRequirement
    {
        public PlayerCharacter character;
        public int level;
        public Quest[] completedQuests;
    }
}
