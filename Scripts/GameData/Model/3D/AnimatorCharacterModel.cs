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
        public static readonly int ANIM_IS_WEAPON_PULLING = Animator.StringToHash("IsWeaponPulling");
        public static readonly int ANIM_HURT = Animator.StringToHash("Hurt");
        public static readonly int ANIM_JUMP = Animator.StringToHash("Jump");
        public static readonly int ANIM_PICKUP = Animator.StringToHash("Pickup");
        public static readonly int ANIM_MOVE_CLIP_MULTIPLIER = Animator.StringToHash("MoveSpeedMultiplier");
        public static readonly int ANIM_ACTION_CLIP_MULTIPLIER = Animator.StringToHash("ActionSpeedMultiplier");
        public static readonly int ANIM_HURT_CLIP_MULTIPLIER = Animator.StringToHash("HurtSpeedMultiplier");
        public static readonly int ANIM_DEAD_CLIP_MULTIPLIER = Animator.StringToHash("DeadSpeedMultiplier");
        public static readonly int ANIM_JUMP_CLIP_MULTIPLIER = Animator.StringToHash("JumpSpeedMultiplier");
        public static readonly int ANIM_FALL_CLIP_MULTIPLIER = Animator.StringToHash("FallSpeedMultiplier");
        public static readonly int ANIM_PICKUP_CLIP_MULTIPLIER = Animator.StringToHash("PickupSpeedMultiplier");
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

        [Header("Action State Settings")]
        [Tooltip("Which layer in Animator controller that you use it to play action animations, You can set this when animator controller type is `Custom`")]
        public int actionStateLayer;
        public string[] actionStateNames = new string[] { "Action", "Action" };

        [Header("Cast Skill State Settings")]
        [Tooltip("Which layer in Animator controller that you use it to play cast skill animations, You can set this when animator controller type is `Custom`")]
        public int castSkillStateLayer;
        public string[] castSkillStateNames = new string[] { "CastSkill", "CastSkill" };

#if UNITY_EDITOR
        [Header("Animation Test Tool")]
        public AnimActionType testAnimActionType;
        public WeaponType testWeaponType;
        public BaseSkill testSkill;
        public int testAttackAnimIndex;
        [InspectorButton(nameof(SetAnimatorClipsForTest))]
        public bool setAnimatorClipsForTest;
#endif

        public AnimatorOverrideController CacheAnimatorController { get; private set; }
        private Dictionary<string, AnimationClip> animationClipOverrides = new Dictionary<string, AnimationClip>();
        private int[] actionStateNameHashes;
        private int[] castSkillStateNameHashes;

        // Private state validater
        private bool isSetupComponent;
        private float idleAnimSpeedRate;
        private float moveAnimSpeedRate;
        private float sprintAnimSpeedRate;
        private float walkAnimSpeedRate;
        private float crouchIdleAnimSpeedRate;
        private float crouchMoveAnimSpeedRate;
        private float crawlIdleAnimSpeedRate;
        private float crawlMoveAnimSpeedRate;
        private float swimIdleAnimSpeedRate;
        private float swimMoveAnimSpeedRate;
        private float jumpAnimSpeedRate;
        private float fallAnimSpeedRate;
        private float hurtAnimSpeedRate;
        private float deadAnimSpeedRate;
        private float pickupAnimSpeedRate;

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
                    if (hasChanges)
                    {
                        actionStateNames = new string[] { "Action" };
                        castSkillStateNames = new string[] { "CastSkill" };
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
                    if (hasChanges)
                    {
                        actionStateNames = new string[] { "Action", "Action" };
                        castSkillStateNames = new string[] { "CastSkill", "CastSkill" };
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
            // Setup action state name hashes
            int indexCounter;
            actionStateNameHashes = new int[actionStateNames.Length];
            for (indexCounter = 0; indexCounter < actionStateNames.Length; ++indexCounter)
                actionStateNameHashes[indexCounter] = Animator.StringToHash(actionStateNames[indexCounter]);
            // Setup cast skill state name hashes
            castSkillStateNameHashes = new int[castSkillStateNames.Length];
            for (indexCounter = 0; indexCounter < castSkillStateNames.Length; ++indexCounter)
                castSkillStateNameHashes[indexCounter] = Animator.StringToHash(castSkillStateNames[indexCounter]);
            SetDefaultAnimations();
        }

        public override void SetDefaultAnimations()
        {
            SetupClips(
                // Move
                defaultAnimations.idleClip,
                defaultAnimations.moveClip,
                defaultAnimations.moveBackwardClip,
                defaultAnimations.moveLeftClip,
                defaultAnimations.moveRightClip,
                defaultAnimations.moveForwardLeftClip,
                defaultAnimations.moveForwardRightClip,
                defaultAnimations.moveBackwardLeftClip,
                defaultAnimations.moveBackwardRightClip,
                // Sprint
                defaultAnimations.sprintClip,
                defaultAnimations.sprintBackwardClip,
                defaultAnimations.sprintLeftClip,
                defaultAnimations.sprintRightClip,
                defaultAnimations.sprintForwardLeftClip,
                defaultAnimations.sprintForwardRightClip,
                defaultAnimations.sprintBackwardLeftClip,
                defaultAnimations.sprintBackwardRightClip,
                // Walk
                defaultAnimations.walkClip,
                defaultAnimations.walkBackwardClip,
                defaultAnimations.walkLeftClip,
                defaultAnimations.walkRightClip,
                defaultAnimations.walkForwardLeftClip,
                defaultAnimations.walkForwardRightClip,
                defaultAnimations.walkBackwardLeftClip,
                defaultAnimations.walkBackwardRightClip,
                // Crouch
                defaultAnimations.crouchIdleClip,
                defaultAnimations.crouchMoveClip,
                defaultAnimations.crouchMoveBackwardClip,
                defaultAnimations.crouchMoveLeftClip,
                defaultAnimations.crouchMoveRightClip,
                defaultAnimations.crouchMoveForwardLeftClip,
                defaultAnimations.crouchMoveForwardRightClip,
                defaultAnimations.crouchMoveBackwardLeftClip,
                defaultAnimations.crouchMoveBackwardRightClip,
                // Crawl
                defaultAnimations.crawlIdleClip,
                defaultAnimations.crawlMoveClip,
                defaultAnimations.crawlMoveBackwardClip,
                defaultAnimations.crawlMoveLeftClip,
                defaultAnimations.crawlMoveRightClip,
                defaultAnimations.crawlMoveForwardLeftClip,
                defaultAnimations.crawlMoveForwardRightClip,
                defaultAnimations.crawlMoveBackwardLeftClip,
                defaultAnimations.crawlMoveBackwardRightClip,
                // Swim
                defaultAnimations.swimIdleClip,
                defaultAnimations.swimMoveClip,
                defaultAnimations.swimMoveBackwardClip,
                defaultAnimations.swimMoveLeftClip,
                defaultAnimations.swimMoveRightClip,
                defaultAnimations.swimMoveForwardLeftClip,
                defaultAnimations.swimMoveForwardRightClip,
                defaultAnimations.swimMoveBackwardLeftClip,
                defaultAnimations.swimMoveBackwardRightClip,
                // Other
                defaultAnimations.jumpClip,
                defaultAnimations.fallClip,
                defaultAnimations.hurtClip,
                defaultAnimations.deadClip,
                defaultAnimations.pickupClip,
                // Speed Rate
                defaultAnimations.idleAnimSpeedRate,
                defaultAnimations.moveAnimSpeedRate,
                defaultAnimations.sprintAnimSpeedRate,
                defaultAnimations.walkAnimSpeedRate,
                defaultAnimations.crouchIdleAnimSpeedRate,
                defaultAnimations.crouchMoveAnimSpeedRate,
                defaultAnimations.crawlIdleAnimSpeedRate,
                defaultAnimations.crawlMoveAnimSpeedRate,
                defaultAnimations.swimIdleAnimSpeedRate,
                defaultAnimations.swimMoveAnimSpeedRate,
                defaultAnimations.jumpAnimSpeedRate,
                defaultAnimations.fallAnimSpeedRate,
                defaultAnimations.hurtAnimSpeedRate,
                defaultAnimations.deadAnimSpeedRate,
                defaultAnimations.pickupAnimSpeedRate);
            base.SetDefaultAnimations();
        }

        private void SetupClips(
            // Move
            AnimationClip idleClip,
            AnimationClip moveClip,
            AnimationClip moveBackwardClip,
            AnimationClip moveLeftClip,
            AnimationClip moveRightClip,
            AnimationClip moveForwardLeftClip,
            AnimationClip moveForwardRightClip,
            AnimationClip moveBackwardLeftClip,
            AnimationClip moveBackwardRightClip,
            // Sprint
            AnimationClip sprintClip,
            AnimationClip sprintBackwardClip,
            AnimationClip sprintLeftClip,
            AnimationClip sprintRightClip,
            AnimationClip sprintForwardLeftClip,
            AnimationClip sprintForwardRightClip,
            AnimationClip sprintBackwardLeftClip,
            AnimationClip sprintBackwardRightClip,
            // Walk
            AnimationClip walkClip,
            AnimationClip walkBackwardClip,
            AnimationClip walkLeftClip,
            AnimationClip walkRightClip,
            AnimationClip walkForwardLeftClip,
            AnimationClip walkForwardRightClip,
            AnimationClip walkBackwardLeftClip,
            AnimationClip walkBackwardRightClip,
            // Crouch
            AnimationClip crouchIdleClip,
            AnimationClip crouchMoveClip,
            AnimationClip crouchMoveBackwardClip,
            AnimationClip crouchMoveLeftClip,
            AnimationClip crouchMoveRightClip,
            AnimationClip crouchMoveForwardLeftClip,
            AnimationClip crouchMoveForwardRightClip,
            AnimationClip crouchMoveBackwardLeftClip,
            AnimationClip crouchMoveBackwardRightClip,
            // Crawl
            AnimationClip crawlIdleClip,
            AnimationClip crawlMoveClip,
            AnimationClip crawlMoveBackwardClip,
            AnimationClip crawlMoveLeftClip,
            AnimationClip crawlMoveRightClip,
            AnimationClip crawlMoveForwardLeftClip,
            AnimationClip crawlMoveForwardRightClip,
            AnimationClip crawlMoveBackwardLeftClip,
            AnimationClip crawlMoveBackwardRightClip,
            // Swim
            AnimationClip swimIdleClip,
            AnimationClip swimMoveClip,
            AnimationClip swimMoveBackwardClip,
            AnimationClip swimMoveLeftClip,
            AnimationClip swimMoveRightClip,
            AnimationClip swimMoveForwardLeftClip,
            AnimationClip swimMoveForwardRightClip,
            AnimationClip swimMoveBackwardLeftClip,
            AnimationClip swimMoveBackwardRightClip,
            // Other
            AnimationClip jumpClip,
            AnimationClip fallClip,
            AnimationClip hurtClip,
            AnimationClip deadClip,
            AnimationClip pickupClip,
            // Speed rate
            float idleAnimSpeedRate,
            float moveAnimSpeedRate,
            float sprintAnimSpeedRate,
            float walkAnimSpeedRate,
            float crouchIdleAnimSpeedRate,
            float crouchMoveAnimSpeedRate,
            float crawlIdleAnimSpeedRate,
            float crawlMoveAnimSpeedRate,
            float swimIdleAnimSpeedRate,
            float swimMoveAnimSpeedRate,
            float jumpAnimSpeedRate,
            float fallAnimSpeedRate,
            float hurtAnimSpeedRate,
            float deadAnimSpeedRate,
            float pickupAnimSpeedRate)
        {
            if (CacheAnimatorController == null)
                return;
            // Move
            animationClipOverrides.Clear();
            OverrideAnimationClip(CLIP_IDLE, idleClip != null ? idleClip : defaultAnimations.idleClip);
            OverrideAnimationClip(CLIP_MOVE, moveClip != null ? moveClip : defaultAnimations.moveClip);
            OverrideAnimationClip(CLIP_MOVE_BACKWARD, moveBackwardClip != null ? moveBackwardClip : defaultAnimations.moveBackwardClip);
            OverrideAnimationClip(CLIP_MOVE_LEFT, moveLeftClip != null ? moveLeftClip : defaultAnimations.moveLeftClip);
            OverrideAnimationClip(CLIP_MOVE_RIGHT, moveRightClip != null ? moveRightClip : defaultAnimations.moveRightClip);
            OverrideAnimationClip(CLIP_MOVE_FORWARD_LEFT, moveForwardLeftClip != null ? moveForwardLeftClip : defaultAnimations.moveForwardLeftClip);
            OverrideAnimationClip(CLIP_MOVE_FORWARD_RIGHT, moveForwardRightClip != null ? moveForwardRightClip : defaultAnimations.moveForwardRightClip);
            OverrideAnimationClip(CLIP_MOVE_BACKWARD_LEFT, moveBackwardLeftClip != null ? moveBackwardLeftClip : defaultAnimations.moveBackwardLeftClip);
            OverrideAnimationClip(CLIP_MOVE_BACKWARD_RIGHT, moveBackwardRightClip != null ? moveBackwardRightClip : defaultAnimations.moveBackwardRightClip);
            // Sprint
            OverrideAnimationClip(CLIP_SPRINT, sprintClip != null ? sprintClip : defaultAnimations.sprintClip);
            OverrideAnimationClip(CLIP_SPRINT_BACKWARD, sprintBackwardClip != null ? sprintBackwardClip : defaultAnimations.sprintBackwardClip);
            OverrideAnimationClip(CLIP_SPRINT_LEFT, sprintLeftClip != null ? sprintLeftClip : defaultAnimations.sprintLeftClip);
            OverrideAnimationClip(CLIP_SPRINT_RIGHT, sprintRightClip != null ? sprintRightClip : defaultAnimations.sprintRightClip);
            OverrideAnimationClip(CLIP_SPRINT_FORWARD_LEFT, sprintForwardLeftClip != null ? sprintForwardLeftClip : defaultAnimations.sprintForwardLeftClip);
            OverrideAnimationClip(CLIP_SPRINT_FORWARD_RIGHT, sprintForwardRightClip != null ? sprintForwardRightClip : defaultAnimations.sprintForwardRightClip);
            OverrideAnimationClip(CLIP_SPRINT_BACKWARD_LEFT, sprintBackwardLeftClip != null ? sprintBackwardLeftClip : defaultAnimations.sprintBackwardLeftClip);
            OverrideAnimationClip(CLIP_SPRINT_BACKWARD_RIGHT, sprintBackwardRightClip != null ? sprintBackwardRightClip : defaultAnimations.sprintBackwardRightClip);
            // Walk
            OverrideAnimationClip(CLIP_WALK, walkClip != null ? walkClip : defaultAnimations.walkClip);
            OverrideAnimationClip(CLIP_WALK_BACKWARD, walkBackwardClip != null ? walkBackwardClip : defaultAnimations.walkBackwardClip);
            OverrideAnimationClip(CLIP_WALK_LEFT, walkLeftClip != null ? walkLeftClip : defaultAnimations.walkLeftClip);
            OverrideAnimationClip(CLIP_WALK_RIGHT, walkRightClip != null ? walkRightClip : defaultAnimations.walkRightClip);
            OverrideAnimationClip(CLIP_WALK_FORWARD_LEFT, walkForwardLeftClip != null ? walkForwardLeftClip : defaultAnimations.walkForwardLeftClip);
            OverrideAnimationClip(CLIP_WALK_FORWARD_RIGHT, walkForwardRightClip != null ? walkForwardRightClip : defaultAnimations.walkForwardRightClip);
            OverrideAnimationClip(CLIP_WALK_BACKWARD_LEFT, walkBackwardLeftClip != null ? walkBackwardLeftClip : defaultAnimations.walkBackwardLeftClip);
            OverrideAnimationClip(CLIP_WALK_BACKWARD_RIGHT, walkBackwardRightClip != null ? walkBackwardRightClip : defaultAnimations.walkBackwardRightClip);
            // Crouch
            OverrideAnimationClip(CLIP_CROUCH_IDLE, crouchIdleClip != null ? crouchIdleClip : defaultAnimations.crouchIdleClip);
            OverrideAnimationClip(CLIP_CROUCH_MOVE, crouchMoveClip != null ? crouchMoveClip : defaultAnimations.crouchMoveClip);
            OverrideAnimationClip(CLIP_CROUCH_MOVE_BACKWARD, crouchMoveBackwardClip != null ? crouchMoveBackwardClip : defaultAnimations.crouchMoveBackwardClip);
            OverrideAnimationClip(CLIP_CROUCH_MOVE_LEFT, crouchMoveLeftClip != null ? crouchMoveLeftClip : defaultAnimations.crouchMoveLeftClip);
            OverrideAnimationClip(CLIP_CROUCH_MOVE_RIGHT, crouchMoveRightClip != null ? crouchMoveRightClip : defaultAnimations.crouchMoveRightClip);
            OverrideAnimationClip(CLIP_CROUCH_MOVE_FORWARD_LEFT, crouchMoveForwardLeftClip != null ? crouchMoveForwardLeftClip : defaultAnimations.crouchMoveForwardLeftClip);
            OverrideAnimationClip(CLIP_CROUCH_MOVE_FORWARD_RIGHT, crouchMoveForwardRightClip != null ? crouchMoveForwardRightClip : defaultAnimations.crouchMoveForwardRightClip);
            OverrideAnimationClip(CLIP_CROUCH_MOVE_BACKWARD_LEFT, crouchMoveBackwardLeftClip != null ? crouchMoveBackwardLeftClip : defaultAnimations.crouchMoveBackwardLeftClip);
            OverrideAnimationClip(CLIP_CROUCH_MOVE_BACKWARD_RIGHT, crouchMoveBackwardRightClip != null ? crouchMoveBackwardRightClip : defaultAnimations.crouchMoveBackwardRightClip);
            // Crawl
            OverrideAnimationClip(CLIP_CRAWL_IDLE, crawlIdleClip != null ? crawlIdleClip : defaultAnimations.crawlIdleClip);
            OverrideAnimationClip(CLIP_CRAWL_MOVE, crawlMoveClip != null ? crawlMoveClip : defaultAnimations.crawlMoveClip);
            OverrideAnimationClip(CLIP_CRAWL_MOVE_BACKWARD, crawlMoveBackwardClip != null ? crawlMoveBackwardClip : defaultAnimations.crawlMoveBackwardClip);
            OverrideAnimationClip(CLIP_CRAWL_MOVE_LEFT, crawlMoveLeftClip != null ? crawlMoveLeftClip : defaultAnimations.crawlMoveLeftClip);
            OverrideAnimationClip(CLIP_CRAWL_MOVE_RIGHT, crawlMoveRightClip != null ? crawlMoveRightClip : defaultAnimations.crawlMoveRightClip);
            OverrideAnimationClip(CLIP_CRAWL_MOVE_FORWARD_LEFT, crawlMoveForwardLeftClip != null ? crawlMoveForwardLeftClip : defaultAnimations.crawlMoveForwardLeftClip);
            OverrideAnimationClip(CLIP_CRAWL_MOVE_FORWARD_RIGHT, crawlMoveForwardRightClip != null ? crawlMoveForwardRightClip : defaultAnimations.crawlMoveForwardRightClip);
            OverrideAnimationClip(CLIP_CRAWL_MOVE_BACKWARD_LEFT, crawlMoveBackwardLeftClip != null ? crawlMoveBackwardLeftClip : defaultAnimations.crawlMoveBackwardLeftClip);
            OverrideAnimationClip(CLIP_CRAWL_MOVE_BACKWARD_RIGHT, crawlMoveBackwardRightClip != null ? crawlMoveBackwardRightClip : defaultAnimations.crawlMoveBackwardRightClip);
            // Swim
            OverrideAnimationClip(CLIP_SWIM_IDLE, swimIdleClip != null ? swimIdleClip : defaultAnimations.swimIdleClip);
            OverrideAnimationClip(CLIP_SWIM_MOVE, swimMoveClip != null ? swimMoveClip : defaultAnimations.swimMoveClip);
            OverrideAnimationClip(CLIP_SWIM_MOVE_BACKWARD, swimMoveBackwardClip != null ? swimMoveBackwardClip : defaultAnimations.swimMoveBackwardClip);
            OverrideAnimationClip(CLIP_SWIM_MOVE_LEFT, swimMoveLeftClip != null ? swimMoveLeftClip : defaultAnimations.swimMoveLeftClip);
            OverrideAnimationClip(CLIP_SWIM_MOVE_RIGHT, swimMoveRightClip != null ? swimMoveRightClip : defaultAnimations.swimMoveRightClip);
            OverrideAnimationClip(CLIP_SWIM_MOVE_FORWARD_LEFT, swimMoveForwardLeftClip != null ? swimMoveForwardLeftClip : defaultAnimations.swimMoveForwardLeftClip);
            OverrideAnimationClip(CLIP_SWIM_MOVE_FORWARD_RIGHT, swimMoveForwardRightClip != null ? swimMoveForwardRightClip : defaultAnimations.swimMoveForwardRightClip);
            OverrideAnimationClip(CLIP_SWIM_MOVE_BACKWARD_LEFT, swimMoveBackwardLeftClip != null ? swimMoveBackwardLeftClip : defaultAnimations.swimMoveBackwardLeftClip);
            OverrideAnimationClip(CLIP_SWIM_MOVE_BACKWARD_RIGHT, swimMoveBackwardRightClip != null ? swimMoveBackwardRightClip : defaultAnimations.swimMoveBackwardRightClip);
            // Other
            OverrideAnimationClip(CLIP_JUMP, jumpClip != null ? jumpClip : defaultAnimations.jumpClip);
            OverrideAnimationClip(CLIP_FALL, fallClip != null ? fallClip : defaultAnimations.fallClip);
            OverrideAnimationClip(CLIP_HURT, hurtClip != null ? hurtClip : defaultAnimations.hurtClip);
            OverrideAnimationClip(CLIP_DEAD, deadClip != null ? deadClip : defaultAnimations.deadClip);
            OverrideAnimationClip(CLIP_PICKUP, pickupClip != null ? pickupClip : defaultAnimations.pickupClip);
            // Speed Rate
            // Stand move
            this.idleAnimSpeedRate = idleAnimSpeedRate > 0f ? idleAnimSpeedRate :
                defaultAnimations.idleAnimSpeedRate > 0f ? defaultAnimations.idleAnimSpeedRate : 1f;
            this.moveAnimSpeedRate = moveAnimSpeedRate > 0f ? moveAnimSpeedRate :
                defaultAnimations.moveAnimSpeedRate > 0f ? defaultAnimations.moveAnimSpeedRate : 1f;
            this.sprintAnimSpeedRate = sprintAnimSpeedRate > 0f ? sprintAnimSpeedRate :
                defaultAnimations.sprintAnimSpeedRate > 0f ? defaultAnimations.sprintAnimSpeedRate : 1f;
            this.walkAnimSpeedRate = walkAnimSpeedRate > 0f ? walkAnimSpeedRate :
                defaultAnimations.walkAnimSpeedRate > 0f ? defaultAnimations.walkAnimSpeedRate : 1f;
            // Crouch move
            this.crouchIdleAnimSpeedRate = crouchIdleAnimSpeedRate > 0f ? crouchIdleAnimSpeedRate :
                defaultAnimations.crouchIdleAnimSpeedRate > 0f ? defaultAnimations.crouchIdleAnimSpeedRate : 1f;
            this.crouchMoveAnimSpeedRate = crouchMoveAnimSpeedRate > 0f ? crouchMoveAnimSpeedRate :
                defaultAnimations.crouchMoveAnimSpeedRate > 0f ? defaultAnimations.crouchMoveAnimSpeedRate : 1f;
            // Crawl move
            this.crawlIdleAnimSpeedRate = crawlIdleAnimSpeedRate > 0f ? crawlIdleAnimSpeedRate :
                defaultAnimations.crawlIdleAnimSpeedRate > 0f ? defaultAnimations.crawlIdleAnimSpeedRate : 1f;
            this.crawlMoveAnimSpeedRate = crawlMoveAnimSpeedRate > 0f ? crawlMoveAnimSpeedRate :
                defaultAnimations.crawlMoveAnimSpeedRate > 0f ? defaultAnimations.crawlMoveAnimSpeedRate : 1f;
            // Swim move
            this.swimIdleAnimSpeedRate = swimIdleAnimSpeedRate > 0f ? swimIdleAnimSpeedRate :
                defaultAnimations.swimIdleAnimSpeedRate > 0f ? defaultAnimations.swimIdleAnimSpeedRate : 1f;
            this.swimMoveAnimSpeedRate = swimMoveAnimSpeedRate > 0f ? swimMoveAnimSpeedRate :
                defaultAnimations.swimMoveAnimSpeedRate > 0f ? defaultAnimations.swimMoveAnimSpeedRate : 1f;
            // Other
            this.jumpAnimSpeedRate = jumpAnimSpeedRate > 0f ? jumpAnimSpeedRate :
                defaultAnimations.jumpAnimSpeedRate > 0f ? defaultAnimations.jumpAnimSpeedRate : 1f;
            this.fallAnimSpeedRate = fallAnimSpeedRate > 0f ? fallAnimSpeedRate :
                defaultAnimations.fallAnimSpeedRate > 0f ? defaultAnimations.fallAnimSpeedRate : 1f;
            this.hurtAnimSpeedRate = hurtAnimSpeedRate > 0f ? hurtAnimSpeedRate :
                defaultAnimations.hurtAnimSpeedRate > 0f ? defaultAnimations.hurtAnimSpeedRate : 1f;
            this.deadAnimSpeedRate = deadAnimSpeedRate > 0f ? deadAnimSpeedRate :
                defaultAnimations.deadAnimSpeedRate > 0f ? defaultAnimations.deadAnimSpeedRate : 1f;
            this.pickupAnimSpeedRate = pickupAnimSpeedRate > 0f ? pickupAnimSpeedRate :
                defaultAnimations.pickupAnimSpeedRate > 0f ? defaultAnimations.pickupAnimSpeedRate : 1f;
        }

        private void OverrideAnimationClip(string key, AnimationClip clip)
        {
            if (clip == null)
                return;
            animationClipOverrides[key] = clip;
            CacheAnimatorController[key] = clip;
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
            GetAnims().CacheWeaponAnimations.TryGetValue(weaponType.DataId, out weaponAnimations);

            SetupClips(
                // Move
                weaponAnimations.idleClip,
                weaponAnimations.moveClip,
                weaponAnimations.moveBackwardClip,
                weaponAnimations.moveLeftClip,
                weaponAnimations.moveRightClip,
                weaponAnimations.moveForwardLeftClip,
                weaponAnimations.moveForwardRightClip,
                weaponAnimations.moveBackwardLeftClip,
                weaponAnimations.moveBackwardRightClip,
                // Sprint
                weaponAnimations.sprintClip,
                weaponAnimations.sprintBackwardClip,
                weaponAnimations.sprintLeftClip,
                weaponAnimations.sprintRightClip,
                weaponAnimations.sprintForwardLeftClip,
                weaponAnimations.sprintForwardRightClip,
                weaponAnimations.sprintBackwardLeftClip,
                weaponAnimations.sprintBackwardRightClip,
                // Walk
                weaponAnimations.walkClip,
                weaponAnimations.walkBackwardClip,
                weaponAnimations.walkLeftClip,
                weaponAnimations.walkRightClip,
                weaponAnimations.walkForwardLeftClip,
                weaponAnimations.walkForwardRightClip,
                weaponAnimations.walkBackwardLeftClip,
                weaponAnimations.walkBackwardRightClip,
                // Crouch
                weaponAnimations.crouchIdleClip,
                weaponAnimations.crouchMoveClip,
                weaponAnimations.crouchMoveBackwardClip,
                weaponAnimations.crouchMoveLeftClip,
                weaponAnimations.crouchMoveRightClip,
                weaponAnimations.crouchMoveForwardLeftClip,
                weaponAnimations.crouchMoveForwardRightClip,
                weaponAnimations.crouchMoveBackwardLeftClip,
                weaponAnimations.crouchMoveBackwardRightClip,
                // Crawl
                weaponAnimations.crawlIdleClip,
                weaponAnimations.crawlMoveClip,
                weaponAnimations.crawlMoveBackwardClip,
                weaponAnimations.crawlMoveLeftClip,
                weaponAnimations.crawlMoveRightClip,
                weaponAnimations.crawlMoveForwardLeftClip,
                weaponAnimations.crawlMoveForwardRightClip,
                weaponAnimations.crawlMoveBackwardLeftClip,
                weaponAnimations.crawlMoveBackwardRightClip,
                // Swim
                weaponAnimations.swimIdleClip,
                weaponAnimations.swimMoveClip,
                weaponAnimations.swimMoveBackwardClip,
                weaponAnimations.swimMoveLeftClip,
                weaponAnimations.swimMoveRightClip,
                weaponAnimations.swimMoveForwardLeftClip,
                weaponAnimations.swimMoveForwardRightClip,
                weaponAnimations.swimMoveBackwardLeftClip,
                weaponAnimations.swimMoveBackwardRightClip,
                // Other
                weaponAnimations.jumpClip,
                weaponAnimations.fallClip,
                weaponAnimations.hurtClip,
                weaponAnimations.deadClip,
                weaponAnimations.pickupClip,
                // Speed rate
                weaponAnimations.idleAnimSpeedRate,
                weaponAnimations.moveAnimSpeedRate,
                weaponAnimations.sprintAnimSpeedRate,
                weaponAnimations.walkAnimSpeedRate,
                weaponAnimations.crouchIdleAnimSpeedRate,
                weaponAnimations.crouchMoveAnimSpeedRate,
                weaponAnimations.crawlIdleAnimSpeedRate,
                weaponAnimations.crawlMoveAnimSpeedRate,
                weaponAnimations.swimIdleAnimSpeedRate,
                weaponAnimations.swimMoveAnimSpeedRate,
                weaponAnimations.jumpAnimSpeedRate,
                weaponAnimations.fallAnimSpeedRate,
                weaponAnimations.hurtAnimSpeedRate,
                weaponAnimations.deadAnimSpeedRate,
                weaponAnimations.pickupAnimSpeedRate);
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
            float idleAnimationSpeedMultiplier;

            // Set move speed based on inputs
            bool moving = false;
            float moveSpeed = 0f;
            if (movementState.HasFlag(MovementState.Forward))
            {
                moveSpeed = 1f;
                moving = true;
            }
            else if (movementState.HasFlag(MovementState.Backward))
            {
                moveSpeed = -1f;
                moving = true;
            }

            // Set side move speed based on inputs
            float sideMoveSpeed = 0f;
            if (movementState.HasFlag(MovementState.Right))
            {
                sideMoveSpeed = 1f;
                moving = true;
            }
            else if (movementState.HasFlag(MovementState.Left))
            {
                sideMoveSpeed = -1f;
                moving = true;
            }

            int moveType = 0;
            switch (extraMovementState)
            {
                case ExtraMovementState.IsCrouching:
                    moveType = 1;
                    moveAnimationSpeedMultiplier *= crouchMoveAnimSpeedRate;
                    idleAnimationSpeedMultiplier = crouchIdleAnimSpeedRate;
                    break;
                case ExtraMovementState.IsCrawling:
                    moveType = 2;
                    moveAnimationSpeedMultiplier *= crawlMoveAnimSpeedRate;
                    idleAnimationSpeedMultiplier = crawlIdleAnimSpeedRate;
                    break;
                case ExtraMovementState.IsSprinting:
                    moveSpeed *= 2;
                    sideMoveSpeed *= 2;
                    moveAnimationSpeedMultiplier *= sprintAnimSpeedRate;
                    idleAnimationSpeedMultiplier = idleAnimSpeedRate;
                    break;
                case ExtraMovementState.IsWalking:
                    moveSpeed *= 0.5f;
                    sideMoveSpeed *= 0.5f;
                    moveAnimationSpeedMultiplier *= walkAnimSpeedRate;
                    idleAnimationSpeedMultiplier = idleAnimSpeedRate;
                    break;
                default:
                    moveAnimationSpeedMultiplier *= moveAnimSpeedRate;
                    idleAnimationSpeedMultiplier = idleAnimSpeedRate;
                    break;
            }

            if (movementState.HasFlag(MovementState.IsUnderWater))
            {
                moveAnimationSpeedMultiplier *= swimMoveAnimSpeedRate;
                idleAnimationSpeedMultiplier = swimIdleAnimSpeedRate;
            }

            // Character may attacking, set character to idle state, 
            // Character is idle, so set move animation speed multiplier to 1
            if (moveAnimationSpeedMultiplier <= 0f || !moving)
            {
                moveSpeed = 0f;
                sideMoveSpeed = 0f;
                moveAnimationSpeedMultiplier = idleAnimationSpeedMultiplier;
            }

            // Set animator parameters
            float deltaTime = animator.updateMode == AnimatorUpdateMode.AnimatePhysics ? Time.fixedDeltaTime : Time.deltaTime;
            animator.SetFloat(ANIM_MOVE_SPEED, isDead ? 0 : moveSpeed, movementDampingTme, deltaTime);
            animator.SetFloat(ANIM_SIDE_MOVE_SPEED, isDead ? 0 : sideMoveSpeed, movementDampingTme, deltaTime);
            animator.SetFloat(ANIM_MOVE_CLIP_MULTIPLIER, moveAnimationSpeedMultiplier);
            animator.SetFloat(ANIM_HURT_CLIP_MULTIPLIER, hurtAnimSpeedRate);
            animator.SetFloat(ANIM_DEAD_CLIP_MULTIPLIER, deadAnimSpeedRate);
            animator.SetFloat(ANIM_JUMP_CLIP_MULTIPLIER, jumpAnimSpeedRate);
            animator.SetFloat(ANIM_FALL_CLIP_MULTIPLIER, fallAnimSpeedRate);
            animator.SetFloat(ANIM_PICKUP_CLIP_MULTIPLIER, pickupAnimSpeedRate);
            animator.SetBool(ANIM_IS_DEAD, isDead);
            animator.SetBool(ANIM_IS_GROUNDED, !movementState.HasFlag(MovementState.IsUnderWater) && movementState.HasFlag(MovementState.IsGrounded));
            animator.SetBool(ANIM_IS_UNDER_WATER, movementState.HasFlag(MovementState.IsUnderWater));
            animator.SetInteger(ANIM_MOVE_TYPE, moveType);
        }

        public override Coroutine PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1f)
        {
            StopActionAnimation();
            StopSkillCastAnimation();
            StopWeaponChargeAnimation();
            return StartedActionCoroutine(StartCoroutine(PlayActionAnimation_Animator(animActionType, dataId, index, playSpeedMultiplier)));
        }

        private IEnumerator PlayActionAnimation_Animator(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier)
        {
            ActionAnimation tempActionAnimation = GetActionAnimation(animActionType, dataId, index);
            playSpeedMultiplier *= tempActionAnimation.GetAnimSpeedRate();
            AudioManager.PlaySfxClipAtAudioSource(tempActionAnimation.GetRandomAudioClip(), genericAudioSource);
            bool hasClip = tempActionAnimation.clip != null && animator.isActiveAndEnabled;
            if (hasClip)
            {
                CacheAnimatorController[CLIP_ACTION] = tempActionAnimation.clip;
                animator.SetFloat(ANIM_ACTION_CLIP_MULTIPLIER, playSpeedMultiplier);
                animator.SetBool(ANIM_DO_ACTION, true);
                animator.SetBool(ANIM_DO_ACTION_ALL_LAYERS, tempActionAnimation.playClipAllLayers);
                if (tempActionAnimation.playClipAllLayers)
                {
                    for (int i = 0; i < animator.layerCount; ++i)
                    {
                        animator.Play(actionStateNameHashes[i], i, 0f);
                    }
                }
                else
                {
                    animator.Play(actionStateNameHashes[actionStateLayer], actionStateLayer, 0f);
                }
            }
            // Waits by current transition + clip duration before end animation
            yield return new WaitForSecondsRealtime(tempActionAnimation.GetClipLength() / playSpeedMultiplier);
            // Stop doing action animation
            if (hasClip)
            {
                animator.SetBool(ANIM_DO_ACTION, false);
                animator.SetBool(ANIM_DO_ACTION_ALL_LAYERS, false);
            }
            // Waits by current transition + extra duration before end playing animation state
            yield return new WaitForSecondsRealtime(tempActionAnimation.GetExtraDuration() / playSpeedMultiplier);
        }

        public override Coroutine PlaySkillCastClip(int dataId, float duration)
        {
            StopActionAnimation();
            StopSkillCastAnimation();
            StopWeaponChargeAnimation();
            return StartedActionCoroutine(StartCoroutine(PlaySkillCastClip_Animator(dataId, duration)));
        }

        private IEnumerator PlaySkillCastClip_Animator(int dataId, float duration)
        {
            AnimationClip castClip = GetSkillCastClip(dataId);
            bool hasClip = castClip != null && animator.isActiveAndEnabled;
            if (hasClip)
            {
                bool playAllLayers = IsSkillCastClipPlayingAllLayers(dataId);
                CacheAnimatorController[CLIP_CAST_SKILL] = castClip;
                animator.SetFloat(ANIM_ACTION_CLIP_MULTIPLIER, 1f);
                animator.SetBool(ANIM_IS_CASTING_SKILL, true);
                animator.SetBool(ANIM_IS_CASTING_SKILL_ALL_LAYERS, playAllLayers);
                if (playAllLayers)
                {
                    for (int i = 0; i < animator.layerCount; ++i)
                    {
                        animator.Play(castSkillStateNameHashes[i], i, 0f);
                    }
                }
                else
                {
                    animator.Play(castSkillStateNameHashes[castSkillStateLayer], castSkillStateLayer, 0f);
                }
            }
            // Waits by skill cast duration
            yield return new WaitForSecondsRealtime(duration);
            // Stop casting skill animation
            if (hasClip)
            {
                animator.SetBool(ANIM_IS_CASTING_SKILL, false);
                animator.SetBool(ANIM_IS_CASTING_SKILL_ALL_LAYERS, false);
            }
        }

        public override void PlayWeaponChargeClip(int dataId, bool isLeftHand)
        {
            StopActionAnimation();
            StopSkillCastAnimation();
            StopWeaponChargeAnimation();
            AnimationClip pullingClip = isLeftHand ? GetRightHandWeaponChargeClip(dataId) : GetLeftHandWeaponChargeClip(dataId);
            bool hasClip = pullingClip != null && animator.isActiveAndEnabled;
            if (hasClip)
            {
                CacheAnimatorController[CLIP_WEAPON_CHARGE] = pullingClip;
                animator.SetBool(ANIM_IS_WEAPON_PULLING, true);
            }
        }

        public override void StopActionAnimation()
        {
            if (animator.isActiveAndEnabled)
            {
                animator.SetBool(ANIM_DO_ACTION, false);
                animator.SetBool(ANIM_DO_ACTION_ALL_LAYERS, false);
            }
        }

        public override void StopSkillCastAnimation()
        {
            if (animator.isActiveAndEnabled)
            {
                animator.SetBool(ANIM_IS_CASTING_SKILL, false);
                animator.SetBool(ANIM_IS_CASTING_SKILL_ALL_LAYERS, false);
            }
        }

        public override void StopWeaponChargeAnimation()
        {
            if (animator.isActiveAndEnabled)
            {
                animator.SetBool(ANIM_IS_WEAPON_PULLING, false);
            }
        }

        public override void PlayHitAnimation()
        {
            if (!animationClipOverrides.ContainsKey(CLIP_HURT))
                return;
            StartCoroutine(PlayHitAnimationRoutine());
        }

        IEnumerator PlayHitAnimationRoutine()
        {
            yield return null;
            if (animator.isActiveAndEnabled)
            {
                animator.ResetTrigger(ANIM_HURT);
                animator.SetTrigger(ANIM_HURT);
            }
        }

        public override float GetJumpAnimationDuration()
        {
            if (!animationClipOverrides.ContainsKey(CLIP_JUMP))
                return 0f;
            return CacheAnimatorController[CLIP_JUMP].length / jumpAnimSpeedRate;
        }

        public override void PlayJumpAnimation()
        {
            if (!animationClipOverrides.ContainsKey(CLIP_JUMP))
                return;
            StartCoroutine(PlayJumpAnimationRoutine());
        }

        IEnumerator PlayJumpAnimationRoutine()
        {
            yield return null;
            if (animator.isActiveAndEnabled)
            {
                animator.ResetTrigger(ANIM_JUMP);
                animator.SetTrigger(ANIM_JUMP);
            }
        }

        public override void PlayPickupAnimation()
        {
            if (!animationClipOverrides.ContainsKey(CLIP_PICKUP))
                return;
            StartCoroutine(PlayPickUpAnimationRoutine());
        }

        IEnumerator PlayPickUpAnimationRoutine()
        {
            yield return null;
            if (animator.isActiveAndEnabled)
            {
                animator.ResetTrigger(ANIM_PICKUP);
                animator.SetTrigger(ANIM_PICKUP);
            }
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
