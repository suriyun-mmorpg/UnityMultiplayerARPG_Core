using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class GameInstance : MonoBehaviour
    {
        public static GameInstance Singleton { get; protected set; }
        [SerializeField]
        private BaseGameplayRule gameplayRule;
        [SerializeField]
        private NetworkSetting networkSetting;
        [Header("Gameplay Objects")]
        public BasePlayerCharacterEntity playerCharacterEntityPrefab;
        public BaseMonsterCharacterEntity monsterCharacterEntityPrefab;
        public ItemDropEntity itemDropEntityPrefab;
        public BuildingEntity buildingEntityPrefab;
        public WarpPortalEntity warpPortalEntityPrefab;
        public NpcEntity npcEntityPrefab;
        public UISceneGameplay uiSceneGameplayPrefab;
        public UISceneGameplay uiSceneGameplayMobilePrefab;
        public ServerCharacter serverCharacterPrefab;
        [Header("Gameplay Database")]
        [Tooltip("Default weapon item, will be used when character not equip any weapon")]
        public Item defaultWeaponItem;
        [Tooltip("Default hit effect, will be used when attacks to enemies")]
        public GameEffectCollection defaultHitEffects;
        public int[] expTree;
        [Tooltip("You can add warp portals here or may add warp portals in the scene directly, So you can leave this empty")]
        public WarpPortalDatabase warpPortalDatabase;
        public NpcDatabase npcDatabase;
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
        public float itemAppearDuration = 60f;
        public float pickUpItemDistance = 1f;
        public float dropDistance = 1f;
        public float conversationDistance = 1f;
        public float buildDistance = 10f;
        [Header("Game Effects")]
        public GameEffect levelUpEffect;
        [Header("New Character")]
        public int startGold = 0;
        public ItemAmount[] startItems;
        [Header("Scene/Maps")]
        public UnityScene homeScene;
        [System.Obsolete("`Start Scene` is deprecated and will be removed next version, use `Map Infos` instead.")]
        public UnityScene startScene;
        [System.Obsolete("`Start Position` is deprecated and will be removed next version, use `Map Infos` instead.")]
        public Vector3 startPosition;
        [System.Obsolete("`Other Scenes` is deprecated and will be removed next version, use `Map Infos` instead.")]
        public UnityScene[] otherScenes;
        [Header("Player Configs")]
        public int minCharacterNameLength = 2;
        public int maxCharacterNameLength = 16;
        [Header("Other Settings")]
        public bool doNotLoadHomeSceneOnStart;
        [Header("Playing In Editor")]
        public bool useMobileInEditor;
        public static readonly Dictionary<int, Attribute> Attributes = new Dictionary<int, Attribute>();
        public static readonly Dictionary<int, Item> Items = new Dictionary<int, Item>();
        public static readonly Dictionary<int, BaseCharacter> AllCharacters = new Dictionary<int, BaseCharacter>();
        public static readonly Dictionary<int, PlayerCharacter> PlayerCharacters = new Dictionary<int, PlayerCharacter>();
        public static readonly Dictionary<int, MonsterCharacter> MonsterCharacters = new Dictionary<int, MonsterCharacter>();
        public static readonly Dictionary<int, Skill> Skills = new Dictionary<int, Skill>();
        public static readonly Dictionary<int, NpcDialog> NpcDialogs = new Dictionary<int, NpcDialog>();
        public static readonly Dictionary<int, Quest> Quests = new Dictionary<int, Quest>();
        public static readonly Dictionary<int, BaseDamageEntity> DamageEntities = new Dictionary<int, BaseDamageEntity>();
        public static readonly Dictionary<int, BuildingObject> BuildingObjects = new Dictionary<int, BuildingObject>();
        public static readonly Dictionary<int, ActionAnimation> ActionAnimations = new Dictionary<int, ActionAnimation>();
        public static readonly Dictionary<int, GameEffectCollection> GameEffectCollections = new Dictionary<int, GameEffectCollection>();
        public static readonly Dictionary<int, CharacterModel> CharacterModels = new Dictionary<int, CharacterModel>();
        public static readonly Dictionary<string, WarpPortals> MapWarpPortals = new Dictionary<string, WarpPortals>();
        public static readonly Dictionary<string, Npcs> MapNpcs = new Dictionary<string, Npcs>();
        public static readonly Dictionary<string, MapInfo> MapInfos = new Dictionary<string, MapInfo>();

        #region Cache Data
        public BaseGameplayRule GameplayRule
        {
            get
            {
                if (gameplayRule == null)
                    gameplayRule = ScriptableObject.CreateInstance<SimpleGameplayRule>();
                return gameplayRule;
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
                    cacheDefaultDamageElement.hitEffects = defaultHitEffects;
                }
                return cacheDefaultDamageElement;
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
                    var damageAmountMinMax = new IncrementalMinMaxFloat();
                    damageAmountMinMax.baseAmount = new MinMaxFloat() { min = 1, max = 1 };
                    damageAmountMinMax.amountIncreaseEachLevel = new MinMaxFloat() { min = 0, max = 0 };
                    var damageAmount = new DamageIncremental()
                    {
                        amount = damageAmountMinMax,
                    };
                    // Default harvest damage amount
                    var harvestDamageAmount = new IncrementalMinMaxFloat();
                    harvestDamageAmount.baseAmount = new MinMaxFloat() { min = 1, max = 1 };
                    harvestDamageAmount.amountIncreaseEachLevel = new MinMaxFloat() { min = 0, max = 0 };
                    // Set damage amount
                    defaultWeaponItem.damageAmount = damageAmount;
                    defaultWeaponItem.harvestDamageAmount = harvestDamageAmount;
                }
                return defaultWeaponItem;
            }
        }
        #endregion

        protected virtual void Awake()
        {
            Application.targetFrameRate = 120;
            Application.runInBackground = true;
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Singleton = this;

            if (playerCharacterEntityPrefab == null)
            {
                Debug.LogError("You must set player character entity prefab");
                return;
            }
            if (monsterCharacterEntityPrefab == null)
            {
                Debug.LogError("You must set monster character entity prefab");
                return;
            }
            if (itemDropEntityPrefab == null)
            {
                Debug.LogError("You must set item drop entity prefab");
                return;
            }
            if (buildingEntityPrefab == null)
            {
                Debug.LogError("You must set building entity prefab");
                return;
            }

            InputManager.useMobileInputOnNonMobile = useMobileInEditor;

            // Load game data
            Attributes.Clear();
            Items.Clear();
            Skills.Clear();
            NpcDialogs.Clear();
            Quests.Clear();
            AllCharacters.Clear();
            PlayerCharacters.Clear();
            MonsterCharacters.Clear();
            DamageEntities.Clear();
            BuildingObjects.Clear();
            ActionAnimations.Clear();
            GameEffectCollections.Clear();
            CharacterModels.Clear();
            MapWarpPortals.Clear();
            MapInfos.Clear();

            // Use Resources Load Async ?
            var gameDataList = Resources.LoadAll<BaseGameData>("");

            var attributes = new List<Attribute>();
            var damageElements = new List<DamageElement>();
            var items = new List<Item>();
            var skills = new List<Skill>();
            var npcDialogs = new List<NpcDialog>();
            var quests = new List<Quest>();
            var playerCharacters = new List<BaseCharacter>();
            var monsterCharacters = new List<BaseCharacter>();
            var mapInfos = new List<MapInfo>();
            // Backward compatibility
            if (startScene.IsSet())
            {
                Debug.LogWarning("[GameInstance] `Start Scene`/`Start Position` is deprecated and will be removed next version, use `Map Infos` instead.");
                var newMapInfo = ScriptableObject.CreateInstance<MapInfo>();
                newMapInfo.scene.SceneName = startScene.SceneName;
                newMapInfo.startPosition = startPosition;
                mapInfos.Add(newMapInfo);
            }
            if (otherScenes.Length > 0)
            {
                Debug.LogWarning("[GameInstance] `Other Scenes` is deprecated and will be removed next version, use `Map Infos` instead.");
                foreach (var otherScene in otherScenes)
                {
                    if (!otherScene.IsSet())
                        continue;
                    var newMapInfo = ScriptableObject.CreateInstance<MapInfo>();
                    newMapInfo.scene.SceneName = otherScene.SceneName;
                    mapInfos.Add(newMapInfo);
                }
            }
            // Filtering game data
            foreach (var gameData in gameDataList)
            {
                if (gameData is Attribute)
                    attributes.Add(gameData as Attribute);
                if (gameData is DamageElement)
                    damageElements.Add(gameData as DamageElement);
                if (gameData is Item)
                    items.Add(gameData as Item);
                if (gameData is Skill)
                    skills.Add(gameData as Skill);
                if (gameData is NpcDialog)
                    npcDialogs.Add(gameData as NpcDialog);
                if (gameData is Quest)
                    quests.Add(gameData as Quest);
                if (gameData is PlayerCharacter)
                    playerCharacters.Add(gameData as PlayerCharacter);
                if (gameData is MonsterCharacter)
                    monsterCharacters.Add(gameData as MonsterCharacter);
                if (gameData is MapInfo)
                    mapInfos.Add(gameData as MapInfo);
            }
            items.Add(DefaultWeaponItem);
            damageElements.Add(DefaultDamageElement);

            AddAttributes(attributes);
            AddItems(items);
            AddSkills(skills);
            AddNpcDialogs(npcDialogs);
            AddQuests(quests);
            AddCharacters(playerCharacters);
            AddCharacters(monsterCharacters);
            AddMapInfos(mapInfos);

            var weaponHitEffects = new List<GameEffectCollection>();
            foreach (var damageElement in damageElements)
            {
                if (damageElement.hitEffects != null)
                    weaponHitEffects.Add(damageElement.hitEffects);
            }
            AddGameEffectCollections(GameEffectCollectionType.WeaponHit, weaponHitEffects);

            if (warpPortalDatabase != null)
                AddMapWarpPortals(warpPortalDatabase.maps);

            if (npcDatabase != null)
                AddMapNpcs(npcDatabase.maps);
        }

        private void Start()
        {
            if (!doNotLoadHomeSceneOnStart)
                UISceneLoading.Singleton.LoadScene(homeScene);
        }

        public List<string> GetGameScenes()
        {
            var scenes = new List<string>();
            foreach (var sceneName in MapInfos.Keys)
            {
                if (sceneName != null &&
                    !string.IsNullOrEmpty(sceneName) &&
                    !scenes.Contains(sceneName))
                    scenes.Add(sceneName);
            }

            return scenes;
        }
        
        public int GetTargetLayerMask()
        {
            var layerMask = 0;
            if (nonTargetingLayers.Length > 0)
            {
                foreach (var nonTargetingLayer in nonTargetingLayers)
                {
                    layerMask = layerMask | ~(nonTargetingLayer.Mask);
                }
            }
            else
                layerMask = -1;
            return layerMask;
        }


        public int GetDamageableLayerMask()
        {
            var layerMask = 0;
            layerMask = layerMask | ~characterLayer.Mask;
            layerMask = layerMask | ~buildingLayer.Mask;
            layerMask = layerMask | ~harvestableLayer.Mask;
            return layerMask;
        }

        public int GetBuildLayerMask()
        {
            var layerMask = -1;
            layerMask = layerMask | ~(characterLayer.Mask);
            layerMask = layerMask | ~(itemDropLayer.Mask);
            layerMask = layerMask | ~(harvestableLayer.Mask);
            return layerMask;
        }

        public int GetItemDropGroundDetectionLayerMask()
        {
            var layerMask = -1;
            layerMask = layerMask | ~(characterLayer.Mask);
            layerMask = layerMask | ~(itemDropLayer.Mask);
            return layerMask;
        }

        public int GetMonsterSpawnGroundDetectionLayerMask()
        {
            var layerMask = -1;
            layerMask = layerMask | ~(buildingLayer.Mask);
            layerMask = layerMask | ~(harvestableLayer.Mask);
            return layerMask;
        }

        public int GetHarvestableSpawnGroundDetectionLayerMask()
        {
            var layerMask = -1;
            layerMask = layerMask | ~(characterLayer.Mask);
            layerMask = layerMask | ~(itemDropLayer.Mask);
            layerMask = layerMask | ~(buildingLayer.Mask);
            layerMask = layerMask | ~(harvestableLayer.Mask);
            return layerMask;
        }

        public static void AddAttributes(IEnumerable<Attribute> attributes)
        {
            foreach (var attribute in attributes)
            {
                if (attribute == null || Attributes.ContainsKey(attribute.DataId))
                    continue;
                Attributes[attribute.DataId] = attribute;
            }
        }

        public static void AddItems(IEnumerable<Item> items)
        {
            var damageEntities = new List<BaseDamageEntity>();
            var buildingObjects = new List<BuildingObject>();
            foreach (var item in items)
            {
                if (item == null || Items.ContainsKey(item.DataId))
                    continue;
                Items[item.DataId] = item;
                if (item.IsWeapon())
                {
                    var weaponType = item.WeaponType;
                    // Initialize animation index
                    AddActionAnimations(ActionAnimationType.WeaponAttack, weaponType.rightHandAttackAnimations);
                    AddActionAnimations(ActionAnimationType.WeaponAttack, weaponType.leftHandAttackAnimations);
                    // Add damage entities
                    if (weaponType.damageInfo.missileDamageEntity != null)
                        damageEntities.Add(weaponType.damageInfo.missileDamageEntity);
                }
                if (item.IsBuilding())
                {
                    if (item.buildingObject != null)
                        buildingObjects.Add(item.buildingObject);
                }
            }
            AddDamageEntities(damageEntities);
            AddBuildingObjects(buildingObjects);
        }

        public static void AddCharacters(IEnumerable<BaseCharacter> characters)
        {
            var characterModels = new List<CharacterModel>();
            var damageEntities = new List<BaseDamageEntity>();
            foreach (var character in characters)
            {
                if (character == null || AllCharacters.ContainsKey(character.DataId))
                    continue;
                AllCharacters[character.DataId] = character;
                if (character is PlayerCharacter)
                {
                    var playerCharacter = character as PlayerCharacter;
                    PlayerCharacters[character.DataId] = playerCharacter;
                }
                else if (character is MonsterCharacter)
                {
                    var monsterCharacter = character as MonsterCharacter;
                    MonsterCharacters[character.DataId] = monsterCharacter;
                    AddActionAnimations(ActionAnimationType.MonsterAttack, monsterCharacter.attackAnimations);
                    var missileDamageEntity = monsterCharacter.damageInfo.missileDamageEntity;
                    if (missileDamageEntity != null)
                        damageEntities.Add(missileDamageEntity);
                }
                characterModels.Add(character.model);
            }
            AddCharacterModels(characterModels);
            AddDamageEntities(damageEntities);
        }

        public static void AddSkills(IEnumerable<Skill> skills)
        {
            var skillHitEffects = new List<GameEffectCollection>();
            var damageEntities = new List<BaseDamageEntity>();
            foreach (var skill in skills)
            {
                if (skill == null || Skills.ContainsKey(skill.DataId))
                    continue;
                Skills[skill.DataId] = skill;
                AddActionAnimations(ActionAnimationType.SkillCast, skill.castAnimations);
                skillHitEffects.Add(skill.hitEffects);
                var missileDamageEntity = skill.damageInfo.missileDamageEntity;
                if (missileDamageEntity != null)
                    damageEntities.Add(missileDamageEntity);
            }
            AddGameEffectCollections(GameEffectCollectionType.SkillHit, skillHitEffects);
            AddDamageEntities(damageEntities);
        }

        public static void AddNpcDialogs(IEnumerable<NpcDialog> npcDialogs)
        {
            foreach (var npcDialog in npcDialogs)
            {
                if (npcDialog == null || NpcDialogs.ContainsKey(npcDialog.DataId))
                    continue;
                NpcDialogs[npcDialog.DataId] = npcDialog;
            }
        }

        public static void AddQuests(IEnumerable<Quest> quests)
        {
            foreach (var quest in quests)
            {
                if (quest == null || Quests.ContainsKey(quest.DataId))
                    continue;
                Quests[quest.DataId] = quest;
            }
        }

        public static void AddDamageEntities(IEnumerable<BaseDamageEntity> damageEntities)
        {
            if (damageEntities == null)
                return;
            foreach (var damageEntity in damageEntities)
            {
                if (damageEntity == null || DamageEntities.ContainsKey(damageEntity.DataId))
                    continue;
                DamageEntities[damageEntity.DataId] = damageEntity;
            }
        }

        public static void AddBuildingObjects(IEnumerable<BuildingObject> buildingObjects)
        {
            if (buildingObjects == null)
                return;
            foreach (var buildingObject in buildingObjects)
            {
                if (buildingObject == null || BuildingObjects.ContainsKey(buildingObject.DataId))
                    continue;
                BuildingObjects[buildingObject.DataId] = buildingObject;
            }
        }

        public static void AddActionAnimations(ActionAnimationType type, IEnumerable<ActionAnimation> actionAnimations)
        {
            if (actionAnimations == null)
                return;
            foreach (var actionAnimation in actionAnimations)
            {
                if (!actionAnimation.Initialize(type))
                    continue;
                if (actionAnimation == null || ActionAnimations.ContainsKey(actionAnimation.Id))
                    continue;
                ActionAnimations[actionAnimation.Id] = actionAnimation;
            }
        }

        public static void AddGameEffectCollections(GameEffectCollectionType type, IEnumerable<GameEffectCollection> gameEffectCollections)
        {
            if (gameEffectCollections == null)
                return;
            foreach (var gameEffectCollection in gameEffectCollections)
            {
                if (!gameEffectCollection.Initialize(type))
                    continue;
                if (gameEffectCollection == null || GameEffectCollections.ContainsKey(gameEffectCollection.Id))
                    continue;
                GameEffectCollections[gameEffectCollection.Id] = gameEffectCollection;
            }
        }

        public static void AddCharacterModels(IEnumerable<CharacterModel> characterModels)
        {
            if (characterModels == null)
                return;
            foreach (var characterModel in characterModels)
            {
                if (characterModel == null || CharacterModels.ContainsKey(characterModel.DataId))
                    continue;
                CharacterModels[characterModel.DataId] = characterModel;
            }
        }

        public static void AddMapWarpPortals(IEnumerable<WarpPortals> mapWarpPortals)
        {
            if (mapWarpPortals == null)
                return;
            foreach (var mapWarpPortal in mapWarpPortals)
            {
                if (mapWarpPortal.map == null || string.IsNullOrEmpty(mapWarpPortal.map.SceneName))
                    continue;
                MapWarpPortals[mapWarpPortal.map.SceneName] = mapWarpPortal;
            }
        }

        public static void AddMapNpcs(IEnumerable<Npcs> mapNpcs)
        {
            if (mapNpcs == null)
                return;
            var npcDialogs = new List<NpcDialog>();
            var characterModels = new List<CharacterModel>();
            foreach (var mapNpc in mapNpcs)
            {
                if (mapNpc.map == null || string.IsNullOrEmpty(mapNpc.map.SceneName))
                    continue;
                MapNpcs[mapNpc.map.SceneName] = mapNpc;
                foreach (var npc in mapNpc.npcs)
                {
                    if (npc.startDialog != null)
                        npcDialogs.Add(npc.startDialog);
                    if (npc.model != null)
                        characterModels.Add(npc.model);
                }
            }
            AddNpcDialogs(npcDialogs);
            AddCharacterModels(characterModels);
        }

        public static void AddMapInfos(IEnumerable<MapInfo> mapInfos)
        {
            if (mapInfos == null)
                return;
            foreach (var mapInfo in mapInfos)
            {
                if (mapInfo.scene == null || string.IsNullOrEmpty(mapInfo.scene.SceneName))
                    continue;
                MapInfos[mapInfo.scene.SceneName] = mapInfo;
            }
        }
    }
}
