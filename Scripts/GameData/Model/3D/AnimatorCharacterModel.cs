using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public partial class AnimatorCharacterModel : BaseRemakeCharacterModel
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
        public static readonly int ANIM_DO_ACTION_ALL_LAYERS = Animator.StringToHash("DoActionAllLayers");
        public static readonly int ANIM_IS_CASTING_SKILL = Animator.StringToHash("IsCastingSkill");
        public static readonly int ANIM_IS_CASTING_SKILL_ALL_LAYERS = Animator.StringToHash("IsCastingSkillAllLayers");
        public static readonly int ANIM_HURT = Animator.StringToHash("Hurt");
        public static readonly int ANIM_JUMP = Animator.StringToHash("Jump");
        public static readonly int ANIM_MOVE_CLIP_MULTIPLIER = Animator.StringToHash("MoveSpeedMultiplier");
        public static readonly int ANIM_ACTION_CLIP_MULTIPLIER = Animator.StringToHash("ActionSpeedMultiplier");
        public static readonly int ANIM_MOVE_TYPE = Animator.StringToHash("MoveType");

        [Header("Settings")]
        [Tooltip("The damping time for the `MoveSpeed` and `SideMoveSpeed` parameters. The higher the value the slower the parameter value changes.")]
        public float movementDampingTme = 0.1f;
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

#if UNITY_EDITOR
        [Header("Animation Test Tool")]
        public AnimActionType testAnimActionType;
        public WeaponType testWeaponType;
        public BaseSkill testSkill;
        public int testAttackAnimIndex;
        [InspectorButton("SetAnimatorClipsForTest")]
        public bool setAnimatorClipsForTest;
#endif

        public AnimatorOverrideController CacheAnimatorController { get; private set; }

        // Private state validater
        private bool isSetupComponent;
        private float moveAnimSpeedRate;
        private float sprintAnimSpeedRate;
        private float crouchMoveAnimSpeedRate;
        private float crawlMoveAnimSpeedRate;
        private float swimMoveAnimSpeedRate;

        protected override void Awake()
        {
            base.Awake();
            SetupComponent();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
#if UNITY_EDITOR
            bool hasChanges = false;
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
                if (animator != null)
                    hasChanges = true;
            }

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
            if (animator == null)
                Logging.LogError(ToString(), "`Animator` is empty");
            if (animatorController == null)
                Logging.LogError(ToString(), "`Animator Controller` is empty");
            if (hasChanges)
            {
                isSetupComponent = false;
                SetupComponent();
                EditorUtility.SetDirty(this);
            }
#endif
        }

        private void SetupComponent()
        {
            if (isSetupComponent)
                return;
            isSetupComponent = true;
            if (CacheAnimatorController == null)
                CacheAnimatorController = new AnimatorOverrideController(animatorController);
            // Use override controller as animator
            if (animator != null && animator.runtimeAnimatorController != CacheAnimatorController)
                animator.runtimeAnimatorController = CacheAnimatorController;
            SetDefaultAnimations();
        }

        public override void SetDefaultAnimations()
        {
            SetupClips(defaultAnimations.idleClip,
                defaultAnimations.moveClip,
                defaultAnimations.moveBackwardClip,
                defaultAnimations.moveLeftClip,
                defaultAnimations.moveRightClip,
                defaultAnimations.moveForwardLeftClip,
                defaultAnimations.moveForwardRightClip,
                defaultAnimations.moveBackwardLeftClip,
                defaultAnimations.moveBackwardRightClip,
                defaultAnimations.sprintClip,
                defaultAnimations.sprintBackwardClip,
                defaultAnimations.sprintLeftClip,
                defaultAnimations.sprintRightClip,
                defaultAnimations.sprintForwardLeftClip,
                defaultAnimations.sprintForwardRightClip,
                defaultAnimations.sprintBackwardLeftClip,
                defaultAnimations.sprintBackwardRightClip,
                defaultAnimations.crouchIdleClip,
                defaultAnimations.crouchMoveClip,
                defaultAnimations.crouchMoveBackwardClip,
                defaultAnimations.crouchMoveLeftClip,
                defaultAnimations.crouchMoveRightClip,
                defaultAnimations.crouchMoveForwardLeftClip,
                defaultAnimations.crouchMoveForwardRightClip,
                defaultAnimations.crouchMoveBackwardLeftClip,
                defaultAnimations.crouchMoveBackwardRightClip,
                defaultAnimations.crawlIdleClip,
                defaultAnimations.crawlMoveClip,
                defaultAnimations.crawlMoveBackwardClip,
                defaultAnimations.crawlMoveLeftClip,
                defaultAnimations.crawlMoveRightClip,
                defaultAnimations.crawlMoveForwardLeftClip,
                defaultAnimations.crawlMoveForwardRightClip,
                defaultAnimations.crawlMoveBackwardLeftClip,
                defaultAnimations.crawlMoveBackwardRightClip,
                defaultAnimations.swimIdleClip,
                defaultAnimations.swimMoveClip,
                defaultAnimations.swimMoveBackwardClip,
                defaultAnimations.swimMoveLeftClip,
                defaultAnimations.swimMoveRightClip,
                defaultAnimations.swimMoveForwardLeftClip,
                defaultAnimations.swimMoveForwardRightClip,
                defaultAnimations.swimMoveBackwardLeftClip,
                defaultAnimations.swimMoveBackwardRightClip,
                defaultAnimations.jumpClip,
                defaultAnimations.fallClip,
                defaultAnimations.hurtClip,
                defaultAnimations.deadClip,
                defaultAnimations.moveAnimSpeedRate,
                defaultAnimations.sprintAnimSpeedRate,
                defaultAnimations.crouchMoveAnimSpeedRate,
                defaultAnimations.crawlMoveAnimSpeedRate,
                defaultAnimations.swimMoveAnimSpeedRate);
            base.SetDefaultAnimations();
        }

        private void SetupClips(
                AnimationClip idleClip,
                AnimationClip moveClip,
                AnimationClip moveBackwardClip,
                AnimationClip moveLeftClip,
                AnimationClip moveRightClip,
                AnimationClip moveForwardLeftClip,
                AnimationClip moveForwardRightClip,
                AnimationClip moveBackwardLeftClip,
                AnimationClip moveBackwardRightClip,
                AnimationClip sprintClip,
                AnimationClip sprintBackwardClip,
                AnimationClip sprintLeftClip,
                AnimationClip sprintRightClip,
                AnimationClip sprintForwardLeftClip,
                AnimationClip sprintForwardRightClip,
                AnimationClip sprintBackwardLeftClip,
                AnimationClip sprintBackwardRightClip,
                AnimationClip crouchIdleClip,
                AnimationClip crouchMoveClip,
                AnimationClip crouchMoveBackwardClip,
                AnimationClip crouchMoveLeftClip,
                AnimationClip crouchMoveRightClip,
                AnimationClip crouchMoveForwardLeftClip,
                AnimationClip crouchMoveForwardRightClip,
                AnimationClip crouchMoveBackwardLeftClip,
                AnimationClip crouchMoveBackwardRightClip,
                AnimationClip crawlIdleClip,
                AnimationClip crawlMoveClip,
                AnimationClip crawlMoveBackwardClip,
                AnimationClip crawlMoveLeftClip,
                AnimationClip crawlMoveRightClip,
                AnimationClip crawlMoveForwardLeftClip,
                AnimationClip crawlMoveForwardRightClip,
                AnimationClip crawlMoveBackwardLeftClip,
                AnimationClip crawlMoveBackwardRightClip,
                AnimationClip swimIdleClip,
                AnimationClip swimMoveClip,
                AnimationClip swimMoveBackwardClip,
                AnimationClip swimMoveLeftClip,
                AnimationClip swimMoveRightClip,
                AnimationClip swimMoveForwardLeftClip,
                AnimationClip swimMoveForwardRightClip,
                AnimationClip swimMoveBackwardLeftClip,
                AnimationClip swimMoveBackwardRightClip,
                AnimationClip jumpClip,
                AnimationClip fallClip,
                AnimationClip hurtClip,
                AnimationClip deadClip,
                float moveAnimSpeedRate,
                float sprintAnimSpeedRate,
                float crouchMoveAnimSpeedRate,
                float crawlMoveAnimSpeedRate,
                float swimMoveAnimSpeedRate)
        {
            if (CacheAnimatorController == null)
                return;
            CacheAnimatorController[CLIP_IDLE] = idleClip != null ? idleClip : defaultAnimations.idleClip;
            CacheAnimatorController[CLIP_MOVE] = moveClip != null ? moveClip : defaultAnimations.moveClip;
            CacheAnimatorController[CLIP_MOVE_BACKWARD] = moveBackwardClip != null ? moveBackwardClip : defaultAnimations.moveBackwardClip;
            CacheAnimatorController[CLIP_MOVE_LEFT] = moveLeftClip != null ? moveLeftClip : defaultAnimations.moveLeftClip;
            CacheAnimatorController[CLIP_MOVE_RIGHT] = moveRightClip != null ? moveRightClip : defaultAnimations.moveRightClip;
            CacheAnimatorController[CLIP_MOVE_FORWARD_LEFT] = moveForwardLeftClip != null ? moveForwardLeftClip : defaultAnimations.moveForwardLeftClip;
            CacheAnimatorController[CLIP_MOVE_FORWARD_RIGHT] = moveForwardRightClip != null ? moveForwardRightClip : defaultAnimations.moveForwardRightClip;
            CacheAnimatorController[CLIP_MOVE_BACKWARD_LEFT] = moveBackwardLeftClip != null ? moveBackwardLeftClip : defaultAnimations.moveBackwardLeftClip;
            CacheAnimatorController[CLIP_MOVE_BACKWARD_RIGHT] = moveBackwardRightClip != null ? moveBackwardRightClip : defaultAnimations.moveBackwardRightClip;
            CacheAnimatorController[CLIP_SPRINT] = sprintClip != null ? sprintClip : defaultAnimations.sprintClip;
            CacheAnimatorController[CLIP_SPRINT_BACKWARD] = sprintBackwardClip != null ? sprintBackwardClip : defaultAnimations.sprintBackwardClip;
            CacheAnimatorController[CLIP_SPRINT_LEFT] = sprintLeftClip != null ? sprintLeftClip : defaultAnimations.sprintLeftClip;
            CacheAnimatorController[CLIP_SPRINT_RIGHT] = sprintRightClip != null ? sprintRightClip : defaultAnimations.sprintRightClip;
            CacheAnimatorController[CLIP_SPRINT_FORWARD_LEFT] = sprintForwardLeftClip != null ? sprintForwardLeftClip : defaultAnimations.sprintForwardLeftClip;
            CacheAnimatorController[CLIP_SPRINT_FORWARD_RIGHT] = sprintForwardRightClip != null ? sprintForwardRightClip : defaultAnimations.sprintForwardRightClip;
            CacheAnimatorController[CLIP_SPRINT_BACKWARD_LEFT] = sprintBackwardLeftClip != null ? sprintBackwardLeftClip : defaultAnimations.sprintBackwardLeftClip;
            CacheAnimatorController[CLIP_SPRINT_BACKWARD_RIGHT] = sprintBackwardRightClip != null ? sprintBackwardRightClip : defaultAnimations.sprintBackwardRightClip;
            CacheAnimatorController[CLIP_CROUCH_IDLE] = crouchIdleClip != null ? crouchIdleClip : defaultAnimations.crouchIdleClip;
            CacheAnimatorController[CLIP_CROUCH_MOVE] = crouchMoveClip != null ? crouchMoveClip : defaultAnimations.crouchMoveClip;
            CacheAnimatorController[CLIP_CROUCH_MOVE_BACKWARD] = crouchMoveBackwardClip != null ? crouchMoveBackwardClip : defaultAnimations.crouchMoveBackwardClip;
            CacheAnimatorController[CLIP_CROUCH_MOVE_LEFT] = crouchMoveLeftClip != null ? crouchMoveLeftClip : defaultAnimations.crouchMoveLeftClip;
            CacheAnimatorController[CLIP_CROUCH_MOVE_RIGHT] = crouchMoveRightClip != null ? crouchMoveRightClip : defaultAnimations.crouchMoveRightClip;
            CacheAnimatorController[CLIP_CROUCH_MOVE_FORWARD_LEFT] = crouchMoveForwardLeftClip != null ? crouchMoveForwardLeftClip : defaultAnimations.crouchMoveForwardLeftClip;
            CacheAnimatorController[CLIP_CROUCH_MOVE_FORWARD_RIGHT] = crouchMoveForwardRightClip != null ? crouchMoveForwardRightClip : defaultAnimations.crouchMoveForwardRightClip;
            CacheAnimatorController[CLIP_CROUCH_MOVE_BACKWARD_LEFT] = crouchMoveBackwardLeftClip != null ? crouchMoveBackwardLeftClip : defaultAnimations.crouchMoveBackwardLeftClip;
            CacheAnimatorController[CLIP_CROUCH_MOVE_BACKWARD_RIGHT] = crouchMoveBackwardRightClip != null ? crouchMoveBackwardRightClip : defaultAnimations.crouchMoveBackwardRightClip;
            CacheAnimatorController[CLIP_CRAWL_IDLE] = crawlIdleClip != null ? crawlIdleClip : defaultAnimations.crawlIdleClip;
            CacheAnimatorController[CLIP_CRAWL_MOVE] = crawlMoveClip != null ? crawlMoveClip : defaultAnimations.crawlMoveClip;
            CacheAnimatorController[CLIP_CRAWL_MOVE_BACKWARD] = crawlMoveBackwardClip != null ? crawlMoveBackwardClip : defaultAnimations.crawlMoveBackwardClip;
            CacheAnimatorController[CLIP_CRAWL_MOVE_LEFT] = crawlMoveLeftClip != null ? crawlMoveLeftClip : defaultAnimations.crawlMoveLeftClip;
            CacheAnimatorController[CLIP_CRAWL_MOVE_RIGHT] = crawlMoveRightClip != null ? crawlMoveRightClip : defaultAnimations.crawlMoveRightClip;
            CacheAnimatorController[CLIP_CRAWL_MOVE_FORWARD_LEFT] = crawlMoveForwardLeftClip != null ? crawlMoveForwardLeftClip : defaultAnimations.crawlMoveForwardLeftClip;
            CacheAnimatorController[CLIP_CRAWL_MOVE_FORWARD_RIGHT] = crawlMoveForwardRightClip != null ? crawlMoveForwardRightClip : defaultAnimations.crawlMoveForwardRightClip;
            CacheAnimatorController[CLIP_CRAWL_MOVE_BACKWARD_LEFT] = crawlMoveBackwardLeftClip != null ? crawlMoveBackwardLeftClip : defaultAnimations.crawlMoveBackwardLeftClip;
            CacheAnimatorController[CLIP_CRAWL_MOVE_BACKWARD_RIGHT] = crawlMoveBackwardRightClip != null ? crawlMoveBackwardRightClip : defaultAnimations.crawlMoveBackwardRightClip;
            CacheAnimatorController[CLIP_SWIM_IDLE] = swimIdleClip != null ? swimIdleClip : defaultAnimations.swimIdleClip;
            CacheAnimatorController[CLIP_SWIM_MOVE] = swimMoveClip != null ? swimMoveClip : defaultAnimations.swimMoveClip;
            CacheAnimatorController[CLIP_SWIM_MOVE_BACKWARD] = swimMoveBackwardClip != null ? swimMoveBackwardClip : defaultAnimations.swimMoveBackwardClip;
            CacheAnimatorController[CLIP_SWIM_MOVE_LEFT] = swimMoveLeftClip != null ? swimMoveLeftClip : defaultAnimations.swimMoveLeftClip;
            CacheAnimatorController[CLIP_SWIM_MOVE_RIGHT] = swimMoveRightClip != null ? swimMoveRightClip : defaultAnimations.swimMoveRightClip;
            CacheAnimatorController[CLIP_SWIM_MOVE_FORWARD_LEFT] = swimMoveForwardLeftClip != null ? swimMoveForwardLeftClip : defaultAnimations.swimMoveForwardLeftClip;
            CacheAnimatorController[CLIP_SWIM_MOVE_FORWARD_RIGHT] = swimMoveForwardRightClip != null ? swimMoveForwardRightClip : defaultAnimations.swimMoveForwardRightClip;
            CacheAnimatorController[CLIP_SWIM_MOVE_BACKWARD_LEFT] = swimMoveBackwardLeftClip != null ? swimMoveBackwardLeftClip : defaultAnimations.swimMoveBackwardLeftClip;
            CacheAnimatorController[CLIP_SWIM_MOVE_BACKWARD_RIGHT] = swimMoveBackwardRightClip != null ? swimMoveBackwardRightClip : defaultAnimations.swimMoveBackwardRightClip;
            CacheAnimatorController[CLIP_JUMP] = jumpClip != null ? jumpClip : defaultAnimations.jumpClip;
            CacheAnimatorController[CLIP_FALL] = fallClip != null ? fallClip : defaultAnimations.fallClip;
            CacheAnimatorController[CLIP_HURT] = hurtClip != null ? hurtClip : defaultAnimations.hurtClip;
            CacheAnimatorController[CLIP_DEAD] = deadClip != null ? deadClip : defaultAnimations.deadClip;
            this.moveAnimSpeedRate = moveAnimSpeedRate > 0f ? moveAnimSpeedRate :
                defaultAnimations.moveAnimSpeedRate > 0f ? defaultAnimations.moveAnimSpeedRate : 1f;
            this.sprintAnimSpeedRate = sprintAnimSpeedRate > 0f ? sprintAnimSpeedRate :
                defaultAnimations.sprintAnimSpeedRate > 0f ? defaultAnimations.sprintAnimSpeedRate : 1f;
            this.crouchMoveAnimSpeedRate = crouchMoveAnimSpeedRate > 0f ? crouchMoveAnimSpeedRate :
                defaultAnimations.crouchMoveAnimSpeedRate > 0f ? defaultAnimations.crouchMoveAnimSpeedRate : 1f;
            this.crawlMoveAnimSpeedRate = crawlMoveAnimSpeedRate > 0f ? crawlMoveAnimSpeedRate :
                defaultAnimations.crawlMoveAnimSpeedRate > 0f ? defaultAnimations.crawlMoveAnimSpeedRate : 1f;
            this.swimMoveAnimSpeedRate = swimMoveAnimSpeedRate > 0f ? swimMoveAnimSpeedRate :
                defaultAnimations.swimMoveAnimSpeedRate > 0f ? defaultAnimations.swimMoveAnimSpeedRate : 1f;
        }

        public override void SetEquipWeapons(EquipWeapons equipWeapons)
        {
            base.SetEquipWeapons(equipWeapons);
            SetupComponent();
            SetClipBasedOnWeapon(equipWeapons);
        }

        protected void SetClipBasedOnWeapon(EquipWeapons equipWeapons)
        {
            if (GameInstance.Singleton == null)
                return;

            IWeaponItem weaponItem = equipWeapons.GetRightHandWeaponItem();
            if (weaponItem == null)
                weaponItem = GameInstance.Singleton.DefaultWeaponItem;

            SetClipBasedOnWeaponType(weaponItem.WeaponType);
        }

        protected void SetClipBasedOnWeaponType(WeaponType weaponType)
        {
            WeaponAnimations weaponAnimations = default(WeaponAnimations);
            GetAnims().CacheWeaponAnimations.TryGetValue(weaponType.DataId, out weaponAnimations);

            SetupClips(weaponAnimations.idleClip,
                weaponAnimations.moveClip,
                weaponAnimations.moveBackwardClip,
                weaponAnimations.moveLeftClip,
                weaponAnimations.moveRightClip,
                weaponAnimations.moveForwardLeftClip,
                weaponAnimations.moveForwardRightClip,
                weaponAnimations.moveBackwardLeftClip,
                weaponAnimations.moveBackwardRightClip,
                weaponAnimations.sprintClip,
                weaponAnimations.sprintBackwardClip,
                weaponAnimations.sprintLeftClip,
                weaponAnimations.sprintRightClip,
                weaponAnimations.sprintForwardLeftClip,
                weaponAnimations.sprintForwardRightClip,
                weaponAnimations.sprintBackwardLeftClip,
                weaponAnimations.sprintBackwardRightClip,
                weaponAnimations.crouchIdleClip,
                weaponAnimations.crouchMoveClip,
                weaponAnimations.crouchMoveBackwardClip,
                weaponAnimations.crouchMoveLeftClip,
                weaponAnimations.crouchMoveRightClip,
                weaponAnimations.crouchMoveForwardLeftClip,
                weaponAnimations.crouchMoveForwardRightClip,
                weaponAnimations.crouchMoveBackwardLeftClip,
                weaponAnimations.crouchMoveBackwardRightClip,
                weaponAnimations.crawlIdleClip,
                weaponAnimations.crawlMoveClip,
                weaponAnimations.crawlMoveBackwardClip,
                weaponAnimations.crawlMoveLeftClip,
                weaponAnimations.crawlMoveRightClip,
                weaponAnimations.crawlMoveForwardLeftClip,
                weaponAnimations.crawlMoveForwardRightClip,
                weaponAnimations.crawlMoveBackwardLeftClip,
                weaponAnimations.crawlMoveBackwardRightClip,
                weaponAnimations.swimIdleClip,
                weaponAnimations.swimMoveClip,
                weaponAnimations.swimMoveBackwardClip,
                weaponAnimations.swimMoveLeftClip,
                weaponAnimations.swimMoveRightClip,
                weaponAnimations.swimMoveForwardLeftClip,
                weaponAnimations.swimMoveForwardRightClip,
                weaponAnimations.swimMoveBackwardLeftClip,
                weaponAnimations.swimMoveBackwardRightClip,
                weaponAnimations.jumpClip,
                weaponAnimations.fallClip,
                weaponAnimations.hurtClip,
                weaponAnimations.deadClip,
                weaponAnimations.moveAnimSpeedRate,
                weaponAnimations.sprintAnimSpeedRate,
                weaponAnimations.crouchMoveAnimSpeedRate,
                weaponAnimations.crawlMoveAnimSpeedRate,
                weaponAnimations.swimMoveAnimSpeedRate);
        }

        public override void PlayMoveAnimation()
        {
            if (!animator.isActiveAndEnabled)
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

            float moveAnimationSpeedMultiplier = this.moveAnimationSpeedMultiplier;

            // Set move speed based on inputs
            int moveSpeed = 0;
            if (movementState.HasFlag(MovementState.Forward))
                moveSpeed = 1;
            else if (movementState.HasFlag(MovementState.Backward))
                moveSpeed = -1;

            // Set side move speed based on inputs
            int sideMoveSpeed = 0;
            if (movementState.HasFlag(MovementState.Right))
                sideMoveSpeed = 1;
            else if (movementState.HasFlag(MovementState.Left))
                sideMoveSpeed = -1;

            int moveType = 0;
            switch (extraMovementState)
            {
                case ExtraMovementState.IsCrouching:
                    moveType = 1;
                    moveAnimationSpeedMultiplier *= crouchMoveAnimSpeedRate;
                    break;
                case ExtraMovementState.IsCrawling:
                    moveType = 2;
                    moveAnimationSpeedMultiplier *= crawlMoveAnimSpeedRate;
                    break;
                case ExtraMovementState.IsSprinting:
                    moveSpeed *= 2;
                    sideMoveSpeed *= 2;
                    moveAnimationSpeedMultiplier *= sprintAnimSpeedRate;
                    break;
                default:
                    moveAnimationSpeedMultiplier *= moveAnimSpeedRate;
                    break;
            }

            if (movementState.HasFlag(MovementState.IsUnderWater))
                moveAnimationSpeedMultiplier *= swimMoveAnimSpeedRate;

            // Character is idle, so set move animation speed multiplier to 1
            if (moveSpeed == 0 && sideMoveSpeed == 0)
                moveAnimationSpeedMultiplier = 1f;

            // Set animator parameters
            float deltaTime = animator.updateMode == AnimatorUpdateMode.AnimatePhysics ? Time.fixedDeltaTime : Time.deltaTime;
            animator.SetFloat(ANIM_MOVE_SPEED, isDead ? 0 : moveSpeed, movementDampingTme, deltaTime);
            animator.SetFloat(ANIM_SIDE_MOVE_SPEED, isDead ? 0 : sideMoveSpeed, movementDampingTme, deltaTime);
            animator.SetFloat(ANIM_MOVE_CLIP_MULTIPLIER, moveAnimationSpeedMultiplier);
            animator.SetBool(ANIM_IS_DEAD, isDead);
            animator.SetBool(ANIM_IS_GROUNDED, !movementState.HasFlag(MovementState.IsUnderWater) && movementState.HasFlag(MovementState.IsGrounded));
            animator.SetBool(ANIM_IS_UNDER_WATER, movementState.HasFlag(MovementState.IsUnderWater));
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
            if (tempActionAnimation.clip)
            {
                CacheAnimatorController[CLIP_ACTION] = tempActionAnimation.clip;
                yield return 0;
            }
            AudioClip audioClip = tempActionAnimation.GetRandomAudioClip();
            if (audioClip != null)
                AudioSource.PlayClipAtPoint(audioClip, CacheTransform.position, AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level);
            if (tempActionAnimation.clip)
            {
                animator.SetFloat(ANIM_ACTION_CLIP_MULTIPLIER, playSpeedMultiplier);
                animator.SetBool(ANIM_DO_ACTION, true);
                animator.SetBool(ANIM_DO_ACTION_ALL_LAYERS, tempActionAnimation.playClipAllLayers);
                if (tempActionAnimation.playClipAllLayers)
                {
                    for (int i = 0; i < animator.layerCount; ++i)
                    {
                        animator.Play(0, i, 0f);
                    }
                }
                else
                {
                    animator.Play(0, actionStateLayer, 0f);
                }
            }
            // Waits by current transition + clip duration before end animation
            yield return new WaitForSecondsRealtime(tempActionAnimation.GetClipLength() / playSpeedMultiplier);
            if (tempActionAnimation.clip)
            {
                animator.SetBool(ANIM_DO_ACTION, false);
                animator.SetBool(ANIM_DO_ACTION_ALL_LAYERS, false);
            }
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
            bool playAllLayers = IsSkillCastClipPlayingAllLayers(dataId);
            CacheAnimatorController[CLIP_CAST_SKILL] = castClip;
            yield return 0;
            animator.SetBool(ANIM_IS_CASTING_SKILL, true);
            animator.SetBool(ANIM_IS_CASTING_SKILL_ALL_LAYERS, playAllLayers);
            if (playAllLayers)
            {
                for (int i = 0; i < animator.layerCount; ++i)
                {
                    animator.Play(0, i, 0f);
                }
            }
            else
            {
                animator.Play(0, castSkillStateLayer, 0f);
            }
            yield return new WaitForSecondsRealtime(duration);
            animator.SetBool(ANIM_IS_CASTING_SKILL, false);
            animator.SetBool(ANIM_IS_CASTING_SKILL_ALL_LAYERS, false);
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
            StartCoroutine(PlayHitAnimationRoutine());
        }

        IEnumerator PlayHitAnimationRoutine()
        {
            yield return null;
            animator.ResetTrigger(ANIM_HURT);
            animator.SetTrigger(ANIM_HURT);
        }

        public override void PlayJumpAnimation()
        {
            StartCoroutine(PlayJumpAnimationRoutine());
        }

        IEnumerator PlayJumpAnimationRoutine()
        {
            yield return null;
            animator.ResetTrigger(ANIM_JUMP);
            animator.SetTrigger(ANIM_JUMP);
        }

#if UNITY_EDITOR
        [ContextMenu("Set Animator Clips For Test")]
        public void SetAnimatorClipsForTest()
        {
            SetupComponent();

            int testActionAnimDataId = 0;
            int testCastSkillAnimDataId = 0;
            switch (testAnimActionType)
            {
                case AnimActionType.AttackRightHand:
                case AnimActionType.AttackLeftHand:
                    if (testWeaponType != null)
                        testActionAnimDataId = testWeaponType.DataId;
                    break;
                case AnimActionType.SkillRightHand:
                case AnimActionType.SkillLeftHand:
                    if (testSkill != null)
                    {
                        testActionAnimDataId = testSkill.DataId;
                        testCastSkillAnimDataId = testSkill.DataId;
                    }
                    break;
                case AnimActionType.ReloadRightHand:
                case AnimActionType.ReloadLeftHand:
                    if (testWeaponType != null)
                        testActionAnimDataId = testWeaponType.DataId;
                    break;
            }

            // Movement animation clips
            SetDefaultAnimations();
            if (testWeaponType != null)
                SetClipBasedOnWeaponType(testWeaponType);

            // Action animation clips
            ActionAnimation tempActionAnimation = GetActionAnimation(testAnimActionType, testActionAnimDataId, testAttackAnimIndex);
            CacheAnimatorController[CLIP_ACTION] = tempActionAnimation.clip;

            // Skill animation clips
            AnimationClip castClip = GetSkillCastClip(testCastSkillAnimDataId);
            CacheAnimatorController[CLIP_CAST_SKILL] = castClip;

            Logging.Log(ToString(), "Animation Clips already set to animator controller, you can test an animations in Animation tab");

            this.InvokeInstanceDevExtMethods("SetAnimatorClipsForTest");
        }
#endif
    }
}
