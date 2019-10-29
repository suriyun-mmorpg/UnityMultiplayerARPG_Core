using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    /// <summary>
    /// This game database will load and setup game data from Resources folder
    /// </summary>
    [CreateAssetMenu(fileName = "Resources Folder Game Database", menuName = "Create GameDatabase/Resources Folder Game Database", order = -5998)]
    public class ResourcesFolderGameDatabase : BaseGameDatabase
    {
        public override void LoadData(GameInstance gameInstance)
        {
            // Use Resources Load Async ?
            Attribute[] attributes = Resources.LoadAll<Attribute>("");
            Item[] items = Resources.LoadAll<Item>("");
            BaseSkill[] skills = Resources.LoadAll<BaseSkill>("");
            NpcDialog[] npcDialogs = Resources.LoadAll<NpcDialog>("");
            Quest[] quests = Resources.LoadAll<Quest>("");
            GuildSkill[] guildSkills = Resources.LoadAll<GuildSkill>("");
            PlayerCharacter[] playerCharacters = Resources.LoadAll<PlayerCharacter>("");
            MonsterCharacter[] monsterCharacters = Resources.LoadAll<MonsterCharacter>("");
            MapInfo[] mapInfos = Resources.LoadAll<MapInfo>("");
            Faction[] factions = Resources.LoadAll<Faction>("");
            BaseCharacterEntity[] characterEntities = Resources.LoadAll<BaseCharacterEntity>("");
            MountEntity[] mountEntities = Resources.LoadAll<MountEntity>("");

            GameInstance.AddAttributes(attributes);
            GameInstance.AddItems(items);
            GameInstance.AddSkills(skills);
            GameInstance.AddNpcDialogs(npcDialogs);
            GameInstance.AddQuests(quests);
            GameInstance.AddGuildSkills(guildSkills);
            GameInstance.AddCharacters(playerCharacters);
            GameInstance.AddCharacters(monsterCharacters);
            GameInstance.AddMapInfos(mapInfos);
            GameInstance.AddFactions(factions);
            GameInstance.AddCharacterEntities(characterEntities);
            GameInstance.AddMountEntities(mountEntities);
            // Tell game instance that data loaded
            gameInstance.LoadedGameData();
        }
    }
}
