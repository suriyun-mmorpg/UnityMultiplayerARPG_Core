using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public abstract class BaseRemakeCharacterModel : BaseCharacterModel
    {
        // Clip name variables
        // Move
        public const string CLIP_IDLE = "__Idle";
        public const string CLIP_MOVE = "__MoveForward";
        public const string CLIP_MOVE_BACKWARD = "__MoveBackward";
        public const string CLIP_MOVE_LEFT = "__MoveLeft";
        public const string CLIP_MOVE_RIGHT = "__MoveRight";
        public const string CLIP_MOVE_FORWARD_LEFT = "__MoveForwardLeft";
        public const string CLIP_MOVE_FORWARD_RIGHT = "__MoveForwardRight";
        public const string CLIP_MOVE_BACKWARD_LEFT = "__MoveBackwardLeft";
        public const string CLIP_MOVE_BACKWARD_RIGHT = "__MoveBackwardRight";
        // Sprint
        public const string CLIP_SPRINT = "__SprintForward";
        public const string CLIP_SPRINT_BACKWARD = "__SprintBackward";
        public const string CLIP_SPRINT_LEFT = "__SprintLeft";
        public const string CLIP_SPRINT_RIGHT = "__SprintRight";
        public const string CLIP_SPRINT_FORWARD_LEFT = "__SprintForwardLeft";
        public const string CLIP_SPRINT_FORWARD_RIGHT = "__SprintForwardRight";
        public const string CLIP_SPRINT_BACKWARD_LEFT = "__SprintBackwardLeft";
        public const string CLIP_SPRINT_BACKWARD_RIGHT = "__SprintBackwardRight";
        // Walk
        public const string CLIP_WALK = "__WalkForward";
        public const string CLIP_WALK_BACKWARD = "__WalkBackward";
        public const string CLIP_WALK_LEFT = "__WalkLeft";
        public const string CLIP_WALK_RIGHT = "__WalkRight";
        public const string CLIP_WALK_FORWARD_LEFT = "__WalkForwardLeft";
        public const string CLIP_WALK_FORWARD_RIGHT = "__WalkForwardRight";
        public const string CLIP_WALK_BACKWARD_LEFT = "__WalkBackwardLeft";
        public const string CLIP_WALK_BACKWARD_RIGHT = "__WalkBackwardRight";
        // Crouch
        public const string CLIP_CROUCH_IDLE = "__CrouchIdle";
        public const string CLIP_CROUCH_MOVE = "__CrouchMoveForward";
        public const string CLIP_CROUCH_MOVE_BACKWARD = "__CrouchMoveBackward";
        public const string CLIP_CROUCH_MOVE_LEFT = "__CrouchMoveLeft";
        public const string CLIP_CROUCH_MOVE_RIGHT = "__CrouchMoveRight";
        public const string CLIP_CROUCH_MOVE_FORWARD_LEFT = "__CrouchMoveForwardLeft";
        public const string CLIP_CROUCH_MOVE_FORWARD_RIGHT = "__CrouchMoveForwardRight";
        public const string CLIP_CROUCH_MOVE_BACKWARD_LEFT = "__CrouchMoveBackwardLeft";
        public const string CLIP_CROUCH_MOVE_BACKWARD_RIGHT = "__CrouchMoveBackwardRight";
        // Crawl
        public const string CLIP_CRAWL_IDLE = "__CrawlIdle";
        public const string CLIP_CRAWL_MOVE = "__CrawlMoveForward";
        public const string CLIP_CRAWL_MOVE_BACKWARD = "__CrawlMoveBackward";
        public const string CLIP_CRAWL_MOVE_LEFT = "__CrawlMoveLeft";
        public const string CLIP_CRAWL_MOVE_RIGHT = "__CrawlMoveRight";
        public const string CLIP_CRAWL_MOVE_FORWARD_LEFT = "__CrawlMoveForwardLeft";
        public const string CLIP_CRAWL_MOVE_FORWARD_RIGHT = "__CrawlMoveForwardRight";
        public const string CLIP_CRAWL_MOVE_BACKWARD_LEFT = "__CrawlMoveBackwardLeft";
        public const string CLIP_CRAWL_MOVE_BACKWARD_RIGHT = "__CrawlMoveBackwardRight";
        // Swim
        public const string CLIP_SWIM_IDLE = "__SwimIdle";
        public const string CLIP_SWIM_MOVE = "__SwimMoveForward";
        public const string CLIP_SWIM_MOVE_BACKWARD = "__SwimMoveBackward";
        public const string CLIP_SWIM_MOVE_LEFT = "__SwimMoveLeft";
        public const string CLIP_SWIM_MOVE_RIGHT = "__SwimMoveRight";
        public const string CLIP_SWIM_MOVE_FORWARD_LEFT = "__SwimMoveForwardLeft";
        public const string CLIP_SWIM_MOVE_FORWARD_RIGHT = "__SwimMoveForwardRight";
        public const string CLIP_SWIM_MOVE_BACKWARD_LEFT = "__SwimMoveBackwardLeft";
        public const string CLIP_SWIM_MOVE_BACKWARD_RIGHT = "__SwimMoveBackwardRight";
        // Other
        public const string CLIP_JUMP = "__Jump";
        public const string CLIP_FALL = "__Fall";
        public const string CLIP_LANDED = "__Landed";
        public const string CLIP_HURT = "__Hurt";
        public const string CLIP_DEAD = "__Dead";
        public const string CLIP_ACTION = "__Action";
        public const string CLIP_CAST_SKILL = "__CastSkill";
        public const string CLIP_WEAPON_CHARGE = "__WeaponCharge";
        public const string CLIP_PICKUP = "__Pickup";

        [Header("Renderer")]
        [Tooltip("This will be used to apply bone weights when equip an equipments")]
        public SkinnedMeshRenderer skinnedMeshRenderer;

        [Header("Animations")]
        public DefaultAnimations defaultAnimations;
        [ArrayElementTitle("weaponType")]
        public WeaponAnimations[] weaponAnimations;
        [ArrayElementTitle("skill")]
        public SkillAnimations[] skillAnimations;

        /// <summary>
        /// Coroutine for attack, use skill and other animations
        /// </summary>
        protected Coroutine actionCoroutine;
        protected bool isDoingAction = false;

        protected override void Awake()
        {
            PrepareMissingMovementAnimations();
            base.Awake();
        }

        public bool TryGetWeaponAnimations(int dataId, out WeaponAnimations anims)
        {
            return CacheAnimationsManager.SetAndTryGetCacheWeaponAnimations(Id, weaponAnimations, skillAnimations, dataId, out anims);
        }

        public bool TryGetSkillAnimations(int dataId, out SkillAnimations anims)
        {
            return CacheAnimationsManager.SetAndTryGetCacheSkillAnimations(Id, weaponAnimations, skillAnimations, dataId, out anims);
        }

        protected Coroutine StartedActionCoroutine(Coroutine coroutine)
        {
            StopActionCoroutine();
            actionCoroutine = coroutine;
            return actionCoroutine;
        }

        protected void StopActionCoroutine()
        {
            if (actionCoroutine != null)
                StopCoroutine(actionCoroutine);
        }

        public void PrepareMissingMovementAnimations()
        {
            DefaultAnimations tempDefaultAnimations = defaultAnimations;
            // Move
            if (tempDefaultAnimations.moveForwardLeftClip == null)
                tempDefaultAnimations.moveForwardLeftClip = tempDefaultAnimations.moveClip;
            if (tempDefaultAnimations.moveForwardRightClip == null)
                tempDefaultAnimations.moveForwardRightClip = tempDefaultAnimations.moveClip;
            if (tempDefaultAnimations.moveLeftClip == null)
                tempDefaultAnimations.moveLeftClip = tempDefaultAnimations.moveClip;
            if (tempDefaultAnimations.moveRightClip == null)
                tempDefaultAnimations.moveRightClip = tempDefaultAnimations.moveClip;
            if (tempDefaultAnimations.moveBackwardClip == null)
                tempDefaultAnimations.moveBackwardClip = tempDefaultAnimations.moveClip;
            if (tempDefaultAnimations.moveBackwardLeftClip == null)
                tempDefaultAnimations.moveBackwardLeftClip = tempDefaultAnimations.moveBackwardClip;
            if (tempDefaultAnimations.moveBackwardRightClip == null)
                tempDefaultAnimations.moveBackwardRightClip = tempDefaultAnimations.moveBackwardClip;
            // Sprint
            if (tempDefaultAnimations.sprintClip == null)
                tempDefaultAnimations.sprintClip = tempDefaultAnimations.moveClip;
            if (tempDefaultAnimations.sprintForwardLeftClip == null)
                tempDefaultAnimations.sprintForwardLeftClip = tempDefaultAnimations.sprintClip;
            if (tempDefaultAnimations.sprintForwardRightClip == null)
                tempDefaultAnimations.sprintForwardRightClip = tempDefaultAnimations.sprintClip;
            if (tempDefaultAnimations.sprintLeftClip == null)
                tempDefaultAnimations.sprintLeftClip = tempDefaultAnimations.sprintClip;
            if (tempDefaultAnimations.sprintRightClip == null)
                tempDefaultAnimations.sprintRightClip = tempDefaultAnimations.sprintClip;
            if (tempDefaultAnimations.sprintBackwardClip == null)
                tempDefaultAnimations.sprintBackwardClip = tempDefaultAnimations.sprintClip;
            if (tempDefaultAnimations.sprintBackwardLeftClip == null)
                tempDefaultAnimations.sprintBackwardLeftClip = tempDefaultAnimations.sprintBackwardClip;
            if (tempDefaultAnimations.sprintBackwardRightClip == null)
                tempDefaultAnimations.sprintBackwardRightClip = tempDefaultAnimations.sprintBackwardClip;
            // Walk
            if (tempDefaultAnimations.walkClip == null)
                tempDefaultAnimations.walkClip = tempDefaultAnimations.moveClip;
            if (tempDefaultAnimations.walkForwardLeftClip == null)
                tempDefaultAnimations.walkForwardLeftClip = tempDefaultAnimations.walkClip;
            if (tempDefaultAnimations.walkForwardRightClip == null)
                tempDefaultAnimations.walkForwardRightClip = tempDefaultAnimations.walkClip;
            if (tempDefaultAnimations.walkLeftClip == null)
                tempDefaultAnimations.walkLeftClip = tempDefaultAnimations.walkClip;
            if (tempDefaultAnimations.walkRightClip == null)
                tempDefaultAnimations.walkRightClip = tempDefaultAnimations.walkClip;
            if (tempDefaultAnimations.walkBackwardClip == null)
                tempDefaultAnimations.walkBackwardClip = tempDefaultAnimations.walkClip;
            if (tempDefaultAnimations.walkBackwardLeftClip == null)
                tempDefaultAnimations.walkBackwardLeftClip = tempDefaultAnimations.walkBackwardClip;
            if (tempDefaultAnimations.walkBackwardRightClip == null)
                tempDefaultAnimations.walkBackwardRightClip = tempDefaultAnimations.walkBackwardClip;
            // Crouch
            if (tempDefaultAnimations.crouchIdleClip == null)
                tempDefaultAnimations.crouchIdleClip = tempDefaultAnimations.idleClip;
            if (tempDefaultAnimations.crouchMoveClip == null)
                tempDefaultAnimations.crouchMoveClip = tempDefaultAnimations.moveClip;
            if (tempDefaultAnimations.crouchMoveForwardLeftClip == null)
                tempDefaultAnimations.crouchMoveForwardLeftClip = tempDefaultAnimations.crouchMoveClip;
            if (tempDefaultAnimations.crouchMoveForwardRightClip == null)
                tempDefaultAnimations.crouchMoveForwardRightClip = tempDefaultAnimations.crouchMoveClip;
            if (tempDefaultAnimations.crouchMoveLeftClip == null)
                tempDefaultAnimations.crouchMoveLeftClip = tempDefaultAnimations.crouchMoveClip;
            if (tempDefaultAnimations.crouchMoveRightClip == null)
                tempDefaultAnimations.crouchMoveRightClip = tempDefaultAnimations.crouchMoveClip;
            if (tempDefaultAnimations.crouchMoveBackwardClip == null)
                tempDefaultAnimations.crouchMoveBackwardClip = tempDefaultAnimations.crouchMoveClip;
            if (tempDefaultAnimations.crouchMoveBackwardLeftClip == null)
                tempDefaultAnimations.crouchMoveBackwardLeftClip = tempDefaultAnimations.crouchMoveBackwardClip;
            if (tempDefaultAnimations.crouchMoveBackwardRightClip == null)
                tempDefaultAnimations.crouchMoveBackwardRightClip = tempDefaultAnimations.crouchMoveBackwardClip;
            // Crawl
            if (tempDefaultAnimations.crawlIdleClip == null)
                tempDefaultAnimations.crawlIdleClip = tempDefaultAnimations.idleClip;
            if (tempDefaultAnimations.crawlMoveClip == null)
                tempDefaultAnimations.crawlMoveClip = tempDefaultAnimations.moveClip;
            if (tempDefaultAnimations.crawlMoveForwardLeftClip == null)
                tempDefaultAnimations.crawlMoveForwardLeftClip = tempDefaultAnimations.crawlMoveClip;
            if (tempDefaultAnimations.crawlMoveForwardRightClip == null)
                tempDefaultAnimations.crawlMoveForwardRightClip = tempDefaultAnimations.crawlMoveClip;
            if (tempDefaultAnimations.crawlMoveLeftClip == null)
                tempDefaultAnimations.crawlMoveLeftClip = tempDefaultAnimations.crawlMoveClip;
            if (tempDefaultAnimations.crawlMoveRightClip == null)
                tempDefaultAnimations.crawlMoveRightClip = tempDefaultAnimations.crawlMoveClip;
            if (tempDefaultAnimations.crawlMoveBackwardClip == null)
                tempDefaultAnimations.crawlMoveBackwardClip = tempDefaultAnimations.crawlMoveClip;
            if (tempDefaultAnimations.crawlMoveBackwardLeftClip == null)
                tempDefaultAnimations.crawlMoveBackwardLeftClip = tempDefaultAnimations.crawlMoveBackwardClip;
            if (tempDefaultAnimations.crawlMoveBackwardRightClip == null)
                tempDefaultAnimations.crawlMoveBackwardRightClip = tempDefaultAnimations.crawlMoveBackwardClip;
            // Swim
            if (tempDefaultAnimations.swimIdleClip == null)
                tempDefaultAnimations.swimIdleClip = tempDefaultAnimations.idleClip;
            if (tempDefaultAnimations.swimMoveClip == null)
                tempDefaultAnimations.swimMoveClip = tempDefaultAnimations.moveClip;
            if (tempDefaultAnimations.swimMoveForwardLeftClip == null)
                tempDefaultAnimations.swimMoveForwardLeftClip = tempDefaultAnimations.swimMoveClip;
            if (tempDefaultAnimations.swimMoveForwardRightClip == null)
                tempDefaultAnimations.swimMoveForwardRightClip = tempDefaultAnimations.swimMoveClip;
            if (tempDefaultAnimations.swimMoveLeftClip == null)
                tempDefaultAnimations.swimMoveLeftClip = tempDefaultAnimations.swimMoveClip;
            if (tempDefaultAnimations.swimMoveRightClip == null)
                tempDefaultAnimations.swimMoveRightClip = tempDefaultAnimations.swimMoveClip;
            if (tempDefaultAnimations.swimMoveBackwardClip == null)
                tempDefaultAnimations.swimMoveBackwardClip = tempDefaultAnimations.swimMoveClip;
            if (tempDefaultAnimations.swimMoveBackwardLeftClip == null)
                tempDefaultAnimations.swimMoveBackwardLeftClip = tempDefaultAnimations.swimMoveBackwardClip;
            if (tempDefaultAnimations.swimMoveBackwardRightClip == null)
                tempDefaultAnimations.swimMoveBackwardRightClip = tempDefaultAnimations.swimMoveBackwardClip;
            // Apply
            defaultAnimations = tempDefaultAnimations;

            // Weapon Animations
            if (weaponAnimations != null && weaponAnimations.Length > 0)
            {
                WeaponAnimations tempWeaponAnimations;
                for (int i = 0; i < weaponAnimations.Length; ++i)
                {
                    tempWeaponAnimations = weaponAnimations[i];
                    // Move
                    if (tempWeaponAnimations.moveForwardLeftClip == null)
                        tempWeaponAnimations.moveForwardLeftClip = tempWeaponAnimations.moveClip;
                    if (tempWeaponAnimations.moveForwardRightClip == null)
                        tempWeaponAnimations.moveForwardRightClip = tempWeaponAnimations.moveClip;
                    if (tempWeaponAnimations.moveLeftClip == null)
                        tempWeaponAnimations.moveLeftClip = tempWeaponAnimations.moveClip;
                    if (tempWeaponAnimations.moveRightClip == null)
                        tempWeaponAnimations.moveRightClip = tempWeaponAnimations.moveClip;
                    if (tempWeaponAnimations.moveBackwardClip == null)
                        tempWeaponAnimations.moveBackwardClip = tempWeaponAnimations.moveClip;
                    if (tempWeaponAnimations.moveBackwardLeftClip == null)
                        tempWeaponAnimations.moveBackwardLeftClip = tempWeaponAnimations.moveBackwardClip;
                    if (tempWeaponAnimations.moveBackwardRightClip == null)
                        tempWeaponAnimations.moveBackwardRightClip = tempWeaponAnimations.moveBackwardClip;
                    // Sprint
                    if (tempWeaponAnimations.sprintClip == null)
                        tempWeaponAnimations.sprintClip = tempWeaponAnimations.moveClip;
                    if (tempWeaponAnimations.sprintForwardLeftClip == null)
                        tempWeaponAnimations.sprintForwardLeftClip = tempWeaponAnimations.sprintClip;
                    if (tempWeaponAnimations.sprintForwardRightClip == null)
                        tempWeaponAnimations.sprintForwardRightClip = tempWeaponAnimations.sprintClip;
                    if (tempWeaponAnimations.sprintLeftClip == null)
                        tempWeaponAnimations.sprintLeftClip = tempWeaponAnimations.sprintClip;
                    if (tempWeaponAnimations.sprintRightClip == null)
                        tempWeaponAnimations.sprintRightClip = tempWeaponAnimations.sprintClip;
                    if (tempWeaponAnimations.sprintBackwardClip == null)
                        tempWeaponAnimations.sprintBackwardClip = tempWeaponAnimations.sprintClip;
                    if (tempWeaponAnimations.sprintBackwardLeftClip == null)
                        tempWeaponAnimations.sprintBackwardLeftClip = tempWeaponAnimations.sprintBackwardClip;
                    if (tempWeaponAnimations.sprintBackwardRightClip == null)
                        tempWeaponAnimations.sprintBackwardRightClip = tempWeaponAnimations.sprintBackwardClip;
                    // Walk
                    if (tempWeaponAnimations.walkClip == null)
                        tempWeaponAnimations.walkClip = tempWeaponAnimations.moveClip;
                    if (tempWeaponAnimations.walkForwardLeftClip == null)
                        tempWeaponAnimations.walkForwardLeftClip = tempWeaponAnimations.walkClip;
                    if (tempWeaponAnimations.walkForwardRightClip == null)
                        tempWeaponAnimations.walkForwardRightClip = tempWeaponAnimations.walkClip;
                    if (tempWeaponAnimations.walkLeftClip == null)
                        tempWeaponAnimations.walkLeftClip = tempWeaponAnimations.walkClip;
                    if (tempWeaponAnimations.walkRightClip == null)
                        tempWeaponAnimations.walkRightClip = tempWeaponAnimations.walkClip;
                    if (tempWeaponAnimations.walkBackwardClip == null)
                        tempWeaponAnimations.walkBackwardClip = tempWeaponAnimations.walkClip;
                    if (tempWeaponAnimations.walkBackwardLeftClip == null)
                        tempWeaponAnimations.walkBackwardLeftClip = tempWeaponAnimations.walkBackwardClip;
                    if (tempWeaponAnimations.walkBackwardRightClip == null)
                        tempWeaponAnimations.walkBackwardRightClip = tempWeaponAnimations.walkBackwardClip;
                    // Crouch
                    if (tempWeaponAnimations.crouchIdleClip == null)
                        tempWeaponAnimations.crouchIdleClip = tempWeaponAnimations.idleClip;
                    if (tempWeaponAnimations.crouchMoveClip == null)
                        tempWeaponAnimations.crouchMoveClip = tempWeaponAnimations.moveClip;
                    if (tempWeaponAnimations.crouchMoveForwardLeftClip == null)
                        tempWeaponAnimations.crouchMoveForwardLeftClip = tempWeaponAnimations.crouchMoveClip;
                    if (tempWeaponAnimations.crouchMoveForwardRightClip == null)
                        tempWeaponAnimations.crouchMoveForwardRightClip = tempWeaponAnimations.crouchMoveClip;
                    if (tempWeaponAnimations.crouchMoveLeftClip == null)
                        tempWeaponAnimations.crouchMoveLeftClip = tempWeaponAnimations.crouchMoveClip;
                    if (tempWeaponAnimations.crouchMoveRightClip == null)
                        tempWeaponAnimations.crouchMoveRightClip = tempWeaponAnimations.crouchMoveClip;
                    if (tempWeaponAnimations.crouchMoveBackwardClip == null)
                        tempWeaponAnimations.crouchMoveBackwardClip = tempWeaponAnimations.crouchMoveClip;
                    if (tempWeaponAnimations.crouchMoveBackwardLeftClip == null)
                        tempWeaponAnimations.crouchMoveBackwardLeftClip = tempWeaponAnimations.crouchMoveBackwardClip;
                    if (tempWeaponAnimations.crouchMoveBackwardRightClip == null)
                        tempWeaponAnimations.crouchMoveBackwardRightClip = tempWeaponAnimations.crouchMoveBackwardClip;
                    // Crawl
                    if (tempWeaponAnimations.crawlIdleClip == null)
                        tempWeaponAnimations.crawlIdleClip = tempWeaponAnimations.idleClip;
                    if (tempWeaponAnimations.crawlMoveClip == null)
                        tempWeaponAnimations.crawlMoveClip = tempWeaponAnimations.moveClip;
                    if (tempWeaponAnimations.crawlMoveForwardLeftClip == null)
                        tempWeaponAnimations.crawlMoveForwardLeftClip = tempWeaponAnimations.crawlMoveClip;
                    if (tempWeaponAnimations.crawlMoveForwardRightClip == null)
                        tempWeaponAnimations.crawlMoveForwardRightClip = tempWeaponAnimations.crawlMoveClip;
                    if (tempWeaponAnimations.crawlMoveLeftClip == null)
                        tempWeaponAnimations.crawlMoveLeftClip = tempWeaponAnimations.crawlMoveClip;
                    if (tempWeaponAnimations.crawlMoveRightClip == null)
                        tempWeaponAnimations.crawlMoveRightClip = tempWeaponAnimations.crawlMoveClip;
                    if (tempWeaponAnimations.crawlMoveBackwardClip == null)
                        tempWeaponAnimations.crawlMoveBackwardClip = tempWeaponAnimations.crawlMoveClip;
                    if (tempWeaponAnimations.crawlMoveBackwardLeftClip == null)
                        tempWeaponAnimations.crawlMoveBackwardLeftClip = tempWeaponAnimations.crawlMoveBackwardClip;
                    if (tempWeaponAnimations.crawlMoveBackwardRightClip == null)
                        tempWeaponAnimations.crawlMoveBackwardRightClip = tempWeaponAnimations.crawlMoveBackwardClip;
                    // Swim
                    if (tempWeaponAnimations.swimIdleClip == null)
                        tempWeaponAnimations.swimIdleClip = tempWeaponAnimations.idleClip;
                    if (tempWeaponAnimations.swimMoveClip == null)
                        tempWeaponAnimations.swimMoveClip = tempWeaponAnimations.moveClip;
                    if (tempWeaponAnimations.swimMoveForwardLeftClip == null)
                        tempWeaponAnimations.swimMoveForwardLeftClip = tempWeaponAnimations.swimMoveClip;
                    if (tempWeaponAnimations.swimMoveForwardRightClip == null)
                        tempWeaponAnimations.swimMoveForwardRightClip = tempWeaponAnimations.swimMoveClip;
                    if (tempWeaponAnimations.swimMoveLeftClip == null)
                        tempWeaponAnimations.swimMoveLeftClip = tempWeaponAnimations.swimMoveClip;
                    if (tempWeaponAnimations.swimMoveRightClip == null)
                        tempWeaponAnimations.swimMoveRightClip = tempWeaponAnimations.swimMoveClip;
                    if (tempWeaponAnimations.swimMoveBackwardClip == null)
                        tempWeaponAnimations.swimMoveBackwardClip = tempWeaponAnimations.swimMoveClip;
                    if (tempWeaponAnimations.swimMoveBackwardLeftClip == null)
                        tempWeaponAnimations.swimMoveBackwardLeftClip = tempWeaponAnimations.swimMoveBackwardClip;
                    if (tempWeaponAnimations.swimMoveBackwardRightClip == null)
                        tempWeaponAnimations.swimMoveBackwardRightClip = tempWeaponAnimations.swimMoveBackwardClip;
                    // Apply
                    weaponAnimations[i] = tempWeaponAnimations;
                }
            }
        }

        public override void AddingNewModel(GameObject newModel, EquipmentContainer equipmentContainer)
        {
            base.AddingNewModel(newModel, equipmentContainer);
            SkinnedMeshRenderer skinnedMesh = newModel.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMesh != null && skinnedMeshRenderer != null)
            {
                skinnedMesh.bones = skinnedMeshRenderer.bones;
                skinnedMesh.rootBone = skinnedMeshRenderer.rootBone;
                if (equipmentContainer.defaultModel != null)
                {
                    SkinnedMeshRenderer defaultSkinnedMesh = equipmentContainer.defaultModel.GetComponentInChildren<SkinnedMeshRenderer>();
                    if (defaultSkinnedMesh != null)
                    {
                        skinnedMesh.bones = defaultSkinnedMesh.bones;
                        skinnedMesh.rootBone = defaultSkinnedMesh.rootBone;
                    }
                }
            }
        }

        public ActionAnimation GetActionAnimation(AnimActionType animActionType, int dataId, int index)
        {
            ActionAnimation tempActionAnimation = default(ActionAnimation);
            switch (animActionType)
            {
                case AnimActionType.AttackRightHand:
                    ActionAnimation[] rightHandAnims = GetRightHandAttackAnimations(dataId);
                    if (index >= rightHandAnims.Length)
                        index = 0;
                    if (index < rightHandAnims.Length)
                        tempActionAnimation = rightHandAnims[index];
                    break;
                case AnimActionType.AttackLeftHand:
                    ActionAnimation[] leftHandAnims = GetLeftHandAttackAnimations(dataId);
                    if (index >= leftHandAnims.Length)
                        index = 0;
                    if (index < leftHandAnims.Length)
                        tempActionAnimation = leftHandAnims[index];
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

        public ActionAnimation[] GetRightHandAttackAnimations(int dataId)
        {
            WeaponAnimations anims;
            if (TryGetWeaponAnimations(dataId, out anims) && anims.rightHandAttackAnimations != null)
                return anims.rightHandAttackAnimations;
            return defaultAnimations.rightHandAttackAnimations;
        }

        public ActionAnimation[] GetLeftHandAttackAnimations(int dataId)
        {
            WeaponAnimations anims;
            if (TryGetWeaponAnimations(dataId, out anims) && anims.leftHandAttackAnimations != null)
                return anims.leftHandAttackAnimations;
            return defaultAnimations.leftHandAttackAnimations;
        }

        public AnimationClip GetRightHandWeaponChargeClip(int dataId)
        {
            WeaponAnimations anims;
            if (TryGetWeaponAnimations(dataId, out anims) && anims.rightHandChargeClip != null)
                return anims.rightHandChargeClip;
            return defaultAnimations.rightHandChargeClip;
        }

        public AnimationClip GetLeftHandWeaponChargeClip(int dataId)
        {
            WeaponAnimations anims;
            if (TryGetWeaponAnimations(dataId, out anims) && anims.leftHandChargeClip != null)
                return anims.leftHandChargeClip;
            return defaultAnimations.leftHandChargeClip;
        }

        public AnimationClip GetSkillCastClip(int dataId)
        {
            SkillAnimations anims;
            if (TryGetSkillAnimations(dataId, out anims) && anims.castClip != null)
                return anims.castClip;
            return defaultAnimations.skillCastClip;
        }

        public bool IsSkillCastClipPlayingAllLayers(int dataId)
        {
            SkillAnimations anims;
            if (TryGetSkillAnimations(dataId, out anims) && anims.castClip != null)
                return anims.playCastClipAllLayers;
            return defaultAnimations.playSkillCastClipAllLayers;
        }

        public ActionAnimation GetSkillActivateAnimation(int dataId)
        {
            SkillAnimations anims;
            if (TryGetSkillAnimations(dataId, out anims) && anims.activateAnimation.clip != null)
                return anims.activateAnimation;
            return defaultAnimations.skillActivateAnimation;
        }

        public ActionAnimation GetRightHandReloadAnimation(int dataId)
        {
            WeaponAnimations anims;
            if (TryGetWeaponAnimations(dataId, out anims) && anims.rightHandReloadAnimation.clip != null)
                return anims.rightHandReloadAnimation;
            return defaultAnimations.rightHandReloadAnimation;
        }

        public ActionAnimation GetLeftHandReloadAnimation(int dataId)
        {
            WeaponAnimations anims;
            if (TryGetWeaponAnimations(dataId, out anims) && anims.leftHandReloadAnimation.clip != null)
                return anims.leftHandReloadAnimation;
            return defaultAnimations.leftHandReloadAnimation;
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
        [ContextMenu("Copy Default Animations", false, 1000401)]
        public void CopyDefaultAnimations()
        {
            CharacterModelDataManager.CopyDefaultAnimations(defaultAnimations);
        }

        [ContextMenu("Paste Default Animations", false, 1000402)]
        public void PasteDefaultAnimations()
        {
            defaultAnimations = CharacterModelDataManager.PasteDefaultAnimations();
            EditorUtility.SetDirty(this);
        }

        [ContextMenu("Copy Weapon Animations", false, 1000403)]
        public void CopyWeaponAnimations()
        {
            CharacterModelDataManager.CopyWeaponAnimations(weaponAnimations);
        }

        [ContextMenu("Paste Weapon Animations", false, 1000404)]
        public void PasteWeaponAnimations()
        {
            WeaponAnimations[] weaponAnimations = CharacterModelDataManager.PasteWeaponAnimations();
            if (weaponAnimations != null)
            {
                this.weaponAnimations = weaponAnimations;
                EditorUtility.SetDirty(this);
            }
        }

        [ContextMenu("Copy Skill Animations", false, 1000405)]
        public void CopySkillAnimations()
        {
            CharacterModelDataManager.CopySkillAnimations(skillAnimations);
        }

        [ContextMenu("Paste Skill Animations", false, 1000406)]
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
    }
}
