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
    public BaseGameplayRule gameplayRule;
    [Tooltip("Default weapon item, will be used when character not equip any weapon")]
    public Item defaultWeaponItem;
    public Item[] items;
    public BaseCharacterDatabase[] characterDatabases;
    public Attribute[] attributes;
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
    public int startGold = 0;
    public ItemAmountPair[] startItems;
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
    public static readonly Dictionary<string, DamageEntity> DamageEntities = new Dictionary<string, DamageEntity>();
    public static readonly Dictionary<string, Skill> Skills = new Dictionary<string, Skill>();
    public static readonly Dictionary<int, ActionAnimation> ActionAnimations = new Dictionary<int, ActionAnimation>();

    public BaseGameplayRule GameplayRule
    {
        get
        {
            if (gameplayRule == null)
                gameplayRule = ScriptableObject.CreateInstance<SimpleGameplayRule>();
            return gameplayRule;
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
                var sampleDamageAttribute = new DamageAttribute()
                {
                    baseDamageAmount = new DamageAmount() { minDamage = 1, maxDamage = 1 },
                    damageAmountIncreaseEachLevel = new DamageAmount(),
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
        AllCharacterDatabases.Clear();
        PlayerCharacterDatabases.Clear();
        MonsterCharacterDatabases.Clear();
        DamageEntities.Clear();
        Items.Clear();
        Skills.Clear();
        ActionAnimations.Clear();

        AddAttributes(attributes);
        AddItems(items);
        AddCharacterDatabases(characterDatabases);
        AddItems(new Item[] { DefaultWeaponItem });

        var startItemsList = new List<Item>();
        foreach (var startItem in startItems)
        {
            if (startItem.item == null)
                continue;
            startItemsList.Add(startItem.item);
        }
        AddItems(startItemsList);
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
                    AddDamageEntities(new DamageEntity[] { missileDamageEntity });
            }
        }
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
                AddSkills(playerCharacterDatabase.skills);
                AddItems(new Item[] { playerCharacterDatabase.rightHandEquipItem, playerCharacterDatabase.leftHandEquipItem });
                AddItems(playerCharacterDatabase.armorItems);
            }
            else if (characterDatabase is MonsterCharacterDatabase)
            {
                var monsterCharacterDatabase = characterDatabase as MonsterCharacterDatabase;
                MonsterCharacterDatabases[characterDatabase.Id] = monsterCharacterDatabase;
                AddActionAnimations(ActionAnimationType.MonsterAttack, monsterCharacterDatabase.attackAnimations);
            }
        }
    }

    public static void AddDamageEntities(IEnumerable<DamageEntity> damageEntities)
    {
        foreach (var damageEntity in damageEntities)
        {
            if (damageEntity == null || DamageEntities.ContainsKey(damageEntity.Identity.AssetId))
                continue;
            DamageEntities[damageEntity.Identity.AssetId] = damageEntity;
        }
    }

    public static void AddSkills(IEnumerable<Skill> skills)
    {
        foreach (var skill in skills)
        {
            if (skill == null || Skills.ContainsKey(skill.Id))
                continue;
            Skills[skill.Id] = skill;
            // Initialize animation index
            AddActionAnimations(ActionAnimationType.SkillCast, new ActionAnimation[] { skill.castAnimation });
            // Add damage entities
            var missileDamageEntity = skill.damageInfo.missileDamageEntity;
            if (missileDamageEntity != null)
                AddDamageEntities(new DamageEntity[] { missileDamageEntity });
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

    public T GetExtra<T>() where T : BaseGameInstanceExtra
    {
        return extra as T;
    }
}
