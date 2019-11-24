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
        Mount,
        AttributeIncrease,
        AttributeReset,
        Skill,
        SkillLearn,
        SkillReset,
    }

    public enum FireType : byte
    {
        SingleFire,
        Automatic,
    }

    [CreateAssetMenu(fileName = "Item", menuName = "Create GameData/Item", order = -4899)]
    public partial class Item : BaseGameData
    {
        [Header("Item Configs")]
        public ItemType itemType;
        public GameObject dropModel;
        public int sellPrice;
        public float weight;
        [Range(1, 1000)]
        public short maxStack = 1;
        public ItemRefine itemRefine;
        [Tooltip("This is duration to lock item at first time when pick up dropped item or bought it from NPC or IAP system")]
        public float lockDuration;

        [Header("Equipment Configs")]
        public EquipmentRequirement requirement;
        public EquipmentSet equipmentSet;
        [Tooltip("Equipment durability, If this set to 0 it will not broken")]
        [Range(0f, 1000f)]
        public float maxDurability;
        [Tooltip("If this is TRUE, your equipment will be destroyed when durability = 0")]
        public bool destroyIfBroken;
        [Range(0, 6)]
        public byte maxSocket;

        [Header("Armor/Shield Configs")]
        public ArmorType armorType;
        public ArmorIncremental armorAmount;

        [Header("Weapon Configs")]
        public WeaponType weaponType;
        public DamageIncremental damageAmount;
        public IncrementalMinMaxFloat harvestDamageAmount;
        [Range(0f, 1f)]
        [Tooltip("This is move speed rate while attacking with this weapon")]
        public float moveSpeedRateWhileAttacking = 0f;
        [Tooltip("For macine gun may set this to 30 as magazine capacity, if this is 0 it will not need to have ammo loaded to shoot but still need ammo in inventory")]
        public short ammoCapacity;
        public BaseWeaponAbility weaponAbility;
        public CrosshairSetting crosshairSetting = new CrosshairSetting()
        {
            expandPerFrameWhileMoving = 3f,
            expandPerFrameWhileAttacking = 5f,
            shrinkPerFrame = 8f,
            minSpread = 10f,
            maxSpread = 50f
        };

        [Header("Equipment Bonus Stats")]
        public CharacterStatsIncremental increaseStats;
        public CharacterStatsIncremental increaseStatsRate;
        [ArrayElementTitle("attribute", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public AttributeIncremental[] increaseAttributes;
        [ArrayElementTitle("attribute", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public AttributeIncremental[] increaseAttributesRate;
        [ArrayElementTitle("damageElement", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public ResistanceIncremental[] increaseResistances;
        [ArrayElementTitle("damageElement", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public ArmorIncremental[] increaseArmors;
        [ArrayElementTitle("damageElement", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public DamageIncremental[] increaseDamages;
        [ArrayElementTitle("skill", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public SkillLevel[] increaseSkillLevels;

        [Header("Fire Configs")]
        public FireType fireType;
        public Vector2 fireStagger;
        public byte fireSpread;

        [Header("Equip Models")]
        public EquipmentModel[] equipmentModels;
        [Tooltip("This will be available with `Weapon` item, set it in case that it will be equipped at left hand")]
        public EquipmentModel[] subEquipmentModels;

        [Header("Buff Configs")]
        public Buff buff;

        [Header("Ammo Configs")]
        public AmmoType ammoType;

        [Header("Building Configs")]
        public BuildingEntity buildingEntity;

        [Header("Pet Configs")]
        public BaseMonsterCharacterEntity petEntity;

        [Header("Socket Enhancer Configs")]
        public EquipmentBonus socketEnhanceEffect;

        [Header("Mount Configs")]
        public MountEntity mountEntity;

        [Header("Attribute Configs")]
        public AttributeAmount attributeAmount;

        [Header("Skill Configs")]
        public SkillLevel skillLevel;

        public override string Title
        {
            get
            {
                if (itemRefine == null)
                    return base.Title;
                return "<color=#" + ColorUtility.ToHtmlStringRGB(itemRefine.titleColor) + ">" + base.Title + "</color>";
            }
        }

        public string RarityTitle
        {
            get
            {
                if (itemRefine == null)
                    return "Normal";
                return "<color=#" + ColorUtility.ToHtmlStringRGB(itemRefine.titleColor) + ">" + itemRefine.Title + "</color>";
            }
        }

        public override bool Validate()
        {
            bool hasChanges = false;
            // Equipment / Pet max stack always equals to 1
            switch (itemType)
            {
                case ItemType.Armor:
                case ItemType.Weapon:
                case ItemType.Shield:
                case ItemType.Pet:
                case ItemType.Mount:
                    if (maxStack != 1)
                    {
                        maxStack = 1;
                        hasChanges = true;
                    }
                    break;
            }
            // Migrate character stats → armor (equipment)
            if (GameDataMigration.MigrateArmor(increaseStats, increaseArmors, out increaseStats, out increaseArmors))
                hasChanges = true;
            // Migrate character stats → armor (armor)
            if (GameDataMigration.MigrateBuffArmor(buff, out buff))
                hasChanges = true;
            return hasChanges;
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            // Add armor type
            GameInstance.AddArmorTypes(new ArmorType[] { armorType });
            // Add weapon type
            GameInstance.AddWeaponTypes(new WeaponType[] { weaponType });
            // Add building entity
            GameInstance.AddBuildingEntities(new BuildingEntity[] { buildingEntity });
            // Add pet entity
            GameInstance.AddCharacterEntities(new BaseCharacterEntity[] { petEntity });
            // Add mount entity
            GameInstance.AddMountEntities(new MountEntity[] { mountEntity });
            // Add skills
            List<SkillLevel> skillLevels = new List<SkillLevel>();
            if (increaseSkillLevels != null && increaseSkillLevels.Length > 0)
                skillLevels.AddRange(increaseSkillLevels);
            skillLevels.Add(skillLevel);
            GameInstance.AddSkillLevels(skillLevels);
        }

        public bool IsEquipment()
        {
            return IsArmor() || IsShield() || IsWeapon();
        }

        public bool IsUsable()
        {
            return IsPotion() || IsPet() || IsMount() || IsAttributeIncrease() || IsAttributeReset() || IsSkill() || IsSkillLearn() || IsSkillReset();
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

        public bool IsMount()
        {
            return itemType == ItemType.Mount;
        }

        public bool IsAttributeIncrease()
        {
            return itemType == ItemType.AttributeIncrease;
        }

        public bool IsAttributeReset()
        {
            return itemType == ItemType.AttributeReset;
        }

        public bool IsSkill()
        {
            return itemType == ItemType.Skill;
        }

        public bool IsSkillLearn()
        {
            return itemType == ItemType.SkillLearn;
        }

        public bool IsSkillReset()
        {
            return itemType == ItemType.SkillReset;
        }

        public int MaxLevel
        {
            get
            {
                if (itemRefine == null || itemRefine.levels == null || itemRefine.levels.Length == 0)
                    return 1;
                return itemRefine.levels.Length;
            }
        }
        
        [System.NonSerialized]
        private Dictionary<Attribute, float> cacheRequireAttributeAmounts;
        public Dictionary<Attribute, float> CacheRequireAttributeAmounts
        {
            get
            {
                if (cacheRequireAttributeAmounts == null)
                    cacheRequireAttributeAmounts = GameDataHelpers.CombineAttributes(requirement.attributeAmounts, new Dictionary<Attribute, float>(), 1f);
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

        public bool CanEquip(BaseCharacterEntity character, short level, out GameMessage.Type gameMessageType)
        {
            gameMessageType = GameMessage.Type.None;
            if (!IsEquipment() || character == null)
                return false;

            // Check is it pass attribute requirement or not
            Dictionary<Attribute, float> attributeAmountsDict = character.GetAttributes(true, false);
            Dictionary<Attribute, float> requireAttributeAmounts = CacheRequireAttributeAmounts;
            foreach (KeyValuePair<Attribute, float> requireAttributeAmount in requireAttributeAmounts)
            {
                if (!attributeAmountsDict.ContainsKey(requireAttributeAmount.Key) ||
                    attributeAmountsDict[requireAttributeAmount.Key] < requireAttributeAmount.Value)
                {
                    gameMessageType = GameMessage.Type.NotEnoughAttributeAmounts;
                    return false;
                }
            }

            // Check another requirements
            if (requirement.character != null && requirement.character != character.GetDatabase())
            {
                gameMessageType = GameMessage.Type.NotMatchCharacterClass;
                return false;
            }

            if (character.Level < requirement.level)
            {
                gameMessageType = GameMessage.Type.NotEnoughLevel;
                return false;
            }

            return true;
        }

        public bool CanAttack(BaseCharacterEntity character)
        {
            if (!IsWeapon() || character == null)
                return false;

            AmmoType requireAmmoType = WeaponType.requireAmmoType;
            return requireAmmoType == null || character.IndexOfAmmoItem(requireAmmoType) >= 0;
        }
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
        [ArrayElementTitle("attribute", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public AttributeAmount[] attributeAmounts;
    }

    [System.Serializable]
    public struct CrosshairSetting
    {
        public bool hidden;
        public float expandPerFrameWhileMoving;
        public float expandPerFrameWhileAttacking;
        public float shrinkPerFrame;
        public float minSpread;
        public float maxSpread;
    }
}
