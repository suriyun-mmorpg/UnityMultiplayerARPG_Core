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
    public partial class PlayerCharacter : BaseCharacter
    {
        [Category(3, "Character Stats")]
        [SerializeField]
        [ArrayElementTitle("skill")]
        private SkillLevel[] skillLevels = new SkillLevel[0];

        [Category(4, "Start Items")]
        [Header("Equipped Items")]
        [SerializeField]
        private BaseItem rightHandEquipItem = null;
        public BaseItem RightHandEquipItem { get { return rightHandEquipItem; } }
        [SerializeField]
        private BaseItem leftHandEquipItem = null;
        public BaseItem LeftHandEquipItem { get { return leftHandEquipItem; } }
        [SerializeField]
        private BaseItem[] armorItems = new BaseItem[0];
        public BaseItem[] ArmorItems { get { return armorItems; } }
        [Header("Items in Inventory")]
        [SerializeField]
        [Tooltip("Items that will be added to character when create new character")]
        [ArrayElementTitle("item")]
        private ItemAmount[] startItems = new ItemAmount[0];
        public ItemAmount[] StartItems { get { return startItems; } }

        [Category(6, "Start Map")]
        [SerializeField]
        private BaseMapInfo startMap = null;
        public BaseMapInfo StartMap
        {
            get
            {
                if (startMap == null)
                    return GameInstance.MapInfos.FirstOrDefault().Value;
                return startMap;
            }
        }
        [SerializeField]
        private bool useOverrideStartPosition = false;
        [SerializeField]
        private Vector3 overrideStartPosition = Vector3.zero;
        public Vector3 StartPosition
        {
            get
            {
                return useOverrideStartPosition ? overrideStartPosition : StartMap.StartPosition;
            }
        }
        [SerializeField]
        private bool useOverrideStartRotation = false;
        [SerializeField]
        private Vector3 overrideStartRotation = Vector3.zero;
        public Vector3 StartRotation
        {
            get
            {
                return useOverrideStartRotation ? overrideStartRotation : StartMap.StartRotation;
            }
        }

        [System.NonSerialized]
        private Dictionary<BaseSkill, short> cacheSkillLevels = null;
        public override Dictionary<BaseSkill, short> CacheSkillLevels
        {
            get
            {
                if (cacheSkillLevels == null)
                    cacheSkillLevels = GameDataHelpers.CombineSkills(skillLevels, new Dictionary<BaseSkill, short>());
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
                    Logging.LogWarning(ToString(), "Right hand equipment is not weapon.");
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
                    Logging.LogWarning(ToString(), "Left hand equipment is not weapon or shield.");
                    leftHandEquipItem = null;
                    hasChanges = true;
                }
                else if (tempRightHandWeapon != null)
                {
                    if (tempLeftHandShield != null && tempRightHandWeapon.GetEquipType() == WeaponItemEquipType.TwoHand)
                    {
                        Logging.LogWarning(ToString(), "Cannot set left hand equipment because it's equipping `TwoHand` item.");
                        leftHandEquipItem = null;
                        hasChanges = true;
                    }
                    else if (tempLeftHandWeapon != null && tempRightHandWeapon.GetEquipType() != WeaponItemEquipType.DualWieldable)
                    {
                        Logging.LogWarning(ToString(), "Cannot set left hand equipment because it isn't equipping `DualWieldable` item.");
                        leftHandEquipItem = null;
                        hasChanges = true;
                    }
                    else if (tempLeftHandWeapon != null && tempLeftHandWeapon.GetEquipType() == WeaponItemEquipType.OffHandOnly)
                    {
                        Logging.LogWarning(ToString(), "Cannot set right hand equipment because it's equipping `OffHandOnly` item.");
                        rightHandEquipItem = null;
                        hasChanges = true;
                    }
                }
                if (tempLeftHandWeapon != null &&
                    (tempLeftHandWeapon.GetEquipType() == WeaponItemEquipType.MainHandOnly ||
                    tempLeftHandWeapon.GetEquipType() == WeaponItemEquipType.TwoHand))
                {
                    Logging.LogWarning(ToString(), "Left hand weapon cannot be `MainHandOnly` or `TwoHand` item.");
                    leftHandEquipItem = null;
                    hasChanges = true;
                }
                if (tempRightHandWeapon != null &&
                    (tempRightHandWeapon.GetEquipType() == WeaponItemEquipType.OffHandOnly))
                {
                    Logging.LogWarning(ToString(), "Right hand weapon cannot be `OffHandOnly` item.");
                    rightHandEquipItem = null;
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
            GameInstance.AddItems(armorItems);
            GameInstance.AddItems(rightHandEquipItem);
            GameInstance.AddItems(leftHandEquipItem);
        }
    }
}
