using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "PlayerCharacter", menuName = "Create GameData/PlayerCharacter")]
public class PlayerCharacter : BaseCharacter
{
    [Header("Skills")]
    public SkillLevel[] skillLevels;

    [Header("Start Equipments")]
    public Item rightHandEquipItem;
    public Item leftHandEquipItem;
    public Item[] armorItems;

    private Dictionary<int, SkillLevel> cacheSkillLevels;
    public Dictionary<int, SkillLevel> CacheSkillLevels
    {
        get
        {
            if (cacheSkillLevels == null)
            {
                cacheSkillLevels = new Dictionary<int, SkillLevel>();
                foreach (var skillLevel in skillLevels)
                {
                    if (skillLevel.skill != null)
                    cacheSkillLevels[skillLevel.skill.HashId] = skillLevel;
                }
            }
            return cacheSkillLevels;
        }
    }

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
}
