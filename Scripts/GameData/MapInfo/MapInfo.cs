using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "MapInfo", menuName = "Create GameData/MapInfo")]
    public class MapInfo : ScriptableObject
    {
        public UnityScene scene;
        [Tooltip("This will be used when new character have been created, and this map data is start map")]
        public Vector3 startPosition;
        public bool canPvp;
    }
}
