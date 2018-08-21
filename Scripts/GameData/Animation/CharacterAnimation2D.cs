using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public enum DirectionType
    {
        Down,
        Up,
        Left,
        Right,
    }

    [System.Serializable]
    public class CharacterAnimation2D
    {
        public AnimationClip down;
        public AnimationClip up;
        public AnimationClip left;
        public AnimationClip right;
        [Tooltip("Set it more than zero to override default trigger duration rate")]
        [Range(0f, 1f)]
        public float triggerDurationRate;
        public float extraDuration;

        public AnimationClip GetClipByDirection(DirectionType directionType)
        {
            switch (directionType)
            {
                case DirectionType.Down:
                    return down;
                case DirectionType.Up:
                    return up;
                case DirectionType.Left:
                    return left;
                case DirectionType.Right:
                    return right;
            }
            return down;
        }
    }

    [System.Serializable]
    public struct WeaponAttack2D
    {
        public WeaponType weaponType;
        public CharacterAnimation2D animation;
    }

    [System.Serializable]
    public struct SkillCast2D
    {
        public Skill skill;
        public CharacterAnimation2D animation;
    }
}
