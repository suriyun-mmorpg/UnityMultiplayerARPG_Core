using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Map Info", menuName = "Create GameData/Map Info")]
    public partial class MapInfo : BaseGameData
    {
        public UnityScene scene;
        [Tooltip("This will be used when new character have been created, and this map data is start map")]
        public Vector3 startPosition;
        public bool canPvp;
    }
}
