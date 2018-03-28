using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "PlayerCharacterDatabase", menuName = "Create GameData/PlayerCharacterDatabase")]
public class PlayerCharacterDatabase : BaseCharacterDatabase
{
    [Header("Attributes/Stats")]
    public AttributeAmount[] baseAttributes;
    public CharacterStatsIncremental stats;

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
            {
                Debug.LogWarning("Right hand equipment is not weapon");
                rightHandEquipItem = null;
            }
        }
        if (leftHandEquipItem != null)
        {
            if (leftHandEquipItem.itemType == ItemType.Weapon)
                tempLeftHandWeapon = leftHandEquipItem;
            if (leftHandEquipItem.itemType == ItemType.Shield)
                tempLeftHandShield = leftHandEquipItem;

            if ((tempLeftHandWeapon == null || tempLeftHandWeapon.weaponType == null) && tempLeftHandShield == null)
            {
                Debug.LogWarning("Left hand equipment is not weapon or shield");
                leftHandEquipItem = null;
            }
            else if (tempRightHandWeapon != null)
            {
                if (tempLeftHandShield != null && tempRightHandWeapon.EquipType == WeaponItemEquipType.TwoHand)
                {
                    Debug.LogWarning("Cannot set left hand equipment because it's equipping two hand weapon");
                    leftHandEquipItem = null;
                }
                else if (tempLeftHandWeapon != null && tempRightHandWeapon.EquipType != WeaponItemEquipType.OneHandCanDual)
                {
                    Debug.LogWarning("Cannot set left hand equipment because it's equipping one hand weapon which cannot equip dual");
                    leftHandEquipItem = null;
                }
            }
            if (leftHandEquipItem != null)
            {
                if (leftHandEquipItem.EquipType == WeaponItemEquipType.OneHand ||
                    leftHandEquipItem.EquipType == WeaponItemEquipType.TwoHand)
                {
                    Debug.LogWarning("Left hand weapon cannot be OneHand or TwoHand");
                    leftHandEquipItem = null;
                }
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

    public override CharacterStats GetCharacterStats(int level)
    {
        return stats.GetCharacterStats(level);
    }
}
