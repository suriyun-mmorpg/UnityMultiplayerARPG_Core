using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    /// <summary>
    /// This game database will load and setup game data from data that set in lists
    /// </summary>
    [CreateAssetMenu(fileName = "Game Database", menuName = "Create GameDatabase/Game Database", order = -5999)]
    public partial class GameDatabase : BaseGameDatabase
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
        public StatusEffect[] statusEffects;
        public PlayerCharacter[] playerCharacters;
        public MonsterCharacter[] monsterCharacters;
        public Harvestable[] harvestables;
        public BaseMapInfo[] mapInfos;
        public Quest[] quests;
        public Faction[] factions;

        protected override async UniTask LoadDataImplement(GameInstance gameInstance)
        {
            GameInstance.AddCharacterEntities(playerCharacterEntities);
            GameInstance.AddCharacterEntities(monsterCharacterEntities);
            GameInstance.AddVehicleEntities(vehicleEntities);
            GameInstance.AddAttributes(attributes);
            GameInstance.AddCurrencies(currencies);
            GameInstance.AddDamageElements(damageElements);
            GameInstance.AddItems(items);
            GameInstance.AddItemCraftFormulas(0, itemCraftFormulas);
            GameInstance.AddArmorTypes(armorTypes);
            GameInstance.AddWeaponTypes(weaponTypes);
            GameInstance.AddAmmoTypes(ammoTypes);
            GameInstance.AddSkills(skills);
            GameInstance.AddGuildSkills(guildSkills);
            GameInstance.AddStatusEffects(statusEffects);
            GameInstance.AddCharacters(playerCharacters);
            GameInstance.AddCharacters(monsterCharacters);
            GameInstance.AddHarvestables(harvestables);
            GameInstance.AddMapInfos(mapInfos);
            GameInstance.AddQuests(quests);
            GameInstance.AddFactions(factions);
            this.InvokeInstanceDevExtMethods("LoadDataImplement", gameInstance);
            await UniTask.Yield();
        }

        public void LoadReferredData()
        {
            GameInstance.ClearData();
            GameInstance.AddAttributes(attributes);
            GameInstance.AddCurrencies(currencies);
            GameInstance.AddDamageElements(damageElements);
            GameInstance.AddItems(items);
            GameInstance.AddItemCraftFormulas(0, itemCraftFormulas);
            GameInstance.AddArmorTypes(armorTypes);
            GameInstance.AddWeaponTypes(weaponTypes);
            GameInstance.AddAmmoTypes(ammoTypes);
            GameInstance.AddSkills(skills);
            GameInstance.AddGuildSkills(guildSkills);
            GameInstance.AddStatusEffects(statusEffects);
            GameInstance.AddCharacters(playerCharacters);
            GameInstance.AddCharacters(monsterCharacters);
            GameInstance.AddHarvestables(harvestables);
            GameInstance.AddMapInfos(mapInfos);
            GameInstance.AddQuests(quests);
            GameInstance.AddFactions(factions);

            if (playerCharacterEntities != null && playerCharacterEntities.Length > 0)
            {
                foreach (BasePlayerCharacterEntity entity in playerCharacterEntities)
                {
                    entity.PrepareRelatesData();
                }
            }

            if (monsterCharacterEntities != null && monsterCharacterEntities.Length > 0)
            {
                foreach (BaseMonsterCharacterEntity entity in monsterCharacterEntities)
                {
                    entity.PrepareRelatesData();
                }
            }

            if (vehicleEntities != null && vehicleEntities.Length > 0)
            {
                foreach (VehicleEntity entity in vehicleEntities)
                {
                    entity.PrepareRelatesData();
                }
            }

            List<Attribute> tempAttributes = new List<Attribute>(GameInstance.Attributes.Values);
            tempAttributes.Sort();
            attributes = tempAttributes.ToArray();

            List<Currency> tempCurrencies = new List<Currency>(GameInstance.Currencies.Values);
            tempCurrencies.Sort();
            currencies = tempCurrencies.ToArray();

            List<DamageElement> tempDamageElements = new List<DamageElement>(GameInstance.DamageElements.Values);
            tempDamageElements.Sort();
            damageElements = tempDamageElements.ToArray();

            List<ArmorType> tempArmorTypes = new List<ArmorType>(GameInstance.ArmorTypes.Values);
            tempArmorTypes.Sort();
            armorTypes = tempArmorTypes.ToArray();

            List<WeaponType> tempWeaponTypes = new List<WeaponType>(GameInstance.WeaponTypes.Values);
            tempWeaponTypes.Sort();
            weaponTypes = tempWeaponTypes.ToArray();

            List<AmmoType> tempAmmoTypes = new List<AmmoType>(GameInstance.AmmoTypes.Values);
            tempAmmoTypes.Sort();
            ammoTypes = tempAmmoTypes.ToArray();

            List<BaseItem> tempItems = new List<BaseItem>(GameInstance.Items.Values);
            tempItems.Sort();
            items = tempItems.ToArray();

            List<ItemCraftFormula> tempItemCraftFormulas = new List<ItemCraftFormula>(GameInstance.ItemCraftFormulas.Values);
            tempItemCraftFormulas.Sort();
            itemCraftFormulas = tempItemCraftFormulas.ToArray();

            List<BaseSkill> tempSkills = new List<BaseSkill>(GameInstance.Skills.Values);
            tempSkills.Sort();
            skills = tempSkills.ToArray();

            List<GuildSkill> tempGuildSkills = new List<GuildSkill>(GameInstance.GuildSkills.Values);
            tempGuildSkills.Sort();
            guildSkills = tempGuildSkills.ToArray();

            List<StatusEffect> tempStatusEffects = new List<StatusEffect>(GameInstance.StatusEffects.Values);
            tempStatusEffects.Sort();
            statusEffects = tempStatusEffects.ToArray();

            List<PlayerCharacter> tempPlayerCharacters = new List<PlayerCharacter>(GameInstance.PlayerCharacters.Values);
            tempPlayerCharacters.Sort();
            playerCharacters = tempPlayerCharacters.ToArray();

            List<MonsterCharacter> tempMonsterCharacters = new List<MonsterCharacter>(GameInstance.MonsterCharacters.Values);
            tempMonsterCharacters.Sort();
            monsterCharacters = tempMonsterCharacters.ToArray();

            List<Harvestable> tempHarvestables = new List<Harvestable>(GameInstance.Harvestables.Values);
            tempHarvestables.Sort();
            harvestables = tempHarvestables.ToArray();

            List<BaseMapInfo> tempMapInfos = new List<BaseMapInfo>(GameInstance.MapInfos.Values);
            mapInfos = tempMapInfos.ToArray();

            List<Quest> tempQuests = new List<Quest>(GameInstance.Quests.Values);
            tempQuests.Sort();
            quests = tempQuests.ToArray();

            List<Faction> tempFactions = new List<Faction>(GameInstance.Factions.Values);
            factions = tempFactions.ToArray();

            this.InvokeInstanceDevExtMethods("LoadReferredData");
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
}
