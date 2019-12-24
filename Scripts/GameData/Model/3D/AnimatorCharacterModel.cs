using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [ExecuteInEditMode]
    public class AnimatorCharacterModel : BaseRemakeCharacterModel
    {
        public enum AnimatorControllerType
        {
            Simple,
            Advance,
            Custom,
        }
        // Animator variables
        public static readonly int ANIM_IS_DEAD = Animator.StringToHash("IsDead");
        public static readonly int ANIM_IS_GROUNDED = Animator.StringToHash("IsGrounded");
        public static readonly int ANIM_IS_UNDER_WATER = Animator.StringToHash("IsUnderWater");
        public static readonly int ANIM_MOVE_SPEED = Animator.StringToHash("MoveSpeed");
        public static readonly int ANIM_SIDE_MOVE_SPEED = Animator.StringToHash("SideMoveSpeed");
        public static readonly int ANIM_DO_ACTION = Animator.StringToHash("DoAction");
        public static readonly int ANIM_IS_CASTING_SKILL = Animator.StringToHash("IsCastingSkill");
        public static readonly int ANIM_HURT = Animator.StringToHash("Hurt");
        public static readonly int ANIM_JUMP = Animator.StringToHash("Jump");
        public static readonly int ANIM_MOVE_CLIP_MULTIPLIER = Animator.StringToHash("MoveSpeedMultiplier");
        public static readonly int ANIM_ACTION_CLIP_MULTIPLIER = Animator.StringToHash("ActionSpeedMultiplier");
        public static readonly int ANIM_MOVE_TYPE = Animator.StringToHash("MoveType");

        [Header("Settings")]
        public AnimatorControllerType controllerType;

        [Header("Relates Components")]
        [Tooltip("It will find `Animator` component on automatically if this is NULL")]
        public Animator animator;
        [Tooltip("You can set this when animator controller type is `Custom`")]
        public RuntimeAnimatorController animatorController;
        [Tooltip("Which layer in Animator controller that you use it to play action animations, You can set this when animator controller type is `Custom`")]
        public int actionStateLayer;
        [Tooltip("Which layer in Animator controller that you use it to play cast skill animations, You can set this when animator controller type is `Custom`")]
        public int castSkillStateLayer;
        [Tooltip("The damping time for the `MoveSpeed` and `SideMoveSpeed` parameters. The higher the value the slower the parameter value changes.")]
        public float movementDampingTme = 0.1f;

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
                case AnimatorControllerType.Simple:
                    changingAnimatorController = Resources.Load("__Animator/__SimpleCharacter") as RuntimeAnimatorController;
                    if (changingAnimatorController != null &&
                        changingAnimatorController != animatorController)
                    {
                        animatorController = changingAnimatorController;
                        hasChanges = true;
                    }
                    if (actionStateLayer != 0)
                    {
                        actionStateLayer = 0;
                        hasChanges = true;
                    }
                    if (castSkillStateLayer != 0)
                    {
                        castSkillStateLayer = 0;
                        hasChanges = true;
                    }
                    break;
                case AnimatorControllerType.Advance:
                    changingAnimatorController = Resources.Load("__Animator/__AdvanceCharacter") as RuntimeAnimatorController;
                    if (changingAnimatorController != null &&
                        changingAnimatorController != animatorController)
                    {
                        animatorController = changingAnimatorController;
                        hasChanges = true;
                    }
                    if (actionStateLayer != 1)
                    {
                        actionStateLayer = 1;
                        hasChanges = true;
                    }
                    if (castSkillStateLayer != 1)
                    {
                        castSkillStateLayer = 1;
                        hasChanges = true;
                    }
                    break;
            }
            if (hasChanges)
                EditorUtility.SetDirty(this);

            SetDefaultAnimations();
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
            SetDefaultAnimations();
        }

        public override void SetDefaultAnimations()
        {
            // Set default clips
            if (CacheAnimatorController != null)
            {
                CacheAnimatorController[CLIP_IDLE] = defaultAnimations.idleClip;
                CacheAnimatorController[CLIP_MOVE] = defaultAnimations.moveClip;
                CacheAnimatorController[CLIP_MOVE_BACKWARD] = defaultAnimations.moveBackwardClip;
                CacheAnimatorController[CLIP_MOVE_LEFT] = defaultAnimations.moveLeftClip;
                CacheAnimatorController[CLIP_MOVE_RIGHT] = defaultAnimations.moveRightClip;
                CacheAnimatorController[CLIP_MOVE_FORWARD_LEFT] = defaultAnimations.moveForwardLeftClip;
                CacheAnimatorController[CLIP_MOVE_FORWARD_RIGHT] = defaultAnimations.moveForwardRightClip;
                CacheAnimatorController[CLIP_MOVE_BACKWARD_LEFT] = defaultAnimations.moveBackwardLeftClip;
                CacheAnimatorController[CLIP_MOVE_BACKWARD_RIGHT] = defaultAnimations.moveBackwardRightClip;
                CacheAnimatorController[CLIP_SPRINT] = defaultAnimations.sprintClip;
                CacheAnimatorController[CLIP_SPRINT_BACKWARD] = defaultAnimations.sprintBackwardClip;
                CacheAnimatorController[CLIP_SPRINT_LEFT] = defaultAnimations.sprintLeftClip;
                CacheAnimatorController[CLIP_SPRINT_RIGHT] = defaultAnimations.sprintRightClip;
                CacheAnimatorController[CLIP_SPRINT_FORWARD_LEFT] = defaultAnimations.sprintForwardLeftClip;
                CacheAnimatorController[CLIP_SPRINT_FORWARD_RIGHT] = defaultAnimations.sprintForwardRightClip;
                CacheAnimatorController[CLIP_SPRINT_BACKWARD_LEFT] = defaultAnimations.sprintBackwardLeftClip;
                CacheAnimatorController[CLIP_SPRINT_BACKWARD_RIGHT] = defaultAnimations.sprintBackwardRightClip;
                CacheAnimatorController[CLIP_CROUCH_IDLE] = defaultAnimations.crouchIdleClip;
                CacheAnimatorController[CLIP_CROUCH_MOVE] = defaultAnimations.crouchMoveClip;
                CacheAnimatorController[CLIP_CROUCH_MOVE_BACKWARD] = defaultAnimations.crouchMoveBackwardClip;
                CacheAnimatorController[CLIP_CROUCH_MOVE_LEFT] = defaultAnimations.crouchMoveLeftClip;
                CacheAnimatorController[CLIP_CROUCH_MOVE_RIGHT] = defaultAnimations.crouchMoveRightClip;
                CacheAnimatorController[CLIP_CROUCH_MOVE_FORWARD_LEFT] = defaultAnimations.crouchMoveForwardLeftClip;
                CacheAnimatorController[CLIP_CROUCH_MOVE_FORWARD_RIGHT] = defaultAnimations.crouchMoveForwardRightClip;
                CacheAnimatorController[CLIP_CROUCH_MOVE_BACKWARD_LEFT] = defaultAnimations.crouchMoveBackwardLeftClip;
                CacheAnimatorController[CLIP_CROUCH_MOVE_BACKWARD_RIGHT] = defaultAnimations.crouchMoveBackwardRightClip;
                CacheAnimatorController[CLIP_CRAWL_IDLE] = defaultAnimations.crawlIdleClip;
                CacheAnimatorController[CLIP_CRAWL_MOVE] = defaultAnimations.crawlMoveClip;
                CacheAnimatorController[CLIP_CRAWL_MOVE_BACKWARD] = defaultAnimations.crawlMoveBackwardClip;
                CacheAnimatorController[CLIP_CRAWL_MOVE_LEFT] = defaultAnimations.crawlMoveLeftClip;
                CacheAnimatorController[CLIP_CRAWL_MOVE_RIGHT] = defaultAnimations.crawlMoveRightClip;
                CacheAnimatorController[CLIP_CRAWL_MOVE_FORWARD_LEFT] = defaultAnimations.crawlMoveForwardLeftClip;
                CacheAnimatorController[CLIP_CRAWL_MOVE_FORWARD_RIGHT] = defaultAnimations.crawlMoveForwardRightClip;
                CacheAnimatorController[CLIP_CRAWL_MOVE_BACKWARD_LEFT] = defaultAnimations.crawlMoveBackwardLeftClip;
                CacheAnimatorController[CLIP_CRAWL_MOVE_BACKWARD_RIGHT] = defaultAnimations.crawlMoveBackwardRightClip;
                CacheAnimatorController[CLIP_SWIM_IDLE] = defaultAnimations.swimIdleClip;
                CacheAnimatorController[CLIP_SWIM_MOVE] = defaultAnimations.swimMoveClip;
                CacheAnimatorController[CLIP_SWIM_MOVE_BACKWARD] = defaultAnimations.swimMoveBackwardClip;
                CacheAnimatorController[CLIP_SWIM_MOVE_LEFT] = defaultAnimations.swimMoveLeftClip;
                CacheAnimatorController[CLIP_SWIM_MOVE_RIGHT] = defaultAnimations.swimMoveRightClip;
                CacheAnimatorController[CLIP_SWIM_MOVE_FORWARD_LEFT] = defaultAnimations.swimMoveForwardLeftClip;
                CacheAnimatorController[CLIP_SWIM_MOVE_FORWARD_RIGHT] = defaultAnimations.swimMoveForwardRightClip;
                CacheAnimatorController[CLIP_SWIM_MOVE_BACKWARD_LEFT] = defaultAnimations.swimMoveBackwardLeftClip;
                CacheAnimatorController[CLIP_SWIM_MOVE_BACKWARD_RIGHT] = defaultAnimations.swimMoveBackwardRightClip;
                CacheAnimatorController[CLIP_JUMP] = defaultAnimations.jumpClip;
                CacheAnimatorController[CLIP_FALL] = defaultAnimations.fallClip;
                CacheAnimatorController[CLIP_HURT] = defaultAnimations.hurtClip;
                CacheAnimatorController[CLIP_DEAD] = defaultAnimations.deadClip;
            }
            base.SetDefaultAnimations();
        }

        public override void SetEquipWeapons(EquipWeapons equipWeapons)
        {
            base.SetEquipWeapons(equipWeapons);
            SetupComponent();
            SetClipBasedOnWeapon(equipWeapons);
        }

        protected void SetClipBasedOnWeapon(EquipWeapons equipWeapons)
        {
            Item weaponItem = equipWeapons.GetRightHandWeaponItem();
            if (weaponItem == null)
                weaponItem = GameInstance.Singleton.DefaultWeaponItem;
            WeaponAnimations weaponAnimations = default(WeaponAnimations);
            GetAnims().CacheWeaponAnimations.TryGetValue(weaponItem.WeaponType.DataId, out weaponAnimations);
            // Set override animator clips
            CacheAnimatorController[CLIP_IDLE] = weaponAnimations.idleClip != null ? weaponAnimations.idleClip : defaultAnimations.idleClip;
            CacheAnimatorController[CLIP_MOVE] = weaponAnimations.moveClip != null ? weaponAnimations.moveClip : defaultAnimations.moveClip;
            CacheAnimatorController[CLIP_MOVE_BACKWARD] = weaponAnimations.moveBackwardClip != null ? weaponAnimations.moveBackwardClip : defaultAnimations.moveBackwardClip;
            CacheAnimatorController[CLIP_MOVE_LEFT] = weaponAnimations.moveLeftClip != null ? weaponAnimations.moveLeftClip : defaultAnimations.moveLeftClip;
            CacheAnimatorController[CLIP_MOVE_RIGHT] = weaponAnimations.moveRightClip != null ? weaponAnimations.moveRightClip : defaultAnimations.moveRightClip;
            CacheAnimatorController[CLIP_MOVE_FORWARD_LEFT] = weaponAnimations.moveForwardLeftClip != null ? weaponAnimations.moveForwardLeftClip : defaultAnimations.moveForwardLeftClip;
            CacheAnimatorController[CLIP_MOVE_FORWARD_RIGHT] = weaponAnimations.moveForwardRightClip != null ? weaponAnimations.moveForwardRightClip : defaultAnimations.moveForwardRightClip;
            CacheAnimatorController[CLIP_MOVE_BACKWARD_LEFT] = weaponAnimations.moveBackwardLeftClip != null ? weaponAnimations.moveBackwardLeftClip : defaultAnimations.moveBackwardLeftClip;
            CacheAnimatorController[CLIP_MOVE_BACKWARD_RIGHT] = weaponAnimations.moveBackwardRightClip != null ? weaponAnimations.moveBackwardRightClip : defaultAnimations.moveBackwardRightClip;
            CacheAnimatorController[CLIP_SPRINT] = weaponAnimations.sprintClip != null ? weaponAnimations.sprintClip : defaultAnimations.sprintClip;
            CacheAnimatorController[CLIP_SPRINT_BACKWARD] = weaponAnimations.sprintBackwardClip != null ? weaponAnimations.sprintBackwardClip : defaultAnimations.sprintBackwardClip;
            CacheAnimatorController[CLIP_SPRINT_LEFT] = weaponAnimations.sprintLeftClip != null ? weaponAnimations.sprintLeftClip : defaultAnimations.sprintLeftClip;
            CacheAnimatorController[CLIP_SPRINT_RIGHT] = weaponAnimations.sprintRightClip != null ? weaponAnimations.sprintRightClip : defaultAnimations.sprintRightClip;
            CacheAnimatorController[CLIP_SPRINT_FORWARD_LEFT] = weaponAnimations.sprintForwardLeftClip != null ? weaponAnimations.sprintForwardLeftClip : defaultAnimations.sprintForwardLeftClip;
            CacheAnimatorController[CLIP_SPRINT_FORWARD_RIGHT] = weaponAnimations.sprintForwardRightClip != null ? weaponAnimations.sprintForwardRightClip : defaultAnimations.sprintForwardRightClip;
            CacheAnimatorController[CLIP_SPRINT_BACKWARD_LEFT] = weaponAnimations.sprintBackwardLeftClip != null ? weaponAnimations.sprintBackwardLeftClip : defaultAnimations.sprintBackwardLeftClip;
            CacheAnimatorController[CLIP_SPRINT_BACKWARD_RIGHT] = weaponAnimations.sprintBackwardRightClip != null ? weaponAnimations.sprintBackwardRightClip : defaultAnimations.sprintBackwardRightClip;
            CacheAnimatorController[CLIP_CROUCH_IDLE] = weaponAnimations.crouchIdleClip != null ? weaponAnimations.crouchIdleClip : defaultAnimations.crouchIdleClip;
            CacheAnimatorController[CLIP_CROUCH_MOVE] = weaponAnimations.crouchMoveClip != null ? weaponAnimations.crouchMoveClip : defaultAnimations.crouchMoveClip;
            CacheAnimatorController[CLIP_CROUCH_MOVE_BACKWARD] = weaponAnimations.crouchMoveBackwardClip != null ? weaponAnimations.crouchMoveBackwardClip : defaultAnimations.crouchMoveBackwardClip;
            CacheAnimatorController[CLIP_CROUCH_MOVE_LEFT] = weaponAnimations.crouchMoveLeftClip != null ? weaponAnimations.crouchMoveLeftClip : defaultAnimations.crouchMoveLeftClip;
            CacheAnimatorController[CLIP_CROUCH_MOVE_RIGHT] = weaponAnimations.crouchMoveRightClip != null ? weaponAnimations.crouchMoveRightClip : defaultAnimations.crouchMoveRightClip;
            CacheAnimatorController[CLIP_CROUCH_MOVE_FORWARD_LEFT] = weaponAnimations.crouchMoveForwardLeftClip != null ? weaponAnimations.crouchMoveForwardLeftClip : defaultAnimations.crouchMoveForwardLeftClip;
            CacheAnimatorController[CLIP_CROUCH_MOVE_FORWARD_RIGHT] = weaponAnimations.crouchMoveForwardRightClip != null ? weaponAnimations.crouchMoveForwardRightClip : defaultAnimations.crouchMoveForwardRightClip;
            CacheAnimatorController[CLIP_CROUCH_MOVE_BACKWARD_LEFT] = weaponAnimations.crouchMoveBackwardLeftClip != null ? weaponAnimations.crouchMoveBackwardLeftClip : defaultAnimations.crouchMoveBackwardLeftClip;
            CacheAnimatorController[CLIP_CROUCH_MOVE_BACKWARD_RIGHT] = weaponAnimations.crouchMoveBackwardRightClip != null ? weaponAnimations.crouchMoveBackwardRightClip : defaultAnimations.crouchMoveBackwardRightClip;
            CacheAnimatorController[CLIP_CRAWL_IDLE] = weaponAnimations.crawlIdleClip != null ? weaponAnimations.crawlIdleClip : defaultAnimations.crawlIdleClip;
            CacheAnimatorController[CLIP_CRAWL_MOVE] = weaponAnimations.crawlMoveClip != null ? weaponAnimations.crawlMoveClip : defaultAnimations.crawlMoveClip;
            CacheAnimatorController[CLIP_CRAWL_MOVE_BACKWARD] = weaponAnimations.crawlMoveBackwardClip != null ? weaponAnimations.crawlMoveBackwardClip : defaultAnimations.crawlMoveBackwardClip;
            CacheAnimatorController[CLIP_CRAWL_MOVE_LEFT] = weaponAnimations.crawlMoveLeftClip != null ? weaponAnimations.crawlMoveLeftClip : defaultAnimations.crawlMoveLeftClip;
            CacheAnimatorController[CLIP_CRAWL_MOVE_RIGHT] = weaponAnimations.crawlMoveRightClip != null ? weaponAnimations.crawlMoveRightClip : defaultAnimations.crawlMoveRightClip;
            CacheAnimatorController[CLIP_CRAWL_MOVE_FORWARD_LEFT] = weaponAnimations.crawlMoveForwardLeftClip != null ? weaponAnimations.crawlMoveForwardLeftClip : defaultAnimations.crawlMoveForwardLeftClip;
            CacheAnimatorController[CLIP_CRAWL_MOVE_FORWARD_RIGHT] = weaponAnimations.crawlMoveForwardRightClip != null ? weaponAnimations.crawlMoveForwardRightClip : defaultAnimations.crawlMoveForwardRightClip;
            CacheAnimatorController[CLIP_CRAWL_MOVE_BACKWARD_LEFT] = weaponAnimations.crawlMoveBackwardLeftClip != null ? weaponAnimations.crawlMoveBackwardLeftClip : defaultAnimations.crawlMoveBackwardLeftClip;
            CacheAnimatorController[CLIP_CRAWL_MOVE_BACKWARD_RIGHT] = weaponAnimations.crawlMoveBackwardRightClip != null ? weaponAnimations.crawlMoveBackwardRightClip : defaultAnimations.crawlMoveBackwardRightClip;
            CacheAnimatorController[CLIP_SWIM_IDLE] = weaponAnimations.swimIdleClip != null ? weaponAnimations.swimIdleClip : defaultAnimations.swimIdleClip;
            CacheAnimatorController[CLIP_SWIM_MOVE] = weaponAnimations.swimMoveClip != null ? weaponAnimations.swimMoveClip : defaultAnimations.swimMoveClip;
            CacheAnimatorController[CLIP_SWIM_MOVE_BACKWARD] = weaponAnimations.swimMoveBackwardClip != null ? weaponAnimations.swimMoveBackwardClip : defaultAnimations.swimMoveBackwardClip;
            CacheAnimatorController[CLIP_SWIM_MOVE_LEFT] = weaponAnimations.swimMoveLeftClip != null ? weaponAnimations.swimMoveLeftClip : defaultAnimations.swimMoveLeftClip;
            CacheAnimatorController[CLIP_SWIM_MOVE_RIGHT] = weaponAnimations.swimMoveRightClip != null ? weaponAnimations.swimMoveRightClip : defaultAnimations.swimMoveRightClip;
            CacheAnimatorController[CLIP_SWIM_MOVE_FORWARD_LEFT] = weaponAnimations.swimMoveForwardLeftClip != null ? weaponAnimations.swimMoveForwardLeftClip : defaultAnimations.swimMoveForwardLeftClip;
            CacheAnimatorController[CLIP_SWIM_MOVE_FORWARD_RIGHT] = weaponAnimations.swimMoveForwardRightClip != null ? weaponAnimations.swimMoveForwardRightClip : defaultAnimations.swimMoveForwardRightClip;
            CacheAnimatorController[CLIP_SWIM_MOVE_BACKWARD_LEFT] = weaponAnimations.swimMoveBackwardLeftClip != null ? weaponAnimations.swimMoveBackwardLeftClip : defaultAnimations.swimMoveBackwardLeftClip;
            CacheAnimatorController[CLIP_SWIM_MOVE_BACKWARD_RIGHT] = weaponAnimations.swimMoveBackwardRightClip != null ? weaponAnimations.swimMoveBackwardRightClip : defaultAnimations.swimMoveBackwardRightClip;
            CacheAnimatorController[CLIP_JUMP] = weaponAnimations.jumpClip != null ? weaponAnimations.jumpClip : defaultAnimations.jumpClip;
            CacheAnimatorController[CLIP_FALL] = weaponAnimations.fallClip != null ? weaponAnimations.fallClip : defaultAnimations.fallClip;
            CacheAnimatorController[CLIP_HURT] = weaponAnimations.hurtClip != null ? weaponAnimations.hurtClip : defaultAnimations.hurtClip;
            CacheAnimatorController[CLIP_DEAD] = weaponAnimations.deadClip != null ? weaponAnimations.deadClip : defaultAnimations.deadClip;
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
            float sideMoveSpeed = 0f;
            if (movementState.HasFlag(MovementState.Forward))
                moveSpeed = 1f;
            else if (movementState.HasFlag(MovementState.Backward))
                moveSpeed = -1f;
            if (movementState.HasFlag(MovementState.Right))
                sideMoveSpeed = 1f;
            else if (movementState.HasFlag(MovementState.Left))
                sideMoveSpeed = -1f;

            int moveType = 0;
            switch (extraMovementState)
            {
                case ExtraMovementState.IsCrouching:
                    moveType = 1;
                    break;
                case ExtraMovementState.IsCrawling:
                    moveType = 2;
                    break;
                case ExtraMovementState.IsSprinting:
                    moveSpeed *= 2f;
                    sideMoveSpeed *= 2f;
                    break;
            }

            // Set animator parameters
            float deltaTime = animator.updateMode == AnimatorUpdateMode.AnimatePhysics ? Time.fixedDeltaTime : Time.deltaTime;
            animator.SetFloat(ANIM_MOVE_SPEED, isDead ? 0 : moveSpeed, movementDampingTme, deltaTime);
            animator.SetFloat(ANIM_SIDE_MOVE_SPEED, isDead ? 0 : sideMoveSpeed, movementDampingTme, deltaTime);
            animator.SetFloat(ANIM_MOVE_CLIP_MULTIPLIER, moveAnimationSpeedMultiplier);
            animator.SetBool(ANIM_IS_DEAD, isDead);
            animator.SetBool(ANIM_IS_GROUNDED, !isUnderWater && movementState.HasFlag(MovementState.IsGrounded));
            animator.SetBool(ANIM_IS_UNDER_WATER, isUnderWater);
            animator.SetInteger(ANIM_MOVE_TYPE, moveType);
        }

        public override Coroutine PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1f)
        {
            return StartCoroutine(PlayActionAnimation_Animator(animActionType, dataId, index, playSpeedMultiplier));
        }

        private IEnumerator PlayActionAnimation_Animator(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier)
        {
            // If animator is not null, play the action animation
            ActionAnimation tempActionAnimation = GetActionAnimation(animActionType, dataId, index);
            if (tempActionAnimation.clip != null)
            {
                CacheAnimatorController[CLIP_ACTION] = tempActionAnimation.clip;
                yield return 0;
            }
            AudioClip audioClip = tempActionAnimation.GetRandomAudioClip();
            if (audioClip != null)
                AudioSource.PlayClipAtPoint(audioClip, CacheTransform.position, AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level);
            if (tempActionAnimation.clip != null)
            {
                animator.SetFloat(ANIM_ACTION_CLIP_MULTIPLIER, playSpeedMultiplier);
                animator.SetBool(ANIM_DO_ACTION, true);
                animator.Play(0, actionStateLayer, 0f);
            }
            // Waits by current transition + clip duration before end animation
            yield return new WaitForSecondsRealtime(tempActionAnimation.GetClipLength() / playSpeedMultiplier);
            if (tempActionAnimation.clip != null)
                animator.SetBool(ANIM_DO_ACTION, false);
            // Waits by current transition + extra duration before end playing animation state
            yield return new WaitForSecondsRealtime(tempActionAnimation.GetExtraDuration() / playSpeedMultiplier);
        }

        public override Coroutine PlaySkillCastClip(int dataId, float duration)
        {
            return StartCoroutine(PlaySkillCastClip_Animator(dataId, duration));
        }

        private IEnumerator PlaySkillCastClip_Animator(int dataId, float duration)
        {
            AnimationClip castClip = GetSkillCastClip(dataId);
            CacheAnimatorController[CLIP_CAST_SKILL] = castClip;
            yield return 0;
            animator.SetBool(ANIM_IS_CASTING_SKILL, true);
            animator.Play(0, castSkillStateLayer, 0f);
            yield return new WaitForSecondsRealtime(duration);
            animator.SetBool(ANIM_IS_CASTING_SKILL, false);
        }

        public override void StopActionAnimation()
        {
            animator.SetBool(ANIM_DO_ACTION, false);
        }

        public override void StopSkillCastAnimation()
        {
            animator.SetBool(ANIM_IS_CASTING_SKILL, false);
        }

        public override void PlayHitAnimation()
        {
            animator.ResetTrigger(ANIM_HURT);
            animator.SetTrigger(ANIM_HURT);
        }

        public override void PlayJumpAnimation()
        {
            animator.ResetTrigger(ANIM_JUMP);
            animator.SetTrigger(ANIM_JUMP);
        }
    }
}
