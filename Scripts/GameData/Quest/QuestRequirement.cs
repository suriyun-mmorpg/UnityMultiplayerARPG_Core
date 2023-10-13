namespace MultiplayerARPG
{
    [System.Serializable]
    public struct QuestRequirement
    {
        public PlayerCharacter character;
        public Faction faction;
        public int level;
        public Quest[] completedQuests;
    }
}
