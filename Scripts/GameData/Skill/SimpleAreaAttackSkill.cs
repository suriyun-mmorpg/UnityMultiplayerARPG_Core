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
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
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
                entity.Setup(skillUser.GetInfo(), weapon, simulateSeed, triggerIndex, spreadIndex, damageAmounts, this, skillLevel, hitRegData, areaDuration.GetAmount(skillLevel), applyDuration.GetAmount(skillLevel));
                BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            }
            // Teleport to aim position
            if (isWarpToAimPosition)
                skillUser.Teleport(aimPosition.position, skillUser.MovementTransform.rotation, false);
        }

        public override KeyValuePair<DamageElement, MinMaxFloat> GetBaseAttackDamageAmount(ICharacterData skillUser, int skillLevel, bool isLeftHand)
        {
            switch (skillAttackType)
            {
                case SkillAttackType.Normal:
                    return GameDataHelpers.GetDamageWithEffectiveness(CacheEffectivenessAttributes, skillUser.GetCaches().Attributes, damageAmount.ToKeyValuePair(skillLevel, 1f));
                case SkillAttackType.BasedOnWeapon:
                    if (isLeftHand && skillUser.GetCaches().LeftHandWeaponDamage.HasValue)
                        return skillUser.GetCaches().LeftHandWeaponDamage.Value;
                    return skillUser.GetCaches().RightHandWeaponDamage.Value;
            }
            return new KeyValuePair<DamageElement, MinMaxFloat>();
        }

        public override Dictionary<DamageElement, float> GetAttackWeaponDamageInflictions(ICharacterData skillUser, int skillLevel)
        {
            return GameDataHelpers.CombineDamageInflictions(weaponDamageInflictions, new Dictionary<DamageElement, float>(), skillLevel);
        }

        public override Dictionary<DamageElement, MinMaxFloat> GetAttackAdditionalDamageAmounts(ICharacterData skillUser, int skillLevel)
        {
            return GameDataHelpers.CombineDamages(additionalDamageAmounts, new Dictionary<DamageElement, MinMaxFloat>(), skillLevel, 1f);
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

        public override bool IsDebuff
        {
            get { return isDebuff; }
        }

        public override Buff Debuff
        {
            get
            {
                if (!IsDebuff)
                    return Buff.Empty;
                return debuff;
            }
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddStatusEffects(attackStatusEffects);
            areaDamageEntity.InitPrefab();
            GameInstance.AddOtherNetworkObjects(areaDamageEntity.Identity);
        }

        public override void OnSkillAttackHit(int skillLevel, EntityInfo instigator, CharacterItem weapon, BaseCharacterEntity target)
        {
            base.OnSkillAttackHit(skillLevel, instigator, weapon, target);
            attackStatusEffects.ApplyStatusEffect(skillLevel, instigator, weapon, target);
        }
    }
}
