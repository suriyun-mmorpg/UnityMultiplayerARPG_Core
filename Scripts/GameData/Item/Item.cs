using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public enum ItemType : byte
    {
        Junk,
        Armor,
        Weapon,
        Shield,
        Potion,
        Ammo,
        Building,
        Pet,
        SocketEnhancer,
    }

    public enum FireType : byte
    {
        SingleFire,
        Automatic,
    }

    public enum WeaponAbility : byte
    {
        None,
        CanZoom,
    }

    [CreateAssetMenu(fileName = "Item", menuName = "Create GameData/Item")]
    public partial class Item : BaseGameData
    {
        public ItemType itemType;
        public GameObject dropModel;
        public int sellPrice;
        public float weight;
        [Range(1, 1000)]
        public short maxStack = 1;
        public ItemRefine itemRefineInfo;
        [Tooltip("This is duration to lock item at first time when pick up dropped item or bought it from NPC or IAP system")]
        public float lockDuration;

        // Armor
        public ArmorType armorType;

        // Weapon
        public WeaponType weaponType;
        public FireType fireType;
        [Tooltip("For macine gun may set this to 30 as magazine capacity, if this is 0 it will not need to have ammo loaded to shoot but still need ammo in inventory")]
        public short ammoCapacity;
        public Vector2 fireStagger;
        public byte fireSpread;
        public WeaponAbility weaponAbility;
        public float zoomFov;
        public bool disableRenderersOnZoom;
        public Sprite zoomCrosshair;
        public CrosshairSetting crosshairSetting = new CrosshairSetting()
        {
            spreadPowerWhileMoving = 3f,
            spreadPowerWhileAttacking = 5f,
            spreadDecreasePower = 2f,
            minSpread = 10f,
            maxSpread = 50f
        };
        [Range(0, 6)]
        public byte maxSocket;
        [Range(0f, 1f)]
        [Tooltip("This is move speed rate while attacking with this weapon")]
        public float moveSpeedRateWhileAttacking = 0f;
        public DamageIncremental damageAmount;
        public IncrementalMinMaxFloat harvestDamageAmount;

        // Equipment
        public EquipmentModel[] equipmentModels;
        [Tooltip("This will be available with `Weapon` item, set it in case that it will be equipped at left hand")]
        public EquipmentModel[] subEquipmentModels;
        public EquipmentRequirement requirement;
        public CharacterStatsIncremental increaseStats;
        public AttributeIncremental[] increaseAttributes;
        public ResistanceIncremental[] increaseResistances;
        public DamageIncremental[] increaseDamages;
        public SkillLevel[] increaseSkillLevels;
        public EquipmentSet equipmentSet;
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
        public BuildingEntity buildingEntity;

        // Pet
        public BaseMonsterCharacterEntity petEntity;

        // Socket Enhancer
        public EquipmentBonus socketEnhanceEffect;

        public override string Title
        {
            get
            {
                if (itemRefineInfo == null)
                    return base.Title;
                return "<color=#" + ColorUtility.ToHtmlStringRGB(itemRefineInfo.titleColor) + ">" + base.Title + "</color>";
            }
        }

        public string RarityTitle
        {
            get
            {
                if (itemRefineInfo == null)
                    return "Normal";
                return "<color=#" + ColorUtility.ToHtmlStringRGB(itemRefineInfo.titleColor) + ">" + itemRefineInfo.title + "</color>";
            }
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            bool hasChanges = false;
            // Equipment / Pet max stack always equals to 1
            switch (itemType)
            {
                case ItemType.Armor:
                case ItemType.Weapon:
                case ItemType.Shield:
                case ItemType.Pet:
                    if (maxStack != 1)
                    {
                        maxStack = 1;
                        hasChanges = true;
                    }
                    break;
            }
            // Mark asset to be dirty when chagnes occured
            if (hasChanges)
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

        public bool IsPet()
        {
            return itemType == ItemType.Pet;
        }

        public bool IsSocketEnhancer()
        {
            return itemType == ItemType.SocketEnhancer;
        }

        public int MaxLevel
        {
            get
            {
                if (itemRefineInfo == null || itemRefineInfo.levels == null || itemRefineInfo.levels.Length == 0)
                    return 1;
                return itemRefineInfo.levels.Length;
            }
        }

        #region Cache Data
        private Dictionary<Attribute, short> cacheRequireAttributeAmounts;
        public Dictionary<Attribute, short> CacheRequireAttributeAmounts
        {
            get
            {
                if (cacheRequireAttributeAmounts == null)
                    cacheRequireAttributeAmounts = GameDataHelpers.CombineAttributes(requirement.attributeAmounts, new Dictionary<Attribute, short>(), 1f);
                return cacheRequireAttributeAmounts;
            }
        }

        public ArmorType ArmorType
        {
            get
            {
                if (armorType == null && gameInstance != null)
                    armorType = gameInstance.DefaultArmorType;
                return armorType;
            }
        }

        public string EquipPosition
        {
            get { return ArmorType == null ? string.Empty : ArmorType.Id; }
        }

        public WeaponType WeaponType
        {
            get
            {
                if (weaponType == null && gameInstance != null)
                    weaponType = gameInstance.DefaultWeaponType;
                return weaponType;
            }
        }

        public WeaponItemEquipType EquipType
        {
            get { return WeaponType == null ? WeaponItemEquipType.OneHand : WeaponType.equipType; }
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

    [System.Serializable]
    public struct EquipmentBonus
    {
        public CharacterStats stats;
        public AttributeAmount[] attributes;
        public ResistanceAmount[] resistances;
        public DamageAmount[] damages;
        public SkillLevel[] skills;
    }

    [System.Serializable]
    public struct CrosshairSetting
    {
        public float spreadPowerWhileMoving;
        public float spreadPowerWhileAttacking;
        public float spreadDecreasePower;
        public float minSpread;
        public float maxSpread;
    }
}
