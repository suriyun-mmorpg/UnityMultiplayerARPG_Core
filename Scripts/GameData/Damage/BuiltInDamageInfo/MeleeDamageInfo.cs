using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class MeleeDamageInfo : BaseCustomDamageInfo
    {
        [Tooltip("If this is TRUE, it will hit only selected target, if no selected target it will hit 1 random target")]
        public bool hitOnlySelectedTarget;
        public float hitDistance;
        [Min(10f)]
        public float hitFov;
        public ImpactEffects impactEffects;

        public override void PrepareRelatesData()
        {
            if (impactEffects != null)
                impactEffects.PrepareRelatesData();
        }

        public override Transform GetDamageTransform(BaseCharacterEntity attacker, bool isLeftHand)
        {
            return attacker.MeleeDamageTransform;
        }

        public override float GetDistance()
        {
            return hitDistance;
        }

        public override float GetFov()
        {
            return hitFov;
        }

        public override bool IsHitValid(HitValidateData hitValidateData, HitRegisterData hitData, DamageableHitBox hitBox)
        {
            if (!hitValidateData.HitsCount.TryGetValue(hitData.GetHitId(), out int hitCount))
            {
                // Set hit count to 0, if it is not in collection
                hitCount = 0;
            }
            if (hitOnlySelectedTarget && hitCount > 0)
                return false;
            return true;
        }

        public override void LaunchDamageEntity(BaseCharacterEntity attacker, bool isLeftHand, CharacterItem weapon, int simulateSeed, byte triggerIndex, byte spreadIndex, Vector3 fireStagger, Dictionary<DamageElement, MinMaxFloat> damageAmounts, BaseSkill skill, int skillLevel, AimPosition aimPosition)
        {
            bool isClient = attacker.IsClient;
            bool isHost = attacker.IsOwnerHost;
            bool isOwnerClient = attacker.IsOwnerClient;
            bool isOwnedByServer = attacker.IsOwnedByServer;

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

            if (!isOwnedByServer && !isClient)
            {
                // Only server entities (such as monsters) and clients will launch raycast damage
                // clients do it for game effects playing, server do it to apply damage
                return;
            }

            // Find hitting objects
            int layerMask = GameInstance.Singleton.GetDamageEntityHitLayerMask();
            int tempHitCount = attacker.AttackPhysicFunctions.OverlapObjects(damagePosition, hitDistance, layerMask, true, QueryTriggerInteraction.Collide);
            if (tempHitCount <= 0)
                return;

            HashSet<uint> hitObjects = new HashSet<uint>();
            bool isPlayImpactEffects = isClient && impactEffects != null;
            DamageableHitBox tempDamageableHitBox;
            GameObject tempGameObject;
            string tempTag;
            DamageableHitBox tempDamageTakenTarget = null;
            DamageableEntity tempSelectedTarget = null;
            bool hasSelectedTarget = hitOnlySelectedTarget && attacker.TryGetTargetEntity(out tempSelectedTarget);
            // Find characters that receiving damages
            for (int i = 0; i < tempHitCount; ++i)
            {
                tempGameObject = attacker.AttackPhysicFunctions.GetOverlapObject(i);

                if (!tempGameObject.GetComponent<IUnHittable>().IsNull())
                    continue;

                tempDamageableHitBox = tempGameObject.GetComponent<DamageableHitBox>();
                if (tempDamageableHitBox == null)
                    continue;

                if (tempDamageableHitBox.GetObjectId() == attacker.ObjectId)
                    continue;

                if (hitObjects.Contains(tempDamageableHitBox.GetObjectId()))
                    continue;

                // Add entity to table, if it found entity in the table next time it will skip. 
                // So it won't applies damage to entity repeatly.
                hitObjects.Add(tempDamageableHitBox.GetObjectId());

                // Target won't receive damage if dead or can't receive damage from this character
                if (tempDamageableHitBox.IsDead() || !tempDamageableHitBox.CanReceiveDamageFrom(instigator) ||
                    !attacker.IsPositionInFov(hitFov, tempDamageableHitBox.GetTransform().position))
                    continue;

                if (hitOnlySelectedTarget)
                {
                    // Check with selected target
                    // Set damage taken target, it will be used in-case it can't find selected target
                    tempDamageTakenTarget = tempDamageableHitBox;
                    // The hitting entity is the selected target so break the loop to apply damage later (outside this loop)
                    if (hasSelectedTarget && tempSelectedTarget.GetObjectId() == tempDamageableHitBox.GetObjectId())
                        break;
                    continue;
                }

                // Target receives damages
                if (isHost || isOwnedByServer)
                    tempDamageableHitBox.ReceiveDamage(attacker.EntityTransform.position, instigator, damageAmounts, weapon, skill, skillLevel, simulateSeed);

                // Prepare hit reg because it is hitting
                if (isOwnerClient)
                {
                    hitRegData.HitTimestamp = BaseGameNetworkManager.Singleton.Timestamp;
                    hitRegData.HitObjectId = tempDamageableHitBox.GetObjectId();
                    hitRegData.HitBoxIndex = tempDamageableHitBox.Index;
                    hitRegData.Destination = tempDamageableHitBox.CacheTransform.position;
                    attacker.CallCmdPerformHitRegValidation(hitRegData);
                }

                // Instantiate impact effects
                if (isPlayImpactEffects)
                {
                    tempTag = tempDamageableHitBox.EntityGameObject.tag;
                    PlayMeleeImpactEffect(attacker, tempTag, tempDamageableHitBox, damagePosition);
                }
            }

            if (hitOnlySelectedTarget && tempDamageTakenTarget != null)
            {
                // Only 1 target will receives damages
                // Pass all receive damage condition, then apply damages
                if (isHost || isOwnedByServer)
                    tempDamageTakenTarget.ReceiveDamage(attacker.EntityTransform.position, instigator, damageAmounts, weapon, skill, skillLevel, simulateSeed);

                // Prepare hit reg because it is hitting
                if (isOwnerClient)
                {
                    hitRegData.HitTimestamp = BaseGameNetworkManager.Singleton.Timestamp;
                    hitRegData.HitObjectId = tempDamageTakenTarget.GetObjectId();
                    hitRegData.HitBoxIndex = tempDamageTakenTarget.Index;
                    hitRegData.Destination = tempDamageTakenTarget.CacheTransform.position;
                    attacker.CallCmdPerformHitRegValidation(hitRegData);
                }

                // Instantiate impact effects
                if (isPlayImpactEffects)
                {
                    tempTag = tempDamageTakenTarget.EntityGameObject.tag;
                    PlayMeleeImpactEffect(attacker, tempTag, tempDamageTakenTarget, damagePosition);
                }
            }
        }

        private void PlayMeleeImpactEffect(BaseCharacterEntity attacker, string tag, DamageableHitBox hitBox, Vector3 damagePosition)
        {
            if (!impactEffects.TryGetEffect(tag, out GameEffect gameEffect))
                return;
            Vector3 targetPosition = hitBox.Bounds.center;
            targetPosition.y = damagePosition.y;
            Vector3 dir = (targetPosition - damagePosition).normalized;
            PoolSystem.GetInstance(gameEffect, hitBox.Bounds.center, Quaternion.LookRotation(Vector3.up, dir));
        }
    }
}