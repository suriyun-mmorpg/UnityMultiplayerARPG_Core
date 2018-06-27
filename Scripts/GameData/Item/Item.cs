using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public enum ItemType
    {
        Junk,
        Armor,
        Weapon,
        Shield,
        Potion,
        Ammo,
        Building,
    }

    [CreateAssetMenu(fileName = "Item", menuName = "Create GameData/Item")]
    public class Item : BaseGameData
    {
        public ItemType itemType;
        public GameObject dropModel;
        public int sellPrice;
        public float weight;
        [Range(1, 1000)]
        public short maxStack = 1;
        [Range(1, 30)]
        public short maxLevel = 1;

        // Armor
        public ArmorType armorType;

        // Weapon
        public WeaponType weaponType;
        public DamageIncremental damageAmount;
        public IncrementalMinMaxFloat harvestDamageAmount;

        // Equipment
        public EquipmentModel[] equipmentModels;
        [Tooltip("This will be available with `Weapon` item, set it in case that it will be equipped at left hand")]
        public EquipmentModel[] subEquipmentModels;
        public EquipmentRequirement requirement;
        public AttributeIncremental[] increaseAttributes;
        public ResistanceIncremental[] increaseResistances;
        public DamageIncremental[] increaseDamages;
        public CharacterStatsIncremental increaseStats;
        [Tooltip("Equipment durability, If this set to 0 it will not broken")]
        [Range(0f, 1000f)]
        public float maxDurability;
        [Tooltip("If this is TRUE, your equipment will be destroyed when durability = 0")]
        public bool destroyIfBroken;

        // Potion
        public Buff buff;

        // Ammo
        public AmmoType ammoType;

        // Building
        public BuildingObject buildingObject;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            // Equipment max stack always equals to 1
            switch (itemType)
            {
                case ItemType.Armor:
                case ItemType.Weapon:
                case ItemType.Shield:
                    maxStack = 1;
                    break;
                case ItemType.Junk:
                case ItemType.Potion:
                case ItemType.Ammo:
                case ItemType.Building:
                    maxLevel = 1;
                    break;
            }
            EditorUtility.SetDirty(this);
        }
#endif

        public bool IsEquipment()
        {
            return IsArmor() || IsShield() || IsWeapon();
        }

        public bool IsDefendEquipment()
        {
            return IsArmor() || IsShield();
        }

        public bool IsJunk()
        {
            return itemType == ItemType.Junk;
        }

        public bool IsArmor()
        {
            return itemType == ItemType.Armor;
        }

        public bool IsShield()
        {
            return itemType == ItemType.Shield;
        }

        public bool IsWeapon()
        {
            return itemType == ItemType.Weapon;
        }

        public bool IsPotion()
        {
            return itemType == ItemType.Potion;
        }

        public bool IsAmmo()
        {
            return itemType == ItemType.Ammo;
        }

        public bool IsBuilding()
        {
            return itemType == ItemType.Building;
        }

        #region Cache Data
        private Dictionary<Attribute, short> cacheRequireAttributeAmounts;
        public Dictionary<Attribute, short> CacheRequireAttributeAmounts
        {
            get
            {
                if (cacheRequireAttributeAmounts == null)
                    cacheRequireAttributeAmounts = GameDataHelpers.MakeAttributeAmountsDictionary(requirement.attributeAmounts, new Dictionary<Attribute, short>(), 1f);
                return cacheRequireAttributeAmounts;
            }
        }

        public ArmorType ArmorType
        {
            get
            {
                if (armorType == null)
                    armorType = gameInstance.DefaultArmorType;
                return armorType;
            }
        }

        public string EquipPosition
        {
            get { return ArmorType.Id; }
        }

        public WeaponType WeaponType
        {
            get
            {
                if (weaponType == null)
                    weaponType = gameInstance.DefaultWeaponType;
                return weaponType;
            }
        }

        public WeaponItemEquipType EquipType
        {
            get { return WeaponType.equipType; }
        }
        #endregion
    }

    [System.Serializable]
    public struct EquipmentModel
    {
        public string equipSocket;
        public GameObject model;
    }

    [System.Serializable]
    public struct ItemAmount
    {
        public Item item;
        public short amount;
    }

    [System.Serializable]
    public struct ItemDrop
    {
        public Item item;
        public short amount;
        [Range(0f, 1f)]
        public float dropRate;
    }

    [System.Serializable]
    public struct ItemDropByWeight
    {
        public Item item;
        public float amountPerDamage;
        public int randomWeight;
    }

    [System.Serializable]
    public struct EquipmentRequirement
    {
        public PlayerCharacter character;
        public short level;
        public AttributeAmount[] attributeAmounts;
    }
}
