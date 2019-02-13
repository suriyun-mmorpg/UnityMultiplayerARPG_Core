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
        Pet,
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
        public AttributeIncremental[] increaseAttributes;
        public ResistanceIncremental[] increaseResistances;
        public DamageIncremental[] increaseDamages;
        public CharacterStatsIncremental increaseStats;
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

        #region Item refining
        public bool CanRefine(IPlayerCharacterData character, short level)
        {
            GameMessage.Type gameMessageType;
            return CanRefine(character, level, out gameMessageType);
        }

        public bool CanRefine(IPlayerCharacterData character, short level, out GameMessage.Type gameMessageType)
        {
            gameMessageType = GameMessage.Type.CannotRefine;
            if (!IsEquipment())
            {
                // Cannot refine because it's not equipment item
                return false;
            }
            if (itemRefineInfo == null)
            {
                // Cannot refine because there is no item refine info
                return false;
            }
            if (level >= itemRefineInfo.levels.Length)
            {
                // Cannot refine because item reached max level
                gameMessageType = GameMessage.Type.RefineItemReachedMaxLevel;
                return false;
            }
            return itemRefineInfo.levels[level - 1].CanRefine(character, out gameMessageType);
        }

        public static void RefineRightHandItem(IPlayerCharacterData character, out GameMessage.Type gameMessageType)
        {
            RefineItem(character, character.EquipWeapons.rightHand, (refinedItem) =>
            {
                character.EquipWeapons.rightHand = refinedItem;
            }, () =>
            {
                character.EquipWeapons.rightHand = CharacterItem.Empty;
            }, out gameMessageType);
        }

        public static void RefineLeftHandItem(IPlayerCharacterData character, out GameMessage.Type gameMessageType)
        {
            RefineItem(character, character.EquipWeapons.leftHand, (refinedItem) =>
            {
                character.EquipWeapons.leftHand = refinedItem;
            }, () =>
            {
                character.EquipWeapons.rightHand = CharacterItem.Empty;
            }, out gameMessageType);
        }

        public static void RefineEquipItem(IPlayerCharacterData character, int index, out GameMessage.Type gameMessageType)
        {
            RefineItemByList(character, character.EquipItems, index, out gameMessageType);
        }

        public static void RefineNonEquipItem(IPlayerCharacterData character, int index, out GameMessage.Type gameMessageType)
        {
            RefineItemByList(character, character.NonEquipItems, index, out gameMessageType);
        }

        private static void RefineItemByList(IPlayerCharacterData character, IList<CharacterItem> list, int index, out GameMessage.Type gameMessageType)
        {
            RefineItem(character, list[index], (refinedItem) =>
            {
                list[index] = refinedItem;
            }, () =>
            {
                list.RemoveAt(index);
            }, out gameMessageType);
        }

        private static void RefineItem(IPlayerCharacterData character, CharacterItem refiningItem, System.Action<CharacterItem> onRefine, System.Action onDestroy, out GameMessage.Type gameMessageType)
        {
            gameMessageType = GameMessage.Type.CannotRefine;
            if (!refiningItem.IsValid())
            {
                // Cannot refine because character item is empty
                return;
            }
            Item equipmentItem = refiningItem.GetEquipmentItem();
            if (equipmentItem == null)
            {
                // Cannot refine because it's not equipment item
                return;
            }
            if (!equipmentItem.CanRefine(character, refiningItem.level, out gameMessageType))
            {
                // Cannot refine because of some reasons
                return;
            }
            ItemRefineLevel refineLevel = equipmentItem.itemRefineInfo.levels[refiningItem.level - 1];
            if (Random.value <= refineLevel.SuccessRate)
            {
                // If success, increase item level
                gameMessageType = GameMessage.Type.RefineSuccess;
                ++refiningItem.level;
                onRefine.Invoke(refiningItem);
            }
            else
            {
                // Fail
                gameMessageType = GameMessage.Type.RefineFail;
                if (refineLevel.RefineFailDestroyItem)
                {
                    // If condition when fail is it has to be destroyed
                    onDestroy.Invoke();
                }
                else
                {
                    // If condition when fail is reduce its level
                    refiningItem.level -= refineLevel.RefineFailDecreaseLevels;
                    if (refiningItem.level < 1)
                        refiningItem.level = 1;
                    onRefine.Invoke(refiningItem);
                }
            }
            if (refineLevel.RequireItemsArray != null)
            {
                // Decrease required items
                foreach (ItemAmount requireItem in refineLevel.RequireItemsArray)
                {
                    if (requireItem.item != null && requireItem.amount > 0)
                        character.DecreaseItems(requireItem.item.DataId, requireItem.amount);
                }
            }
            // Decrease required gold
            character.Gold -= refineLevel.RequireGold;
        }
        #endregion

        #region Item repair
        public bool CanRepair(IPlayerCharacterData character, float durability, out int requireGold)
        {
            GameMessage.Type gameMessageType;
            return CanRepair(character, durability, out requireGold, out gameMessageType);
        }

        public bool CanRepair(IPlayerCharacterData character, float durability, out int requireGold, out GameMessage.Type gameMessageType)
        {
            requireGold = 0;
            gameMessageType = GameMessage.Type.CannotRepair;
            if (!IsEquipment())
            {
                // Cannot repair because it's not equipment item
                return false;
            }
            if (itemRefineInfo == null)
            {
                // Cannot repair because there is no item refine info
                return false;
            }
            float durabilityRate = durability / maxDurability;
            foreach (ItemRepairPrice repairPrice in itemRefineInfo.repairPrices)
            {
                if (durabilityRate < repairPrice.DurabilityRate)
                {
                    requireGold = repairPrice.RequireGold;
                    return repairPrice.CanRepair(character, out gameMessageType);
                }
            }
            return true;
        }

        public static void RepairRightHandItem(IPlayerCharacterData character, out GameMessage.Type gameMessageType)
        {
            RepairItem(character, character.EquipWeapons.rightHand, (repairedItem) =>
            {
                character.EquipWeapons.rightHand = repairedItem;
            }, out gameMessageType);
        }

        public static void RepairLeftHandItem(IPlayerCharacterData character, out GameMessage.Type gameMessageType)
        {
            RepairItem(character, character.EquipWeapons.leftHand, (repairedItem) =>
            {
                character.EquipWeapons.leftHand = repairedItem;
            }, out gameMessageType);
        }

        public static void RepairEquipItem(IPlayerCharacterData character, int index, out GameMessage.Type gameMessageType)
        {
            RepairItemByList(character, character.EquipItems, index, out gameMessageType);
        }

        public static void RepairNonEquipItem(IPlayerCharacterData character, int index, out GameMessage.Type gameMessageType)
        {
            RepairItemByList(character, character.NonEquipItems, index, out gameMessageType);
        }

        private static void RepairItemByList(IPlayerCharacterData character, IList<CharacterItem> list, int index, out GameMessage.Type gameMessageType)
        {
            RepairItem(character, list[index], (repairedItem) =>
            {
                list[index] = repairedItem;
            }, out gameMessageType);
        }

        private static void RepairItem(IPlayerCharacterData character, CharacterItem repairingItem, System.Action<CharacterItem> onRepaired, out GameMessage.Type gameMessageType)
        {
            gameMessageType = GameMessage.Type.CannotRepair;
            if (!repairingItem.IsValid())
            {
                // Cannot refine because character item is empty
                return;
            }
            Item equipmentItem = repairingItem.GetEquipmentItem();
            if (equipmentItem == null)
            {
                // Cannot refine because it's not equipment item
                return;
            }
            int requireGold = 0;
            if (equipmentItem.CanRepair(character, repairingItem.durability, out requireGold, out gameMessageType))
            {
                gameMessageType = GameMessage.Type.RepairSuccess;
                // Repair item
                repairingItem.durability = equipmentItem.maxDurability;
                onRepaired.Invoke(repairingItem);
                // Decrease required gold
                character.Gold -= requireGold;
            }
        }
        #endregion

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
                    cacheRequireAttributeAmounts = GameDataHelpers.MakeAttributes(requirement.attributeAmounts, new Dictionary<Attribute, short>(), 1f);
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
}
