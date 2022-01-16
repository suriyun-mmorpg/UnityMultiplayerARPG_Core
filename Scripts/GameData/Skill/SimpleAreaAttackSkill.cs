using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Simple Area Attack Skill", menuName = "Create GameData/Skill/Simple Area Attack Skill", order = -4988)]
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
        public HarvestType harvestType;
        public IncrementalMinMaxFloat harvestDamageAmount;

        [Category(4, "Warp Settings")]
        public bool isWarpToAimPosition;

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

        protected override void ApplySkillImplement(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand, CharacterItem weapon, int hitIndex, Dictionary<DamageElement, MinMaxFloat> damageAmounts, uint targetObjectId, AimPosition aimPosition, int randomSeed, long? time)
        {
            if (BaseGameNetworkManager.Singleton.IsServer)
            {
                // Spawn area entity
                // Aim position type always is `Position`
                LiteNetLibIdentity spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                    areaDamageEntity.Identity.HashAssetId,
                    aimPosition.position,
                    GameInstance.Singleton.GameplayRule.GetSummonRotation(skillUser));
                AreaDamageEntity entity = spawnObj.GetComponent<AreaDamageEntity>();
                entity.Setup(skillUser.GetInfo(), weapon, damageAmounts, this, skillLevel, areaDuration.GetAmount(skillLevel), applyDuration.GetAmount(skillLevel));
                BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            }
            // Teleport to aim position
            if (isWarpToAimPosition)
                skillUser.Teleport(aimPosition.position, skillUser.MovementTransform.rotation);
        }

        public override KeyValuePair<DamageElement, MinMaxFloat> GetBaseAttackDamageAmount(ICharacterData skillUser, short skillLevel, bool isLeftHand)
        {
            switch (skillAttackType)
            {
                case SkillAttackType.Normal:
                    return damageAmount.ToKeyValuePair(skillLevel, 1f, GetEffectivenessDamage(skillUser));
                case SkillAttackType.BasedOnWeapon:
                    return skillUser.GetWeaponDamages(ref isLeftHand);
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

        public override HarvestType GetHarvestType()
        {
            return harvestType;
        }

        public override IncrementalMinMaxFloat GetHarvestDamageAmount()
        {
            return harvestDamageAmount;
        }

        protected float GetEffectivenessDamage(ICharacterData skillUser)
        {
            return GameDataHelpers.GetEffectivenessDamage(CacheEffectivenessAttributes, skillUser);
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
            areaDamageEntity.InitPrefab();
            GameInstance.AddOtherNetworkObjects(areaDamageEntity.Identity);
        }
    }
}
