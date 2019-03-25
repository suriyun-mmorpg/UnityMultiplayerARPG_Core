using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class CharacterModel : BaseCharacterModel
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
        public ActionAnimation[] defaultAttackAnimations;
        public AnimationClip defaultSkillCastClip;
        public ActionAnimation defaultSkillActivateAnimation;
        public WeaponAnimations[] weaponAnimations;
        public SkillAnimations[] skillAnimations;

        // Deprecated
        [HideInInspector]
        public ActionAnimation[] defaultSkillCastAnimations;
        [HideInInspector]
        public SkillCastAnimations[] skillCastAnimations;

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

        private Dictionary<int, WeaponAnimations> cacheWeaponAnimations;
        public Dictionary<int, WeaponAnimations> CacheWeaponAnimations
        {
            get
            {
                if (cacheWeaponAnimations == null)
                {
                    cacheWeaponAnimations = new Dictionary<int, WeaponAnimations>();
                    foreach (WeaponAnimations weaponAnimation in weaponAnimations)
                    {
                        if (weaponAnimation.weaponType == null) continue;
                        cacheWeaponAnimations[weaponAnimation.weaponType.DataId] = weaponAnimation;
                    }
                }
                return cacheWeaponAnimations;
            }
        }

        private Dictionary<int, SkillAnimations> cacheSkillAnimations;
        public Dictionary<int, SkillAnimations> CacheSkillAnimations
        {
            get
            {
                if (cacheSkillAnimations == null)
                {
                    cacheSkillAnimations = new Dictionary<int, SkillAnimations>();
                    foreach (SkillAnimations skillAnimation in skillAnimations)
                    {
                        if (skillAnimation.skill == null) continue;
                        cacheSkillAnimations[skillAnimation.skill.DataId] = skillAnimation;
                    }
                }
                return cacheSkillAnimations;
            }
        }

        private AnimatorOverrideController cacheAnimatorController;
        public AnimatorOverrideController CacheAnimatorController
        {
            get
            {
                SetupComponent();
                return cacheAnimatorController;
            }
        }

        private void Awake()
        {
            MigrateSkillCastAnimations();
            SetupComponent();
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (MigrateSkillCastAnimations())
                EditorUtility.SetDirty(this);
#endif
        }

        private bool MigrateSkillCastAnimations()
        {
            bool hasChanges = false;
            if (defaultSkillCastAnimations != null &&
                defaultSkillCastAnimations.Length > 0)
            {
                hasChanges = true;
                defaultSkillActivateAnimation = defaultSkillCastAnimations[0];
                defaultSkillCastAnimations = null;
            }

            if (skillCastAnimations != null &&
                skillCastAnimations.Length > 0)
            {
                hasChanges = true;
                skillAnimations = new SkillAnimations[skillCastAnimations.Length];
                for (int i = 0; i < skillCastAnimations.Length; ++i)
                {
                    SkillAnimations data = new SkillAnimations();
                    data.skill = skillCastAnimations[i].skill;
                    if (skillCastAnimations[i].castAnimations != null &&
                        skillCastAnimations[i].castAnimations.Length > 0)
                        data.activateAnimation = skillCastAnimations[i].castAnimations[0];
                    skillAnimations[i] = data;
                }
                skillCastAnimations = null;
            }
            return hasChanges;
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
                    {
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
                    }
                    CrossFadeLegacyAnimation(LEGACY_CLIP_IDLE, legacyAnimationData.idleClipFadeLength, WrapMode.Loop);
                    break;
            }
        }

        public override void SetEquipWeapons(EquipWeapons equipWeapons)
        {
            base.SetEquipWeapons(equipWeapons);
            SetupComponent();
            Item weaponItem = GameInstance.Singleton.DefaultWeaponItem;
            if (equipWeapons.rightHand.NotEmptySlot() && equipWeapons.rightHand.GetWeaponItem() != null)
                weaponItem = equipWeapons.rightHand.GetWeaponItem();
            if (weaponItem != null)
            {
                WeaponAnimations weaponAnimations;
                if (CacheWeaponAnimations.TryGetValue(weaponItem.WeaponType.DataId, out weaponAnimations))
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

        public override void AddingNewModel(GameObject newModel)
        {
            base.AddingNewModel(newModel);
            SkinnedMeshRenderer skinnedMesh = newModel.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMesh != null && skinnedMeshRenderer != null)
            {
                skinnedMesh.bones = skinnedMeshRenderer.bones;
                skinnedMesh.rootBone = skinnedMeshRenderer.rootBone;
            }
        }

        public override void UpdateAnimation(bool isDead, MovementFlag movementState, float playMoveSpeedMultiplier = 1f)
        {
            switch (animatorType)
            {
                case AnimatorType.Animator:
                    UpdateAnimation_Animator(isDead, movementState, playMoveSpeedMultiplier);
                    break;
                case AnimatorType.LegacyAnimtion:
                    UpdateAnimation_LegacyAnimation(isDead, movementState, playMoveSpeedMultiplier);
                    break;
            }
        }

        #region Update Animation Functions
        private void UpdateAnimation_Animator(bool isDead, MovementFlag movementState, float playMoveSpeedMultiplier)
        {
            if (!animator.gameObject.activeInHierarchy)
                return;

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
            if (movementState.HasFlag(MovementFlag.Forward))
                moveSpeed = 1;
            else if (movementState.HasFlag(MovementFlag.Backward))
                moveSpeed = -1;
            if (movementState.HasFlag(MovementFlag.Right))
                sideMoveSpeed = 1;
            else if (movementState.HasFlag(MovementFlag.Left))
                sideMoveSpeed = -1;
            // Set animator parameters
            animator.SetFloat(ANIM_MOVE_SPEED, isDead ? 0 : moveSpeed);
            animator.SetFloat(ANIM_SIDE_MOVE_SPEED, isDead ? 0 : sideMoveSpeed);
            animator.SetFloat(ANIM_MOVE_CLIP_MULTIPLIER, playMoveSpeedMultiplier);
            animator.SetBool(ANIM_IS_DEAD, isDead);
            animator.SetBool(ANIM_IS_GROUNDED, movementState.HasFlag(MovementFlag.IsGrounded));
        }

        private void UpdateAnimation_LegacyAnimation(bool isDead, MovementFlag movementState, float playMoveSpeedMultiplier)
        {
            if (isDead)
                CrossFadeLegacyAnimation(LEGACY_CLIP_DEAD, legacyAnimationData.deadClipFadeLength, WrapMode.Once);
            else
            {
                if (legacyAnimation.GetClip(LEGACY_CLIP_ACTION) != null && legacyAnimation.IsPlaying(LEGACY_CLIP_ACTION))
                    return;
                if (legacyAnimation.GetClip(LEGACY_CLIP_CAST_SKILL) != null && legacyAnimation.IsPlaying(LEGACY_CLIP_CAST_SKILL))
                    return;
                if (!movementState.HasFlag(MovementFlag.IsGrounded))
                    CrossFadeLegacyAnimation(LEGACY_CLIP_FALL, legacyAnimationData.fallClipFadeLength, WrapMode.Loop);
                else
                {
                    // Forward Right
                    if (movementState.HasFlag(MovementFlag.Forward) && movementState.HasFlag(MovementFlag.Right))
                        CrossFadeLegacyAnimation(LEGACY_CLIP_MOVE_FORWARD_RIGHT, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Forward Left
                    else if (movementState.HasFlag(MovementFlag.Forward) && movementState.HasFlag(MovementFlag.Left))
                        CrossFadeLegacyAnimation(LEGACY_CLIP_MOVE_FORWARD_LEFT, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Backward Right
                    else if (movementState.HasFlag(MovementFlag.Backward) && movementState.HasFlag(MovementFlag.Right))
                        CrossFadeLegacyAnimation(LEGACY_CLIP_MOVE_BACKWARD_RIGHT, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Backward Left
                    else if (movementState.HasFlag(MovementFlag.Backward) && movementState.HasFlag(MovementFlag.Left))
                        CrossFadeLegacyAnimation(LEGACY_CLIP_MOVE_BACKWARD_LEFT, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Forward
                    else if (movementState.HasFlag(MovementFlag.Forward))
                        CrossFadeLegacyAnimation(LEGACY_CLIP_MOVE, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Backward
                    else if (movementState.HasFlag(MovementFlag.Backward))
                        CrossFadeLegacyAnimation(LEGACY_CLIP_MOVE_BACKWARD, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Right
                    else if (movementState.HasFlag(MovementFlag.Right))
                        CrossFadeLegacyAnimation(LEGACY_CLIP_MOVE_RIGHT, legacyAnimationData.moveClipFadeLength, WrapMode.Loop);
                    // Left
                    else if (movementState.HasFlag(MovementFlag.Left))
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
                return;
            }
            animator.SetBool(ANIM_DO_ACTION, false);
        }

        #region Action Animation Functions
        private IEnumerator PlayActionAnimation_Animator(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier)
        {
            // If animator is not null, play the action animation
            ActionAnimation tempActionAnimation = GetActionAnimation(animActionType, dataId, index);
            if (tempActionAnimation.clip != null)
            {
                animator.SetBool(ANIM_DO_ACTION, false);
                yield return 0;
                CacheAnimatorController[defaultActionClipName] = tempActionAnimation.clip;
                AudioClip audioClip = tempActionAnimation.GetRandomAudioClip();
                if (audioClip != null)
                    AudioSource.PlayClipAtPoint(audioClip, CacheTransform.position, AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level);
                animator.SetFloat(ANIM_ACTION_CLIP_MULTIPLIER, playSpeedMultiplier);
                animator.SetBool(ANIM_DO_ACTION, true);
                // Waits by current transition + clip duration before end animation
                yield return new WaitForSecondsRealtime(tempActionAnimation.GetClipLength() / playSpeedMultiplier);
                animator.SetBool(ANIM_DO_ACTION, false);
                // Waits by current transition + extra duration before end playing animation state
                yield return new WaitForSecondsRealtime(tempActionAnimation.GetExtraDuration() / playSpeedMultiplier);
            }
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
                AudioClip audioClip = tempActionAnimation.GetRandomAudioClip();
                if (audioClip != null)
                    AudioSource.PlayClipAtPoint(audioClip, CacheTransform.position, AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level);
                isPlayingActionAnimation = true;
                CrossFadeLegacyAnimation(LEGACY_CLIP_ACTION, legacyAnimationData.actionClipFadeLength, WrapMode.Once);
                // Waits by current transition + clip duration before end animation
                yield return new WaitForSecondsRealtime(tempActionAnimation.GetClipLength() / playSpeedMultiplier);
                CrossFadeLegacyAnimation(LEGACY_CLIP_IDLE, legacyAnimationData.idleClipFadeLength, WrapMode.Loop);
                // Waits by current transition + extra duration before end playing animation state
                yield return new WaitForSecondsRealtime(tempActionAnimation.GetExtraDuration() / playSpeedMultiplier);
                isPlayingActionAnimation = false;
            }
        }

        private IEnumerator PlaySkillCastClip_Animator(int dataId, float duration)
        {
            AnimationClip castClip = GetSkillCastClip(dataId);
            if (castClip != null)
            {
                CacheAnimatorController[defaultCastSkillClipName] = castClip;
                yield return 0;
                animator.SetBool(ANIM_IS_CASTING_SKILL, true);
                yield return new WaitForSecondsRealtime(duration);
                animator.SetBool(ANIM_IS_CASTING_SKILL, false);
            }
        }

        private IEnumerator PlaySkillCastClip_LegacyAnimation(int dataId, float duration)
        {
            AnimationClip castClip = GetSkillCastClip(dataId);
            if (castClip != null)
            {
                if (legacyAnimation.GetClip(LEGACY_CLIP_CAST_SKILL) != null)
                    legacyAnimation.RemoveClip(LEGACY_CLIP_CAST_SKILL);
                legacyAnimation.AddClip(castClip, LEGACY_CLIP_CAST_SKILL);
                CrossFadeLegacyAnimation(LEGACY_CLIP_CAST_SKILL, legacyAnimationData.actionClipFadeLength, WrapMode.Loop);
                yield return new WaitForSecondsRealtime(duration);
                if (!isPlayingActionAnimation)
                    CrossFadeLegacyAnimation(LEGACY_CLIP_IDLE, legacyAnimationData.idleClipFadeLength, WrapMode.Loop);
            }
        }
        #endregion
        
        public override void PlayHurtAnimation()
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
                case AnimActionType.Skill:
                    tempActionAnimation = GetSkillActivateAnimation(dataId);
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
            if (CacheWeaponAnimations.ContainsKey(dataId) &&
                CacheWeaponAnimations[dataId].rightHandAttackAnimations != null)
                return CacheWeaponAnimations[dataId].rightHandAttackAnimations;
            return defaultAttackAnimations;
        }

        public ActionAnimation[] GetLeftHandAttackAnimations(WeaponType weaponType)
        {
            return GetLeftHandAttackAnimations(weaponType.DataId);
        }

        public ActionAnimation[] GetLeftHandAttackAnimations(int dataId)
        {
            if (CacheWeaponAnimations.ContainsKey(dataId) &&
                CacheWeaponAnimations[dataId].leftHandAttackAnimations != null)
                return CacheWeaponAnimations[dataId].leftHandAttackAnimations;
            return defaultAttackAnimations;
        }

        public AnimationClip GetSkillCastClip(int dataId)
        {
            if (CacheSkillAnimations.ContainsKey(dataId) &&
                CacheSkillAnimations[dataId].castClip != null)
                return CacheSkillAnimations[dataId].castClip;
            return defaultSkillCastClip;
        }

        public ActionAnimation GetSkillActivateAnimation(int dataId)
        {
            if (CacheSkillAnimations.ContainsKey(dataId) &&
                CacheSkillAnimations[dataId].activateAnimation.clip != null)
                return CacheSkillAnimations[dataId].activateAnimation;
            return defaultSkillActivateAnimation;
        }

        public override bool GetRandomRightHandAttackAnimation(
            int dataId,
            out int animationIndex,
            out float triggerDuration,
            out float totalDuration)
        {
            ActionAnimation[] tempActionAnimations = GetRightHandAttackAnimations(dataId);
            animationIndex = 0;
            triggerDuration = 0f;
            totalDuration = 0f;
            if (tempActionAnimations.Length == 0) return false;
            animationIndex = Random.Range(0, tempActionAnimations.Length);
            triggerDuration = tempActionAnimations[animationIndex].GetTriggerDuration();
            totalDuration = tempActionAnimations[animationIndex].GetTotalDuration();
            return true;
        }

        public override bool GetRandomLeftHandAttackAnimation(
            int dataId,
            out int animationIndex,
            out float triggerDuration,
            out float totalDuration)
        {
            ActionAnimation[] tempActionAnimations = GetLeftHandAttackAnimations(dataId);
            animationIndex = 0;
            triggerDuration = 0f;
            totalDuration = 0f;
            if (tempActionAnimations.Length == 0) return false;
            animationIndex = Random.Range(0, tempActionAnimations.Length);
            triggerDuration = tempActionAnimations[animationIndex].GetTriggerDuration();
            totalDuration = tempActionAnimations[animationIndex].GetTotalDuration();
            return true;
        }

        public override bool GetSkillActivateAnimation(
            int dataId,
            out float triggerDuration,
            out float totalDuration)
        {
            ActionAnimation tempActionAnimation = GetSkillActivateAnimation(dataId);
            triggerDuration = tempActionAnimation.GetTriggerDuration();
            totalDuration = tempActionAnimation.GetTotalDuration();
            return true;
        }

        public override bool HasSkillAnimations(int dataId)
        {
            return CacheSkillAnimations.ContainsKey(dataId);
        }
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
