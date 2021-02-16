using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    /// <summary>
    /// This game database will load and setup game data from data that set in lists
    /// </summary>
    [CreateAssetMenu(fileName = "Game Database", menuName = "Create GameDatabase/Game Database", order = -5999)]
    public class GameDatabase : BaseGameDatabase
    {
        [Header("Entity")]
        public UnityHelpBox entityHelpBox = new UnityHelpBox("Game database will load referring game data from an entities when game instance initializing");
        public BasePlayerCharacterEntity[] playerCharacterEntities;
        public BaseMonsterCharacterEntity[] monsterCharacterEntities;
        [FormerlySerializedAs("mountEntities")]
        public VehicleEntity[] vehicleEntities;

        [Header("Game Data")]
        public UnityHelpBox gameDataHelpBox = new UnityHelpBox("Only Attributes, Items, Guild Skills, Map Infos, Quests and Factions game data are required. Other game data can be loaded because they were referred by those game data.");
        public Attribute[] attributes;
        public Currency[] currencies;
        public BaseItem[] items;
        public ItemCraftFormula[] itemCraftFormulas;
        public GuildSkill[] guildSkills;
        public BaseMapInfo[] mapInfos;
        public Quest[] quests;
        public Faction[] factions;

        // TODO: WIP game database editor
        /*
        public Attribute[] attributes;
        public Currency[] currencies;
        public DamageElement[] damageElements;
        public BaseItem[] items;
        public ItemCraftFormula[] itemCraftFormulas;
        public ArmorType[] armorTypes;
        public WeaponType[] weaponTypes;
        public AmmoType[] ammoTypes;
        public BaseSkill[] skills;
        public GuildSkill[] guildSkills;
        public BaseCharacter[] characters;
        public Harvestable[] harvestables;
        public BaseMapInfo[] mapInfos;
        public BaseNpcDialog[] npcDialogs;
        public Quest[] quests;
        public Faction[] factions;
        */

        public override void LoadData(GameInstance gameInstance)
        {
            GameInstance.AddAttributes(attributes);
            GameInstance.AddCurrencies(currencies);
            GameInstance.AddItems(items);
            GameInstance.AddItemCraftFormulas(itemCraftFormulas);
            GameInstance.AddGuildSkills(guildSkills);
            GameInstance.AddCharacterEntities(playerCharacterEntities);
            GameInstance.AddCharacterEntities(monsterCharacterEntities);
            GameInstance.AddVehicleEntities(vehicleEntities);
            GameInstance.AddMapInfos(mapInfos);
            GameInstance.AddQuests(quests);
            GameInstance.AddFactions(factions);
            // Tell game instance that data loaded
            gameInstance.LoadedGameData();
        }

        public void LoadReferredData()
        {
            // TODO: Users have to use this function to load and set referred data to game database
            // then game database editor will be able to load and show them in game data finder.
        }
    }
}
