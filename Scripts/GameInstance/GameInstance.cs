using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
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

    public enum CurrentPositionSaveMode
    {
        UseCurrentPosition,
        UseRespawnPosition
    }

    public enum TestInEditorMode
    {
        Standalone,
        Mobile
    } // TODO: Add console mode

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
        private BaseGameSaveSystem saveSystem;
        [SerializeField]
        private BaseGameplayRule gameplayRule;
        [SerializeField]
        private BaseGMCommands gmCommands;
        [SerializeField]
        private NetworkSetting networkSetting;

        [Header("Gameplay Objects")]
        public ItemDropEntity itemDropEntityPrefab;
        public WarpPortalEntity warpPortalEntityPrefab;
        public BaseUISceneGameplay uiSceneGameplayPrefab;
        [Tooltip("If this is empty, it will use `UI Scene Gameplay Prefab` as gameplay UI prefab")]
        public BaseUISceneGameplay uiSceneGameplayMobilePrefab;
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
        private BaseItem defaultWeaponItem;
        [Tooltip("Default damage element, will be used when attacks to enemies or receives damages from enemies")]
        [SerializeField]
        private DamageElement defaultDamageElement;
        [Tooltip("Default hit effects, will be used when attack to enemies or receive damages from enemies")]
        [SerializeField]
        private GameEffect[] defaultDamageHitEffects;
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
        [Tooltip("How character position load when start game")]
        public CurrentPositionSaveMode currentPositionSaveMode;
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
        [Tooltip("If dropped items does not picked up within this duration, it will be destroyed from the server")]
        public float itemAppearDuration = 60f;
        [Tooltip("If dropped items does not picked up by killer within this duration, anyone can pick up the items")]
        public float itemLootLockDuration = 5f;
        [Tooltip("If dealing request does not accepted within this duration, the request will be cancelled")]
        public float dealingRequestDuration = 5f;
        [Tooltip("This is a distance that allows a player to pick up an item")]
        public float pickUpItemDistance = 1f;
        [Tooltip("This is a distance that random drop item around a player")]
        public float dropDistance = 1f;
        [Tooltip("This is a distance that allows players to start converstion with NPC, send requests to other player entities and activate an building entities")]
        public float conversationDistance = 1f;
        [Tooltip("This is a distance that other players will receives local chat")]
        public float localChatDistance = 10f;
        [Tooltip("This is a distance from controlling character that combat texts will instantiates")]
        public float combatTextDistance = 20f;
        [Tooltip("Maximum number of equip weapon set")]
        [Range(1, 16)]
        public byte maxEquipWeaponSet = 2;

        [Header("Gameplay Configs - Items, Inventory and Storage")]
        public ItemTypeFilter dismantleFilter = new ItemTypeFilter()
        {
            includeArmor = true,
            includeShield = true,
            includeWeapon = true
        };
        [Tooltip("If this is `TRUE`, player will be able to refine an items by themself, doesn't have to talk to NPCs")]
        public bool canRefineItemByPlayer;
        [Tooltip("If this is `TRUE`, player will be able to dismantle an items by themself, doesn't have to talk to NPCs")]
        public bool canDismantleItemByPlayer;
        [Tooltip("If this is `TRUE`, player will be able to repair an items by themself, doesn't have to talk to NPCs")]
        public bool canRepairItemByPlayer;
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
        [Tooltip("If this is NULL, it will use `startGold` and `startItems`")]
        public NewCharacterSetting newCharacterSetting;
        [Tooltip("Amount of gold that will be added to character when create new character")]
        public int startGold = 0;
        [Tooltip("Items that will be added to character when create new character")]
        [ArrayElementTitle("item")]
        public ItemAmount[] startItems;

        [Header("Scene/Maps")]
        public UnityScene homeScene;
        [Tooltip("If this is empty, it will use `Home Mobile Scene` as home scene")]
        public UnityScene homeMobileScene;

        [Header("Player Configs")]
        public int minCharacterNameLength = 2;
        public int maxCharacterNameLength = 16;
        [Tooltip("Max characters that player can create, set it to 0 to unlimit")]
        public byte maxCharacterSaves = 5;

        [Header("Playing In Editor")]
        public TestInEditorMode testInEditorMode;

        public static readonly Dictionary<int, Attribute> Attributes = new Dictionary<int, Attribute>();
        public static readonly Dictionary<int, BaseItem> Items = new Dictionary<int, BaseItem>();
        public static readonly Dictionary<int, Harvestable> Harvestables = new Dictionary<int, Harvestable>();
        public static readonly Dictionary<int, ArmorType> ArmorTypes = new Dictionary<int, ArmorType>();
        public static readonly Dictionary<int, WeaponType> WeaponTypes = new Dictionary<int, WeaponType>();
        public static readonly Dictionary<int, BaseCharacter> Characters = new Dictionary<int, BaseCharacter>();
        public static readonly Dictionary<int, PlayerCharacter> PlayerCharacters = new Dictionary<int, PlayerCharacter>();
        public static readonly Dictionary<int, MonsterCharacter> MonsterCharacters = new Dictionary<int, MonsterCharacter>();
        public static readonly Dictionary<int, BaseSkill> Skills = new Dictionary<int, BaseSkill>();
        public static readonly Dictionary<int, NpcDialog> NpcDialogs = new Dictionary<int, NpcDialog>();
        public static readonly Dictionary<int, Quest> Quests = new Dictionary<int, Quest>();
        public static readonly Dictionary<int, GuildSkill> GuildSkills = new Dictionary<int, GuildSkill>();
        public static readonly Dictionary<int, DamageElement> DamageElements = new Dictionary<int, DamageElement>();
        public static readonly Dictionary<int, EquipmentSet> EquipmentSets = new Dictionary<int, EquipmentSet>();
        public static readonly Dictionary<int, BuildingEntity> BuildingEntities = new Dictionary<int, BuildingEntity>();
        public static readonly Dictionary<int, BaseCharacterEntity> CharacterEntities = new Dictionary<int, BaseCharacterEntity>();
        public static readonly Dictionary<int, BasePlayerCharacterEntity> PlayerCharacterEntities = new Dictionary<int, BasePlayerCharacterEntity>();
        public static readonly Dictionary<int, BaseMonsterCharacterEntity> MonsterCharacterEntities = new Dictionary<int, BaseMonsterCharacterEntity>();
        public static readonly Dictionary<int, ItemDropEntity> ItemDropEntities = new Dictionary<int, ItemDropEntity>();
        public static readonly Dictionary<int, HarvestableEntity> HarvestableEntities = new Dictionary<int, HarvestableEntity>();
        public static readonly Dictionary<int, VehicleEntity> VehicleEntities = new Dictionary<int, VehicleEntity>();
        public static readonly Dictionary<int, WarpPortalEntity> WarpPortalEntities = new Dictionary<int, WarpPortalEntity>();
        public static readonly Dictionary<int, NpcEntity> NpcEntities = new Dictionary<int, NpcEntity>();
        public static readonly Dictionary<string, List<WarpPortal>> MapWarpPortals = new Dictionary<string, List<WarpPortal>>();
        public static readonly Dictionary<string, List<Npc>> MapNpcs = new Dictionary<string, List<Npc>>();
        public static readonly Dictionary<string, BaseMapInfo> MapInfos = new Dictionary<string, BaseMapInfo>();
        public static readonly Dictionary<int, Faction> Factions = new Dictionary<int, Faction>();
        public static readonly HashSet<IPoolDescriptor> PoolingObjectPrefabs = new HashSet<IPoolDescriptor>();

        #region Cache Data
        public DimensionType DimensionType
        {
            get { return dimensionType; }
        }

        public bool IsLimitInventorySlot
        {
            get { return inventorySystem == InventorySystem.LimitSlots; }
        }

        public BaseGameSaveSystem SaveSystem
        {
            get { return saveSystem; }
        }

        public BaseGameplayRule GameplayRule
        {
            get { return gameplayRule; }
        }

        public BaseGMCommands GMCommands
        {
            get { return gmCommands; }
        }

        public BaseGameDatabase GameDatabase
        {
            get { return gameDatabase; }
        }

        public NetworkSetting NetworkSetting
        {
            get { return networkSetting; }
        }

        public SocialSystemSetting SocialSystemSetting
        {
            get { return socialSystemSetting; }
        }

        public BaseUISceneGameplay UISceneGameplayPrefab
        {
            get
            {
                if ((Application.isMobilePlatform || (testInEditorMode == TestInEditorMode.Mobile && Application.isEditor)) && uiSceneGameplayMobilePrefab != null)
                    return uiSceneGameplayMobilePrefab;
                return uiSceneGameplayPrefab;
            }
        }

        public string HomeSceneName
        {
            get
            {
                if ((Application.isMobilePlatform || (testInEditorMode == TestInEditorMode.Mobile && Application.isEditor)) && homeMobileScene.IsSet())
                    return homeMobileScene;
                return homeScene;
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

        public ArmorType DefaultArmorType { get; private set; }
        public WeaponType DefaultWeaponType { get; private set; }

        public IWeaponItem DefaultWeaponItem
        {
            get { return defaultWeaponItem as IWeaponItem; }
        }

        public DamageElement DefaultDamageElement
        {
            get { return defaultDamageElement; }
        }

        public GameEffect[] DefaultDamageHitEffects
        {
            get { return defaultDamageHitEffects; }
        }

        public bool HasNewCharacterSetting
        {
            get { return newCharacterSetting != null; }
        }

        public HashSet<int> NonTargetLayersValues { get; private set; }
        #endregion

        protected virtual void Awake()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                // Set target framerate when running headless to reduce CPU usage
                Application.targetFrameRate = 30;
            }
            else
            {
                // Not running headless, set target framerate higher
                Application.targetFrameRate = 60;
            }
            Application.runInBackground = true;
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Singleton = this;

            InputManager.useMobileInputOnNonMobile = testInEditorMode == TestInEditorMode.Mobile && Application.isEditor;

            DefaultArmorType = ScriptableObject.CreateInstance<ArmorType>()
                .GenerateDefaultArmorType();

            DefaultWeaponType = ScriptableObject.CreateInstance<WeaponType>()
                .GenerateDefaultWeaponType();

            // Setup default weapon item if not existed
            if (defaultWeaponItem == null || !defaultWeaponItem.IsWeapon())
            {
                defaultWeaponItem = ScriptableObject.CreateInstance<Item>()
                    .GenerateDefaultItem(DefaultWeaponType);
            }

            // Setup default damage element if not existed
            if (defaultDamageElement == null)
            {
                defaultDamageElement = ScriptableObject.CreateInstance<DamageElement>();
                defaultDamageElement.name = GameDataConst.DEFAULT_DAMAGE_ID;
                defaultDamageElement.title = GameDataConst.DEFAULT_DAMAGE_TITLE;
                defaultDamageElement.damageHitEffects = DefaultDamageHitEffects;
            }

            // Setup save system if not existed
            if (saveSystem == null)
                saveSystem = ScriptableObject.CreateInstance<DefaultGameSaveSystem>();

            // Setup gameplay rule if not existed
            if (gameplayRule == null)
                gameplayRule = ScriptableObject.CreateInstance<DefaultGameplayRule>();

            // Setup GM commands if not existed
            if (gmCommands == null)
                gmCommands = ScriptableObject.CreateInstance<DefaultGMCommands>();

            // Setup game database if not existed
            if (gameDatabase == null)
                gameDatabase = ScriptableObject.CreateInstance<ResourcesFolderGameDatabase>();

            // Setup network setting if not existed
            if (networkSetting == null)
                networkSetting = ScriptableObject.CreateInstance<NetworkSetting>();

            // Setup social system setting if not existed
            if (socialSystemSetting == null)
                socialSystemSetting = ScriptableObject.CreateInstance<SocialSystemSetting>();

            // Setup non target layers
            NonTargetLayersValues = new HashSet<int>();
            foreach (UnityLayer layer in nonTargetingLayers)
            {
                NonTargetLayersValues.Add(layer.LayerIndex);
            }

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
            BuildingEntities.Clear();
            CharacterEntities.Clear();
            PlayerCharacterEntities.Clear();
            MonsterCharacterEntities.Clear();
            WarpPortalEntities.Clear();
            NpcEntities.Clear();
            MapWarpPortals.Clear();
            MapNpcs.Clear();
            MapInfos.Clear();
            PoolingObjectPrefabs.Clear();

            this.InvokeInstanceDevExtMethods("Awake");
        }

        protected virtual void Start()
        {
            GameDatabase.LoadData(this);
        }

        public void LoadedGameData()
        {
            this.InvokeInstanceDevExtMethods("LoadedGameData");

            // Add required default game data
            AddItems(new BaseItem[] { DefaultWeaponItem as BaseItem });
            AddWeaponTypes(new WeaponType[] { DefaultWeaponType });
            AddPoolingObjects(new IPoolDescriptor[] { levelUpEffect });
            AddPoolingObjects(DefaultDamageHitEffects);

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
            yield return UISceneLoading.Singleton.LoadScene(HomeSceneName);
        }

        public List<string> GetGameMapIds()
        {
            List<string> mapIds = new List<string>();
            foreach (BaseMapInfo mapInfo in MapInfos.Values)
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
            layerMask = layerMask | 1 << PhysicLayers.TransparentFX;
            layerMask = layerMask | 1 << PhysicLayers.IgnoreRaycast;
            layerMask = layerMask | 1 << PhysicLayers.Water;
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
            layerMask = layerMask | 1 << PhysicLayers.TransparentFX;
            layerMask = layerMask | 1 << PhysicLayers.IgnoreRaycast;
            layerMask = layerMask | 1 << PhysicLayers.Water;
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
            layerMask = layerMask | 1 << PhysicLayers.TransparentFX;
            layerMask = layerMask | 1 << PhysicLayers.IgnoreRaycast;
            layerMask = layerMask | 1 << PhysicLayers.Water;
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
            layerMask = layerMask | 1 << PhysicLayers.TransparentFX;
            layerMask = layerMask | 1 << PhysicLayers.IgnoreRaycast;
            layerMask = layerMask | 1 << PhysicLayers.Water;
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
            layerMask = layerMask | 1 << PhysicLayers.TransparentFX;
            layerMask = layerMask | 1 << PhysicLayers.IgnoreRaycast;
            layerMask = layerMask | 1 << PhysicLayers.Water;
            layerMask = layerMask | characterLayer.Mask;
            layerMask = layerMask | itemDropLayer.Mask;
            layerMask = layerMask | buildingLayer.Mask;
            layerMask = layerMask | harvestableLayer.Mask;
            return ~layerMask;
        }

        #region Add game data functions
        public static void AddAttributes(IEnumerable<Attribute> attributes)
        {
            if (attributes == null)
                return;
            foreach (Attribute attribute in attributes)
            {
                AddGameData(Attributes, attribute);
            }
        }

        public static void AddItems(IEnumerable<ItemAmount> itemAmounts)
        {
            if (itemAmounts == null)
                return;
            List<BaseItem> items = new List<BaseItem>();
            foreach (ItemAmount itemAmount in itemAmounts)
            {
                if (itemAmount.item == null)
                    continue;
                items.Add(itemAmount.item);
            }
            AddItems(items);
        }

        public static void AddItems(IEnumerable<ItemDrop> itemDrops)
        {
            if (itemDrops == null)
                return;
            List<BaseItem> items = new List<BaseItem>();
            foreach (ItemDrop itemDrop in itemDrops)
            {
                if (itemDrop.item == null)
                    continue;
                items.Add(itemDrop.item);
            }
            AddItems(items);
        }

        public static void AddItems(IEnumerable<ItemDropByWeight> itemDrops)
        {
            if (itemDrops == null)
                return;
            List<BaseItem> items = new List<BaseItem>();
            foreach (ItemDropByWeight itemDrop in itemDrops)
            {
                if (itemDrop.item == null)
                    continue;
                items.Add(itemDrop.item);
            }
            AddItems(items);
        }

        public static void AddItems(IEnumerable<BaseItem> items)
        {
            if (items == null)
                return;
            foreach (BaseItem item in items)
            {
                AddGameData(Items, item);
            }
        }

        public static void AddHarvestables(IEnumerable<Harvestable> harvestables)
        {
            if (harvestables == null)
                return;
            foreach (Harvestable harvestable in harvestables)
            {
                AddGameData(Harvestables, harvestable);
            }
        }

        public static void AddSkills(IEnumerable<SkillLevel> skillLevels)
        {
            if (skillLevels == null)
                return;
            List<BaseSkill> skills = new List<BaseSkill>();
            foreach (SkillLevel skillLevel in skillLevels)
            {
                if (skillLevel.skill == null)
                    continue;
                skills.Add(skillLevel.skill);
            }
            AddSkills(skills);
        }

        public static void AddSkills(IEnumerable<BaseSkill> skills)
        {
            if (skills == null)
                return;
            foreach (BaseSkill skill in skills)
            {
                AddGameData(Skills, skill);
            }
        }

        public static void AddNpcDialogs(IEnumerable<NpcDialog> npcDialogs)
        {
            if (npcDialogs == null)
                return;
            foreach (NpcDialog npcDialog in npcDialogs)
            {
                if (npcDialog == null || NpcDialogs.ContainsKey(npcDialog.DataId))
                    continue;
                npcDialog.Validate();
                NpcDialogs[npcDialog.DataId] = npcDialog;
                npcDialog.PrepareRelatesData();
            }
        }

        public static void AddQuests(IEnumerable<Quest> quests)
        {
            if (quests == null)
                return;
            foreach (Quest quest in quests)
            {
                AddGameData(Quests, quest);
            }
        }

        public static void AddGuildSkills(IEnumerable<GuildSkill> guildSkills)
        {
            if (guildSkills == null)
                return;
            foreach (GuildSkill guildSkill in guildSkills)
            {
                AddGameData(GuildSkills, guildSkill);
            }
        }

        public static void AddCharacters(IEnumerable<BaseCharacter> characters)
        {
            if (characters == null)
                return;
            foreach (BaseCharacter character in characters)
            {
                if (character == null)
                    continue;
                if (!Characters.ContainsKey(character.DataId))
                    Characters[character.DataId] = character;
                if (character is PlayerCharacter)
                    AddGameData(PlayerCharacters, character as PlayerCharacter);
                else if (character is MonsterCharacter)
                    AddGameData(MonsterCharacters, character as MonsterCharacter);
            }
        }

        public static void AddArmorTypes(IEnumerable<ArmorType> armorTypes)
        {
            if (armorTypes == null)
                return;
            foreach (ArmorType armorType in armorTypes)
            {
                AddGameData(ArmorTypes, armorType);
            }
        }

        public static void AddWeaponTypes(IEnumerable<WeaponType> weaponTypes)
        {
            if (weaponTypes == null)
                return;
            foreach (WeaponType weaponType in weaponTypes)
            {
                AddGameData(WeaponTypes, weaponType);
            }
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
                    if (npc.graph != null)
                        npcDialogs.AddRange(npc.graph.GetDialogs());
                }
            }
            AddNpcEntities(npcEntities);
            AddNpcDialogs(npcDialogs);
        }

        public static void AddMapInfos(IEnumerable<BaseMapInfo> mapInfos)
        {
            if (mapInfos == null)
                return;
            foreach (BaseMapInfo mapInfo in mapInfos)
            {
                if (mapInfo == null || MapInfos.ContainsKey(mapInfo.Id) || !mapInfo.IsSceneSet())
                    continue;
                mapInfo.Validate();
                MapInfos[mapInfo.Id] = mapInfo;
                mapInfo.PrepareRelatesData();
            }
        }

        public static void AddFactions(IEnumerable<Faction> factions)
        {
            if (factions == null)
                return;
            foreach (Faction faction in factions)
            {
                AddGameData(Factions, faction);
            }
        }

        public static void AddDamageElements(IEnumerable<DamageAmount> damageAmounts)
        {
            if (damageAmounts == null)
                return;
            List<DamageElement> elements = new List<DamageElement>();
            foreach (DamageAmount damageAmount in damageAmounts)
            {
                if (damageAmount.damageElement == null)
                    continue;
                elements.Add(damageAmount.damageElement);
            }
            AddDamageElements(elements);
        }

        public static void AddDamageElements(IEnumerable<DamageIncremental> damageIncrementals)
        {
            if (damageIncrementals == null)
                return;
            List<DamageElement> elements = new List<DamageElement>();
            foreach (DamageIncremental damageIncremental in damageIncrementals)
            {
                if (damageIncremental.damageElement == null)
                    continue;
                elements.Add(damageIncremental.damageElement);
            }
            AddDamageElements(elements);
        }

        public static void AddDamageElements(IEnumerable<DamageElement> damageElements)
        {
            if (damageElements == null)
                return;
            foreach (DamageElement damageElement in damageElements)
            {
                AddGameData(DamageElements, damageElement);
            }
        }
        public static void AddEquipmentSets(IEnumerable<EquipmentSet> equipmentSets)
        {
            if (equipmentSets == null)
                return;
            foreach (EquipmentSet equipmentSet in equipmentSets)
            {
                AddGameData(EquipmentSets, equipmentSet);
            }
        }
        #endregion

        #region Add game entity functions
        public static void AddCharacterEntities(IEnumerable<BaseCharacterEntity> characterEntities)
        {
            if (characterEntities == null)
                return;
            foreach (BaseCharacterEntity characterEntity in characterEntities)
            {
                if (characterEntity == null)
                    continue;
                if (!characterEntity.Identity.IsSceneObject && !CharacterEntities.ContainsKey(characterEntity.Identity.HashAssetId))
                    CharacterEntities[characterEntity.Identity.HashAssetId] = characterEntity;
                if (characterEntity is BasePlayerCharacterEntity)
                    AddGameEntity(PlayerCharacterEntities, characterEntity as BasePlayerCharacterEntity);
                else if (characterEntity is BaseMonsterCharacterEntity)
                    AddGameEntity(MonsterCharacterEntities, characterEntity as BaseMonsterCharacterEntity);
            }
        }

        public static void AddItemDropEntities(IEnumerable<ItemDropEntity> itemDropEntities)
        {
            if (itemDropEntities == null)
                return;
            foreach (ItemDropEntity itemDropEntity in itemDropEntities)
            {
                AddGameEntity(ItemDropEntities, itemDropEntity);
            }
        }

        public static void AddHarvestableEntities(IEnumerable<HarvestableEntity> harvestableEntities)
        {
            if (harvestableEntities == null)
                return;
            foreach (HarvestableEntity harvestableEntity in harvestableEntities)
            {
                AddGameEntity(HarvestableEntities, harvestableEntity);
            }
        }

        public static void AddVehicleEntities(IEnumerable<VehicleEntity> vehicleEntities)
        {
            if (vehicleEntities == null)
                return;
            foreach (VehicleEntity vehicleEntity in vehicleEntities)
            {
                AddGameEntity(VehicleEntities, vehicleEntity);
            }
        }

        public static void AddBuildingEntities(IEnumerable<BuildingEntity> buildingEntities)
        {
            if (buildingEntities == null)
                return;
            foreach (BuildingEntity buildingEntity in buildingEntities)
            {
                AddGameEntity(BuildingEntities, buildingEntity);
            }
        }

        public static void AddWarpPortalEntities(IEnumerable<WarpPortalEntity> warpPortalEntities)
        {
            if (warpPortalEntities == null)
                return;
            foreach (WarpPortalEntity warpPortalEntity in warpPortalEntities)
            {
                AddGameEntity(WarpPortalEntities, warpPortalEntity);
            }
        }

        public static void AddNpcEntities(IEnumerable<NpcEntity> npcEntities)
        {
            if (npcEntities == null)
                return;
            foreach (NpcEntity npcEntity in npcEntities)
            {
                AddGameEntity(NpcEntities, npcEntity);
            }
        }
        #endregion

        public static void AddPoolingObjects(IEnumerable<IPoolDescriptor> poolingObjects)
        {
            if (poolingObjects == null)
                return;
            foreach (IPoolDescriptor poolingObject in poolingObjects)
            {
                if (poolingObject == null || PoolingObjectPrefabs.Contains(poolingObject))
                    continue;
                PoolingObjectPrefabs.Add(poolingObject);
            }
        }

        private static void AddGameData<T>(Dictionary<int, T> dict, T data)
            where T : BaseGameData
        {
            if (data == null)
                return;
            if (!dict.ContainsKey(data.DataId))
            {
                data.Validate();
                dict[data.DataId] = data;
                data.PrepareRelatesData();
            }
        }

        private static void AddGameEntity<T>(Dictionary<int, T> dict, T entity)
            where T : BaseGameEntity
        {
            if (entity == null)
                return;
            if (!entity.Identity.IsSceneObject && !dict.ContainsKey(entity.Identity.HashAssetId))
            {
                dict[entity.Identity.HashAssetId] = entity;
                entity.PrepareRelatesData();
            }
            else if (entity.Identity.IsSceneObject)
            {
                entity.PrepareRelatesData();
            }
        }
    }
}
