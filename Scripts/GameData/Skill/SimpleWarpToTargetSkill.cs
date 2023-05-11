using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.SIMPLE_WARP_TO_TARGET_SKILL_FILE, menuName = GameDataMenuConsts.SIMPLE_WARP_TO_TARGET_SKILL_MENU, order = GameDataMenuConsts.SIMPLE_WARP_TO_TARGET_SKILL_ORDER)]
    public class SimpleWarpToTargetSkill : BaseAreaSkill
    {
        protected override void ApplySkillImplement(BaseCharacterEntity skillUser, int skillLevel, bool isLeftHand, CharacterItem weapon, int triggerIndex, Dictionary<DamageElement, MinMaxFloat> damageAmounts, uint targetObjectId, AimPosition aimPosition, int randomSeed)
        {
            // Teleport to aim position
            skillUser.Teleport(aimPosition.position, skillUser.MovementTransform.rotation);
        }

        public override KeyValuePair<DamageElement, MinMaxFloat> GetBaseAttackDamageAmount(ICharacterData skillUser, int skillLevel, bool isLeftHand)
        {
            return new KeyValuePair<DamageElement, MinMaxFloat>();
        }

        public override Dictionary<DamageElement, MinMaxFloat> GetAttackAdditionalDamageAmounts(ICharacterData skillUser, int skillLevel)
        {
            return new Dictionary<DamageElement, MinMaxFloat>();
        }

        public override Dictionary<DamageElement, float> GetAttackWeaponDamageInflictions(ICharacterData skillUser, int skillLevel)
        {
            return new Dictionary<DamageElement, float>();
        }
    }
}
