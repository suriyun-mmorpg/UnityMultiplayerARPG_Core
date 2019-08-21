using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_PURCHASING && UNITY_PURCHASING && (UNITY_IOS || UNITY_ANDROID)
using UnityEngine.Purchasing;
#endif

namespace MultiplayerARPG
{
    public enum InventorySystem
    {
        Simple,
        LimitSlots,
    }
#if ENABLE_PURCHASING && UNITY_PURCHASING && (UNITY_IOS || UNITY_ANDROID)
    public partial class GameInstance : MonoBehaviour, IStoreListener
#else
    public partial class GameInstance : MonoBehaviour
#endif
    {
        // Events
        private System.Action onGameDataLoaded;
        public static GameInstance Singleton { get; protected set; }
        [Header("Game Instance Configs")]
        [SerializeField]
        private DimensionType dimensionType;
        [SerializeField]
        private BaseGameplayRule gameplayRule;
        [SerializeField]
        private NetworkSetting networkSetting;

        [Header("Gameplay Objects")]
        public ItemDropEntity itemDropEntityPrefab;
        public WarpPortalEntity warpPortalEntityPrefab;
        public UISceneGameplay uiSceneGameplayPrefab;
        public UISceneGameplay uiSceneGameplayMobilePrefab;
        [Tooltip("Default controller prefab will be used when controller prefab at player character entity is null")]
        public BasePlayerCharacterController defaultControllerPrefab;
        [Tooltip("This is camera controller when start game as server (not start with client as host)")]
        public ServerCharacter serverCharacterPrefab;
        [Tooltip("These objects will be instantiate as owning character's children")]
        public GameObject[] owningCharacterObjects;
        [Tooltip("These objects will be instantiate as owning character's children to show in minimap")]
        public GameObject[] owningCharacterMiniMapObjects;
        [Tooltip("These objects will be instantiate as non owning character's children to show in minimap")]
        public GameObject[] nonOwningCharacterMiniMapObjects;
        [Tooltip("These objects will be instantiate as monster character's children to show in minimap")]
        public GameObject[] monsterCharacterMiniMapObjects;
        [Tooltip("These objects will be instantiate as npc's children to show in minimap")]
        public GameObject[] npcMiniMapObjects;
        [Tooltip("This UI will be instaniate as owning character's child to show character name / HP / MP / Food / Water")]
        public UICharacterEntity owningCharacterUI;
        [Tooltip("This UI will be instaniate as non owning character's child to show character name / HP / MP / Food / Water")]
        public UICharacterEntity nonOwningCharacterUI;
        [Tooltip("This UI will be instaniate as monster character's child to show character name / HP / MP / Food / Water")]
        public UICharacterEntity monsterCharacterUI;
        [Tooltip("This UI will be instaniate as NPC's child to show character name")]
        public UINpcEntity npcUI;
        [Tooltip("This UI will be instaniate as NPC's child to show quest indecator")]
        public NpcQuestIndicator npcQuestIndicator;

        [Header("Gameplay Database / Settings")]
        [Tooltip("Default weapon item, will be used when character not equip any weapon")]
        [SerializeField]
        private Item defaultWeaponItem;
        [Tooltip("Default hit effect, will be used when attacks to enemies")]
        [SerializeField]
        private GameEffectCollection defaultHitEffects;
        [SerializeField]
        private int[] expTree;
        [Tooltip("You can add game data here or leave this empty to let it load data from Resources folders")]
        [SerializeField]
        private BaseGameDatabase gameDatabase;
        [Tooltip("You can add warp portals here or may add warp portals in the scene directly, So you can leave this empty")]
        [SerializeField]
        private WarpPortalDatabase warpPortalDatabase;
        [Tooltip("You can add NPCs here or may add NPCs in the scene directly, so you can leave this empty")]
        [SerializeField]
        private NpcDatabase npcDatabase;
        [Tooltip("You can add social system settings or leave this empty to use default settings")]
        [SerializeField]
        private SocialSystemSetting socialSystemSetting;

        [Header("Gameplay Configs")]
        public UnityTag playerTag;
        public UnityTag monsterTag;
        public UnityTag npcTag;
        public UnityTag itemDropTag;
        public UnityTag buildingTag;
        public UnityTag harvestableTag;
        public UnityLayer characterLayer;
        public UnityLayer itemDropLayer;
        public UnityLayer buildingLayer;
        public UnityLayer harvestableLayer;
        public UnityLayer[] nonTargetingLayers;
        [Tooltip("This is duration for Item Entities to appears befor destroyed")]
        public float itemAppearDuration = 60f;
        [Tooltip("This is duration for Item Entities to allow only player who kill monster to pick up item")]
        public float itemLootLockDuration = 5f;
        [Tooltip("This is duration for players to decides to do any action by another players")]
        public float coCharacterActionDuration = 5f;
        [Tooltip("This is a distance that allows a player to pick up an item")]
        public float pickUpItemDistance = 1f;
        [Tooltip("This is a distance that random drop item around a player")]
        public float dropDistance = 1f;
        [Tooltip("This is a distance that allows a player to converstion with NPC / send requests to other players")]
        public float conversationDistance = 1f;
        [Tooltip("This is a distance that allows a player to builds an building")]
        public float buildDistance = 10f;
        [Tooltip("This is a distance that other players will receives local chat")]
        public float localChatDistance = 10f;

        [Header("Gameplay Configs - Inventory and Storage")]
        public InventorySystem inventorySystem;
        [Tooltip("Base slot limit for all characters, it will be used when `InventorySystem` is `LimitSlots`")]
        public short baseSlotLimit;
        public Storage playerStorage;
        public Storage guildStorage;

        [Header("Gameplay Configs - Summon Monster")]
        [Tooltip("This is a distance that random summon around a character")]
        public float minSummonDistance = 2f;
        [Tooltip("This is a distance that random summon around a character")]
        public float maxSummonDistance = 3f;
        [Tooltip("Distance to warn character that ally being attacked")]
        public float enemySpottedNotifyDistance = 5f;
        [Tooltip("Min distance to follow summoner")]
        public float minFollowSummonerDistance = 5f;
        [Tooltip("Max distance to follow summoner, if distance between characters more than this it will teleport to summoner")]
        public float maxFollowSummonerDistance = 10f;

        [Header("Gameplay Configs - Summon Pet")]
        [Tooltip("This is duration to lock item before it is able to spawn later after character dead")]
        public float petDeadLockDuration = 60f;
        [Tooltip("This is duration to lock item before it is able to spawn later after unsummon")]
        public float petUnSummonLockDuration = 30f;

        [Header("Gameplay Configs - Instance Dungeon")]
        [Tooltip("Distance from party leader character to join instance map")]
        public float joinInstanceMapDistance = 20f;

        [Header("Game Effects")]
        public GameEffect levelUpEffect;

        [Header("New Character")]
        public int startGold = 0;
        public ItemAmount[] startItems;

        [Header("Scene/Maps")]
        public UnityScene homeScene;

        [Header("Player Configs")]
        public int minCharacterNameLength = 2;
        public int maxCharacterNameLength = 16;

        [Header("Playing In Editor")]
        public bool useMobileInEditor;

        public static readonly Dictionary<int, Attribute> Attributes = new Dictionary<int, Attribute>();
        public static readonly Dictionary<int, Item> Items = new Dictionary<int, Item>();
        public static readonly Dictionary<int, WeaponType> WeaponTypes = new Dictionary<int, WeaponType>();
        public static readonly Dictionary<int, BaseCharacter> Characters = new Dictionary<int, BaseCharacter>();
        public static readonly Dictionary<int, PlayerCharacter> PlayerCharacters = new Dictionary<int, PlayerCharacter>();
        public static readonly Dictionary<int, MonsterCharacter> MonsterCharacters = new Dictionary<int, MonsterCharacter>();
        public static readonly Dictionary<int, Skill> Skills = new Dictionary<int, Skill>();
        public static readonly Dictionary<int, NpcDialog> NpcDialogs = new Dictionary<int, NpcDialog>();
        public static readonly Dictionary<int, Quest> Quests = new Dictionary<int, Quest>();
        public static readonly Dictionary<int, GuildSkill> GuildSkills = new Dictionary<int, GuildSkill>();
        public static readonly Dictionary<int, BaseDamageEntity> DamageEntities = new Dictionary<int, BaseDamageEntity>();
        public static readonly Dictionary<int, BuildingEntity> BuildingEntities = new Dictionary<int, BuildingEntity>();
        public static readonly Dictionary<int, BaseCharacterEntity> CharacterEntities = new Dictionary<int, BaseCharacterEntity>();
        public static readonly Dictionary<int, BasePlayerCharacterEntity> PlayerCharacterEntities = new Dictionary<int, BasePlayerCharacterEntity>();
        public static readonly Dictionary<int, BaseMonsterCharacterEntity> MonsterCharacterEntities = new Dictionary<int, BaseMonsterCharacterEntity>();
        public static readonly Dictionary<int, MountEntity> MountEntities = new Dictionary<int, MountEntity>();
        public static readonly Dictionary<int, WarpPortalEntity> WarpPortalEntities = new Dictionary<int, WarpPortalEntity>();
        public static readonly Dictionary<int, NpcEntity> NpcEntities = new Dictionary<int, NpcEntity>();
        public static readonly Dictionary<string, List<WarpPortal>> MapWarpPortals = new Dictionary<string, List<WarpPortal>>();
        public static readonly Dictionary<string, List<Npc>> MapNpcs = new Dictionary<string, List<Npc>>();
        public static readonly Dictionary<string, MapInfo> MapInfos = new Dictionary<string, MapInfo>();
        public static readonly Dictionary<int, Faction> Factions = new Dictionary<int, Faction>();

        #region Cache Data
        public DimensionType DimensionType
        {
            get { return dimensionType; }
        }

        public bool IsLimitInventorySlot
        {
            get { return inventorySystem == InventorySystem.LimitSlots; }
        }

        public BaseGameplayRule GameplayRule
        {
            get
            {
                if (gameplayRule == null)
                    gameplayRule = ScriptableObject.CreateInstance<SimpleGameplayRule>();
                return gameplayRule;
            }
        }

        public BaseGameDatabase GameDatabase
        {
            get
            {
                if (gameDatabase == null)
                    gameDatabase = ScriptableObject.CreateInstance<ResourcesFolderGameDatabase>();
                return gameDatabase;
            }
        }

        public NetworkSetting NetworkSetting
        {
            get
            {
                if (networkSetting == null)
                    networkSetting = ScriptableObject.CreateInstance<NetworkSetting>();
                return networkSetting;
            }
        }

        public UISceneGameplay UISceneGameplayPrefab
        {
            get
            {
                if ((Application.isMobilePlatform || (useMobileInEditor && Application.isEditor)) && uiSceneGameplayMobilePrefab != null)
                    return uiSceneGameplayMobilePrefab;
                return uiSceneGameplayPrefab;
            }
        }

        private DamageElement cacheDefaultDamageElement;
        public DamageElement DefaultDamageElement
        {
            get
            {
                if (cacheDefaultDamageElement == null)
                {
                    cacheDefaultDamageElement = ScriptableObject.CreateInstance<DamageElement>();
                    cacheDefaultDamageElement.name = GameDataConst.DEFAULT_DAMAGE_ID;
                    cacheDefaultDamageElement.title = GameDataConst.DEFAULT_DAMAGE_TITLE;
                    cacheDefaultDamageElement.hitEffects = DefaultHitEffects;
                }
                return cacheDefaultDamageElement;
            }
        }

        public int[] ExpTree
        {
            get
            {
                if (expTree == null)
                    expTree = new int[] { 0 };
                return expTree;
            }
            set
            {
                if (value != null)
                    expTree = value;
            }
        }

        private ArmorType cacheDefaultArmorType;
        public ArmorType DefaultArmorType
        {
            get
            {
                if (cacheDefaultArmorType == null)
                {
                    cacheDefaultArmorType = ScriptableObject.CreateInstance<ArmorType>();
                    cacheDefaultArmorType.name = GameDataConst.UNKNOW_ARMOR_TYPE_ID;
                    cacheDefaultArmorType.title = GameDataConst.UNKNOW_ARMOR_TYPE_TITLE;
                }
                return cacheDefaultArmorType;
            }
        }

        private WeaponType cacheDefaultWeaponType;
        public WeaponType DefaultWeaponType
        {
            get
            {
                if (cacheDefaultWeaponType == null)
                {
                    cacheDefaultWeaponType = ScriptableObject.CreateInstance<WeaponType>();
                    cacheDefaultWeaponType.name = GameDataConst.UNKNOW_WEAPON_TYPE_ID;
                    cacheDefaultWeaponType.title = GameDataConst.UNKNOW_WEAPON_TYPE_TITLE;
                    cacheDefaultWeaponType.effectivenessAttributes = new DamageEffectivenessAttribute[0];
                    cacheDefaultWeaponType.damageInfo = new DamageInfo();
                }
                return cacheDefaultWeaponType;
            }
        }

        public Item DefaultWeaponItem
        {
            get
            {
                if (defaultWeaponItem == null)
                {
                    defaultWeaponItem = ScriptableObject.CreateInstance<Item>();
                    defaultWeaponItem.name = GameDataConst.DEFAULT_WEAPON_ID;
                    defaultWeaponItem.title = GameDataConst.DEFAULT_WEAPON_TITLE;
                    defaultWeaponItem.itemType = ItemType.Weapon;
                    defaultWeaponItem.weaponType = DefaultWeaponType;
                    // Default damage amount
                    IncrementalMinMaxFloat damageAmountMinMax = new IncrementalMinMaxFloat();
                    damageAmountMinMax.baseAmount = new MinMaxFloat() { min = 1, max = 1 };
                    damageAmountMinMax.amountIncreaseEachLevel = new MinMaxFloat() { min = 0, max = 0 };
                    DamageIncremental damageAmount = new DamageIncremental()
                    {
                        amount = damageAmountMinMax,
                    };
                    // Default harvest damage amount
                    IncrementalMinMaxFloat harvestDamageAmount = new IncrementalMinMaxFloat();
                    harvestDamageAmount.baseAmount = new MinMaxFloat() { min = 1, max = 1 };
                    harvestDamageAmount.amountIncreaseEachLevel = new MinMaxFloat() { min = 0, max = 0 };
                    // Set damage amount
                    defaultWeaponItem.damageAmount = damageAmount;
                    defaultWeaponItem.harvestDamageAmount = harvestDamageAmount;
                }
                return defaultWeaponItem;
            }
        }

        public GameEffectCollection DefaultHitEffects
        {
            get
            {
                return defaultHitEffects;
            }
        }

        public SocialSystemSetting SocialSystemSetting
        {
            get
            {
                if (socialSystemSetting == null)
                    socialSystemSetting = ScriptableObject.CreateInstance<SocialSystemSetting>();
                return socialSystemSetting;
            }
        }
        #endregion

        protected virtual void Awake()
        {
            Application.targetFrameRate = 60;
            Application.runInBackground = true;
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Singleton = this;

            InputManager.useMobileInputOnNonMobile = useMobileInEditor && Application.isEditor;

            // Load game data
            Attributes.Clear();
            Items.Clear();
            WeaponTypes.Clear();
            Characters.Clear();
            PlayerCharacters.Clear();
            MonsterCharacters.Clear();
            Skills.Clear();
            NpcDialogs.Clear();
            Quests.Clear();
            GuildSkills.Clear();
            DamageEntities.Clear();
            BuildingEntities.Clear();
            CharacterEntities.Clear();
            PlayerCharacterEntities.Clear();
            MonsterCharacterEntities.Clear();
            WarpPortalEntities.Clear();
            NpcEntities.Clear();
            MapWarpPortals.Clear();
            MapNpcs.Clear();
            MapInfos.Clear();

            this.InvokeInstanceDevExtMethods("Awake");
        }

        protected virtual void Start()
        {
            GameDatabase.LoadData(this);
        }

        public void LoadedGameData()
        {
            this.InvokeInstanceDevExtMethods("LoadedGameData");

            if (warpPortalDatabase != null)
                AddMapWarpPortals(warpPortalDatabase.maps);

            if (npcDatabase != null)
                AddMapNpcs(npcDatabase.maps);

            InitializePurchasing();

            if (onGameDataLoaded != null)
                onGameDataLoaded.Invoke();
            else
                OnGameDataLoaded();
        }

        public void SetOnGameDataLoadedCallback(System.Action callback)
        {
            onGameDataLoaded = callback;
        }

        public void OnGameDataLoaded()
        {
            StartCoroutine(LoadHomeSceneOnLoadedGameDataRoutine());
        }

        IEnumerator LoadHomeSceneOnLoadedGameDataRoutine()
        {
            yield return new WaitForEndOfFrame();
            UISceneLoading.Singleton.LoadScene(homeScene);
        }

        public List<string> GetGameMapIds()
        {
            List<string> mapIds = new List<string>();
            foreach (MapInfo mapInfo in MapInfos.Values)
            {
                if (mapInfo != null && !string.IsNullOrEmpty(mapInfo.Id) && !mapIds.Contains(mapInfo.Id))
                    mapIds.Add(mapInfo.Id);
            }

            return mapIds;
        }
        
        /// <summary>
        /// All layers except `nonTargetingLayers`, `TransparentFX`, `IgnoreRaycast`, `Water` will be used for raycasting
        /// </summary>
        /// <returns></returns>
        public int GetTargetLayerMask()
        {
            // 0 = Nothing, -1 = AllLayers
            int layerMask = 0;
            if (nonTargetingLayers.Length > 0)
            {
                foreach (UnityLayer nonTargetingLayer in nonTargetingLayers)
                {
                    layerMask = layerMask | nonTargetingLayer.Mask;
                }
            }
            layerMask = layerMask | 1 << 1;  // TransparentFX
            layerMask = layerMask | 1 << 2;  // IgnoreRaycast
            layerMask = layerMask | 1 << 3;  // Water
            return ~layerMask;
        }

        /// <summary>
        /// Only `characterLayer`, `buildingLayer`, `harvestableLayer` will be used for sphere casts
        /// </summary>
        /// <returns></returns>
        public int GetDamageableLayerMask()
        {
            int layerMask = 0;
            layerMask = layerMask | characterLayer.Mask;
            layerMask = layerMask | buildingLayer.Mask;
            layerMask = layerMask | harvestableLayer.Mask;
            return layerMask;
        }

        /// <summary>
        /// All layers except `characterLayer`, `itemDropLayer, `harvestableLayer`, `TransparentFX`, `IgnoreRaycast`, `Water` will be used for raycasting
        /// </summary>
        /// <returns></returns>
        public int GetBuildLayerMask()
        {
            int layerMask = 0;
            layerMask = layerMask | 1 << 1;  // TransparentFX
            layerMask = layerMask | 1 << 2;  // IgnoreRaycast
            layerMask = layerMask | 1 << 3;  // Water
            layerMask = layerMask | characterLayer.Mask;
            layerMask = layerMask | itemDropLayer.Mask;
            layerMask = layerMask | harvestableLayer.Mask;
            return ~layerMask;
        }

        /// <summary>
        /// All layers except `characterLayer`, `itemDropLayer, `TransparentFX`, `IgnoreRaycast`, `Water` will be used for raycasting
        /// </summary>
        /// <returns></returns>
        public int GetItemDropGroundDetectionLayerMask()
        {
            int layerMask = 0;
            layerMask = layerMask | 1 << 1;  // TransparentFX
            layerMask = layerMask | 1 << 2;  // IgnoreRaycast
            layerMask = layerMask | 1 << 3;  // Water
            layerMask = layerMask | characterLayer.Mask;
            layerMask = layerMask | itemDropLayer.Mask;
            return ~layerMask;
        }

        /// <summary>
        /// All layers except `buildingLayer`, `harvestableLayer, `TransparentFX`, `IgnoreRaycast`, `Water` will be used for raycasting
        /// </summary>
        /// <returns></returns>
        public int GetMonsterSpawnGroundDetectionLayerMask()
        {
            int layerMask = 0;
            layerMask = layerMask | 1 << 1;  // TransparentFX
            layerMask = layerMask | 1 << 2;  // IgnoreRaycast
            layerMask = layerMask | 1 << 3;  // Water
            layerMask = layerMask | buildingLayer.Mask;
            layerMask = layerMask | harvestableLayer.Mask;
            return ~layerMask;
        }

        /// <summary>
        /// All layers except `characterLayer`, `itemDropLayer`, `buildingLayer`, `harvestableLayer, `TransparentFX`, `IgnoreRaycast`, `Water` will be used for raycasting
        /// </summary>
        /// <returns></returns>
        public int GetHarvestableSpawnGroundDetectionLayerMask()
        {
            int layerMask = 0;
            layerMask = layerMask | 1 << 1;  // TransparentFX
            layerMask = layerMask | 1 << 2;  // IgnoreRaycast
            layerMask = layerMask | 1 << 3;  // Water
            layerMask = layerMask | characterLayer.Mask;
            layerMask = layerMask | itemDropLayer.Mask;
            layerMask = layerMask | buildingLayer.Mask;
            layerMask = layerMask | harvestableLayer.Mask;
            return ~layerMask;
        }

        public static void AddAttributes(IEnumerable<Attribute> attributes)
        {
            if (attributes == null)
                return;
            foreach (Attribute attribute in attributes)
            {
                if (attribute == null || Attributes.ContainsKey(attribute.DataId))
                    continue;
                Attributes[attribute.DataId] = attribute;
            }
        }

        public static void AddItems(IEnumerable<Item> items)
        {
            if (items == null)
                return;
            List<WeaponType> weaponTypes = new List<WeaponType>();
            List<BuildingEntity> buildingEntities = new List<BuildingEntity>();
            List<SkillLevel> skillLevels = new List<SkillLevel>();
            foreach (Item item in items)
            {
                if (item == null || Items.ContainsKey(item.DataId))
                    continue;
                Items[item.DataId] = item;
                // Add weapon types
                if (item.weaponType != null)
                    weaponTypes.Add(item.weaponType);
                // Add building entities
                if (item.buildingEntity != null)
                    buildingEntities.Add(item.buildingEntity);
                // Add skills
                skillLevels.AddRange(item.increaseSkillLevels);
                skillLevels.Add(item.skillLevel);
            }
            AddWeaponTypes(weaponTypes);
            AddBuildingEntities(buildingEntities);
            AddSkillLevels(skillLevels);
        }

        public static void AddCharacters(IEnumerable<BaseCharacter> characters)
        {
            if (characters == null)
                return;
            List<Skill> skills = new List<Skill>();
            List<DamageInfo> damageInfos = new List<DamageInfo>();
            foreach (BaseCharacter character in characters)
            {
                if (character == null || Characters.ContainsKey(character.DataId))
                    continue;

                Characters[character.DataId] = character;
                skills.AddRange(character.CacheSkillLevels.Keys);

                if (character is PlayerCharacter)
                {
                    PlayerCharacter playerCharacter = character as PlayerCharacter;
                    PlayerCharacters[character.DataId] = playerCharacter;
                }
                else if (character is MonsterCharacter)
                {
                    MonsterCharacter monsterCharacter = character as MonsterCharacter;
                    MonsterCharacters[character.DataId] = monsterCharacter;
                    if (monsterCharacter.damageInfo != null)
                        damageInfos.Add(monsterCharacter.damageInfo);
                }
            }
            AddSkills(skills);
            AddDamageInfos(damageInfos);
        }

        public static void AddCharacterEntities(IEnumerable<BaseCharacterEntity> characterEntities)
        {
            if (characterEntities == null)
                return;
            List<BaseCharacter> characters = new List<BaseCharacter>();
            foreach (BaseCharacterEntity characterEntity in characterEntities)
            {
                if (characterEntity == null || CharacterEntities.ContainsKey(characterEntity.Identity.HashAssetId))
                    continue;

                CharacterEntities[characterEntity.Identity.HashAssetId] = characterEntity;
                if (characterEntity is BasePlayerCharacterEntity)
                {
                    BasePlayerCharacterEntity playerCharacterEntity = characterEntity as BasePlayerCharacterEntity;
                    PlayerCharacterEntities[characterEntity.Identity.HashAssetId] = playerCharacterEntity;
                    characters.AddRange(playerCharacterEntity.playerCharacters);
                }
                else if (characterEntity is BaseMonsterCharacterEntity)
                {
                    BaseMonsterCharacterEntity monsterCharacterEntity = characterEntity as BaseMonsterCharacterEntity;
                    MonsterCharacterEntities[characterEntity.Identity.HashAssetId] = monsterCharacterEntity;
                    characters.Add(monsterCharacterEntity.monsterCharacter);
                }
            }
            AddCharacters(characters);
        }

        public static void AddMountEntities(IEnumerable<MountEntity> mountEntities)
        {
            if (mountEntities == null)
                return;
            foreach (MountEntity mountEntity in mountEntities)
            {
                if (mountEntity == null || MountEntities.ContainsKey(mountEntity.Identity.HashAssetId))
                    continue;
                MountEntities[mountEntity.Identity.HashAssetId] = mountEntity;
            }
        }

        public static void AddSkillLevels(IEnumerable<SkillLevel> skillLevels)
        {
            if (skillLevels == null)
                return;
            List<Skill> skills = new List<Skill>();
            foreach (SkillLevel skillLevel in skillLevels)
            {
                if (skillLevel.skill == null)
                    continue;
                skills.Add(skillLevel.skill);
            }
            AddSkills(skills);
        }

        public static void AddSkills(IEnumerable<Skill> skills)
        {
            if (skills == null)
                return;
            List<DamageInfo> damageInfos = new List<DamageInfo>();
            foreach (Skill skill in skills)
            {
                if (skill == null || Skills.ContainsKey(skill.DataId))
                    continue;
                Skills[skill.DataId] = skill;
                if (skill.damageInfo != null)
                    damageInfos.Add(skill.damageInfo);
            }
            AddDamageInfos(damageInfos);
        }

        public static void AddNpcDialogs(IEnumerable<NpcDialog> npcDialogs)
        {
            if (npcDialogs == null)
                return;
            foreach (NpcDialog npcDialog in npcDialogs)
            {
                if (npcDialog == null || NpcDialogs.ContainsKey(npcDialog.DataId))
                    continue;
                if (npcDialog.menus != null && npcDialog.menus.Length > 0)
                {
                    // Add dialogs from menus
                    List<NpcDialog> menuDialogs = new List<NpcDialog>();
                    foreach (NpcDialogMenu menu in npcDialog.menus)
                    {
                        if (menu.dialog != null && !NpcDialogs.ContainsKey(menu.dialog.DataId))
                            menuDialogs.Add(menu.dialog);
                    }
                    AddNpcDialogs(menuDialogs);
                }
                NpcDialogs[npcDialog.DataId] = npcDialog;
            }
        }

        public static void AddQuests(IEnumerable<Quest> quests)
        {
            if (quests == null)
                return;
            foreach (Quest quest in quests)
            {
                if (quest == null || Quests.ContainsKey(quest.DataId))
                    continue;
                Quests[quest.DataId] = quest;
            }
        }

        public static void AddGuildSkills(IEnumerable<GuildSkill> guildSkills)
        {
            if (guildSkills == null)
                return;
            foreach (GuildSkill guildSkill in guildSkills)
            {
                if (guildSkill == null || GuildSkills.ContainsKey(guildSkill.DataId))
                    continue;
                GuildSkills[guildSkill.DataId] = guildSkill;
            }
        }

        public static void AddDamageInfos(IEnumerable<DamageInfo> damageInfos)
        {
            if (damageInfos == null)
                return;
            List<BaseDamageEntity> damageEntities = new List<BaseDamageEntity>();
            foreach (DamageInfo damageInfo in damageInfos)
            {
                if (damageInfo == null)
                    continue;
                if (damageInfo.missileDamageEntity != null)
                    damageEntities.Add(damageInfo.missileDamageEntity);
            }
            AddDamageEntities(damageEntities);
        }

        public static void AddDamageEntities(IEnumerable<BaseDamageEntity> damageEntities)
        {
            if (damageEntities == null)
                return;
            foreach (BaseDamageEntity damageEntity in damageEntities)
            {
                if (damageEntity == null || DamageEntities.ContainsKey(damageEntity.DataId))
                    continue;
                DamageEntities[damageEntity.DataId] = damageEntity;
            }
        }

        public static void AddBuildingEntities(IEnumerable<BuildingEntity> buildingEntities)
        {
            if (buildingEntities == null)
                return;
            foreach (BuildingEntity buildingEntity in buildingEntities)
            {
                if (buildingEntity == null || BuildingEntities.ContainsKey(buildingEntity.DataId))
                    continue;
                BuildingEntities[buildingEntity.DataId] = buildingEntity;
            }
        }

        public static void AddWarpPortalEntities(IEnumerable<WarpPortalEntity> warpPortalEntities)
        {
            if (warpPortalEntities == null)
                return;
            foreach (WarpPortalEntity warpPortalEntity in warpPortalEntities)
            {
                if (warpPortalEntity == null || WarpPortalEntities.ContainsKey(warpPortalEntity.Identity.HashAssetId))
                    continue;
                WarpPortalEntities[warpPortalEntity.Identity.HashAssetId] = warpPortalEntity;
            }
        }

        public static void AddNpcEntities(IEnumerable<NpcEntity> npcEntities)
        {
            if (npcEntities == null)
                return;
            foreach (NpcEntity npcEntity in npcEntities)
            {
                if (npcEntity == null || NpcEntities.ContainsKey(npcEntity.Identity.HashAssetId))
                    continue;
                NpcEntities[npcEntity.Identity.HashAssetId] = npcEntity;
            }
        }

        public static void AddWeaponTypes(IEnumerable<WeaponType> weaponTypes)
        {
            if (weaponTypes == null)
                return;
            List<DamageInfo> damageInfos = new List<DamageInfo>();
            foreach (WeaponType weaponType in weaponTypes)
            {
                if (weaponType == null || WeaponTypes.ContainsKey(weaponType.DataId))
                    continue;
                WeaponTypes[weaponType.DataId] = weaponType;
                if (weaponType.damageInfo != null)
                    damageInfos.Add(weaponType.damageInfo);
            }
            AddDamageInfos(damageInfos);
        }

        public static void AddMapWarpPortals(IEnumerable<WarpPortals> mapWarpPortals)
        {
            if (mapWarpPortals == null)
                return;
            List<WarpPortalEntity> warpPortalEntities = new List<WarpPortalEntity>();
            foreach (WarpPortals mapWarpPortal in mapWarpPortals)
            {
                if (mapWarpPortal.mapInfo == null)
                    continue;
                if (MapWarpPortals.ContainsKey(mapWarpPortal.mapInfo.Id))
                    MapWarpPortals[mapWarpPortal.mapInfo.Id].AddRange(mapWarpPortal.warpPortals);
                else
                    MapWarpPortals[mapWarpPortal.mapInfo.Id] = new List<WarpPortal>(mapWarpPortal.warpPortals);
                foreach (WarpPortal warpPortal in mapWarpPortal.warpPortals)
                {
                    if (warpPortal.entityPrefab != null)
                        warpPortalEntities.Add(warpPortal.entityPrefab);
                }
            }
            AddWarpPortalEntities(warpPortalEntities);
        }

        public static void AddMapNpcs(IEnumerable<Npcs> mapNpcs)
        {
            if (mapNpcs == null)
                return;
            List<NpcEntity> npcEntities = new List<NpcEntity>();
            List<NpcDialog> npcDialogs = new List<NpcDialog>();
            foreach (Npcs mapNpc in mapNpcs)
            {
                if (mapNpc.mapInfo == null)
                    continue;
                if (MapNpcs.ContainsKey(mapNpc.mapInfo.Id))
                    MapNpcs[mapNpc.mapInfo.Id].AddRange(mapNpc.npcs);
                else
                    MapNpcs[mapNpc.mapInfo.Id] = new List<Npc>(mapNpc.npcs);
                foreach (Npc npc in mapNpc.npcs)
                {
                    if (npc.entityPrefab != null)
                        npcEntities.Add(npc.entityPrefab);
                    if (npc.startDialog != null)
                        npcDialogs.Add(npc.startDialog);
                }
            }
            AddNpcEntities(npcEntities);
            AddNpcDialogs(npcDialogs);
        }

        public static void AddMapInfos(IEnumerable<MapInfo> mapInfos)
        {
            if (mapInfos == null)
                return;
            foreach (MapInfo mapInfo in mapInfos)
            {
                if (mapInfo == null || !mapInfo.IsSceneSet())
                    continue;
                MapInfos[mapInfo.Id] = mapInfo;
            }
        }

        public static void AddFactions(IEnumerable<Faction> factions)
        {
            if (factions == null)
                return;
            foreach (Faction faction in factions)
            {
                if (faction == null)
                    continue;
                Factions[faction.DataId] = faction;
            }
        }
    }
}
