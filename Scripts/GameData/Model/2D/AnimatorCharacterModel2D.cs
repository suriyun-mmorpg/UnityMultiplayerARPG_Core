using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class AnimatorCharacterModel2D :
        BaseCharacterModelWithCacheAnims<AnimatorWeaponAnimations2D, AnimatorSkillAnimations2D>,
        ICharacterModel2D
    {
        public enum AnimatorControllerType
        {
            FourDirections,
            EightDirections,
            Custom,
        }
        // Clip name variables
        // Idle
        public const string CLIP_IDLE_DOWN = "__IdleDown";
        public const string CLIP_IDLE_UP = "__IdleUp";
        public const string CLIP_IDLE_LEFT = "__IdleLeft";
        public const string CLIP_IDLE_RIGHT = "__IdleRight";
        public const string CLIP_IDLE_DOWN_LEFT = "__IdleDownLeft";
        public const string CLIP_IDLE_DOWN_RIGHT = "__IdleDownRight";
        public const string CLIP_IDLE_UP_LEFT = "__IdleUpLeft";
        public const string CLIP_IDLE_UP_RIGHT = "__IdleUpRight";
        // Move
        public const string CLIP_MOVE_DOWN = "__MoveDown";
        public const string CLIP_MOVE_UP = "__MoveUp";
        public const string CLIP_MOVE_LEFT = "__MoveLeft";
        public const string CLIP_MOVE_RIGHT = "__MoveRight";
        public const string CLIP_MOVE_DOWN_LEFT = "__MoveDownLeft";
        public const string CLIP_MOVE_DOWN_RIGHT = "__MoveDownRight";
        public const string CLIP_MOVE_UP_LEFT = "__MoveUpLeft";
        public const string CLIP_MOVE_UP_RIGHT = "__MoveUpRight";
        // Dead
        public const string CLIP_DEAD_DOWN = "__DeadDown";
        public const string CLIP_DEAD_UP = "__DeadUp";
        public const string CLIP_DEAD_LEFT = "__DeadLeft";
        public const string CLIP_DEAD_RIGHT = "__DeadRight";
        public const string CLIP_DEAD_DOWN_LEFT = "__DeadDownLeft";
        public const string CLIP_DEAD_DOWN_RIGHT = "__DeadDownRight";
        public const string CLIP_DEAD_UP_LEFT = "__DeadUpLeft";
        public const string CLIP_DEAD_UP_RIGHT = "__DeadUpRight";
        // Action
        public const string CLIP_ACTION_DOWN = "__ActionDown";
        public const string CLIP_ACTION_UP = "__ActionUp";
        public const string CLIP_ACTION_LEFT = "__ActionLeft";
        public const string CLIP_ACTION_RIGHT = "__ActionRight";
        public const string CLIP_ACTION_DOWN_LEFT = "__ActionDownLeft";
        public const string CLIP_ACTION_DOWN_RIGHT = "__ActionDownRight";
        public const string CLIP_ACTION_UP_LEFT = "__ActionUpLeft";
        public const string CLIP_ACTION_UP_RIGHT = "__ActionUpRight";
        // Cast Skill
        public const string CLIP_CAST_SKILL_DOWN = "__CastSkillDown";
        public const string CLIP_CAST_SKILL_UP = "__CastSkillUp";
        public const string CLIP_CAST_SKILL_LEFT = "__CastSkillLeft";
        public const string CLIP_CAST_SKILL_RIGHT = "__CastSkillRight";
        public const string CLIP_CAST_SKILL_DOWN_LEFT = "__CastSkillDownLeft";
        public const string CLIP_CAST_SKILL_DOWN_RIGHT = "__CastSkillDownRight";
        public const string CLIP_CAST_SKILL_UP_LEFT = "__CastSkillUpLeft";
        public const string CLIP_CAST_SKILL_UP_RIGHT = "__CastSkillUpRight";
        // Animator variables
        public static readonly int ANIM_DIRECTION_X = Animator.StringToHash("DirectionX");
        public static readonly int ANIM_DIRECTION_Y = Animator.StringToHash("DirectionY");
        public static readonly int ANIM_IS_DEAD = Animator.StringToHash("IsDead");
        public static readonly int ANIM_MOVE_SPEED = Animator.StringToHash("MoveSpeed");
        public static readonly int ANIM_DO_ACTION = Animator.StringToHash("DoAction");
        public static readonly int ANIM_IS_CASTING_SKILL = Animator.StringToHash("IsCastingSkill");
        public static readonly int ANIM_MOVE_CLIP_MULTIPLIER = Animator.StringToHash("MoveSpeedMultiplier");
        public static readonly int ANIM_ACTION_CLIP_MULTIPLIER = Animator.StringToHash("ActionSpeedMultiplier");

        [Header("2D Animations")]
        public AnimatorCharacterAnimation2D idleAnimation2D;
        public AnimatorCharacterAnimation2D moveAnimation2D;
        public AnimatorCharacterAnimation2D deadAnimation2D;
        public AnimatorActionAnimation2D defaultAttackAnimation2D;
        [FormerlySerializedAs("defaultSkillCastClip2D")]
        public AnimatorCharacterAnimation2D defaultSkillCastAnimation2D;
        public AnimatorActionAnimation2D defaultSkillActivateAnimation2D;
        public AnimatorActionAnimation2D defaultReloadAnimation2D;
        [ArrayElementTitle("weaponType", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public AnimatorWeaponAnimations2D[] weaponAnimations2D;
        [ArrayElementTitle("skill", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public AnimatorSkillAnimations2D[] skillAnimations2D;
        public float magnitudeToPlayMoveClip = 0.1f;

        [Header("Settings")]
        public AnimatorControllerType controllerType;

        [Header("Relates Components")]
        [Tooltip("It will find `Animator` component on automatically if this is NULL")]
        public Animator animator;
        [Tooltip("You can set this when animator controller type is `Custom`")]
        public RuntimeAnimatorController animatorController;

        public DirectionType2D CurrentDirectionType { get; set; }

        private AnimatorOverrideController cacheAnimatorController;
        public AnimatorOverrideController CacheAnimatorController
        {
            get
            {
                SetupComponent();
                return cacheAnimatorController;
            }
        }

        // Private state validater
        private bool isSetupComponent;

        protected override void Awake()
        {
            SetupComponent();
            base.Awake();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
#if UNITY_EDITOR
            bool hasChanges = false;
            RuntimeAnimatorController changingAnimatorController;
            switch (controllerType)
            {
                case AnimatorControllerType.FourDirections:
                    changingAnimatorController = Resources.Load("__Animator/__2DFourDirectionsCharacter") as RuntimeAnimatorController;
                    if (changingAnimatorController != null &&
                        changingAnimatorController != animatorController)
                    {
                        animatorController = changingAnimatorController;
                        hasChanges = true;
                    }
                    break;
                case AnimatorControllerType.EightDirections:
                    changingAnimatorController = Resources.Load("__Animator/__2DEightDirectionsCharacter") as RuntimeAnimatorController;
                    if (changingAnimatorController != null &&
                        changingAnimatorController != animatorController)
                    {
                        animatorController = changingAnimatorController;
                        hasChanges = true;
                    }
                    break;
            }
            if (hasChanges)
                EditorUtility.SetDirty(this);
#endif
        }

        private void SetupComponent()
        {
            if (isSetupComponent)
                return;
            // Set setup state to avoid it trying to setup again later
            isSetupComponent = true;
            if (cacheAnimatorController == null)
                cacheAnimatorController = new AnimatorOverrideController(animatorController);
            // Use override controller as animator
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            if (animator != null && animator.runtimeAnimatorController != cacheAnimatorController)
                animator.runtimeAnimatorController = cacheAnimatorController;
            // Set default clips
            // Idle
            CacheAnimatorController[CLIP_IDLE_DOWN] = idleAnimation2D.down;
            CacheAnimatorController[CLIP_IDLE_UP] = idleAnimation2D.up;
            CacheAnimatorController[CLIP_IDLE_LEFT] = idleAnimation2D.left;
            CacheAnimatorController[CLIP_IDLE_RIGHT] = idleAnimation2D.right;
            CacheAnimatorController[CLIP_IDLE_DOWN_LEFT] = idleAnimation2D.downLeft;
            CacheAnimatorController[CLIP_IDLE_DOWN_RIGHT] = idleAnimation2D.downRight;
            CacheAnimatorController[CLIP_IDLE_UP_LEFT] = idleAnimation2D.upLeft;
            CacheAnimatorController[CLIP_IDLE_UP_RIGHT] = idleAnimation2D.upRight;
            // Move
            CacheAnimatorController[CLIP_MOVE_DOWN] = moveAnimation2D.down;
            CacheAnimatorController[CLIP_MOVE_UP] = moveAnimation2D.up;
            CacheAnimatorController[CLIP_MOVE_LEFT] = moveAnimation2D.left;
            CacheAnimatorController[CLIP_MOVE_RIGHT] = moveAnimation2D.right;
            CacheAnimatorController[CLIP_MOVE_DOWN_LEFT] = moveAnimation2D.downLeft;
            CacheAnimatorController[CLIP_MOVE_DOWN_RIGHT] = moveAnimation2D.downRight;
            CacheAnimatorController[CLIP_MOVE_UP_LEFT] = moveAnimation2D.upLeft;
            CacheAnimatorController[CLIP_MOVE_UP_RIGHT] = moveAnimation2D.upRight;
            // Dead
            CacheAnimatorController[CLIP_DEAD_DOWN] = deadAnimation2D.down;
            CacheAnimatorController[CLIP_DEAD_UP] = deadAnimation2D.up;
            CacheAnimatorController[CLIP_DEAD_LEFT] = deadAnimation2D.left;
            CacheAnimatorController[CLIP_DEAD_RIGHT] = deadAnimation2D.right;
            CacheAnimatorController[CLIP_DEAD_DOWN_LEFT] = deadAnimation2D.downLeft;
            CacheAnimatorController[CLIP_DEAD_DOWN_RIGHT] = deadAnimation2D.downRight;
            CacheAnimatorController[CLIP_DEAD_UP_LEFT] = deadAnimation2D.upLeft;
            CacheAnimatorController[CLIP_DEAD_UP_RIGHT] = deadAnimation2D.upRight;
        }

        public override void PlayMoveAnimation()
        {
            if (!animator.gameObject.activeInHierarchy)
                return;

            if (animator.runtimeAnimatorController != CacheAnimatorController)
                animator.runtimeAnimatorController = CacheAnimatorController;

            if (isDead)
            {
                // Clear action animations when dead
                if (animator.GetBool(ANIM_DO_ACTION))
                    animator.SetBool(ANIM_DO_ACTION, false);
                if (animator.GetBool(ANIM_IS_CASTING_SKILL))
                    animator.SetBool(ANIM_IS_CASTING_SKILL, false);
            }

            float moveSpeed = 0f;
            if (movementState.HasFlag(MovementState.Forward) ||
                movementState.HasFlag(MovementState.Backward) ||
                movementState.HasFlag(MovementState.Right) ||
                movementState.HasFlag(MovementState.Left))
                moveSpeed = 1;
            // Set animator parameters
            animator.SetFloat(ANIM_MOVE_SPEED, isDead ? 0 : moveSpeed);
            animator.SetFloat(ANIM_MOVE_CLIP_MULTIPLIER, moveAnimationSpeedMultiplier);
            animator.SetBool(ANIM_IS_DEAD, isDead);
        }

        private AnimatorActionAnimation2D GetActionAnimation(AnimActionType animActionType, int dataId)
        {
            AnimatorActionAnimation2D animation2D = null;
            AnimatorWeaponAnimations2D weaponAnimations2D;
            AnimatorSkillAnimations2D skillAnimations2D;
            switch (animActionType)
            {
                case AnimActionType.AttackRightHand:
                    if (!GetAnims().CacheWeaponAnimations.TryGetValue(dataId, out weaponAnimations2D))
                        animation2D = defaultAttackAnimation2D;
                    else
                        animation2D = weaponAnimations2D.rightHandAttackAnimation;
                    break;
                case AnimActionType.AttackLeftHand:
                    if (!GetAnims().CacheWeaponAnimations.TryGetValue(dataId, out weaponAnimations2D))
                        animation2D = defaultAttackAnimation2D;
                    else
                        animation2D = weaponAnimations2D.leftHandAttackAnimation;
                    break;
                case AnimActionType.SkillRightHand:
                case AnimActionType.SkillLeftHand:
                    if (!GetAnims().CacheSkillAnimations.TryGetValue(dataId, out skillAnimations2D))
                        animation2D = defaultSkillActivateAnimation2D;
                    else
                        animation2D = skillAnimations2D.activateAnimation;
                    break;
                case AnimActionType.ReloadRightHand:
                    if (!GetAnims().CacheWeaponAnimations.TryGetValue(dataId, out weaponAnimations2D))
                        animation2D = defaultReloadAnimation2D;
                    else
                        animation2D = weaponAnimations2D.rightHandReloadAnimation;
                    break;
                case AnimActionType.ReloadLeftHand:
                    if (!GetAnims().CacheWeaponAnimations.TryGetValue(dataId, out weaponAnimations2D))
                        animation2D = defaultReloadAnimation2D;
                    else
                        animation2D = weaponAnimations2D.leftHandReloadAnimation;
                    break;
            }
            return animation2D;
        }

        public override Coroutine PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1f)
        {
            return StartCoroutine(PlayActionAnimation_Animator(animActionType, dataId, index, playSpeedMultiplier));
        }

        private IEnumerator PlayActionAnimation_Animator(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier)
        {
            // If animator is not null, play the action animation
            AnimatorActionAnimation2D animation2D = GetActionAnimation(animActionType, dataId);
            // Action
            CacheAnimatorController[CLIP_ACTION_DOWN] = animation2D.down;
            CacheAnimatorController[CLIP_ACTION_UP] = animation2D.up;
            CacheAnimatorController[CLIP_ACTION_LEFT] = animation2D.left;
            CacheAnimatorController[CLIP_ACTION_RIGHT] = animation2D.right;
            CacheAnimatorController[CLIP_ACTION_DOWN_LEFT] = animation2D.downLeft;
            CacheAnimatorController[CLIP_ACTION_DOWN_RIGHT] = animation2D.downRight;
            CacheAnimatorController[CLIP_ACTION_UP_LEFT] = animation2D.upLeft;
            CacheAnimatorController[CLIP_ACTION_UP_RIGHT] = animation2D.upRight;
            yield return 0;
            AnimationClip clip = animation2D.GetClipByDirection(CurrentDirectionType);
            AudioClip audioClip = animation2D.GetRandomAudioClip();
            if (audioClip != null)
                AudioSource.PlayClipAtPoint(audioClip, CacheTransform.position, AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level);
            animator.SetFloat(ANIM_ACTION_CLIP_MULTIPLIER, playSpeedMultiplier);
            animator.SetBool(ANIM_DO_ACTION, true);
            animator.Play(0, 0, 0f);
            // Waits by current transition + clip duration before end animation
            yield return new WaitForSecondsRealtime(clip.length / playSpeedMultiplier);
            animator.SetBool(ANIM_DO_ACTION, false);
            // Waits by current transition + extra duration before end playing animation state
            yield return new WaitForSecondsRealtime(animation2D.extraDuration / playSpeedMultiplier);
        }

        public override Coroutine PlaySkillCastClip(int dataId, float duration)
        {
            return StartCoroutine(PlaySkillCastClip_Animator(dataId, duration));
        }

        private IEnumerator PlaySkillCastClip_Animator(int dataId, float duration)
        {
            AnimatorCharacterAnimation2D animation2D;
            AnimatorSkillAnimations2D skillAnimations2D;
            if (!GetAnims().CacheSkillAnimations.TryGetValue(dataId, out skillAnimations2D))
                animation2D = defaultSkillActivateAnimation2D;
            else
                animation2D = skillAnimations2D.castAnimation;

            if (animation2D != null)
            {
                // Cast Skill
                CacheAnimatorController[CLIP_CAST_SKILL_DOWN] = animation2D.down;
                CacheAnimatorController[CLIP_CAST_SKILL_UP] = animation2D.up;
                CacheAnimatorController[CLIP_CAST_SKILL_LEFT] = animation2D.left;
                CacheAnimatorController[CLIP_CAST_SKILL_RIGHT] = animation2D.right;
                CacheAnimatorController[CLIP_CAST_SKILL_DOWN_LEFT] = animation2D.downLeft;
                CacheAnimatorController[CLIP_CAST_SKILL_DOWN_RIGHT] = animation2D.downRight;
                CacheAnimatorController[CLIP_CAST_SKILL_UP_LEFT] = animation2D.upLeft;
                CacheAnimatorController[CLIP_CAST_SKILL_UP_RIGHT] = animation2D.upRight;
                yield return 0;
                animator.SetBool(ANIM_IS_CASTING_SKILL, true);
                animator.Play(0, 0, 0f);
                yield return new WaitForSecondsRealtime(duration);
                animator.SetBool(ANIM_IS_CASTING_SKILL, false);
            }
        }

        public override void StopActionAnimation()
        {
            animator.SetBool(ANIM_DO_ACTION, false);
        }

        public override void StopSkillCastAnimation()
        {
            animator.SetBool(ANIM_IS_CASTING_SKILL, false);
        }

        public override bool GetRandomRightHandAttackAnimation(int dataId, out int animationIndex, out float[] triggerDurations, out float totalDuration)
        {
            animationIndex = 0;
            return GetRightHandAttackAnimation(dataId, animationIndex, out triggerDurations, out totalDuration);
        }

        public override bool GetRandomLeftHandAttackAnimation(int dataId, out int animationIndex, out float[] triggerDurations, out float totalDuration)
        {
            animationIndex = 0;
            return GetLeftHandAttackAnimation(dataId, animationIndex, out triggerDurations, out totalDuration);
        }

        public override bool GetRightHandAttackAnimation(int dataId, int animationIndex, out float[] triggerDurations, out float totalDuration)
        {
            AnimatorActionAnimation2D animation2D = defaultAttackAnimation2D;
            AnimatorWeaponAnimations2D weaponAnims;
            if (GetAnims().CacheWeaponAnimations.TryGetValue(dataId, out weaponAnims))
                animation2D = weaponAnims.rightHandAttackAnimation;
            triggerDurations = new float[] { 0f };
            totalDuration = 0f;
            if (animation2D == null) return false;
            AnimationClip clip = animation2D.GetClipByDirection(CurrentDirectionType);
            if (clip == null) return false;
            triggerDurations = animation2D.GetTriggerDurations(clip.length);
            totalDuration = animation2D.GetTotalDuration(clip.length);
            return true;
        }

        public override bool GetLeftHandAttackAnimation(int dataId, int animationIndex, out float[] triggerDurations, out float totalDuration)
        {
            AnimatorActionAnimation2D animation2D = defaultAttackAnimation2D;
            AnimatorWeaponAnimations2D weaponAnims;
            if (GetAnims().CacheWeaponAnimations.TryGetValue(dataId, out weaponAnims))
                animation2D = weaponAnims.leftHandAttackAnimation;
            triggerDurations = new float[] { 0f };
            totalDuration = 0f;
            if (animation2D == null) return false;
            AnimationClip clip = animation2D.GetClipByDirection(CurrentDirectionType);
            if (clip == null) return false;
            triggerDurations = animation2D.GetTriggerDurations(clip.length);
            totalDuration = animation2D.GetTotalDuration(clip.length);
            return true;
        }

        public override bool GetSkillActivateAnimation(int dataId, out float[] triggerDurations, out float totalDuration)
        {
            AnimatorActionAnimation2D animation2D = defaultSkillActivateAnimation2D;
            AnimatorSkillAnimations2D skillAnims;
            if (GetAnims().CacheSkillAnimations.TryGetValue(dataId, out skillAnims))
                animation2D = skillAnims.activateAnimation;
            triggerDurations = new float[] { 0f };
            totalDuration = 0f;
            if (animation2D == null) return false;
            AnimationClip clip = animation2D.GetClipByDirection(CurrentDirectionType);
            if (clip == null) return false;
            triggerDurations = animation2D.GetTriggerDurations(clip.length);
            totalDuration = animation2D.GetTotalDuration(clip.length);
            return true;
        }

        public override bool GetRightHandReloadAnimation(int dataId, out float[] triggerDurations, out float totalDuration)
        {
            AnimatorActionAnimation2D animation2D = defaultReloadAnimation2D;
            AnimatorWeaponAnimations2D weaponAnims;
            if (GetAnims().CacheWeaponAnimations.TryGetValue(dataId, out weaponAnims))
                animation2D = weaponAnims.rightHandReloadAnimation;
            triggerDurations = new float[] { 0f };
            totalDuration = 0f;
            if (animation2D == null) return false;
            AnimationClip clip = animation2D.GetClipByDirection(CurrentDirectionType);
            if (clip == null) return false;
            triggerDurations = animation2D.GetTriggerDurations(clip.length);
            totalDuration = animation2D.GetTotalDuration(clip.length);
            return true;
        }

        public override bool GetLeftHandReloadAnimation(int dataId, out float[] triggerDurations, out float totalDuration)
        {
            AnimatorActionAnimation2D animation2D = defaultReloadAnimation2D;
            AnimatorWeaponAnimations2D weaponAnims;
            if (GetAnims().CacheWeaponAnimations.TryGetValue(dataId, out weaponAnims))
                animation2D = weaponAnims.leftHandReloadAnimation;
            triggerDurations = new float[] { 0f };
            totalDuration = 0f;
            if (animation2D == null) return false;
            AnimationClip clip = animation2D.GetClipByDirection(CurrentDirectionType);
            if (clip == null) return false;
            triggerDurations = animation2D.GetTriggerDurations(clip.length);
            totalDuration = animation2D.GetTotalDuration(clip.length);
            return true;
        }

        public override SkillActivateAnimationType UseSkillActivateAnimationType(int dataId)
        {
            if (!GetAnims().CacheSkillAnimations.ContainsKey(dataId))
                return SkillActivateAnimationType.UseActivateAnimation;
            return GetAnims().CacheSkillAnimations[dataId].activateAnimationType;
        }

        protected override AnimatorWeaponAnimations2D[] GetWeaponAnims()
        {
            return weaponAnimations2D;
        }

        protected override AnimatorSkillAnimations2D[] GetSkillAnims()
        {
            return skillAnimations2D;
        }
    }
}
