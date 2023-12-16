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

        [Tooltip("Distance to start an attack, this is NOT distance to hit and apply damage, this value should be less than `hitDistance` or `missileDistance` to make sure it will hit the enemy properly. If this value <= 0 or > `hitDistance` or `missileDistance` it will re-calculate by `hitDistance` or `missileDistance`")]
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

        private BaseCustomDamageInfo _builtInDamageInfo;

        private BaseCustomDamageInfo GetDamageInfo()
        {
            switch (damageType)
            {
                case DamageType.Custom:
                    return customDamageInfo;
                case DamageType.Throwable:
                    if (_builtInDamageInfo == null)
                    {
                        ThrowableDamageInfo tempThrowableDamageInfo = ScriptableObject.CreateInstance<ThrowableDamageInfo>();
                        tempThrowableDamageInfo.throwForce = throwForce;
                        tempThrowableDamageInfo.throwableLifeTime = throwableLifeTime;
                        tempThrowableDamageInfo.throwableDamageEntity = throwableDamageEntity;
                        _builtInDamageInfo = tempThrowableDamageInfo;
                    }
                    break;
                case DamageType.Raycast:
                    if (_builtInDamageInfo == null)
                    {
                        RaycastDamageInfo tempRaycastDamageInfo = ScriptableObject.CreateInstance<RaycastDamageInfo>();
                        tempRaycastDamageInfo.missileDistance = missileDistance;
                        tempRaycastDamageInfo.missileSpeed = missileSpeed;
                        tempRaycastDamageInfo.projectileEffect = projectileEffect;
                        tempRaycastDamageInfo.pierceThroughEntities = pierceThroughEntities;
                        tempRaycastDamageInfo.impactEffects = impactEffects;
                        _builtInDamageInfo = tempRaycastDamageInfo;
                    }
                    break;
                case DamageType.Missile:
                    if (_builtInDamageInfo == null)
                    {
                        MissileDamageInfo tempMissileDamageInfo = ScriptableObject.CreateInstance<MissileDamageInfo>();
                        tempMissileDamageInfo.hitOnlySelectedTarget = hitOnlySelectedTarget;
                        tempMissileDamageInfo.missileDistance = missileDistance;
                        tempMissileDamageInfo.missileSpeed = missileSpeed;
                        tempMissileDamageInfo.missileDamageEntity = missileDamageEntity;
                        _builtInDamageInfo = tempMissileDamageInfo;
                    }
                    break;
                default:
                    if (_builtInDamageInfo == null)
                    {
                        MeleeDamageInfo tempMeleeDamageInfo = ScriptableObject.CreateInstance<MeleeDamageInfo>();
                        tempMeleeDamageInfo.hitOnlySelectedTarget = hitOnlySelectedTarget;
                        tempMeleeDamageInfo.hitDistance = hitDistance;
                        tempMeleeDamageInfo.hitFov = hitFov;
                        tempMeleeDamageInfo.impactEffects = impactEffects;
                        _builtInDamageInfo = tempMeleeDamageInfo;
                    }
                    break;
            }
            return _builtInDamageInfo;
        }

        public float GetDistance()
        {
            float dist = GetDamageInfo().GetDistance();
            if (startAttackDistance > 0 && startAttackDistance < dist)
                dist = startAttackDistance;
            return dist;
        }

        public float GetFov()
        {
            return GetDamageInfo().GetFov();
        }

        public Transform GetDamageTransform(BaseCharacterEntity attacker, bool isLeftHand)
        {
            return GetDamageInfo().GetDamageTransform(attacker, isLeftHand);
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
            AimPosition aimPosition)
        {
            // No attacker
            if (attacker == null)
                return;

            // Don't launch if character dead
            if (attacker.IsServer && attacker.IsDead())
                return;

            GetDamageInfo().LaunchDamageEntity(
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
                aimPosition);

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

        public void PrepareRelatesData()
        {
            GetDamageInfo().PrepareRelatesData();
        }

        public bool IsHitValid(HitValidateData hitValidateData, HitRegisterData hitData, DamageableHitBox hitBox)
        {
            return GetDamageInfo().IsHitValid(hitValidateData, hitData, hitBox);
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
