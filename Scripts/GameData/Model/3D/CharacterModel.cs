using System.Collections;
using LiteNetLibManager;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [System.Obsolete("`Character Model` is deprecate and stopped development, use context menu to convert to newer character model")]
    public partial class CharacterModel : BaseCharacterModel
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
        public const string CLIP_IDLE = "__Idle";
        public const string CLIP_MOVE = "__MoveForward";
        public const string CLIP_MOVE_BACKWARD = "__MoveBackward";
        public const string CLIP_MOVE_LEFT = "__MoveLeft";
        public const string CLIP_MOVE_RIGHT = "__MoveRight";
        public const string CLIP_MOVE_FORWARD_LEFT = "__MoveForwardLeft";
        public const string CLIP_MOVE_FORWARD_RIGHT = "__MoveForwardRight";
        public const string CLIP_MOVE_BACKWARD_LEFT = "__MoveBackwardLeft";
        public const string CLIP_MOVE_BACKWARD_RIGHT = "__MoveBackwardRight";
        public const string CLIP_JUMP = "__Jump";
        public const string CLIP_FALL = "__Fall";
        public const string CLIP_HURT = "__Hurt";
        public const string CLIP_DEAD = "__Dead";
        public const string CLIP_ACTION = "__Action";
        public const string CLIP_CAST_SKILL = "__CastSkill";

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
        [ArrayElementTitle("clip")]
        public ActionAnimation[] defaultAttackAnimations;
        public AnimationClip defaultSkillCastClip;
        public ActionAnimation defaultSkillActivateAnimation;
        public ActionAnimation defaultReloadAnimation;
        [ArrayElementTitle("weaponType")]
        public WeaponAnimations[] weaponAnimations;
        [ArrayElementTitle("skill")]
        public SkillAnimations[] skillAnimations;

#if UNITY_EDITOR
        [Header("Animation Test Tool")]
        public AnimActionType testAnimActionType;
        public WeaponType testWeaponType;
        public BaseSkill testSkill;
        public int testAttackAnimIndex;
        [InspectorButton(nameof(SetAnimatorClipsForTest))]
        public bool setAnimatorClipsForTest;
#endif

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

        public AnimatorOverrideController CacheAnimatorController
        {
            get; private set;
        }

        protected override void Awake()
        {
            base.Awake();
            SetupComponent();
        }

        public bool TryGetWeaponAnimations(int dataId, out WeaponAnimations anims)
        {
            return CacheAnimationsManager.SetAndTryGetCacheWeaponAnimations(Id, weaponAnimations, skillAnimations, dataId, out anims);
        }

        public bool TryGetSkillAnimations(int dataId, out SkillAnimations anims)
        {
            return CacheAnimationsManager.SetAndTryGetCacheSkillAnimations(Id, weaponAnimations, skillAnimations, dataId, out anims);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
#if UNITY_EDITOR
            bool hasChanges = false;
            switch (animatorType)
            {
                case AnimatorType.Animator:
                    if (animator == null)
                    {
                        animator = GetComponentInChildren<Animator>();
                        if (animator != null)
                            hasChanges = true;
                    }
                    if (animator == null)
                        Logging.LogError(ToString(), "`Animator` is empty");
                    if (animatorController == null)
                        Logging.LogError(ToString(), "`Animator Controller` is empty");
                    break;
                case AnimatorType.LegacyAnimtion:
                    if (legacyAnimation == null)
                    {
                        legacyAnimation = GetComponentInChildren<Animation>();
                        if (legacyAnimation != null)
                            hasChanges = true;
                    }
                    if (legacyAnimation == null)
                        Logging.LogError(ToString(), "`Legacy Animation` is empty");
                    break;
            }
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
            switch (animatorType)
            {
                case AnimatorType.Animator:
                    if (CacheAnimatorController == null)
                    {
                        CacheAnimatorController = new AnimatorOverrideController(animatorController);
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
                        // Setup generic clips
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
                    if (animator != null && animator.runtimeAnimatorController != CacheAnimatorController)
                        animator.runtimeAnimatorController = CacheAnimatorController;
                    break;
                case AnimatorType.LegacyAnimtion:
                    legacyAnimation.AddClip(legacyAnimationData.idleClip, CLIP_IDLE);
                    legacyAnimation.AddClip(legacyAnimationData.moveClip, CLIP_MOVE);
                    legacyAnimation.AddClip(legacyAnimationData.moveBackwardClip, CLIP_MOVE_BACKWARD);
                    legacyAnimation.AddClip(legacyAnimationData.moveLeftClip, CLIP_MOVE_LEFT);
                    legacyAnimation.AddClip(legacyAnimationData.moveRightClip, CLIP_MOVE_RIGHT);
                    legacyAnimation.AddClip(legacyAnimationData.moveForwardLeftClip, CLIP_MOVE_FORWARD_LEFT);
                    legacyAnimation.AddClip(legacyAnimationData.moveForwardRightClip, CLIP_MOVE_FORWARD_RIGHT);
                    legacyAnimation.AddClip(legacyAnimationData.moveBackwardLeftClip, CLIP_MOVE_BACKWARD_LEFT);
                    legacyAnimation.AddClip(legacyAnimationData.moveBackwardRightClip, CLIP_MOVE_BACKWARD_RIGHT);
                    legacyAnimation.AddClip(legacyAnimationData.jumpClip, CLIP_JUMP);
                    legacyAnimation.AddClip(legacyAnimationData.fallClip, CLIP_FALL);
                    legacyAnimation.AddClip(legacyAnimationData.hurtClip, CLIP_HURT);
                    legacyAnimation.AddClip(legacyAnimationData.deadClip, CLIP_DEAD);
                    CrossFadeLegacyAnimation(CLIP_IDLE, legacyAnimationData.idleClipFadeLength, WrapMode.Loop);
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
            if (GameInstance.Singleton == null)
                return;

            IWeaponItem weaponItem = equipWeapons.GetRightHandWeaponItem();
            if (weaponItem == null)
                weaponItem = equipWeapons.GetLeftHandWeaponItem();
            if (weaponItem == null)
                weaponItem = GameInstance.Singleton.DefaultWeaponItem;

            SetClipBasedOnWeaponType(weaponItem.WeaponType);
        }

        protected void SetClipBasedOnWeaponType(WeaponType weaponType)
        {
            WeaponAnimations weaponAnimations;
            if (TryGetWeaponAnimations(weaponType.DataId, out weaponAnimations))
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
            if (legacyAnimation.GetClip(CLIP_IDLE) != null)
                legacyAnimation.RemoveClip(CLIP_IDLE);
            if (legacyAnimation.GetClip(CLIP_MOVE) != null)
                legacyAnimation.RemoveClip(CLIP_MOVE);
            if (legacyAnimation.GetClip(CLIP_MOVE_BACKWARD) != null)
                legacyAnimation.RemoveClip(CLIP_MOVE_BACKWARD);
            if (legacyAnimation.GetClip(CLIP_MOVE_LEFT) != null)
                legacyAnimation.RemoveClip(CLIP_MOVE_LEFT);
            if (legacyAnimation.GetClip(CLIP_MOVE_RIGHT) != null)
                legacyAnimation.RemoveClip(CLIP_MOVE_RIGHT);
            if (legacyAnimation.GetClip(CLIP_MOVE_FORWARD_LEFT) != null)
                legacyAnimation.RemoveClip(CLIP_MOVE_FORWARD_LEFT);
            if (legacyAnimation.GetClip(CLIP_MOVE_FORWARD_RIGHT) != null)
                legacyAnimation.RemoveClip(CLIP_MOVE_FORWARD_RIGHT);
            if (legacyAnimation.GetClip(CLIP_MOVE_BACKWARD_LEFT) != null)
                legacyAnimation.RemoveClip(CLIP_MOVE_BACKWARD_LEFT);
            if (legacyAnimation.GetClip(CLIP_MOVE_BACKWARD_RIGHT) != null)
                legacyAnimation.RemoveClip(CLIP_MOVE_BACKWARD_RIGHT);
            if (legacyAnimation.GetClip(CLIP_JUMP) != null)
                legacyAnimation.RemoveClip(CLIP_JUMP);
            if (legacyAnimation.GetClip(CLIP_FALL) != null)
                legacyAnimation.RemoveClip(CLIP_FALL);
            if (legacyAnimation.GetClip(CLIP_HURT) != null)
                legacyAnimation.RemoveClip(CLIP_HURT);
            if (legacyAnimation.GetClip(CLIP_DEAD) != null)
                legacyAnimation.RemoveClip(CLIP_DEAD);
            // Setup generic clips
            legacyAnimation.AddClip(idleClip, CLIP_IDLE);
            legacyAnimation.AddClip(moveClip, CLIP_MOVE);
            legacyAnimation.AddClip(moveBackwardClip, CLIP_MOVE_BACKWARD);
            legacyAnimation.AddClip(moveLeftClip, CLIP_MOVE_LEFT);
            legacyAnimation.AddClip(moveRightClip, CLIP_MOVE_RIGHT);
            legacyAnimation.AddClip(moveForwardLeftClip, CLIP_MOVE_FORWARD_LEFT);
            legacyAnimation.AddClip(moveForwardRightClip, CLIP_MOVE_FORWARD_RIGHT);
            legacyAnimation.AddClip(moveBackwardLeftClip, CLIP_MOVE_BACKWARD_LEFT);
            legacyAnimation.AddClip(moveBackwardRightClip, CLIP_MOVE_BACKWARD_RIGHT);
            legacyAnimation.AddClip(jumpClip, CLIP_JUMP);
            legacyAnimation.AddClip(fallClip, CLIP_FALL);
            legacyAnimation.AddClip(hurtClip, CLIP_HURT);
            legacyAnimation.AddClip(deadClip, CLIP_DEAD);
            CrossFadeLegacyAnimation(CLIP_IDLE, 0, WrapMode.Loop);
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
                CrossFadeLegacyAnimation(CLIP_DEAD, legacyAnimationData.deadClipFadeLength, WrapMode.Once);
            else
            {
                if (legacyAnimation.GetClip(CLIP_ACTION) != null && legacyAnimation.IsPlaying(CLIP_ACTION))
                    return;
                if (legacyAnimation.GetClip(CLIP_CAST_SKILL) != null && legacyAnimation.IsPlaying(CLIP_CAST_SKILL))
                    return;
                if (!movementState.HasFlag(MovementState.IsGrounded))
                    CrossFadeLegacyAnimation(CLIP_FALL, legacyAnimationData.fallClipFadeLength, WrapMode.Loop);
                else
                {
                    // Forward Right
                    if (movementState.HasFlag(MovementState.Forward) && movementState.HasFlag(MovementState.Right))
                        CrossFadeLegacyAnimation(CLIP_MOVE_FORWARD_RIGHT, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Forward Left
                    else if (movementState.HasFlag(MovementState.Forward) && movementState.HasFlag(MovementState.Left))
                        CrossFadeLegacyAnimation(CLIP_MOVE_FORWARD_LEFT, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Backward Right
                    else if (movementState.HasFlag(MovementState.Backward) && movementState.HasFlag(MovementState.Right))
                        CrossFadeLegacyAnimation(CLIP_MOVE_BACKWARD_RIGHT, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Backward Left
                    else if (movementState.HasFlag(MovementState.Backward) && movementState.HasFlag(MovementState.Left))
                        CrossFadeLegacyAnimation(CLIP_MOVE_BACKWARD_LEFT, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Forward
                    else if (movementState.HasFlag(MovementState.Forward))
                        CrossFadeLegacyAnimation(CLIP_MOVE, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Backward
                    else if (movementState.HasFlag(MovementState.Backward))
                        CrossFadeLegacyAnimation(CLIP_MOVE_BACKWARD, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Right
                    else if (movementState.HasFlag(MovementState.Right))
                        CrossFadeLegacyAnimation(CLIP_MOVE_RIGHT, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Left
                    else if (movementState.HasFlag(MovementState.Left))
                        CrossFadeLegacyAnimation(CLIP_MOVE_LEFT, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Idle
                    else
                        CrossFadeLegacyAnimation(CLIP_IDLE, legacyAnimationData.idleClipFadeLength, WrapMode.Loop);
                }
            }
        }

        private void CrossFadeLegacyAnimation(string clipName, float fadeLength, WrapMode wrapMode)
        {
            if (!legacyAnimation.IsPlaying(clipName))
            {
                // Don't play dead animation looply
                if (clipName == CLIP_DEAD && lastFadedLegacyClipName == CLIP_DEAD)
                    return;
                lastFadedLegacyClipName = clipName;
                legacyAnimation.wrapMode = wrapMode;
                legacyAnimation.CrossFade(clipName, fadeLength);
            }
        }
        #endregion

        public override void PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1f)
        {
            if (animatorType == AnimatorType.LegacyAnimtion)
                StartCoroutine(PlayActionAnimation_LegacyAnimation(animActionType, dataId, index, playSpeedMultiplier));
            else
                StartCoroutine(PlayActionAnimation_Animator(animActionType, dataId, index, playSpeedMultiplier));
        }

        public override void PlaySkillCastClip(int dataId, float duration)
        {
            if (animatorType == AnimatorType.LegacyAnimtion)
                StartCoroutine(PlaySkillCastClip_LegacyAnimation(dataId, duration));
            else
                StartCoroutine(PlaySkillCastClip_Animator(dataId, duration));
        }

        public override void PlayWeaponChargeClip(int dataId, bool isLeftHand)
        {
            // This character model not supports weapon pulling clip, use `AnimatorCharacterModel` or `AnimationCharacterModel` instead.
        }

        public override void StopActionAnimation()
        {
            if (animatorType == AnimatorType.LegacyAnimtion)
            {
                CrossFadeLegacyAnimation(CLIP_IDLE, legacyAnimationData.idleClipFadeLength, WrapMode.Loop);
                isPlayingActionAnimation = false;
                return;
            }
            animator.SetBool(ANIM_DO_ACTION, false);
        }

        public override void StopSkillCastAnimation()
        {
            if (animatorType == AnimatorType.LegacyAnimtion)
            {
                CrossFadeLegacyAnimation(CLIP_IDLE, legacyAnimationData.idleClipFadeLength, WrapMode.Loop);
                return;
            }
            animator.SetBool(ANIM_IS_CASTING_SKILL, false);
        }

        public override void StopWeaponChargeAnimation()
        {
            // This character model not supports weapon pulling clip, use `AnimatorCharacterModel` or `AnimationCharacterModel` instead.
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
            AudioManager.PlaySfxClipAtAudioSource(tempActionAnimation.GetRandomAudioClip(), genericAudioSource);
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
            yield return new WaitForSecondsRealtime(tempActionAnimation.GetExtendDuration() / playSpeedMultiplier);
        }

        private IEnumerator PlayActionAnimation_LegacyAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier)
        {
            // If animator is not null, play the action animation
            ActionAnimation tempActionAnimation = GetActionAnimation(animActionType, dataId, index);
            if (tempActionAnimation.clip != null)
            {
                if (legacyAnimation.GetClip(CLIP_ACTION) != null)
                    legacyAnimation.RemoveClip(CLIP_ACTION);
                legacyAnimation.AddClip(tempActionAnimation.clip, CLIP_ACTION);
            }
            AudioManager.PlaySfxClipAtPoint(tempActionAnimation.GetRandomAudioClip(), CacheTransform.position);
            isPlayingActionAnimation = true;
            if (tempActionAnimation.clip != null)
                CrossFadeLegacyAnimation(CLIP_ACTION, legacyAnimationData.actionClipFadeLength, WrapMode.Once);
            // Waits by current transition + clip duration before end animation
            yield return new WaitForSecondsRealtime(tempActionAnimation.GetClipLength() / playSpeedMultiplier);
            if (tempActionAnimation.clip != null)
                CrossFadeLegacyAnimation(CLIP_IDLE, legacyAnimationData.idleClipFadeLength, WrapMode.Loop);
            // Waits by current transition + extra duration before end playing animation state
            yield return new WaitForSecondsRealtime(tempActionAnimation.GetExtendDuration() / playSpeedMultiplier);
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
            if (legacyAnimation.GetClip(CLIP_CAST_SKILL) != null)
                legacyAnimation.RemoveClip(CLIP_CAST_SKILL);
            legacyAnimation.AddClip(castClip, CLIP_CAST_SKILL);
            CrossFadeLegacyAnimation(CLIP_CAST_SKILL, legacyAnimationData.actionClipFadeLength, WrapMode.Loop);
            yield return new WaitForSecondsRealtime(duration);
            if (!isPlayingActionAnimation)
                CrossFadeLegacyAnimation(CLIP_IDLE, legacyAnimationData.idleClipFadeLength, WrapMode.Loop);
        }
        #endregion

        public override void PlayHitAnimation()
        {
            if (animatorType == AnimatorType.LegacyAnimtion)
            {
                CrossFadeLegacyAnimation(CLIP_HURT, legacyAnimationData.hurtClipFadeLength, WrapMode.Once);
                return;
            }
            StartCoroutine(PlayHitAnimationRoutine());
        }

        IEnumerator PlayHitAnimationRoutine()
        {
            yield return null;
            animator.ResetTrigger(ANIM_HURT);
            animator.SetTrigger(ANIM_HURT);
        }

        public override float GetJumpAnimationDuration()
        {
            if (animatorType == AnimatorType.LegacyAnimtion)
            {
                if (legacyAnimation.GetClip(CLIP_JUMP) == null)
                    return 0f;
                return legacyAnimation.GetClip(CLIP_JUMP).length;
            }
            if (CacheAnimatorController[CLIP_JUMP] == null)
                return 0f;
            return CacheAnimatorController[CLIP_JUMP].length;
        }

        public override void PlayJumpAnimation()
        {
            if (animatorType == AnimatorType.LegacyAnimtion)
            {
                CrossFadeLegacyAnimation(CLIP_JUMP, legacyAnimationData.jumpClipFadeLength, WrapMode.Once);
                return;
            }
            StartCoroutine(PlayJumpAnimationRoutine());
        }

        IEnumerator PlayJumpAnimationRoutine()
        {
            yield return null;
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
            WeaponAnimations anims;
            if (TryGetWeaponAnimations(dataId, out anims) && anims.rightHandAttackAnimations != null)
                return anims.rightHandAttackAnimations;
            return defaultAttackAnimations;
        }

        public ActionAnimation[] GetLeftHandAttackAnimations(WeaponType weaponType)
        {
            return GetLeftHandAttackAnimations(weaponType.DataId);
        }

        public ActionAnimation[] GetLeftHandAttackAnimations(int dataId)
        {
            WeaponAnimations anims;
            if (TryGetWeaponAnimations(dataId, out anims) && anims.leftHandAttackAnimations != null)
                return anims.leftHandAttackAnimations;
            return defaultAttackAnimations;
        }

        public AnimationClip GetSkillCastClip(int dataId)
        {
            SkillAnimations anims;
            if (TryGetSkillAnimations(dataId, out anims) && anims.castClip != null)
                return anims.castClip;
            return defaultSkillCastClip;
        }

        public ActionAnimation GetSkillActivateAnimation(int dataId)
        {
            SkillAnimations anims;
            if (TryGetSkillAnimations(dataId, out anims) && anims.activateAnimation.clip != null)
                return anims.activateAnimation;
            return defaultSkillActivateAnimation;
        }

        public ActionAnimation GetRightHandReloadAnimation(int dataId)
        {
            WeaponAnimations anims;
            if (TryGetWeaponAnimations(dataId, out anims) && anims.rightHandReloadAnimation.clip != null)
                return anims.rightHandReloadAnimation;
            return defaultReloadAnimation;
        }

        public ActionAnimation GetLeftHandReloadAnimation(int dataId)
        {
            WeaponAnimations anims;
            if (TryGetWeaponAnimations(dataId, out anims) && anims.leftHandReloadAnimation.clip != null)
                return anims.leftHandReloadAnimation;
            return defaultReloadAnimation;
        }

        public override bool GetRandomRightHandAttackAnimation(
            int dataId,
            int randomSeed,
            out int animationIndex,
            out float animSpeedRate,
            out float[] triggerDurations,
            out float totalDuration)
        {
            animationIndex = GenericUtils.RandomInt(randomSeed, 0, GetRightHandAttackAnimations(dataId).Length);
            return GetRightHandAttackAnimation(dataId, animationIndex, out animSpeedRate, out triggerDurations, out totalDuration);
        }

        public override bool GetRandomLeftHandAttackAnimation(
            int dataId,
            int randomSeed,
            out int animationIndex,
            out float animSpeedRate,
            out float[] triggerDurations,
            out float totalDuration)
        {
            animationIndex = GenericUtils.RandomInt(randomSeed, 0, GetLeftHandAttackAnimations(dataId).Length);
            return GetLeftHandAttackAnimation(dataId, animationIndex, out animSpeedRate, out triggerDurations, out totalDuration);
        }

        public override bool GetRightHandAttackAnimation(
            int dataId,
            int animationIndex,
            out float animSpeedRate,
            out float[] triggerDurations,
            out float totalDuration)
        {
            ActionAnimation[] tempActionAnimations = GetRightHandAttackAnimations(dataId);
            animSpeedRate = 1f;
            triggerDurations = new float[] { 0f };
            totalDuration = 0f;
            if (tempActionAnimations.Length == 0 || animationIndex >= tempActionAnimations.Length) return false;
            animationIndex = Random.Range(0, tempActionAnimations.Length);
            animSpeedRate = tempActionAnimations[animationIndex].GetAnimSpeedRate();
            triggerDurations = tempActionAnimations[animationIndex].GetTriggerDurations();
            totalDuration = tempActionAnimations[animationIndex].GetTotalDuration();
            return true;
        }

        public override bool GetLeftHandAttackAnimation(
            int dataId,
            int animationIndex,
            out float animSpeedRate,
            out float[] triggerDurations,
            out float totalDuration)
        {
            ActionAnimation[] tempActionAnimations = GetLeftHandAttackAnimations(dataId);
            animSpeedRate = 1f;
            triggerDurations = new float[] { 0f };
            totalDuration = 0f;
            if (tempActionAnimations.Length == 0 || animationIndex >= tempActionAnimations.Length) return false;
            animationIndex = Random.Range(0, tempActionAnimations.Length);
            animSpeedRate = tempActionAnimations[animationIndex].GetAnimSpeedRate();
            triggerDurations = tempActionAnimations[animationIndex].GetTriggerDurations();
            totalDuration = tempActionAnimations[animationIndex].GetTotalDuration();
            return true;
        }

        public override bool GetSkillActivateAnimation(
            int dataId,
            out float animSpeedRate,
            out float[] triggerDurations,
            out float totalDuration)
        {
            ActionAnimation tempActionAnimation = GetSkillActivateAnimation(dataId);
            animSpeedRate = tempActionAnimation.GetAnimSpeedRate();
            triggerDurations = tempActionAnimation.GetTriggerDurations();
            totalDuration = tempActionAnimation.GetTotalDuration();
            return true;
        }

        public override bool GetRightHandReloadAnimation(
            int dataId,
            out float animSpeedRate,
            out float[] triggerDurations,
            out float totalDuration)
        {
            ActionAnimation tempActionAnimation = GetRightHandReloadAnimation(dataId);
            animSpeedRate = tempActionAnimation.GetAnimSpeedRate();
            triggerDurations = tempActionAnimation.GetTriggerDurations();
            totalDuration = tempActionAnimation.GetTotalDuration();
            return true;
        }

        public override bool GetLeftHandReloadAnimation(
            int dataId,
            out float animSpeedRate,
            out float[] triggerDurations,
            out float totalDuration)
        {
            ActionAnimation tempActionAnimation = GetLeftHandReloadAnimation(dataId);
            animSpeedRate = tempActionAnimation.GetAnimSpeedRate();
            triggerDurations = tempActionAnimation.GetTriggerDurations();
            totalDuration = tempActionAnimation.GetTotalDuration();
            return true;
        }

        public override SkillActivateAnimationType GetSkillActivateAnimationType(int dataId)
        {
            SkillAnimations anims;
            if (!TryGetSkillAnimations(dataId, out anims))
                return SkillActivateAnimationType.UseActivateAnimation;
            return anims.activateAnimationType;
        }

#if UNITY_EDITOR
        [ContextMenu("Copy Weapon Animations", false, 1000401)]
        public void CopyWeaponAnimations()
        {
            CharacterModelDataManager.CopyWeaponAnimations(weaponAnimations);
        }

        [ContextMenu("Paste Weapon Animations", false, 1000402)]
        public void PasteWeaponAnimations()
        {
            WeaponAnimations[] weaponAnimations = CharacterModelDataManager.PasteWeaponAnimations();
            if (weaponAnimations != null)
            {
                this.weaponAnimations = weaponAnimations;
                EditorUtility.SetDirty(this);
            }
        }

        [ContextMenu("Copy Skill Animations", false, 1000403)]
        public void CopySkillAnimations()
        {
            CharacterModelDataManager.CopySkillAnimations(skillAnimations);
        }

        [ContextMenu("Paste Skill Animations", false, 1000404)]
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

#if UNITY_EDITOR
        [ContextMenu("Set Animator Clips For Test", false, 1000405)]
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

            try
            {
                // Action animation clips
                ActionAnimation tempActionAnimation = GetActionAnimation(testAnimActionType, testActionAnimDataId, testAttackAnimIndex);
                CacheAnimatorController[defaultAnimatorData.actionClip.name] = tempActionAnimation.clip;

                // Skill animation clips
                AnimationClip castClip = GetSkillCastClip(testCastSkillAnimDataId);
                CacheAnimatorController[defaultAnimatorData.castSkillClip.name] = castClip;

                Logging.Log(ToString(), "Animation Clips already set to animator controller, you can test an animations in Animation tab");
            }
            catch (System.Exception ex)
            {
                Logging.LogException(ToString(), ex);
            }

            this.InvokeInstanceDevExtMethods("SetAnimatorClipsForTest");
        }

        [ContextMenu("Convert To Newer Character Model", false, 1000500)]
        public void ConvertToNewerCharacterModel()
        {
            ConvertToNewerCharacterModelImplement();
        }

        protected virtual void ConvertToNewerCharacterModelImplement()
        {
            switch (animatorType)
            {
                case AnimatorType.Animator:
                    ConvertToAnimatorCharacterModel();
                    break;
                case AnimatorType.LegacyAnimtion:
                    ConvertToAnimationCharacterModel();
                    break;
            }
            EditorUtility.DisplayDialog("Character Model Conversion", "New Character Model component has been added.\n\nThe old component doesn't removed yet to let you check values.\n\nThen, you have to remove the old one.", "OK");
        }

        private void ConvertToAnimatorCharacterModel()
        {
            AnimatorCharacterModel model = gameObject.GetOrAddComponent<AnimatorCharacterModel>();
            model.skinnedMeshRenderer = skinnedMeshRenderer;
            model.weaponAnimations = weaponAnimations;
            model.skillAnimations = skillAnimations;
            model.defaultAnimations = new DefaultAnimations()
            {
                idleClip = defaultAnimatorData.idleClip,
                moveClip = defaultAnimatorData.moveClip,
                moveBackwardClip = defaultAnimatorData.moveBackwardClip,
                moveLeftClip = defaultAnimatorData.moveLeftClip,
                moveRightClip = defaultAnimatorData.moveRightClip,
                moveForwardLeftClip = defaultAnimatorData.moveForwardLeftClip,
                moveForwardRightClip = defaultAnimatorData.moveForwardRightClip,
                moveBackwardLeftClip = defaultAnimatorData.moveBackwardLeftClip,
                moveBackwardRightClip = defaultAnimatorData.moveBackwardRightClip,
                jumpClip = defaultAnimatorData.jumpClip,
                fallClip = defaultAnimatorData.fallClip,
                hurtClip = defaultAnimatorData.hurtClip,
                deadClip = defaultAnimatorData.deadClip,
                rightHandAttackAnimations = defaultAttackAnimations,
                leftHandAttackAnimations = defaultAttackAnimations,
                rightHandReloadAnimation = defaultReloadAnimation,
                leftHandReloadAnimation = defaultReloadAnimation,
                skillCastClip = defaultSkillCastClip,
                skillActivateAnimation = defaultSkillActivateAnimation,
            };
            model.HiddingObjects = hiddingObjects;
            model.HiddingRenderers = hiddingRenderers;
            model.FpsHiddingObjects = fpsHiddingObjects;
            model.FpsHiddingRenderers = fpsHiddingRenderers;
            model.EffectContainers = effectContainers;
            model.EquipmentContainers = equipmentContainers;
            model.ActivateObjectsWhenSwitchModel = activateObjectsWhenSwitchModel;
            model.DeactivateObjectsWhenSwitchModel = deactivateObjectsWhenSwitchModel;
            model.VehicleModels = vehicleModels;
            EditorUtility.SetDirty(model);
        }

        private void ConvertToAnimationCharacterModel()
        {
            AnimationCharacterModel model = gameObject.GetOrAddComponent<AnimationCharacterModel>();
            model.skinnedMeshRenderer = skinnedMeshRenderer;
            model.weaponAnimations = weaponAnimations;
            model.skillAnimations = skillAnimations;
            model.defaultAnimations = new DefaultAnimations()
            {
                idleClip = legacyAnimationData.idleClip,
                moveClip = legacyAnimationData.moveClip,
                moveBackwardClip = legacyAnimationData.moveBackwardClip,
                moveLeftClip = legacyAnimationData.moveLeftClip,
                moveRightClip = legacyAnimationData.moveRightClip,
                moveForwardLeftClip = legacyAnimationData.moveForwardLeftClip,
                moveForwardRightClip = legacyAnimationData.moveForwardRightClip,
                moveBackwardLeftClip = legacyAnimationData.moveBackwardLeftClip,
                moveBackwardRightClip = legacyAnimationData.moveBackwardRightClip,
                jumpClip = legacyAnimationData.jumpClip,
                fallClip = legacyAnimationData.fallClip,
                hurtClip = legacyAnimationData.hurtClip,
                deadClip = legacyAnimationData.deadClip,
                rightHandAttackAnimations = defaultAttackAnimations,
                leftHandAttackAnimations = defaultAttackAnimations,
                rightHandReloadAnimation = defaultReloadAnimation,
                leftHandReloadAnimation = defaultReloadAnimation,
                skillCastClip = defaultSkillCastClip,
                skillActivateAnimation = defaultSkillActivateAnimation,
            };
            model.HiddingObjects = hiddingObjects;
            model.HiddingRenderers = hiddingRenderers;
            model.FpsHiddingObjects = fpsHiddingObjects;
            model.FpsHiddingRenderers = fpsHiddingRenderers;
            model.EffectContainers = effectContainers;
            model.EquipmentContainers = equipmentContainers;
            model.ActivateObjectsWhenSwitchModel = activateObjectsWhenSwitchModel;
            model.DeactivateObjectsWhenSwitchModel = deactivateObjectsWhenSwitchModel;
            model.VehicleModels = vehicleModels;
            EditorUtility.SetDirty(model);
        }
#endif
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
