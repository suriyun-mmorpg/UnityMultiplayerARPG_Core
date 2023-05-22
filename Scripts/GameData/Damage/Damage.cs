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
            int simulateSeed,
            byte triggerIndex,
            byte spreadIndex,
            Vector3 fireStagger,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            int skillLevel,
            AimPosition aimPosition,
            DamageOriginPreparedDelegate onOriginPrepared,
            DamageHitDelegate onHit)
        {
            if (attacker == null)
                return;

            switch (damageType)
            {
                case DamageType.Missile:
                    LaunchMissileDamage(
                        attacker,
                        isLeftHand,
                        weapon,
                        simulateSeed,
                        triggerIndex,
                        spreadIndex,
                        fireStagger,
                        damageAmounts,
                        skill,
                        skillLevel,
                        aimPosition,
                        onOriginPrepared,
                        onHit);
                    break;
                case DamageType.Raycast:
                    LaunchRaycastDamage(
                        attacker,
                        isLeftHand,
                        weapon,
                        simulateSeed,
                        triggerIndex,
                        spreadIndex,
                        fireStagger,
                        damageAmounts,
                        skill,
                        skillLevel,
                        aimPosition,
                        onOriginPrepared,
                        onHit);
                    break;
                case DamageType.Throwable:
                    LaunchThrowableDamage(
                        attacker,
                        isLeftHand,
                        weapon,
                        simulateSeed,
                        triggerIndex,
                        spreadIndex,
                        fireStagger,
                        damageAmounts,
                        skill,
                        skillLevel,
                        aimPosition,
                        onOriginPrepared,
                        onHit);
                    break;
                case DamageType.Custom:
                    customDamageInfo.LaunchDamageEntity(
                        attacker,
                        isLeftHand,
                        weapon,
                        simulateSeed,
                        triggerIndex,
                        spreadIndex,
                        fireStagger,
                        damageAmounts,
                        skill,
                        skillLevel,
                        aimPosition,
                        onOriginPrepared,
                        onHit);
                    break;
                default:
                    LaunchMeleeDamage(
                        attacker,
                        isLeftHand,
                        weapon,
                        simulateSeed,
                        triggerIndex,
                        spreadIndex,
                        fireStagger,
                        damageAmounts,
                        skill,
                        skillLevel,
                        aimPosition,
                        onOriginPrepared,
                        onHit);
                    break;
            }

            // Trigger attacker's on launch damage entity event
            attacker.OnLaunchDamageEntity(
                isLeftHand,
                weapon,
                simulateSeed,
                triggerIndex,
                spreadIndex,
                damageAmounts,
                skill,
                skillLevel,
                aimPosition);
        }

        private void LaunchMeleeDamage(
            BaseCharacterEntity attacker,
            bool isLeftHand,
            CharacterItem weapon,
            int simulateSeed,
            byte triggerIndex,
            byte spreadIndex,
            Vector3 fireStagger,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            int skillLevel,
            AimPosition aimPosition,
            DamageOriginPreparedDelegate onOriginPrepared,
            DamageHitDelegate onHit)
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
            if (onOriginPrepared != null)
                onOriginPrepared.Invoke(simulateSeed, triggerIndex, spreadIndex, damagePosition, damageDirection, damageRotation);

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

                // Trigger hit action because it is hitting
                if (onHit != null)
                    onHit.Invoke(simulateSeed, triggerIndex, spreadIndex, tempDamageableHitBox.GetObjectId(), tempDamageableHitBox.Index, tempDamageableHitBox.CacheTransform.position);

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

                // Trigger hit action because it is hitting
                if (onHit != null)
                    onHit.Invoke(simulateSeed, triggerIndex, spreadIndex, tempDamageTakenTarget.GetObjectId(), tempDamageTakenTarget.Index, tempDamageTakenTarget.CacheTransform.position);

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

        private void LaunchMissileDamage(
            BaseCharacterEntity attacker,
            bool isLeftHand,
            CharacterItem weapon,
            int simulateSeed,
            byte triggerIndex,
            byte spreadIndex,
            Vector3 fireStagger,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            int skillLevel,
            AimPosition aimPosition,
            DamageOriginPreparedDelegate onOriginPrepared,
            DamageHitDelegate onHit)
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
            if (onOriginPrepared != null)
                onOriginPrepared.Invoke(simulateSeed, triggerIndex, spreadIndex, damagePosition, damageDirection, damageRotation);

            DamageableEntity lockingTarget;
            if (!hitOnlySelectedTarget || !attacker.TryGetTargetEntity(out lockingTarget))
                lockingTarget = null;

            // Instantiate missile damage entity
            float missileDistance = this.missileDistance;
            float missileSpeed = this.missileSpeed;
            PoolSystem.GetInstance(missileDamageEntity, damagePosition, damageRotation).Setup(instigator, weapon, simulateSeed, triggerIndex, spreadIndex, damageAmounts, skill, skillLevel, onHit, missileDistance, missileSpeed, lockingTarget);
        }

        private void LaunchRaycastDamage(
            BaseCharacterEntity attacker,
            bool isLeftHand,
            CharacterItem weapon,
            int simulateSeed,
            byte triggerIndex,
            byte spreadIndex,
            Vector3 fireStagger,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            int skillLevel,
            AimPosition aimPosition,
            DamageOriginPreparedDelegate onOriginPrepared,
            DamageHitDelegate onHit)
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
            if (onOriginPrepared != null)
                onOriginPrepared.Invoke(simulateSeed, triggerIndex, spreadIndex, damagePosition, damageDirection, damageRotation);

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

                // Trigger hit action because it is hitting
                if (onHit != null)
                    onHit.Invoke(simulateSeed, triggerIndex, spreadIndex, tempDamageableHitBox.GetObjectId(), tempDamageableHitBox.Index, tempHitPoint);

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

        private void LaunchThrowableDamage(
            BaseCharacterEntity attacker,
            bool isLeftHand,
            CharacterItem weapon,
            int simulateSeed,
            byte triggerIndex,
            byte spreadIndex,
            Vector3 fireStagger,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            int skillLevel,
            AimPosition aimPosition,
            DamageOriginPreparedDelegate onOriginPrepared,
            DamageHitDelegate onHit)
        {
            if (throwableDamageEntity == null)
                return;

            // Get generic attack data
            EntityInfo instigator = attacker.GetInfo();
            System.Random random = new System.Random(unchecked(simulateSeed + ((triggerIndex + 1) * (spreadIndex + 1) * 16)));
            Vector3 stagger = new Vector3(GenericUtils.RandomFloat(random.Next(), -fireStagger.x, fireStagger.x), GenericUtils.RandomFloat(random.Next(), -fireStagger.y, fireStagger.y));
            this.GetDamagePositionAndRotation(attacker, isLeftHand, aimPosition, stagger, out Vector3 damagePosition, out Vector3 damageDirection, out Quaternion damageRotation);
            if (onOriginPrepared != null)
                onOriginPrepared.Invoke(simulateSeed, triggerIndex, spreadIndex, damagePosition, damageDirection, damageRotation);

            // Instantiate throwable damage entity
            // TODO: May predict and move missile ahead of time based on client's RTT
            float throwForce = this.throwForce;
            float throwableLifeTime = this.throwableLifeTime;
            PoolSystem.GetInstance(throwableDamageEntity, damagePosition, damageRotation).Setup(instigator, weapon, simulateSeed, triggerIndex, spreadIndex, damageAmounts, skill, skillLevel, onHit, throwForce, throwableLifeTime);
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

        public bool IsHitReachedMax(int alreadyHitCount)
        {
            switch (damageType)
            {
                case DamageType.Melee:
                    if (hitOnlySelectedTarget)
                        return alreadyHitCount > 0;
                    else
                        return false;
                case DamageType.Missile:
                case DamageType.Throwable:
                    // Can hit unlimited objects within attack/explode range
                    return false;
                case DamageType.Raycast:
                    return alreadyHitCount > pierceThroughEntities;
                case DamageType.Custom:
                    return customDamageInfo.IsHitReachedMax(alreadyHitCount);
            }
            return true;
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
