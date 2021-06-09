using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseEquipmentItem : BaseItem, IEquipmentItem
    {
        [Header("Equipment Configs")]
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
            set { equipmentModels = value; }
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

        [SerializeField]
        private StatusEffectApplying[] selfStatusEffectsWhenAttacking;
        public StatusEffectApplying[] SelfStatusEffectsWhenAttacking
        {
            get { return selfStatusEffectsWhenAttacking; }
        }

        [SerializeField]
        private StatusEffectApplying[] enemyStatusEffectsWhenAttacking;
        public StatusEffectApplying[] EnemyStatusEffectsWhenAttacking
        {
            get { return enemyStatusEffectsWhenAttacking; }
        }

        [SerializeField]
        private StatusEffectApplying[] selfStatusEffectsWhenAttacked;
        public StatusEffectApplying[] SelfStatusEffectsWhenAttacked
        {
            get { return selfStatusEffectsWhenAttacked; }
        }

        [SerializeField]
        private StatusEffectApplying[] enemyStatusEffectsWhenAttacked;
        public StatusEffectApplying[] EnemyStatusEffectsWhenAttacked
        {
            get { return enemyStatusEffectsWhenAttacked; }
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddAttributes(IncreaseAttributes);
            GameInstance.AddAttributes(IncreaseAttributesRate);
            GameInstance.AddDamageElements(IncreaseResistances);
            GameInstance.AddDamageElements(IncreaseArmors);
            GameInstance.AddDamageElements(IncreaseDamages);
            GameInstance.AddSkills(IncreaseSkillLevels);
            GameInstance.AddStatusEffects(SelfStatusEffectsWhenAttacking);
            GameInstance.AddStatusEffects(EnemyStatusEffectsWhenAttacking);
            GameInstance.AddStatusEffects(SelfStatusEffectsWhenAttacked);
            GameInstance.AddStatusEffects(EnemyStatusEffectsWhenAttacked);
            GameInstance.AddEquipmentSets(EquipmentSet);
            GameInstance.AddPoolingWeaponLaunchEffects(EquipmentModels);
        }
    }
}
