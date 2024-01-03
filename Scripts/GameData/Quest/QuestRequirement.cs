using System.Runtime.InteropServices;

namespace MultiplayerARPG
{
    [System.Serializable]
    [StructLayout(LayoutKind.Auto)]
    public partial struct QuestRequirement
    {
        public PlayerCharacter character;
        public Faction faction;
        public int level;
        public Quest[] completedQuests;
    }
}
