using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public enum DamageType : byte
    {
        Melee,
        Missile,
        Raycast,
        Custom = 254
    }

    [System.Serializable]
    public struct DamageInfo
    {
        public DamageType damageType;

        [StringShowConditional(nameof(damageType), new string[] { nameof(DamageType.Melee), nameof(DamageType.Missile) })]
        [Tooltip("If this is TRUE, it will hit only selected target, if no selected target it will hit 1 random target")]
        public bool hitOnlySelectedTarget;

        [StringShowConditional(nameof(damageType), new string[] { nameof(DamageType.Melee) })]
        public float hitDistance;
        [StringShowConditional(nameof(damageType), new string[] { nameof(DamageType.Melee) })]
        [Range(10f, 360f)]
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
        [StringShowConditional(nameof(damageType), new string[] { nameof(DamageType.Raycast) })]
        public ImpactEffects impactEffects;

        [StringShowConditional(nameof(damageType), new string[] { nameof(DamageType.Custom) })]
        public BaseCustomDamageInfo customDamageInfo;

        public float GetDistance()
        {
            float distance = 0f;
            switch (damageType)
            {
                case DamageType.Melee:
                    distance = hitDistance;
                    break;
                case DamageType.Missile:
                case DamageType.Raycast:
                    distance = missileDistance;
                    break;
                case DamageType.Custom:
                    distance = customDamageInfo.GetDistance();
                    break;
            }
            return distance;
        }

        public float GetFov()
        {
            float fov = 0f;
            switch (damageType)
            {
                case DamageType.Melee:
                    fov = hitFov;
                    break;
                case DamageType.Missile:
                case DamageType.Raycast:
                    fov = 10f;
                    break;
                case DamageType.Custom:
                    fov = customDamageInfo.GetFov();
                    break;
            }
            return fov;
        }

        public Transform GetDamageTransform(BaseCharacterEntity attacker, bool isLeftHand)
        {
            Transform transform = null;
            switch (damageType)
            {
                case DamageType.Melee:
                    // Use melee damage transform for distance calculation
                    transform = attacker.MeleeDamageTransform;
                    break;
                case DamageType.Missile:
                case DamageType.Raycast:
                    // Always use missile transform for distance calculation
                    // custom transforms (set via `EquipmentEntity`) will be used for muzzle effects and fake shot effects only
                    transform = attacker.MissileDamageTransform;
                    break;
                case DamageType.Custom:
                    transform = customDamageInfo.GetDamageTransform(attacker, isLeftHand);
                    break;
            }
            return transform;
        }

        public Transform GetDamageEffectTransform(BaseCharacterEntity attacker, bool isLeftHand)
        {
            Transform transform = null;
            switch (damageType)
            {
                case DamageType.Melee:
                    transform = attacker.MeleeDamageTransform;
                    break;
                case DamageType.Missile:
                case DamageType.Raycast:
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
                    transform = customDamageInfo.GetDamageEffectTransform(attacker, isLeftHand);
                    break;
            }
            return transform;
        }

        private void GetDamagePositionAndRotation(BaseCharacterEntity attacker, bool isLeftHand, bool forEffect, Vector3 aimPosition, Vector3 stagger, out Vector3 position, out Vector3 direction, out Quaternion rotation)
        {
            if (GameInstance.Singleton.DimensionType == DimensionType.Dimension2D)
                GetDamagePositionAndRotation2D(attacker, isLeftHand, forEffect, aimPosition, stagger, out position, out direction, out rotation);
            else
                GetDamagePositionAndRotation3D(attacker, isLeftHand, forEffect, aimPosition, stagger, out position, out direction, out rotation);
        }

        private void GetDamagePositionAndRotation2D(BaseCharacterEntity attacker, bool isLeftHand, bool forEffect, Vector3 aimPosition, Vector3 stagger, out Vector3 position, out Vector3 direction, out Quaternion rotation)
        {
            Transform transform = forEffect ? GetDamageEffectTransform(attacker, isLeftHand) : GetDamageTransform(attacker, isLeftHand);
            position = transform.position;
            direction = attacker.Direction2D;
            rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(direction.y, direction.x) * (180 / Mathf.PI)) + 90);
        }

        private void GetDamagePositionAndRotation3D(BaseCharacterEntity attacker, bool isLeftHand, bool forEffect, Vector3 aimPosition, Vector3 stagger, out Vector3 position, out Vector3 direction, out Quaternion rotation)
        {
            Transform aimTransform = forEffect ? GetDamageEffectTransform(attacker, isLeftHand) : GetDamageTransform(attacker, isLeftHand);
            position = aimTransform.position;
            Vector3 eulerAngles = Quaternion.LookRotation(aimPosition - position).eulerAngles + stagger;
            rotation = Quaternion.Euler(eulerAngles);
            direction = rotation * Vector3.forward;
        }

        /// <summary>
        /// This function can be called at both client and server
        /// For server it will instantiates damage entities if needed
        /// For client it will instantiates special effects
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="isLeftHand"></param>
        /// <param name="weapon"></param>
        /// <param name="damageAmounts"></param>
        /// <param name="skill"></param>
        /// <param name="skillLevel"></param>
        /// <param name="aimPosition"></param>
        /// <param name="stagger"></param>
        public void LaunchDamageEntity(
            BaseCharacterEntity attacker,
            bool isLeftHand,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            short skillLevel,
            Vector3 aimPosition,
            Vector3 stagger)
        {
            if (attacker == null)
                return;

            if (damageType == DamageType.Custom)
            {
                customDamageInfo.LaunchDamageEntity(
                    attacker,
                    isLeftHand,
                    weapon,
                    damageAmounts,
                    skill,
                    skillLevel,
                    aimPosition,
                    stagger);
                return;
            }

            bool isServer = attacker.IsServer;
            bool isClient = attacker.IsClient;
            int damageableLayerMask = GameInstance.Singleton.GetDamageableLayerMask();

            DamageableHitBox tempDamageableHitBox = null;

            // Damage transform data
            Vector3 damagePosition;
            Vector3 damageDirection;
            Quaternion damageRotation;
            GetDamagePositionAndRotation(attacker, isLeftHand, false, aimPosition, stagger, out damagePosition, out damageDirection, out damageRotation);

            // Damage effect transform data
            Vector3 damageEffectPosition;
            Vector3 damageEffectDirection;
            Quaternion damageEffectRotation;
            GetDamagePositionAndRotation(attacker, isLeftHand, true, aimPosition, stagger, out damageEffectPosition, out damageEffectDirection, out damageEffectRotation);
#if UNITY_EDITOR
            attacker.SetDebugDamage(damagePosition, damageDirection, damageRotation);
#endif

            GameObject tempGameObject;
            HashSet<uint> hitObjectIds = new HashSet<uint>();
            switch (damageType)
            {
                case DamageType.Melee:
                    if (hitOnlySelectedTarget)
                    {
                        DamageableHitBox damageTakenTarget = null;
                        DamageableEntity selectedTarget = null;
                        bool hasSelectedTarget = attacker.TryGetTargetEntity(out selectedTarget);
                        // If hit only selected target, find selected character (only 1 character) to apply damage
                        int tempOverlapSize = attacker.AttackPhysicFunctions.OverlapObjects(damagePosition, hitDistance, damageableLayerMask, true);
                        if (tempOverlapSize == 0)
                            return;

                        // Find characters that receiving damages
                        for (int tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
                        {
                            tempGameObject = attacker.AttackPhysicFunctions.GetOverlapObject(tempLoopCounter);
                            // Skip unhittable entities
                            if (tempGameObject.GetComponent<IUnHittable>() != null)
                                continue;

                            tempDamageableHitBox = tempGameObject.GetComponent<DamageableHitBox>();
                            if (tempDamageableHitBox == null)
                                continue;

                            if (tempDamageableHitBox.GetObjectId() == attacker.ObjectId ||
                                hitObjectIds.Contains(tempDamageableHitBox.GetObjectId()))
                                continue;

                            // Add entity to table, if it found entity in the table next time it will skip. 
                            // So it won't applies damage to entity repeatly.
                            hitObjectIds.Add(tempDamageableHitBox.GetObjectId());

                            // Target won't receive damage if dead or can't receive damage from this character
                            if (tempDamageableHitBox.IsDead() || !tempDamageableHitBox.CanReceiveDamageFrom(attacker) ||
                                !attacker.IsPositionInFov(hitFov, tempDamageableHitBox.GetTransform().position))
                                continue;

                            // Check with selected target
                            if (hasSelectedTarget && selectedTarget.GetObjectId() == tempDamageableHitBox.GetObjectId())
                            {
                                // This is selected target, so this is character which must receives damages
                                damageTakenTarget = tempDamageableHitBox;
                                break;
                            }
                            // Set damage taken targetit will be used in-case it can't find selected target
                            damageTakenTarget = tempDamageableHitBox;
                        }
                        // Only 1 target will receives damages
                        if (damageTakenTarget != null)
                        {
                            // Pass all receive damage condition, then apply damages
                            if (isClient)
                                damageTakenTarget.PlayHitEffects(damageAmounts.Keys, skill);
                            if (isServer)
                                damageTakenTarget.ReceiveDamage(attacker.CacheTransform.position, attacker, damageAmounts, weapon, skill, skillLevel);
                        }
                    }
                    else
                    {
                        // If not hit only selected target, find characters within hit fov to applies damages
                        int tempOverlapSize = attacker.AttackPhysicFunctions.OverlapObjects(damagePosition, hitDistance, damageableLayerMask, true);
                        if (tempOverlapSize == 0)
                            return;

                        // Find characters that receiving damages
                        for (int tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
                        {
                            tempGameObject = attacker.AttackPhysicFunctions.GetOverlapObject(tempLoopCounter);

                            if (tempGameObject.GetComponent<IUnHittable>() != null)
                                continue;

                            tempDamageableHitBox = tempGameObject.GetComponent<DamageableHitBox>();
                            if (tempDamageableHitBox == null)
                                continue;

                            if (tempDamageableHitBox.GetObjectId() == attacker.ObjectId ||
                                hitObjectIds.Contains(tempDamageableHitBox.GetObjectId()))
                                continue;

                            // Add entity to table, if it found entity in the table next time it will skip. 
                            // So it won't applies damage to entity repeatly.
                            hitObjectIds.Add(tempDamageableHitBox.GetObjectId());

                            // Target won't receive damage if dead or can't receive damage from this character
                            if (tempDamageableHitBox.IsDead() ||
                                !tempDamageableHitBox.CanReceiveDamageFrom(attacker) ||
                                !attacker.IsPositionInFov(hitFov, tempDamageableHitBox.GetTransform().position))
                                continue;

                            // Target receives damages
                            if (isClient)
                                tempDamageableHitBox.PlayHitEffects(damageAmounts.Keys, skill);
                            if (isServer)
                                tempDamageableHitBox.ReceiveDamage(attacker.CacheTransform.position, attacker, damageAmounts, weapon, skill, skillLevel);
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
                        PoolSystem.GetInstance(missileDamageEntity, damageEffectPosition, damageEffectRotation)
                            .Setup(attacker, weapon, damageAmounts, skill, skillLevel, missileDistance, missileSpeed, tempDamageableHitBox);
                    }
                    break;
                case DamageType.Raycast:
                    float minDistance = missileDistance;
                    // Just raycast to any entity to apply damage
                    int tempRaycastSize = attacker.AttackPhysicFunctions.Raycast(damagePosition, damageDirection, missileDistance, Physics.DefaultRaycastLayers);
                    if (tempRaycastSize > 0)
                    {
                        // Sort index
                        Vector3 point;
                        Vector3 normal;
                        float distance;
                        bool hasImpactEffects = impactEffects != null;
                        // Find characters that receiving damages
                        for (int tempLoopCounter = 0; tempLoopCounter < tempRaycastSize; ++tempLoopCounter)
                        {
                            point = attacker.AttackPhysicFunctions.GetRaycastPoint(tempLoopCounter);
                            normal = attacker.AttackPhysicFunctions.GetRaycastNormal(tempLoopCounter);
                            distance = attacker.AttackPhysicFunctions.GetRaycastDistance(tempLoopCounter);
                            tempGameObject = attacker.AttackPhysicFunctions.GetRaycastColliderGameObject(tempLoopCounter);

                            if (tempGameObject.layer == PhysicLayers.TransparentFX ||
                                tempGameObject.layer == PhysicLayers.IgnoreRaycast ||
                                tempGameObject.layer == PhysicLayers.Water)
                                return;

                            if (tempGameObject.GetComponent<IUnHittable>() != null)
                                continue;

                            if (distance < minDistance)
                                minDistance = distance;

                            tempDamageableHitBox = tempGameObject.GetComponent<DamageableHitBox>();
                            // Hit wall... so break the loop
                            if (tempDamageableHitBox == null)
                                break;

                            if (tempDamageableHitBox.GetObjectId() == attacker.ObjectId ||
                                hitObjectIds.Contains(tempDamageableHitBox.GetObjectId()))
                                continue;

                            // Add entity to table, if it found entity in the table next time it will skip. 
                            // So it won't applies damage to entity repeatly.
                            hitObjectIds.Add(tempDamageableHitBox.GetObjectId());

                            // Target won't receive damage if dead or can't receive damage from this character
                            if (tempDamageableHitBox.IsDead() ||
                                !tempDamageableHitBox.CanReceiveDamageFrom(attacker))
                                continue;

                            // Target receives damages
                            if (isClient)
                                tempDamageableHitBox.PlayHitEffects(damageAmounts.Keys, skill);
                            if (isServer)
                                tempDamageableHitBox.ReceiveDamage(attacker.CacheTransform.position, attacker, damageAmounts, weapon, skill, skillLevel);

                            // Instantiate impact effects
                            if (isClient && hasImpactEffects)
                                PoolSystem.GetInstance(impactEffects.TryGetEffect(tempGameObject.tag), point, Quaternion.LookRotation(Vector3.up, normal));

                            // Update pierce trough entities count
                            if (pierceThroughEntities <= 0)
                                break;
                            --pierceThroughEntities;
                        } // End of for...loop (raycast result)
                    }
                    // Spawn projectile effect, it will move to target but it won't apply damage because it is just effect
                    if (isClient && projectileEffect != null)
                    {
                        PoolSystem.GetInstance(projectileEffect, damageEffectPosition, damageEffectRotation)
                            .Setup(minDistance, missileSpeed);
                    }
                    break;
            }
        }

        public void PrepareRelatesData()
        {
            GameInstance.AddPoolingObjects(new IPoolDescriptor[]
            {
                missileDamageEntity,
                projectileEffect
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
