using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Npc Database", menuName = "Create GameDatabase/Npc Database", order = -5899)]
    public class NpcDatabase : ScriptableObject
    {
        public Npcs[] maps;
    }
}
