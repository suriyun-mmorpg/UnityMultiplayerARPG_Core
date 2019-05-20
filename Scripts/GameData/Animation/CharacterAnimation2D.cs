using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Flags]
    public enum DirectionType : byte
    {
        Down = 1 << 0,
        Up = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
        DownLeft = Down | Left,
        DownRight = Down | Right,
        UpLeft = Up | Left,
        UpRight = Up | Right,
    }

    [System.Serializable]
    public class CharacterAnimation2D
    {
        [Header("4-Directions")]
        public AnimationClip2D down;
        public AnimationClip2D up;
        public AnimationClip2D left;
        public AnimationClip2D right;
        [Header("8-Directions")]
        public AnimationClip2D downLeft;
        public AnimationClip2D downRight;
        public AnimationClip2D upLeft;
        public AnimationClip2D upRight;

        public AnimationClip2D GetClipByDirection(DirectionType directionType)
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
                case DirectionType.DownLeft:
                    // Return down if it is support 4-direction
                    if (downLeft == null)
                        return down;
                    return downLeft;
                case DirectionType.DownRight:
                    // Return down if it is support 4-direction
                    if (downRight == null)
                        return down;
                    return downRight;
                case DirectionType.UpLeft:
                    // Return up if it is support 4-direction
                    if (upLeft == null)
                        return up;
                    return upLeft;
                case DirectionType.UpRight:
                    // Return up if it is support 4-direction
                    if (upRight == null)
                        return up;
                    return upRight;
            }
            // Default direction is down
            return down;
        }
    }

    [System.Serializable]
    public class ActionAnimation2D : CharacterAnimation2D
    {
        [Tooltip("This will be in use with attack/skill animations, This is rate of total animation duration at when it should hit enemy or apply skill")]
        [Range(0f, 1f)]
        public float triggerDurationRate;
        [Tooltip("This will be in use with attack/skill animations, This is duration after played animation clip to add delay before next animation")]
        public float extraDuration;
        [Tooltip("This will be in use with attack/skill animations, These audio clips playing randomly while play this animation (not loop)")]
        public AudioClip[] audioClips;

        public AudioClip GetRandomAudioClip()
        {
            AudioClip clip = null;
            if (audioClips != null && audioClips.Length > 0)
                clip = audioClips[Random.Range(0, audioClips.Length)];
            return clip;
        }
    }

    [System.Serializable]
    public struct WeaponAnimations2D
    {
        public WeaponType weaponType;
        public ActionAnimation2D rightHandAttackAnimation;
        public ActionAnimation2D leftHandAttackAnimation;
        public ActionAnimation2D rightHandReloadAnimation;
        public ActionAnimation2D leftHandReloadAnimation;
    }

    [System.Serializable]
    public struct SkillCastAnimations2D
    {
        public Skill skill;
        public ActionAnimation2D animation;
    }
}
