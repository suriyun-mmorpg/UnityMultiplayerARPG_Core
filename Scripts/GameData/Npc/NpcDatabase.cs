using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Npc Database", menuName = "Create GameData/Npc Database")]
    public class NpcDatabase : ScriptableObject
    {
        public Npcs[] maps;
    }
}
