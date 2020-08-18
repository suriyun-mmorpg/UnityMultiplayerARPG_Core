using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LiteNetLibManager;
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
        [ArrayElementTitle("skill")]
        private SkillLevel[] skillLevels;

        [Header("Start Equipments")]
        public BaseItem rightHandEquipItem;
        public BaseItem leftHandEquipItem;
        public BaseItem[] armorItems;

        [Header("Start Items")]
        [Tooltip("Items that will be added to character when create new character")]
        [ArrayElementTitle("item")]
        public ItemAmount[] startItems;

        [Header("Start Map")]
        public BaseMapInfo startMap;
        public BaseMapInfo StartMap
        {
            get
            {
                if (startMap == null)
                    return GameInstance.MapInfos.FirstOrDefault().Value;
                return startMap;
            }
        }
        public bool useOverrideStartPosition;
        public Vector3 overrideStartPosition;
        public Vector3 StartPosition
        {
            get
            {
                return useOverrideStartPosition ? overrideStartPosition : StartMap.StartPosition;
            }
        }

        public SkillLevel[] SkillLevels
        {
            get { return skillLevels; }
        }

        [System.NonSerialized]
        private Dictionary<BaseSkill, short> cacheSkillLevels;
        public override Dictionary<BaseSkill, short> CacheSkillLevels
        {
            get
            {
                if (cacheSkillLevels == null)
                    cacheSkillLevels = GameDataHelpers.CombineSkills(SkillLevels, new Dictionary<BaseSkill, short>());
                return cacheSkillLevels;
            }
        }
        
        public override bool Validate()
        {
            bool hasChanges = base.Validate();
            IWeaponItem tempRightHandWeapon = null;
            IWeaponItem tempLeftHandWeapon = null;
            IShieldItem tempLeftHandShield = null;
            if (rightHandEquipItem != null)
            {
                if (rightHandEquipItem.IsWeapon())
                    tempRightHandWeapon = rightHandEquipItem as IWeaponItem;

                if (tempRightHandWeapon == null || tempRightHandWeapon.WeaponType == null)
                {
                    Logging.LogWarning(ToString(), "Right hand equipment is not weapon");
                    rightHandEquipItem = null;
                    hasChanges = true;
                }
            }
            if (leftHandEquipItem != null)
            {
                if (leftHandEquipItem.IsWeapon())
                    tempLeftHandWeapon = leftHandEquipItem as IWeaponItem;
                if (leftHandEquipItem.IsShield())
                    tempLeftHandShield = leftHandEquipItem as IShieldItem;

                if ((tempLeftHandWeapon == null || tempLeftHandWeapon.WeaponType == null) && tempLeftHandShield == null)
                {
                    Logging.LogWarning(ToString(), "Left hand equipment is not weapon or shield");
                    leftHandEquipItem = null;
                    hasChanges = true;
                }
                else if (tempRightHandWeapon != null)
                {
                    if (tempLeftHandShield != null && tempRightHandWeapon.EquipType == WeaponItemEquipType.TwoHand)
                    {
                        Logging.LogWarning(ToString(), "Cannot set left hand equipment because it's equipping two hand weapon");
                        leftHandEquipItem = null;
                        hasChanges = true;
                    }
                    else if (tempLeftHandWeapon != null && tempRightHandWeapon.EquipType != WeaponItemEquipType.OneHandCanDual)
                    {
                        Logging.LogWarning(ToString(), "Cannot set left hand equipment because it's equipping one hand weapon which cannot equip dual");
                        leftHandEquipItem = null;
                        hasChanges = true;
                    }
                }
                if (tempLeftHandWeapon != null &&
                    (tempLeftHandWeapon.EquipType == WeaponItemEquipType.OneHand ||
                    tempLeftHandWeapon.EquipType == WeaponItemEquipType.TwoHand))
                {
                    Logging.LogWarning(ToString(), "Left hand weapon cannot be OneHand or TwoHand");
                    leftHandEquipItem = null;
                    hasChanges = true;
                }
            }
            List<string> equipedPositions = new List<string>();
            BaseItem armorItem;
            for (int i = 0; i < armorItems.Length; ++i)
            {
                armorItem = armorItems[i];
                if (armorItem == null)
                    continue;

                if (!armorItem.IsArmor())
                {
                    // Item is not armor, so set it to NULL
                    armorItems[i] = null;
                    hasChanges = true;
                    continue;
                }

                if (equipedPositions.Contains((armorItem as IArmorItem).EquipPosition))
                {
                    // Already equip armor at the position, it cannot equip same position again, So set it to NULL
                    armorItems[i] = null;
                    hasChanges = true;
                }
                else
                {
                    equipedPositions.Add((armorItem as IArmorItem).EquipPosition);
                }
            }
            return hasChanges;
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            List<BaseItem> items = new List<BaseItem>();
            if (armorItems != null && armorItems.Length > 0)
                items.AddRange(armorItems);
            items.Add(rightHandEquipItem);
            items.Add(leftHandEquipItem);
        }
    }
}
