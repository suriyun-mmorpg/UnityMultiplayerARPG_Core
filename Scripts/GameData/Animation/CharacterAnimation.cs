using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ActionAnimation
    {
        public AnimationClip clip;
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

        public float GetClipLength()
        {
            if (clip == null)
                return 0f;
            return clip.length;
        }

        public float GetExtraDuration()
        {
            return extraDuration;
        }

        public float GetTriggerDuration()
        {
            return GetClipLength() * triggerDurationRate;
        }

        public float GetTotalDuration()
        {
            return GetClipLength() + extraDuration;
        }
    }

    [System.Serializable]
    public struct WeaponAnimations
    {
        public WeaponType weaponType;
        public AnimationClip idleClip;
        public AnimationClip moveClip;
        public AnimationClip moveBackwardClip;
        public AnimationClip moveLeftClip;
        public AnimationClip moveRightClip;
        public AnimationClip moveForwardLeftClip;
        public AnimationClip moveForwardRightClip;
        public AnimationClip moveBackwardLeftClip;
        public AnimationClip moveBackwardRightClip;
        public AnimationClip jumpClip;
        public AnimationClip fallClip;
        public AnimationClip hurtClip;
        public AnimationClip deadClip;
        public ActionAnimation[] rightHandAttackAnimations;
        public ActionAnimation[] leftHandAttackAnimations;
    }

    [System.Serializable]
    public struct SkillAnimations
    {
        public Skill skill;
        public AnimationClip castClip;
        public ActionAnimation activateAnimation;
    }

    // TODO: This is deprecated, it will be removed later
    [System.Serializable]
    public struct SkillCastAnimations
    {
        public Skill skill;
        public ActionAnimation[] castAnimations;
    }
}
