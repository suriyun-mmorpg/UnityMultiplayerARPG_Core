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
        public AnimationClip2D down;
        public AnimationClip2D up;
        public AnimationClip2D left;
        public AnimationClip2D right;

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
            }
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
        public ActionAnimation2D animation;
    }

    [System.Serializable]
    public struct SkillCastAnimations2D
    {
        public Skill skill;
        public ActionAnimation2D animation;
    }
}
