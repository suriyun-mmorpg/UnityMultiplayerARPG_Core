using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.SIMPLE_AREA_ATTACK_SKILL_FILE, menuName = GameDataMenuConsts.SIMPLE_AREA_ATTACK_SKILL_MENU, order = GameDataMenuConsts.SIMPLE_AREA_ATTACK_SKILL_ORDER)]
    public partial class SimpleAreaAttackSkill : BaseAreaSkill
    {
        public enum SkillAttackType : byte
        {
            Normal,
            BasedOnWeapon,
        }
        [Category("Area Settings")]
        public AreaDamageEntity areaDamageEntity;

        [Category(3, "Attacking")]
        public SkillAttackType skillAttackType;
        public DamageIncremental damageAmount;
        public DamageEffectivenessAttribute[] effectivenessAttributes;
        public DamageInflictionIncremental[] weaponDamageInflictions;
        public IncrementalFloat weaponDamageMultiplicator;
        public DamageIncremental[] additionalDamageAmounts;
        public bool increaseDamageAmountsWithBuffs;
        public bool isDebuff;
        public Buff debuff;
        public StatusEffectApplying[] attackStatusEffects;
        public HarvestType harvestType;
        public IncrementalMinMaxFloat harvestDamageAmount;
        public GameEffect[] damageHitEffects;

        [Category(4, "Warp Settings")]
        public bool isWarpToAimPosition;

        [System.NonSerialized]
        private Dictionary<Attribute, float> _cacheEffectivenessAttributes;
        public Dictionary<Attribute, float> CacheEffectivenessAttributes
        {
            get
            {
                if (_cacheEffectivenessAttributes == null)
                    _cacheEffectivenessAttributes = GameDataHelpers.CombineDamageEffectivenessAttributes(effectivenessAttributes, new Dictionary<Attribute, float>());
                return _cacheEffectivenessAttributes;
            }
        }

        public override GameEffect[] DamageHitEffects
        {
            get
            {
                return damageHitEffects;
            }
        }

        protected override void ApplySkillImplement(
            BaseCharacterEntity skillUser,
            int skillLevel,
            bool isLeftHand,
            CharacterItem weapon,
            int simulateSeed,
            byte triggerIndex,
            byte spreadIndex,
            List<Dictionary<DamageElement, MinMaxFloat>> damageAmounts,
            uint targetObjectId,
            AimPosition aimPosition)
        {
            if (BaseGameNetworkManager.Singleton.IsServer)
            {
                // Prepare hit reg data
                HitRegisterData hitRegData = new HitRegisterData()
                {
                    SimulateSeed = simulateSeed,
                    TriggerIndex = triggerIndex,
                    SpreadIndex = spreadIndex,
                    LaunchTimestamp = BaseGameNetworkManager.Singleton.Timestamp,
                    Origin = aimPosition.position,
                    Direction = aimPosition.direction,
                };

                // Spawn area entity
                // Aim position type always is `Position`
                LiteNetLibIdentity spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                    areaDamageEntity.Identity.HashAssetId,
                    aimPosition.position,
                    GameInstance.Singleton.GameplayRule.GetSummonRotation(skillUser));
                AreaDamageEntity entity = spawnObj.GetComponent<AreaDamageEntity>();
                entity.Setup(skillUser.GetInfo(), weapon, simulateSeed, triggerIndex, spreadIndex, damageAmounts[triggerIndex], this, skillLevel, hitRegData, areaDuration.GetAmount(skillLevel), applyDuration.GetAmount(skillLevel));
                BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            }
            // Teleport to aim position
            if (isWarpToAimPosition)
                skillUser.Teleport(aimPosition.position, skillUser.MovementTransform.rotation, false);
        }

        public override bool TryGetBaseAttackDamageAmount(ICharacterData skillUser, int skillLevel, bool isLeftHand, out KeyValuePair<DamageElement, MinMaxFloat> result)
        {
            switch (skillAttackType)
            {
                case SkillAttackType.Normal:
                    result = GameDataHelpers.GetDamageWithEffectiveness(CacheEffectivenessAttributes, skillUser.GetCaches().Attributes, damageAmount.ToKeyValuePair(skillLevel, 1f));
                    return true;
                case SkillAttackType.BasedOnWeapon:
                    if (isLeftHand && skillUser.GetCaches().LeftHandWeaponDamage.HasValue)
                    {
                        result = skillUser.GetCaches().LeftHandWeaponDamage.Value;
                        return true;
                    }
                    result = skillUser.GetCaches().RightHandWeaponDamage.Value;
                    return true;
            }
            return base.TryGetBaseAttackDamageAmount(skillUser, skillLevel, isLeftHand, out result);
        }

        public override bool TryGetAttackWeaponDamageInflictions(ICharacterData skillUser, int skillLevel, out Dictionary<DamageElement, float> result)
        {
            if (IsAttack)
            {
                result = GameDataHelpers.CombineDamageInflictions(weaponDamageInflictions, new Dictionary<DamageElement, float>(), skillLevel);
                return true;
            }
            return base.TryGetAttackWeaponDamageInflictions(skillUser, skillLevel, out result);
        }

        public override bool TryGetAttackWeaponDamageMultiplicator(ICharacterData skillUser, int skillLevel, out float result)
        {
            if (IsAttack)
            {
                result = weaponDamageMultiplicator.GetAmount(skillLevel);
                return result > 0f;
            }
            return base.TryGetAttackWeaponDamageMultiplicator(skillUser, skillLevel, out result);
        }

        public override bool TryGetAttackAdditionalDamageAmounts(ICharacterData skillUser, int skillLevel, out Dictionary<DamageElement, MinMaxFloat> result)
        {
            if (IsAttack)
            {
                result = GameDataHelpers.CombineDamages(additionalDamageAmounts, new Dictionary<DamageElement, MinMaxFloat>(), skillLevel, 1f);
                return true;
            }
            return base.TryGetAttackAdditionalDamageAmounts(skillUser, skillLevel, out result);
        }

        public override bool IsIncreaseAttackDamageAmountsWithBuffs(ICharacterData skillUser, int skillLevel)
        {
            return increaseDamageAmountsWithBuffs;
        }

        public override HarvestType HarvestType
        {
            get { return harvestType; }
        }

        public override IncrementalMinMaxFloat HarvestDamageAmount
        {
            get { return harvestDamageAmount; }
        }

        public override bool IsAttack
        {
            get { return true; }
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            areaDamageEntity.InitPrefab();
            GameInstance.AddOtherNetworkObjects(areaDamageEntity.Identity);
        }

        public override bool TryGetDebuff(out Buff debuff)
        {
            if (IsAttack && isDebuff)
            {
                debuff = this.debuff;
                return true;
            }
            return base.TryGetDebuff(out debuff);
        }

        public override bool TryGetAttackStatusEffectApplyings(out StatusEffectApplying[] statusEffectApplyings)
        {
            if (IsAttack)
            {
                statusEffectApplyings = attackStatusEffects;
                return true;
            }
            return base.TryGetAttackStatusEffectApplyings(out statusEffectApplyings);
        }
    }
}
