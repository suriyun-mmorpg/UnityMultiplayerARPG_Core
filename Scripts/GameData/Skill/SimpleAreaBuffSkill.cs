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
            aimPosition = GameplayUtils.FindGround(GameplayUtils.ClampPosition(skillUser.CacheTransform.position, aimPosition, castDistance.GetAmount(skillLevel)),
                AreaSkillControls.GROUND_DETECTION_DISTANCE,
                GameInstance.Singleton.GetMonsterSpawnGroundDetectionLayerMask());
            PoolSystem.GetInstance(areaBuffEntity, aimPosition, skillUser.GetSummonRotation())
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
            GameInstance.AddPoolingObjects(new IPoolDescriptor[] { areaBuffEntity });
        }
    }
}
