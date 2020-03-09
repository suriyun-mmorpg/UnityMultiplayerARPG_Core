using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public enum DamageType : byte
    {
        Melee,
        Missile,
        Raycast,
        Custom = 255
    }

    [System.Serializable]
    public struct DamageInfo
    {
        public DamageType damageType;

        [StringShowConditional(conditionFieldName: "damageType", conditionValues: new string[] { "Melee", "Missile" })]
        [Tooltip("If this is TRUE, it will hit only selected target, if no selected target it will hit 1 random target")]
        public bool hitOnlySelectedTarget;

        [StringShowConditional(conditionFieldName: "damageType", conditionValues: new string[] { "Melee" })]
        public float hitDistance;
        [StringShowConditional(conditionFieldName: "damageType", conditionValues: new string[] { "Melee" })]
        [Range(10f, 360f)]
        public float hitFov;

        [StringShowConditional(conditionFieldName: "damageType", conditionValues: new string[] { "Missile", "Raycast" })]
        public float missileDistance;
        [StringShowConditional(conditionFieldName: "damageType", conditionValues: new string[] { "Missile", "Raycast" })]
        public float missileSpeed;
        [StringShowConditional(conditionFieldName: "damageType", conditionValues: new string[] { "Missile" })]
        public MissileDamageEntity missileDamageEntity;

        [StringShowConditional(conditionFieldName: "damageType", conditionValues: new string[] { "Raycast" })]
        public ProjectileEffect projectileEffect;
        [StringShowConditional(conditionFieldName: "damageType", conditionValues: new string[] { "Raycast" })]
        public byte pierceThroughEntities;
        // TODO: Add impact effect

        [StringShowConditional(conditionFieldName: "damageType", conditionValues: new string[] { "Custom" })]
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
                    transform = customDamageInfo.GetDamageTransform(attacker, isLeftHand);
                    break;
            }
            return transform;
        }

        private void GetDamagePositionAndRotation(BaseCharacterEntity attacker, bool isLeftHand, Vector3 aimPosition, Vector3 stagger, out Vector3 position, out Vector3 direction, out Quaternion rotation)
        {
            if (GameInstance.Singleton.DimensionType == DimensionType.Dimension2D)
                GetDamagePositionAndRotation2D(attacker, isLeftHand, aimPosition, stagger, out position, out direction, out rotation);
            else
                GetDamagePositionAndRotation3D(attacker, isLeftHand, aimPosition, stagger, out position, out direction, out rotation);
        }

        private void GetDamagePositionAndRotation2D(BaseCharacterEntity attacker, bool isLeftHand, Vector3 aimPosition, Vector3 stagger, out Vector3 position, out Vector3 direction, out Quaternion rotation)
        {
            Transform transform = GetDamageTransform(attacker, isLeftHand);
            position = transform.position;
            direction = attacker.Direction2D;
            rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(direction.y, direction.x) * (180 / Mathf.PI)) + 90);
        }

        private void GetDamagePositionAndRotation3D(BaseCharacterEntity attacker, bool isLeftHand, Vector3 aimPosition, Vector3 stagger, out Vector3 position, out Vector3 direction, out Quaternion rotation)
        {
            Transform aimTransform = GetDamageTransform(attacker, isLeftHand);
            position = aimTransform.position;
            Quaternion forwardRotation = Quaternion.LookRotation(aimPosition - position);
            Vector3 forwardStagger = forwardRotation * stagger;
            direction = aimPosition + forwardStagger - position;
            rotation = Quaternion.LookRotation(direction);
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

            IDamageableEntity tempDamageableEntity = null;
            Vector3 damagePosition;
            Vector3 damageDirection;
            Quaternion damageRotation;

            GetDamagePositionAndRotation(attacker, isLeftHand, aimPosition, stagger, out damagePosition, out damageDirection, out damageRotation);
#if UNITY_EDITOR
            attacker.SetDebugDamage(damagePosition, damageRotation);
#endif

            GameObject tempGameObject;
            HashSet<uint> hitObjectIds = new HashSet<uint>();
            switch (damageType)
            {
                case DamageType.Melee:
                    if (hitOnlySelectedTarget)
                    {
                        IDamageableEntity damageTakenTarget = null;
                        IDamageableEntity selectedTarget = null;
                        bool hasSelectedTarget = attacker.TryGetTargetEntity(out selectedTarget);
                        // If hit only selected target, find selected character (only 1 character) to apply damage
                        int tempOverlapSize = attacker.OverlapObjects_ForAttackFunctions(damagePosition, hitDistance, damageableLayerMask);
                        if (tempOverlapSize == 0)
                            return;

                        // Find characters that receiving damages
                        for (int tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
                        {
                            tempGameObject = attacker.GetOverlapObject_ForAttackFunctions(tempLoopCounter);
                            // Skip unhittable entities
                            if (tempGameObject.GetComponent<IUnHittable>() != null)
                                continue;

                            tempDamageableEntity = tempGameObject.GetComponent<IDamageableEntity>();
                            if (tempDamageableEntity == null)
                                continue;

                            if (tempDamageableEntity.GetObjectId() == attacker.ObjectId ||
                                hitObjectIds.Contains(tempDamageableEntity.GetObjectId()))
                                continue;

                            // Add entity to table, if it found entity in the table next time it will skip. 
                            // So it won't applies damage to entity repeatly.
                            hitObjectIds.Add(tempDamageableEntity.GetObjectId());

                            // Target won't receive damage if dead or can't receive damage from this character
                            if (tempDamageableEntity.IsDead() || !tempDamageableEntity.CanReceiveDamageFrom(attacker) ||
                                !attacker.IsPositionInFov(hitFov, tempDamageableEntity.GetTransform().position))
                                continue;

                            // Check with selected target
                            if (hasSelectedTarget && selectedTarget.GetObjectId() == tempDamageableEntity.GetObjectId())
                            {
                                // This is selected target, so this is character which must receives damages
                                damageTakenTarget = tempDamageableEntity;
                                break;
                            }
                            // Set damage taken targetit will be used in-case it can't find selected target
                            damageTakenTarget = tempDamageableEntity;
                        }
                        // Only 1 target will receives damages
                        if (damageTakenTarget != null)
                        {
                            // Pass all receive damage condition, then apply damages
                            if (isClient)
                                damageTakenTarget.PlayHitEffects(damageAmounts.Keys, skill);
                            if (isServer)
                                damageTakenTarget.ReceiveDamage(attacker, weapon, damageAmounts, skill, skillLevel);
                        }
                    }
                    else
                    {
                        // If not hit only selected target, find characters within hit fov to applies damages
                        int tempOverlapSize = attacker.OverlapObjects_ForAttackFunctions(damagePosition, hitDistance, damageableLayerMask);
                        if (tempOverlapSize == 0)
                            return;

                        // Find characters that receiving damages
                        for (int tempLoopCounter = 0; tempLoopCounter < tempOverlapSize; ++tempLoopCounter)
                        {
                            tempGameObject = attacker.GetOverlapObject_ForAttackFunctions(tempLoopCounter);
                            // Skip unhittable entities
                            if (tempGameObject.GetComponent<IUnHittable>() != null)
                                continue;

                            tempDamageableEntity = tempGameObject.GetComponent<IDamageableEntity>();
                            if (tempDamageableEntity == null)
                                continue;

                            if (tempDamageableEntity.GetObjectId() == attacker.ObjectId ||
                                hitObjectIds.Contains(tempDamageableEntity.GetObjectId()))
                                continue;

                            // Add entity to table, if it found entity in the table next time it will skip. 
                            // So it won't applies damage to entity repeatly.
                            hitObjectIds.Add(tempDamageableEntity.GetObjectId());

                            // Target won't receive damage if dead or can't receive damage from this character
                            if (tempDamageableEntity.IsDead() ||
                                !tempDamageableEntity.CanReceiveDamageFrom(attacker) ||
                                !attacker.IsPositionInFov(hitFov, tempDamageableEntity.GetTransform().position))
                                continue;

                            // Target receives damages
                            if (isClient)
                                tempDamageableEntity.PlayHitEffects(damageAmounts.Keys, skill);
                            if (isServer)
                                tempDamageableEntity.ReceiveDamage(attacker, weapon, damageAmounts, skill, skillLevel);
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
                            if (!attacker.TryGetTargetEntity(out tempDamageableEntity))
                                tempDamageableEntity = null;
                        }
                        Object.Instantiate(missileDamageEntity, damagePosition, damageRotation)
                            .Setup(attacker, weapon, damageAmounts, skill, skillLevel, missileDistance, missileSpeed, tempDamageableEntity);
                    }
                    break;
                case DamageType.Raycast:
                    float minDistance = missileDistance;
                    // Just raycast to any entity to apply damage
                    int tempRaycastSize = attacker.RaycastObjects_ForAttackFunctions(damagePosition, damageDirection, missileDistance, Physics.DefaultRaycastLayers);
                    if (tempRaycastSize > 0)
                    {
                        // Sort index
                        Vector3 point;
                        Vector3 normal;
                        float distance;
                        // Find characters that receiving damages
                        for (int tempLoopCounter = 0; tempLoopCounter < tempRaycastSize; ++tempLoopCounter)
                        {
                            tempGameObject = attacker.GetRaycastObject_ForAttackFunctions(tempLoopCounter, out point, out normal, out distance).gameObject;

                            // Skip layers
                            if (tempGameObject.layer == PhysicLayers.TransparentFX ||
                                tempGameObject.layer == PhysicLayers.IgnoreRaycast ||
                                tempGameObject.layer == PhysicLayers.Water)
                                return;

                            // Skip unhittable entities
                            if (tempGameObject.GetComponent<IUnHittable>() != null)
                                continue;

                            if (distance < minDistance)
                                minDistance = distance;

                            tempDamageableEntity = tempGameObject.GetComponent<IDamageableEntity>();
                            // Hit wall... so break the loop
                            if (tempDamageableEntity == null)
                                break;

                            if (tempDamageableEntity.GetObjectId() == attacker.ObjectId ||
                                hitObjectIds.Contains(tempDamageableEntity.GetObjectId()))
                                continue;

                            // Add entity to table, if it found entity in the table next time it will skip. 
                            // So it won't applies damage to entity repeatly.
                            hitObjectIds.Add(tempDamageableEntity.GetObjectId());

                            // Target won't receive damage if dead or can't receive damage from this character
                            if (tempDamageableEntity.IsDead() ||
                                !tempDamageableEntity.CanReceiveDamageFrom(attacker))
                                continue;

                            // Target receives damages
                            if (isClient)
                                tempDamageableEntity.PlayHitEffects(damageAmounts.Keys, skill);
                            if (isServer)
                                tempDamageableEntity.ReceiveDamage(attacker, weapon, damageAmounts, skill, skillLevel);

                            // Update pierce trough entities count
                            if (pierceThroughEntities <= 0)
                                break;
                            --pierceThroughEntities;
                        } // End of for...loop (raycast result)
                    }
                    // Spawn projectile effect, it will move to target but it won't apply damage because it is just effect
                    if (isClient && projectileEffect != null)
                    {
                        Object.Instantiate(projectileEffect, damagePosition, damageRotation)
                            .Setup(minDistance, missileSpeed);
                    }
                    break;
            }
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
