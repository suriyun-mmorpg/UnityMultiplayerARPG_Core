using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class MissileDamageInfo : BaseCustomDamageInfo
    {
        [Tooltip("If this is TRUE, it will hit only selected target, if no selected target it will hit 1 random target")]
        public bool hitOnlySelectedTarget;
        public float missileDistance;
        public float missileSpeed;
        public MissileDamageEntity missileDamageEntity;

        public override void PrepareRelatesData()
        {
            GameInstance.AddPoolingObjects(missileDamageEntity);
        }

        public override Transform GetDamageTransform(BaseCharacterEntity attacker, bool isLeftHand)
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

        public override float GetDistance()
        {
            return missileDistance;
        }

        public override float GetFov()
        {
            return 10f;
        }

        public override bool IsHitValid(HitValidateData hitValidateData, HitRegisterData hitData, DamageableHitBox hitBox)
        {
            float dist = Vector3.Distance(hitData.Origin, hitData.Destination);
            float maxExtents = Mathf.Max(hitBox.Bounds.extents.x, hitBox.Bounds.extents.y, hitBox.Bounds.extents.z);
            // Too far
            if (dist > missileDistance + maxExtents)
                return false;
            
            float duration = (float)(hitData.HitTimestamp - hitData.LaunchTimestamp) * 0.001f;
            // Take too short time to hit
            if (Mathf.Abs((dist / duration) / missileSpeed) < 0.8f)
                return false;
            return true;
        }

        public override void LaunchDamageEntity(BaseCharacterEntity attacker, bool isLeftHand, CharacterItem weapon, int simulateSeed, byte triggerIndex, byte spreadIndex, Vector3 fireStagger, Dictionary<DamageElement, MinMaxFloat> damageAmounts, BaseSkill skill, int skillLevel, AimPosition aimPosition)
        {
            // Spawn missile damage entity, it will move to target then apply damage when hit
            // Instantiates on both client and server (damage applies at server)
            if (missileDamageEntity == null)
                return;

            // Get generic attack data
            EntityInfo instigator = attacker.GetInfo();
            System.Random random = new System.Random(unchecked(simulateSeed + ((triggerIndex + 1) * (spreadIndex + 1) * 16)));
            Vector3 stagger = new Vector3(GenericUtils.RandomFloat(random.Next(), -fireStagger.x, fireStagger.x), GenericUtils.RandomFloat(random.Next(), -fireStagger.y, fireStagger.y));
            this.GetDamagePositionAndRotation(attacker, isLeftHand, aimPosition, stagger, out Vector3 damagePosition, out Vector3 damageDirection, out Quaternion damageRotation);
            // Prepare hit reg data
            HitRegisterData hitRegData = new HitRegisterData()
            {
                SimulateSeed = simulateSeed,
                TriggerIndex = triggerIndex,
                SpreadIndex = spreadIndex,
                LaunchTimestamp = BaseGameNetworkManager.Singleton.Timestamp,
                Origin = damagePosition,
                Direction = damageDirection,
            };

            DamageableEntity lockingTarget;
            if (!hitOnlySelectedTarget || !attacker.TryGetTargetEntity(out lockingTarget))
                lockingTarget = null;

            // Instantiate missile damage entity
            float missileDistance = this.missileDistance;
            float missileSpeed = this.missileSpeed;
            PoolSystem.GetInstance(missileDamageEntity, damagePosition, damageRotation).Setup(instigator, weapon, simulateSeed, triggerIndex, spreadIndex, damageAmounts, skill, skillLevel, hitRegData, missileDistance, missileSpeed, lockingTarget);
        }
    }
}