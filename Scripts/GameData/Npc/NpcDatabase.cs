using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "NpcDatabase", menuName = "Create GameData/NpcDatabase")]
    public class NpcDatabase : ScriptableObject
    {
        public Npcs[] maps;
    }
}
