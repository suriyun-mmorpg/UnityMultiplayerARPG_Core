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
    public ItemDropEntity itemDropEntityPrefab;
    public FollowCameraControls minimapCameraPrefab;
    public FollowCameraControls gameplayCameraPrefab;
    public GameObject targetObject;
    public UISceneGameplay uiSceneGameplayPrefab;
    [Header("Gameplay Database")]
    public BaseGameplayRule gameplayRule;
    [Tooltip("Default weapon item, will be used when character not equip any weapon")]
    public WeaponItem defaultWeaponItem;
    public CharacterPrototype[] characterPrototypes;
    public Item[] items;
    public int[] expTree;
    [Header("Gameplay Configs")]
    public UnityLayer playerCharacterLayer;
    public UnityLayer monsterCharacterLayer;
    public UnityLayer npcCharacterLayer;
    public UnityLayer itemDropLayer;
    public int increaseStatPointEachLevel = 5;
    public int increaseSkillPointEachLevel = 1;
    public int startGold = 0;
    public ItemAmountPair[] startItems;
    public float pickUpItemDistance = 2f;
    public float dropDistance = 2f;
    [Header("Scene")]
    public UnityScene homeScene;
    public UnityScene startScene;
    public Vector3 startPosition;
    [Header("Player Configs")]
    public int minCharacterNameLength = 2;
    public int maxCharacterNameLength = 16;
    public static readonly Dictionary<string, Attribute> Attributes = new Dictionary<string, Attribute>();
    public static readonly Dictionary<string, CharacterClass> CharacterClasses = new Dictionary<string, CharacterClass>();
    public static readonly Dictionary<string, CharacterModel> CharacterModels = new Dictionary<string, CharacterModel>();
    public static readonly Dictionary<string, CharacterPrototype> CharacterPrototypes = new Dictionary<string, CharacterPrototype>();
    public static readonly Dictionary<string, DamageElement> DamageElements = new Dictionary<string, DamageElement>();
    public static readonly Dictionary<string, DamageEntity> DamageEntities = new Dictionary<string, DamageEntity>();
    public static readonly Dictionary<string, Item> Items = new Dictionary<string, Item>();
    public static readonly Dictionary<string, Skill> Skills = new Dictionary<string, Skill>();

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
                var sampleAttackAnimation = new ActionAnimation()
                {
                    actionId = 0,
                    triggerDuration = 0.4f,
                    totalDuration = 0.8f,
                };
                cacheDefaultWeaponType.rightHandAttackAnimations = new ActionAnimation[1]
                {
                    sampleAttackAnimation,
                };
                cacheDefaultWeaponType.leftHandAttackAnimations = new ActionAnimation[1]
                {
                    sampleAttackAnimation,
                };
                cacheDefaultWeaponType.damage = new Damage();
            }
            return cacheDefaultWeaponType;
        }
    }

    public WeaponItem DefaultWeaponItem
    {
        get
        {
            if (defaultWeaponItem == null)
            {
                defaultWeaponItem = ScriptableObject.CreateInstance<WeaponItem>();
                defaultWeaponItem.name = GameDataConst.DEFAULT_WEAPON_ID;
                defaultWeaponItem.title = GameDataConst.DEFAULT_WEAPON_TITLE;
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
            Debug.LogError("You must set character entity prefab");
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
        CharacterClasses.Clear();
        CharacterModels.Clear();
        CharacterPrototypes.Clear();
        DamageElements.Clear();
        DamageEntities.Clear();
        Items.Clear();
        Skills.Clear();

        AddCharacterPrototypes(characterPrototypes);
        AddItems(new Item[] { DefaultWeaponItem });
        AddItems(items);

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

    public static void AddCharacterClasses(IEnumerable<CharacterClass> characterClasses)
    {
        foreach (var characterClass in characterClasses)
        {
            if (characterClass == null || CharacterClasses.ContainsKey(characterClass.Id))
                continue;
            CharacterClasses[characterClass.Id] = characterClass;
            var attributes = new List<Attribute>();
            foreach (var baseAttribute in characterClass.baseAttributes)
            {
                if (baseAttribute.attribute == null || Attributes.ContainsKey(baseAttribute.attribute.Id))
                    continue;
                attributes.Add(baseAttribute.attribute);
            }
            AddAttributes(attributes);
            AddSkills(characterClass.skills);
            AddItems(new Item[] { characterClass.rightHandEquipItem, characterClass.leftHandEquipItem });
            AddItems(characterClass.otherEquipItems);
        }
    }

    public static void AddCharacterModels(IEnumerable<CharacterModel> characterModels)
    {
        foreach (var characterModel in characterModels)
        {
            if (characterModel == null || CharacterModels.ContainsKey(characterModel.Id))
                continue;
            CharacterModels[characterModel.Id] = characterModel;
        }
    }

    public static void AddCharacterPrototypes(IEnumerable<CharacterPrototype> characterPrototypes)
    {
        foreach (var characterPrototype in characterPrototypes)
        {
            if (characterPrototype == null || CharacterPrototypes.ContainsKey(characterPrototype.Id))
                continue;
            CharacterPrototypes[characterPrototype.Id] = characterPrototype;
            AddCharacterClasses(new CharacterClass[] { characterPrototype.characterClass });
            AddCharacterModels(new CharacterModel[] { characterPrototype.characterModel });
        }
    }

    public static void AddDamageElements(IEnumerable<DamageElement> damageElements)
    {
        foreach (var damageElement in damageElements)
        {
            if (damageElement == null || DamageElements.ContainsKey(damageElement.Id))
                continue;
            DamageElements[damageElement.Id] = damageElement;
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

    public static void AddItems(IEnumerable<Item> items)
    {
        foreach (var item in items)
        {
            if (item == null || Items.ContainsKey(item.Id))
                continue;
            Items[item.Id] = item;
            if (item is BaseEquipmentItem)
            {
                var equipmentItem = item as BaseEquipmentItem;
                var attributes = new List<Attribute>();
                var requireAttributes = equipmentItem.requirement.attributeAmounts;
                foreach (var requireAttribute in requireAttributes)
                {
                    if (requireAttribute.attribute == null || Attributes.ContainsKey(requireAttribute.attribute.Id))
                        continue;
                    attributes.Add(requireAttribute.attribute);
                }
                AddAttributes(attributes);
            }
            if (item is WeaponItem)
            {
                var weaponItem = item as WeaponItem;
                var attributes = new List<Attribute>();
                foreach (var effectivenessAttribute in weaponItem.WeaponType.effectivenessAttributes)
                {
                    if (effectivenessAttribute.attribute == null || Attributes.ContainsKey(effectivenessAttribute.attribute.Id))
                        continue;
                    attributes.Add(effectivenessAttribute.attribute);
                }
                AddAttributes(attributes);
                var damageElements = new List<DamageElement>();
                var damageAttributes = weaponItem.increaseDamageAttributes;
                foreach (var damageAttribute in damageAttributes)
                {
                    if (damageAttribute.damageElement == null || DamageElements.ContainsKey(damageAttribute.damageElement.Id))
                        continue;
                    damageElements.Add(damageAttribute.damageElement);
                }
                AddDamageElements(damageElements);
                var missileDamageEntity = weaponItem.WeaponType.damage.missileDamageEntity;
                if (missileDamageEntity != null)
                    AddDamageEntities(new DamageEntity[] { missileDamageEntity });
            }
        }
    }

    public static void AddSkills(IEnumerable<Skill> skills)
    {
        foreach (var skill in skills)
        {
            if (skill == null || Skills.ContainsKey(skill.Id))
                continue;
            Skills[skill.Id] = skill;
            var damageElements = new List<DamageElement>();
            var damageAttributes = skill.additionalDamageAttributes;
            foreach (var damageAttribute in damageAttributes)
            {
                if (damageAttribute.damageElement == null || DamageElements.ContainsKey(damageAttribute.damageElement.Id))
                    continue;
                damageElements.Add(damageAttribute.damageElement);
            }
            AddDamageElements(damageElements);
            var missileDamageEntity = skill.damage.missileDamageEntity;
            if (missileDamageEntity != null)
                AddDamageEntities(new DamageEntity[] { missileDamageEntity });
        }
    }

    public T GetExtra<T>() where T : BaseGameInstanceExtra
    {
        return extra as T;
    }
}
