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
        public static readonly int ANIM_MOVE_SPEED = Animator.StringToHash("MoveSpeed");
        public static readonly int ANIM_SIDE_MOVE_SPEED = Animator.StringToHash("SideMoveSpeed");
        public static readonly int ANIM_DO_ACTION = Animator.StringToHash("DoAction");
        public static readonly int ANIM_IS_CASTING_SKILL = Animator.StringToHash("IsCastingSkill");
        public static readonly int ANIM_HURT = Animator.StringToHash("Hurt");
        public static readonly int ANIM_JUMP = Animator.StringToHash("Jump");
        public static readonly int ANIM_MOVE_CLIP_MULTIPLIER = Animator.StringToHash("MoveSpeedMultiplier");
        public static readonly int ANIM_ACTION_CLIP_MULTIPLIER = Animator.StringToHash("ActionSpeedMultiplier");

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
                CacheAnimatorController[CLIP_JUMP] = defaultAnimations.jumpClip;
                CacheAnimatorController[CLIP_FALL] = defaultAnimations.fallClip;
                CacheAnimatorController[CLIP_HURT] = defaultAnimations.hurtClip;
                CacheAnimatorController[CLIP_DEAD] = defaultAnimations.deadClip;
            }
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
            CacheAnimatorController[CLIP_IDLE] = defaultAnimations.idleClip;
            CacheAnimatorController[CLIP_MOVE] = defaultAnimations.moveClip;
            CacheAnimatorController[CLIP_MOVE_BACKWARD] = defaultAnimations.moveBackwardClip;
            CacheAnimatorController[CLIP_MOVE_LEFT] = defaultAnimations.moveLeftClip;
            CacheAnimatorController[CLIP_MOVE_RIGHT] = defaultAnimations.moveRightClip;
            CacheAnimatorController[CLIP_MOVE_FORWARD_LEFT] = defaultAnimations.moveForwardLeftClip;
            CacheAnimatorController[CLIP_MOVE_FORWARD_RIGHT] = defaultAnimations.moveForwardRightClip;
            CacheAnimatorController[CLIP_MOVE_BACKWARD_LEFT] = defaultAnimations.moveBackwardLeftClip;
            CacheAnimatorController[CLIP_MOVE_BACKWARD_RIGHT] = defaultAnimations.moveBackwardRightClip;
            CacheAnimatorController[CLIP_JUMP] = defaultAnimations.jumpClip;
            CacheAnimatorController[CLIP_FALL] = defaultAnimations.fallClip;
            CacheAnimatorController[CLIP_HURT] = defaultAnimations.hurtClip;
            CacheAnimatorController[CLIP_DEAD] = defaultAnimations.deadClip;
        }

        public override void SetEquipWeapons(EquipWeapons equipWeapons)
        {
            base.SetEquipWeapons(equipWeapons);
            SetupComponent();
            SetClipBasedOnWeapon(equipWeapons);
        }

        protected void SetClipBasedOnWeapon(EquipWeapons equipWeapons)
        {
            Item weaponItem = GameInstance.Singleton.DefaultWeaponItem;
            if (equipWeapons.rightHand.NotEmptySlot() && equipWeapons.rightHand.GetWeaponItem() != null)
                weaponItem = equipWeapons.rightHand.GetWeaponItem();
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
                moveSpeed = 1;
            else if (movementState.HasFlag(MovementState.Backward))
                moveSpeed = -1;
            if (movementState.HasFlag(MovementState.Right))
                sideMoveSpeed = 1;
            else if (movementState.HasFlag(MovementState.Left))
                sideMoveSpeed = -1;
            // Set animator parameters
            animator.SetFloat(ANIM_MOVE_SPEED, isDead ? 0 : moveSpeed);
            animator.SetFloat(ANIM_SIDE_MOVE_SPEED, isDead ? 0 : sideMoveSpeed);
            animator.SetFloat(ANIM_MOVE_CLIP_MULTIPLIER, moveAnimationSpeedMultiplier);
            animator.SetBool(ANIM_IS_DEAD, isDead);
            animator.SetBool(ANIM_IS_GROUNDED, movementState.HasFlag(MovementState.IsGrounded));
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
