using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Map Info", menuName = "Create GameData/Map Info", order = -4799)]
    public partial class MapInfo : BaseMapInfo
    {
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
                    int factionDataId;
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

        public override void GetRespawnPoint(IPlayerCharacterData playerCharacterData, out string mapName, out Vector3 position)
        {
            base.GetRespawnPoint(playerCharacterData, out mapName, out position);
            switch (overrideRespawnPointMode)
            {
                case OverrideRespawnPointMode.Override:
                    List<OverrideRespawnPoint> overrideRespawnPoints;
                    if (CacheOverrideRespawnPoints.TryGetValue(playerCharacterData.FactionId, out overrideRespawnPoints) ||
                        CacheOverrideRespawnPoints.TryGetValue(0, out overrideRespawnPoints))
                    {
                        OverrideRespawnPoint overrideRespawnPoint = overrideRespawnPoints[Random.Range(0, overrideRespawnPoints.Count)];
                        if (overrideRespawnPoint.respawnMapInfo != null)
                            mapName = overrideRespawnPoint.respawnMapInfo.Id;
                        else
                            mapName = BaseGameNetworkManager.CurrentMapInfo.Id;
                        position = overrideRespawnPoint.respawnPosition;
                    }
                    break;
            }
        }

        protected override bool IsPlayerAlly(BasePlayerCharacterEntity playerCharacter, EntityInfo targetEntity)
        {
            if (string.IsNullOrEmpty(targetEntity.Id))
                return false;

            if (targetEntity.Type == EntityTypes.Player)
            {
                switch (pvpMode)
                {
                    case PvpMode.Pvp:
                        return targetEntity.PartyId != 0 && targetEntity.PartyId == playerCharacter.PartyId;
                    case PvpMode.FactionPvp:
                        return targetEntity.FactionId != 0 && targetEntity.FactionId == playerCharacter.FactionId;
                    case PvpMode.GuildPvp:
                        return targetEntity.GuildId != 0 && targetEntity.GuildId == playerCharacter.GuildId;
                    default:
                        return true;
                }
            }

            if (targetEntity.Type == EntityTypes.Monster)
            {
                // If this character is summoner so it is ally
                if (targetEntity.Summoner.HasValue)
                {
                    // If summoned by someone, will have same allies with summoner
                    return playerCharacter.IsAlly(targetEntity.Summoner.Value);
                }
                else
                {
                    // Monster always not player's ally
                    return false;
                }
            }

            return false;
        }

        protected override bool IsMonsterAlly(BaseMonsterCharacterEntity monsterCharacter, EntityInfo targetEntity)
        {
            if (string.IsNullOrEmpty(targetEntity.Id))
                return false;

            if (monsterCharacter.IsSummoned)
            {
                // If summoned by someone, will have same allies with summoner
                return targetEntity.Id.Equals(monsterCharacter.Summoner.Id) || monsterCharacter.Summoner.IsAlly(targetEntity);
            }

            if (targetEntity.Type == EntityTypes.Monster)
            {
                // If another monster has same allyId so it is ally
                if (targetEntity.Summoner.HasValue)
                    return monsterCharacter.IsAlly(targetEntity.Summoner.Value);
                return GameInstance.MonsterCharacters[targetEntity.DataId].AllyId == monsterCharacter.CharacterDatabase.AllyId;
            }

            return false;
        }

        protected override bool IsPlayerEnemy(BasePlayerCharacterEntity playerCharacter, EntityInfo targetEntity)
        {
            if (string.IsNullOrEmpty(targetEntity.Id))
                return false;

            if (targetEntity.Type == EntityTypes.Player)
            {
                switch (pvpMode)
                {
                    case PvpMode.Pvp:
                        return targetEntity.PartyId == 0 || targetEntity.PartyId != playerCharacter.PartyId;
                    case PvpMode.FactionPvp:
                        return targetEntity.FactionId == 0 || targetEntity.FactionId != playerCharacter.FactionId;
                    case PvpMode.GuildPvp:
                        return targetEntity.GuildId == 0 || targetEntity.GuildId != playerCharacter.GuildId;
                    default:
                        return false;
                }
            }

            if (targetEntity.Type == EntityTypes.Monster)
            {
                // If this character is not summoner so it is enemy
                if (targetEntity.Summoner.HasValue)
                {
                    // If summoned by someone, will have same enemies with summoner
                    return playerCharacter.IsEnemy(targetEntity.Summoner.Value);
                }
                else
                {
                    // Monster always be player's enemy
                    return true;
                }
            }

            return false;
        }

        protected override bool IsMonsterEnemy(BaseMonsterCharacterEntity monsterCharacter, EntityInfo targetEntity)
        {
            if (string.IsNullOrEmpty(targetEntity.Id))
                return false;

            if (monsterCharacter.IsSummoned)
            {
                // If summoned by someone, will have same enemies with summoner
                return targetEntity.Id.Equals(monsterCharacter.Summoner.Id) && monsterCharacter.Summoner.IsEnemy(targetEntity);
            }

            // Attack only player by default
            return targetEntity.Type == EntityTypes.Player;
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

    [System.Serializable]
    public struct OverrideRespawnPoint
    {
        [Tooltip("If this is not empty, character who have the same faction will respawn to this point")]
        public Faction forFaction;
        [Tooltip("IF this is empty, it will respawn in current map")]
        public BaseMapInfo respawnMapInfo;
        public Vector3 respawnPosition;
    }
}
