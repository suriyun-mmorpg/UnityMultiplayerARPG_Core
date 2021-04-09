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
        public PlayerCharacter[] playerCharacters;
        public MonsterCharacter[] monsterCharacters;
        public Harvestable[] harvestables;
        public BaseMapInfo[] mapInfos;
        public Quest[] quests;
        public Faction[] factions;

        public override void LoadData(GameInstance gameInstance)
        {
            GameInstance.AddCharacterEntities(playerCharacterEntities);
            GameInstance.AddCharacterEntities(monsterCharacterEntities);
            GameInstance.AddVehicleEntities(vehicleEntities);
            GameInstance.AddAttributes(attributes);
            GameInstance.AddCurrencies(currencies);
            GameInstance.AddDamageElements(damageElements);
            GameInstance.AddItems(items);
            GameInstance.AddItemCraftFormulas(itemCraftFormulas);
            GameInstance.AddArmorTypes(armorTypes);
            GameInstance.AddWeaponTypes(weaponTypes);
            GameInstance.AddAmmoTypes(ammoTypes);
            GameInstance.AddSkills(skills);
            GameInstance.AddGuildSkills(guildSkills);
            GameInstance.AddCharacters(playerCharacters);
            GameInstance.AddCharacters(monsterCharacters);
            GameInstance.AddHarvestables(harvestables);
            GameInstance.AddMapInfos(mapInfos);
            GameInstance.AddQuests(quests);
            GameInstance.AddFactions(factions);
            // Tell game instance that data loaded
            gameInstance.LoadedGameData();
        }

        public void LoadReferredData()
        {
            GameInstance.ClearData();
            GameInstance.AddCharacterEntities(playerCharacterEntities);
            GameInstance.AddCharacterEntities(monsterCharacterEntities);
            GameInstance.AddVehicleEntities(vehicleEntities);
            GameInstance.AddAttributes(attributes);
            GameInstance.AddCurrencies(currencies);
            GameInstance.AddDamageElements(damageElements);
            GameInstance.AddItems(items);
            GameInstance.AddItemCraftFormulas(itemCraftFormulas);
            GameInstance.AddArmorTypes(armorTypes);
            GameInstance.AddWeaponTypes(weaponTypes);
            GameInstance.AddAmmoTypes(ammoTypes);
            GameInstance.AddSkills(skills);
            GameInstance.AddGuildSkills(guildSkills);
            GameInstance.AddCharacters(playerCharacters);
            GameInstance.AddCharacters(monsterCharacters);
            GameInstance.AddHarvestables(harvestables);
            GameInstance.AddMapInfos(mapInfos);
            GameInstance.AddQuests(quests);
            GameInstance.AddFactions(factions);
        }
    }
}
