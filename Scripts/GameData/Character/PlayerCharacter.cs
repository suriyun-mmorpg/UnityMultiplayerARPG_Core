using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Player Character", menuName = "Create GameData/Player Character", order = -4999)]
    public sealed partial class PlayerCharacter : BaseCharacter
    {
        [Header("Skills")]
        [SerializeField]
        private SkillLevel[] skillLevels;

        [Header("Start Equipments")]
        public Item rightHandEquipItem;
        public Item leftHandEquipItem;
        public Item[] armorItems;

        [Header("Start Map")]
        public MapInfo startMap;

        public MapInfo StartMap
        {
            get
            {
                if (startMap == null)
                    return GameInstance.MapInfos.FirstOrDefault().Value;
                return startMap;
            }
        }

        public SkillLevel[] SkillLevels
        {
            get { return skillLevels; }
        }

        [System.NonSerialized]
        private Dictionary<Skill, short> cacheSkillLevels;
        public override Dictionary<Skill, short> CacheSkillLevels
        {
            get
            {
                if (cacheSkillLevels == null)
                    cacheSkillLevels = GameDataHelpers.CombineSkills(SkillLevels, new Dictionary<Skill, short>());
                return cacheSkillLevels;
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            bool hasChanges = false;
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
                    hasChanges = true;
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
                    hasChanges = true;
                }
                else if (tempRightHandWeapon != null)
                {
                    if (tempLeftHandShield != null && tempRightHandWeapon.EquipType == WeaponItemEquipType.TwoHand)
                    {
                        Debug.LogWarning("Cannot set left hand equipment because it's equipping two hand weapon");
                        leftHandEquipItem = null;
                        hasChanges = true;
                    }
                    else if (tempLeftHandWeapon != null && tempRightHandWeapon.EquipType != WeaponItemEquipType.OneHandCanDual)
                    {
                        Debug.LogWarning("Cannot set left hand equipment because it's equipping one hand weapon which cannot equip dual");
                        leftHandEquipItem = null;
                        hasChanges = true;
                    }
                }
                if (leftHandEquipItem != null)
                {
                    if (leftHandEquipItem.EquipType == WeaponItemEquipType.OneHand ||
                        leftHandEquipItem.EquipType == WeaponItemEquipType.TwoHand)
                    {
                        Debug.LogWarning("Left hand weapon cannot be OneHand or TwoHand");
                        leftHandEquipItem = null;
                        hasChanges = true;
                    }
                }
            }
            List<string> equipedPositions = new List<string>();
            for (int i = 0; i < armorItems.Length; ++i)
            {
                Item armorItem = armorItems[i];
                if (armorItem == null)
                    continue;

                if (armorItem.itemType != ItemType.Armor)
                {
                    // Item is not armor, so set it to NULL
                    armorItems[i] = null;
                    hasChanges = true;
                    continue;
                }

                if (equipedPositions.Contains(armorItem.EquipPosition))
                {
                    // Already equip armor at the position, it cannot equip same position again, So set it to NULL
                    armorItems[i] = null;
                    hasChanges = true;
                }
                else
                    equipedPositions.Add(armorItem.EquipPosition);
            }
            // Mark asset to be dirty when chagnes occured
            if (hasChanges)
                EditorUtility.SetDirty(this);
        }
#endif
    }
}
