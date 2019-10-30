using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    /// <summary>
    /// This game database will load and setup game data from data that set in lists
    /// </summary>
    [CreateAssetMenu(fileName = "Game Database", menuName = "Create GameDatabase/Game Database", order = -5999)]
    public class GameDatabase : BaseGameDatabase
    {
        public Attribute[] attributes;
        public Item[] items;
        public GuildSkill[] guildSkills;
        public BasePlayerCharacterEntity[] playerCharacterEntities;
        public BaseMonsterCharacterEntity[] monsterCharacterEntities;
        public MountEntity[] mountEntities;
        public MapInfo[] mapInfos;
        public Faction[] factions;
        
        public override void LoadData(GameInstance gameInstance)
        {
            GameInstance.AddAttributes(attributes);
            GameInstance.AddItems(items);
            GameInstance.AddGuildSkills(guildSkills);
            GameInstance.AddCharacterEntities(playerCharacterEntities);
            GameInstance.AddCharacterEntities(monsterCharacterEntities);
            GameInstance.AddMountEntities(mountEntities);
            GameInstance.AddMapInfos(mapInfos);
            GameInstance.AddFactions(factions);
            // Tell game instance that data loaded
            gameInstance.LoadedGameData();
        }
    }
}
