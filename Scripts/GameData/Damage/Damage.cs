using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public enum DamageType
    {
        Melee,
        Missile,
    }

    [System.Serializable]
    public class DamageInfo
    {
        public DamageType damageType;

        [Tooltip("If this is TRUE, it will hit only selected target, if no selected target it will hit 1 found target")]
        public bool hitOnlySelectedTarget;

        [StringShowConditional(conditionFieldName: "damageType", conditionValue: "Melee")]
        [Tooltip("This will be sum with character's radius before find hitting characters")]
        public float hitDistance = 1f;
        [StringShowConditional(conditionFieldName: "damageType", conditionValue: "Melee")]
        [Range(10f, 360f)]
        public float hitFov;

        [StringShowConditional(conditionFieldName: "damageType", conditionValue: "Missile")]
        public float missileDistance = 5f;
        [StringShowConditional(conditionFieldName: "damageType", conditionValue: "Missile")]
        public float missileSpeed = 5f;
        [StringShowConditional(conditionFieldName: "damageType", conditionValue: "Missile")]
        public MissileDamageEntity missileDamageEntity;

        public float GetDistance()
        {
            float distance = 0f;
            switch (damageType)
            {
                case DamageType.Melee:
                    distance = hitDistance;
                    break;
                case DamageType.Missile:
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
                    fov = 15f;
                    break;
            }
            return fov;
        }
    }

    [System.Serializable]
    public struct DamageIncremental
    {
        [Tooltip("You can leave Damage to be empty to make it as physical damage which won't calculate with resistance stats")]
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
