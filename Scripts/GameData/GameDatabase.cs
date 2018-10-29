using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Game Database", menuName = "Create GameDatabase/Game Database")]
    public class GameDatabase : ScriptableObject
    {
        public Attribute[] attributes;
        public DamageElement[] damageElements;
        public Item[] items;
        public Skill[] skills;
        public NpcDialog[] npcDialogs;
        public Quest[] quests;
        public GuildSkill[] guildSkills;
        public PlayerCharacter[] playerCharacters;
        public MonsterCharacter[] monsterCharacters;
        public BasePlayerCharacterEntity[] playerCharacterEntities;
        public BaseMonsterCharacterEntity[] monsterCharacterEntities;
        public MapInfo[] mapInfos;
        
        public virtual void LoadData(GameInstance gameInstance)
        {
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
            
            var weaponHitEffects = new List<GameEffectCollection>();
            if (gameInstance.DefaultDamageElement.hitEffects != null)
                weaponHitEffects.Add(gameInstance.DefaultDamageElement.hitEffects);
            foreach (var damageElement in damageElements)
            {
                if (damageElement.hitEffects != null)
                    weaponHitEffects.Add(damageElement.hitEffects);
            }
            GameInstance.AddGameEffectCollections(weaponHitEffects);

            // Loaded game data from game database
            gameInstance.LoadedGameData();
        }
    }
}
