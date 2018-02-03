using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameInstance : MonoBehaviour
{
    public static GameInstance Singleton { get; protected set; }
    public CharacterEntity characterEntityPrefab;
    public CharacterPrototype[] characterPrototypes;
    public Item[] items;
    public Skill[] skills;
    public int[] expTree;
    public int increaseStatPointEachLevel = 5;
    public int increaseSkillPointEachLevel = 1;
    public int startGold = 0;
    public static readonly Dictionary<string, CharacterAttribute> CharacterAttributes = new Dictionary<string, CharacterAttribute>();
    public static readonly Dictionary<string, CharacterClass> CharacterClasses = new Dictionary<string, CharacterClass>();
    public static readonly Dictionary<string, CharacterPrototype> CharacterPrototypes = new Dictionary<string, CharacterPrototype>();
    public static readonly Dictionary<string, Damage> Damages = new Dictionary<string, Damage>();
    public static readonly Dictionary<string, Item> Items = new Dictionary<string, Item>();
    public static readonly Dictionary<string, Skill> Skills = new Dictionary<string, Skill>();

    protected virtual void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Singleton = this;
        
        ClientScene.RegisterPrefab(characterEntityPrefab.gameObject);
        AddCharacterPrototypes(characterPrototypes);
        AddItems(items);
        AddSkills(skills);
    }

    public static void AddCharacterAttributes(CharacterAttribute[] characterAttributes)
    {
        foreach (var characterAttribute in characterAttributes)
        {
            if (characterAttribute == null || CharacterAttributes.ContainsKey(characterAttribute.Id))
                continue;
            CharacterAttributes[characterAttribute.Id] = characterAttribute;
        }
    }

    public static void AddCharacterClasses(CharacterClass[] characterClasses)
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
            AddCharacterAttributes(attributes.ToArray());
        }
    }

    public static void AddCharacterPrototypes(CharacterPrototype[] characterPrototypes)
    {
        foreach (var characterPrototype in characterPrototypes)
        {
            if (characterPrototype == null || CharacterPrototypes.ContainsKey(characterPrototype.Id))
                continue;
            CharacterPrototypes[characterPrototype.Id] = characterPrototype;
            AddCharacterClasses(new CharacterClass[] { characterPrototype.characterClass });
        }
    }

    public static void AddDamages(Damage[] damages)
    {
        foreach (var damage in damages)
        {
            if (damage == null || Damages.ContainsKey(damage.Id))
                continue;
            Damages[damage.Id] = damage;
        }
    }

    public static void AddItems(Item[] items)
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
                AddCharacterAttributes(attributes.ToArray());
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
                AddCharacterAttributes(attributes.ToArray());
                var damages = new List<Damage>();
                foreach (var damage in weaponItem.damages)
                {
                    if (damage == null || damage.damage == null || Damages.ContainsKey(damage.damage.Id))
                        continue;
                    damages.Add(damage.damage);
                }
                AddDamages(damages.ToArray());
            }
        }
    }

    public static void AddSkills(Skill[] skills)
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
            AddDamages(damages.ToArray());
        }
    }
}
