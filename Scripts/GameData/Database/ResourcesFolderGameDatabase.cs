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
            BaseGameData[] gameDataList = Resources.LoadAll<BaseGameData>("");
            BaseCharacterEntity[] characterEntities = Resources.LoadAll<BaseCharacterEntity>("");
            MountEntity[] mountEntities = Resources.LoadAll<MountEntity>("");

            List<Attribute> attributes = new List<Attribute>();
            List<DamageElement> damageElements = new List<DamageElement>();
            List<Item> items = new List<Item>();
            List<Skill> skills = new List<Skill>();
            List<NpcDialog> npcDialogs = new List<NpcDialog>();
            List<Quest> quests = new List<Quest>();
            List<GuildSkill> guildSkills = new List<GuildSkill>();
            List<PlayerCharacter> playerCharacters = new List<PlayerCharacter>();
            List<MonsterCharacter> monsterCharacters = new List<MonsterCharacter>();
            List<MapInfo> mapInfos = new List<MapInfo>();
            List<Faction> factions = new List<Faction>();

            // Filtering game data
            foreach (BaseGameData gameData in gameDataList)
            {
                if (gameData is Attribute)
                    attributes.Add(gameData as Attribute);
                if (gameData is DamageElement)
                    damageElements.Add(gameData as DamageElement);
                if (gameData is Item)
                    items.Add(gameData as Item);
                if (gameData is Skill)
                    skills.Add(gameData as Skill);
                if (gameData is NpcDialog)
                    npcDialogs.Add(gameData as NpcDialog);
                if (gameData is Quest)
                    quests.Add(gameData as Quest);
                if (gameData is GuildSkill)
                    guildSkills.Add(gameData as GuildSkill);
                if (gameData is PlayerCharacter)
                    playerCharacters.Add(gameData as PlayerCharacter);
                if (gameData is MonsterCharacter)
                    monsterCharacters.Add(gameData as MonsterCharacter);
                if (gameData is MapInfo)
                    mapInfos.Add(gameData as MapInfo);
                if (gameData is Faction)
                    factions.Add(gameData as Faction);
            }

            GameInstance.AddAttributes(attributes);
            GameInstance.AddItems(items);
            GameInstance.AddSkills(skills);
            GameInstance.AddNpcDialogs(npcDialogs);
            GameInstance.AddQuests(quests);
            GameInstance.AddGuildSkills(guildSkills);
            GameInstance.AddCharacters(playerCharacters);
            GameInstance.AddCharacters(monsterCharacters);
            GameInstance.AddCharacterEntities(characterEntities);
            GameInstance.AddMountEntities(mountEntities);
            GameInstance.AddMapInfos(mapInfos);
            GameInstance.AddFactions(factions);
            // Tell game instance that data loaded
            gameInstance.LoadedGameData();
        }
    }
}
