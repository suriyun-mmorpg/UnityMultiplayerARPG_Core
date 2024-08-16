using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG.GameData.Model.Playables
{
    [System.Serializable]
    public struct AnimState
    {
        public AnimationClip clip;
        [Tooltip("If this <= 0, it will not be used to calculate with animation speed multiplier")]
        public float animSpeedRate;
        [Tooltip("If this <= 0, it will use default transition duration setting from model component")]
        public float transitionDuration;
        public bool isAdditive;
        public bool applyFootIk;
        public bool applyPlayableIk;

        public float GetSpeed(float rate)
        {
            return (animSpeedRate > 0 ? animSpeedRate : 1) * rate;
        }

        public float GetClipLength(float rate)
        {
            return clip.length / GetSpeed(rate);
        }
    }

    [System.Serializable]
    public struct ActionState
    {
        public AnimationClip clip;
        [Tooltip("If this is `null`, it will use default avatar mask setting from model component")]
        public AvatarMask avatarMask;
        [Tooltip("If this <= 0, it will not be used to calculate with animation speed multiplier")]
        public float animSpeedRate;
        [Tooltip("If this <= 0, it will use default transition duration setting from model component")]
        public float transitionDuration;
        public bool isAdditive;
        public bool applyFootIk;
        public bool applyPlayableIk;
        [Tooltip("Turn this on to skip movement validation while playing this animation")]
        public bool skipMovementValidation;
        [Tooltip("Turn this on to use root motion while playing this animation")]
        public bool shouldUseRootMotion;

        public float GetSpeed(float rate)
        {
            return (animSpeedRate > 0 ? animSpeedRate : 1) * rate;
        }

        public float GetClipLength(float rate)
        {
            return clip.length / GetSpeed(rate);
        }
    }

    [System.Serializable]
    public struct HolsterAnimation
    {
        [Header("Sheath")]
        [FormerlySerializedAs("holsterState")]
        public ActionState sheathState;
        [Range(0f, 1f)]
        [FormerlySerializedAs("holsteredDurationRate")]
        public float sheathedDurationRate;

        [Header("Unsheath")]
        [FormerlySerializedAs("drawState")]
        public ActionState unsheathState;
        [Range(0f, 1f)]
        [FormerlySerializedAs("drawnDurationRate")]
        public float unsheathedDurationRate;
    }

    [System.Serializable]
    public struct MoveStates
    {
        public AnimState forwardState;
        public AnimState backwardState;
        public AnimState leftState;
        public AnimState rightState;
        public AnimState upState;
        public AnimState downState;
        public AnimState forwardLeftState;
        public AnimState forwardRightState;
        public AnimState backwardLeftState;
        public AnimState backwardRightState;
    }

    [System.Serializable]
    public struct WieldMoveStates
    {
        public ActionState forwardState;
        public ActionState backwardState;
        public ActionState leftState;
        public ActionState rightState;
        public ActionState upState;
        public ActionState downState;
        public ActionState forwardLeftState;
        public ActionState forwardRightState;
        public ActionState backwardLeftState;
        public ActionState backwardRightState;
    }

    [System.Serializable]
    public struct ActionAnimation
    {
        public ActionState state;
        [Tooltip("This will be in used with attacking/skill animations, This is rate of total animation duration at when it should hit enemy or apply skill")]
        [Range(0f, 1f)]
        public float[] triggerDurationRates;
        [Tooltip("How animation duration defined")]
        public AnimationDurationType durationType;
        [StringShowConditional(nameof(durationType), nameof(AnimationDurationType.ByFixedDuration))]
        [Tooltip("This will be used when `durationType` equals to `ByFixValue` to define animation duration")]
        public float fixedDuration;
        [Tooltip("This will be in use with attacking/skill animations, This is duration after action animation clip played to add some delay before next animation")]
        public float extendDuration;
        [Tooltip("This will be in use with attacking/skill animations, These audio clips will be played randomly while play this animation (not loop). PS. You actually can use animation event instead :P")]
        public AudioClip[] audioClips;

        public AudioClip GetRandomAudioClip()
        {
            if (audioClips == null || audioClips.Length == 0)
                return null;
            return audioClips[Random.Range(0, audioClips.Length)];
        }

        public float GetClipLength()
        {
            switch (durationType)
            {
                case AnimationDurationType.ByClipLength:
                    if (state.clip == null)
                        return 0f;
                    return state.clip.length;
                case AnimationDurationType.ByFixedDuration:
                    return fixedDuration;
            }
            return 0f;
        }

        public float GetExtendDuration()
        {
            return extendDuration;
        }

        public float GetAnimSpeedRate()
        {
            return state.animSpeedRate > 0 ? state.animSpeedRate : 1f;
        }

        public float[] GetTriggerDurations()
        {
            float clipLength = GetClipLength();
            if (triggerDurationRates == null || triggerDurationRates.Length == 0)
                return new float[] { clipLength * 0.5f };
            float previousRate = 0f;
            float[] durations = new float[triggerDurationRates.Length];
            for (int i = 0; i < durations.Length; ++i)
            {
                durations[i] = clipLength * (triggerDurationRates[i] - previousRate);
                previousRate = triggerDurationRates[i];
            }
            return durations;
        }

        public float GetTotalDuration()
        {
            return GetClipLength() + extendDuration;
        }
    }

    [System.Serializable]
    public struct EnterExitStates
    {
        public ActionState enterState;
        public ActionState exitState;
    }

    [System.Serializable]
    public struct WeaponAnimations : IWeaponAnims
    {
        public WeaponType weaponType;

        [Header("Movements while standing")]
        public AnimState idleState;
        public MoveStates moveStates;
        public MoveStates sprintStates;
        public MoveStates walkStates;

        [Header("Movements while crouching")]
        public AnimState crouchIdleState;
        public MoveStates crouchMoveStates;

        [Header("Movements while crawling")]
        public AnimState crawlIdleState;
        public MoveStates crawlMoveStates;

        [Header("Movements while swimming")]
        public AnimState swimIdleState;
        public MoveStates swimMoveStates;

        [Header("Airborne")]
        public AnimState jumpState;
        public AnimState fallState;
        public AnimState landedState;

        [Header("Hurt")]
        public ActionState hurtState;

        [Header("Dead")]
        public AnimState deadState;

        [Header("Dash")]
        public AnimState dashStartState;
        public AnimState dashLoopState;
        public AnimState dashEndState;

        [Header("Pickup")]
        public ActionState pickupState;

        [Header("Attack animations")]
        public ActionState rightHandChargeState;
        public ActionState leftHandChargeState;
        [ArrayElementTitle("clip")]
        public ActionAnimation[] rightHandAttackAnimations;
        [ArrayElementTitle("clip")]
        public ActionAnimation[] leftHandAttackAnimations;

        [Header("Reload(Gun) animations")]
        public ActionAnimation rightHandReloadAnimation;
        public ActionAnimation leftHandReloadAnimation;

        [Header("Sheath/Unsheath Animations")]
        [FormerlySerializedAs("rightHandHolsterAnimation")]
        public HolsterAnimation rightHandWeaponSheathingAnimation;
        [FormerlySerializedAs("leftHandHolsterAnimation")]
        public HolsterAnimation leftHandWeaponSheathingAnimation;

        public WeaponType Data { get { return weaponType; } }
    }

    [System.Serializable]
    public struct WieldWeaponAnimations : IWeaponAnims
    {
        public WeaponType weaponType;

        [Header("Movements while standing")]
        public ActionState idleState;
        public WieldMoveStates moveStates;
        public WieldMoveStates sprintStates;
        public WieldMoveStates walkStates;

        [Header("Movements while crouching")]
        public ActionState crouchIdleState;
        public WieldMoveStates crouchMoveStates;

        [Header("Movements while crawling")]
        public ActionState crawlIdleState;
        public WieldMoveStates crawlMoveStates;

        [Header("Movements while swimming")]
        public ActionState swimIdleState;
        public WieldMoveStates swimMoveStates;

        [Header("Airborne")]
        public ActionState jumpState;
        public ActionState fallState;
        public ActionState landedState;

        [Header("Dead")]
        public ActionState deadState;

        [Header("Dash")]
        public ActionState dashStartState;
        public ActionState dashLoopState;
        public ActionState dashEndState;

        public WeaponType Data { get { return weaponType; } }
    }

    [System.Serializable]
    public struct SkillAnimations : ISkillAnims
    {
        public BaseSkill skill;
        public ActionState castState;
        public SkillActivateAnimationType activateAnimationType;
        [StringShowConditional(nameof(activateAnimationType), nameof(SkillActivateAnimationType.UseActivateAnimation))]
        public ActionAnimation activateAnimation;
        public BaseSkill Data { get { return skill; } }
    }

    [System.Serializable]
    public struct DefaultAnimations
    {
        [Header("Movements while standing")]
        public AnimState idleState;
        public MoveStates moveStates;
        public MoveStates sprintStates;
        public MoveStates walkStates;

        [Header("Movements while crouching")]
        public AnimState crouchIdleState;
        public MoveStates crouchMoveStates;

        [Header("Movements while crawling")]
        public AnimState crawlIdleState;
        public MoveStates crawlMoveStates;

        [Header("Movements while swimming")]
        public AnimState swimIdleState;
        public MoveStates swimMoveStates;

        [Header("Airborne")]
        public AnimState jumpState;
        public AnimState fallState;
        public AnimState landedState;

        [Header("Vehicle Animation")]
        public EnterExitStates vehicleEnterExitStates;

        [Header("Ladder Animation")]
        public EnterExitStates climbBottomEnterExitStates;
        public EnterExitStates climbTopEnterExitStates;
        public AnimState climbIdleState;
        public MoveStates climbMoveStates;

        [Header("Hurt")]
        public ActionState hurtState;

        [Header("Dead")]
        public AnimState deadState;

        [Header("Dash")]
        public AnimState dashStartState;
        public AnimState dashLoopState;
        public AnimState dashEndState;

        [Header("Pickup")]
        public ActionState pickupState;

        [Header("Attack animations")]
        public ActionState rightHandChargeState;
        public ActionState leftHandChargeState;
        [ArrayElementTitle("clip")]
        public ActionAnimation[] rightHandAttackAnimations;
        [ArrayElementTitle("clip")]
        public ActionAnimation[] leftHandAttackAnimations;

        [Header("Reload(Gun) animations")]
        public ActionAnimation rightHandReloadAnimation;
        public ActionAnimation leftHandReloadAnimation;

        [Header("Skill animations")]
        public ActionState skillCastState;
        public ActionAnimation skillActivateAnimation;

        [Header("Sheath/Unsheath Animations")]
        [FormerlySerializedAs("rightHandHolsterAnimation")]
        public HolsterAnimation rightHandWeaponSheathingAnimation;
        [FormerlySerializedAs("leftHandHolsterAnimation")]
        public HolsterAnimation leftHandWeaponSheathingAnimation;
        public HolsterAnimation leftHandShieldSheathingAnimation;
    }
}
