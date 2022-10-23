using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public enum DamageType : byte
    {
        Melee,
        Missile,
        Raycast,
        Throwable,
        Custom = 254
    }

    [System.Serializable]
    public struct DamageInfo : IDamageInfo
    {
        public DamageType damageType;

        [StringShowConditional(nameof(damageType), new string[] { nameof(DamageType.Melee), nameof(DamageType.Missile) })]
        [Tooltip("If this is TRUE, it will hit only selected target, if no selected target it will hit 1 random target")]
        public bool hitOnlySelectedTarget;

        [Tooltip("Distance to start an attack, this does NOT distance to hit and apply damage, this value should be less than `hitDistance` or `missileDistance` to make sure it will hit the enemy properly. If this value <= 0 or > `hitDistance` or `missileDistance` it will re-calculate by `hitDistance` or `missileDistance`")]
        public float startAttackDistance;

        [StringShowConditional(nameof(damageType), new string[] { nameof(DamageType.Melee) })]
        public float hitDistance;
        [StringShowConditional(nameof(damageType), new string[] { nameof(DamageType.Melee) })]
        [Min(10f)]
        public float hitFov;

        [StringShowConditional(nameof(damageType), new string[] { nameof(DamageType.Missile), nameof(DamageType.Raycast) })]
        public float missileDistance;
        [StringShowConditional(nameof(damageType), new string[] { nameof(DamageType.Missile), nameof(DamageType.Raycast) })]
        public float missileSpeed;
        [StringShowConditional(nameof(damageType), new string[] { nameof(DamageType.Missile) })]
        public MissileDamageEntity missileDamageEntity;

        [StringShowConditional(nameof(damageType), new string[] { nameof(DamageType.Raycast) })]
        public ProjectileEffect projectileEffect;
        [StringShowConditional(nameof(damageType), new string[] { nameof(DamageType.Raycast) })]
        public byte pierceThroughEntities;
        [StringShowConditional(nameof(damageType), new string[] { nameof(DamageType.Melee), nameof(DamageType.Raycast) })]
        public ImpactEffects impactEffects;

        [StringShowConditional(nameof(damageType), new string[] { nameof(DamageType.Throwable) })]
        public float throwForce;
        [StringShowConditional(nameof(damageType), new string[] { nameof(DamageType.Throwable) })]
        public float throwableLifeTime;
        [StringShowConditional(nameof(damageType), new string[] { nameof(DamageType.Throwable) })]
        public ThrowableDamageEntity throwableDamageEntity;

        [StringShowConditional(nameof(damageType), new string[] { nameof(DamageType.Custom) })]
        public BaseCustomDamageInfo customDamageInfo;

        public float GetDistance()
        {
            float calculatedDistance = startAttackDistance;
            switch (damageType)
            {
                case DamageType.Melee:
                    if (calculatedDistance <= 0f || calculatedDistance > hitDistance)
                        calculatedDistance = hitDistance - (hitDistance * 0.1f);
                    break;
                case DamageType.Missile:
                case DamageType.Raycast:
                    if (calculatedDistance <= 0f || calculatedDistance > missileDistance)
                        calculatedDistance = missileDistance - (missileDistance * 0.1f);
                    break;
                case DamageType.Throwable:
                    // NOTE: It is actually can't find actual distance by simple math because it has many factors,
                    // Such as thrown position, distance from ground, gravity. 
                    // So all throwable weapons are suited for shooter games only.
                    if (calculatedDistance <= 0f)
                        calculatedDistance = throwForce * 0.5f;
                    break;
                case DamageType.Custom:
                    if (calculatedDistance <= 0f)
                        calculatedDistance = customDamageInfo.GetDistance();
                    break;
            }
            return calculatedDistance;
        }

        public float GetFov()
        {
            switch (damageType)
            {
                case DamageType.Melee:
                    return hitFov;
                case DamageType.Missile:
                case DamageType.Raycast:
                case DamageType.Throwable:
                    return 10f;
                case DamageType.Custom:
                    return customDamageInfo.GetFov();
            }
            return 0f;
        }

        public Transform GetDamageTransform(BaseCharacterEntity attacker, bool isLeftHand)
        {
            Transform transform = null;
            switch (damageType)
            {
                case DamageType.Melee:
                    transform = attacker.MeleeDamageTransform;
                    break;
                case DamageType.Missile:
                case DamageType.Raycast:
                case DamageType.Throwable:
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
                    break;
                case DamageType.Custom:
                    transform = customDamageInfo.GetDamageTransform(attacker, isLeftHand);
                    break;
            }
            return transform;
        }

        public void LaunchDamageEntity(
            BaseCharacterEntity attacker,
            bool isLeftHand,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            short skillLevel,
            int randomSeed,
            AimPosition aimPosition,
            Vector3 stagger,
            out Dictionary<uint, int> hitBoxes)
        {
            hitBoxes = new Dictionary<uint, int>();
            if (attacker == null)
                return;

            if (damageType == DamageType.Custom)
            {
                // Launch damage entity by custom class
                customDamageInfo.LaunchDamageEntity(
                    attacker,
                    isLeftHand,
                    weapon,
                    damageAmounts,
                    skill,
                    skillLevel,
                    randomSeed,
                    aimPosition,
                    stagger,
                    out hitBoxes);
                // Trigger attacker's on launch damage entity event before exit from this function
                attacker.OnLaunchDamageEntity(
                    isLeftHand,
                    weapon,
                    damageAmounts,
                    skill,
                    skillLevel,
                    randomSeed,
                    aimPosition,
                    stagger,
                    hitBoxes);
                // Exit from this function because launch damage entity functionality done by custom damage info class
                return;
            }

            bool isServer = attacker.IsServer;
            bool isClient = attacker.IsClient;
            bool isHost = attacker.IsHost;
            bool isOwnerClient = attacker.IsOwnerClient;
            bool isOwnedByServer = attacker.IsOwnedByServer;
            EntityInfo instigator = attacker.GetInfo();
            int damageableLayerMask = GameInstance.Singleton.GetDamageableLayerMask();

            DamageableHitBox tempDamageableHitBox = null;

            // Damage transform data
            Vector3 damagePosition;
            Vector3 damageDirection;
            Quaternion damageRotation;
            this.GetDamagePositionAndRotation(attacker, isLeftHand, aimPosition, stagger, out damagePosition, out damageDirection, out damageRotation);
#if UNITY_EDITOR
            attacker.SetDebugDamage(damagePosition, damageDirection, damageRotation, isLeftHand);
#endif

            bool hasImpactEffects = impactEffects != null;
            GameObject tempGameObject;
            switch (damageType)
            {
                case DamageType.Melee:
                    if (isOwnedByServer || isClient)
                    {
                        // Find hitting objects
                        int tempOverlapSize = attacker.AttackPhysicFunctions.OverlapObjects(damagePosition, hitDistance, damageableLayerMask, true);
                        if (tempOverlapSize > 0)
                        {
                            List<HitData> hitDataCollection = new List<HitData>();
                            int damageTakenTargetIndex = 0;
                            DamageableHitBox damageReceivingTarget = null;
                            DamageableEntity selectedTarget = null;
                            bool hasSelectedTarget = hitOnlySelectedTarget && attacker.TryGetTargetEntity(out selectedTarget);
                            // Find characters that receiving damages
                            for (int tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
                            {
                                tempGameObject = attacker.AttackPhysicFunctions.GetOverlapObject(tempLoopCounter);

                                if (!tempGameObject.GetComponent<IUnHittable>().IsNull())
                                    continue;

                                tempDamageableHitBox = tempGameObject.GetComponent<DamageableHitBox>();
                                if (tempDamageableHitBox == null)
                                    continue;

                                if (tempDamageableHitBox.GetObjectId() == attacker.ObjectId)
                                    continue;

                                if (hitBoxes.ContainsKey(tempDamageableHitBox.GetObjectId()))
                                    continue;

                                // Add entity to table, if it found entity in the table next time it will skip. 
                                // So it won't applies damage to entity repeatly.
                                hitBoxes[tempDamageableHitBox.GetObjectId()] = tempDamageableHitBox.Index;

                                // Target won't receive damage if dead or can't receive damage from this character
                                if (tempDamageableHitBox.IsDead() || !tempDamageableHitBox.CanReceiveDamageFrom(instigator) ||
                                    !attacker.IsPositionInFov(hitFov, tempDamageableHitBox.GetTransform().position))
                                    continue;

                                if (hitOnlySelectedTarget)
                                {
                                    // Check with selected target
                                    if (hasSelectedTarget && selectedTarget.GetObjectId() == tempDamageableHitBox.GetObjectId())
                                    {
                                        // This is selected target, so this is character which must receives damages
                                        damageTakenTargetIndex = tempLoopCounter;
                                        damageReceivingTarget = tempDamageableHitBox;
                                        break;
                                    }
                                    // Set damage taken targetit will be used in-case it can't find selected target
                                    damageTakenTargetIndex = tempLoopCounter;
                                    damageReceivingTarget = tempDamageableHitBox;
                                    // Hit only selected target, will apply damage later (outside this loop)
                                    continue;
                                }

                                // Target receives damages
                                if (isHost || isOwnedByServer)
                                    tempDamageableHitBox.ReceiveDamage(attacker.EntityTransform.position, instigator, damageAmounts, weapon, skill, skillLevel, randomSeed);

                                // It hit something, store data for hit register preparation later
                                if (!isHost && isOwnerClient)
                                {
                                    hitDataCollection.Add(new HitData()
                                    {
                                        HitObjectId = tempDamageableHitBox.GetObjectId(),
                                        HitBoxIndex = tempDamageableHitBox.Index,
                                        HitPoint = tempDamageableHitBox.transform.position,
                                    });
                                }

                                // Instantiate impact effects
                                if (isClient && hasImpactEffects)
                                {
                                    Vector3 closestPoint = attacker.AttackPhysicFunctions.GetOverlapColliderClosestPoint(tempLoopCounter, damagePosition);
                                    PoolSystem.GetInstance(impactEffects.TryGetEffect(tempDamageableHitBox.tag), closestPoint, Quaternion.LookRotation((closestPoint - damagePosition).normalized));
                                }
                            }

                            if (hitOnlySelectedTarget && damageReceivingTarget != null)
                            {
                                // Only 1 target will receives damages
                                // Pass all receive damage condition, then apply damages
                                if (isHost || isOwnedByServer)
                                    damageReceivingTarget.ReceiveDamage(attacker.EntityTransform.position, instigator, damageAmounts, weapon, skill, skillLevel, randomSeed);

                                // It hit something, store data for hit register preparation later
                                if (!isHost && isOwnerClient)
                                {
                                    hitDataCollection.Add(new HitData()
                                    {
                                        HitObjectId = damageReceivingTarget.GetObjectId(),
                                        HitBoxIndex = damageReceivingTarget.Index,
                                        HitPoint = damageReceivingTarget.transform.position,
                                    });
                                }

                                // Instantiate impact effects
                                if (isClient && hasImpactEffects)
                                {
                                    Vector3 closestPoint = attacker.AttackPhysicFunctions.GetOverlapColliderClosestPoint(damageTakenTargetIndex, damagePosition);
                                    PoolSystem.GetInstance(impactEffects.TryGetEffect(damageReceivingTarget.tag), closestPoint, Quaternion.LookRotation((closestPoint - damagePosition).normalized));
                                }
                            }
                            // Prepare hit registration
                            if (!isHost && isOwnerClient && hitDataCollection.Count > 0)
                                BaseGameNetworkManager.Singleton.HitRegistrationManager.PrepareToRegister(this, randomSeed, attacker, damagePosition, damageDirection, hitDataCollection);
                        }
                    }
                    break;
                case DamageType.Missile:
                    // Spawn missile damage entity, it will move to target then apply damage when hit
                    // Instantiates on both client and server (damage applies at server)
                    if (missileDamageEntity != null)
                    {
                        if (hitOnlySelectedTarget)
                        {
                            if (!attacker.TryGetTargetEntity(out tempDamageableHitBox))
                                tempDamageableHitBox = null;
                        }
                        // TODO: May predict and move missile ahead of time based on client's RTT
                        PoolSystem.GetInstance(missileDamageEntity, damagePosition, damageRotation)
                            .Setup(instigator, weapon, damageAmounts, skill, skillLevel, missileDistance, missileSpeed, tempDamageableHitBox);
                    }
                    break;
                case DamageType.Raycast:
                    if (isOwnedByServer || isClient)
                    {
                        float minDistance = missileDistance;
                        // Just raycast to any entity to apply damage
                        int tempRaycastSize = attacker.AttackPhysicFunctions.Raycast(damagePosition, damageDirection, missileDistance, GameInstance.Singleton.GetDamageEntityHitLayerMask());
                        if (tempRaycastSize > 0)
                        {
                            List<HitData> hitDataCollection = new List<HitData>();
                            byte pierceThroughEntities = this.pierceThroughEntities;
                            Vector3 point;
                            Vector3 normal;
                            float distance;
                            // Find characters that receiving damages
                            for (int tempLoopCounter = 0; tempLoopCounter < tempRaycastSize; ++tempLoopCounter)
                            {
                                point = attacker.AttackPhysicFunctions.GetRaycastPoint(tempLoopCounter);
                                normal = attacker.AttackPhysicFunctions.GetRaycastNormal(tempLoopCounter);
                                distance = attacker.AttackPhysicFunctions.GetRaycastDistance(tempLoopCounter);
                                tempGameObject = attacker.AttackPhysicFunctions.GetRaycastObject(tempLoopCounter);

                                if (!tempGameObject.GetComponent<IUnHittable>().IsNull())
                                    continue;

                                if (distance < minDistance)
                                    minDistance = distance;

                                tempDamageableHitBox = tempGameObject.GetComponent<DamageableHitBox>();
                                if (tempDamageableHitBox == null)
                                {
                                    if (!GameInstance.Singleton.IsDamageableLayer(tempGameObject.layer))
                                    {
                                        // Hit wall... so break the loop
                                        break;
                                    }
                                    continue;
                                }

                                if (tempDamageableHitBox.GetObjectId() == attacker.ObjectId)
                                    continue;

                                if (hitBoxes.ContainsKey(tempDamageableHitBox.GetObjectId()))
                                    continue;

                                // Add entity to table, if it found entity in the table next time it will skip. 
                                // So it won't applies damage to entity repeatly.
                                hitBoxes[tempDamageableHitBox.GetObjectId()] = tempDamageableHitBox.Index;

                                // Target won't receive damage if dead or can't receive damage from this character
                                if (tempDamageableHitBox.IsDead() || !tempDamageableHitBox.CanReceiveDamageFrom(instigator))
                                    continue;

                                // Target receives damages
                                if (isHost || isOwnedByServer)
                                    tempDamageableHitBox.ReceiveDamage(attacker.EntityTransform.position, instigator, damageAmounts, weapon, skill, skillLevel, randomSeed);

                                // It hit something, store data for hit register preparation later
                                if (!isHost && isOwnerClient)
                                {
                                    hitDataCollection.Add(new HitData()
                                    {
                                        HitObjectId = tempDamageableHitBox.GetObjectId(),
                                        HitBoxIndex = tempDamageableHitBox.Index,
                                        HitPoint = point,
                                    });
                                }

                                // Instantiate impact effects
                                if (isClient && hasImpactEffects)
                                    PoolSystem.GetInstance(impactEffects.TryGetEffect(tempDamageableHitBox.tag), point, Quaternion.LookRotation(Vector3.up, normal));

                                // Update pierce trough entities count
                                if (pierceThroughEntities <= 0)
                                    break;
                                --pierceThroughEntities;
                            }
                            // Prepare hit registration
                            if (!isHost && isOwnerClient && hitDataCollection.Count > 0)
                                BaseGameNetworkManager.Singleton.HitRegistrationManager.PrepareToRegister(this, randomSeed, attacker, damagePosition, damageDirection, hitDataCollection);
                        }
                        // Spawn projectile effect, it will move to target but it won't apply damage because it is just effect
                        if (isClient && projectileEffect != null)
                        {
                            PoolSystem.GetInstance(projectileEffect, damagePosition, damageRotation)
                                .Setup(minDistance, missileSpeed);
                        }
                    }
                    break;
                case DamageType.Throwable:
                    if (throwableDamageEntity != null)
                    {
                        // TODO: May predict and move missile ahead of time based on client's RTT
                        PoolSystem.GetInstance(throwableDamageEntity, damagePosition, damageRotation)
                            .Setup(instigator, weapon, damageAmounts, skill, skillLevel, throwForce, throwableLifeTime);
                    }
                    break;
            }

            // Trigger attacker's on launch damage entity event
            attacker.OnLaunchDamageEntity(
                isLeftHand,
                weapon,
                damageAmounts,
                skill,
                skillLevel,
                randomSeed,
                aimPosition,
                stagger,
                hitBoxes);
        }

        public void PrepareRelatesData()
        {
            GameInstance.AddPoolingObjects(new IPoolDescriptor[]
            {
                missileDamageEntity,
                throwableDamageEntity,
                projectileEffect,
            });
            if (customDamageInfo != null)
                customDamageInfo.PrepareRelatesData();
            if (impactEffects != null)
                impactEffects.PrepareRelatesData();
        }
    }

    [System.Serializable]
    public struct DamageAmount
    {
        [Tooltip("If `damageElement` is empty it will use default damage element from game instance")]
        public DamageElement damageElement;
        public MinMaxFloat amount;
    }

    [System.Serializable]
    public struct DamageRandomAmount
    {
        [Tooltip("If `damageElement` is empty it will use default damage element from game instance")]
        public DamageElement damageElement;
        public MinMaxFloat minAmount;
        public MinMaxFloat maxAmount;
        [Range(0, 1f)]
        public float applyRate;

        public bool Apply(System.Random random)
        {
            return random.NextDouble() <= applyRate;
        }

        public DamageAmount GetRandomedAmount(System.Random random)
        {
            return new DamageAmount()
            {
                damageElement = damageElement,
                amount = new MinMaxFloat()
                {
                    min = random.RandomFloat(minAmount.min, minAmount.max),
                    max = random.RandomFloat(maxAmount.min, maxAmount.max),
                },
            };
        }
    }

    [System.Serializable]
    public struct DamageIncremental
    {
        [Tooltip("If `damageElement` is empty it will use default damage element from game instance")]
        public DamageElement damageElement;
        public IncrementalMinMaxFloat amount;
    }

    [System.Serializable]
    public struct DamageEffectivenessAttribute
    {
        public Attribute attribute;
        public float effectiveness;
    }

    [System.Serializable]
    public struct DamageInflictionAmount
    {
        public DamageElement damageElement;
        public float rate;
    }

    [System.Serializable]
    public struct DamageInflictionIncremental
    {
        public DamageElement damageElement;
        public IncrementalFloat rate;
    }
}
