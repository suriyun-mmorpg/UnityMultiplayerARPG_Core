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

        public class CacheAnimatorController
        {
            public AnimatorOverrideController Default { get; private set; }
            public Dictionary<int, AnimatorOverrideController> Weapons { get; private set; }
            public CacheAnimatorController(int id, RuntimeAnimatorController animatorController, DefaultAnimations defaultAnimations, WeaponAnimations[] weaponAnimations)
            {
                Default = SetupController($"{id}_Default", new AnimatorOverrideController(animatorController), defaultAnimations);
                Weapons = new Dictionary<int, AnimatorOverrideController>();
                for (int i = 0; i < weaponAnimations.Length; ++i)
                {
                    if (Weapons.ContainsKey(weaponAnimations[i].weaponType.DataId))
                        continue;
                    Weapons[weaponAnimations[i].weaponType.DataId] = SetupController(
                        $"{id}_{weaponAnimations[i].weaponType.Id}",
                        new AnimatorOverrideController(animatorController),
                        defaultAnimations,
                        // Move
                        weaponAnimations[i].idleClip,
                        weaponAnimations[i].moveClip,
                        weaponAnimations[i].moveBackwardClip,
                        weaponAnimations[i].moveLeftClip,
                        weaponAnimations[i].moveRightClip,
                        weaponAnimations[i].moveForwardLeftClip,
                        weaponAnimations[i].moveForwardRightClip,
                        weaponAnimations[i].moveBackwardLeftClip,
                        weaponAnimations[i].moveBackwardRightClip,
                        // Sprint
                        weaponAnimations[i].sprintClip,
                        weaponAnimations[i].sprintBackwardClip,
                        weaponAnimations[i].sprintLeftClip,
                        weaponAnimations[i].sprintRightClip,
                        weaponAnimations[i].sprintForwardLeftClip,
                        weaponAnimations[i].sprintForwardRightClip,
                        weaponAnimations[i].sprintBackwardLeftClip,
                        weaponAnimations[i].sprintBackwardRightClip,
                        // Walk
                        weaponAnimations[i].walkClip,
                        weaponAnimations[i].walkBackwardClip,
                        weaponAnimations[i].walkLeftClip,
                        weaponAnimations[i].walkRightClip,
                        weaponAnimations[i].walkForwardLeftClip,
                        weaponAnimations[i].walkForwardRightClip,
                        weaponAnimations[i].walkBackwardLeftClip,
                        weaponAnimations[i].walkBackwardRightClip,
                        // Crouch
                        weaponAnimations[i].crouchIdleClip,
                        weaponAnimations[i].crouchMoveClip,
                        weaponAnimations[i].crouchMoveBackwardClip,
                        weaponAnimations[i].crouchMoveLeftClip,
                        weaponAnimations[i].crouchMoveRightClip,
                        weaponAnimations[i].crouchMoveForwardLeftClip,
                        weaponAnimations[i].crouchMoveForwardRightClip,
                        weaponAnimations[i].crouchMoveBackwardLeftClip,
                        weaponAnimations[i].crouchMoveBackwardRightClip,
                        // Crawl
                        weaponAnimations[i].crawlIdleClip,
                        weaponAnimations[i].crawlMoveClip,
                        weaponAnimations[i].crawlMoveBackwardClip,
                        weaponAnimations[i].crawlMoveLeftClip,
                        weaponAnimations[i].crawlMoveRightClip,
                        weaponAnimations[i].crawlMoveForwardLeftClip,
                        weaponAnimations[i].crawlMoveForwardRightClip,
                        weaponAnimations[i].crawlMoveBackwardLeftClip,
                        weaponAnimations[i].crawlMoveBackwardRightClip,
                        // Swim
                        weaponAnimations[i].swimIdleClip,
                        weaponAnimations[i].swimMoveClip,
                        weaponAnimations[i].swimMoveBackwardClip,
                        weaponAnimations[i].swimMoveLeftClip,
                        weaponAnimations[i].swimMoveRightClip,
                        weaponAnimations[i].swimMoveForwardLeftClip,
                        weaponAnimations[i].swimMoveForwardRightClip,
                        weaponAnimations[i].swimMoveBackwardLeftClip,
                        weaponAnimations[i].swimMoveBackwardRightClip,
                        // Other
                        weaponAnimations[i].jumpClip,
                        weaponAnimations[i].fallClip,
                        weaponAnimations[i].landedClip,
                        weaponAnimations[i].hurtClip,
                        weaponAnimations[i].deadClip,
                        weaponAnimations[i].pickupClip);
                }
            }

            private AnimatorOverrideController SetupController(
                string name,
                AnimatorOverrideController animatorController,
                DefaultAnimations defaultAnimations,
                // Move
                AnimationClip idleClip = null,
                AnimationClip moveClip = null,
                AnimationClip moveBackwardClip = null,
                AnimationClip moveLeftClip = null,
                AnimationClip moveRightClip = null,
                AnimationClip moveForwardLeftClip = null,
                AnimationClip moveForwardRightClip = null,
                AnimationClip moveBackwardLeftClip = null,
                AnimationClip moveBackwardRightClip = null,
                // Sprint
                AnimationClip sprintClip = null,
                AnimationClip sprintBackwardClip = null,
                AnimationClip sprintLeftClip = null,
                AnimationClip sprintRightClip = null,
                AnimationClip sprintForwardLeftClip = null,
                AnimationClip sprintForwardRightClip = null,
                AnimationClip sprintBackwardLeftClip = null,
                AnimationClip sprintBackwardRightClip = null,
                // Walk
                AnimationClip walkClip = null,
                AnimationClip walkBackwardClip = null,
                AnimationClip walkLeftClip = null,
                AnimationClip walkRightClip = null,
                AnimationClip walkForwardLeftClip = null,
                AnimationClip walkForwardRightClip = null,
                AnimationClip walkBackwardLeftClip = null,
                AnimationClip walkBackwardRightClip = null,
                // Crouch
                AnimationClip crouchIdleClip = null,
                AnimationClip crouchMoveClip = null,
                AnimationClip crouchMoveBackwardClip = null,
                AnimationClip crouchMoveLeftClip = null,
                AnimationClip crouchMoveRightClip = null,
                AnimationClip crouchMoveForwardLeftClip = null,
                AnimationClip crouchMoveForwardRightClip = null,
                AnimationClip crouchMoveBackwardLeftClip = null,
                AnimationClip crouchMoveBackwardRightClip = null,
                // Crawl
                AnimationClip crawlIdleClip = null,
                AnimationClip crawlMoveClip = null,
                AnimationClip crawlMoveBackwardClip = null,
                AnimationClip crawlMoveLeftClip = null,
                AnimationClip crawlMoveRightClip = null,
                AnimationClip crawlMoveForwardLeftClip = null,
                AnimationClip crawlMoveForwardRightClip = null,
                AnimationClip crawlMoveBackwardLeftClip = null,
                AnimationClip crawlMoveBackwardRightClip = null,
                // Swim
                AnimationClip swimIdleClip = null,
                AnimationClip swimMoveClip = null,
                AnimationClip swimMoveBackwardClip = null,
                AnimationClip swimMoveLeftClip = null,
                AnimationClip swimMoveRightClip = null,
                AnimationClip swimMoveForwardLeftClip = null,
                AnimationClip swimMoveForwardRightClip = null,
                AnimationClip swimMoveBackwardLeftClip = null,
                AnimationClip swimMoveBackwardRightClip = null,
                // Other
                AnimationClip jumpClip = null,
                AnimationClip fallClip = null,
                AnimationClip landedClip = null,
                AnimationClip hurtClip = null,
                AnimationClip deadClip = null,
                AnimationClip pickupClip = null)
            {
                animatorController.name = name;
                // Move
                animatorController[CLIP_IDLE] = idleClip != null ? idleClip : defaultAnimations.idleClip;
                animatorController[CLIP_MOVE] = moveClip != null ? moveClip : defaultAnimations.moveClip;
                animatorController[CLIP_MOVE_BACKWARD] = moveBackwardClip != null ? moveBackwardClip : defaultAnimations.moveBackwardClip;
                animatorController[CLIP_MOVE_LEFT] = moveLeftClip != null ? moveLeftClip : defaultAnimations.moveLeftClip;
                animatorController[CLIP_MOVE_RIGHT] = moveRightClip != null ? moveRightClip : defaultAnimations.moveRightClip;
                animatorController[CLIP_MOVE_FORWARD_LEFT] = moveForwardLeftClip != null ? moveForwardLeftClip : defaultAnimations.moveForwardLeftClip;
                animatorController[CLIP_MOVE_FORWARD_RIGHT] = moveForwardRightClip != null ? moveForwardRightClip : defaultAnimations.moveForwardRightClip;
                animatorController[CLIP_MOVE_BACKWARD_LEFT] = moveBackwardLeftClip != null ? moveBackwardLeftClip : defaultAnimations.moveBackwardLeftClip;
                animatorController[CLIP_MOVE_BACKWARD_RIGHT] = moveBackwardRightClip != null ? moveBackwardRightClip : defaultAnimations.moveBackwardRightClip;
                // Sprint
                animatorController[CLIP_SPRINT] = sprintClip != null ? sprintClip : defaultAnimations.sprintClip;
                animatorController[CLIP_SPRINT_BACKWARD] = sprintBackwardClip != null ? sprintBackwardClip : defaultAnimations.sprintBackwardClip;
                animatorController[CLIP_SPRINT_LEFT] = sprintLeftClip != null ? sprintLeftClip : defaultAnimations.sprintLeftClip;
                animatorController[CLIP_SPRINT_RIGHT] = sprintRightClip != null ? sprintRightClip : defaultAnimations.sprintRightClip;
                animatorController[CLIP_SPRINT_FORWARD_LEFT] = sprintForwardLeftClip != null ? sprintForwardLeftClip : defaultAnimations.sprintForwardLeftClip;
                animatorController[CLIP_SPRINT_FORWARD_RIGHT] = sprintForwardRightClip != null ? sprintForwardRightClip : defaultAnimations.sprintForwardRightClip;
                animatorController[CLIP_SPRINT_BACKWARD_LEFT] = sprintBackwardLeftClip != null ? sprintBackwardLeftClip : defaultAnimations.sprintBackwardLeftClip;
                animatorController[CLIP_SPRINT_BACKWARD_RIGHT] = sprintBackwardRightClip != null ? sprintBackwardRightClip : defaultAnimations.sprintBackwardRightClip;
                // Walk
                animatorController[CLIP_WALK] = walkClip != null ? walkClip : defaultAnimations.walkClip;
                animatorController[CLIP_WALK_BACKWARD] = walkBackwardClip != null ? walkBackwardClip : defaultAnimations.walkBackwardClip;
                animatorController[CLIP_WALK_LEFT] = walkLeftClip != null ? walkLeftClip : defaultAnimations.walkLeftClip;
                animatorController[CLIP_WALK_RIGHT] = walkRightClip != null ? walkRightClip : defaultAnimations.walkRightClip;
                animatorController[CLIP_WALK_FORWARD_LEFT] = walkForwardLeftClip != null ? walkForwardLeftClip : defaultAnimations.walkForwardLeftClip;
                animatorController[CLIP_WALK_FORWARD_RIGHT] = walkForwardRightClip != null ? walkForwardRightClip : defaultAnimations.walkForwardRightClip;
                animatorController[CLIP_WALK_BACKWARD_LEFT] = walkBackwardLeftClip != null ? walkBackwardLeftClip : defaultAnimations.walkBackwardLeftClip;
                animatorController[CLIP_WALK_BACKWARD_RIGHT] = walkBackwardRightClip != null ? walkBackwardRightClip : defaultAnimations.walkBackwardRightClip;
                // Crouch
                animatorController[CLIP_CROUCH_IDLE] = crouchIdleClip != null ? crouchIdleClip : defaultAnimations.crouchIdleClip;
                animatorController[CLIP_CROUCH_MOVE] = crouchMoveClip != null ? crouchMoveClip : defaultAnimations.crouchMoveClip;
                animatorController[CLIP_CROUCH_MOVE_BACKWARD] = crouchMoveBackwardClip != null ? crouchMoveBackwardClip : defaultAnimations.crouchMoveBackwardClip;
                animatorController[CLIP_CROUCH_MOVE_LEFT] = crouchMoveLeftClip != null ? crouchMoveLeftClip : defaultAnimations.crouchMoveLeftClip;
                animatorController[CLIP_CROUCH_MOVE_RIGHT] = crouchMoveRightClip != null ? crouchMoveRightClip : defaultAnimations.crouchMoveRightClip;
                animatorController[CLIP_CROUCH_MOVE_FORWARD_LEFT] = crouchMoveForwardLeftClip != null ? crouchMoveForwardLeftClip : defaultAnimations.crouchMoveForwardLeftClip;
                animatorController[CLIP_CROUCH_MOVE_FORWARD_RIGHT] = crouchMoveForwardRightClip != null ? crouchMoveForwardRightClip : defaultAnimations.crouchMoveForwardRightClip;
                animatorController[CLIP_CROUCH_MOVE_BACKWARD_LEFT] = crouchMoveBackwardLeftClip != null ? crouchMoveBackwardLeftClip : defaultAnimations.crouchMoveBackwardLeftClip;
                animatorController[CLIP_CROUCH_MOVE_BACKWARD_RIGHT] = crouchMoveBackwardRightClip != null ? crouchMoveBackwardRightClip : defaultAnimations.crouchMoveBackwardRightClip;
                // Crawl
                animatorController[CLIP_CRAWL_IDLE] = crawlIdleClip != null ? crawlIdleClip : defaultAnimations.crawlIdleClip;
                animatorController[CLIP_CRAWL_MOVE] = crawlMoveClip != null ? crawlMoveClip : defaultAnimations.crawlMoveClip;
                animatorController[CLIP_CRAWL_MOVE_BACKWARD] = crawlMoveBackwardClip != null ? crawlMoveBackwardClip : defaultAnimations.crawlMoveBackwardClip;
                animatorController[CLIP_CRAWL_MOVE_LEFT] = crawlMoveLeftClip != null ? crawlMoveLeftClip : defaultAnimations.crawlMoveLeftClip;
                animatorController[CLIP_CRAWL_MOVE_RIGHT] = crawlMoveRightClip != null ? crawlMoveRightClip : defaultAnimations.crawlMoveRightClip;
                animatorController[CLIP_CRAWL_MOVE_FORWARD_LEFT] = crawlMoveForwardLeftClip != null ? crawlMoveForwardLeftClip : defaultAnimations.crawlMoveForwardLeftClip;
                animatorController[CLIP_CRAWL_MOVE_FORWARD_RIGHT] = crawlMoveForwardRightClip != null ? crawlMoveForwardRightClip : defaultAnimations.crawlMoveForwardRightClip;
                animatorController[CLIP_CRAWL_MOVE_BACKWARD_LEFT] = crawlMoveBackwardLeftClip != null ? crawlMoveBackwardLeftClip : defaultAnimations.crawlMoveBackwardLeftClip;
                animatorController[CLIP_CRAWL_MOVE_BACKWARD_RIGHT] = crawlMoveBackwardRightClip != null ? crawlMoveBackwardRightClip : defaultAnimations.crawlMoveBackwardRightClip;
                // Swim
                animatorController[CLIP_SWIM_IDLE] = swimIdleClip != null ? swimIdleClip : defaultAnimations.swimIdleClip;
                animatorController[CLIP_SWIM_MOVE] = swimMoveClip != null ? swimMoveClip : defaultAnimations.swimMoveClip;
                animatorController[CLIP_SWIM_MOVE_BACKWARD] = swimMoveBackwardClip != null ? swimMoveBackwardClip : defaultAnimations.swimMoveBackwardClip;
                animatorController[CLIP_SWIM_MOVE_LEFT] = swimMoveLeftClip != null ? swimMoveLeftClip : defaultAnimations.swimMoveLeftClip;
                animatorController[CLIP_SWIM_MOVE_RIGHT] = swimMoveRightClip != null ? swimMoveRightClip : defaultAnimations.swimMoveRightClip;
                animatorController[CLIP_SWIM_MOVE_FORWARD_LEFT] = swimMoveForwardLeftClip != null ? swimMoveForwardLeftClip : defaultAnimations.swimMoveForwardLeftClip;
                animatorController[CLIP_SWIM_MOVE_FORWARD_RIGHT] = swimMoveForwardRightClip != null ? swimMoveForwardRightClip : defaultAnimations.swimMoveForwardRightClip;
                animatorController[CLIP_SWIM_MOVE_BACKWARD_LEFT] = swimMoveBackwardLeftClip != null ? swimMoveBackwardLeftClip : defaultAnimations.swimMoveBackwardLeftClip;
                animatorController[CLIP_SWIM_MOVE_BACKWARD_RIGHT] = swimMoveBackwardRightClip != null ? swimMoveBackwardRightClip : defaultAnimations.swimMoveBackwardRightClip;
                // Other
                animatorController[CLIP_JUMP] = jumpClip != null ? jumpClip : defaultAnimations.jumpClip;
                animatorController[CLIP_FALL] = fallClip != null ? fallClip : defaultAnimations.fallClip;
                animatorController[CLIP_LANDED] = landedClip != null ? landedClip : defaultAnimations.landedClip;
                animatorController[CLIP_HURT] = hurtClip != null ? hurtClip : defaultAnimations.hurtClip;
                animatorController[CLIP_DEAD] = deadClip != null ? deadClip : defaultAnimations.deadClip;
                animatorController[CLIP_PICKUP] = pickupClip != null ? pickupClip : defaultAnimations.pickupClip;

                return animatorController;
            }
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
        public static readonly int ANIM_IS_WEAPON_CHARGE = Animator.StringToHash("IsWeaponCharge");
        public static readonly int ANIM_HURT = Animator.StringToHash("Hurt");
        public static readonly int ANIM_JUMP = Animator.StringToHash("Jump");
        public static readonly int ANIM_PICKUP = Animator.StringToHash("Pickup");
        public static readonly int ANIM_MOVE_CLIP_MULTIPLIER = Animator.StringToHash("MoveSpeedMultiplier");
        public static readonly int ANIM_ACTION_CLIP_MULTIPLIER = Animator.StringToHash("ActionSpeedMultiplier");
        public static readonly int ANIM_HURT_CLIP_MULTIPLIER = Animator.StringToHash("HurtSpeedMultiplier");
        public static readonly int ANIM_DEAD_CLIP_MULTIPLIER = Animator.StringToHash("DeadSpeedMultiplier");
        public static readonly int ANIM_JUMP_CLIP_MULTIPLIER = Animator.StringToHash("JumpSpeedMultiplier");
        public static readonly int ANIM_FALL_CLIP_MULTIPLIER = Animator.StringToHash("FallSpeedMultiplier");
        public static readonly int ANIM_LANDED_CLIP_MULTIPLIER = Animator.StringToHash("LandedSpeedMultiplier");
        public static readonly int ANIM_PICKUP_CLIP_MULTIPLIER = Animator.StringToHash("PickupSpeedMultiplier");
        public static readonly int ANIM_MOVE_TYPE = Animator.StringToHash("MoveType");
        public static readonly Dictionary<int, CacheAnimatorController> CacheAnimatorControllers = new Dictionary<int, CacheAnimatorController>();

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

        public AnimatorOverrideController CurrentAnimatorController
        {
            get { return animator.runtimeAnimatorController as AnimatorOverrideController; }
            private set { animator.runtimeAnimatorController = value; }
        }
        private int[] actionStateNameHashes;
        private int[] castSkillStateNameHashes;
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
        private float landedAnimSpeedRate;
        private float hurtAnimSpeedRate;
        private float deadAnimSpeedRate;
        private float pickupAnimSpeedRate;

        protected override void Awake()
        {
            base.Awake();
            SetupComponent();
        }

        protected void SetCacheAnimatorController(int id, RuntimeAnimatorController animatorController, DefaultAnimations defaultAnimations, WeaponAnimations[] weaponAnimations)
        {
            CacheAnimatorControllers[id] = new CacheAnimatorController(id, animatorController, defaultAnimations, weaponAnimations);
        }

        protected CacheAnimatorController GetCacheAnimatorController(int id)
        {
            return CacheAnimatorControllers[id];
        }

        protected CacheAnimatorController SetAndGetCacheAnimatorController(int id, RuntimeAnimatorController animatorController, DefaultAnimations defaultAnimations, WeaponAnimations[] weaponAnimations)
        {
            if (!CacheAnimatorControllers.ContainsKey(id))
                SetCacheAnimatorController(id, animatorController, defaultAnimations, weaponAnimations);
            return GetCacheAnimatorController(id);
        }

        protected override void OnValidate()
        {
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
            base.OnValidate();
        }

        private void SetupComponent()
        {
            if (isSetupComponent)
                return;
            isSetupComponent = true;
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
            CurrentAnimatorController = SetAndGetCacheAnimatorController(Id, animatorController, defaultAnimations, weaponAnimations).Default;
            SetupSpeedRates();
            base.SetDefaultAnimations();
        }

        private void SetupSpeedRates(
            float idleAnimSpeedRate = 0f,
            float moveAnimSpeedRate = 0f,
            float sprintAnimSpeedRate = 0f,
            float walkAnimSpeedRate = 0f,
            float crouchIdleAnimSpeedRate = 0f,
            float crouchMoveAnimSpeedRate = 0f,
            float crawlIdleAnimSpeedRate = 0f,
            float crawlMoveAnimSpeedRate = 0f,
            float swimIdleAnimSpeedRate = 0f,
            float swimMoveAnimSpeedRate = 0f,
            float jumpAnimSpeedRate = 0f,
            float fallAnimSpeedRate = 0f,
            float landedAnimSpeedRate = 0f,
            float hurtAnimSpeedRate = 0f,
            float deadAnimSpeedRate = 0f,
            float pickupAnimSpeedRate = 0f)
        {
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
            this.landedAnimSpeedRate = landedAnimSpeedRate > 0f ? landedAnimSpeedRate :
                defaultAnimations.landedAnimSpeedRate > 0f ? defaultAnimations.landedAnimSpeedRate : 1f;
            this.hurtAnimSpeedRate = hurtAnimSpeedRate > 0f ? hurtAnimSpeedRate :
                defaultAnimations.hurtAnimSpeedRate > 0f ? defaultAnimations.hurtAnimSpeedRate : 1f;
            this.deadAnimSpeedRate = deadAnimSpeedRate > 0f ? deadAnimSpeedRate :
                defaultAnimations.deadAnimSpeedRate > 0f ? defaultAnimations.deadAnimSpeedRate : 1f;
            this.pickupAnimSpeedRate = pickupAnimSpeedRate > 0f ? pickupAnimSpeedRate :
                defaultAnimations.pickupAnimSpeedRate > 0f ? defaultAnimations.pickupAnimSpeedRate : 1f;
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
            CacheAnimatorController cacheAnimatorController = SetAndGetCacheAnimatorController(Id, animatorController, defaultAnimations, weaponAnimations);
            WeaponAnimations currentWeaponAnimations;
            if (weaponType == null ||
                !cacheAnimatorController.Weapons.ContainsKey(weaponType.DataId) ||
                !TryGetWeaponAnimations(weaponType.DataId, out currentWeaponAnimations))
            {
                CurrentAnimatorController = cacheAnimatorController.Default;
                SetupSpeedRates();
            }
            else
            {
                CurrentAnimatorController = cacheAnimatorController.Weapons[weaponType.DataId];
                SetupSpeedRates(
                    currentWeaponAnimations.idleAnimSpeedRate,
                    currentWeaponAnimations.moveAnimSpeedRate,
                    currentWeaponAnimations.sprintAnimSpeedRate,
                    currentWeaponAnimations.walkAnimSpeedRate,
                    currentWeaponAnimations.crouchIdleAnimSpeedRate,
                    currentWeaponAnimations.crouchMoveAnimSpeedRate,
                    currentWeaponAnimations.crawlIdleAnimSpeedRate,
                    currentWeaponAnimations.crawlMoveAnimSpeedRate,
                    currentWeaponAnimations.swimIdleAnimSpeedRate,
                    currentWeaponAnimations.swimMoveAnimSpeedRate,
                    currentWeaponAnimations.jumpAnimSpeedRate,
                    currentWeaponAnimations.fallAnimSpeedRate,
                    currentWeaponAnimations.landedAnimSpeedRate,
                    currentWeaponAnimations.hurtAnimSpeedRate,
                    currentWeaponAnimations.deadAnimSpeedRate,
                    currentWeaponAnimations.pickupAnimSpeedRate);
            }
        }

        public override void PlayMoveAnimation()
        {
            if (!animator.isActiveAndEnabled)
                return;

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
            bool isUnderWater = movementState.HasFlag(MovementState.IsUnderWater);
            bool isGrounded = !isUnderWater && movementState.HasFlag(MovementState.IsGrounded);
            animator.SetFloat(ANIM_MOVE_SPEED, isDead ? 0 : moveSpeed, movementDampingTme, deltaTime);
            animator.SetFloat(ANIM_SIDE_MOVE_SPEED, isDead ? 0 : sideMoveSpeed, movementDampingTme, deltaTime);
            animator.SetFloat(ANIM_MOVE_CLIP_MULTIPLIER, moveAnimationSpeedMultiplier);
            animator.SetFloat(ANIM_HURT_CLIP_MULTIPLIER, hurtAnimSpeedRate);
            animator.SetFloat(ANIM_DEAD_CLIP_MULTIPLIER, deadAnimSpeedRate);
            animator.SetFloat(ANIM_JUMP_CLIP_MULTIPLIER, jumpAnimSpeedRate);
            animator.SetFloat(ANIM_FALL_CLIP_MULTIPLIER, fallAnimSpeedRate);
            animator.SetFloat(ANIM_LANDED_CLIP_MULTIPLIER, landedAnimSpeedRate);
            animator.SetFloat(ANIM_PICKUP_CLIP_MULTIPLIER, pickupAnimSpeedRate);
            animator.SetBool(ANIM_IS_DEAD, isDead);
            animator.SetBool(ANIM_IS_GROUNDED, isGrounded);
            animator.SetBool(ANIM_IS_UNDER_WATER, isUnderWater);
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
            AudioManager.PlaySfxClipAtAudioSource(tempActionAnimation.GetRandomAudioClip(), genericAudioSource);
            bool hasClip = tempActionAnimation.clip != null && animator.isActiveAndEnabled;
            if (hasClip)
            {
                CurrentAnimatorController[CLIP_ACTION] = tempActionAnimation.clip;
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
                CurrentAnimatorController[CLIP_CAST_SKILL] = castClip;
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
            AnimationClip chargeClip = isLeftHand ? GetLeftHandWeaponChargeClip(dataId) : GetRightHandWeaponChargeClip(dataId);
            bool hasClip = chargeClip != null && animator.isActiveAndEnabled;
            if (hasClip)
            {
                CurrentAnimatorController[CLIP_WEAPON_CHARGE] = chargeClip;
                animator.SetBool(ANIM_IS_WEAPON_CHARGE, true);
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
                animator.SetBool(ANIM_IS_WEAPON_CHARGE, false);
            }
        }

        public override void PlayHitAnimation()
        {
            if (!CurrentAnimatorController[CLIP_HURT])
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
            if (!CurrentAnimatorController[CLIP_JUMP])
                return 0f;
            return CurrentAnimatorController[CLIP_JUMP].length / jumpAnimSpeedRate;
        }

        public override void PlayJumpAnimation()
        {
            if (!CurrentAnimatorController[CLIP_JUMP])
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
            if (!CurrentAnimatorController[CLIP_PICKUP])
                return;
            StartCoroutine(PlayPickupAnimationRoutine());
        }

        IEnumerator PlayPickupAnimationRoutine()
        {
            yield return null;
            if (animator.isActiveAndEnabled)
            {
                animator.ResetTrigger(ANIM_PICKUP);
                animator.SetTrigger(ANIM_PICKUP);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Set Animator Clips For Test", false, 1000501)]
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
            CurrentAnimatorController[CLIP_ACTION] = tempActionAnimation.clip;

            // Skill animation clips
            AnimationClip castClip = GetSkillCastClip(testCastSkillAnimDataId);
            CurrentAnimatorController[CLIP_CAST_SKILL] = castClip;

            Logging.Log(ToString(), "Animation Clips already set to animator controller, you can test an animations in Animation tab");

            this.InvokeInstanceDevExtMethods("SetAnimatorClipsForTest");
        }
#endif
    }
}
