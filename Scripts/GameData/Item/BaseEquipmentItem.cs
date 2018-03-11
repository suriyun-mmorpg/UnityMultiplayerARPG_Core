using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class BaseEquipmentItem : Item
{
    public GameObject equipmentModel;
    [Header("Requirements")]
    public EquipmentRequirement requirement;
    [Header("Attributes Bonus")]
    public AttributeIncremental[] increaseAttributes;
    [Header("Resistances Bonus")]
    public ResistanceIncremental[] increaseResistances;
    [Header("Damages Bonus")]
    public DamageAttribute[] increaseDamageAttributes;
    [Header("Stats Bonus")]
    public CharacterStats baseStats;
    public CharacterStats statsIncreaseEachLevel;

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        // Equipment max stack always equals to 1
        maxStack = 1;
        EditorUtility.SetDirty(this);
    }
#endif

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
}

[System.Serializable]
public struct EquipmentRequirement
{
    public CharacterClass characterClass;
    public int characterLevel;
    public AttributeAmount[] attributeAmounts;
}
