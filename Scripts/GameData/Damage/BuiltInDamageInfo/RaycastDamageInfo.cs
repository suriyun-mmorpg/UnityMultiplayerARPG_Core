using Cysharp.Threading.Tasks;
using Insthync.AddressableAssetTools;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class RaycastDamageInfo : BaseCustomDamageInfo
    {
        public float missileDistance;
        public float missileSpeed;
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [AddressableAssetConversion(nameof(addressableProjectEffect))]
        public ProjectileEffect projectileEffect;
#endif
        public ProjectileEffect ProjectileEffect
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return projectileEffect;
#else
                return null;
#endif
            }
        }
        public AssetReferenceProjectileEffect addressableProjectEffect;
        public AssetReferenceProjectileEffect AddressableProjectEffect
        {
            get => addressableProjectEffect;
        }
        public byte pierceThroughEntities;
        public ImpactEffects impactEffects;

        public override void PrepareRelatesData()
        {
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
            // Prevent pass through wall hacking
            // It will raycast from origin to hit position
            int layerMask = GameInstance.Singleton.GetAttackObstacleLayerMask();
            RaycastHit[] tempWallHits;
            RaycastHit2D[] tempWallHit2Ds;
            int tempWallHitCount = 0;
            switch (GameInstance.Singleton.DimensionType)
            {
                case DimensionType.Dimension3D:
                    tempWallHits = new RaycastHit[pierceThroughEntities + 1];
                    tempWallHitCount = PhysicUtils.SortedRaycastNonAlloc3D(new Ray(hitData.Origin, hitData.Direction), tempWallHits, Vector3.Distance(hitData.Origin, hitData.HitOrigin), layerMask, QueryTriggerInteraction.Ignore);
                    break;
                case DimensionType.Dimension2D:
                    tempWallHit2Ds = new RaycastHit2D[pierceThroughEntities + 1];
                    tempWallHitCount = PhysicUtils.SortedRaycastNonAlloc2D(new Ray2D(hitData.Origin, (Vector3)hitData.Direction), tempWallHit2Ds, Vector2.Distance(hitData.Origin, hitData.HitOrigin), layerMask);
                    break;
            }
            return tempWallHitCount <= pierceThroughEntities;
        }

        public override bool IsHeadshotInstantDeath()
        {
            return true;
        }

        public override UniTask LaunchDamageEntity(BaseCharacterEntity attacker, bool isLeftHand, CharacterItem weapon, int simulateSeed, byte triggerIndex, byte spreadIndex, Vector3 fireSpreadRange, List<Dictionary<DamageElement, MinMaxFloat>> damageAmounts, BaseSkill skill, int skillLevel, AimPosition aimPosition)
        {
            bool isClient = attacker.IsClient;
            bool isServer = attacker.IsServer;
            bool isOwnerClient = attacker.IsOwnerClient;
            bool isOwnedByServer = attacker.IsOwnedByServer;
            bool willProceedHitRegByClient = !attacker.IsOwnedByServer && !attacker.IsOwnerHost;

            // Get generic attack data
            EntityInfo instigator = attacker.GetInfo();
            System.Random random = new System.Random(unchecked(simulateSeed + ((triggerIndex + 1) * (spreadIndex + 1) * 16)));
            Vector3 spreadRange = new Vector3(GenericUtils.RandomFloat(random.Next(), -fireSpreadRange.x, fireSpreadRange.x), GenericUtils.RandomFloat(random.Next(), -fireSpreadRange.y, fireSpreadRange.y));
            this.GetDamagePositionAndRotation(attacker, isLeftHand, aimPosition, spreadRange, out Vector3 damagePosition, out Vector3 damageDirection, out Quaternion damageRotation);
            // Prepare hit reg data
            HitRegisterData hitRegData = new HitRegisterData()
            {
                SimulateSeed = simulateSeed,
                TriggerIndex = triggerIndex,
                SpreadIndex = spreadIndex,
                LaunchTimestamp = BaseGameNetworkManager.Singleton.ServerTimestamp,
                Origin = damagePosition,
                Direction = damageDirection,
            };
#if UNITY_EDITOR
            attacker.SetDebugDamage(new BaseCharacterEntity.DebugDamageLaunch()
            {
                position = damagePosition,
                rotation = damageRotation,
                direction = damageDirection,
                isLeftHand = isLeftHand,
                fov = GetFov(),
                distance = GetDistance(),
            });
#endif

            if (!isOwnedByServer && !isClient)
            {
                // Only server entities (such as monsters) and clients will launch raycast damage
                // clients do it for game effects playing, server do it to apply damage
                return default;
            }

            float projectileDistance = missileDistance;
#if !UNITY_SERVER
            bool isPlayImpactEffects = isClient && impactEffects != null;
            List<ImpactEffectPlayingData> impactEffectsData = new List<ImpactEffectPlayingData>();
#endif
            int layerMask = GameInstance.Singleton.GetDamageEntityHitLayerMask();
            int tempHitCount = attacker.AttackPhysicFunctions.Raycast(damagePosition, damageDirection, missileDistance, layerMask, QueryTriggerInteraction.Collide, true);
            if (tempHitCount <= 0)
            {
#if !UNITY_SERVER
                // Spawn projectile effect, it will move to target but it won't apply damage because it is just effect
                if (isClient)
                    PlayProjectileEffect(damagePosition, damageRotation, projectileDistance, impactEffectsData);
#endif
                return default;
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
                if (tempDamageableHitBox == null || !tempDamageableHitBox.Entity)
                {
                    if (GameInstance.Singleton.IsDamageableLayer(tempGameObject.layer))
                    {
                        // Hit something which is part of damageable entities
                        continue;
                    }

#if !UNITY_SERVER
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
#endif

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
                if (isServer && !willProceedHitRegByClient)
                    tempDamageableHitBox.ReceiveDamage(attacker.EntityTransform.position, instigator, damageAmounts[triggerIndex], weapon, skill, skillLevel, simulateSeed);

                // Prepare hit reg because it is hitting
                if (isOwnerClient && willProceedHitRegByClient)
                {
                    hitRegData.HitTimestamp = BaseGameNetworkManager.Singleton.ServerTimestamp;
                    hitRegData.HitObjectId = tempDamageableHitBox.GetObjectId();
                    hitRegData.HitBoxIndex = tempDamageableHitBox.Index;
                    hitRegData.HitOrigin = tempHitPoint;
                    attacker.CallCmdPerformHitRegValidation(hitRegData);
                }

#if !UNITY_SERVER
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
#endif

                // Update pierce trough entities count
                if (pierceThroughEntities <= 0)
                {
                    if (tempHitDistance > projectileDistance)
                        projectileDistance = tempHitDistance;
                    break;
                }
                --pierceThroughEntities;
            }
#if !UNITY_SERVER
            // Spawn projectile effect, it will move to target but it won't apply damage because it is just effect
            if (isClient)
                PlayProjectileEffect(damagePosition, damageRotation, projectileDistance, impactEffectsData);
#endif
            return default;
        }

#if !UNITY_SERVER
        private async void PlayProjectileEffect(Vector3 damagePosition, Quaternion damageRotation, float projectileDistance, List<ImpactEffectPlayingData> impactEffectsData)
        {
            ProjectileEffect loadedProjectileEffect = await AddressableProjectEffect
                .GetOrLoadAssetAsyncOrUsePrefab(ProjectileEffect);

            if (loadedProjectileEffect == null)
                return;

            PoolSystem.GetInstance(loadedProjectileEffect, damagePosition, damageRotation)
                .Setup(projectileDistance, missileSpeed, impactEffects, damagePosition, impactEffectsData);
        }
#endif
    }
}