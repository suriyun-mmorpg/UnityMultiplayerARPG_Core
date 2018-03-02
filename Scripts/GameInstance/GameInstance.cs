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
    public DamageEntity damageEntityPrefab;
    public ItemDropEntity itemDropEntityPrefab;
    public FollowCameraControls gameplayCameraPrefab;
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
    public int inventorySize = 30;
    public float pickUpItemDistance = 2f;
    public float dropDistance = 2f;
    [Header("Scene")]
    public string homeSceneName = "Home";
    public string startSceneName;
    public Vector3 startPosition;
    [Header("Player Configs")]
    public int minCharacterNameLength = 2;
    public int maxCharacterNameLength = 16;
    public static readonly Dictionary<string, CharacterAttribute> CharacterAttributes = new Dictionary<string, CharacterAttribute>();
    public static readonly Dictionary<string, CharacterClass> CharacterClasses = new Dictionary<string, CharacterClass>();
    public static readonly Dictionary<string, CharacterPrototype> CharacterPrototypes = new Dictionary<string, CharacterPrototype>();
    public static readonly Dictionary<string, Damage> Damages = new Dictionary<string, Damage>();
    public static readonly Dictionary<string, DamageEntity> DamageEntities = new Dictionary<string, DamageEntity>();
    public static readonly Dictionary<string, Item> Items = new Dictionary<string, Item>();
    public static readonly Dictionary<string, Skill> Skills = new Dictionary<string, Skill>();

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
        if (damageEntityPrefab == null)
        {
            Debug.LogError("You must set damage entity prefab");
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
        if (defaultWeaponItem == null)
        {
            Debug.LogError("You must set default weapon item");
            return;
        }

        AddCharacterPrototypes(characterPrototypes);
        AddDamageEntities(new DamageEntity[] { damageEntityPrefab });
        AddItems(new Item[] { defaultWeaponItem });
        AddItems(items);
        AddSkills(skills);
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

    public static void AddDamages(IEnumerable<Damage> damages)
    {
        foreach (var damage in damages)
        {
            if (damage == null || Damages.ContainsKey(damage.Id))
                continue;
            Damages[damage.Id] = damage;
        }
    }

    public static void AddDamageEntities(IEnumerable<DamageEntity> damageEntities)
    {
        foreach (var damageEntity in damageEntities)
        {
            if (damageEntity == null || DamageEntities.ContainsKey(damageEntity.name))
                continue;
            DamageEntities[damageEntity.name] = damageEntity;
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
                foreach (var effectivenessAttribute in weaponItem.effectivenessAttributes)
                {
                    if (effectivenessAttribute == null || effectivenessAttribute.attribute == null || CharacterAttributes.ContainsKey(effectivenessAttribute.attribute.Id))
                        continue;
                    attributes.Add(effectivenessAttribute.attribute);
                }
                AddCharacterAttributes(attributes);
                var damages = new List<Damage>();
                foreach (var damage in weaponItem.damages)
                {
                    if (damage == null || damage.damage == null || Damages.ContainsKey(damage.damage.Id))
                        continue;
                    damages.Add(damage.damage);
                }
                AddDamages(damages);
                AddDamageEntities(new DamageEntity[] { weaponItem.DamageEntityPrefab });
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
            var damages = new List<Damage>();
            foreach (var damage in skill.damages)
            {
                if (damage == null || damage.damage == null || Damages.ContainsKey(damage.damage.Id))
                    continue;
                damages.Add(damage.damage);
            }
            AddDamages(damages);
            AddDamageEntities(new DamageEntity[] { skill.DamageEntityPrefab });
        }
    }

    public T GetExtra<T>() where T : BaseGameInstanceExtra
    {
        return extra as T;
    }
}
