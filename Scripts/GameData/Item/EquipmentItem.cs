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
    public CharacterClass requireClass;
    [Header("Requirements")]
    public int requireCharacterLevel;
    public CharacterAttributeAmount[] requireAttributes;
    [Header("Add Attributes")]
    public CharacterAttributeIncremental[] increaseAttributes;
    [Header("Add Resistances")]
    public CharacterResistanceIncremental[] increaseResistances;
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
}
