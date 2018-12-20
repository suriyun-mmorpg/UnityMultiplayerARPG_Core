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
        [Tooltip("If this is `TRUE`, player can battle other players")]
        public bool canPvp;
        [Tooltip("If this is `TRUE`, player can return to save point by `return` key. Else it will able to do that when dead only")]
        public bool canReturnToSavePoint;
        [Tooltip("If this is `TRUE`, player will return to `overrideRespawnPointPosition` in the `overrideRespawnPointScene`")]
        public bool overrideRespawnPoint;
        [Tooltip("Scene which player will returning to, when dead or press `return` key")]
        public UnityScene overrideRespawnPointScene;
        [Tooltip("Position in the `overrideRespawnPointScene` which player will returning to, when dead or press `return` key")]
        public Vector3 overrideRespawnPointPosition;
    }
}
