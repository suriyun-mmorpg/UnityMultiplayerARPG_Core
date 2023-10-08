using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class RaycastDamageInfo : BaseCustomDamageInfo
    {
        public float missileDistance;
        public float missileSpeed;
        public ProjectileEffect projectileEffect;
        public byte pierceThroughEntities;
        public ImpactEffects impactEffects;

        public override void PrepareRelatesData()
        {
            GameInstance.AddPoolingObjects(projectileEffect);
            if (impactEffects != null)
                impactEffects.PrepareRelatesData();
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
            if (!hitValidateData.HitsCount.TryGetValue(hitData.GetHitId(), out int hitCount))
            {
                // Set hit count to 0, if it is not in collection
                hitCount = 0;
            }
            if (hitCount > pierceThroughEntities)
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

            bool isPlayImpactEffects = isClient && impactEffects != null;
            float projectileDistance = missileDistance;
            List<ImpactEffectPlayingData> impactEffectsData = new List<ImpactEffectPlayingData>();
            int layerMask = GameInstance.Singleton.GetDamageEntityHitLayerMask();
            int tempHitCount = attacker.AttackPhysicFunctions.Raycast(damagePosition, damageDirection, missileDistance, layerMask, QueryTriggerInteraction.Collide);
            if (tempHitCount <= 0)
            {
                // Spawn projectile effect, it will move to target but it won't apply damage because it is just effect
                if (isClient)
                {
                    PoolSystem.GetInstance(projectileEffect, damagePosition, damageRotation)
                        .Setup(projectileDistance, missileSpeed, impactEffects, damagePosition, impactEffectsData);
                }
                return;
            }

            HashSet<uint> hitObjects = new HashSet<uint>();
            projectileDistance = float.MinValue;
            byte pierceThroughEntities = this.pierceThroughEntities;
            Vector3 tempHitPoint;
            Vector3 tempHitNormal;
            float tempHitDistance;
            GameObject tempGameObject;
            string tempTag;
            DamageableHitBox tempDamageableHitBox;
            // Find characters that receiving damages
            for (int tempLoopCounter = 0; tempLoopCounter < tempHitCount; ++tempLoopCounter)
            {
                tempHitPoint = attacker.AttackPhysicFunctions.GetRaycastPoint(tempLoopCounter);
                tempHitNormal = attacker.AttackPhysicFunctions.GetRaycastNormal(tempLoopCounter);
                tempHitDistance = attacker.AttackPhysicFunctions.GetRaycastDistance(tempLoopCounter);
                tempGameObject = attacker.AttackPhysicFunctions.GetRaycastObject(tempLoopCounter);

                if (!tempGameObject.GetComponent<IUnHittable>().IsNull())
                    continue;

                tempDamageableHitBox = tempGameObject.GetComponent<DamageableHitBox>();
                if (tempDamageableHitBox == null)
                {
                    if (GameInstance.Singleton.IsDamageableLayer(tempGameObject.layer))
                    {
                        // Hit something which is part of damageable entities
                        continue;
                    }

                    // Hit wall... so play impact effects and update piercing
                    // Prepare data to instantiate impact effects
                    if (isPlayImpactEffects)
                    {
                        tempTag = tempGameObject.tag;
                        impactEffectsData.Add(new ImpactEffectPlayingData()
                        {
                            tag = tempTag,
                            point = tempHitPoint,
                            normal = tempHitNormal,
                        });
                    }

                    // Update pierce trough entities count
                    if (pierceThroughEntities <= 0)
                    {
                        if (tempHitDistance > projectileDistance)
                            projectileDistance = tempHitDistance;
                        break;
                    }
                    --pierceThroughEntities;
                    continue;
                }

                if (tempDamageableHitBox.GetObjectId() == attacker.ObjectId)
                    continue;

                if (hitObjects.Contains(tempDamageableHitBox.GetObjectId()))
                    continue;

                // Add entity to table, if it found entity in the table next time it will skip. 
                // So it won't applies damage to entity repeatly.
                hitObjects.Add(tempDamageableHitBox.GetObjectId());

                // Target won't receive damage if dead or can't receive damage from this character
                if (tempDamageableHitBox.IsDead() || !tempDamageableHitBox.CanReceiveDamageFrom(instigator))
                    continue;

                // Target receives damages
                if (isHost || isOwnedByServer)
                    tempDamageableHitBox.ReceiveDamage(attacker.EntityTransform.position, instigator, damageAmounts, weapon, skill, skillLevel, simulateSeed);

                // Prepare hit reg because it is hitting
                hitRegData.HitTimestamp = BaseGameNetworkManager.Singleton.Timestamp;
                hitRegData.HitObjectId = tempDamageableHitBox.GetObjectId();
                hitRegData.HitBoxIndex = tempDamageableHitBox.Index;
                hitRegData.Destination = tempHitPoint;
                BaseGameNetworkManager.Singleton.HitRegistrationManager.PrepareHitRegData(hitRegData);

                // Prepare data to instantiate impact effects
                if (isPlayImpactEffects)
                {
                    tempTag = tempDamageableHitBox.EntityGameObject.tag;
                    impactEffectsData.Add(new ImpactEffectPlayingData()
                    {
                        tag = tempTag,
                        point = tempHitPoint,
                        normal = tempHitNormal,
                    });
                }

                // Update pierce trough entities count
                if (pierceThroughEntities <= 0)
                {
                    if (tempHitDistance > projectileDistance)
                        projectileDistance = tempHitDistance;
                    break;
                }
                --pierceThroughEntities;
            }

            // Spawn projectile effect, it will move to target but it won't apply damage because it is just effect
            if (isClient)
            {
                PoolSystem.GetInstance(projectileEffect, damagePosition, damageRotation)
                    .Setup(projectileDistance, missileSpeed, impactEffects, damagePosition, impactEffectsData);
            }
        }
    }
}