using Insthync.UnityEditorUtils;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseCharacter : BaseGameData
    {
        [Category("Generic Settings")]
        [SerializeField]
        protected ExpTable expTable;
        public ExpTable ExpTable
        {
            get
            {
                if (expTable == null)
                    return GameInstance.Singleton.ExpTable;
                return expTable;
            }
        }
        
        [Category(3, "Character Stats")]
        [SerializeField]
        private CharacterStatsIncremental stats;
        public virtual CharacterStatsIncremental Stats { get { return stats; } set { stats = value; } }

        [SerializeField]
        [ArrayElementTitle("attribute")]
        private AttributeIncremental[] attributes;
        public virtual AttributeIncremental[] Attributes { get { return attributes; } set { attributes = value; } }

        [SerializeField]
        [ArrayElementTitle("damageElement")]
        private ResistanceIncremental[] resistances;
        public virtual ResistanceIncremental[] Resistances { get { return resistances; } set { resistances = value; } }

        [SerializeField]
        [ArrayElementTitle("damageElement")]
        private ArmorIncremental[] armors;
        public virtual ArmorIncremental[] Armors { get { return armors; } set { armors = value; } }

        [SerializeField]
        [ArrayElementTitle("statusEffect")]
        private StatusEffectResistanceIncremental[] statusEffectResistances;
        public virtual StatusEffectResistanceIncremental[] StatusEffectResistances { get { return statusEffectResistances; } set { statusEffectResistances = value; } }

        [Tooltip("Status effects that can be applied to the attacker when attacking.")]
        [SerializeField]
        [ArrayElementTitle("statusEffect")]
        private StatusEffectApplying[] selfStatusEffectsWhenAttacking = new StatusEffectApplying[0];
        public StatusEffectApplying[] SelfStatusEffectsWhenAttacking { get { return selfStatusEffectsWhenAttacking; } set { selfStatusEffectsWhenAttacking = value; } }

        [Tooltip("Status effects that can be applied to the enemy when attacking.")]
        [SerializeField]
        [ArrayElementTitle("statusEffect")]
        private StatusEffectApplying[] enemyStatusEffectsWhenAttacking = new StatusEffectApplying[0];
        public StatusEffectApplying[] EnemyStatusEffectsWhenAttacking { get { return enemyStatusEffectsWhenAttacking; } set { enemyStatusEffectsWhenAttacking = value; } }

        [Tooltip("Status effects that can be applied to the attacker when attacked.")]
        [SerializeField]
        [ArrayElementTitle("statusEffect")]
        private StatusEffectApplying[] selfStatusEffectsWhenAttacked = new StatusEffectApplying[0];
        public StatusEffectApplying[] SelfStatusEffectsWhenAttacked { get { return selfStatusEffectsWhenAttacked; } set { selfStatusEffectsWhenAttacked = value; } }

        [Tooltip("Status effects that can be applied to the enemy when attacked.")]
        [SerializeField]
        [ArrayElementTitle("statusEffect")]
        private StatusEffectApplying[] enemyStatusEffectsWhenAttacked = new StatusEffectApplying[0];
        public StatusEffectApplying[] EnemyStatusEffectsWhenAttacked { get { return enemyStatusEffectsWhenAttacked; } set { enemyStatusEffectsWhenAttacked = value; } }

        public CharacterStats GetCharacterStats(int level)
        {
            return Stats.GetCharacterStats(level);
        }

        public void GetCharacterAttributes(int level, Dictionary<Attribute, float> result)
        {
            result.Clear();
            GameDataHelpers.CombineAttributes(Attributes, result, level, 1f);
        }

        public void GetCharacterResistances(int level, Dictionary<DamageElement, float> result)
        {
            result.Clear();
            GameDataHelpers.CombineResistances(Resistances, result, level, 1f);
        }

        public void GetCharacterArmors(int level, Dictionary<DamageElement, float> result)
        {
            result.Clear();
            GameDataHelpers.CombineArmors(Armors, result, level, 1f);
        }

        public void GetCharacterStatusEffectResistances(int level, Dictionary<StatusEffect, float> result)
        {
            result.Clear();
            GameDataHelpers.CombineStatusEffectResistances(StatusEffectResistances, result, level, 1f);
        }

        public void ApplySelfStatusEffectsWhenAttacking(int level, EntityInfo applier, BaseCharacterEntity target)
        {
            if (level <= 0 || target == null)
                return;
            selfStatusEffectsWhenAttacking.ApplyStatusEffect(level, applier, CharacterItem.Empty, target);
        }

        public void ApplyEnemyStatusEffectsWhenAttacking(int level, EntityInfo applier, BaseCharacterEntity target)
        {
            if (level <= 0 || target == null)
                return;
            enemyStatusEffectsWhenAttacking.ApplyStatusEffect(level, applier, CharacterItem.Empty, target);
        }

        public void ApplySelfStatusEffectsWhenAttacked(int level, EntityInfo applier, BaseCharacterEntity target)
        {
            if (level <= 0 || target == null)
                return;
            selfStatusEffectsWhenAttacked.ApplyStatusEffect(level, applier, CharacterItem.Empty, target);
        }

        public void ApplyEnemyStatusEffectsWhenAttacked(int level, EntityInfo applier, BaseCharacterEntity target)
        {
            if (level <= 0 || target == null)
                return;
            enemyStatusEffectsWhenAttacked.ApplyStatusEffect(level, applier, CharacterItem.Empty, target);
        }

        public abstract HashSet<int> GetLearnableSkillDataIds();

        public abstract void GetSkillLevels(int level, Dictionary<BaseSkill, int> result);

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddAttributes(Attributes);
            GameInstance.AddDamageElements(Resistances);
            GameInstance.AddDamageElements(Armors);
            GameInstance.AddStatusEffects(StatusEffectResistances);
        }
    }
}
