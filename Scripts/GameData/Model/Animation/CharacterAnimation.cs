using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ActionAnimation
    {
        public AnimationClip clip;
        public bool playClipAllLayers;
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
        [FormerlySerializedAs("fixDurationValue")]
        public float fixedDuration;
        [Tooltip("This will be in use with attack/skill animations, This is duration after action animation clip played to add some delay before next animation")]
        [FormerlySerializedAs("extraDuration")]
        public float extendDuration;
        [Tooltip("This will be in use with attack/skill animations, These audio clips will be played randomly while play this animation (not loop). PS. You actually can use animation event instead :P")]
        public AudioClip[] audioClips;

        public AudioClip GetRandomAudioClip()
        {
            if (audioClips == null || audioClips.Length == 0)
                return null;
            return audioClips[Random.Range(0, audioClips.Length)];
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
                    if (clip == null)
                        return 0f;
                    return clip.length;
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

        [Header("Movements while standing (move)")]
        public bool showMoveClipSettings;
        [BoolShowConditional(nameof(showMoveClipSettings), true)]
        public AnimationClip moveClip;
        [BoolShowConditional(nameof(showMoveClipSettings), true)]
        public AnimationClip moveBackwardClip;
        [BoolShowConditional(nameof(showMoveClipSettings), true)]
        public AnimationClip moveLeftClip;
        [BoolShowConditional(nameof(showMoveClipSettings), true)]
        public AnimationClip moveRightClip;
        [BoolShowConditional(nameof(showMoveClipSettings), true)]
        public AnimationClip moveForwardLeftClip;
        [BoolShowConditional(nameof(showMoveClipSettings), true)]
        public AnimationClip moveForwardRightClip;
        [BoolShowConditional(nameof(showMoveClipSettings), true)]
        public AnimationClip moveBackwardLeftClip;
        [BoolShowConditional(nameof(showMoveClipSettings), true)]
        public AnimationClip moveBackwardRightClip;
        [BoolShowConditional(nameof(showMoveClipSettings), true)]
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float moveAnimSpeedRate;

        [Header("Movements while standing (sprint)")]
        public bool showSprintClipSettings;
        [BoolShowConditional(nameof(showSprintClipSettings), true)]
        public AnimationClip sprintClip;
        [BoolShowConditional(nameof(showSprintClipSettings), true)]
        public AnimationClip sprintBackwardClip;
        [BoolShowConditional(nameof(showSprintClipSettings), true)]
        public AnimationClip sprintLeftClip;
        [BoolShowConditional(nameof(showSprintClipSettings), true)]
        public AnimationClip sprintRightClip;
        [BoolShowConditional(nameof(showSprintClipSettings), true)]
        public AnimationClip sprintForwardLeftClip;
        [BoolShowConditional(nameof(showSprintClipSettings), true)]
        public AnimationClip sprintForwardRightClip;
        [BoolShowConditional(nameof(showSprintClipSettings), true)]
        public AnimationClip sprintBackwardLeftClip;
        [BoolShowConditional(nameof(showSprintClipSettings), true)]
        public AnimationClip sprintBackwardRightClip;
        [BoolShowConditional(nameof(showSprintClipSettings), true)]
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float sprintAnimSpeedRate;

        [Header("Movements while standing (walk)")]
        public bool showWalkClipSettings;
        [BoolShowConditional(nameof(showWalkClipSettings), true)]
        public AnimationClip walkClip;
        [BoolShowConditional(nameof(showWalkClipSettings), true)]
        public AnimationClip walkBackwardClip;
        [BoolShowConditional(nameof(showWalkClipSettings), true)]
        public AnimationClip walkLeftClip;
        [BoolShowConditional(nameof(showWalkClipSettings), true)]
        public AnimationClip walkRightClip;
        [BoolShowConditional(nameof(showWalkClipSettings), true)]
        public AnimationClip walkForwardLeftClip;
        [BoolShowConditional(nameof(showWalkClipSettings), true)]
        public AnimationClip walkForwardRightClip;
        [BoolShowConditional(nameof(showWalkClipSettings), true)]
        public AnimationClip walkBackwardLeftClip;
        [BoolShowConditional(nameof(showWalkClipSettings), true)]
        public AnimationClip walkBackwardRightClip;
        [BoolShowConditional(nameof(showWalkClipSettings), true)]
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float walkAnimSpeedRate;

        [Header("Movements while crouching")]
        public AnimationClip crouchIdleClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float crouchIdleAnimSpeedRate;
        public bool showCrouchMoveClipSettings;
        [BoolShowConditional(nameof(showCrouchMoveClipSettings), true)]
        public AnimationClip crouchMoveClip;
        [BoolShowConditional(nameof(showCrouchMoveClipSettings), true)]
        public AnimationClip crouchMoveBackwardClip;
        [BoolShowConditional(nameof(showCrouchMoveClipSettings), true)]
        public AnimationClip crouchMoveLeftClip;
        [BoolShowConditional(nameof(showCrouchMoveClipSettings), true)]
        public AnimationClip crouchMoveRightClip;
        [BoolShowConditional(nameof(showCrouchMoveClipSettings), true)]
        public AnimationClip crouchMoveForwardLeftClip;
        [BoolShowConditional(nameof(showCrouchMoveClipSettings), true)]
        public AnimationClip crouchMoveForwardRightClip;
        [BoolShowConditional(nameof(showCrouchMoveClipSettings), true)]
        public AnimationClip crouchMoveBackwardLeftClip;
        [BoolShowConditional(nameof(showCrouchMoveClipSettings), true)]
        public AnimationClip crouchMoveBackwardRightClip;
        [BoolShowConditional(nameof(showCrouchMoveClipSettings), true)]
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float crouchMoveAnimSpeedRate;

        [Header("Movements while crawling")]
        public AnimationClip crawlIdleClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float crawlIdleAnimSpeedRate;
        public bool showCrawlMoveClipSettings;
        [BoolShowConditional(nameof(showCrawlMoveClipSettings), true)]
        public AnimationClip crawlMoveClip;
        [BoolShowConditional(nameof(showCrawlMoveClipSettings), true)]
        public AnimationClip crawlMoveBackwardClip;
        [BoolShowConditional(nameof(showCrawlMoveClipSettings), true)]
        public AnimationClip crawlMoveLeftClip;
        [BoolShowConditional(nameof(showCrawlMoveClipSettings), true)]
        public AnimationClip crawlMoveRightClip;
        [BoolShowConditional(nameof(showCrawlMoveClipSettings), true)]
        public AnimationClip crawlMoveForwardLeftClip;
        [BoolShowConditional(nameof(showCrawlMoveClipSettings), true)]
        public AnimationClip crawlMoveForwardRightClip;
        [BoolShowConditional(nameof(showCrawlMoveClipSettings), true)]
        public AnimationClip crawlMoveBackwardLeftClip;
        [BoolShowConditional(nameof(showCrawlMoveClipSettings), true)]
        public AnimationClip crawlMoveBackwardRightClip;
        [BoolShowConditional(nameof(showCrawlMoveClipSettings), true)]
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float crawlMoveAnimSpeedRate;

        [Header("Movements while swimming")]
        public AnimationClip swimIdleClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float swimIdleAnimSpeedRate;
        public bool showSwimMoveClipSettings;
        [BoolShowConditional(nameof(showSwimMoveClipSettings), true)]
        public AnimationClip swimMoveClip;
        [BoolShowConditional(nameof(showSwimMoveClipSettings), true)]
        public AnimationClip swimMoveBackwardClip;
        [BoolShowConditional(nameof(showSwimMoveClipSettings), true)]
        public AnimationClip swimMoveLeftClip;
        [BoolShowConditional(nameof(showSwimMoveClipSettings), true)]
        public AnimationClip swimMoveRightClip;
        [BoolShowConditional(nameof(showSwimMoveClipSettings), true)]
        public AnimationClip swimMoveForwardLeftClip;
        [BoolShowConditional(nameof(showSwimMoveClipSettings), true)]
        public AnimationClip swimMoveForwardRightClip;
        [BoolShowConditional(nameof(showSwimMoveClipSettings), true)]
        public AnimationClip swimMoveBackwardLeftClip;
        [BoolShowConditional(nameof(showSwimMoveClipSettings), true)]
        public AnimationClip swimMoveBackwardRightClip;
        [BoolShowConditional(nameof(showSwimMoveClipSettings), true)]
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float swimMoveAnimSpeedRate;

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
        public AnimationClip pickupClip;
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
        public AnimationClip castClip;
        public bool playCastClipAllLayers;
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

        [Header("Movements while standing (move)")]
        public bool showMoveClipSettings;
        [BoolShowConditional(nameof(showMoveClipSettings), true)]
        public AnimationClip moveClip;
        [BoolShowConditional(nameof(showMoveClipSettings), true)]
        public AnimationClip moveBackwardClip;
        [BoolShowConditional(nameof(showMoveClipSettings), true)]
        public AnimationClip moveLeftClip;
        [BoolShowConditional(nameof(showMoveClipSettings), true)]
        public AnimationClip moveRightClip;
        [BoolShowConditional(nameof(showMoveClipSettings), true)]
        public AnimationClip moveForwardLeftClip;
        [BoolShowConditional(nameof(showMoveClipSettings), true)]
        public AnimationClip moveForwardRightClip;
        [BoolShowConditional(nameof(showMoveClipSettings), true)]
        public AnimationClip moveBackwardLeftClip;
        [BoolShowConditional(nameof(showMoveClipSettings), true)]
        public AnimationClip moveBackwardRightClip;
        [BoolShowConditional(nameof(showMoveClipSettings), true)]
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float moveAnimSpeedRate;

        [Header("Movements while standing (sprint)")]
        public bool showSprintClipSettings;
        [BoolShowConditional(nameof(showSprintClipSettings), true)]
        public AnimationClip sprintClip;
        [BoolShowConditional(nameof(showSprintClipSettings), true)]
        public AnimationClip sprintBackwardClip;
        [BoolShowConditional(nameof(showSprintClipSettings), true)]
        public AnimationClip sprintLeftClip;
        [BoolShowConditional(nameof(showSprintClipSettings), true)]
        public AnimationClip sprintRightClip;
        [BoolShowConditional(nameof(showSprintClipSettings), true)]
        public AnimationClip sprintForwardLeftClip;
        [BoolShowConditional(nameof(showSprintClipSettings), true)]
        public AnimationClip sprintForwardRightClip;
        [BoolShowConditional(nameof(showSprintClipSettings), true)]
        public AnimationClip sprintBackwardLeftClip;
        [BoolShowConditional(nameof(showSprintClipSettings), true)]
        public AnimationClip sprintBackwardRightClip;
        [BoolShowConditional(nameof(showSprintClipSettings), true)]
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float sprintAnimSpeedRate;

        [Header("Movements while standing (walk)")]
        public bool showWalkClipSettings;
        [BoolShowConditional(nameof(showWalkClipSettings), true)]
        public AnimationClip walkClip;
        [BoolShowConditional(nameof(showWalkClipSettings), true)]
        public AnimationClip walkBackwardClip;
        [BoolShowConditional(nameof(showWalkClipSettings), true)]
        public AnimationClip walkLeftClip;
        [BoolShowConditional(nameof(showWalkClipSettings), true)]
        public AnimationClip walkRightClip;
        [BoolShowConditional(nameof(showWalkClipSettings), true)]
        public AnimationClip walkForwardLeftClip;
        [BoolShowConditional(nameof(showWalkClipSettings), true)]
        public AnimationClip walkForwardRightClip;
        [BoolShowConditional(nameof(showWalkClipSettings), true)]
        public AnimationClip walkBackwardLeftClip;
        [BoolShowConditional(nameof(showWalkClipSettings), true)]
        public AnimationClip walkBackwardRightClip;
        [BoolShowConditional(nameof(showWalkClipSettings), true)]
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float walkAnimSpeedRate;

        [Header("Movements while crouching")]
        public AnimationClip crouchIdleClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float crouchIdleAnimSpeedRate;
        public bool showCrouchMoveClipSettings;
        [BoolShowConditional(nameof(showCrouchMoveClipSettings), true)]
        public AnimationClip crouchMoveClip;
        [BoolShowConditional(nameof(showCrouchMoveClipSettings), true)]
        public AnimationClip crouchMoveBackwardClip;
        [BoolShowConditional(nameof(showCrouchMoveClipSettings), true)]
        public AnimationClip crouchMoveLeftClip;
        [BoolShowConditional(nameof(showCrouchMoveClipSettings), true)]
        public AnimationClip crouchMoveRightClip;
        [BoolShowConditional(nameof(showCrouchMoveClipSettings), true)]
        public AnimationClip crouchMoveForwardLeftClip;
        [BoolShowConditional(nameof(showCrouchMoveClipSettings), true)]
        public AnimationClip crouchMoveForwardRightClip;
        [BoolShowConditional(nameof(showCrouchMoveClipSettings), true)]
        public AnimationClip crouchMoveBackwardLeftClip;
        [BoolShowConditional(nameof(showCrouchMoveClipSettings), true)]
        public AnimationClip crouchMoveBackwardRightClip;
        [BoolShowConditional(nameof(showCrouchMoveClipSettings), true)]
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float crouchMoveAnimSpeedRate;

        [Header("Movements while crawling")]
        public AnimationClip crawlIdleClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float crawlIdleAnimSpeedRate;
        public bool showCrawlMoveClipSettings;
        [BoolShowConditional(nameof(showCrawlMoveClipSettings), true)]
        public AnimationClip crawlMoveClip;
        [BoolShowConditional(nameof(showCrawlMoveClipSettings), true)]
        public AnimationClip crawlMoveBackwardClip;
        [BoolShowConditional(nameof(showCrawlMoveClipSettings), true)]
        public AnimationClip crawlMoveLeftClip;
        [BoolShowConditional(nameof(showCrawlMoveClipSettings), true)]
        public AnimationClip crawlMoveRightClip;
        [BoolShowConditional(nameof(showCrawlMoveClipSettings), true)]
        public AnimationClip crawlMoveForwardLeftClip;
        [BoolShowConditional(nameof(showCrawlMoveClipSettings), true)]
        public AnimationClip crawlMoveForwardRightClip;
        [BoolShowConditional(nameof(showCrawlMoveClipSettings), true)]
        public AnimationClip crawlMoveBackwardLeftClip;
        [BoolShowConditional(nameof(showCrawlMoveClipSettings), true)]
        public AnimationClip crawlMoveBackwardRightClip;
        [BoolShowConditional(nameof(showCrawlMoveClipSettings), true)]
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float crawlMoveAnimSpeedRate;

        [Header("Movements while swimming")]
        public AnimationClip swimIdleClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float swimIdleAnimSpeedRate;
        public bool showSwimMoveClipSettings;
        [BoolShowConditional(nameof(showSwimMoveClipSettings), true)]
        public AnimationClip swimMoveClip;
        [BoolShowConditional(nameof(showSwimMoveClipSettings), true)]
        public AnimationClip swimMoveBackwardClip;
        [BoolShowConditional(nameof(showSwimMoveClipSettings), true)]
        public AnimationClip swimMoveLeftClip;
        [BoolShowConditional(nameof(showSwimMoveClipSettings), true)]
        public AnimationClip swimMoveRightClip;
        [BoolShowConditional(nameof(showSwimMoveClipSettings), true)]
        public AnimationClip swimMoveForwardLeftClip;
        [BoolShowConditional(nameof(showSwimMoveClipSettings), true)]
        public AnimationClip swimMoveForwardRightClip;
        [BoolShowConditional(nameof(showSwimMoveClipSettings), true)]
        public AnimationClip swimMoveBackwardLeftClip;
        [BoolShowConditional(nameof(showSwimMoveClipSettings), true)]
        public AnimationClip swimMoveBackwardRightClip;
        [BoolShowConditional(nameof(showSwimMoveClipSettings), true)]
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float swimMoveAnimSpeedRate;

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
        public AnimationClip pickupClip;
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

        [Header("Skill animations")]
        public AnimationClip skillCastClip;
        public bool playSkillCastClipAllLayers;
        public ActionAnimation skillActivateAnimation;
    }
}
