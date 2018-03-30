using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ItemType
{
    Junk,
    Armor,
    Weapon,
    Shield
}

[CreateAssetMenu(fileName = "Item", menuName = "Create GameData/Item")]
public class Item : BaseGameData
{
    public ItemType itemType;
    public int sellPrice;
    [Range(1, 1000)]
    public int maxStack = 1;
    public float weight;

    [Space(10)]
    [Header("Equipment")]
    public EquipmentModel[] equipmentModels;
    [Header("Requirements")]
    public EquipmentRequirement requirement;
    [Header("Attributes Bonus")]
    public AttributeIncremental[] increaseAttributes;
    [Header("Resistances Bonus")]
    public ResistanceIncremental[] increaseResistances;
    [Header("Damages Bonus")]
    public DamageAttribute[] increaseDamageAttributes;
    [Header("Stats Bonus")]
    public CharacterStatsIncremental increaseStats;

    [Space(10)]
    [Header("Armor")]
    public ArmorType armorType;

    [Space(10)]
    [Header("Weapon")]
    public WeaponType weaponType;
    public DamageAttribute damageAttribute;

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        // Equipment max stack always equals to 1
        switch (itemType)
        {
            case ItemType.Armor:
            case ItemType.Weapon:
            case ItemType.Shield:
                maxStack = 1;
                break;
        }
        EditorUtility.SetDirty(this);
    }
#endif

    public bool IsEquipment()
    {
        return itemType != ItemType.Junk;
    }

    public bool IsDefendEquipment()
    {
        return itemType == ItemType.Armor || itemType == ItemType.Shield;
    }

    public bool IsJunk()
    {
        return itemType == ItemType.Junk;
    }

    public bool IsArmor()
    {
        return itemType == ItemType.Armor;
    }

    public bool IsWeapon()
    {
        return itemType == ItemType.Weapon;
    }

    public bool IsShield()
    {
        return itemType == ItemType.Shield;
    }

    #region Cache Data
    private Dictionary<Attribute, int> cacheRequireAttributeAmounts;
    public Dictionary<Attribute, int> CacheRequireAttributeAmounts
    {
        get
        {
            if (cacheRequireAttributeAmounts == null)
                cacheRequireAttributeAmounts = GameDataHelpers.MakeAttributeAmountsDictionary(requirement.attributeAmounts, new Dictionary<Attribute, int>());
            return cacheRequireAttributeAmounts;
        }
    }

    public ArmorType ArmorType
    {
        get
        {
            if (armorType == null)
                armorType = GameInstance.Singleton.DefaultArmorType;
            return armorType;
        }
    }

    public string EquipPosition
    {
        get { return ArmorType.Id; }
    }

    public WeaponType WeaponType
    {
        get
        {
            if (weaponType == null)
                weaponType = GameInstance.Singleton.DefaultWeaponType;
            return weaponType;
        }
    }

    public WeaponItemEquipType EquipType
    {
        get { return WeaponType.equipType; }
    }
    #endregion
}

[System.Serializable]
public struct EquipmentModel
{
    public string equipSocket;
    public GameObject model;
}

[System.Serializable]
public struct ItemAmountPair
{
    public Item item;
    public int amount;
}

[System.Serializable]
public struct ItemDrop
{
    public Item item;
    public int amount;
    [Range(0f, 1f)]
    public float dropRate;
}

[System.Serializable]
public struct EquipmentRequirement
{
    public PlayerCharacterDatabase character;
    public int level;
    public AttributeAmount[] attributeAmounts;
}