using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.SIMPLE_RESURRECTION_SKILL_FILE, menuName = GameDataMenuConsts.SIMPLE_RESURRECTION_SKILL_MENU, order = GameDataMenuConsts.SIMPLE_RESURRECTION_SKILL_ORDER)]
    public class SimpleResurrectionSkill : BaseSkill
    {
        [Category("Buff")]
        public IncrementalFloat buffDistance;
        public Buff buff;
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
            AimPosition aimPosition,
            DamageOriginPreparedDelegate onDamageOriginPrepared,
            DamageHitDelegate onDamageHit)
        {
            // Resurrect target
            BasePlayerCharacterEntity targetEntity;
            if (!skillUser.CurrentGameManager.TryGetEntityByObjectId(targetObjectId, out targetEntity) || !targetEntity.IsDead())
                return;
            
            targetEntity.CurrentHp = Mathf.CeilToInt(targetEntity.GetCaches().MaxHp * resurrectHpRate);
            targetEntity.CurrentMp = Mathf.CeilToInt(targetEntity.GetCaches().MaxMp * resurrectMpRate);
            targetEntity.CurrentStamina = Mathf.CeilToInt(targetEntity.GetCaches().MaxStamina * resurrectStaminaRate);
            targetEntity.CurrentFood = Mathf.CeilToInt(targetEntity.GetCaches().MaxFood * resurrectFoodRate);
            targetEntity.CurrentWater = Mathf.CeilToInt(targetEntity.GetCaches().MaxWater * resurrectWaterRate);
            targetEntity.StopMove();
            targetEntity.CallAllOnRespawn();
            targetEntity.ApplyBuff(DataId, BuffType.SkillBuff, skillLevel, skillUser.GetInfo(), weapon);
        }

        public override float GetCastDistance(BaseCharacterEntity skillUser, int skillLevel, bool isLeftHand)
        {
            return buffDistance.GetAmount(skillLevel);
        }

        public override SkillType SkillType
        {
            get { return SkillType.Active; }
        }

        public override bool IsBuff
        {
            get { return true; }
        }

        public override bool RequiredTarget
        {
            get { return true; }
        }

        public override Buff Buff
        {
            get { return buff; }
        }

        public override bool CanUse(BaseCharacterEntity character, int level, bool isLeftHand, uint targetObjectId, out UITextKeys gameMessage, bool isItem = false)
        {
            if (!base.CanUse(character, level, isLeftHand, targetObjectId, out gameMessage, isItem))
                return false;
            
            BasePlayerCharacterEntity targetEntity;
            if (!character.CurrentGameManager.TryGetEntityByObjectId(targetObjectId, out targetEntity) || !targetEntity.IsDead())
                return false;

            return true;
        }
    }
}
