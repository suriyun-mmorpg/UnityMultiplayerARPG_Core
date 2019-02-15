using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct Npcs
    {
        public MapInfo mapInfo;
        public Npc[] npcs;
        [Header("Deprecated")]
        [System.Obsolete("`Map` is deprecated, use `Map Info` instead")]
        [Tooltip("`Map` is deprecated, use `Map Info` instead")]
        public UnityScene map;
    }
}
