using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseEquipmentItem : BaseItem, IEquipmentItem
    {
        [SerializeField]
        private EquipmentRequirement requirement;
        public EquipmentRequirement Requirement
        {
            get { return requirement; }
        }

        [System.NonSerialized]
        private Dictionary<Attribute, float> cacheRequireAttributeAmounts;
        public Dictionary<Attribute, float> RequireAttributeAmounts
        {
            get
            {
                if (cacheRequireAttributeAmounts == null)
                    cacheRequireAttributeAmounts = GameDataHelpers.CombineAttributes(requirement.attributeAmounts, new Dictionary<Attribute, float>(), 1f);
                return cacheRequireAttributeAmounts;
            }
        }

        [SerializeField]
        private EquipmentSet equipmentSet;
        public EquipmentSet EquipmentSet
        {
            get { return equipmentSet; }
        }

        [SerializeField]
        private float maxDurability;
        public float MaxDurability
        {
            get { return maxDurability; }
        }

        [SerializeField]
        private bool destroyIfBroken;
        public bool DestroyIfBroken
        {
            get { return destroyIfBroken; }
        }

        [SerializeField]
        private byte maxSocket;
        public byte MaxSocket
        {
            get { return maxSocket; }
        }

        [SerializeField]
        private EquipmentModel[] equipmentModels;
        public EquipmentModel[] EquipmentModels
        {
            get { return equipmentModels; }
        }

        [SerializeField]
        private CharacterStatsIncremental increaseStats;
        public CharacterStatsIncremental IncreaseStats
        {
            get { return increaseStats; }
        }

        [SerializeField]
        private CharacterStatsIncremental increaseStatsRate;
        public CharacterStatsIncremental IncreaseStatsRate
        {
            get { return increaseStatsRate; }
        }

        [SerializeField]
        private AttributeIncremental[] increaseAttributes;
        public AttributeIncremental[] IncreaseAttributes
        {
            get { return increaseAttributes; }
        }

        [SerializeField]
        private AttributeIncremental[] increaseAttributesRate;
        public AttributeIncremental[] IncreaseAttributesRate
        {
            get { return increaseAttributesRate; }
        }

        [SerializeField]
        private ResistanceIncremental[] increaseResistances;
        public ResistanceIncremental[] IncreaseResistances
        {
            get { return increaseResistances; }
        }

        [SerializeField]
        private ArmorIncremental[] increaseArmors;
        public ArmorIncremental[] IncreaseArmors
        {
            get { return increaseArmors; }
        }

        [SerializeField]
        private DamageIncremental[] increaseDamages;
        public DamageIncremental[] IncreaseDamages
        {
            get { return increaseDamages; }
        }

        [SerializeField]
        private SkillLevel[] increaseSkillLevels;
        public SkillLevel[] IncreaseSkillLevels
        {
            get { return increaseSkillLevels; }
        }
    }
}
