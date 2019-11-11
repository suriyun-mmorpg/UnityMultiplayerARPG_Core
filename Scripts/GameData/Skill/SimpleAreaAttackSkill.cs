using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Create GameData/Skill/Simple Area Attack Skill", order = -4988)]
    public partial class SimpleAreaAttackSkill : BaseAreaSkill
    {
        public enum SkillAttackType : byte
        {
            Normal,
            BasedOnWeapon,
        }

        public SkillAttackType skillAttackType;
        public AreaDamageEntity areaDamageEntity;
        public GameEffectCollection hitEffects;
        public DamageIncremental damageAmount;
        public DamageEffectivenessAttribute[] effectivenessAttributes;
        public DamageInflictionIncremental[] weaponDamageInflictions;
        public DamageIncremental[] additionalDamageAmounts;
        public bool increaseDamageAmountsWithBuffs;
        public bool isDebuff;
        public Buff debuff;

        private Dictionary<Attribute, float> cacheEffectivenessAttributes;
        public Dictionary<Attribute, float> CacheEffectivenessAttributes
        {
            get
            {
                if (cacheEffectivenessAttributes == null)
                    cacheEffectivenessAttributes = GameDataHelpers.CombineDamageEffectivenessAttributes(effectivenessAttributes, new Dictionary<Attribute, float>());
                return cacheEffectivenessAttributes;
            }
        }

        public override void ApplySkill(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand, CharacterItem weapon, int hitIndex, Dictionary<DamageElement, MinMaxFloat> damageAmounts, Vector3 aimPosition)
        {
            // Spawn area entity
            aimPosition = AreaSkillControls.FindGround(AreaSkillControls.ValidateDistance(skillUser.CacheTransform.position, aimPosition, castDistance.GetAmount(skillLevel)));
            AreaDamageEntity damageEntity = Instantiate(areaDamageEntity, aimPosition, skillUser.GetSummonRotation());
            damageEntity.Setup(skillUser, weapon, GetAttackDamages(skillUser, skillLevel, isLeftHand), this, skillLevel, areaDuration.GetAmount(skillLevel), applyDuration.GetAmount(skillLevel));
        }

        public override KeyValuePair<DamageElement, MinMaxFloat> GetBaseAttackDamageAmount(ICharacterData skillUser, short skillLevel, bool isLeftHand)
        {
            switch (skillAttackType)
            {
                case SkillAttackType.Normal:
                    return GameDataHelpers.MakeDamage(damageAmount, skillLevel, 1f, GetEffectivenessDamage(skillUser));
                case SkillAttackType.BasedOnWeapon:
                    return skillUser.GetWeaponDamage(ref isLeftHand);
            }
            return new KeyValuePair<DamageElement, MinMaxFloat>();
        }

        public override Dictionary<DamageElement, float> GetAttackWeaponDamageInflictions(ICharacterData skillUser, short skillLevel)
        {
            return GameDataHelpers.CombineDamageInflictions(weaponDamageInflictions, new Dictionary<DamageElement, float>(), skillLevel);
        }

        public override Dictionary<DamageElement, MinMaxFloat> GetAttackAdditionalDamageAmounts(ICharacterData skillUser, short skillLevel)
        {
            return GameDataHelpers.CombineDamages(additionalDamageAmounts, new Dictionary<DamageElement, MinMaxFloat>(), skillLevel, 1f);
        }

        public override bool IsIncreaseAttackDamageAmountsWithBuffs(ICharacterData skillUser, short skillLevel)
        {
            return increaseDamageAmountsWithBuffs;
        }

        protected float GetEffectivenessDamage(ICharacterData skillUser)
        {
            return GameDataHelpers.GetEffectivenessDamage(CacheEffectivenessAttributes, skillUser);
        }

        public override bool IsAttack()
        {
            return true;
        }

        public override bool IsBuff()
        {
            return false;
        }

        public override bool IsDebuff()
        {
            return isDebuff;
        }

        public sealed override Buff GetDebuff()
        {
            if (!IsDebuff())
                return default(Buff);
            return debuff;
        }

        public sealed override GameEffectCollection GetHitEffect()
        {
            return hitEffects;
        }
    }
}
