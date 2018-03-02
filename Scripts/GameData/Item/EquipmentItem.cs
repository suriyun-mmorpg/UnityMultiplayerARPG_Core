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
    public CharacterClass requireClass;
    public CharacterAttributeAmount[] requireAttributes;
    public CharacterStats baseStats;
    public CharacterStats statsIncreaseEachLevel;
    public CharacterStatsPercentage statsPercentageIncreaseEachLevel;

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        // Equipment max stack always equals to 1
        maxStack = 1;
        EditorUtility.SetDirty(this);
    }
#endif
}
