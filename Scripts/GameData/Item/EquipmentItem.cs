using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "EquipmentItem", menuName = "Create GameData/EquipmentItem")]
public class EquipmentItem : Item
{
    public string equipPosition;
    public GameObject equipmentModel;
    [Header("Requirements")]
    public EquipmentRequirement requirement;
    [Header("Add Attributes")]
    public AttributeIncremental[] increaseAttributes;
    [Header("Add Resistances")]
    public ResistanceIncremental[] increaseResistances;
    [Header("Add Stats")]
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
