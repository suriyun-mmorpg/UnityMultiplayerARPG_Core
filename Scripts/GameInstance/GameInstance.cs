using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
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

    public enum PlayerDropItemMode
    {
        DropOnGround,
        DestroyItem,
    }

    public enum DeadDropItemMode
    {
        DropOnGround,
        CorpseLooting,
    }

    public enum TestInEditorMode
    {
        Standalone,
        Mobile,
        MobileWithKeyInputs,
        Console,
    }

    [DefaultExecutionOrder(-999)]
    [RequireComponent(typeof(EventSystemManager))]
#if ENABLE_PURCHASING && UNITY_PURCHASING && (UNITY_IOS || UNITY_ANDROID)
    public partial class GameInstance : MonoBehaviour, IStoreListener
#else
    public partial class GameInstance : MonoBehaviour
#endif
    {
        public static readonly string LogTag = nameof(GameInstance);
        public static GameInstance Singleton { get; protected set; }
        public static IClientCashShopHandlers ClientCashShopHandlers { get; set; }
        public static IClientMailHandlers ClientMailHandlers { get; set; }
        public static IClientCharacterHandlers ClientCharacterHandlers { get; set; }
        public static IClientInventoryHandlers ClientInventoryHandlers { get; set; }
        public static IClientStorageHandlers ClientStorageHandlers { get; set; }
        public static IClientPartyHandlers ClientPartyHandlers { get; set; }
        public static IClientGuildHandlers ClientGuildHandlers { get; set; }
        public static IClientGachaHandlers ClientGachaHandlers { get; set; }
        public static IClientFriendHandlers ClientFriendHandlers { get; set; }
        public static IClientBankHandlers ClientBankHandlers { get; set; }
        public static IClientOnlineCharacterHandlers ClientOnlineCharacterHandlers { get; set; }
        public static IClientChatHandlers ClientChatHandlers { get; set; }
        public static IServerMailHandlers ServerMailHandlers { get; set; }
        public static IServerUserHandlers ServerUserHandlers { get; set; }
        public static IServerBuildingHandlers ServerBuildingHandlers { get; set; }
        public static IServerGameMessageHandlers ServerGameMessageHandlers { get; set; }
        public static IServerCharacterHandlers ServerCharacterHandlers { get; set; }
        public static IServerStorageHandlers ServerStorageHandlers { get; set; }
        public static IServerPartyHandlers ServerPartyHandlers { get; set; }
        public static IServerGuildHandlers ServerGuildHandlers { get; set; }
        public static IServerChatHandlers ServerChatHandlers { get; set; }
        public static IItemUIVisibilityManager ItemUIVisibilityManager { get; set; }
        public static IItemsContainerUIVisibilityManager ItemsContainerUIVisibilityManager { get; set; }
        public static ICustomSummonManager CustomSummonManager { get; set; }
        public static string UserId { get; set; }
        public static string UserToken { get; set; }
        public static string SelectedCharacterId { get; set; }
        private static IPlayerCharacterData s_playingCharacter;
        public static IPlayerCharacterData PlayingCharacter
        {
            get { return s_playingCharacter; }
            set
            {
                if (value != s_playingCharacter)
                {
                    s_playingCharacter = value;
                    if (onSetPlayingCharacter != null)
                        onSetPlayingCharacter.Invoke(value);
                }
            }
        }
        public static BasePlayerCharacterEntity PlayingCharacterEntity { get { return PlayingCharacter as BasePlayerCharacterEntity; } }
        public static PartyData JoinedParty { get; set; }
        public static GuildData JoinedGuild { get; set; }
        public static StorageType OpenedStorageType { get; set; }
        public static string OpenedStorageOwnerId { get; set; }

        public static readonly Dictionary<int, Attribute> Attributes = new Dictionary<int, Attribute>();
        public static readonly Dictionary<int, Currency> Currencies = new Dictionary<int, Currency>();
        public static readonly Dictionary<int, BaseItem> Items = new Dictionary<int, BaseItem>();
        public static readonly Dictionary<int, Dictionary<int, BaseItem>> ItemsByAmmoType = new Dictionary<int, Dictionary<int, BaseItem>>();
        public static readonly Dictionary<int, ItemCraftFormula> ItemCraftFormulas = new Dictionary<int, ItemCraftFormula>();
        public static readonly Dictionary<int, Harvestable> Harvestables = new Dictionary<int, Harvestable>();
        public static readonly Dictionary<int, BaseCharacter> Characters = new Dictionary<int, BaseCharacter>();
        public static readonly Dictionary<int, PlayerCharacter> PlayerCharacters = new Dictionary<int, PlayerCharacter>();
        public static readonly Dictionary<int, MonsterCharacter> MonsterCharacters = new Dictionary<int, MonsterCharacter>();
        public static readonly Dictionary<int, ArmorType> ArmorTypes = new Dictionary<int, ArmorType>();
        public static readonly Dictionary<int, WeaponType> WeaponTypes = new Dictionary<int, WeaponType>();
        public static readonly Dictionary<int, AmmoType> AmmoTypes = new Dictionary<int, AmmoType>();
        public static readonly Dictionary<int, BaseSkill> Skills = new Dictionary<int, BaseSkill>();
        public static readonly Dictionary<int, BaseNpcDialog> NpcDialogs = new Dictionary<int, BaseNpcDialog>();
        public static readonly Dictionary<int, Quest> Quests = new Dictionary<int, Quest>();
        public static readonly Dictionary<int, PlayerIcon> PlayerIcons = new Dictionary<int, PlayerIcon>();
        public static readonly Dictionary<int, PlayerFrame> PlayerFrames = new Dictionary<int, PlayerFrame>();
        public static readonly Dictionary<int, PlayerTitle> PlayerTitles = new Dictionary<int, PlayerTitle>();
        public static readonly Dictionary<int, GuildSkill> GuildSkills = new Dictionary<int, GuildSkill>();
        public static readonly Dictionary<int, GuildIcon> GuildIcons = new Dictionary<int, GuildIcon>();
        public static readonly Dictionary<int, Gacha> Gachas = new Dictionary<int, Gacha>();
        public static readonly Dictionary<int, StatusEffect> StatusEffects = new Dictionary<int, StatusEffect>();
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
        public static readonly Dictionary<int, LiteNetLibIdentity> OtherNetworkObjectPrefabs = new Dictionary<int, LiteNetLibIdentity>();
        public static readonly HashSet<IPoolDescriptor> PoolingObjectPrefabs = new HashSet<IPoolDescriptor>();

        [Header("Gameplay Systems")]
        [SerializeField]
        private DimensionType dimensionType = DimensionType.Dimension3D;
        [SerializeField]
        private BaseGameSaveSystem saveSystem = null;
        [SerializeField]
        private BaseGameplayRule gameplayRule = null;
        [SerializeField]
        private BaseDayNightTimeUpdater dayNightTimeUpdater = null;
        [SerializeField]
        private BaseGMCommands gmCommands = null;
        [SerializeField]
        private NetworkSetting networkSetting = null;

        [Header("Gameplay Objects")]
        public ItemDropEntity itemDropEntityPrefab = null;
        public WarpPortalEntity warpPortalEntityPrefab = null;
        public ItemsContainerEntity playerCorpsePrefab = null;
        public ItemsContainerEntity monsterCorpsePrefab = null;
        public BaseUISceneGameplay uiSceneGameplayPrefab = null;
        [Tooltip("If this is empty, it will use `UI Scene Gameplay Prefab` as gameplay UI prefab")]
        public BaseUISceneGameplay uiSceneGameplayMobilePrefab = null;
        [Tooltip("If this is empty, it will use `UI Scene Gameplay Prefab` as gameplay UI prefab")]
        public BaseUISceneGameplay uiSceneGameplayConsolePrefab = null;
        [Tooltip("Default controller prefab will be used when controller prefab at player character entity is null")]
        public BasePlayerCharacterController defaultControllerPrefab = null;
        [Tooltip("This is camera controller when start game as server (not start with client as host)")]
        public ServerCharacter serverCharacterPrefab = null;
        [Tooltip("These objects will be instantiate as owning character's children")]
        public GameObject[] owningCharacterObjects = new GameObject[0];
        [Tooltip("These objects will be instantiate as owning character's children to show in minimap")]
        public GameObject[] owningCharacterMiniMapObjects = new GameObject[0];
        [Tooltip("These objects will be instantiate as non-owning character's children")]
        public GameObject[] nonOwningCharacterObjects = new GameObject[0];
        [Tooltip("These objects will be instantiate as non-owning character's children to show in minimap")]
        public GameObject[] nonOwningCharacterMiniMapObjects = new GameObject[0];
        [Tooltip("These objects will be instantiate as monster character's children")]
        public GameObject[] monsterCharacterObjects = new GameObject[0];
        [Tooltip("These objects will be instantiate as monster character's children to show in minimap")]
        public GameObject[] monsterCharacterMiniMapObjects = new GameObject[0];
        [Tooltip("These objects will be instantiate as npc's children")]
        public GameObject[] npcObjects = new GameObject[0];
        [Tooltip("These objects will be instantiate as npc's children to show in minimap")]
        public GameObject[] npcMiniMapObjects = new GameObject[0];
        [Tooltip("This UI will be instaniate as owning character's child to show character name / HP / MP / Food / Water")]
        public UICharacterEntity owningCharacterUI = null;
        [Tooltip("This UI will be instaniate as non owning character's child to show character name / HP / MP / Food / Water")]
        public UICharacterEntity nonOwningCharacterUI = null;
        [Tooltip("This UI will be instaniate as monster character's child to show character name / HP / MP / Food / Water")]
        public UICharacterEntity monsterCharacterUI = null;
        [Tooltip("This UI will be instaniate as NPC's child to show character name")]
        public UINpcEntity npcUI = null;
        [Tooltip("This UI will be instaniate as NPC's child to show quest indecator")]
        public NpcQuestIndicator npcQuestIndicator = null;

        [Header("Gameplay Effects")]
        [SerializeField]
        private GameEffect levelUpEffect = null;
        [SerializeField]
        private GameEffect[] stunEffects = new GameEffect[0];
        [SerializeField]
        private GameEffect[] muteEffects = new GameEffect[0];
        [SerializeField]
        private GameEffect[] freezeEffects = new GameEffect[0];

        [Header("Gameplay Database and Default Data")]
        [Tooltip("Exp tree for both player character and monster character")]
        [SerializeField]
        private int[] expTree = new int[0];
        [Tooltip("You should add game data to game database and set the game database to this. If you leave this empty, it will load game data from Resources folders")]
        [SerializeField]
        private BaseGameDatabase gameDatabase = null;
        [Tooltip("You can add warp portals to warp portal database or may add warp portals into the scene directly, So you can leave this empty uf you are going to add warp portals into the scene directly only")]
        [SerializeField]
        private NpcDatabase npcDatabase = null;
        [Tooltip("You can add social system settings or leave this empty to use default settings")]
        [SerializeField]
        private WarpPortalDatabase warpPortalDatabase = null;
        [Tooltip("You can add NPCs to NPC database or may add NPCs into the scene directly, so you can leave this empty if you are going to add NPCs into the scene directly only")]
        [SerializeField]
        private SocialSystemSetting socialSystemSetting = null;
        [Tooltip("Default weapon item, will be used when character not equip any weapon")]
        [SerializeField]
        private BaseItem defaultWeaponItem = null;
        [Tooltip("Default damage element, will be used when attacks to enemies or receives damages from enemies")]
        [SerializeField]
        private DamageElement defaultDamageElement = null;
        [Tooltip("Default hit effects, will be used when attack to enemies or receive damages from enemies")]
        [SerializeField]
        private GameEffect[] defaultDamageHitEffects = new GameEffect[0];

        [Header("Object Tags and Layers")]
        [Tooltip("Tag for player character entities, this tag will set to player character entities game object when instantiated")]
        public UnityTag playerTag = new UnityTag("PlayerTag");
        [Tooltip("Tag for monster character entities, this tag will set to monster character entities game object when instantiated")]
        public UnityTag monsterTag = new UnityTag("MonsterTag");
        [Tooltip("Tag for NPC entities, this tag will set to NPC entities game object when instantiated")]
        public UnityTag npcTag = new UnityTag("NpcTag");
        [Tooltip("Tag for vehicle entities, this tag will set to vehicle entities game object when instantiated")]
        public UnityTag vehicleTag = new UnityTag("VehicleTag");
        [Tooltip("Tag for item drop entities, this tag will set to item drop entities game object when instantiated")]
        public UnityTag itemDropTag = new UnityTag("ItemDropTag");
        [Tooltip("Tag for building entities, this tag will set to building entities game object when instantiated")]
        public UnityTag buildingTag = new UnityTag("BuildingTag");
        [Tooltip("Tag for harvestable entities, this tag will set to harvestable entities game object when instantiated")]
        public UnityTag harvestableTag = new UnityTag("HarvestableTag");
        [Tooltip("Layer for player character entities, this layer will be set to player character entities game object when instantiated")]
        public UnityLayer playerLayer = new UnityLayer(17);
        [Tooltip("Layer for playing character entities, this layer will be set to playing character entities game object when instantiated")]
        public UnityLayer playingLayer = new UnityLayer(17);
        [Tooltip("Layer for monster character entities, this layer will be set to monster character entities game object when instantiated")]
        public UnityLayer monsterLayer = new UnityLayer(18);
        [Tooltip("Layer for NPC entities, this layer will be set to NPC entities game object when instantiated")]
        public UnityLayer npcLayer = new UnityLayer(19);
        [Tooltip("Layer for vehicle entities, this layer will be set to vehicle entities game object when instantiated")]
        public UnityLayer vehicleLayer = new UnityLayer(20);
        [Tooltip("Layer for item drop entities, this layer will set to item drop entities game object when instantiated")]
        public UnityLayer itemDropLayer = new UnityLayer(9);
        [Tooltip("Layer for building entities, this layer will set to building entities game object when instantiated")]
        public UnityLayer buildingLayer = new UnityLayer(13);
        [Tooltip("Layer for harvestable entities, this layer will set to harvestable entities game object when instantiated")]
        public UnityLayer harvestableLayer = new UnityLayer(14);
        [Tooltip("Layers which will be used when raycasting to find hitting obstacle/wall/floor/ceil when attacking damageable objects")]
        public UnityLayer[] attackObstacleLayers = new UnityLayer[]
        {
            new UnityLayer(0),
        };
        [Tooltip("Layers which will be ignored when raycasting")]
        [FormerlySerializedAs("nonTargetingLayers")]
        public UnityLayer[] ignoreRaycastLayers = new UnityLayer[] {
            new UnityLayer(11)
        };

        [Header("Gameplay Configs - Generic")]
        [Tooltip("If dropped items does not picked up within this duration, it will be destroyed from the server")]
        public float itemAppearDuration = 60f;
        [Tooltip("If dropped items does not picked up by killer within this duration, anyone can pick up the items")]
        public float itemLootLockDuration = 5f;
        [Tooltip("If this is `TRUE` anyone can pick up items which drops by players immediately")]
        public bool canPickupItemsWhichDropsByPlayersImmediately = false;
        [Tooltip("If dealing request does not accepted within this duration, the request will be cancelled")]
        public float dealingRequestDuration = 5f;
        [Tooltip("If this is `TRUE`, dealing feature will be disabled, all players won't be able to deal items to each other")]
        public bool disableDealing = false;
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
        [Tooltip("This is a distance from monster killer to other characters in party to share EXP, if this value is <= 0, it will share EXP to all other characters in the same map")]
        public float partyShareExpDistance = 0f;
        [Tooltip("This is a distance from monster killer to other characters in party to share item (allow other characters to pickup item immediately), if this value is <= 0, it will share item to all other characters in the same map")]
        public float partyShareItemDistance = 0f;
        [Tooltip("Maximum number of equip weapon set")]
        [Range(1, 16)]
        public byte maxEquipWeaponSet = 2;
        [Tooltip("How character position load when start game")]
        public CurrentPositionSaveMode currentPositionSaveMode = CurrentPositionSaveMode.UseCurrentPosition;
        [Tooltip("How player drop item")]
        public PlayerDropItemMode playerDropItemMode = PlayerDropItemMode.DropOnGround;
        [Tooltip("How player character drop item when dying (it will drop items if map info was set to drop items)")]
        public DeadDropItemMode playerDeadDropItemMode = DeadDropItemMode.DropOnGround;
        [Tooltip("If all items does not picked up from corpse within this duration, it will be destroyed from the server")]
        public float playerCorpseAppearDuration = 60f;
        [Tooltip("How monster character drop item when dying")]
        public DeadDropItemMode monsterDeadDropItemMode = DeadDropItemMode.DropOnGround;
        [Tooltip("If all items does not picked up from corpse within this duration, it will be destroyed from the server")]
        public float monsterCorpseAppearDuration = 60f;
        [Tooltip("Delay before return move speed while attack or use skill to generic move speed")]
        public float returnMoveSpeedDelayAfterAction = 0.1f;
        [Tooltip("Delay before mount again")]
        public float mountDelay = 1f;
        [Tooltip("Delay before use item again")]
        public float useItemDelay = 0.25f;
        [Tooltip("If this is `TRUE`, it will clear skills cooldown when character dead")]
        public bool clearSkillCooldownOnDead = true;

        [Header("Gameplay Configs - Items, Inventory and Storage")]
        public ItemTypeFilter dismantleFilter = new ItemTypeFilter()
        {
            includeArmor = true,
            includeShield = true,
            includeWeapon = true
        };
        [Tooltip("If this is `TRUE`, player will be able to refine an items by themself, doesn't have to talk to NPCs")]
        public bool canRefineItemByPlayer = false;
        [Tooltip("If this is `TRUE`, player will be able to dismantle an items by themself, doesn't have to talk to NPCs")]
        public bool canDismantleItemByPlayer = false;
        [Tooltip("If this is `TRUE`, player will be able to repair an items by themself, doesn't have to talk to NPCs")]
        public bool canRepairItemByPlayer = false;
        [Tooltip("How player's inventory works")]
        public InventorySystem inventorySystem = InventorySystem.Simple;
        [Tooltip("If this is `TRUE`, weight limit won't be applied")]
        public bool noInventoryWeightLimit;
        [Tooltip("If this is `TRUE` it won't fill empty slots")]
        public bool doNotFillEmptySlots = false;
        [Tooltip("Base slot limit for all characters, it will be used when `InventorySystem` is `LimitSlots`")]
        public int baseSlotLimit = 0;
        public Storage playerStorage = default;
        public Storage guildStorage = default;
        public EnhancerRemoval enhancerRemoval = default;

        [Header("Gameplay Configs - Summon Monster")]
        [Tooltip("This is a distance that random summon around a character")]
        public float minSummonDistance = 2f;
        [Tooltip("This is a distance that random summon around a character")]
        public float maxSummonDistance = 3f;
        [Tooltip("Min distance to follow summoner")]
        public float minFollowSummonerDistance = 5f;
        [Tooltip("Max distance to follow summoner, if distance between characters more than this it will teleport to summoner")]
        public float maxFollowSummonerDistance = 10f;

        [Header("Gameplay Configs - Summon Pet Item")]
        [Tooltip("This is duration to lock item before it is able to summon later after character dead")]
        public float petDeadLockDuration = 60f;
        [Tooltip("This is duration to lock item before it is able to summon later after unsummon")]
        public float petUnSummonLockDuration = 30f;

        [Header("Gameplay Configs - Instance Dungeon")]
        [Tooltip("Distance from party leader character to join instance map")]
        public float joinInstanceMapDistance = 20f;

        [Header("New Character")]
        [Tooltip("If this is NULL, it will use `startGold` and `startItems`")]
        public NewCharacterSetting newCharacterSetting;
        [Tooltip("Amount of gold that will be added to character when create new character")]
        public int startGold = 0;
        [Tooltip("Items that will be added to character when create new character")]
        [ArrayElementTitle("item")]
        public ItemAmount[] startItems = new ItemAmount[0];

        [Header("Scene/Maps")]
        public UnityScene homeScene = default;
        [Tooltip("If this is empty, it will use `Home Scene` as home scene")]
        public UnityScene homeMobileScene = default;
        [Tooltip("If this is empty, it will use `Home Scene` as home scene")]
        public UnityScene homeConsoleScene = default;

        [Header("Server Settings")]
        public bool updateAnimationAtServer = true;

        [Header("Player Configs")]
        public int minCharacterNameLength = 2;
        public int maxCharacterNameLength = 16;
        [Tooltip("Max characters that player can create, set it to 0 to unlimit")]
        public byte maxCharacterSaves = 5;

        [Header("Platforms Configs")]
        public int serverTargetFrameRate = 30;

        [Header("Playing In Editor")]
        public TestInEditorMode testInEditorMode = TestInEditorMode.Standalone;

        // Static events
        public static event System.Action<IPlayerCharacterData> onSetPlayingCharacter;
        // Events
        public System.Action onGameDataLoaded;

        #region Cache Data
        public EventSystemManager EventSystemManager { get; private set; }

        public DimensionType DimensionType
        {
            get { return dimensionType; }
        }

        public bool IsLimitInventorySlot
        {
            get { return inventorySystem == InventorySystem.LimitSlots; }
        }

        public bool IsLimitInventoryWeight
        {
            get { return !noInventoryWeightLimit; }
        }

        public BaseGameSaveSystem SaveSystem
        {
            get { return saveSystem; }
        }

        public BaseGameplayRule GameplayRule
        {
            get { return gameplayRule; }
        }

        public BaseDayNightTimeUpdater DayNightTimeUpdater
        {
            get { return dayNightTimeUpdater; }
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
                if ((Application.isMobilePlatform || IsMobileTestInEditor()) && uiSceneGameplayMobilePrefab != null)
                    return uiSceneGameplayMobilePrefab;
                if ((Application.isConsolePlatform || IsConsoleTestInEditor()) && uiSceneGameplayConsolePrefab != null)
                    return uiSceneGameplayConsolePrefab;
                return uiSceneGameplayPrefab;
            }
        }

        public string HomeSceneName
        {
            get
            {
                if ((Application.isMobilePlatform || IsMobileTestInEditor()) && homeMobileScene.IsSet())
                    return homeMobileScene;
                if ((Application.isConsolePlatform || IsConsoleTestInEditor()) && homeConsoleScene.IsSet())
                    return homeConsoleScene;
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

        public GameEffect LevelUpEffect
        {
            get { return levelUpEffect; }
        }

        public GameEffect[] StunEffects
        {
            get { return stunEffects; }
        }

        public GameEffect[] MuteEffects
        {
            get { return muteEffects; }
        }

        public GameEffect[] FreezeEffects
        {
            get { return freezeEffects; }
        }

        public ArmorType DefaultArmorType
        {
            get; private set;
        }

        public WeaponType DefaultWeaponType
        {
            get; private set;
        }

        public IWeaponItem DefaultWeaponItem
        {
            get { return defaultWeaponItem as IWeaponItem; }
        }

        public IWeaponItem MonsterWeaponItem
        {
            get; private set;
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

        public HashSet<int> IgnoreRaycastLayersValues { get; private set; }

        public readonly Dictionary<string, bool> LoadHomeScenePreventions = new Dictionary<string, bool>();
        public bool DoNotLoadHomeScene
        {
            get
            {
                foreach (bool doNotLoad in LoadHomeScenePreventions.Values)
                {
                    if (doNotLoad)
                        return true;
                }
                return false;
            }
        }
        #endregion

        protected virtual void Awake()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                // Set target framerate when running headless to reduce CPU usage
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = serverTargetFrameRate;
            }
            Application.runInBackground = true;
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Singleton = this;
            LoadHomeScenePreventions.Clear();
            EventSystemManager = gameObject.GetOrAddComponent<EventSystemManager>();
            InputManager.useMobileInputOnNonMobile = IsMobileTestInEditor();
            InputManager.useNonMobileInput = testInEditorMode == TestInEditorMode.MobileWithKeyInputs && Application.isEditor;

            DefaultArmorType = ScriptableObject.CreateInstance<ArmorType>()
                .GenerateDefaultArmorType();

            DefaultWeaponType = ScriptableObject.CreateInstance<WeaponType>()
                .GenerateDefaultWeaponType();

            // Setup default weapon item if not existed
            if (defaultWeaponItem == null || !defaultWeaponItem.IsWeapon())
            {
                defaultWeaponItem = ScriptableObject.CreateInstance<Item>()
                    .GenerateDefaultItem(DefaultWeaponType);
                // Use the same item with default weapon item (if default weapon not set by user)
                MonsterWeaponItem = defaultWeaponItem as IWeaponItem;
            }

            // Setup monster weapon item if not existed
            if (MonsterWeaponItem == null)
            {
                MonsterWeaponItem = ScriptableObject.CreateInstance<Item>()
                    .GenerateDefaultItem(DefaultWeaponType);
            }

            // Setup default damage element if not existed
            if (defaultDamageElement == null)
            {
                defaultDamageElement = ScriptableObject.CreateInstance<DamageElement>()
                    .GenerateDefaultDamageElement(DefaultDamageHitEffects);
            }

            // Setup save system if not existed
            if (saveSystem == null)
                saveSystem = ScriptableObject.CreateInstance<DefaultGameSaveSystem>();

            // Setup gameplay rule if not existed
            if (gameplayRule == null)
                gameplayRule = ScriptableObject.CreateInstance<DefaultGameplayRule>();

            // Reset gold and exp rate
            gameplayRule.GoldRate = 1f;
            gameplayRule.ExpRate = 1f;

            // Setup day night time updater if not existed
            if (dayNightTimeUpdater == null)
                dayNightTimeUpdater = ScriptableObject.CreateInstance<DefaultDayNightTimeUpdater>();

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
            IgnoreRaycastLayersValues = new HashSet<int>();
            foreach (UnityLayer layer in ignoreRaycastLayers)
            {
                IgnoreRaycastLayersValues.Add(layer.LayerIndex);
            }

            ClearData();
            this.InvokeInstanceDevExtMethods("Awake");
        }

        protected virtual void Start()
        {
            GameDatabase.LoadData(this).Forget();
        }

        protected virtual void OnDestroy()
        {
            this.InvokeInstanceDevExtMethods("OnDestroy");
        }

        public static void ClearData()
        {
            Attributes.Clear();
            Currencies.Clear();
            Items.Clear();
            ItemsByAmmoType.Clear();
            ItemCraftFormulas.Clear();
            Harvestables.Clear();
            Characters.Clear();
            PlayerCharacters.Clear();
            MonsterCharacters.Clear();
            ArmorTypes.Clear();
            WeaponTypes.Clear();
            AmmoTypes.Clear();
            Skills.Clear();
            NpcDialogs.Clear();
            Quests.Clear();
            PlayerIcons.Clear();
            PlayerFrames.Clear();
            PlayerTitles.Clear();
            GuildSkills.Clear();
            GuildIcons.Clear();
            Gachas.Clear();
            StatusEffects.Clear();
            DamageElements.Clear();
            EquipmentSets.Clear();
            BuildingEntities.Clear();
            CharacterEntities.Clear();
            PlayerCharacterEntities.Clear();
            MonsterCharacterEntities.Clear();
            ItemDropEntities.Clear();
            HarvestableEntities.Clear();
            VehicleEntities.Clear();
            WarpPortalEntities.Clear();
            NpcEntities.Clear();
            MapWarpPortals.Clear();
            MapNpcs.Clear();
            MapInfos.Clear();
            Factions.Clear();
            PoolingObjectPrefabs.Clear();
            OtherNetworkObjectPrefabs.Clear();
            GameEntityModel.GeneratingId = 0;
        }

        public bool IsMobileTestInEditor()
        {
            return (testInEditorMode == TestInEditorMode.Mobile || testInEditorMode == TestInEditorMode.MobileWithKeyInputs) && Application.isEditor;
        }

        public bool IsConsoleTestInEditor()
        {
            return testInEditorMode == TestInEditorMode.Console && Application.isEditor;
        }

        public void LoadedGameData()
        {
            this.InvokeInstanceDevExtMethods("LoadedGameData");
            // Add ammo items to dictionary
            foreach (BaseItem item in Items.Values)
            {
                if (item.IsAmmo())
                {
                    IAmmoItem ammoItem = item as IAmmoItem;
                    if (!ItemsByAmmoType.ContainsKey(ammoItem.AmmoType.DataId))
                        ItemsByAmmoType.Add(ammoItem.AmmoType.DataId, new Dictionary<int, BaseItem>());
                    ItemsByAmmoType[ammoItem.AmmoType.DataId][item.DataId] = item;
                }
            }

            // Add required default game data
            AddItems(new BaseItem[] {
                DefaultWeaponItem as BaseItem,
                MonsterWeaponItem as BaseItem
            });
            AddPoolingObjects(LevelUpEffect);
            AddPoolingObjects(StunEffects);
            AddPoolingObjects(MuteEffects);
            AddPoolingObjects(FreezeEffects);
            AddPoolingObjects(DefaultDamageHitEffects);

            if (newCharacterSetting != null && newCharacterSetting.startItems != null)
                AddItems(newCharacterSetting.startItems);

            if (startItems != null)
                AddItems(startItems);

            if (warpPortalDatabase != null && warpPortalDatabase.maps != null)
                AddMapWarpPortals(warpPortalDatabase.maps);

            if (npcDatabase != null && npcDatabase.maps != null)
                AddMapNpcs(npcDatabase.maps);

            if (Application.isPlaying)
                InitializePurchasing();

            OnGameDataLoaded();

            System.GC.Collect();
        }

        public void OnGameDataLoaded()
        {
            if (onGameDataLoaded != null)
                onGameDataLoaded.Invoke();
            if (Application.isPlaying && !DoNotLoadHomeScene)
                LoadHomeScene();
        }

        public void LoadHomeScene()
        {
            StartCoroutine(LoadHomeSceneRoutine());
        }

        IEnumerator LoadHomeSceneRoutine()
        {
            if (UISceneLoading.Singleton)
                yield return UISceneLoading.Singleton.LoadScene(HomeSceneName);
            else
                yield return SceneManager.LoadSceneAsync(HomeSceneName);
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

        private int MixWithAttackObstacleLayers(int layerMask)
        {
            if (attackObstacleLayers.Length > 0)
            {
                foreach (UnityLayer attackObstacleLayer in attackObstacleLayers)
                {
                    layerMask = layerMask | attackObstacleLayer.Mask;
                }
            }
            return layerMask;
        }

        private int MixWithIgnoreRaycastLayers(int layerMask)
        {
            if (ignoreRaycastLayers.Length > 0)
            {
                foreach (UnityLayer ignoreRaycastLayer in ignoreRaycastLayers)
                {
                    layerMask = layerMask | ignoreRaycastLayer.Mask;
                }
            }
            layerMask = layerMask | 1 << PhysicLayers.IgnoreRaycast;
            return layerMask;
        }

        /// <summary>
        /// All layers except `nonTargetingLayers`, `TransparentFX`, `IgnoreRaycast`, `Water` will be used for raycasting
        /// </summary>
        /// <returns></returns>
        public int GetTargetLayerMask()
        {
            // 0 = Nothing, -1 = AllLayers
            int layerMask = 0;
            layerMask = layerMask | 1 << PhysicLayers.TransparentFX;
            layerMask = layerMask | 1 << PhysicLayers.Water;
            layerMask = MixWithIgnoreRaycastLayers(layerMask);
            return ~layerMask;
        }

        /// <summary>
        /// Check is layer is layer for any damageable entities or not
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public bool IsDamageableLayer(int layer)
        {
            return layer == playerLayer ||
                layer == playingLayer ||
                layer == monsterLayer ||
                layer == vehicleLayer ||
                layer == buildingLayer ||
                layer == harvestableLayer;
        }

        /// <summary>
        /// Only `playerLayer`, `playingLayer`, `monsterLayer`, `vehicleLayer`, `buildingLayer`, `harvestableLayer` will be used for hit detection casting
        /// </summary>
        /// <returns></returns>
        public int GetDamageableLayerMask()
        {
            int layerMask = 0;
            layerMask = layerMask | playerLayer.Mask;
            layerMask = layerMask | playingLayer.Mask;
            layerMask = layerMask | monsterLayer.Mask;
            layerMask = layerMask | vehicleLayer.Mask;
            layerMask = layerMask | buildingLayer.Mask;
            layerMask = layerMask | harvestableLayer.Mask;
            return layerMask;
        }

        /// <summary>
        /// Only `playerLayer`, `playingLayer`, `monsterLayer`, `vehicleLayer`, `buildingLayer`, `harvestableLayer` and wall layers will be used for hit detection casting
        /// </summary>
        /// <returns></returns>
        public int GetDamageEntityHitLayerMask()
        {
            int layerMask = 0;
            layerMask = layerMask | playerLayer.Mask;
            layerMask = layerMask | playingLayer.Mask;
            layerMask = layerMask | monsterLayer.Mask;
            layerMask = layerMask | vehicleLayer.Mask;
            layerMask = layerMask | buildingLayer.Mask;
            layerMask = layerMask | harvestableLayer.Mask;
            layerMask = MixWithAttackObstacleLayers(layerMask);
            return layerMask;
        }

        /// <summary>
        /// All layers except `playerLayer`, `playingLayer`, `monsterLayer`, `npcLayer`, `vehicleLayer`, `itemDropLayer, `harvestableLayer`, `TransparentFX`, `IgnoreRaycast`, `Water` will be used for raycasting
        /// </summary>
        /// <returns></returns>
        public int GetBuildLayerMask()
        {
            int layerMask = 0;
            layerMask = layerMask | 1 << PhysicLayers.TransparentFX;
            layerMask = layerMask | 1 << PhysicLayers.Water;
            layerMask = layerMask | 1 << PhysicLayers.IgnoreRaycast;
            layerMask = layerMask | playerLayer.Mask;
            layerMask = layerMask | playingLayer.Mask;
            layerMask = layerMask | monsterLayer.Mask;
            layerMask = layerMask | npcLayer.Mask;
            layerMask = layerMask | vehicleLayer.Mask;
            layerMask = layerMask | itemDropLayer.Mask;
            layerMask = layerMask | harvestableLayer.Mask;
            return ~layerMask;
        }

        /// <summary>
        /// All layers except `playerLayer`, `playingLayer`, `monsterLayer`, `npcLayer`, `vehicleLayer`, `itemDropLayer, `TransparentFX`, `IgnoreRaycast`, `Water` and non-target layers will be used for raycasting
        /// </summary>
        /// <returns></returns>
        public int GetItemDropGroundDetectionLayerMask()
        {
            int layerMask = 0;
            layerMask = layerMask | 1 << PhysicLayers.TransparentFX;
            layerMask = layerMask | 1 << PhysicLayers.Water;
            layerMask = layerMask | playerLayer.Mask;
            layerMask = layerMask | playingLayer.Mask;
            layerMask = layerMask | monsterLayer.Mask;
            layerMask = layerMask | npcLayer.Mask;
            layerMask = layerMask | vehicleLayer.Mask;
            layerMask = layerMask | itemDropLayer.Mask;
            layerMask = MixWithIgnoreRaycastLayers(layerMask);
            return ~layerMask;
        }

        /// <summary>
        /// All layers except `playerLayer`, `playingLayer`, `monsterLayer`, `npcLayer`, `vehicleLayer`, `itemDropLayer, `TransparentFX`, `IgnoreRaycast`, `Water` and non-target layers will be used for raycasting
        /// </summary>
        /// <returns></returns>
        public int GetGameEntityGroundDetectionLayerMask()
        {
            int layerMask = 0;
            layerMask = layerMask | 1 << PhysicLayers.TransparentFX;
            layerMask = layerMask | 1 << PhysicLayers.Water;
            layerMask = layerMask | playerLayer.Mask;
            layerMask = layerMask | playingLayer.Mask;
            layerMask = layerMask | monsterLayer.Mask;
            layerMask = layerMask | npcLayer.Mask;
            layerMask = layerMask | vehicleLayer.Mask;
            layerMask = layerMask | itemDropLayer.Mask;
            layerMask = MixWithIgnoreRaycastLayers(layerMask);
            return ~layerMask;
        }

        /// <summary>
        /// All layers except `playerLayer`, `playingLayer`, `monsterLayer`, `npcLayer`, `vehicleLayer`, `itemDropLayer`, `buildingLayer`, `harvestableLayer, `TransparentFX`, `IgnoreRaycast`, `Water` and non-target layers will be used for raycasting
        /// </summary>
        /// <returns></returns>
        public int GetHarvestableSpawnGroundDetectionLayerMask()
        {
            int layerMask = 0;
            layerMask = layerMask | 1 << PhysicLayers.TransparentFX;
            layerMask = layerMask | 1 << PhysicLayers.Water;
            layerMask = layerMask | playerLayer.Mask;
            layerMask = layerMask | playingLayer.Mask;
            layerMask = layerMask | monsterLayer.Mask;
            layerMask = layerMask | npcLayer.Mask;
            layerMask = layerMask | vehicleLayer.Mask;
            layerMask = layerMask | itemDropLayer.Mask;
            layerMask = layerMask | buildingLayer.Mask;
            layerMask = layerMask | harvestableLayer.Mask;
            layerMask = MixWithIgnoreRaycastLayers(layerMask);
            return ~layerMask;
        }

        /// <summary>
        /// All layers except `playerLayer`, `playingLayer`, `monsterLayer`, `npcLayer`, `vehicleLayer`, `itemDropLayer`, `harvestableLayer, `TransparentFX`, `IgnoreRaycast`, `Water` and non-target layers will be used for raycasting
        /// </summary>
        /// <returns></returns>
        public int GetAreaSkillGroundDetectionLayerMask()
        {
            int layerMask = 0;
            layerMask = layerMask | 1 << PhysicLayers.TransparentFX;
            layerMask = layerMask | 1 << PhysicLayers.Water;
            layerMask = layerMask | playerLayer.Mask;
            layerMask = layerMask | playingLayer.Mask;
            layerMask = layerMask | monsterLayer.Mask;
            layerMask = layerMask | npcLayer.Mask;
            layerMask = layerMask | vehicleLayer.Mask;
            layerMask = layerMask | itemDropLayer.Mask;
            layerMask = layerMask | harvestableLayer.Mask;
            layerMask = MixWithIgnoreRaycastLayers(layerMask);
            return ~layerMask;
        }

        #region Add game data functions
        public static void AddAttributes(params Attribute[] attributes)
        {
            AddAttributes((IEnumerable<Attribute>)attributes);
        }

        public static void AddAttributes(IEnumerable<Attribute> attributes)
        {
            AddManyGameData(Attributes, attributes);
        }

        public static void AddAttributes(params AttributeAmount[] attributeAmounts)
        {
            AddAttributes((IEnumerable<AttributeAmount>)attributeAmounts);
        }

        public static void AddAttributes(IEnumerable<AttributeAmount> attributeAmounts)
        {
            if (attributeAmounts == null)
                return;
            foreach (AttributeAmount attributeAmount in attributeAmounts)
            {
                AddGameData(Attributes, attributeAmount.attribute);
            }
        }

        public static void AddAttributes(params AttributeRandomAmount[] attributeAmounts)
        {
            AddAttributes((IEnumerable<AttributeRandomAmount>)attributeAmounts);
        }

        public static void AddAttributes(IEnumerable<AttributeRandomAmount> attributeAmounts)
        {
            if (attributeAmounts == null)
                return;
            foreach (AttributeRandomAmount attributeAmount in attributeAmounts)
            {
                AddGameData(Attributes, attributeAmount.attribute);
            }
        }

        public static void AddAttributes(params AttributeIncremental[] attributeIncrementals)
        {
            AddAttributes((IEnumerable<AttributeIncremental>)attributeIncrementals);
        }

        public static void AddAttributes(IEnumerable<AttributeIncremental> attributeIncrementals)
        {
            if (attributeIncrementals == null)
                return;
            foreach (AttributeIncremental attributeIncremental in attributeIncrementals)
            {
                AddGameData(Attributes, attributeIncremental.attribute);
            }
        }

        public static void AddCurrencies(params Currency[] currencies)
        {
            AddCurrencies((IEnumerable<Currency>)currencies);
        }

        public static void AddCurrencies(IEnumerable<Currency> currencies)
        {
            AddManyGameData(Currencies, currencies);
        }

        public static void AddCurrencies(params CurrencyAmount[] currencyAmounts)
        {
            AddCurrencies((IEnumerable<CurrencyAmount>)currencyAmounts);
        }

        public static void AddCurrencies(IEnumerable<CurrencyAmount> currencyAmounts)
        {
            if (currencyAmounts == null)
                return;
            foreach (CurrencyAmount currencyAmount in currencyAmounts)
            {
                AddGameData(Currencies, currencyAmount.currency);
            }
        }

        public static void AddItems(params ItemAmount[] itemAmounts)
        {
            AddItems((IEnumerable<ItemAmount>)itemAmounts);
        }

        public static void AddItems(IEnumerable<ItemAmount> itemAmounts)
        {
            if (itemAmounts == null)
                return;
            foreach (ItemAmount itemAmount in itemAmounts)
            {
                AddItems(itemAmount.item);
            }
        }

        public static void AddItems(params ItemDrop[] itemDrops)
        {
            AddItems((IEnumerable<ItemDrop>)itemDrops);
        }

        public static void AddItems(IEnumerable<ItemDrop> itemDrops)
        {
            if (itemDrops == null)
                return;
            foreach (ItemDrop itemDrop in itemDrops)
            {
                AddItems(itemDrop.item);
            }
        }

        public static void AddItems(params ItemDropForHarvestable[] itemDrops)
        {
            AddItems((IEnumerable<ItemDropForHarvestable>)itemDrops);
        }

        public static void AddItems(IEnumerable<ItemDropForHarvestable> itemDrops)
        {
            if (itemDrops == null)
                return;
            foreach (ItemDropForHarvestable itemDrop in itemDrops)
            {
                AddItems(itemDrop.item);
            }
        }

        public static void AddItems(params ItemRandomByWeight[] itemDrops)
        {
            AddItems((IEnumerable<ItemRandomByWeight>)itemDrops);
        }

        public static void AddItems(IEnumerable<ItemRandomByWeight> itemDrops)
        {
            if (itemDrops == null)
                return;
            foreach (ItemRandomByWeight itemDrop in itemDrops)
            {
                AddItems(itemDrop.item);
            }
        }

        public static void AddItems(params BaseItem[] items)
        {
            AddItems((IEnumerable<BaseItem>)items);
        }

        public static void AddItems(IEnumerable<BaseItem> items)
        {
            AddManyGameData(Items, items);
        }

        public static void AddItemCraftFormulas(int sourceId, params ItemCraftFormula[] itemCraftFormulas)
        {
            AddItemCraftFormulas(sourceId, (IEnumerable<ItemCraftFormula>)itemCraftFormulas);
        }

        public static void AddItemCraftFormulas(int sourceId, IEnumerable<ItemCraftFormula> itemCraftFormulas)
        {
            foreach (ItemCraftFormula formula in itemCraftFormulas)
            {
                formula.SourceIds.Add(sourceId);
            }
            AddManyGameData(ItemCraftFormulas, itemCraftFormulas);
        }

        public static void AddHarvestables(params Harvestable[] harvestables)
        {
            AddHarvestables((IEnumerable<Harvestable>)harvestables);
        }

        public static void AddHarvestables(IEnumerable<Harvestable> harvestables)
        {
            AddManyGameData(Harvestables, harvestables);
        }

        public static void AddArmorTypes(params ArmorType[] armorTypes)
        {
            AddArmorTypes((IEnumerable<ArmorType>)armorTypes);
        }

        public static void AddArmorTypes(IEnumerable<ArmorType> armorTypes)
        {
            AddManyGameData(ArmorTypes, armorTypes);
        }

        public static void AddWeaponTypes(params WeaponType[] weaponTypes)
        {
            AddWeaponTypes((IEnumerable<WeaponType>)weaponTypes);
        }

        public static void AddWeaponTypes(IEnumerable<WeaponType> weaponTypes)
        {
            AddManyGameData(WeaponTypes, weaponTypes);
        }

        public static void AddAmmoTypes(params AmmoType[] ammoTypes)
        {
            AddAmmoTypes((IEnumerable<AmmoType>)ammoTypes);
        }

        public static void AddAmmoTypes(IEnumerable<AmmoType> ammoTypes)
        {
            AddManyGameData(AmmoTypes, ammoTypes);
        }

        public static void AddSkills(params SkillLevel[] skillLevels)
        {
            AddSkills((IEnumerable<SkillLevel>)skillLevels);
        }

        public static void AddSkills(IEnumerable<SkillLevel> skillLevels)
        {
            if (skillLevels == null)
                return;
            foreach (SkillLevel skillLevel in skillLevels)
            {
                AddGameData(Skills, skillLevel.skill);
            }
        }

        public static void AddSkills(params SkillIncremental[] skillLevels)
        {
            AddSkills((IEnumerable<SkillIncremental>)skillLevels);
        }

        public static void AddSkills(IEnumerable<SkillIncremental> skillLevels)
        {
            if (skillLevels == null)
                return;
            foreach (SkillIncremental skillLevel in skillLevels)
            {
                AddGameData(Skills, skillLevel.skill);
            }
        }

        public static void AddSkills(params SkillRandomLevel[] skillLevels)
        {
            AddSkills((IEnumerable<SkillRandomLevel>)skillLevels);
        }

        public static void AddSkills(IEnumerable<SkillRandomLevel> skillLevels)
        {
            if (skillLevels == null)
                return;
            foreach (SkillRandomLevel skillLevel in skillLevels)
            {
                AddGameData(Skills, skillLevel.skill);
            }
        }

        public static void AddSkills(params BaseSkill[] skills)
        {
            AddSkills((IEnumerable<BaseSkill>)skills);
        }

        public static void AddSkills(IEnumerable<BaseSkill> skills)
        {
            AddManyGameData(Skills, skills);
        }

        public static void AddNpcDialogs(params BaseNpcDialog[] npcDialogs)
        {
            AddNpcDialogs((IEnumerable<BaseNpcDialog>)npcDialogs);
        }

        public static void AddNpcDialogs(IEnumerable<BaseNpcDialog> npcDialogs)
        {
            AddManyGameData(NpcDialogs, npcDialogs);
        }

        public static void AddQuests(params Quest[] quests)
        {
            AddQuests((IEnumerable<Quest>)quests);
        }

        public static void AddQuests(IEnumerable<Quest> quests)
        {
            AddManyGameData(Quests, quests);
        }

        public static void AddPlayerIcons(params PlayerIcon[] playerIcons)
        {
            AddPlayerIcons((IEnumerable<PlayerIcon>)playerIcons);
        }

        public static void AddPlayerIcons(IEnumerable<PlayerIcon> playerIcons)
        {
            AddManyGameData(PlayerIcons, playerIcons);
        }

        public static void AddPlayerFrames(params PlayerFrame[] playerFrames)
        {
            AddPlayerFrames((IEnumerable<PlayerFrame>)playerFrames);
        }

        public static void AddPlayerFrames(IEnumerable<PlayerFrame> playerFrames)
        {
            AddManyGameData(PlayerFrames, playerFrames);
        }

        public static void AddPlayerTitles(params PlayerTitle[] playerTitles)
        {
            AddPlayerTitles((IEnumerable<PlayerTitle>)playerTitles);
        }

        public static void AddPlayerTitles(IEnumerable<PlayerTitle> playerTitles)
        {
            AddManyGameData(PlayerTitles, playerTitles);
        }

        public static void AddGuildSkills(params GuildSkill[] guildSkills)
        {
            AddGuildSkills((IEnumerable<GuildSkill>)guildSkills);
        }

        public static void AddGuildSkills(IEnumerable<GuildSkill> guildSkills)
        {
            AddManyGameData(GuildSkills, guildSkills);
        }

        public static void AddGuildIcons(params GuildIcon[] guildIcons)
        {
            AddGuildIcons((IEnumerable<GuildIcon>)guildIcons);
        }

        public static void AddGuildIcons(IEnumerable<GuildIcon> guildIcons)
        {
            AddManyGameData(GuildIcons, guildIcons);
        }

        public static void AddGachas(params Gacha[] gachas)
        {
            AddGachas((IEnumerable<Gacha>)gachas);
        }

        public static void AddGachas(IEnumerable<Gacha> gachas)
        {
            AddManyGameData(Gachas, gachas);
        }

        public static void AddStatusEffects(params StatusEffectApplying[] statusEffects)
        {
            AddStatusEffects((IEnumerable<StatusEffectApplying>)statusEffects);
        }

        public static void AddStatusEffects(IEnumerable<StatusEffectApplying> statusEffects)
        {
            if (statusEffects == null)
                return;
            foreach (StatusEffectApplying statusEffect in statusEffects)
            {
                AddStatusEffects(statusEffect.statusEffect);
            }
        }

        public static void AddStatusEffects(params StatusEffect[] statusEffects)
        {
            AddStatusEffects((IEnumerable<StatusEffect>)statusEffects);
        }

        public static void AddStatusEffects(IEnumerable<StatusEffect> statusEffects)
        {
            AddManyGameData(StatusEffects, statusEffects);
        }

        public static void AddCharacters(params BaseCharacter[] characters)
        {
            AddCharacters((IEnumerable<BaseCharacter>)characters);
        }

        public static void AddCharacters(IEnumerable<BaseCharacter> characters)
        {
            if (characters == null)
                return;
            foreach (BaseCharacter character in characters)
            {
                if (AddGameData(Characters, character))
                {
                    if (character is PlayerCharacter playerCharacter)
                        AddGameData(PlayerCharacters, playerCharacter);
                    else if (character is MonsterCharacter monsterCharacter)
                        AddGameData(MonsterCharacters, monsterCharacter);
                }
            }
        }

        public static void AddMapWarpPortals(params WarpPortals[] mapWarpPortals)
        {
            AddMapWarpPortals((IEnumerable<WarpPortals>)mapWarpPortals);
        }

        public static void AddMapWarpPortals(IEnumerable<WarpPortals> mapWarpPortals)
        {
            if (mapWarpPortals == null)
                return;
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
                    AddGameEntity(WarpPortalEntities, warpPortal.entityPrefab);
                }
            }
        }

        public static void AddMapNpcs(params Npcs[] mapNpcs)
        {
            AddMapNpcs((IEnumerable<Npcs>)mapNpcs);
        }

        public static void AddMapNpcs(IEnumerable<Npcs> mapNpcs)
        {
            if (mapNpcs == null)
                return;
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
                    AddGameEntity(NpcEntities, npc.entityPrefab);
                    if (npc.startDialog != null)
                        AddGameData(NpcDialogs, npc.startDialog);
                    if (npc.graph != null)
                        AddNpcDialogs(npc.graph.GetDialogs());
                }
            }
        }

        public static void AddMapInfos(params BaseMapInfo[] mapInfos)
        {
            AddMapInfos((IEnumerable<BaseMapInfo>)mapInfos);
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

        public static void AddFactions(params Faction[] factions)
        {
            AddFactions((IEnumerable<Faction>)factions);
        }

        public static void AddFactions(IEnumerable<Faction> factions)
        {
            AddManyGameData(Factions, factions);
        }

        public static void AddDamageElements(params ArmorAmount[] armorAmounts)
        {
            AddDamageElements((IEnumerable<ArmorAmount>)armorAmounts);
        }

        public static void AddDamageElements(IEnumerable<ArmorAmount> armorAmounts)
        {
            if (armorAmounts == null)
                return;
            foreach (ArmorAmount armorAmount in armorAmounts)
            {
                AddGameData(DamageElements, armorAmount.damageElement);
            }
        }

        public static void AddDamageElements(params ArmorRandomAmount[] armorAmounts)
        {
            AddDamageElements((IEnumerable<ArmorRandomAmount>)armorAmounts);
        }

        public static void AddDamageElements(IEnumerable<ArmorRandomAmount> armorAmounts)
        {
            if (armorAmounts == null)
                return;
            foreach (ArmorRandomAmount armorAmount in armorAmounts)
            {
                AddGameData(DamageElements, armorAmount.damageElement);
            }
        }

        public static void AddDamageElements(params ArmorIncremental[] armorIncrementals)
        {
            AddDamageElements((IEnumerable<ArmorIncremental>)armorIncrementals);
        }

        public static void AddDamageElements(IEnumerable<ArmorIncremental> armorIncrementals)
        {
            if (armorIncrementals == null)
                return;
            foreach (ArmorIncremental armorIncremental in armorIncrementals)
            {
                AddGameData(DamageElements, armorIncremental.damageElement);
            }
        }

        public static void AddDamageElements(params DamageAmount[] damageAmounts)
        {
            AddDamageElements((IEnumerable<DamageAmount>)damageAmounts);
        }

        public static void AddDamageElements(IEnumerable<DamageAmount> damageAmounts)
        {
            if (damageAmounts == null)
                return;
            foreach (DamageAmount damageAmount in damageAmounts)
            {
                AddGameData(DamageElements, damageAmount.damageElement);
            }
        }

        public static void AddDamageElements(params DamageRandomAmount[] damageAmounts)
        {
            AddDamageElements((IEnumerable<DamageRandomAmount>)damageAmounts);
        }

        public static void AddDamageElements(IEnumerable<DamageRandomAmount> damageAmounts)
        {
            if (damageAmounts == null)
                return;
            foreach (DamageRandomAmount damageAmount in damageAmounts)
            {
                AddGameData(DamageElements, damageAmount.damageElement);
            }
        }

        public static void AddDamageElements(params DamageIncremental[] damageIncrementals)
        {
            AddDamageElements((IEnumerable<DamageIncremental>)damageIncrementals);
        }

        public static void AddDamageElements(IEnumerable<DamageIncremental> damageIncrementals)
        {
            if (damageIncrementals == null)
                return;
            foreach (DamageIncremental damageIncremental in damageIncrementals)
            {
                AddGameData(DamageElements, damageIncremental.damageElement);
            }
        }

        public static void AddDamageElements(params DamageElement[] damageElements)
        {
            AddDamageElements((IEnumerable<DamageElement>)damageElements);
        }

        public static void AddDamageElements(IEnumerable<DamageElement> damageElements)
        {
            AddManyGameData(DamageElements, damageElements);
        }

        public static void AddDamageElements(params ResistanceAmount[] resistanceAmounts)
        {
            AddDamageElements((IEnumerable<ResistanceAmount>)resistanceAmounts);
        }

        public static void AddDamageElements(IEnumerable<ResistanceAmount> resistanceAmounts)
        {
            if (resistanceAmounts == null)
                return;
            foreach (ResistanceAmount resistanceAmount in resistanceAmounts)
            {
                AddGameData(DamageElements, resistanceAmount.damageElement);
            }
        }

        public static void AddDamageElements(params ResistanceRandomAmount[] resistanceAmounts)
        {
            AddDamageElements((IEnumerable<ResistanceRandomAmount>)resistanceAmounts);
        }

        public static void AddDamageElements(IEnumerable<ResistanceRandomAmount> resistanceAmounts)
        {
            if (resistanceAmounts == null)
                return;
            foreach (ResistanceRandomAmount resistanceAmount in resistanceAmounts)
            {
                AddGameData(DamageElements, resistanceAmount.damageElement);
            }
        }

        public static void AddDamageElements(params ResistanceIncremental[] resistanceIncrementals)
        {
            AddDamageElements((IEnumerable<ResistanceIncremental>)resistanceIncrementals);
        }

        public static void AddDamageElements(IEnumerable<ResistanceIncremental> resistanceIncrementals)
        {
            if (resistanceIncrementals == null)
                return;
            foreach (ResistanceIncremental resistanceIncremental in resistanceIncrementals)
            {
                AddGameData(DamageElements, resistanceIncremental.damageElement);
            }
        }

        public static void AddEquipmentSets(params EquipmentSet[] equipmentSets)
        {
            AddEquipmentSets((IEnumerable<EquipmentSet>)equipmentSets);
        }

        public static void AddEquipmentSets(IEnumerable<EquipmentSet> equipmentSets)
        {
            AddManyGameData(EquipmentSets, equipmentSets);
        }
        #endregion

        #region Add game entity functions
        public static void AddCharacterEntities(params BaseCharacterEntity[] characterEntities)
        {
            AddCharacterEntities((IEnumerable<BaseCharacterEntity>)characterEntities);
        }

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
                if (characterEntity is BasePlayerCharacterEntity playerCharacterEntity)
                    AddGameEntity(PlayerCharacterEntities, playerCharacterEntity);
                else if (characterEntity is BaseMonsterCharacterEntity monsterCharacterEntity)
                    AddGameEntity(MonsterCharacterEntities, monsterCharacterEntity);
            }
        }

        public static void AddItemDropEntities(params ItemDropEntity[] itemDropEntities)
        {
            AddItemDropEntities((IEnumerable<ItemDropEntity>)itemDropEntities);
        }

        public static void AddItemDropEntities(IEnumerable<ItemDropEntity> itemDropEntities)
        {
            AddManyGameEntity(ItemDropEntities, itemDropEntities);
        }

        public static void AddHarvestableEntities(params HarvestableEntity[] harvestableEntities)
        {
            AddHarvestableEntities((IEnumerable<HarvestableEntity>)harvestableEntities);
        }

        public static void AddHarvestableEntities(IEnumerable<HarvestableEntity> harvestableEntities)
        {
            AddManyGameEntity(HarvestableEntities, harvestableEntities);
        }

        public static void AddVehicleEntities(params VehicleEntity[] vehicleEntities)
        {
            AddVehicleEntities((IEnumerable<VehicleEntity>)vehicleEntities);
        }

        public static void AddVehicleEntities(IEnumerable<VehicleEntity> vehicleEntities)
        {
            AddManyGameEntity(VehicleEntities, vehicleEntities);
        }

        public static void AddBuildingEntities(params BuildingEntity[] buildingEntities)
        {
            AddBuildingEntities((IEnumerable<BuildingEntity>)buildingEntities);
        }

        public static void AddBuildingEntities(IEnumerable<BuildingEntity> buildingEntities)
        {
            AddManyGameEntity(BuildingEntities, buildingEntities);
        }

        public static void AddWarpPortalEntities(params WarpPortalEntity[] warpPortalEntities)
        {
            AddWarpPortalEntities((IEnumerable<WarpPortalEntity>)warpPortalEntities);
        }

        public static void AddWarpPortalEntities(IEnumerable<WarpPortalEntity> warpPortalEntities)
        {
            AddManyGameEntity(WarpPortalEntities, warpPortalEntities);
        }

        public static void AddNpcEntities(params NpcEntity[] npcEntities)
        {
            AddNpcEntities((IEnumerable<NpcEntity>)npcEntities);
        }

        public static void AddNpcEntities(IEnumerable<NpcEntity> npcEntities)
        {
            AddManyGameEntity(NpcEntities, npcEntities);
        }
        #endregion

        public static void AddPoolingObjects(params IPoolDescriptor[] poolingObjects)
        {
            AddPoolingObjects((IEnumerable<IPoolDescriptor>)poolingObjects);
        }

        public static void AddPoolingObjects(IEnumerable<IPoolDescriptor> poolingObjects)
        {
            if (poolingObjects == null)
                return;
            foreach (IPoolDescriptor poolingObject in poolingObjects)
            {
                if ((poolingObject as Object) == null || PoolingObjectPrefabs.Contains(poolingObject))
                    continue;
                PoolingObjectPrefabs.Add(poolingObject);
            }
        }

        public static void AddOtherNetworkObjects(params LiteNetLibIdentity[] networkObjects)
        {
            AddOtherNetworkObjects((IEnumerable<LiteNetLibIdentity>)networkObjects);
        }

        public static void AddOtherNetworkObjects(IEnumerable<LiteNetLibIdentity> networkObjects)
        {
            if (networkObjects == null)
                return;
            foreach (LiteNetLibIdentity networkObject in networkObjects)
            {
                if (networkObject == null || OtherNetworkObjectPrefabs.ContainsKey(networkObject.HashAssetId))
                    continue;
                OtherNetworkObjectPrefabs.Add(networkObject.HashAssetId, networkObject);
            }
        }

        public static void AddPoolingWeaponLaunchEffects(IEnumerable<EquipmentModel> equipmentModels)
        {
            if (equipmentModels == null)
                return;
            List<GameObject> modelObjects = new List<GameObject>();
            foreach (EquipmentModel equipmentModel in equipmentModels)
            {
                if (equipmentModel.meshPrefab == null)
                    continue;
                modelObjects.Add(equipmentModel.meshPrefab);
            }
            AddPoolingObjects(modelObjects.GetComponents<BaseEquipmentEntity>());
        }

        public static void AddPoolingObjects(IEnumerable<IPoolDescriptorCollection> collections)
        {
            if (collections == null)
                return;
            foreach (IPoolDescriptorCollection collection in collections)
            {
                AddPoolingObjects(collection.PoolDescriptors);
            }
        }

        private static void AddManyGameData<T>(Dictionary<int, T> dict, IEnumerable<T> list)
            where T : IGameData
        {
            if (list == null)
                return;
            foreach (T entry in list)
            {
                AddGameData(dict, entry);
            }
        }

        private static bool AddGameData<T>(Dictionary<int, T> dict, T data)
            where T : IGameData
        {
            if ((data as Object) == null)
                return false;
            if (!dict.ContainsKey(data.DataId))
            {
                data.Validate();
                dict[data.DataId] = data;
                data.PrepareRelatesData();
            }
            return true;
        }

        private static void AddManyGameEntity<T>(Dictionary<int, T> dict, IEnumerable<T> list)
            where T : IGameEntity
        {
            if (list == null)
                return;
            foreach (T entry in list)
            {
                AddGameEntity(dict, entry);
            }
        }

        private static bool AddGameEntity<T>(Dictionary<int, T> dict, T entity)
            where T : IGameEntity
        {
            if ((entity as Object) == null)
                return false;
            if (!entity.Identity.IsSceneObject && !dict.ContainsKey(entity.Identity.HashAssetId))
            {
                dict[entity.Identity.HashAssetId] = entity;
                entity.PrepareRelatesData();
                // Assign game entity model's ID for caching
                GameEntityModel tempModel = entity.GetTransform().root.gameObject.GetComponent<GameEntityModel>();
                if (tempModel != null)
                    tempModel.AssignId();
                GameEntityModel[] tempModels = entity.GetTransform().root.gameObject.GetComponentsInChildren<GameEntityModel>(true);
                for (int i = 0; i < tempModels.Length; ++i)
                {
                    tempModel = tempModels[i];
                    if (tempModel != null)
                        tempModel.AssignId();
                }
            }
            else if (entity.Identity.IsSceneObject)
            {
                entity.PrepareRelatesData();
            }
            return true;
        }
    }
}
