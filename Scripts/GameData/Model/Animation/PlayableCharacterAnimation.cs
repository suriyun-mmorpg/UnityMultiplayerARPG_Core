using UnityEngine;

namespace MultiplayerARPG.GameData.Model.Playables
{
    [System.Serializable]
    public struct AnimationClipAndAvatarMask
    {
        public AnimationClip clip;
        public AvatarMask mask;
    }

    [System.Serializable]
    public struct MoveClips
    {
        public AnimationClip forwardClip;
        public AnimationClip backwardClip;
        public AnimationClip leftClip;
        public AnimationClip rightClip;
        public AnimationClip forwardLeftClip;
        public AnimationClip forwardRightClip;
        public AnimationClip backwardLeftClip;
        public AnimationClip backwardRightClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float animSpeedRate;
    }

    [System.Serializable]
    public struct ActionAnimation
    {
        public AnimationClipAndAvatarMask clip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float animSpeedRate;
        [Tooltip("This will be in use with attack/skill animations, This is rate of total animation duration at when it should hit enemy or apply skill")]
        [Range(0f, 1f)]
        public float triggerDurationRate;
        [Tooltip("If this length more than 1, will use each entry as trigger duration rate")]
        [Range(0f, 1f)]
        public float[] multiHitTriggerDurationRates;
        [Tooltip("How animation duration defined")]
        public AnimationDurationType durationType;
        [StringShowConditional(nameof(durationType), nameof(AnimationDurationType.ByFixedDuration))]
        [Tooltip("This will be used when `durationType` equals to `ByFixValue` to define animation duration")]
        public float fixedDuration;
        [Tooltip("This will be in use with attack/skill animations, This is duration after action animation clip played to add some delay before next animation")]
        public float extendDuration;
        [Tooltip("This will be in use with attack/skill animations, These audio clips will be played randomly while play this animation (not loop). PS. You actually can use animation event instead :P")]
        public AudioClip[] audioClips;

        public AudioClip GetRandomAudioClip()
        {
            AudioClip clip = null;
            if (audioClips != null && audioClips.Length > 0)
                clip = audioClips[Random.Range(0, audioClips.Length)];
            return clip;
        }

        public float GetAnimSpeedRate()
        {
            return animSpeedRate > 0 ? animSpeedRate : 1f;
        }

        public float GetClipLength()
        {
            switch (durationType)
            {
                case AnimationDurationType.ByClipLength:
                    if (clip.clip == null)
                        return 0f;
                    return clip.clip.length;
                case AnimationDurationType.ByFixedDuration:
                    return fixedDuration;
            }
            return 0f;
        }

        public float GetExtendDuration()
        {
            return extendDuration;
        }

        public float[] GetTriggerDurations()
        {
            float clipLength = GetClipLength();
            if (multiHitTriggerDurationRates != null &&
                multiHitTriggerDurationRates.Length > 0)
            {
                float previousRate = 0f;
                float[] durations = new float[multiHitTriggerDurationRates.Length];
                for (int i = 0; i < durations.Length; ++i)
                {
                    durations[i] = clipLength * (multiHitTriggerDurationRates[i] - previousRate);
                    previousRate = multiHitTriggerDurationRates[i];
                }
                return durations;
            }
            return new float[] { clipLength * triggerDurationRate };
        }

        public float GetTotalDuration()
        {
            return GetClipLength() + extendDuration;
        }
    }

    [System.Serializable]
    public struct WeaponAnimations : IWeaponAnims
    {
        public WeaponType weaponType;

        [Header("Movements while standing")]
        public AnimationClip idleClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float idleAnimSpeedRate;
        public MoveClips moveClips;
        public MoveClips sprintClips;
        public MoveClips walkClips;

        [Header("Movements while crouching")]
        public AnimationClip crouchIdleClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float crouchIdleAnimSpeedRate;
        public MoveClips crouchClips;

        [Header("Movements while crawling")]
        public AnimationClip crawlIdleClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float crawlIdleAnimSpeedRate;
        public MoveClips crawlClips;

        [Header("Movements while swimming")]
        public AnimationClip swimIdleClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float swimIdleAnimSpeedRate;
        public MoveClips swimClips;

        [Header("Jump")]
        public AnimationClip jumpClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float jumpAnimSpeedRate;

        [Header("Fall")]
        public AnimationClip fallClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float fallAnimSpeedRate;

        [Header("Landed")]
        public AnimationClip landedClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float landedAnimSpeedRate;

        [Header("Hurt")]
        public AnimationClip hurtClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float hurtAnimSpeedRate;

        [Header("Dead")]
        public AnimationClip deadClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float deadAnimSpeedRate;

        [Header("Pickup")]
        public AnimationClipAndAvatarMask pickupClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float pickupAnimSpeedRate;

        [Header("Attack animations")]
        public AnimationClip rightHandChargeClip;
        public AnimationClip leftHandChargeClip;
        [ArrayElementTitle("clip")]
        public ActionAnimation[] rightHandAttackAnimations;
        [ArrayElementTitle("clip")]
        public ActionAnimation[] leftHandAttackAnimations;

        [Header("Reload(Gun) animations")]
        public ActionAnimation rightHandReloadAnimation;
        public ActionAnimation leftHandReloadAnimation;

        public WeaponType Data { get { return weaponType; } }
    }

    [System.Serializable]
    public struct SkillAnimations : ISkillAnims
    {
        public BaseSkill skill;
        public AnimationClipAndAvatarMask castClip;
        public SkillActivateAnimationType activateAnimationType;
        [StringShowConditional(nameof(activateAnimationType), nameof(SkillActivateAnimationType.UseActivateAnimation))]
        public ActionAnimation activateAnimation;
        public BaseSkill Data { get { return skill; } }
    }

    [System.Serializable]
    public struct DefaultAnimations
    {
        [Header("Movements while standing")]
        public AnimationClip idleClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float idleAnimSpeedRate;
        public MoveClips moveClips;
        public MoveClips sprintClips;
        public MoveClips walkClips;

        [Header("Movements while crouching")]
        public AnimationClip crouchIdleClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float crouchIdleAnimSpeedRate;
        public MoveClips crouchClips;

        [Header("Movements while crawling")]
        public AnimationClip crawlIdleClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float crawlIdleAnimSpeedRate;
        public MoveClips crawlClips;

        [Header("Movements while swimming")]
        public AnimationClip swimIdleClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float swimIdleAnimSpeedRate;
        public MoveClips swimClips;

        [Header("Jump")]
        public AnimationClip jumpClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float jumpAnimSpeedRate;

        [Header("Fall")]
        public AnimationClip fallClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float fallAnimSpeedRate;

        [Header("Landed")]
        public AnimationClip landedClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float landedAnimSpeedRate;

        [Header("Hurt")]
        public AnimationClip hurtClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float hurtAnimSpeedRate;

        [Header("Dead")]
        public AnimationClip deadClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float deadAnimSpeedRate;

        [Header("Pickup")]
        public AnimationClipAndAvatarMask pickupClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float pickupAnimSpeedRate;

        [Header("Attack animations")]
        public AnimationClipAndAvatarMask rightHandChargeClip;
        public AnimationClipAndAvatarMask leftHandChargeClip;
        [ArrayElementTitle("clip")]
        public ActionAnimation[] rightHandAttackAnimations;
        [ArrayElementTitle("clip")]
        public ActionAnimation[] leftHandAttackAnimations;

        [Header("Reload(Gun) animations")]
        public ActionAnimation rightHandReloadAnimation;
        public ActionAnimation leftHandReloadAnimation;

        [Header("Skill animations")]
        public AnimationClipAndAvatarMask skillCastClip;
        public ActionAnimation skillActivateAnimation;
    }
}
