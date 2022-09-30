using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Cysharp.Threading.Tasks;
using LiteNetLibManager;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    /// <summary>
    /// This game database will load and setup game data from data that set in lists
    /// </summary>
    [CreateAssetMenu(fileName = GameDataMenuConsts.GAME_DATABASE_FILE, menuName = GameDataMenuConsts.GAME_DATABASE_MENU, order = GameDataMenuConsts.GAME_DATABASE_ORDER)]
    public partial class GameDatabase : BaseGameDatabase
    {
        [Header("Entity")]
        public UnityHelpBox entityHelpBox = new UnityHelpBox("Game database will load referring game data from an entities when game instance initializing");
        public BasePlayerCharacterEntity[] playerCharacterEntities;
        public BaseMonsterCharacterEntity[] monsterCharacterEntities;
        [FormerlySerializedAs("mountEntities")]
        public VehicleEntity[] vehicleEntities;
        public LiteNetLibIdentity[] otherNetworkObjects;

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
        public PlayerIcon[] playerIcons;
        public PlayerFrame[] playerFrames;
        public PlayerTitle[] playerTitles;
        public GuildSkill[] guildSkills;
        public GuildIcon[] guildIcons;
        public StatusEffect[] statusEffects;
        public PlayerCharacter[] playerCharacters;
        public MonsterCharacter[] monsterCharacters;
        public Harvestable[] harvestables;
        public BaseMapInfo[] mapInfos;
        public Quest[] quests;
        public Faction[] factions;
        public Gacha[] gachas;

        [Header("Set Asset Bundle Name Tools")]
        public string attributeBundleName = "attribute";
        public string currencyBundleName = "currency";
        public string damageElementBundleName = "damageElement";
        public string characterBundleName = "character";
        public string itemBundleName = "item";
        public string mapInfoBundleName = "map";
        public string sceneBundleName = "scene";
        public string skillBundleName = "skill";
        public string playerIconBundleName = "playerIcon";
        public string playerFrameBundleName = "playerFrame";
        public string playerTitleBundleName = "playerTitle";
        public string guildSkillBundleName = "guildSkill";
        public string guildIconBundleName = "guildIcon";
        public string questBundleName = "quest";
        public string factionBundleName = "faction";
        public string gachaBundleName = "gacha";
        public bool setNameForUnnamedAssetsOnly = true;

        protected override async UniTask LoadDataImplement(GameInstance gameInstance)
        {
            GameInstance.AddCharacterEntities(playerCharacterEntities);
            GameInstance.AddCharacterEntities(monsterCharacterEntities);
            GameInstance.AddVehicleEntities(vehicleEntities);
            GameInstance.AddOtherNetworkObjects(otherNetworkObjects);
            GameInstance.AddAttributes(attributes);
            GameInstance.AddCurrencies(currencies);
            GameInstance.AddDamageElements(damageElements);
            GameInstance.AddItems(items);
            GameInstance.AddItemCraftFormulas(0, itemCraftFormulas);
            GameInstance.AddArmorTypes(armorTypes);
            GameInstance.AddWeaponTypes(weaponTypes);
            GameInstance.AddAmmoTypes(ammoTypes);
            GameInstance.AddSkills(skills);
            GameInstance.AddPlayerIcons(playerIcons);
            GameInstance.AddPlayerFrames(playerFrames);
            GameInstance.AddPlayerTitles(playerTitles);
            GameInstance.AddGuildSkills(guildSkills);
            GameInstance.AddGuildIcons(guildIcons);
            GameInstance.AddStatusEffects(statusEffects);
            GameInstance.AddCharacters(playerCharacters);
            GameInstance.AddCharacters(monsterCharacters);
            GameInstance.AddHarvestables(harvestables);
            GameInstance.AddMapInfos(mapInfos);
            GameInstance.AddQuests(quests);
            GameInstance.AddFactions(factions);
            GameInstance.AddGachas(gachas);
            this.InvokeInstanceDevExtMethods("LoadDataImplement", gameInstance);
            await UniTask.Yield();
        }

#if UNITY_EDITOR
        [ContextMenu("Set Asset Bundle Name", false, 1000100)]
        public void SetAssetBundleName()
        {
            LoadReferredData();
            string assetPath;
            AssetImporter assetImporter;
            foreach (Attribute asset in GameInstance.Attributes.Values)
            {
                assetPath = AssetDatabase.GetAssetPath(asset.GetInstanceID());
                assetImporter = AssetImporter.GetAtPath(assetPath);
                if (string.IsNullOrEmpty(assetImporter.assetBundleName))
                    assetImporter.SetAssetBundleNameAndVariant(attributeBundleName, "");
            }
            foreach (Currency asset in GameInstance.Currencies.Values)
            {
                assetPath = AssetDatabase.GetAssetPath(asset.GetInstanceID());
                assetImporter = AssetImporter.GetAtPath(assetPath);
                if (string.IsNullOrEmpty(assetImporter.assetBundleName))
                    assetImporter.SetAssetBundleNameAndVariant(currencyBundleName, "");
            }
            foreach (DamageElement asset in GameInstance.DamageElements.Values)
            {
                assetPath = AssetDatabase.GetAssetPath(asset.GetInstanceID());
                assetImporter = AssetImporter.GetAtPath(assetPath);
                if (string.IsNullOrEmpty(assetImporter.assetBundleName))
                    assetImporter.SetAssetBundleNameAndVariant(damageElementBundleName, "");
            }
            foreach (BasePlayerCharacterEntity asset in GameInstance.PlayerCharacterEntities.Values)
            {
                assetPath = AssetDatabase.GetAssetPath(asset.GetInstanceID());
                assetImporter = AssetImporter.GetAtPath(assetPath);
                if (string.IsNullOrEmpty(assetImporter.assetBundleName))
                    assetImporter.SetAssetBundleNameAndVariant(characterBundleName, "");
            }
            foreach (BaseMonsterCharacterEntity asset in GameInstance.MonsterCharacterEntities.Values)
            {
                assetPath = AssetDatabase.GetAssetPath(asset.GetInstanceID());
                assetImporter = AssetImporter.GetAtPath(assetPath);
                if (string.IsNullOrEmpty(assetImporter.assetBundleName))
                    assetImporter.SetAssetBundleNameAndVariant(characterBundleName, "");
            }
            foreach (VehicleEntity asset in GameInstance.VehicleEntities.Values)
            {
                assetPath = AssetDatabase.GetAssetPath(asset.GetInstanceID());
                assetImporter = AssetImporter.GetAtPath(assetPath);
                if (string.IsNullOrEmpty(assetImporter.assetBundleName))
                    assetImporter.SetAssetBundleNameAndVariant(characterBundleName, "");
            }
            foreach (BaseCharacter asset in GameInstance.Characters.Values)
            {
                assetPath = AssetDatabase.GetAssetPath(asset.GetInstanceID());
                assetImporter = AssetImporter.GetAtPath(assetPath);
                if (string.IsNullOrEmpty(assetImporter.assetBundleName))
                    assetImporter.SetAssetBundleNameAndVariant(characterBundleName, "");
            }
            foreach (BaseItem asset in GameInstance.Items.Values)
            {
                assetPath = AssetDatabase.GetAssetPath(asset.GetInstanceID());
                assetImporter = AssetImporter.GetAtPath(assetPath);
                if (string.IsNullOrEmpty(assetImporter.assetBundleName))
                    assetImporter.SetAssetBundleNameAndVariant(itemBundleName, "");
            }
            foreach (BaseMapInfo asset in GameInstance.MapInfos.Values)
            {
                assetPath = AssetDatabase.GetAssetPath(asset.GetInstanceID());
                assetImporter = AssetImporter.GetAtPath(assetPath);
                if (string.IsNullOrEmpty(assetImporter.assetBundleName))
                    assetImporter.SetAssetBundleNameAndVariant(mapInfoBundleName, "");
                if (asset.Scene.SceneAsset == null)
                {
                    Debug.LogWarning("Map info: " + asset.Id + " has no scene set.");
                    continue;
                }
                assetPath = AssetDatabase.GetAssetPath(asset.Scene.SceneAsset.GetInstanceID());
                assetImporter = AssetImporter.GetAtPath(assetPath);
                if (string.IsNullOrEmpty(assetImporter.assetBundleName))
                    assetImporter.SetAssetBundleNameAndVariant(sceneBundleName, "");
            }
            foreach (BaseSkill asset in GameInstance.Skills.Values)
            {
                assetPath = AssetDatabase.GetAssetPath(asset.GetInstanceID());
                assetImporter = AssetImporter.GetAtPath(assetPath);
                if (string.IsNullOrEmpty(assetImporter.assetBundleName))
                    assetImporter.SetAssetBundleNameAndVariant(skillBundleName, "");
            }
            foreach (PlayerIcon asset in GameInstance.PlayerIcons.Values)
            {
                assetPath = AssetDatabase.GetAssetPath(asset.GetInstanceID());
                assetImporter = AssetImporter.GetAtPath(assetPath);
                if (string.IsNullOrEmpty(assetImporter.assetBundleName))
                    assetImporter.SetAssetBundleNameAndVariant(playerIconBundleName, "");
            }
            foreach (PlayerFrame asset in GameInstance.PlayerFrames.Values)
            {
                assetPath = AssetDatabase.GetAssetPath(asset.GetInstanceID());
                assetImporter = AssetImporter.GetAtPath(assetPath);
                if (string.IsNullOrEmpty(assetImporter.assetBundleName))
                    assetImporter.SetAssetBundleNameAndVariant(playerFrameBundleName, "");
            }
            foreach (PlayerTitle asset in GameInstance.PlayerTitles.Values)
            {
                assetPath = AssetDatabase.GetAssetPath(asset.GetInstanceID());
                assetImporter = AssetImporter.GetAtPath(assetPath);
                if (string.IsNullOrEmpty(assetImporter.assetBundleName))
                    assetImporter.SetAssetBundleNameAndVariant(playerTitleBundleName, "");
            }
            foreach (GuildSkill asset in GameInstance.GuildSkills.Values)
            {
                assetPath = AssetDatabase.GetAssetPath(asset.GetInstanceID());
                assetImporter = AssetImporter.GetAtPath(assetPath);
                if (string.IsNullOrEmpty(assetImporter.assetBundleName))
                    assetImporter.SetAssetBundleNameAndVariant(guildSkillBundleName, "");
            }
            foreach (GuildIcon asset in GameInstance.GuildIcons.Values)
            {
                assetPath = AssetDatabase.GetAssetPath(asset.GetInstanceID());
                assetImporter = AssetImporter.GetAtPath(assetPath);
                if (string.IsNullOrEmpty(assetImporter.assetBundleName))
                    assetImporter.SetAssetBundleNameAndVariant(guildIconBundleName, "");
            }
            foreach (Quest asset in GameInstance.Quests.Values)
            {
                assetPath = AssetDatabase.GetAssetPath(asset.GetInstanceID());
                assetImporter = AssetImporter.GetAtPath(assetPath);
                if (string.IsNullOrEmpty(assetImporter.assetBundleName))
                    assetImporter.SetAssetBundleNameAndVariant(questBundleName, "");
            }
            foreach (Faction asset in GameInstance.Factions.Values)
            {
                assetPath = AssetDatabase.GetAssetPath(asset.GetInstanceID());
                assetImporter = AssetImporter.GetAtPath(assetPath);
                if (string.IsNullOrEmpty(assetImporter.assetBundleName))
                    assetImporter.SetAssetBundleNameAndVariant(factionBundleName, "");
            }
            foreach (Gacha asset in GameInstance.Gachas.Values)
            {
                assetPath = AssetDatabase.GetAssetPath(asset.GetInstanceID());
                assetImporter = AssetImporter.GetAtPath(assetPath);
                if (string.IsNullOrEmpty(assetImporter.assetBundleName))
                    assetImporter.SetAssetBundleNameAndVariant(gachaBundleName, "");
            }
        }
#endif

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
            GameInstance.AddPlayerIcons(playerIcons);
            GameInstance.AddPlayerFrames(playerFrames);
            GameInstance.AddPlayerTitles(playerTitles);
            GameInstance.AddGuildSkills(guildSkills);
            GameInstance.AddGuildIcons(guildIcons);
            GameInstance.AddStatusEffects(statusEffects);
            GameInstance.AddCharacters(playerCharacters);
            GameInstance.AddCharacters(monsterCharacters);
            GameInstance.AddHarvestables(harvestables);
            GameInstance.AddMapInfos(mapInfos);
            GameInstance.AddQuests(quests);
            GameInstance.AddFactions(factions);
            GameInstance.AddGachas(gachas);
            GameInstance.AddCharacterEntities(playerCharacterEntities);
            GameInstance.AddCharacterEntities(monsterCharacterEntities);
            GameInstance.AddVehicleEntities(vehicleEntities);

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

            List<PlayerIcon> tempPlayerIcons = new List<PlayerIcon>(GameInstance.PlayerIcons.Values);
            tempPlayerIcons.Sort();
            playerIcons = tempPlayerIcons.ToArray();

            List<PlayerFrame> tempPlayerFrames = new List<PlayerFrame>(GameInstance.PlayerFrames.Values);
            tempPlayerFrames.Sort();
            playerFrames = tempPlayerFrames.ToArray();

            List<PlayerTitle> tempPlayerTitles = new List<PlayerTitle>(GameInstance.PlayerTitles.Values);
            tempPlayerTitles.Sort();
            playerTitles = tempPlayerTitles.ToArray();

            List<GuildSkill> tempGuildSkills = new List<GuildSkill>(GameInstance.GuildSkills.Values);
            tempGuildSkills.Sort();
            guildSkills = tempGuildSkills.ToArray();

            List<GuildIcon> tempGuildIcons = new List<GuildIcon>(GameInstance.GuildIcons.Values);
            tempGuildIcons.Sort();
            guildIcons = tempGuildIcons.ToArray();

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

            List<Gacha> tempGachas = new List<Gacha>(GameInstance.Gachas.Values);
            gachas = tempGachas.ToArray();

            this.InvokeInstanceDevExtMethods("LoadReferredData");
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
}
