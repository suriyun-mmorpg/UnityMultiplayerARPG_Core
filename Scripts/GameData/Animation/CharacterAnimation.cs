using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class CharacterAnimation
    {
        public AnimationClip clip;
    }

    [System.Serializable]
    public class ActionAnimation : CharacterAnimation
    {
        [SerializeField]
        [Tooltip("This will be in use with attack/skill animations, This is rate of total animation duration at when it should hit enemy or apply skill")]
        [Range(0f, 1f)]
        private float triggerDurationRate;
        [SerializeField]
        [Tooltip("This will be in use with attack/skill animations, This is duration after played animation clip to add delay before next animation")]
        private float extraDuration;
        [SerializeField]
        [Tooltip("This will be in use with attack/skill animations, These audio clips playing randomly while play this animation (not loop)")]
        private AudioClip[] audioClips;
        [Header("DEPRECATED")]
        [Tooltip("This will be removed on next version, please move data to your Character Model")]
        [SerializeField]
        [System.Obsolete("This will be removed on next version, please move data to your Character Model")]
        private ActionAnimationOverrideData[] overrideData;

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
    public class ActionAnimationOverrideData
    {
        public CharacterModel target;
        [Tooltip("Must set it to override default animation data")]
        public AnimationClip clip;
        [Tooltip("Set it more than zero to override default trigger duration rate")]
        [Range(0f, 1f)]
        public float triggerDurationRate;
        public float extraDuration;
        [Tooltip("Set it length more than zero to override default audio clips")]
        public AudioClip[] audioClips;
    }

    [System.Serializable]
    public struct WeaponAnimations
    {
        public WeaponType weaponType;
        public ActionAnimation[] rightHandAttackAnimations;
        public ActionAnimation[] leftHandAttackAnimations;
    }

    [System.Serializable]
    public struct SkillCastAnimations
    {
        public Skill skill;
        public ActionAnimation[] castAnimations;
    }
}
