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
    public CharacterEntity characterEntityPrefab;
    public ItemDropEntity itemDropEntityPrefab;
    public FollowCameraControls gameplayCameraPrefab;
    public UISceneGameplay uiSceneGameplayPrefab;
    [Header("Gameplay Database")]
    [Tooltip("Default weapon item, will be used when character not equip any weapon")]
    public WeaponItem defaultWeaponItem;
    public CharacterPrototype[] characterPrototypes;
    public Item[] items;
    public Skill[] skills;
    public int[] expTree;
    [Header("Gameplay Configs")]
    public int increaseStatPointEachLevel = 5;
    public int increaseSkillPointEachLevel = 1;
    public int startGold = 0;
    public ItemAmountPair[] startItems;
    public float pickUpItemDistance = 2f;
    public float dropDistance = 2f;
    [Header("Scene")]
    public string homeSceneName = "Home";
    public string startSceneName;
    public Vector3 startPosition;
    [Header("Player Configs")]
    public int minCharacterNameLength = 2;
    public int maxCharacterNameLength = 16;
    public static readonly HashSet<string> EquipmentPositions = new HashSet<string>();
    public static readonly Dictionary<string, CharacterAttribute> CharacterAttributes = new Dictionary<string, CharacterAttribute>();
    public static readonly Dictionary<string, CharacterClass> CharacterClasses = new Dictionary<string, CharacterClass>();
    public static readonly Dictionary<string, CharacterPrototype> CharacterPrototypes = new Dictionary<string, CharacterPrototype>();
    public static readonly Dictionary<string, DamageElement> DamageElements = new Dictionary<string, DamageElement>();
    public static readonly Dictionary<string, DamageEntity> DamageEntities = new Dictionary<string, DamageEntity>();
    public static readonly Dictionary<string, Item> Items = new Dictionary<string, Item>();
    public static readonly Dictionary<string, Skill> Skills = new Dictionary<string, Skill>();

    private DamageElement tempDefaultDamageElement;
    public DamageElement DefaultDamageElement
    {
        get
        {
            if (tempDefaultDamageElement == null)
            {
                tempDefaultDamageElement = ScriptableObject.CreateInstance<DamageElement>();
                tempDefaultDamageElement.name = GameDataConst.DEFAULT_DAMAGE_ID;
                tempDefaultDamageElement.title = GameDataConst.DEFAULT_DAMAGE_TITLE;
            }
            return tempDefaultDamageElement;
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

        if (characterEntityPrefab == null)
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
        if (defaultWeaponItem == null)
        {
            Debug.LogError("You must set default weapon item");
            return;
        }


        EquipmentPositions.Clear();
        CharacterAttributes.Clear();
        CharacterClasses.Clear();
        CharacterPrototypes.Clear();
        DamageElements.Clear();
        DamageEntities.Clear();
        Items.Clear();
        Skills.Clear();

        EquipmentPositions.Add(GameDataConst.EQUIP_POSITION_RIGHT_HAND);
        EquipmentPositions.Add(GameDataConst.EQUIP_POSITION_LEFT_HAND);

        AddCharacterPrototypes(characterPrototypes);
        AddItems(new Item[] { defaultWeaponItem });
        AddItems(items);
        AddSkills(skills);

        var tempStartItems = new List<Item>();
        foreach (var startItem in startItems)
        {
            if (startItem.item == null)
                continue;
            tempStartItems.Add(startItem.item);
        }
        AddItems(tempStartItems);
    }

    public static void AddCharacterAttributes(IEnumerable<CharacterAttribute> characterAttributes)
    {
        foreach (var characterAttribute in characterAttributes)
        {
            if (characterAttribute == null || CharacterAttributes.ContainsKey(characterAttribute.Id))
                continue;
            CharacterAttributes[characterAttribute.Id] = characterAttribute;
        }
    }

    public static void AddCharacterClasses(IEnumerable<CharacterClass> characterClasses)
    {
        foreach (var characterClass in characterClasses)
        {
            if (characterClass == null || CharacterClasses.ContainsKey(characterClass.Id))
                continue;
            CharacterClasses[characterClass.Id] = characterClass;
            var attributes = new List<CharacterAttribute>();
            foreach (var baseAttribute in characterClass.baseAttributes)
            {
                if (baseAttribute == null || baseAttribute.attribute == null || CharacterAttributes.ContainsKey(baseAttribute.attribute.Id))
                    continue;
                attributes.Add(baseAttribute.attribute);
            }
            AddCharacterAttributes(attributes);
            AddSkills(characterClass.skills);
            AddItems(new Item[] { characterClass.rightHandEquipItem, characterClass.leftHandEquipItem });
            AddItems(characterClass.otherEquipItems);
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
            if (item is EquipmentItem)
            {
                var equipmentItem = item as EquipmentItem;
                if (!EquipmentPositions.Contains(equipmentItem.equipPosition))
                    EquipmentPositions.Add(equipmentItem.equipPosition);

                var attributes = new List<CharacterAttribute>();
                foreach (var requireAttribute in equipmentItem.requireAttributes)
                {
                    if (requireAttribute == null || requireAttribute.attribute == null || CharacterAttributes.ContainsKey(requireAttribute.attribute.Id))
                        continue;
                    attributes.Add(requireAttribute.attribute);
                }
                AddCharacterAttributes(attributes);
            }
            if (item is WeaponItem)
            {
                var weaponItem = item as WeaponItem;
                var attributes = new List<CharacterAttribute>();
                foreach (var effectivenessAttribute in weaponItem.WeaponType.effectivenessAttributes)
                {
                    if (effectivenessAttribute == null || effectivenessAttribute.attribute == null || CharacterAttributes.ContainsKey(effectivenessAttribute.attribute.Id))
                        continue;
                    attributes.Add(effectivenessAttribute.attribute);
                }
                AddCharacterAttributes(attributes);
                var damageElements = new List<DamageElement>();
                var damageAttributes = weaponItem.additionalDamageAttributes;
                foreach (var damageAttribute in damageAttributes)
                {
                    if (damageAttribute == null || damageAttribute.damageElement == null || DamageElements.ContainsKey(damageAttribute.damageElement.Id))
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
                if (damageAttribute == null || damageAttribute.damageElement == null || DamageElements.ContainsKey(damageAttribute.damageElement.Id))
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
