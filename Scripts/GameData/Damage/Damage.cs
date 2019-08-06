using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public enum DamageType
    {
        Melee,
        Missile,
        Raycast,
    }

    [System.Serializable]
    public class DamageInfo
    {
        public DamageType damageType;

        [StringShowConditional(conditionFieldName: "damageType", conditionValues: new string[] { "Melee", "Missile" })]
        [Tooltip("If this is TRUE, it will hit only selected target, if no selected target it will hit 1 found target")]
        public bool hitOnlySelectedTarget;

        [StringShowConditional(conditionFieldName: "damageType", conditionValues: new string[] { "Melee" })]
        public float hitDistance = 1f;
        [StringShowConditional(conditionFieldName: "damageType", conditionValues: new string[] { "Melee" })]
        [Range(10f, 360f)]
        public float hitFov;

        [StringShowConditional(conditionFieldName: "damageType", conditionValues: new string[] { "Missile", "Raycast" })]
        public float missileDistance = 5f;
        [StringShowConditional(conditionFieldName: "damageType", conditionValues: new string[] { "Missile" })]
        public float missileSpeed = 5f;
        [StringShowConditional(conditionFieldName: "damageType", conditionValues: new string[] { "Missile" })]
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
        [Tooltip("You can leave Damage to be empty to make it as physical damage which won't calculate with resistance stats")]
        public DamageElement damageElement;
        public MinMaxFloat amount;
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
