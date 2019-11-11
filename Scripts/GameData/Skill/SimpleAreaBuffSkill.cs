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

        public override void ApplySkill(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand, CharacterItem weapon, int hitIndex, Dictionary<DamageElement, MinMaxFloat> damageAmounts, Vector3 aimPosition)
        {
            // Spawn area entity
            aimPosition = AreaSkillControls.FindGround(AreaSkillControls.ValidateDistance(skillUser.CacheTransform.position, aimPosition, castDistance.GetAmount(skillLevel)));
            AreaBuffEntity buffEntity = Instantiate(areaBuffEntity, aimPosition, skillUser.GetSummonRotation());
            buffEntity.Setup(skillUser, this, skillLevel, areaDuration.GetAmount(skillLevel), applyDuration.GetAmount(skillLevel));
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

        public override sealed Buff GetBuff()
        {
            return buff;
        }
    }
}
