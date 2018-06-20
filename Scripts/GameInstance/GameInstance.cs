using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameInstance : MonoBehaviour
{
    public static GameInstance Singleton { get; protected set; }
    [SerializeField]
    private BaseGameplayRule gameplayRule;
    [SerializeField]
    private NetworkSetting networkSetting;
    [Header("Gameplay Objects")]
    public PlayerCharacterEntity playerCharacterEntityPrefab;
    public MonsterCharacterEntity monsterCharacterEntityPrefab;
    public ItemDropEntity itemDropEntityPrefab;
    public UISceneGameplay uiSceneGameplayPrefab;
    public UISceneGameplay uiSceneGameplayMobilePrefab;
    public ServerCharacter serverCharacterPrefab;
    [Header("Gameplay Database")]
    [Tooltip("Default weapon item, will be used when character not equip any weapon")]
    public Item defaultWeaponItem;
    [Tooltip("Default hit effect, will be used when attacks to enemies")]
    public GameEffectCollection defaultHitEffects;
    public int[] expTree;
    [Header("Gameplay Configs")]
    public UnityTag playerTag;
    public UnityTag monsterTag;
    public UnityTag npcTag;
    public UnityTag itemDropTag;
    public UnityLayer characterLayer;
    public UnityLayer itemDropLayer;
    public UnityLayer[] nonTargetingLayers;
    public float itemAppearDuration = 60f;
    public float pickUpItemDistance = 1f;
    public float dropDistance = 1f;
    public float conversationDistance = 1f;
    [Header("Game Effects")]
    public GameEffect levelUpEffect;
    [Header("New Character")]
    public int startGold = 0;
    public ItemAmount[] startItems;
    [Header("Scene")]
    public UnityScene homeScene;
    public UnityScene startScene;
    public Vector3 startPosition;
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
    public static readonly Dictionary<int, BuildingEntity> BuildingEntities = new Dictionary<int, BuildingEntity>();
    public static readonly Dictionary<int, ActionAnimation> ActionAnimations = new Dictionary<int, ActionAnimation>();
    public static readonly Dictionary<int, GameEffectCollection> GameEffectCollections = new Dictionary<int, GameEffectCollection>();
    
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
                var damageAmountMinMax = new IncrementalMinMaxFloat();
                damageAmountMinMax.baseAmount = new MinMaxFloat() { min = 1, max = 1 };
                damageAmountMinMax.amountIncreaseEachLevel = new MinMaxFloat() { min = 0, max = 0 };
                var damageAmount = new DamageIncremental()
                {
                    amount = damageAmountMinMax,
                };
                defaultWeaponItem.damageAmount = damageAmount;
            }
            return defaultWeaponItem;
        }
    }

    protected virtual void Awake()
    {
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
        BuildingEntities.Clear();
        ActionAnimations.Clear();
        GameEffectCollections.Clear();

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

        var weaponHitEffects = new List<GameEffectCollection>();
        foreach (var damageElement in damageElements)
        {
            if (damageElement.hitEffects != null)
                weaponHitEffects.Add(damageElement.hitEffects);
        }
        AddGameEffectCollections(GameEffectCollectionType.WeaponHit, weaponHitEffects);
    }

    private void Start()
    {
        if (!doNotLoadHomeSceneOnStart)
            UISceneLoading.Singleton.LoadScene(homeScene);
    }

    public List<string> GetGameScenes()
    {
        var scenes = new List<string>();
        if (startScene != null &&
            !string.IsNullOrEmpty(startScene.SceneName))
            scenes.Add(startScene.SceneName);

        foreach (var scene in otherScenes)
        {
            if (scene != null &&
                !string.IsNullOrEmpty(scene.SceneName) &&
                !scenes.Contains(scene.SceneName))
                scenes.Add(scene.SceneName);
        }

        return scenes;
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
        var buildingEntities = new List<BuildingEntity>();
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
                if (item.buildingEntity != null)
                    buildingEntities.Add(item.buildingEntity);
            }
        }
        AddDamageEntities(damageEntities);
        AddBuildingEntities(buildingEntities);
    }

    public static void AddCharacters(IEnumerable<BaseCharacter> characters)
    {
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
            }
        }
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

    public static void AddBuildingEntities(IEnumerable<BuildingEntity> buildingEntities)
    {
        if (buildingEntities == null)
            return;
        foreach (var buildingEntity in buildingEntities)
        {
            if (buildingEntity == null || BuildingEntities.ContainsKey(buildingEntity.DataId))
                continue;
            BuildingEntities[buildingEntity.DataId] = buildingEntity;
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
}
