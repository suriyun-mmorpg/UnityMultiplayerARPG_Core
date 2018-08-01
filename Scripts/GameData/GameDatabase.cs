using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "GameDatabase", menuName = "Create GameDatabase/GameDatabase")]
    public class GameDatabase : ScriptableObject
    {
        public Attribute[] attributes;
        public DamageElement[] damageElements;
        public Item[] items;
        public Skill[] skills;
        public NpcDialog[] npcDialogs;
        public Quest[] quests;
        public PlayerCharacter[] playerCharacters;
        public MonsterCharacter[] monsterCharacters;
        public MapInfo[] mapInfos;
        
        public virtual void LoadData(GameInstance gameInstance)
        {
            var attributes = new List<Attribute>();
            var damageElements = new List<DamageElement>();
            var items = new List<Item>();
            var skills = new List<Skill>();
            var npcDialogs = new List<NpcDialog>();
            var quests = new List<Quest>();
            var playerCharacters = new List<BaseCharacter>();
            var monsterCharacters = new List<BaseCharacter>();
            var mapInfos = new List<MapInfo>();

            items.Add(gameInstance.DefaultWeaponItem);
            damageElements.Add(gameInstance.DefaultDamageElement);

            attributes.AddRange(this.attributes);
            damageElements.AddRange(this.damageElements);
            items.AddRange(this.items);
            skills.AddRange(this.skills);
            npcDialogs.AddRange(this.npcDialogs);
            quests.AddRange(this.quests);
            playerCharacters.AddRange(this.playerCharacters);
            monsterCharacters.AddRange(this.monsterCharacters);
            mapInfos.AddRange(this.mapInfos);

            GameInstance.AddAttributes(attributes);
            GameInstance.AddItems(items);
            GameInstance.AddSkills(skills);
            GameInstance.AddNpcDialogs(npcDialogs);
            GameInstance.AddQuests(quests);
            GameInstance.AddCharacters(playerCharacters);
            GameInstance.AddCharacters(monsterCharacters);
            GameInstance.AddMapInfos(mapInfos);

            var weaponHitEffects = new List<GameEffectCollection>();
            foreach (var damageElement in damageElements)
            {
                if (damageElement.hitEffects != null)
                    weaponHitEffects.Add(damageElement.hitEffects);
            }
            GameInstance.AddGameEffectCollections(GameEffectCollectionType.WeaponHit, weaponHitEffects);

            // Loaded game data from game database
            gameInstance.LoadedGameData();
        }
    }
}
