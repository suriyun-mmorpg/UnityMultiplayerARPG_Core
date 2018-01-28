using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "EquipmentItem", menuName = "Create GameData/EquipmentItem")]
public class EquipmentItem : Item
{
    public const string EQUIP_POSITION_RIGHT_HAND = "RIGHT_HAND";
    public const string EQUIP_POSITION_LEFT_HAND = "LEFT_HAND";
    public string equipPosition;
    public CharacterAttributeAmount[] requireAttributes;
    public CharacterStats baseStats;
    public CharacterStats statsIncreaseEachLevel;
    public CharacterStatsPercentage statsPercentageIncreaseEachLevel;

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        if (equipPosition.Equals(EQUIP_POSITION_LEFT_HAND) || equipPosition.Equals(EQUIP_POSITION_RIGHT_HAND))
        {
            equipPosition = string.Empty;
            Debug.LogError("Equip Position cannot be " + EQUIP_POSITION_RIGHT_HAND + " or " + EQUIP_POSITION_LEFT_HAND);
        }
        // Equipment max stack always equals to 1
        maxStack = 1;
        EditorUtility.SetDirty(this);
    }
#endif
}
