using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "CharacterClass", menuName = "Create GameData/CharacterClass")]
public class CharacterClass : BaseGameData
{
    [Header("Attributes/Stats")]
    public AttributeAmount[] baseAttributes;
    public CharacterStats baseStats;
    public CharacterStats statsIncreaseEachLevel;

    [Header("Skills")]
    public Skill[] skills;

    [Header("Start Equipments")]
    public EquipmentItem rightHandEquipItem;
    public EquipmentItem leftHandEquipItem;
    public EquipmentItem[] otherEquipItems;

#if UNITY_EDITOR
    void OnValidate()
    {
        WeaponItem rightHandAsWeapon = null;
        WeaponItem leftHandAsWeapon = null;
        ShieldItem leftHandAsShield = null;
        if (rightHandEquipItem != null)
        {
            rightHandAsWeapon = rightHandEquipItem as WeaponItem;
            if (rightHandAsWeapon == null || rightHandAsWeapon.weaponType == null)
                rightHandEquipItem = null;
        }
        if (leftHandEquipItem != null)
        {
            leftHandAsWeapon = leftHandEquipItem as WeaponItem;
            leftHandAsShield = leftHandEquipItem as ShieldItem;
            if ((leftHandAsWeapon == null || leftHandAsWeapon.weaponType == null) && leftHandAsShield == null)
                leftHandEquipItem = null;
            else if (rightHandAsWeapon != null)
            {
                if (leftHandAsShield != null && rightHandAsWeapon.weaponType.equipType == WeaponItemEquipType.TwoHand)
                    leftHandEquipItem = null;
                else if (leftHandAsWeapon != null && rightHandAsWeapon.weaponType.equipType != WeaponItemEquipType.OneHandCanDual)
                    leftHandEquipItem = null;
            }
        }
        var equipedPositions = new List<string>();
        for (var i = 0; i < otherEquipItems.Length; ++i)
        {
            var otherEquipItem = otherEquipItems[i];
            if (otherEquipItem == null)
                continue;
            if (otherEquipItem as WeaponItem != null ||
                otherEquipItem as ShieldItem != null)
            {
                otherEquipItems[i] = null;
                continue;
            }
            if (equipedPositions.Contains(otherEquipItem.equipPosition))
                otherEquipItems[i] = null;
            else
                equipedPositions.Add(otherEquipItem.equipPosition);
        }
        EditorUtility.SetDirty(this);
    }
#endif
}
