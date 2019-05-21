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
            BaseCharacterEntity[] characterEntityList = Resources.LoadAll<BaseCharacterEntity>("");

            List<Attribute> attributes = new List<Attribute>();
            List<DamageElement> damageElements = new List<DamageElement>();
            List<Item> items = new List<Item>();
            List<Skill> skills = new List<Skill>();
            List<NpcDialog> npcDialogs = new List<NpcDialog>();
            List<Quest> quests = new List<Quest>();
            List<GuildSkill> guildSkills = new List<GuildSkill>();
            List<PlayerCharacter> playerCharacters = new List<PlayerCharacter>();
            List<MonsterCharacter> monsterCharacters = new List<MonsterCharacter>();
            List<BasePlayerCharacterEntity> playerCharacterEntities = new List<BasePlayerCharacterEntity>();
            List<BaseMonsterCharacterEntity> monsterCharacterEntities = new List<BaseMonsterCharacterEntity>();
            List<MapInfo> mapInfos = new List<MapInfo>();

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
            }

            // Filtering character entity
            foreach (BaseCharacterEntity characterEntity in characterEntityList)
            {
                if (characterEntity is BasePlayerCharacterEntity)
                    playerCharacterEntities.Add(characterEntity as BasePlayerCharacterEntity);
                if (characterEntity is BaseMonsterCharacterEntity)
                    monsterCharacterEntities.Add(characterEntity as BaseMonsterCharacterEntity);
            }

            GameInstance.AddAttributes(attributes);
            GameInstance.AddItems(new Item[] { gameInstance.DefaultWeaponItem });
            GameInstance.AddItems(items);
            GameInstance.AddWeaponTypes(new WeaponType[] { gameInstance.DefaultWeaponType });
            GameInstance.AddSkills(skills);
            GameInstance.AddNpcDialogs(npcDialogs);
            GameInstance.AddQuests(quests);
            GameInstance.AddGuildSkills(guildSkills);
            GameInstance.AddCharacters(playerCharacters);
            GameInstance.AddCharacters(monsterCharacters);
            GameInstance.AddCharacterEntities(playerCharacterEntities);
            GameInstance.AddCharacterEntities(monsterCharacterEntities);
            GameInstance.AddMapInfos(mapInfos);
            // Add hit effects
            List<GameEffectCollection> weaponHitEffects = new List<GameEffectCollection>();
            if (gameInstance.DefaultDamageElement.hitEffects != null)
                weaponHitEffects.Add(gameInstance.DefaultDamageElement.hitEffects);
            foreach (DamageElement damageElement in damageElements)
            {
                if (damageElement.hitEffects != null)
                    weaponHitEffects.Add(damageElement.hitEffects);
            }
            GameInstance.AddGameEffectCollections(weaponHitEffects);
            // Tell game instance that data loaded
            gameInstance.LoadedGameData();
        }
    }
}
