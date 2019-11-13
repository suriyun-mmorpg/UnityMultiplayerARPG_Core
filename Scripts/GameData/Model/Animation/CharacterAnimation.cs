using UnityEngine;

namespace MultiplayerARPG
{
    public enum AnimationDurationType
    {
        ByClipLength,
        ByFixValue,
    }

    public enum SkillActivateAnimationType
    {
        UseActivateAnimation,
        UseAttackAnimation,
    }

    [System.Serializable]
    public struct ActionAnimation
    {
        public AnimationClip clip;
        [Tooltip("This will be in use with attack/skill animations, This is rate of total animation duration at when it should hit enemy or apply skill")]
        [Range(0f, 1f)]
        public float triggerDurationRate;
        [Tooltip("If this length more than 1, will use each entry for trigger duration rate")]
        [Range(0f, 1f)]
        public float[] multiHitTriggerDurationRates;
        [Tooltip("How animation duration defined")]
        public AnimationDurationType durationType;
        [StringShowConditional("durationType", "ByFixValue")]
        [Tooltip("This will be used when `durationType` equals to `ByFixValue` to define animation duration")]
        public float fixDurationValue;
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
            switch (durationType)
            {
                case AnimationDurationType.ByClipLength:
                    if (clip == null)
                        return 0f;
                    return clip.length;
                case AnimationDurationType.ByFixValue:
                    return fixDurationValue;
            }
            return 0f;
        }

        public float GetExtraDuration()
        {
            return extraDuration;
        }
        
        public float[] GetTriggerDurations()
        {
            if (multiHitTriggerDurationRates != null &&
                multiHitTriggerDurationRates.Length > 0)
            {
                float[] durations = new float[multiHitTriggerDurationRates.Length];
                for (int i = 0; i < durations.Length; ++i)
                {
                    durations[i] = GetClipLength() * multiHitTriggerDurationRates[i];
                }
                return durations;
            }
            return new float[] { GetClipLength() * triggerDurationRate };
        }

        public float GetTotalDuration()
        {
            return GetClipLength() + extraDuration;
        }
    }

    [System.Serializable]
    public struct WeaponAnimations : IWeaponAnims
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
        [ArrayElementTitle("clip", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public ActionAnimation[] rightHandAttackAnimations;
        [ArrayElementTitle("clip", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public ActionAnimation[] leftHandAttackAnimations;
        public ActionAnimation rightHandReloadAnimation;
        public ActionAnimation leftHandReloadAnimation;
        public WeaponType Data { get { return weaponType; } }
    }

    [System.Serializable]
    public struct SkillAnimations : ISkillAnims
    {
        public BaseSkill skill;
        public AnimationClip castClip;
        public SkillActivateAnimationType activateAnimationType;
        [StringShowConditional("activateAnimationType", "UseActivateAnimation")]
        public ActionAnimation activateAnimation;
        public BaseSkill Data { get { return skill; } }
    }

    [System.Serializable]
    public struct DefaultAnimations
    {
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
        [ArrayElementTitle("clip", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public ActionAnimation[] rightHandAttackAnimations;
        [ArrayElementTitle("clip", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public ActionAnimation[] leftHandAttackAnimations;
        public ActionAnimation rightHandReloadAnimation;
        public ActionAnimation leftHandReloadAnimation;
        public AnimationClip skillCastClip;
        public ActionAnimation skillActivateAnimation;
    }
}
