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
        public bool playClipAllLayers;
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

        [Header("Movements while standing")]
        public AnimationClip idleClip;
        public AnimationClip moveClip;
        public AnimationClip moveBackwardClip;
        public AnimationClip moveLeftClip;
        public AnimationClip moveRightClip;
        public AnimationClip moveForwardLeftClip;
        public AnimationClip moveForwardRightClip;
        public AnimationClip moveBackwardLeftClip;
        public AnimationClip moveBackwardRightClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float moveAnimSpeedRate;

        [Header("Movements while standing (sprint)")]
        public AnimationClip sprintClip;
        public AnimationClip sprintBackwardClip;
        public AnimationClip sprintLeftClip;
        public AnimationClip sprintRightClip;
        public AnimationClip sprintForwardLeftClip;
        public AnimationClip sprintForwardRightClip;
        public AnimationClip sprintBackwardLeftClip;
        public AnimationClip sprintBackwardRightClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float sprintAnimSpeedRate;

        [Header("Movements while standing (walk)")]
        public AnimationClip walkClip;
        public AnimationClip walkBackwardClip;
        public AnimationClip walkLeftClip;
        public AnimationClip walkRightClip;
        public AnimationClip walkForwardLeftClip;
        public AnimationClip walkForwardRightClip;
        public AnimationClip walkBackwardLeftClip;
        public AnimationClip walkBackwardRightClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float walkAnimSpeedRate;

        [Header("Movements while crouching")]
        public AnimationClip crouchIdleClip;
        public AnimationClip crouchMoveClip;
        public AnimationClip crouchMoveBackwardClip;
        public AnimationClip crouchMoveLeftClip;
        public AnimationClip crouchMoveRightClip;
        public AnimationClip crouchMoveForwardLeftClip;
        public AnimationClip crouchMoveForwardRightClip;
        public AnimationClip crouchMoveBackwardLeftClip;
        public AnimationClip crouchMoveBackwardRightClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float crouchMoveAnimSpeedRate;

        [Header("Movements while crawling")]
        public AnimationClip crawlIdleClip;
        public AnimationClip crawlMoveClip;
        public AnimationClip crawlMoveBackwardClip;
        public AnimationClip crawlMoveLeftClip;
        public AnimationClip crawlMoveRightClip;
        public AnimationClip crawlMoveForwardLeftClip;
        public AnimationClip crawlMoveForwardRightClip;
        public AnimationClip crawlMoveBackwardLeftClip;
        public AnimationClip crawlMoveBackwardRightClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float crawlMoveAnimSpeedRate;

        [Header("Movements while swimming")]
        public AnimationClip swimIdleClip;
        public AnimationClip swimMoveClip;
        public AnimationClip swimMoveBackwardClip;
        public AnimationClip swimMoveLeftClip;
        public AnimationClip swimMoveRightClip;
        public AnimationClip swimMoveForwardLeftClip;
        public AnimationClip swimMoveForwardRightClip;
        public AnimationClip swimMoveBackwardLeftClip;
        public AnimationClip swimMoveBackwardRightClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float swimMoveAnimSpeedRate;

        [Header("Other movements")]
        public AnimationClip jumpClip;
        public AnimationClip fallClip;
        public AnimationClip hurtClip;
        public AnimationClip deadClip;

        [Header("Attack movements")]
        [ArrayElementTitle("clip", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public ActionAnimation[] rightHandAttackAnimations;
        [ArrayElementTitle("clip", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public ActionAnimation[] leftHandAttackAnimations;

        [Header("Reload(Gun) movements")]
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
        [StringShowConditional("activateAnimationType", "UseActivateAnimation")]
        public ActionAnimation activateAnimation;
        public BaseSkill Data { get { return skill; } }
    }

    [System.Serializable]
    public struct DefaultAnimations
    {
        [Header("Movements while standing")]
        public AnimationClip idleClip;
        public AnimationClip moveClip;
        public AnimationClip moveBackwardClip;
        public AnimationClip moveLeftClip;
        public AnimationClip moveRightClip;
        public AnimationClip moveForwardLeftClip;
        public AnimationClip moveForwardRightClip;
        public AnimationClip moveBackwardLeftClip;
        public AnimationClip moveBackwardRightClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float moveAnimSpeedRate;

        [Header("Movements while standing (sprint)")]
        public AnimationClip sprintClip;
        public AnimationClip sprintBackwardClip;
        public AnimationClip sprintLeftClip;
        public AnimationClip sprintRightClip;
        public AnimationClip sprintForwardLeftClip;
        public AnimationClip sprintForwardRightClip;
        public AnimationClip sprintBackwardLeftClip;
        public AnimationClip sprintBackwardRightClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float sprintAnimSpeedRate;

        [Header("Movements while standing (walk)")]
        public AnimationClip walkClip;
        public AnimationClip walkBackwardClip;
        public AnimationClip walkLeftClip;
        public AnimationClip walkRightClip;
        public AnimationClip walkForwardLeftClip;
        public AnimationClip walkForwardRightClip;
        public AnimationClip walkBackwardLeftClip;
        public AnimationClip walkBackwardRightClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float walkAnimSpeedRate;

        [Header("Movements while crouching")]
        public AnimationClip crouchIdleClip;
        public AnimationClip crouchMoveClip;
        public AnimationClip crouchMoveBackwardClip;
        public AnimationClip crouchMoveLeftClip;
        public AnimationClip crouchMoveRightClip;
        public AnimationClip crouchMoveForwardLeftClip;
        public AnimationClip crouchMoveForwardRightClip;
        public AnimationClip crouchMoveBackwardLeftClip;
        public AnimationClip crouchMoveBackwardRightClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float crouchMoveAnimSpeedRate;

        [Header("Movements while crawling")]
        public AnimationClip crawlIdleClip;
        public AnimationClip crawlMoveClip;
        public AnimationClip crawlMoveBackwardClip;
        public AnimationClip crawlMoveLeftClip;
        public AnimationClip crawlMoveRightClip;
        public AnimationClip crawlMoveForwardLeftClip;
        public AnimationClip crawlMoveForwardRightClip;
        public AnimationClip crawlMoveBackwardLeftClip;
        public AnimationClip crawlMoveBackwardRightClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float crawlMoveAnimSpeedRate;

        [Header("Movements while swimming")]
        public AnimationClip swimIdleClip;
        public AnimationClip swimMoveClip;
        public AnimationClip swimMoveBackwardClip;
        public AnimationClip swimMoveLeftClip;
        public AnimationClip swimMoveRightClip;
        public AnimationClip swimMoveForwardLeftClip;
        public AnimationClip swimMoveForwardRightClip;
        public AnimationClip swimMoveBackwardLeftClip;
        public AnimationClip swimMoveBackwardRightClip;
        [Tooltip("If this <= 0, it will not be used to calculates with animation speed multiplier")]
        public float swimMoveAnimSpeedRate;

        [Header("Other movements")]
        public AnimationClip jumpClip;
        public AnimationClip fallClip;
        public AnimationClip hurtClip;
        public AnimationClip deadClip;

        [Header("Attack movements")]
        [ArrayElementTitle("clip", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public ActionAnimation[] rightHandAttackAnimations;
        [ArrayElementTitle("clip", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public ActionAnimation[] leftHandAttackAnimations;

        [Header("Reload(Gun) movements")]
        public ActionAnimation rightHandReloadAnimation;
        public ActionAnimation leftHandReloadAnimation;

        [Header("Skill movements")]
        public AnimationClip skillCastClip;
        public bool playSkillCastClipAllLayers;
        public ActionAnimation skillActivateAnimation;
    }
}
