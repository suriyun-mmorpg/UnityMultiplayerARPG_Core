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

    public enum CurrentPositionSaveMode
    {
        UseCurrentPosition,
        UseRespawnPosition
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
        [Tooltip("Default damage element, will be used when attacks to enemies or receives damages from enemies")]
        [SerializeField]
        private DamageElement defaultDamageElement;
        [Tooltip("Default hit effect, will be used when attacks to enemies or receives damages from enemies")]
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
        [Tooltip("This is a distance that allows a player to converstion with NPC / send requests to other players")]
        public float conversationDistance = 1f;
        [Tooltip("This is a distance that allows a player to builds an building")]
        public float buildDistance = 10f;
        [Tooltip("This is a distance that other players will receives local chat")]
        public float localChatDistance = 10f;
        [Tooltip("Maximum number of equip weapon set")]
        [Range(1, 16)]
        public byte maxEquipWeaponSet = 2;

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
        [Tooltip("If this is NULL, it will use `startGold` and `startItems`")]
        public NewCharacterSetting newCharacterSetting;
        [Tooltip("Amount of gold that will be added to character when create new character")]
        public int startGold = 0;
        [Tooltip("Items that will be added to character when create new character")]
        [ArrayElementTitle("item", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
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
        public static readonly Dictionary<int, ArmorType> ArmorTypes = new Dictionary<int, ArmorType>();
        public static readonly Dictionary<int, WeaponType> WeaponTypes = new Dictionary<int, WeaponType>();
        public static readonly Dictionary<int, BaseCharacter> Characters = new Dictionary<int, BaseCharacter>();
        public static readonly Dictionary<int, PlayerCharacter> PlayerCharacters = new Dictionary<int, PlayerCharacter>();
        public static readonly Dictionary<int, MonsterCharacter> MonsterCharacters = new Dictionary<int, MonsterCharacter>();
        public static readonly Dictionary<int, BaseSkill> Skills = new Dictionary<int, BaseSkill>();
        public static readonly Dictionary<int, NpcDialog> NpcDialogs = new Dictionary<int, NpcDialog>();
        public static readonly Dictionary<int, Quest> Quests = new Dictionary<int, Quest>();
        public static readonly Dictionary<int, GuildSkill> GuildSkills = new Dictionary<int, GuildSkill>();
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

        public DamageElement DefaultDamageElement
        {
            get
            {
                if (defaultDamageElement == null)
                {
                    defaultDamageElement = ScriptableObject.CreateInstance<DamageElement>();
                    defaultDamageElement.name = GameDataConst.DEFAULT_DAMAGE_ID;
                    defaultDamageElement.title = GameDataConst.DEFAULT_DAMAGE_TITLE;
                    defaultDamageElement.hitEffects = DefaultHitEffects;
                }
                return defaultDamageElement;
            }
        }

        public GameEffectCollection DefaultHitEffects
        {
            get { return defaultHitEffects; }
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

        public bool HasNewCharacterSetting
        {
            get { return newCharacterSetting != null; }
        }

        private List<int> cacheNonTargetLayersValues;
        public List<int> NonTargetLayersValues
        {
            get
            {
                if (cacheNonTargetLayersValues == null)
                {
                    cacheNonTargetLayersValues = new List<int>();
                    foreach (UnityLayer layer in nonTargetingLayers)
                    {
                        cacheNonTargetLayersValues.Add(layer.LayerIndex);
                    }
                }
                return cacheNonTargetLayersValues;
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

            // Add required default game data
            AddItems(new Item[] { DefaultWeaponItem });
            AddWeaponTypes(new WeaponType[] { DefaultWeaponType });

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
            layerMask = layerMask | 1 << 4;  // Water
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
            layerMask = layerMask | 1 << 4;  // Water
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
            layerMask = layerMask | 1 << 4;  // Water
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
            layerMask = layerMask | 1 << 4;  // Water
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
            layerMask = layerMask | 1 << 4;  // Water
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
                attribute.Validate();
                Attributes[attribute.DataId] = attribute;
                attribute.PrepareRelatesData();
            }
        }

        public static void AddItems(IEnumerable<Item> items)
        {
            if (items == null)
                return;
            foreach (Item item in items)
            {
                if (item == null || Items.ContainsKey(item.DataId))
                    continue;
                item.Validate();
                Items[item.DataId] = item;
                item.PrepareRelatesData();
                // Validate equipment set
                if (item.equipmentSet != null)
                {
                    item.equipmentSet.Validate();
                    item.equipmentSet.PrepareRelatesData();
                }
            }
        }

        public static void AddCharacters(IEnumerable<BaseCharacter> characters)
        {
            if (characters == null)
                return;
            foreach (BaseCharacter character in characters)
            {
                if (character == null || Characters.ContainsKey(character.DataId))
                    continue;
                character.Validate();
                Characters[character.DataId] = character;
                character.PrepareRelatesData();
                if (character is PlayerCharacter)
                {
                    PlayerCharacter playerCharacter = character as PlayerCharacter;
                    PlayerCharacters[character.DataId] = playerCharacter;
                }
                else if (character is MonsterCharacter)
                {
                    MonsterCharacter monsterCharacter = character as MonsterCharacter;
                    MonsterCharacters[character.DataId] = monsterCharacter;
                }
            }
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
                    characters.Add(monsterCharacterEntity.MonsterDatabase);
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
                if (skill == null || Skills.ContainsKey(skill.DataId))
                    continue;
                skill.Validate();
                Skills[skill.DataId] = skill;
                skill.PrepareRelatesData();
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
                if (quest == null || Quests.ContainsKey(quest.DataId))
                    continue;
                quest.Validate();
                Quests[quest.DataId] = quest;
                quest.PrepareRelatesData();
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
                guildSkill.Validate();
                GuildSkills[guildSkill.DataId] = guildSkill;
                guildSkill.PrepareRelatesData();
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

        public static void AddArmorTypes(IEnumerable<ArmorType> armorTypes)
        {
            if (armorTypes == null)
                return;
            foreach (ArmorType armorType in armorTypes)
            {
                if (armorType == null || ArmorTypes.ContainsKey(armorType.DataId))
                    continue;
                armorType.Validate();
                ArmorTypes[armorType.DataId] = armorType;
                armorType.PrepareRelatesData();
            }
        }

        public static void AddWeaponTypes(IEnumerable<WeaponType> weaponTypes)
        {
            if (weaponTypes == null)
                return;
            foreach (WeaponType weaponType in weaponTypes)
            {
                if (weaponType == null || WeaponTypes.ContainsKey(weaponType.DataId))
                    continue;
                weaponType.Validate();
                WeaponTypes[weaponType.DataId] = weaponType;
                weaponType.PrepareRelatesData();
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

        public static void AddMapInfos(IEnumerable<MapInfo> mapInfos)
        {
            if (mapInfos == null)
                return;
            foreach (MapInfo mapInfo in mapInfos)
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
                if (faction == null || Factions.ContainsKey(faction.DataId))
                    continue;
                faction.Validate();
                Factions[faction.DataId] = faction;
                faction.PrepareRelatesData();
            }
        }
    }
}
