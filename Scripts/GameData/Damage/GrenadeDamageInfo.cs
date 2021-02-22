using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class GrenadeDamageInfo : BaseCustomDamageInfo
    {
        [SerializeField]
        private GrenadeDamageEntity grenadeDamageEntity;
        [SerializeField]
        private float throwForce = 5f;
        [SerializeField]
        private float lifeTime = 2f;

        public override Transform GetDamageEffectTransform(BaseCharacterEntity attacker, bool isLeftHand)
        {
            Transform transform = null;
            if (attacker.ModelManager.IsFps)
            {
                if (attacker.FpsModel && attacker.FpsModel.gameObject.activeSelf)
                {
                    // Spawn bullets from fps model
                    transform = isLeftHand ? attacker.FpsModel.GetLeftHandMissileDamageTransform() : attacker.FpsModel.GetRightHandMissileDamageTransform();
                }
            }
            else
            {
                // Spawn bullets from tps model
                transform = isLeftHand ? attacker.CharacterModel.GetLeftHandMissileDamageTransform() : attacker.CharacterModel.GetRightHandMissileDamageTransform();
            }

            if (transform == null)
            {
                // Still no missile transform, use default missile transform
                transform = attacker.MissileDamageTransform;
            }
            return transform;
        }

        public override Transform GetDamageTransform(BaseCharacterEntity attacker, bool isLeftHand)
        {
            return attacker.MissileDamageTransform;
        }

        public override float GetDistance()
        {
            return throwForce * lifeTime;
        }

        public override float GetFov()
        {
            return 10f;
        }

        public override void LaunchDamageEntity(BaseCharacterEntity attacker, bool isLeftHand, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, BaseSkill skill, short skillLevel, Vector3 aimPosition, Vector3 stagger)
        {
            // Damage effect transform data
            Vector3 damageEffectPosition;
            Vector3 damageEffectDirection;
            Quaternion damageEffectRotation;
            this.GetDamagePositionAndRotation(attacker, isLeftHand, true, aimPosition, stagger, out damageEffectPosition, out damageEffectDirection, out damageEffectRotation);
            PoolSystem.GetInstance(grenadeDamageEntity, damageEffectPosition, damageEffectRotation)
                .Setup(attacker.GetInfo(), weapon, damageAmounts, skill, skillLevel, throwForce, lifeTime);
        }
    }
}
