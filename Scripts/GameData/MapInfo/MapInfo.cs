using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Map Info", menuName = "Create GameData/Map Info", order = -4799)]
    public partial class MapInfo : BaseGameData
    {
        [Header("Map Info Configs")]
        public UnityScene scene;
        [Tooltip("This will be used when new character have been created, and this map data is start map")]
        public Vector3 startPosition;
        [Tooltip("When character fall to this position, character will dead")]
        public float deadY = -100f;
        [Tooltip("If this is `TRUE`, player can return to save point by `return` key. Else it will able to do that when dead only")]
        public bool canReturnToSavePoint;
        [Tooltip("If this is `Pvp`, player can battle all other players. `FactionPvp`, player can battle difference faction players. `GuildPvp`, player can battle difference guild players")]
        public PvpMode pvpMode;
        [Tooltip("If this is `Override`, player will return to map and position in `overrideRespawnPoints`")]
        public OverrideRespawnPointMode overrideRespawnPointMode;
        public OverrideRespawnPoint[] overrideRespawnPoints;

        [System.NonSerialized]
        private Dictionary<int, List<OverrideRespawnPoint>> cacheOverrideRespawnPoints;
        public Dictionary<int, List<OverrideRespawnPoint>> CacheOverrideRespawnPoints
        {
            get
            {
                if (cacheOverrideRespawnPoints == null)
                {
                    cacheOverrideRespawnPoints = new Dictionary<int, List<OverrideRespawnPoint>>();
                    int factionDataId = 0;
                    foreach (OverrideRespawnPoint overrideRespawnPoint in overrideRespawnPoints)
                    {
                        factionDataId = 0;
                        if (overrideRespawnPoint.forFaction != null)
                            factionDataId = overrideRespawnPoint.forFaction.DataId;
                        if (!cacheOverrideRespawnPoints.ContainsKey(factionDataId))
                            cacheOverrideRespawnPoints.Add(factionDataId, new List<OverrideRespawnPoint>());
                        cacheOverrideRespawnPoints[factionDataId].Add(overrideRespawnPoint);
                    }
                }
                return cacheOverrideRespawnPoints;
            }
        }

        [Header("Deprecated")]
        [Tooltip("This is deprecated, use `pvpMode`")]
        public bool canPvp;
        [Tooltip("This is deprecated, use `overrideRespawnPointMove`")]
        public bool overrideRespawnPoint;
        [Tooltip("This is deprecated, use `overrideRespawnPoints`")]
        public MapInfo overrideRespawnPointMap;
        [Tooltip("This is deprecated, use `overrideRespawnPoints`")]
        public Vector3 overrideRespawnPointPosition;

        public void GetRespawnPoint(IPlayerCharacterData playerCharacterData, out string mapName, out Vector3 position)
        {
            mapName = playerCharacterData.RespawnMapName;
            position = playerCharacterData.RespawnPosition;
            switch (overrideRespawnPointMode)
            {
                case OverrideRespawnPointMode.Override:
                    List<OverrideRespawnPoint> overrideRespawnPoints;
                    if (CacheOverrideRespawnPoints.TryGetValue(playerCharacterData.FactionId, out overrideRespawnPoints))
                    {
                        OverrideRespawnPoint overrideRespawnPoint = overrideRespawnPoints[Random.Range(0, overrideRespawnPoints.Count)];
                        mapName = overrideRespawnPoint.respawnMapInfo.Id;
                        position = overrideRespawnPoint.respawnPosition;
                    }
                    break;
            }
        }
    }

    public enum PvpMode
    {
        None,
        Pvp,
        FactionPvp,
        GuildPvp,
    }

    public enum OverrideRespawnPointMode
    {
        None,
        Override,
    }

    [SerializeField]
    public struct OverrideRespawnPoint
    {
        [Tooltip("If this is not empty, character who have the same faction will respawn to this point")]
        public Faction forFaction;
        public MapInfo respawnMapInfo;
        public Vector3 respawnPosition;
    }


}
