using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Create GameData/Skill/Simple Resurrection Skill", order = -4986)]
    public class SimpleResurrectionSkill : BaseSkill
    {
        [Header("Resurrect Skill")]
        [Range(0.01f, 1f)]
        public float resurrectHpRate = 0.1f;
        [Range(0.01f, 1f)]
        public float resurrectMpRate = 0.1f;
        [Range(0.01f, 1f)]
        public float resurrectStaminaRate = 0.1f;
        [Range(0.01f, 1f)]
        public float resurrectFoodRate = 0.1f;
        [Range(0.01f, 1f)]
        public float resurrectWaterRate = 0.1f;

        [Header("Buffs")]
        public IncrementalFloat buffDistance;
        public Buff buff;

        public override void ApplySkill(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand, CharacterItem weapon, int hitIndex, Dictionary<DamageElement, MinMaxFloat> damageAmounts, Vector3 aimPosition)
        {
            // Resurrect target
            BasePlayerCharacterEntity targetEntity;
            if (!skillUser.TryGetTargetEntity(out targetEntity) || !targetEntity.IsDead())
                return;
            
            targetEntity.CurrentHp = Mathf.CeilToInt(targetEntity.GetCaches().MaxHp * resurrectHpRate);
            targetEntity.CurrentMp = Mathf.CeilToInt(targetEntity.GetCaches().MaxMp * resurrectMpRate);
            targetEntity.CurrentStamina = Mathf.CeilToInt(targetEntity.GetCaches().MaxStamina * resurrectStaminaRate);
            targetEntity.CurrentFood = Mathf.CeilToInt(targetEntity.GetCaches().MaxFood * resurrectFoodRate);
            targetEntity.CurrentWater = Mathf.CeilToInt(targetEntity.GetCaches().MaxWater * resurrectWaterRate);
            targetEntity.StopMove();
            targetEntity.RequestOnRespawn();
            targetEntity.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, skillUser);
        }

        public override Dictionary<DamageElement, MinMaxFloat> GetAttackAdditionalDamageAmounts(ICharacterData skillUser, short skillLevel)
        {
            return new Dictionary<DamageElement, MinMaxFloat>();
        }

        public override float GetCastDistance(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand)
        {
            return buffDistance.GetAmount(skillLevel);
        }

        public override float GetCastFov(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand)
        {
            return 360f;
        }

        public override Dictionary<DamageElement, float> GetAttackWeaponDamageInflictions(ICharacterData skillUser, short skillLevel)
        {
            return new Dictionary<DamageElement, float>();
        }

        public override KeyValuePair<DamageElement, MinMaxFloat> GetBaseAttackDamageAmount(ICharacterData skillUser, short skillLevel, bool isLeftHand)
        {
            return new KeyValuePair<DamageElement, MinMaxFloat>();
        }

        public override SkillType GetSkillType()
        {
            return SkillType.Active;
        }

        public override bool IsAttack()
        {
            return false;
        }

        public override bool IsBuff()
        {
            return true;
        }

        public override bool IsDebuff()
        {
            return false;
        }

        public override bool RequiredTarget()
        {
            return true;
        }

        public override Buff GetBuff()
        {
            return buff;
        }

        public override bool CanUse(BaseCharacterEntity character, short level, bool isLeftHand, out GameMessage.Type gameMessageType, bool isItem = false)
        {
            gameMessageType = GameMessage.Type.None;
            if (!base.CanUse(character, level, isLeftHand, out gameMessageType, isItem))
                return false;
            
            BasePlayerCharacterEntity targetEntity;
            if (!character.TryGetTargetEntity(out targetEntity) || !targetEntity.IsDead())
                return false;
            
            return true;
        }
    }
}
