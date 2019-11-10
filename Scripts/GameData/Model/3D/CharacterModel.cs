using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [ExecuteInEditMode]
    public class CharacterModel : BaseCharacterModelWithCacheAnims<WeaponAnimations, SkillAnimations>
    {
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
        // Legacy Animation variables
        public const string LEGACY_CLIP_IDLE = "_Idle";
        public const string LEGACY_CLIP_MOVE = "_Move";
        public const string LEGACY_CLIP_MOVE_BACKWARD = "_MoveBackward";
        public const string LEGACY_CLIP_MOVE_LEFT = "_MoveLeft";
        public const string LEGACY_CLIP_MOVE_RIGHT = "_MoveRight";
        public const string LEGACY_CLIP_MOVE_FORWARD_LEFT = "_MoveForwardLeft";
        public const string LEGACY_CLIP_MOVE_FORWARD_RIGHT = "_MoveForwardRight";
        public const string LEGACY_CLIP_MOVE_BACKWARD_LEFT = "_MoveBackwardLeft";
        public const string LEGACY_CLIP_MOVE_BACKWARD_RIGHT = "_MoveBackwardRight";
        public const string LEGACY_CLIP_JUMP = "_Jump";
        public const string LEGACY_CLIP_FALL = "_Fall";
        public const string LEGACY_CLIP_HURT = "_Hurt";
        public const string LEGACY_CLIP_DEAD = "_Dead";
        public const string LEGACY_CLIP_ACTION = "_Action";
        public const string LEGACY_CLIP_CAST_SKILL = "_CastSkill";

        public enum AnimatorType
        {
            Animator,
            LegacyAnimtion,
        }
        [Header("Animation Component Type")]
        public AnimatorType animatorType;

        [Header("Animator Settings")]
        public Animator animator;
        public RuntimeAnimatorController animatorController;
        public DefaultAnimatorData defaultAnimatorData = new DefaultAnimatorData()
        {
            idleClip = null,
            moveClip = null,
            moveBackwardClip = null,
            moveLeftClip = null,
            moveRightClip = null,
            moveForwardLeftClip = null,
            moveForwardRightClip = null,
            moveBackwardLeftClip = null,
            moveBackwardRightClip = null,
            jumpClip = null,
            fallClip = null,
            hurtClip = null,
            deadClip = null,
            actionClip = null,
            castSkillClip = null,
        };
        public int actionStateLayer;
        public int castSkillStateLayer;

        [Header("Legacy Animation Settings")]
        public Animation legacyAnimation;
        public LegacyAnimationData legacyAnimationData = new LegacyAnimationData()
        {
            idleClip = null,
            moveClip = null,
            moveBackwardClip = null,
            moveLeftClip = null,
            moveRightClip = null,
            moveForwardLeftClip = null,
            moveForwardRightClip = null,
            moveBackwardLeftClip = null,
            moveBackwardRightClip = null,
            jumpClip = null,
            fallClip = null,
            hurtClip = null,
            deadClip = null,
            actionClipFadeLength = 0.1f,
            idleClipFadeLength = 0.1f,
            moveClipFadeLength = 0.1f,
            jumpClipFadeLength = 0.1f,
            fallClipFadeLength = 0.1f,
            hurtClipFadeLength = 0.1f,
            deadClipFadeLength = 0.1f,
        };

        [Header("Renderer")]
        public SkinnedMeshRenderer skinnedMeshRenderer;

        [Header("Animations")]
        [ArrayElementTitle("clip", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public ActionAnimation[] defaultAttackAnimations;
        public AnimationClip defaultSkillCastClip;
        public ActionAnimation defaultSkillActivateAnimation;
        public ActionAnimation defaultReloadAnimation;
        [ArrayElementTitle("weaponType", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public WeaponAnimations[] weaponAnimations;
        [ArrayElementTitle("skill", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public SkillAnimations[] skillAnimations;

        // Temp data
        private string defaultIdleClipName;
        private string defaultMoveClipName;
        private string defaultMoveBackwardClipName;
        private string defaultMoveLeftClipName;
        private string defaultMoveRightClipName;
        private string defaultMoveForwardLeftClipName;
        private string defaultMoveForwardRightClipName;
        private string defaultMoveBackwardLeftClipName;
        private string defaultMoveBackwardRightClipName;
        private string defaultJumpClipName;
        private string defaultFallClipName;
        private string defaultHurtClipName;
        private string defaultDeadClipName;
        private string defaultActionClipName;
        private string defaultCastSkillClipName;
        private string lastFadedLegacyClipName;
        // Private state validater
        private bool isSetupComponent;
        private bool isPlayingActionAnimation;

        private AnimatorOverrideController cacheAnimatorController;
        public AnimatorOverrideController CacheAnimatorController
        {
            get
            {
                SetupComponent();
                return cacheAnimatorController;
            }
        }

        protected override void Awake()
        {
            SetupComponent();
            base.Awake();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
#if UNITY_EDITOR
            if (animatorType == AnimatorType.Animator &&
                CacheAnimatorController != null)
            {
                SetupGenericClips_Animator(
                    defaultAnimatorData.idleClip,
                    defaultAnimatorData.moveClip,
                    defaultAnimatorData.moveBackwardClip,
                    defaultAnimatorData.moveLeftClip,
                    defaultAnimatorData.moveRightClip,
                    defaultAnimatorData.moveForwardLeftClip,
                    defaultAnimatorData.moveForwardRightClip,
                    defaultAnimatorData.moveBackwardLeftClip,
                    defaultAnimatorData.moveBackwardRightClip,
                    defaultAnimatorData.jumpClip,
                    defaultAnimatorData.fallClip,
                    defaultAnimatorData.hurtClip,
                    defaultAnimatorData.deadClip);
            }
#endif
        }

        private void SetupComponent()
        {
            if (isSetupComponent)
                return;
            isSetupComponent = true;
            switch (animatorType)
            {
                case AnimatorType.Animator:
                    if (cacheAnimatorController == null)
                    {
                        cacheAnimatorController = new AnimatorOverrideController(animatorController);
                        defaultIdleClipName = defaultAnimatorData.idleClip != null ? defaultAnimatorData.idleClip.name : string.Empty;
                        defaultMoveClipName = defaultAnimatorData.moveClip != null ? defaultAnimatorData.moveClip.name : string.Empty;
                        defaultMoveBackwardClipName = defaultAnimatorData.moveBackwardClip != null ? defaultAnimatorData.moveBackwardClip.name : string.Empty;
                        defaultMoveLeftClipName = defaultAnimatorData.moveLeftClip != null ? defaultAnimatorData.moveLeftClip.name : string.Empty;
                        defaultMoveRightClipName = defaultAnimatorData.moveRightClip != null ? defaultAnimatorData.moveRightClip.name : string.Empty;
                        defaultMoveForwardLeftClipName = defaultAnimatorData.moveForwardLeftClip != null ? defaultAnimatorData.moveForwardLeftClip.name : string.Empty;
                        defaultMoveForwardRightClipName = defaultAnimatorData.moveForwardRightClip != null ? defaultAnimatorData.moveForwardRightClip.name : string.Empty;
                        defaultMoveBackwardLeftClipName = defaultAnimatorData.moveBackwardLeftClip != null ? defaultAnimatorData.moveBackwardLeftClip.name : string.Empty;
                        defaultMoveBackwardRightClipName = defaultAnimatorData.moveBackwardRightClip != null ? defaultAnimatorData.moveBackwardRightClip.name : string.Empty;
                        defaultJumpClipName = defaultAnimatorData.jumpClip != null ? defaultAnimatorData.jumpClip.name : string.Empty;
                        defaultFallClipName = defaultAnimatorData.fallClip != null ? defaultAnimatorData.fallClip.name : string.Empty;
                        defaultHurtClipName = defaultAnimatorData.hurtClip != null ? defaultAnimatorData.hurtClip.name : string.Empty;
                        defaultDeadClipName = defaultAnimatorData.deadClip != null ? defaultAnimatorData.deadClip.name : string.Empty;
                        defaultActionClipName = defaultAnimatorData.actionClip != null ? defaultAnimatorData.actionClip.name : string.Empty;
                        defaultCastSkillClipName = defaultAnimatorData.castSkillClip != null ? defaultAnimatorData.castSkillClip.name : string.Empty;
                    }
                    // Use override controller as animator
                    if (animator == null)
                        animator = GetComponentInChildren<Animator>();
                    if (animator != null && animator.runtimeAnimatorController != cacheAnimatorController)
                        animator.runtimeAnimatorController = cacheAnimatorController;
                    break;
                case AnimatorType.LegacyAnimtion:
                    if (legacyAnimation == null)
                        legacyAnimation = GetComponentInChildren<Animation>();
                    legacyAnimation.AddClip(legacyAnimationData.idleClip, LEGACY_CLIP_IDLE);
                    legacyAnimation.AddClip(legacyAnimationData.moveClip, LEGACY_CLIP_MOVE);
                    legacyAnimation.AddClip(legacyAnimationData.moveBackwardClip, LEGACY_CLIP_MOVE_BACKWARD);
                    legacyAnimation.AddClip(legacyAnimationData.moveLeftClip, LEGACY_CLIP_MOVE_LEFT);
                    legacyAnimation.AddClip(legacyAnimationData.moveRightClip, LEGACY_CLIP_MOVE_RIGHT);
                    legacyAnimation.AddClip(legacyAnimationData.moveForwardLeftClip, LEGACY_CLIP_MOVE_FORWARD_LEFT);
                    legacyAnimation.AddClip(legacyAnimationData.moveForwardRightClip, LEGACY_CLIP_MOVE_FORWARD_RIGHT);
                    legacyAnimation.AddClip(legacyAnimationData.moveBackwardLeftClip, LEGACY_CLIP_MOVE_BACKWARD_LEFT);
                    legacyAnimation.AddClip(legacyAnimationData.moveBackwardRightClip, LEGACY_CLIP_MOVE_BACKWARD_RIGHT);
                    legacyAnimation.AddClip(legacyAnimationData.jumpClip, LEGACY_CLIP_JUMP);
                    legacyAnimation.AddClip(legacyAnimationData.fallClip, LEGACY_CLIP_FALL);
                    legacyAnimation.AddClip(legacyAnimationData.hurtClip, LEGACY_CLIP_HURT);
                    legacyAnimation.AddClip(legacyAnimationData.deadClip, LEGACY_CLIP_DEAD);
                    CrossFadeLegacyAnimation(LEGACY_CLIP_IDLE, legacyAnimationData.idleClipFadeLength, WrapMode.Loop);
                    break;
            }
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
            if (weaponItem != null)
            {
                WeaponAnimations weaponAnimations;
                if (GetAnims().CacheWeaponAnimations.TryGetValue(weaponItem.WeaponType.DataId, out weaponAnimations))
                {
                    switch (animatorType)
                    {
                        case AnimatorType.Animator:
                            SetupGenericClips_Animator(
                                weaponAnimations.idleClip,
                                weaponAnimations.moveClip,
                                weaponAnimations.moveBackwardClip,
                                weaponAnimations.moveLeftClip,
                                weaponAnimations.moveRightClip,
                                weaponAnimations.moveForwardLeftClip,
                                weaponAnimations.moveForwardRightClip,
                                weaponAnimations.moveBackwardLeftClip,
                                weaponAnimations.moveBackwardRightClip,
                                weaponAnimations.jumpClip,
                                weaponAnimations.fallClip,
                                weaponAnimations.hurtClip,
                                weaponAnimations.deadClip);
                            break;
                        case AnimatorType.LegacyAnimtion:
                            SetupGenericClips_LegacyAnimation(
                                weaponAnimations.idleClip,
                                weaponAnimations.moveClip,
                                weaponAnimations.moveBackwardClip,
                                weaponAnimations.moveLeftClip,
                                weaponAnimations.moveRightClip,
                                weaponAnimations.moveForwardLeftClip,
                                weaponAnimations.moveForwardRightClip,
                                weaponAnimations.moveBackwardLeftClip,
                                weaponAnimations.moveBackwardRightClip,
                                weaponAnimations.jumpClip,
                                weaponAnimations.fallClip,
                                weaponAnimations.hurtClip,
                                weaponAnimations.deadClip);
                            break;
                    }
                }
                else
                {
                    switch (animatorType)
                    {
                        case AnimatorType.Animator:
                            SetupGenericClips_Animator(null, null, null, null, null, null, null, null, null, null, null, null, null);
                            break;
                        case AnimatorType.LegacyAnimtion:
                            SetupGenericClips_LegacyAnimation(null, null, null, null, null, null, null, null, null, null, null, null, null);
                            break;
                    }
                }
            }
        }

        private void SetupGenericClips_Animator(
            AnimationClip idleClip,
            AnimationClip moveClip,
            AnimationClip moveBackwardClip,
            AnimationClip moveLeftClip,
            AnimationClip moveRightClip,
            AnimationClip moveForwardLeftClip,
            AnimationClip moveForwardRightClip,
            AnimationClip moveBackwardLeftClip,
            AnimationClip moveBackwardRightClip,
            AnimationClip jumpClip,
            AnimationClip fallClip,
            AnimationClip hurtClip,
            AnimationClip deadClip)
        {
            if (idleClip == null)
                idleClip = defaultAnimatorData.idleClip;
            if (moveClip == null)
                moveClip = defaultAnimatorData.moveClip;
            if (moveBackwardClip == null)
                moveBackwardClip = defaultAnimatorData.moveBackwardClip;
            if (moveLeftClip == null)
                moveLeftClip = defaultAnimatorData.moveLeftClip;
            if (moveRightClip == null)
                moveRightClip = defaultAnimatorData.moveRightClip;
            if (moveForwardLeftClip == null)
                moveForwardLeftClip = defaultAnimatorData.moveForwardLeftClip;
            if (moveForwardRightClip == null)
                moveForwardRightClip = defaultAnimatorData.moveForwardRightClip;
            if (moveBackwardLeftClip == null)
                moveBackwardLeftClip = defaultAnimatorData.moveBackwardLeftClip;
            if (moveBackwardRightClip == null)
                moveBackwardRightClip = defaultAnimatorData.moveBackwardRightClip;
            if (jumpClip == null)
                jumpClip = defaultAnimatorData.jumpClip;
            if (fallClip == null)
                fallClip = defaultAnimatorData.fallClip;
            if (hurtClip == null)
                hurtClip = defaultAnimatorData.hurtClip;
            if (deadClip == null)
                deadClip = defaultAnimatorData.deadClip;
            // Setup generic clips
            if (!string.IsNullOrEmpty(defaultIdleClipName))
                CacheAnimatorController[defaultIdleClipName] = idleClip;
            if (!string.IsNullOrEmpty(defaultMoveClipName))
                CacheAnimatorController[defaultMoveClipName] = moveClip;
            if (!string.IsNullOrEmpty(defaultMoveBackwardClipName))
                CacheAnimatorController[defaultMoveBackwardClipName] = moveBackwardClip;
            if (!string.IsNullOrEmpty(defaultMoveLeftClipName))
                CacheAnimatorController[defaultMoveLeftClipName] = moveLeftClip;
            if (!string.IsNullOrEmpty(defaultMoveRightClipName))
                CacheAnimatorController[defaultMoveRightClipName] = moveRightClip;
            if (!string.IsNullOrEmpty(defaultMoveForwardLeftClipName))
                CacheAnimatorController[defaultMoveForwardLeftClipName] = moveForwardLeftClip;
            if (!string.IsNullOrEmpty(defaultMoveForwardRightClipName))
                CacheAnimatorController[defaultMoveForwardRightClipName] = moveForwardRightClip;
            if (!string.IsNullOrEmpty(defaultMoveBackwardLeftClipName))
                CacheAnimatorController[defaultMoveBackwardLeftClipName] = moveBackwardLeftClip;
            if (!string.IsNullOrEmpty(defaultMoveBackwardRightClipName))
                CacheAnimatorController[defaultMoveBackwardRightClipName] = moveBackwardRightClip;
            if (!string.IsNullOrEmpty(defaultJumpClipName))
                CacheAnimatorController[defaultJumpClipName] = jumpClip;
            if (!string.IsNullOrEmpty(defaultFallClipName))
                CacheAnimatorController[defaultFallClipName] = fallClip;
            if (!string.IsNullOrEmpty(defaultHurtClipName))
                CacheAnimatorController[defaultHurtClipName] = hurtClip;
            if (!string.IsNullOrEmpty(defaultDeadClipName))
                CacheAnimatorController[defaultDeadClipName] = deadClip;
        }

        private void SetupGenericClips_LegacyAnimation(
            AnimationClip idleClip,
            AnimationClip moveClip,
            AnimationClip moveBackwardClip,
            AnimationClip moveLeftClip,
            AnimationClip moveRightClip,
            AnimationClip moveForwardLeftClip,
            AnimationClip moveForwardRightClip,
            AnimationClip moveBackwardLeftClip,
            AnimationClip moveBackwardRightClip,
            AnimationClip jumpClip,
            AnimationClip fallClip,
            AnimationClip hurtClip,
            AnimationClip deadClip)
        {
            if (idleClip == null)
                idleClip = legacyAnimationData.idleClip;
            if (moveClip == null)
                moveClip = legacyAnimationData.moveClip;
            if (moveBackwardClip == null)
                moveBackwardClip = legacyAnimationData.moveBackwardClip;
            if (moveLeftClip == null)
                moveLeftClip = legacyAnimationData.moveLeftClip;
            if (moveRightClip == null)
                moveRightClip = legacyAnimationData.moveRightClip;
            if (moveForwardLeftClip == null)
                moveForwardLeftClip = legacyAnimationData.moveForwardLeftClip;
            if (moveForwardRightClip == null)
                moveForwardRightClip = legacyAnimationData.moveForwardRightClip;
            if (moveBackwardLeftClip == null)
                moveBackwardLeftClip = legacyAnimationData.moveBackwardLeftClip;
            if (moveBackwardRightClip == null)
                moveBackwardRightClip = legacyAnimationData.moveBackwardRightClip;
            if (jumpClip == null)
                jumpClip = legacyAnimationData.jumpClip;
            if (fallClip == null)
                fallClip = legacyAnimationData.fallClip;
            if (hurtClip == null)
                hurtClip = legacyAnimationData.hurtClip;
            if (deadClip == null)
                deadClip = legacyAnimationData.deadClip;
            // Remove clips
            if (legacyAnimation.GetClip(LEGACY_CLIP_IDLE) != null)
                legacyAnimation.RemoveClip(LEGACY_CLIP_IDLE);
            if (legacyAnimation.GetClip(LEGACY_CLIP_MOVE) != null)
                legacyAnimation.RemoveClip(LEGACY_CLIP_MOVE);
            if (legacyAnimation.GetClip(LEGACY_CLIP_MOVE_BACKWARD) != null)
                legacyAnimation.RemoveClip(LEGACY_CLIP_MOVE_BACKWARD);
            if (legacyAnimation.GetClip(LEGACY_CLIP_MOVE_LEFT) != null)
                legacyAnimation.RemoveClip(LEGACY_CLIP_MOVE_LEFT);
            if (legacyAnimation.GetClip(LEGACY_CLIP_MOVE_RIGHT) != null)
                legacyAnimation.RemoveClip(LEGACY_CLIP_MOVE_RIGHT);
            if (legacyAnimation.GetClip(LEGACY_CLIP_MOVE_FORWARD_LEFT) != null)
                legacyAnimation.RemoveClip(LEGACY_CLIP_MOVE_FORWARD_LEFT);
            if (legacyAnimation.GetClip(LEGACY_CLIP_MOVE_FORWARD_RIGHT) != null)
                legacyAnimation.RemoveClip(LEGACY_CLIP_MOVE_FORWARD_RIGHT);
            if (legacyAnimation.GetClip(LEGACY_CLIP_MOVE_BACKWARD_LEFT) != null)
                legacyAnimation.RemoveClip(LEGACY_CLIP_MOVE_BACKWARD_LEFT);
            if (legacyAnimation.GetClip(LEGACY_CLIP_MOVE_BACKWARD_RIGHT) != null)
                legacyAnimation.RemoveClip(LEGACY_CLIP_MOVE_BACKWARD_RIGHT);
            if (legacyAnimation.GetClip(LEGACY_CLIP_JUMP) != null)
                legacyAnimation.RemoveClip(LEGACY_CLIP_JUMP);
            if (legacyAnimation.GetClip(LEGACY_CLIP_FALL) != null)
                legacyAnimation.RemoveClip(LEGACY_CLIP_FALL);
            if (legacyAnimation.GetClip(LEGACY_CLIP_HURT) != null)
                legacyAnimation.RemoveClip(LEGACY_CLIP_HURT);
            if (legacyAnimation.GetClip(LEGACY_CLIP_DEAD) != null)
                legacyAnimation.RemoveClip(LEGACY_CLIP_DEAD);
            // Setup generic clips
            legacyAnimation.AddClip(idleClip, LEGACY_CLIP_IDLE);
            legacyAnimation.AddClip(moveClip, LEGACY_CLIP_MOVE);
            legacyAnimation.AddClip(moveBackwardClip, LEGACY_CLIP_MOVE_BACKWARD);
            legacyAnimation.AddClip(moveLeftClip, LEGACY_CLIP_MOVE_LEFT);
            legacyAnimation.AddClip(moveRightClip, LEGACY_CLIP_MOVE_RIGHT);
            legacyAnimation.AddClip(moveForwardLeftClip, LEGACY_CLIP_MOVE_FORWARD_LEFT);
            legacyAnimation.AddClip(moveForwardRightClip, LEGACY_CLIP_MOVE_FORWARD_RIGHT);
            legacyAnimation.AddClip(moveBackwardLeftClip, LEGACY_CLIP_MOVE_BACKWARD_LEFT);
            legacyAnimation.AddClip(moveBackwardRightClip, LEGACY_CLIP_MOVE_BACKWARD_RIGHT);
            legacyAnimation.AddClip(jumpClip, LEGACY_CLIP_JUMP);
            legacyAnimation.AddClip(fallClip, LEGACY_CLIP_FALL);
            legacyAnimation.AddClip(hurtClip, LEGACY_CLIP_HURT);
            legacyAnimation.AddClip(deadClip, LEGACY_CLIP_DEAD);
            CrossFadeLegacyAnimation(LEGACY_CLIP_IDLE, 0, WrapMode.Loop);
        }

        public override void AddingNewModel(GameObject newModel, EquipmentContainer equipmentContainer)
        {
            base.AddingNewModel(newModel, equipmentContainer);
            SkinnedMeshRenderer skinnedMesh = newModel.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMesh != null && skinnedMeshRenderer != null)
            {
                skinnedMesh.bones = skinnedMeshRenderer.bones;
                skinnedMesh.rootBone = skinnedMeshRenderer.rootBone;
            }
            if (skinnedMesh != null && equipmentContainer.defaultModel != null)
            {
                SkinnedMeshRenderer defaultSkinnedMesh = equipmentContainer.defaultModel.GetComponentInChildren<SkinnedMeshRenderer>();
                if (defaultSkinnedMesh != null)
                {
                    skinnedMesh.bones = defaultSkinnedMesh.bones;
                    skinnedMesh.rootBone = defaultSkinnedMesh.rootBone;
                }
            }
        }

        public override void PlayMoveAnimation()
        {
            switch (animatorType)
            {
                case AnimatorType.Animator:
                    UpdateAnimation_Animator();
                    break;
                case AnimatorType.LegacyAnimtion:
                    UpdateAnimation_LegacyAnimation();
                    break;
            }
        }

        #region Update Animation Functions
        private void UpdateAnimation_Animator()
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

        private void UpdateAnimation_LegacyAnimation()
        {
            if (isDead)
                CrossFadeLegacyAnimation(LEGACY_CLIP_DEAD, legacyAnimationData.deadClipFadeLength, WrapMode.Once);
            else
            {
                if (legacyAnimation.GetClip(LEGACY_CLIP_ACTION) != null && legacyAnimation.IsPlaying(LEGACY_CLIP_ACTION))
                    return;
                if (legacyAnimation.GetClip(LEGACY_CLIP_CAST_SKILL) != null && legacyAnimation.IsPlaying(LEGACY_CLIP_CAST_SKILL))
                    return;
                if (!movementState.HasFlag(MovementState.IsGrounded))
                    CrossFadeLegacyAnimation(LEGACY_CLIP_FALL, legacyAnimationData.fallClipFadeLength, WrapMode.Loop);
                else
                {
                    // Forward Right
                    if (movementState.HasFlag(MovementState.Forward) && movementState.HasFlag(MovementState.Right))
                        CrossFadeLegacyAnimation(LEGACY_CLIP_MOVE_FORWARD_RIGHT, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Forward Left
                    else if (movementState.HasFlag(MovementState.Forward) && movementState.HasFlag(MovementState.Left))
                        CrossFadeLegacyAnimation(LEGACY_CLIP_MOVE_FORWARD_LEFT, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Backward Right
                    else if (movementState.HasFlag(MovementState.Backward) && movementState.HasFlag(MovementState.Right))
                        CrossFadeLegacyAnimation(LEGACY_CLIP_MOVE_BACKWARD_RIGHT, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Backward Left
                    else if (movementState.HasFlag(MovementState.Backward) && movementState.HasFlag(MovementState.Left))
                        CrossFadeLegacyAnimation(LEGACY_CLIP_MOVE_BACKWARD_LEFT, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Forward
                    else if (movementState.HasFlag(MovementState.Forward))
                        CrossFadeLegacyAnimation(LEGACY_CLIP_MOVE, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Backward
                    else if (movementState.HasFlag(MovementState.Backward))
                        CrossFadeLegacyAnimation(LEGACY_CLIP_MOVE_BACKWARD, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Right
                    else if (movementState.HasFlag(MovementState.Right))
                        CrossFadeLegacyAnimation(LEGACY_CLIP_MOVE_RIGHT, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Left
                    else if (movementState.HasFlag(MovementState.Left))
                        CrossFadeLegacyAnimation(LEGACY_CLIP_MOVE_LEFT, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Idle
                    else
                        CrossFadeLegacyAnimation(LEGACY_CLIP_IDLE, legacyAnimationData.idleClipFadeLength, WrapMode.Loop);
                }
            }
        }

        private void CrossFadeLegacyAnimation(string clipName, float fadeLength, WrapMode wrapMode)
        {
            if (!legacyAnimation.IsPlaying(clipName))
            {
                // Don't play dead animation looply
                if (clipName == LEGACY_CLIP_DEAD && lastFadedLegacyClipName == LEGACY_CLIP_DEAD)
                    return;
                lastFadedLegacyClipName = clipName;
                legacyAnimation.wrapMode = wrapMode;
                legacyAnimation.CrossFade(clipName, fadeLength);
            }
        }
        #endregion

        public override Coroutine PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1f)
        {
            if (animatorType == AnimatorType.LegacyAnimtion)
                return StartCoroutine(PlayActionAnimation_LegacyAnimation(animActionType, dataId, index, playSpeedMultiplier));
            return StartCoroutine(PlayActionAnimation_Animator(animActionType, dataId, index, playSpeedMultiplier));
        }

        public override Coroutine PlaySkillCastClip(int dataId, float duration)
        {
            if (animatorType == AnimatorType.LegacyAnimtion)
                return StartCoroutine(PlaySkillCastClip_LegacyAnimation(dataId, duration));
            return StartCoroutine(PlaySkillCastClip_Animator(dataId, duration));
        }

        public override void StopActionAnimation()
        {
            if (animatorType == AnimatorType.LegacyAnimtion)
            {
                CrossFadeLegacyAnimation(LEGACY_CLIP_IDLE, legacyAnimationData.idleClipFadeLength, WrapMode.Loop);
                isPlayingActionAnimation = false;
                return;
            }
            animator.SetBool(ANIM_DO_ACTION, false);
        }

        public override void StopSkillCastAnimation()
        {
            if (animatorType == AnimatorType.LegacyAnimtion)
            {
                CrossFadeLegacyAnimation(LEGACY_CLIP_IDLE, legacyAnimationData.idleClipFadeLength, WrapMode.Loop);
                return;
            }
            animator.SetBool(ANIM_IS_CASTING_SKILL, false);
        }

        #region Action Animation Functions
        private IEnumerator PlayActionAnimation_Animator(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier)
        {
            // If animator is not null, play the action animation
            ActionAnimation tempActionAnimation = GetActionAnimation(animActionType, dataId, index);
            if (tempActionAnimation.clip != null)
            {
                CacheAnimatorController[defaultActionClipName] = tempActionAnimation.clip;
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

        private IEnumerator PlayActionAnimation_LegacyAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier)
        {
            // If animator is not null, play the action animation
            ActionAnimation tempActionAnimation = GetActionAnimation(animActionType, dataId, index);
            if (tempActionAnimation.clip != null)
            {
                if (legacyAnimation.GetClip(LEGACY_CLIP_ACTION) != null)
                    legacyAnimation.RemoveClip(LEGACY_CLIP_ACTION);
                legacyAnimation.AddClip(tempActionAnimation.clip, LEGACY_CLIP_ACTION);
            }
            AudioClip audioClip = tempActionAnimation.GetRandomAudioClip();
            if (audioClip != null)
                AudioSource.PlayClipAtPoint(audioClip, CacheTransform.position, AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level);
            isPlayingActionAnimation = true;
            if (tempActionAnimation.clip != null)
                CrossFadeLegacyAnimation(LEGACY_CLIP_ACTION, legacyAnimationData.actionClipFadeLength, WrapMode.Once);
            // Waits by current transition + clip duration before end animation
            yield return new WaitForSecondsRealtime(tempActionAnimation.GetClipLength() / playSpeedMultiplier);
            if (tempActionAnimation.clip != null)
                CrossFadeLegacyAnimation(LEGACY_CLIP_IDLE, legacyAnimationData.idleClipFadeLength, WrapMode.Loop);
            // Waits by current transition + extra duration before end playing animation state
            yield return new WaitForSecondsRealtime(tempActionAnimation.GetExtraDuration() / playSpeedMultiplier);
            isPlayingActionAnimation = false;
        }

        private IEnumerator PlaySkillCastClip_Animator(int dataId, float duration)
        {
            AnimationClip castClip = GetSkillCastClip(dataId);
            CacheAnimatorController[defaultCastSkillClipName] = castClip;
            yield return 0;
            animator.SetBool(ANIM_IS_CASTING_SKILL, true);
            animator.Play(0, castSkillStateLayer, 0f);
            yield return new WaitForSecondsRealtime(duration);
            animator.SetBool(ANIM_IS_CASTING_SKILL, false);
        }

        private IEnumerator PlaySkillCastClip_LegacyAnimation(int dataId, float duration)
        {
            AnimationClip castClip = GetSkillCastClip(dataId);
            if (legacyAnimation.GetClip(LEGACY_CLIP_CAST_SKILL) != null)
                legacyAnimation.RemoveClip(LEGACY_CLIP_CAST_SKILL);
            legacyAnimation.AddClip(castClip, LEGACY_CLIP_CAST_SKILL);
            CrossFadeLegacyAnimation(LEGACY_CLIP_CAST_SKILL, legacyAnimationData.actionClipFadeLength, WrapMode.Loop);
            yield return new WaitForSecondsRealtime(duration);
            if (!isPlayingActionAnimation)
                CrossFadeLegacyAnimation(LEGACY_CLIP_IDLE, legacyAnimationData.idleClipFadeLength, WrapMode.Loop);
        }
        #endregion

        public override void PlayHitAnimation()
        {
            if (animatorType == AnimatorType.LegacyAnimtion)
            {
                CrossFadeLegacyAnimation(LEGACY_CLIP_HURT, legacyAnimationData.hurtClipFadeLength, WrapMode.Once);
                return;
            }
            animator.ResetTrigger(ANIM_HURT);
            animator.SetTrigger(ANIM_HURT);
        }

        public override void PlayJumpAnimation()
        {
            if (animatorType == AnimatorType.LegacyAnimtion)
            {
                CrossFadeLegacyAnimation(LEGACY_CLIP_JUMP, legacyAnimationData.jumpClipFadeLength, WrapMode.Once);
                return;
            }
            animator.ResetTrigger(ANIM_JUMP);
            animator.SetTrigger(ANIM_JUMP);
        }

        #region Animation data helpers
        public ActionAnimation GetActionAnimation(AnimActionType animActionType, int dataId, int index)
        {
            ActionAnimation tempActionAnimation = default(ActionAnimation);
            switch (animActionType)
            {
                case AnimActionType.AttackRightHand:
                    tempActionAnimation = GetRightHandAttackAnimations(dataId)[index];
                    break;
                case AnimActionType.AttackLeftHand:
                    tempActionAnimation = GetLeftHandAttackAnimations(dataId)[index];
                    break;
                case AnimActionType.SkillRightHand:
                case AnimActionType.SkillLeftHand:
                    tempActionAnimation = GetSkillActivateAnimation(dataId);
                    break;
                case AnimActionType.ReloadRightHand:
                    tempActionAnimation = GetRightHandReloadAnimation(dataId);
                    break;
                case AnimActionType.ReloadLeftHand:
                    tempActionAnimation = GetLeftHandReloadAnimation(dataId);
                    break;
            }
            return tempActionAnimation;
        }

        public ActionAnimation[] GetRightHandAttackAnimations(WeaponType weaponType)
        {
            return GetRightHandAttackAnimations(weaponType.DataId);
        }

        public ActionAnimation[] GetRightHandAttackAnimations(int dataId)
        {
            if (GetAnims().CacheWeaponAnimations.ContainsKey(dataId) &&
                GetAnims().CacheWeaponAnimations[dataId].rightHandAttackAnimations != null)
                return GetAnims().CacheWeaponAnimations[dataId].rightHandAttackAnimations;
            return defaultAttackAnimations;
        }

        public ActionAnimation[] GetLeftHandAttackAnimations(WeaponType weaponType)
        {
            return GetLeftHandAttackAnimations(weaponType.DataId);
        }

        public ActionAnimation[] GetLeftHandAttackAnimations(int dataId)
        {
            if (GetAnims().CacheWeaponAnimations.ContainsKey(dataId) &&
                GetAnims().CacheWeaponAnimations[dataId].leftHandAttackAnimations != null)
                return GetAnims().CacheWeaponAnimations[dataId].leftHandAttackAnimations;
            return defaultAttackAnimations;
        }

        public AnimationClip GetSkillCastClip(int dataId)
        {
            if (GetAnims().CacheSkillAnimations.ContainsKey(dataId) &&
                GetAnims().CacheSkillAnimations[dataId].castClip != null)
                return GetAnims().CacheSkillAnimations[dataId].castClip;
            return defaultSkillCastClip;
        }

        public ActionAnimation GetSkillActivateAnimation(int dataId)
        {
            if (GetAnims().CacheSkillAnimations.ContainsKey(dataId) &&
                GetAnims().CacheSkillAnimations[dataId].activateAnimation.clip != null)
                return GetAnims().CacheSkillAnimations[dataId].activateAnimation;
            return defaultSkillActivateAnimation;
        }

        public ActionAnimation GetRightHandReloadAnimation(int dataId)
        {
            if (GetAnims().CacheWeaponAnimations.ContainsKey(dataId) &&
                 GetAnims().CacheWeaponAnimations[dataId].rightHandReloadAnimation.clip != null)
                return GetAnims().CacheWeaponAnimations[dataId].rightHandReloadAnimation;
            return defaultReloadAnimation;
        }

        public ActionAnimation GetLeftHandReloadAnimation(int dataId)
        {
            if (GetAnims().CacheWeaponAnimations.ContainsKey(dataId) &&
                GetAnims().CacheWeaponAnimations[dataId].leftHandReloadAnimation.clip != null)
                return GetAnims().CacheWeaponAnimations[dataId].leftHandReloadAnimation;
            return defaultReloadAnimation;
        }

        public override bool GetRandomRightHandAttackAnimation(
            int dataId,
            out int animationIndex,
            out float[] triggerDurations,
            out float totalDuration)
        {
            animationIndex = Random.Range(0, GetRightHandAttackAnimations(dataId).Length);
            return GetRightHandAttackAnimation(dataId, animationIndex, out triggerDurations, out totalDuration);
        }

        public override bool GetRandomLeftHandAttackAnimation(
            int dataId,
            out int animationIndex,
            out float[] triggerDurations,
            out float totalDuration)
        {
            animationIndex = Random.Range(0, GetLeftHandAttackAnimations(dataId).Length);
            return GetLeftHandAttackAnimation(dataId, animationIndex, out triggerDurations, out totalDuration);
        }

        public override bool GetRightHandAttackAnimation(int dataId, int animationIndex, out float[] triggerDurations, out float totalDuration)
        {
            ActionAnimation[] tempActionAnimations = GetRightHandAttackAnimations(dataId);
            triggerDurations = new float[] { 0f };
            totalDuration = 0f;
            if (tempActionAnimations.Length == 0 || animationIndex >= tempActionAnimations.Length) return false;
            animationIndex = Random.Range(0, tempActionAnimations.Length);
            triggerDurations = tempActionAnimations[animationIndex].GetTriggerDurations();
            totalDuration = tempActionAnimations[animationIndex].GetTotalDuration();
            return true;
        }

        public override bool GetLeftHandAttackAnimation(int dataId, int animationIndex, out float[] triggerDurations, out float totalDuration)
        {
            ActionAnimation[] tempActionAnimations = GetLeftHandAttackAnimations(dataId);
            triggerDurations = new float[] { 0f };
            totalDuration = 0f;
            if (tempActionAnimations.Length == 0 || animationIndex >= tempActionAnimations.Length) return false;
            animationIndex = Random.Range(0, tempActionAnimations.Length);
            triggerDurations = tempActionAnimations[animationIndex].GetTriggerDurations();
            totalDuration = tempActionAnimations[animationIndex].GetTotalDuration();
            return true;
        }

        public override bool GetSkillActivateAnimation(
            int dataId,
            out float[] triggerDurations,
            out float totalDuration)
        {
            ActionAnimation tempActionAnimation = GetSkillActivateAnimation(dataId);
            triggerDurations = tempActionAnimation.GetTriggerDurations();
            totalDuration = tempActionAnimation.GetTotalDuration();
            return true;
        }

        public override bool GetRightHandReloadAnimation(
            int dataId,
            out float[] triggerDurations,
            out float totalDuration)
        {
            ActionAnimation tempActionAnimation = GetRightHandReloadAnimation(dataId);
            triggerDurations = tempActionAnimation.GetTriggerDurations();
            totalDuration = tempActionAnimation.GetTotalDuration();
            return true;
        }

        public override bool GetLeftHandReloadAnimation(
            int dataId,
            out float[] triggerDurations,
            out float totalDuration)
        {
            ActionAnimation tempActionAnimation = GetLeftHandReloadAnimation(dataId);
            triggerDurations = tempActionAnimation.GetTriggerDurations();
            totalDuration = tempActionAnimation.GetTotalDuration();
            return true;
        }

        public override SkillActivateAnimationType UseSkillActivateAnimationType(int dataId)
        {
            if (!GetAnims().CacheSkillAnimations.ContainsKey(dataId))
                return SkillActivateAnimationType.UseActivateAnimation;
            return GetAnims().CacheSkillAnimations[dataId].activateAnimationType;
        }

        protected override WeaponAnimations[] GetWeaponAnims()
        {
            return weaponAnimations;
        }

        protected override SkillAnimations[] GetSkillAnims()
        {
            return skillAnimations;
        }

#if UNITY_EDITOR
        [ContextMenu("Copy Weapon Animations")]
        public void CopyWeaponAnimations()
        {
            CharacterModelDataManager.CopyWeaponAnimations(weaponAnimations);
        }

        [ContextMenu("Paste Weapon Animations")]
        public void PasteWeaponAnimations()
        {
            WeaponAnimations[] weaponAnimations = CharacterModelDataManager.PasteWeaponAnimations();
            if (weaponAnimations != null)
            {
                this.weaponAnimations = weaponAnimations;
                EditorUtility.SetDirty(this);
            }
        }

        [ContextMenu("Copy Skill Animations")]
        public void CopySkillAnimations()
        {
            CharacterModelDataManager.CopySkillAnimations(skillAnimations);
        }

        [ContextMenu("Paste Skill Animations")]
        public void PasteSkillAnimations()
        {
            SkillAnimations[] skillAnimations = CharacterModelDataManager.PasteSkillAnimations();
            if (skillAnimations != null)
            {
                this.skillAnimations = skillAnimations;
                EditorUtility.SetDirty(this);
            }
        }
#endif
        #endregion
    }

    [System.Serializable]
    public struct LegacyAnimationData
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
        public float actionClipFadeLength;
        public float idleClipFadeLength;
        public float moveClipFadeLength;
        public float jumpClipFadeLength;
        public float fallClipFadeLength;
        public float hurtClipFadeLength;
        public float deadClipFadeLength;
    }

    [System.Serializable]
    public struct DefaultAnimatorData
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
        public AnimationClip actionClip;
        public AnimationClip castSkillClip;
    }
}
