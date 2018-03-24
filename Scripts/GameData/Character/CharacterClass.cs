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
    public Item rightHandEquipItem;
    public Item leftHandEquipItem;
    public Item[] armorItems;

#if UNITY_EDITOR
    void OnValidate()
    {
        Item tempRightHandWeapon = null;
        Item tempLeftHandWeapon = null;
        Item tempLeftHandShield = null;
        if (rightHandEquipItem != null)
        {
            if (rightHandEquipItem.itemType == ItemType.Weapon)
                tempRightHandWeapon = rightHandEquipItem;

            if (tempRightHandWeapon == null || tempRightHandWeapon.weaponType == null)
                rightHandEquipItem = null;
        }
        if (leftHandEquipItem != null)
        {
            if (leftHandEquipItem.itemType == ItemType.Weapon)
                tempLeftHandWeapon = leftHandEquipItem;
            if (leftHandEquipItem.itemType == ItemType.Shield)
                tempLeftHandShield = leftHandEquipItem;

            if ((tempLeftHandWeapon == null || tempLeftHandWeapon.weaponType == null) && tempLeftHandShield == null)
                leftHandEquipItem = null;
            else if (tempRightHandWeapon != null)
            {
                if (tempLeftHandShield != null && tempRightHandWeapon.EquipType == WeaponItemEquipType.TwoHand)
                    leftHandEquipItem = null;
                else if (tempLeftHandWeapon != null && tempRightHandWeapon.EquipType != WeaponItemEquipType.OneHandCanDual)
                    leftHandEquipItem = null;
            }
        }
        var equipedPositions = new List<string>();
        for (var i = 0; i < armorItems.Length; ++i)
        {
            var armorItem = armorItems[i];
            if (armorItem == null)
                continue;

            if (armorItem.itemType != ItemType.Armor)
            {
                armorItems[i] = null;
                continue;
            }

            if (equipedPositions.Contains(armorItem.EquipPosition))
                armorItems[i] = null;
            else
                equipedPositions.Add(armorItem.EquipPosition);
        }
        EditorUtility.SetDirty(this);
    }
#endif
}
