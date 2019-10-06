using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Create GameData/Skill/Simple Area Attack Skill", order = -4988)]
    public class SimpleAreaAttackSkill : BaseSkill
    {
        public float castRadius;
        public IncrementalFloat areaDuration;
        public IncrementalFloat applyDuration;
        public AreaDamageEntity areaDamageEntity;
        public GameObject targetObjectPrefab;
        public GameEffectCollection hitEffects;
        public DamageIncremental damageAmount;
        public DamageIncremental[] additionalDamageAmounts;
        public bool increaseDamageWithBuffs;
        public bool isDebuff;
        public Buff debuff;

        private GameObject cacheTargetObject;
        public GameObject CacheTargetObject
        {
            get
            {
                if (cacheTargetObject == null)
                {
                    cacheTargetObject = Instantiate(targetObjectPrefab);
                    cacheTargetObject.SetActive(false);
                }
                return cacheTargetObject;
            }
        }

        public override SkillType GetSkillType()
        {
            return SkillType.Active;
        }

        public override GameEffectCollection GetHitEffect()
        {
            return hitEffects;
        }

        public override void ApplySkill(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, Vector3 aimPosition)
        {
            // Apply debuff
            CharacterBuff debuff = CharacterBuff.Empty;
            if (isDebuff)
                debuff = CharacterBuff.Create(BuffType.SkillDebuff, DataId, skillLevel);
            // Spawn area entity
            // TODO: validate aim position
            AreaDamageEntity damageEntity = Instantiate(areaDamageEntity, aimPosition, skillUser.GetSummonRotation());
            damageEntity.Setup(skillUser, weapon, GetAttackDamages(skillUser, skillLevel, isLeftHand), debuff, this, skillLevel, areaDuration.GetAmount(skillLevel), applyDuration.GetAmount(skillLevel));
        }

        public override float GetAttackDistance(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand)
        {
            return castRadius;
        }

        public override float GetAttackFov(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand)
        {
            return 360f;
        }

        public override Dictionary<DamageElement, MinMaxFloat> GetAttackDamages(ICharacterData skillUser, short skillLevel, bool isLeftHand)
        {
            Dictionary<DamageElement, MinMaxFloat> damageAmounts = new Dictionary<DamageElement, MinMaxFloat>();

            // Sum damage with skill damage because this skill damages based on itself
            damageAmounts = GameDataHelpers.CombineDamages(
                damageAmounts,
                GetAttackAdditionalDamageAmounts(skillUser, skillLevel));
            // Sum damage with additional damage amounts
            damageAmounts = GameDataHelpers.CombineDamages(
                damageAmounts,
                GetBaseAttackDamageAmount(skillUser, skillLevel, isLeftHand));

            if (increaseDamageWithBuffs)
            {
                // Sum damage with buffs
                damageAmounts = GameDataHelpers.CombineDamages(
                    damageAmounts,
                    skillUser.GetCaches().IncreaseDamages);
            }

            return damageAmounts;
        }

        public override KeyValuePair<DamageElement, MinMaxFloat> GetBaseAttackDamageAmount(ICharacterData skillUser, short skillLevel, bool isLeftHand)
        {
            return GameDataHelpers.MakeDamage(
                damageAmount,
                skillLevel,
                1f, // Equipment Stats Rate, this is not based on equipment so its rate is 1f
                0f  // No effectiveness attributes
                );
        }

        public override Dictionary<DamageElement, MinMaxFloat> GetAttackAdditionalDamageAmounts(ICharacterData skillUser, short skillLevel)
        {
            return GameDataHelpers.CombineDamages(additionalDamageAmounts, new Dictionary<DamageElement, MinMaxFloat>(), skillLevel, 1f);
        }

        public override Dictionary<DamageElement, float> GetAttackWeaponDamageInflictions(ICharacterData skillUser, short skillLevel)
        {
            return new Dictionary<DamageElement, float>();
        }

        public override ItemCraft GetItemCraft()
        {
            return default(ItemCraft);
        }

        public override MountEntity GetMountEntity()
        {
            return null;
        }

        public override BaseMonsterCharacterEntity GetSummonMonsterEntity()
        {
            return null;
        }

        public override bool IsAttack()
        {
            return true;
        }

        public override bool IsBuff()
        {
            return false;
        }

        public override Buff GetBuff()
        {
            return default(Buff);
        }

        public override bool IsDebuff()
        {
            return isDebuff;
        }

        public override Buff GetDebuff()
        {
            if (!IsDebuff())
                return default(Buff);
            return debuff;
        }

        public override bool HasCustomAimControls()
        {
            return true;
        }

        public override Vector3? UpdateAimControls(short skillLevel)
        {
            throw new System.NotImplementedException();
        }
    }
}
