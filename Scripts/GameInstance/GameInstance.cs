using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameInstance : MonoBehaviour
{
    public static GameInstance Singleton { get; protected set; }
    public BaseGameInstanceExtra extra;
    [Header("Gameplay Objects")]
    public PlayerCharacterEntity playerCharacterEntityPrefab;
    public MonsterCharacterEntity monsterCharacterEntityPrefab;
    public ItemDropEntity itemDropEntityPrefab;
    public FollowCameraControls minimapCameraPrefab;
    public FollowCameraControls gameplayCameraPrefab;
    public GameObject targetObject;
    public UISceneGameplay uiSceneGameplayPrefab;
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
    public int increaseStatPointEachLevel = 5;
    public int increaseSkillPointEachLevel = 1;
    public float moveSpeedMultiplier = 5f;
    public int startGold = 0;
    public ItemAmountPair[] startItems;
    public float itemDisappearDuration = 60f;
    public float pickUpItemDistance = 1f;
    public float dropDistance = 1f;
    public float conversationDistance = 1f;
    [Header("Scene")]
    public UnityScene homeScene;
    public UnityScene startScene;
    public Vector3 startPosition;
    [Header("Player Configs")]
    public int minCharacterNameLength = 2;
    public int maxCharacterNameLength = 16;
    public static readonly Dictionary<string, Attribute> Attributes = new Dictionary<string, Attribute>();
    public static readonly Dictionary<string, Item> Items = new Dictionary<string, Item>();
    public static readonly Dictionary<string, BaseCharacterDatabase> AllCharacterDatabases = new Dictionary<string, BaseCharacterDatabase>();
    public static readonly Dictionary<string, PlayerCharacterDatabase> PlayerCharacterDatabases = new Dictionary<string, PlayerCharacterDatabase>();
    public static readonly Dictionary<string, MonsterCharacterDatabase> MonsterCharacterDatabases = new Dictionary<string, MonsterCharacterDatabase>();
    public static readonly Dictionary<string, BaseDamageEntity> DamageEntities = new Dictionary<string, BaseDamageEntity>();
    public static readonly Dictionary<string, Skill> Skills = new Dictionary<string, Skill>();
    public static readonly Dictionary<int, ActionAnimation> ActionAnimations = new Dictionary<int, ActionAnimation>();
    public static readonly Dictionary<int, GameEffectCollection> GameEffectCollections = new Dictionary<int, GameEffectCollection>();

    private BaseGameplayRule cacheGameplayRule;
    public BaseGameplayRule GameplayRule
    {
        get
        {
            if (cacheGameplayRule == null)
                cacheGameplayRule = GetComponent<BaseGameplayRule>();
            if (cacheGameplayRule == null)
                cacheGameplayRule = gameObject.AddComponent<SimpleGameplayRule>();
            return cacheGameplayRule;
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
                var sampleDamageAttributeAmount = new IncrementalMinMaxFloat();
                sampleDamageAttributeAmount.baseAmount = new MinMaxFloat() { min = 1, max = 1 };
                sampleDamageAttributeAmount.amountIncreaseEachLevel = new MinMaxFloat() { min = 0, max = 0 };
                var sampleDamageAttribute = new DamageIncremental()
                {
                    amount = sampleDamageAttributeAmount,
                };
                defaultWeaponItem.damageAttribute = sampleDamageAttribute;
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
        if (gameplayCameraPrefab == null)
        {
            Debug.LogError("You must set gameplay camera prefab");
            return;
        }
        if (uiSceneGameplayPrefab == null)
        {
            Debug.LogError("You must set ui scene gameplay prefab");
            return;
        }
        
        Attributes.Clear();
        Items.Clear();
        Skills.Clear();
        AllCharacterDatabases.Clear();
        PlayerCharacterDatabases.Clear();
        MonsterCharacterDatabases.Clear();
        DamageEntities.Clear();
        ActionAnimations.Clear();
        GameEffectCollections.Clear();

        var gameDataList = Resources.LoadAll<BaseGameData>("");
        var attributes = new List<Attribute>();
        var damageElements = new List<DamageElement>();
        var items = new List<Item>();
        var skills = new List<Skill>();
        var playerCharacterDatabases = new List<BaseCharacterDatabase>();
        var monsterCharacterDatabases = new List<BaseCharacterDatabase>();
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
            if (gameData is PlayerCharacterDatabase)
                playerCharacterDatabases.Add(gameData as PlayerCharacterDatabase);
            if (gameData is MonsterCharacterDatabase)
                monsterCharacterDatabases.Add(gameData as MonsterCharacterDatabase);
        }
        items.Add(DefaultWeaponItem);
        damageElements.Add(DefaultDamageElement);

        AddAttributes(attributes);
        AddItems(items);
        AddSkills(skills);
        AddCharacterDatabases(playerCharacterDatabases);
        AddCharacterDatabases(monsterCharacterDatabases);

        var weaponHitEffects = new List<GameEffectCollection>();
        foreach (var damageElement in damageElements)
        {
            if (damageElement.hitEffects != null)
                weaponHitEffects.Add(damageElement.hitEffects);
        }
        AddGameEffectCollections(GameEffectCollectionType.WeaponHit, weaponHitEffects);
    }

    public static void AddAttributes(IEnumerable<Attribute> attributes)
    {
        foreach (var attribute in attributes)
        {
            if (attribute == null || Attributes.ContainsKey(attribute.Id))
                continue;
            Attributes[attribute.Id] = attribute;
        }
    }

    public static void AddItems(IEnumerable<Item> items)
    {
        var damageEntities = new List<BaseDamageEntity>();
        foreach (var item in items)
        {
            if (item == null || Items.ContainsKey(item.Id))
                continue;
            Items[item.Id] = item;
            if (item.IsWeapon())
            {
                var weaponType = item.WeaponType;
                // Initialize animation index
                AddActionAnimations(ActionAnimationType.WeaponAttack, weaponType.rightHandAttackAnimations);
                AddActionAnimations(ActionAnimationType.WeaponAttack, weaponType.leftHandAttackAnimations);
                // Add damage entities
                var missileDamageEntity = weaponType.damageInfo.missileDamageEntity;
                if (missileDamageEntity != null)
                    damageEntities.Add(missileDamageEntity);
            }
        }
        AddDamageEntities(damageEntities);
    }

    public static void AddCharacterDatabases(IEnumerable<BaseCharacterDatabase> characterDatabases)
    {
        foreach (var characterDatabase in characterDatabases)
        {
            if (characterDatabase == null || AllCharacterDatabases.ContainsKey(characterDatabase.Id))
                continue;
            AllCharacterDatabases[characterDatabase.Id] = characterDatabase;
            if (characterDatabase is PlayerCharacterDatabase)
            {
                var playerCharacterDatabase = characterDatabase as PlayerCharacterDatabase;
                PlayerCharacterDatabases[characterDatabase.Id] = playerCharacterDatabase;
            }
            else if (characterDatabase is MonsterCharacterDatabase)
            {
                var monsterCharacterDatabase = characterDatabase as MonsterCharacterDatabase;
                MonsterCharacterDatabases[characterDatabase.Id] = monsterCharacterDatabase;
                AddActionAnimations(ActionAnimationType.MonsterAttack, monsterCharacterDatabase.attackAnimations);
            }
        }
    }

    public static void AddSkills(IEnumerable<Skill> skills)
    {
        var castAnimations = new List<ActionAnimation>();
        var skillHitEffects = new List<GameEffectCollection>();
        var damageEntities = new List<BaseDamageEntity>();
        foreach (var skill in skills)
        {
            if (skill == null || Skills.ContainsKey(skill.Id))
                continue;
            Skills[skill.Id] = skill;
            castAnimations.Add(skill.castAnimation);
            skillHitEffects.Add(skill.hitEffects);
            var missileDamageEntity = skill.damageInfo.missileDamageEntity;
            if (missileDamageEntity != null)
                damageEntities.Add(missileDamageEntity);
        }
        AddActionAnimations(ActionAnimationType.SkillCast, castAnimations);
        AddGameEffectCollections(GameEffectCollectionType.SkillHit, skillHitEffects);
        AddDamageEntities(damageEntities);
    }

    public static void AddDamageEntities(IEnumerable<BaseDamageEntity> damageEntities)
    {
        foreach (var damageEntity in damageEntities)
        {
            if (damageEntity == null || DamageEntities.ContainsKey(damageEntity.Identity.AssetId))
                continue;
            DamageEntities[damageEntity.Identity.AssetId] = damageEntity;
        }
    }

    public static void AddActionAnimations(ActionAnimationType type, IEnumerable<ActionAnimation> actionAnimations)
    {
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
        foreach (var gameEffectCollection in gameEffectCollections)
        {
            if (!gameEffectCollection.Initialize(type))
                continue;
            if (gameEffectCollection == null || GameEffectCollections.ContainsKey(gameEffectCollection.Id))
                continue;
            GameEffectCollections[gameEffectCollection.Id] = gameEffectCollection;
        }
    }

    public T GetExtra<T>() where T : BaseGameInstanceExtra
    {
        return extra as T;
    }
}
