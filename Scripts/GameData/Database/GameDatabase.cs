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
        public DamageElement[] damageElements;
        public Item[] items;
        public NpcDialog[] npcDialogs;
        public Quest[] quests;
        public GuildSkill[] guildSkills;
        public BasePlayerCharacterEntity[] playerCharacterEntities;
        public BaseMonsterCharacterEntity[] monsterCharacterEntities;
        public MountEntity[] mountEntities;
        public MapInfo[] mapInfos;
        public Faction[] factions;
        
        public override void LoadData(GameInstance gameInstance)
        {
            GameInstance.AddAttributes(attributes);
            GameInstance.AddItems(new Item[] { gameInstance.DefaultWeaponItem });
            GameInstance.AddItems(items);
            GameInstance.AddWeaponTypes(new WeaponType[] { gameInstance.DefaultWeaponType });
            GameInstance.AddNpcDialogs(npcDialogs);
            GameInstance.AddQuests(quests);
            GameInstance.AddGuildSkills(guildSkills);
            GameInstance.AddCharacterEntities(playerCharacterEntities);
            GameInstance.AddCharacterEntities(monsterCharacterEntities);
            GameInstance.AddMountEntities(mountEntities);
            GameInstance.AddMapInfos(mapInfos);
            GameInstance.AddFactions(factions);
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
