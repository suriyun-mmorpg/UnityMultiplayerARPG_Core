using UnityEngine;

namespace MultiplayerARPG
{
    public enum DamageType : byte
    {
        Melee,
        Missile,
        Raycast,
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
        // TODO: Add impact effect

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
            }
            return fov;
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
