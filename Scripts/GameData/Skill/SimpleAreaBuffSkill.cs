using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Create GameData/Skill/Simple Area Buff Skill", order = -4987)]
    public partial class SimpleAreaBuffSkill : BaseAreaSkill
    {
        public AreaBuffEntity areaBuffEntity;
        public Buff buff;

        protected override void ApplySkillImplement(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand, CharacterItem weapon, int hitIndex, Dictionary<DamageElement, MinMaxFloat> damageAmounts, AimPosition aimPosition, int randomSeed, long? time)
        {
            // Spawn area entity
            // Aim position type always is `Position`
            PoolSystem.GetInstance(areaBuffEntity, aimPosition.position, GameInstance.Singleton.GameplayRule.GetSummonRotation(skillUser))
                .Setup(skillUser, this, skillLevel, areaDuration.GetAmount(skillLevel), applyDuration.GetAmount(skillLevel));
        }

        public override KeyValuePair<DamageElement, MinMaxFloat> GetBaseAttackDamageAmount(ICharacterData skillUser, short skillLevel, bool isLeftHand)
        {
            return new KeyValuePair<DamageElement, MinMaxFloat>();
        }

        public override Dictionary<DamageElement, MinMaxFloat> GetAttackAdditionalDamageAmounts(ICharacterData skillUser, short skillLevel)
        {
            return new Dictionary<DamageElement, MinMaxFloat>();
        }

        public override Dictionary<DamageElement, float> GetAttackWeaponDamageInflictions(ICharacterData skillUser, short skillLevel)
        {
            return new Dictionary<DamageElement, float>();
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

        public override Buff GetBuff()
        {
            return buff;
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddPoolingObjects(areaBuffEntity);
        }
    }
}
