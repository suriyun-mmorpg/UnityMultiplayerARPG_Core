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

        public override void GetRespawnPoint(IPlayerCharacterData playerCharacterData, out string mapName, out Vector3 position)
        {
            base.GetRespawnPoint(playerCharacterData, out mapName, out position);
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

        protected override bool IsPlayerAlly(BasePlayerCharacterEntity playerCharacter, BaseCharacterEntity targetCharacter)
        {
            if (targetCharacter == null)
                return false;

            if (targetCharacter is BasePlayerCharacterEntity)
            {
                BasePlayerCharacterEntity targetPlayer = targetCharacter as BasePlayerCharacterEntity;
                switch (pvpMode)
                {
                    case PvpMode.Pvp:
                        return targetPlayer.PartyId != 0 && targetPlayer.PartyId == playerCharacter.PartyId;
                    case PvpMode.FactionPvp:
                        return targetPlayer.FactionId != 0 && targetPlayer.FactionId == playerCharacter.FactionId;
                    case PvpMode.GuildPvp:
                        return targetPlayer.GuildId != 0 && targetPlayer.GuildId == playerCharacter.GuildId;
                    default:
                        return false;
                }
            }

            if (targetCharacter is BaseMonsterCharacterEntity)
            {
                // If this character is summoner so it is ally
                BaseMonsterCharacterEntity targetMonster = targetCharacter as BaseMonsterCharacterEntity;
                if (targetMonster.IsSummoned)
                {
                    // If summoned by someone, will have same allies with summoner
                    return targetMonster.Summoner.IsAlly(playerCharacter);
                }
                else
                {
                    // Monster always not player's ally
                    return false;
                }
            }

            return false;
        }

        protected override bool IsMonsterAlly(BaseMonsterCharacterEntity monsterCharacter, BaseCharacterEntity targetCharacter)
        {
            if (targetCharacter == null)
                return false;

            if (monsterCharacter.IsSummoned)
            {
                // If summoned by someone, will have same allies with summoner
                return targetCharacter == monsterCharacter.Summoner || monsterCharacter.Summoner.IsAlly(targetCharacter);
            }

            if (targetCharacter is BaseMonsterCharacterEntity)
            {
                // If another monster has same allyId so it is ally
                BaseMonsterCharacterEntity targetMonster = targetCharacter as BaseMonsterCharacterEntity;
                if (targetMonster.IsSummoned)
                    return monsterCharacter.IsAlly(targetMonster.Summoner);
                return targetMonster.CharacterDatabase.allyId == monsterCharacter.CharacterDatabase.allyId;
            }

            return false;
        }

        protected override bool IsPlayerEnemy(BasePlayerCharacterEntity playerCharacter, BaseCharacterEntity targetCharacter)
        {
            if (targetCharacter == null)
                return false;

            if (targetCharacter is BasePlayerCharacterEntity)
            {
                BasePlayerCharacterEntity targetPlayer = targetCharacter as BasePlayerCharacterEntity;
                switch (pvpMode)
                {
                    case PvpMode.Pvp:
                        return targetPlayer.PartyId == 0 || targetPlayer.PartyId != playerCharacter.PartyId;
                    case PvpMode.FactionPvp:
                        return targetPlayer.FactionId == 0 || targetPlayer.FactionId != playerCharacter.FactionId;
                    case PvpMode.GuildPvp:
                        return targetPlayer.GuildId == 0 || targetPlayer.GuildId != playerCharacter.GuildId;
                    default:
                        return false;
                }
            }

            if (targetCharacter is BaseMonsterCharacterEntity)
            {
                // If this character is not summoner so it is enemy
                BaseMonsterCharacterEntity targetMonster = targetCharacter as BaseMonsterCharacterEntity;
                if (targetMonster.IsSummoned)
                {
                    // If summoned by someone, will have same enemies with summoner
                    return targetMonster.Summoner.IsEnemy(playerCharacter);
                }
                else
                {
                    // Monster always be player's enemy
                    return true;
                }
            }

            return false;
        }

        protected override bool IsMonsterEnemy(BaseMonsterCharacterEntity monsterCharacter, BaseCharacterEntity targetCharacter)
        {
            if (targetCharacter == null)
                return false;

            if (monsterCharacter.IsSummoned)
            {
                // If summoned by someone, will have same enemies with summoner
                return targetCharacter != monsterCharacter.Summoner && monsterCharacter.Summoner.IsEnemy(targetCharacter);
            }

            // Attack only player by default
            return targetCharacter is BasePlayerCharacterEntity;
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
        public BaseMapInfo respawnMapInfo;
        public Vector3 respawnPosition;
    }
}
