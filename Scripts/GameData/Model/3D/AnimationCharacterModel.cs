using System.Collections;
using System.Collections.Generic;
using LiteNetLibManager;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class AnimationCharacterModel : BaseRemakeCharacterModel
    {
        [Header("Settings")]
        public float actionClipFadeLength = 0.1f;
        public float idleClipFadeLength = 0.1f;
        public float moveClipFadeLength = 0.1f;
        public float jumpClipFadeLength = 0.1f;
        public float fallClipFadeLength = 0.1f;
        public float landClipFadeLength = 0.1f;
        public float hurtClipFadeLength = 0.1f;
        public float deadClipFadeLength = 0.1f;
        public float pickupClipFadeLength = 0.1f;

        [Header("Relates Components")]
        public Animation legacyAnimation;

        private bool isSetupComponent;
        private string lastFadedLegacyClipName;
        private bool isLanded = true;

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
            if (legacyAnimation == null)
            {
                legacyAnimation = GetComponentInChildren<Animation>();
                if (legacyAnimation != null)
                    hasChanges = true;
            }
            if (legacyAnimation == null)
                Logging.LogError(ToString(), $"{name}(AnimationCharacterModel) -> `LegacyAnimation` is empty, please set reference of `Animation` component to `AnimationCharacterModel` -> `LegacyAnimation` field.");
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
            if (IsMainModel)
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
                defaultAnimations.landedClip,
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
                defaultAnimations.landedAnimSpeedRate,
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
            AnimationClip landedClip,
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
            float landedAnimSpeedRate,
            float hurtAnimSpeedRate,
            float deadAnimSpeedRate,
            float pickupAnimSpeedRate)
        {
            if (legacyAnimation == null)
                return;
            // Remove clips
            // Move
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
            // Sprint
            if (legacyAnimation.GetClip(CLIP_SPRINT) != null)
                legacyAnimation.RemoveClip(CLIP_SPRINT);
            if (legacyAnimation.GetClip(CLIP_SPRINT_BACKWARD) != null)
                legacyAnimation.RemoveClip(CLIP_SPRINT_BACKWARD);
            if (legacyAnimation.GetClip(CLIP_SPRINT_LEFT) != null)
                legacyAnimation.RemoveClip(CLIP_SPRINT_LEFT);
            if (legacyAnimation.GetClip(CLIP_SPRINT_RIGHT) != null)
                legacyAnimation.RemoveClip(CLIP_SPRINT_RIGHT);
            if (legacyAnimation.GetClip(CLIP_SPRINT_FORWARD_LEFT) != null)
                legacyAnimation.RemoveClip(CLIP_SPRINT_FORWARD_LEFT);
            if (legacyAnimation.GetClip(CLIP_SPRINT_FORWARD_RIGHT) != null)
                legacyAnimation.RemoveClip(CLIP_SPRINT_FORWARD_RIGHT);
            if (legacyAnimation.GetClip(CLIP_SPRINT_BACKWARD_LEFT) != null)
                legacyAnimation.RemoveClip(CLIP_SPRINT_BACKWARD_LEFT);
            if (legacyAnimation.GetClip(CLIP_SPRINT_BACKWARD_RIGHT) != null)
                legacyAnimation.RemoveClip(CLIP_SPRINT_BACKWARD_RIGHT);
            // Walk
            if (legacyAnimation.GetClip(CLIP_WALK) != null)
                legacyAnimation.RemoveClip(CLIP_WALK);
            if (legacyAnimation.GetClip(CLIP_WALK_BACKWARD) != null)
                legacyAnimation.RemoveClip(CLIP_WALK_BACKWARD);
            if (legacyAnimation.GetClip(CLIP_WALK_LEFT) != null)
                legacyAnimation.RemoveClip(CLIP_WALK_LEFT);
            if (legacyAnimation.GetClip(CLIP_WALK_RIGHT) != null)
                legacyAnimation.RemoveClip(CLIP_WALK_RIGHT);
            if (legacyAnimation.GetClip(CLIP_WALK_FORWARD_LEFT) != null)
                legacyAnimation.RemoveClip(CLIP_WALK_FORWARD_LEFT);
            if (legacyAnimation.GetClip(CLIP_WALK_FORWARD_RIGHT) != null)
                legacyAnimation.RemoveClip(CLIP_WALK_FORWARD_RIGHT);
            if (legacyAnimation.GetClip(CLIP_WALK_BACKWARD_LEFT) != null)
                legacyAnimation.RemoveClip(CLIP_WALK_BACKWARD_LEFT);
            if (legacyAnimation.GetClip(CLIP_WALK_BACKWARD_RIGHT) != null)
                legacyAnimation.RemoveClip(CLIP_WALK_BACKWARD_RIGHT);
            // Crouch
            if (legacyAnimation.GetClip(CLIP_CROUCH_IDLE) != null)
                legacyAnimation.RemoveClip(CLIP_CROUCH_IDLE);
            if (legacyAnimation.GetClip(CLIP_CROUCH_MOVE) != null)
                legacyAnimation.RemoveClip(CLIP_CROUCH_MOVE);
            if (legacyAnimation.GetClip(CLIP_CROUCH_MOVE_BACKWARD) != null)
                legacyAnimation.RemoveClip(CLIP_CROUCH_MOVE_BACKWARD);
            if (legacyAnimation.GetClip(CLIP_CROUCH_MOVE_LEFT) != null)
                legacyAnimation.RemoveClip(CLIP_CROUCH_MOVE_LEFT);
            if (legacyAnimation.GetClip(CLIP_CROUCH_MOVE_RIGHT) != null)
                legacyAnimation.RemoveClip(CLIP_CROUCH_MOVE_RIGHT);
            if (legacyAnimation.GetClip(CLIP_CROUCH_MOVE_FORWARD_LEFT) != null)
                legacyAnimation.RemoveClip(CLIP_CROUCH_MOVE_FORWARD_LEFT);
            if (legacyAnimation.GetClip(CLIP_CROUCH_MOVE_FORWARD_RIGHT) != null)
                legacyAnimation.RemoveClip(CLIP_CROUCH_MOVE_FORWARD_RIGHT);
            if (legacyAnimation.GetClip(CLIP_CROUCH_MOVE_BACKWARD_LEFT) != null)
                legacyAnimation.RemoveClip(CLIP_CROUCH_MOVE_BACKWARD_LEFT);
            if (legacyAnimation.GetClip(CLIP_CROUCH_MOVE_BACKWARD_RIGHT) != null)
                legacyAnimation.RemoveClip(CLIP_CROUCH_MOVE_BACKWARD_RIGHT);
            // Crawl
            if (legacyAnimation.GetClip(CLIP_CRAWL_IDLE) != null)
                legacyAnimation.RemoveClip(CLIP_CRAWL_IDLE);
            if (legacyAnimation.GetClip(CLIP_CRAWL_MOVE) != null)
                legacyAnimation.RemoveClip(CLIP_CRAWL_MOVE);
            if (legacyAnimation.GetClip(CLIP_CRAWL_MOVE_BACKWARD) != null)
                legacyAnimation.RemoveClip(CLIP_CRAWL_MOVE_BACKWARD);
            if (legacyAnimation.GetClip(CLIP_CRAWL_MOVE_LEFT) != null)
                legacyAnimation.RemoveClip(CLIP_CRAWL_MOVE_LEFT);
            if (legacyAnimation.GetClip(CLIP_CRAWL_MOVE_RIGHT) != null)
                legacyAnimation.RemoveClip(CLIP_CRAWL_MOVE_RIGHT);
            if (legacyAnimation.GetClip(CLIP_CRAWL_MOVE_FORWARD_LEFT) != null)
                legacyAnimation.RemoveClip(CLIP_CRAWL_MOVE_FORWARD_LEFT);
            if (legacyAnimation.GetClip(CLIP_CRAWL_MOVE_FORWARD_RIGHT) != null)
                legacyAnimation.RemoveClip(CLIP_CRAWL_MOVE_FORWARD_RIGHT);
            if (legacyAnimation.GetClip(CLIP_CRAWL_MOVE_BACKWARD_LEFT) != null)
                legacyAnimation.RemoveClip(CLIP_CRAWL_MOVE_BACKWARD_LEFT);
            if (legacyAnimation.GetClip(CLIP_CRAWL_MOVE_BACKWARD_RIGHT) != null)
                legacyAnimation.RemoveClip(CLIP_CRAWL_MOVE_BACKWARD_RIGHT);
            // Swim
            if (legacyAnimation.GetClip(CLIP_SWIM_IDLE) != null)
                legacyAnimation.RemoveClip(CLIP_SWIM_IDLE);
            if (legacyAnimation.GetClip(CLIP_SWIM_MOVE) != null)
                legacyAnimation.RemoveClip(CLIP_SWIM_MOVE);
            if (legacyAnimation.GetClip(CLIP_SWIM_MOVE_BACKWARD) != null)
                legacyAnimation.RemoveClip(CLIP_SWIM_MOVE_BACKWARD);
            if (legacyAnimation.GetClip(CLIP_SWIM_MOVE_LEFT) != null)
                legacyAnimation.RemoveClip(CLIP_SWIM_MOVE_LEFT);
            if (legacyAnimation.GetClip(CLIP_SWIM_MOVE_RIGHT) != null)
                legacyAnimation.RemoveClip(CLIP_SWIM_MOVE_RIGHT);
            if (legacyAnimation.GetClip(CLIP_SWIM_MOVE_FORWARD_LEFT) != null)
                legacyAnimation.RemoveClip(CLIP_SWIM_MOVE_FORWARD_LEFT);
            if (legacyAnimation.GetClip(CLIP_SWIM_MOVE_FORWARD_RIGHT) != null)
                legacyAnimation.RemoveClip(CLIP_SWIM_MOVE_FORWARD_RIGHT);
            if (legacyAnimation.GetClip(CLIP_SWIM_MOVE_BACKWARD_LEFT) != null)
                legacyAnimation.RemoveClip(CLIP_SWIM_MOVE_BACKWARD_LEFT);
            if (legacyAnimation.GetClip(CLIP_SWIM_MOVE_BACKWARD_RIGHT) != null)
                legacyAnimation.RemoveClip(CLIP_SWIM_MOVE_BACKWARD_RIGHT);
            // Other
            if (legacyAnimation.GetClip(CLIP_JUMP) != null)
                legacyAnimation.RemoveClip(CLIP_JUMP);
            if (legacyAnimation.GetClip(CLIP_FALL) != null)
                legacyAnimation.RemoveClip(CLIP_FALL);
            if (legacyAnimation.GetClip(CLIP_HURT) != null)
                legacyAnimation.RemoveClip(CLIP_HURT);
            if (legacyAnimation.GetClip(CLIP_DEAD) != null)
                legacyAnimation.RemoveClip(CLIP_DEAD);
            if (legacyAnimation.GetClip(CLIP_PICKUP) != null)
                legacyAnimation.RemoveClip(CLIP_PICKUP);
            // Setup generic clips
            // Move
            legacyAnimation.AddClip(idleClip != null ? idleClip : defaultAnimations.idleClip, CLIP_IDLE);
            legacyAnimation.AddClip(moveClip != null ? moveClip : defaultAnimations.moveClip, CLIP_MOVE);
            legacyAnimation.AddClip(moveBackwardClip != null ? moveBackwardClip : defaultAnimations.moveBackwardClip, CLIP_MOVE_BACKWARD);
            legacyAnimation.AddClip(moveLeftClip != null ? moveLeftClip : defaultAnimations.moveLeftClip, CLIP_MOVE_LEFT);
            legacyAnimation.AddClip(moveRightClip != null ? moveRightClip : defaultAnimations.moveRightClip, CLIP_MOVE_RIGHT);
            legacyAnimation.AddClip(moveForwardLeftClip != null ? moveForwardLeftClip : defaultAnimations.moveForwardLeftClip, CLIP_MOVE_FORWARD_LEFT);
            legacyAnimation.AddClip(moveForwardRightClip != null ? moveForwardRightClip : defaultAnimations.moveForwardRightClip, CLIP_MOVE_FORWARD_RIGHT);
            legacyAnimation.AddClip(moveBackwardLeftClip != null ? moveBackwardLeftClip : defaultAnimations.moveBackwardLeftClip, CLIP_MOVE_BACKWARD_LEFT);
            legacyAnimation.AddClip(moveBackwardRightClip != null ? moveBackwardRightClip : defaultAnimations.moveBackwardRightClip, CLIP_MOVE_BACKWARD_RIGHT);
            // Sprint
            legacyAnimation.AddClip(sprintClip != null ? sprintClip : defaultAnimations.sprintClip, CLIP_SPRINT);
            legacyAnimation.AddClip(sprintBackwardClip != null ? sprintBackwardClip : defaultAnimations.sprintBackwardClip, CLIP_SPRINT_BACKWARD);
            legacyAnimation.AddClip(sprintLeftClip != null ? sprintLeftClip : defaultAnimations.sprintLeftClip, CLIP_SPRINT_LEFT);
            legacyAnimation.AddClip(sprintRightClip != null ? sprintRightClip : defaultAnimations.sprintRightClip, CLIP_SPRINT_RIGHT);
            legacyAnimation.AddClip(sprintForwardLeftClip != null ? sprintForwardLeftClip : defaultAnimations.sprintForwardLeftClip, CLIP_SPRINT_FORWARD_LEFT);
            legacyAnimation.AddClip(sprintForwardRightClip != null ? sprintForwardRightClip : defaultAnimations.sprintForwardRightClip, CLIP_SPRINT_FORWARD_RIGHT);
            legacyAnimation.AddClip(sprintBackwardLeftClip != null ? sprintBackwardLeftClip : defaultAnimations.sprintBackwardLeftClip, CLIP_SPRINT_BACKWARD_LEFT);
            legacyAnimation.AddClip(sprintBackwardRightClip != null ? sprintBackwardRightClip : defaultAnimations.sprintBackwardRightClip, CLIP_SPRINT_BACKWARD_RIGHT);
            // Walk
            legacyAnimation.AddClip(walkClip != null ? walkClip : defaultAnimations.walkClip, CLIP_WALK);
            legacyAnimation.AddClip(walkBackwardClip != null ? walkBackwardClip : defaultAnimations.walkBackwardClip, CLIP_WALK_BACKWARD);
            legacyAnimation.AddClip(walkLeftClip != null ? walkLeftClip : defaultAnimations.walkLeftClip, CLIP_WALK_LEFT);
            legacyAnimation.AddClip(walkRightClip != null ? walkRightClip : defaultAnimations.walkRightClip, CLIP_WALK_RIGHT);
            legacyAnimation.AddClip(walkForwardLeftClip != null ? walkForwardLeftClip : defaultAnimations.walkForwardLeftClip, CLIP_WALK_FORWARD_LEFT);
            legacyAnimation.AddClip(walkForwardRightClip != null ? walkForwardRightClip : defaultAnimations.walkForwardRightClip, CLIP_WALK_FORWARD_RIGHT);
            legacyAnimation.AddClip(walkBackwardLeftClip != null ? walkBackwardLeftClip : defaultAnimations.walkBackwardLeftClip, CLIP_WALK_BACKWARD_LEFT);
            legacyAnimation.AddClip(walkBackwardRightClip != null ? walkBackwardRightClip : defaultAnimations.walkBackwardRightClip, CLIP_WALK_BACKWARD_RIGHT);
            // Crouch
            legacyAnimation.AddClip(crouchIdleClip != null ? crouchIdleClip : defaultAnimations.crouchIdleClip, CLIP_CROUCH_IDLE);
            legacyAnimation.AddClip(crouchMoveClip != null ? crouchMoveClip : defaultAnimations.crouchMoveClip, CLIP_CROUCH_MOVE);
            legacyAnimation.AddClip(crouchMoveBackwardClip != null ? crouchMoveBackwardClip : defaultAnimations.crouchMoveBackwardClip, CLIP_CROUCH_MOVE_BACKWARD);
            legacyAnimation.AddClip(crouchMoveLeftClip != null ? crouchMoveLeftClip : defaultAnimations.crouchMoveLeftClip, CLIP_CROUCH_MOVE_LEFT);
            legacyAnimation.AddClip(crouchMoveRightClip != null ? crouchMoveRightClip : defaultAnimations.crouchMoveRightClip, CLIP_CROUCH_MOVE_RIGHT);
            legacyAnimation.AddClip(crouchMoveForwardLeftClip != null ? crouchMoveForwardLeftClip : defaultAnimations.crouchMoveForwardLeftClip, CLIP_CROUCH_MOVE_FORWARD_LEFT);
            legacyAnimation.AddClip(crouchMoveForwardRightClip != null ? crouchMoveForwardRightClip : defaultAnimations.crouchMoveForwardRightClip, CLIP_CROUCH_MOVE_FORWARD_RIGHT);
            legacyAnimation.AddClip(crouchMoveBackwardLeftClip != null ? crouchMoveBackwardLeftClip : defaultAnimations.crouchMoveBackwardLeftClip, CLIP_CROUCH_MOVE_BACKWARD_LEFT);
            legacyAnimation.AddClip(crouchMoveBackwardRightClip != null ? crouchMoveBackwardRightClip : defaultAnimations.crouchMoveBackwardRightClip, CLIP_CROUCH_MOVE_BACKWARD_RIGHT);
            // Crawl
            legacyAnimation.AddClip(crawlIdleClip != null ? crawlIdleClip : defaultAnimations.crawlIdleClip, CLIP_CRAWL_IDLE);
            legacyAnimation.AddClip(crawlMoveClip != null ? crawlMoveClip : defaultAnimations.crawlMoveClip, CLIP_CRAWL_MOVE);
            legacyAnimation.AddClip(crawlMoveBackwardClip != null ? crawlMoveBackwardClip : defaultAnimations.crawlMoveBackwardClip, CLIP_CRAWL_MOVE_BACKWARD);
            legacyAnimation.AddClip(crawlMoveLeftClip != null ? crawlMoveLeftClip : defaultAnimations.crawlMoveLeftClip, CLIP_CRAWL_MOVE_LEFT);
            legacyAnimation.AddClip(crawlMoveRightClip != null ? crawlMoveRightClip : defaultAnimations.crawlMoveRightClip, CLIP_CRAWL_MOVE_RIGHT);
            legacyAnimation.AddClip(crawlMoveForwardLeftClip != null ? crawlMoveForwardLeftClip : defaultAnimations.crawlMoveForwardLeftClip, CLIP_CRAWL_MOVE_FORWARD_LEFT);
            legacyAnimation.AddClip(crawlMoveForwardRightClip != null ? crawlMoveForwardRightClip : defaultAnimations.crawlMoveForwardRightClip, CLIP_CRAWL_MOVE_FORWARD_RIGHT);
            legacyAnimation.AddClip(crawlMoveBackwardLeftClip != null ? crawlMoveBackwardLeftClip : defaultAnimations.crawlMoveBackwardLeftClip, CLIP_CRAWL_MOVE_BACKWARD_LEFT);
            legacyAnimation.AddClip(crawlMoveBackwardRightClip != null ? crawlMoveBackwardRightClip : defaultAnimations.crawlMoveBackwardRightClip, CLIP_CRAWL_MOVE_BACKWARD_RIGHT);
            // Swim
            legacyAnimation.AddClip(swimIdleClip != null ? swimIdleClip : defaultAnimations.swimIdleClip, CLIP_SWIM_IDLE);
            legacyAnimation.AddClip(swimMoveClip != null ? swimMoveClip : defaultAnimations.swimMoveClip, CLIP_SWIM_MOVE);
            legacyAnimation.AddClip(swimMoveBackwardClip != null ? swimMoveBackwardClip : defaultAnimations.swimMoveBackwardClip, CLIP_SWIM_MOVE_BACKWARD);
            legacyAnimation.AddClip(swimMoveLeftClip != null ? swimMoveLeftClip : defaultAnimations.swimMoveLeftClip, CLIP_SWIM_MOVE_LEFT);
            legacyAnimation.AddClip(swimMoveRightClip != null ? swimMoveRightClip : defaultAnimations.swimMoveRightClip, CLIP_SWIM_MOVE_RIGHT);
            legacyAnimation.AddClip(swimMoveForwardLeftClip != null ? swimMoveForwardLeftClip : defaultAnimations.swimMoveForwardLeftClip, CLIP_SWIM_MOVE_FORWARD_LEFT);
            legacyAnimation.AddClip(swimMoveForwardRightClip != null ? swimMoveForwardRightClip : defaultAnimations.swimMoveForwardRightClip, CLIP_SWIM_MOVE_FORWARD_RIGHT);
            legacyAnimation.AddClip(swimMoveBackwardLeftClip != null ? swimMoveBackwardLeftClip : defaultAnimations.swimMoveBackwardLeftClip, CLIP_SWIM_MOVE_BACKWARD_LEFT);
            legacyAnimation.AddClip(swimMoveBackwardRightClip != null ? swimMoveBackwardRightClip : defaultAnimations.swimMoveBackwardRightClip, CLIP_SWIM_MOVE_BACKWARD_RIGHT);
            // Other
            legacyAnimation.AddClip(jumpClip != null ? jumpClip : defaultAnimations.jumpClip, CLIP_JUMP);
            legacyAnimation.AddClip(fallClip != null ? fallClip : defaultAnimations.fallClip, CLIP_FALL);
            legacyAnimation.AddClip(hurtClip != null ? hurtClip : defaultAnimations.hurtClip, CLIP_HURT);
            legacyAnimation.AddClip(deadClip != null ? deadClip : defaultAnimations.deadClip, CLIP_DEAD);
            legacyAnimation.AddClip(pickupClip != null ? pickupClip : defaultAnimations.pickupClip, CLIP_PICKUP);
            // Set speed
            // Idle
            legacyAnimation[CLIP_IDLE].speed = idleAnimSpeedRate > 0f ? idleAnimSpeedRate : 1f;
            // Move
            legacyAnimation[CLIP_MOVE].speed =
                legacyAnimation[CLIP_MOVE_BACKWARD].speed =
                legacyAnimation[CLIP_MOVE_LEFT].speed =
                legacyAnimation[CLIP_MOVE_RIGHT].speed =
                legacyAnimation[CLIP_MOVE_FORWARD_LEFT].speed =
                legacyAnimation[CLIP_MOVE_FORWARD_RIGHT].speed =
                legacyAnimation[CLIP_MOVE_BACKWARD_LEFT].speed =
                legacyAnimation[CLIP_MOVE_BACKWARD_RIGHT].speed = moveAnimSpeedRate > 0f ? moveAnimSpeedRate : 1f;
            // Sprint
            legacyAnimation[CLIP_SPRINT].speed =
                legacyAnimation[CLIP_SPRINT_BACKWARD].speed =
                legacyAnimation[CLIP_SPRINT_LEFT].speed =
                legacyAnimation[CLIP_SPRINT_RIGHT].speed =
                legacyAnimation[CLIP_SPRINT_FORWARD_LEFT].speed =
                legacyAnimation[CLIP_SPRINT_FORWARD_RIGHT].speed =
                legacyAnimation[CLIP_SPRINT_BACKWARD_LEFT].speed =
                legacyAnimation[CLIP_SPRINT_BACKWARD_RIGHT].speed = sprintAnimSpeedRate > 0f ? sprintAnimSpeedRate : 1f;
            // Walk
            legacyAnimation[CLIP_WALK].speed =
                legacyAnimation[CLIP_WALK_BACKWARD].speed =
                legacyAnimation[CLIP_WALK_LEFT].speed =
                legacyAnimation[CLIP_WALK_RIGHT].speed =
                legacyAnimation[CLIP_WALK_FORWARD_LEFT].speed =
                legacyAnimation[CLIP_WALK_FORWARD_RIGHT].speed =
                legacyAnimation[CLIP_WALK_BACKWARD_LEFT].speed =
                legacyAnimation[CLIP_WALK_BACKWARD_RIGHT].speed = walkAnimSpeedRate > 0f ? walkAnimSpeedRate : 1f;
            // Crouch
            // Idle
            legacyAnimation[CLIP_CROUCH_IDLE].speed = crouchIdleAnimSpeedRate > 0f ? crouchIdleAnimSpeedRate : 1f;
            // Move
            legacyAnimation[CLIP_CROUCH_MOVE].speed =
                legacyAnimation[CLIP_CROUCH_MOVE_BACKWARD].speed =
                legacyAnimation[CLIP_CROUCH_MOVE_LEFT].speed =
                legacyAnimation[CLIP_CROUCH_MOVE_RIGHT].speed =
                legacyAnimation[CLIP_CROUCH_MOVE_FORWARD_LEFT].speed =
                legacyAnimation[CLIP_CROUCH_MOVE_FORWARD_RIGHT].speed =
                legacyAnimation[CLIP_CROUCH_MOVE_BACKWARD_LEFT].speed =
                legacyAnimation[CLIP_CROUCH_MOVE_BACKWARD_RIGHT].speed = crouchMoveAnimSpeedRate > 0f ? crouchMoveAnimSpeedRate : 1f;
            // Crawl
            // Idle
            legacyAnimation[CLIP_CRAWL_IDLE].speed = crawlIdleAnimSpeedRate > 0f ? crawlIdleAnimSpeedRate : 1f;
            // Move
            legacyAnimation[CLIP_CRAWL_MOVE].speed =
                legacyAnimation[CLIP_CRAWL_MOVE_BACKWARD].speed =
                legacyAnimation[CLIP_CRAWL_MOVE_LEFT].speed =
                legacyAnimation[CLIP_CRAWL_MOVE_RIGHT].speed =
                legacyAnimation[CLIP_CRAWL_MOVE_FORWARD_LEFT].speed =
                legacyAnimation[CLIP_CRAWL_MOVE_FORWARD_RIGHT].speed =
                legacyAnimation[CLIP_CRAWL_MOVE_BACKWARD_LEFT].speed =
                legacyAnimation[CLIP_CRAWL_MOVE_BACKWARD_RIGHT].speed = crawlMoveAnimSpeedRate > 0f ? crawlMoveAnimSpeedRate : 1f;
            // Swim
            // Idle
            legacyAnimation[CLIP_SWIM_IDLE].speed = swimIdleAnimSpeedRate > 0f ? swimIdleAnimSpeedRate : 1f;
            // Move
            legacyAnimation[CLIP_SWIM_MOVE].speed =
                legacyAnimation[CLIP_SWIM_MOVE_BACKWARD].speed =
                legacyAnimation[CLIP_SWIM_MOVE_LEFT].speed =
                legacyAnimation[CLIP_SWIM_MOVE_RIGHT].speed =
                legacyAnimation[CLIP_SWIM_MOVE_FORWARD_LEFT].speed =
                legacyAnimation[CLIP_SWIM_MOVE_FORWARD_RIGHT].speed =
                legacyAnimation[CLIP_SWIM_MOVE_BACKWARD_LEFT].speed =
                legacyAnimation[CLIP_SWIM_MOVE_BACKWARD_RIGHT].speed = swimMoveAnimSpeedRate > 0f ? swimMoveAnimSpeedRate : 1f;
            // Other
            legacyAnimation[CLIP_JUMP].speed = jumpAnimSpeedRate;
            legacyAnimation[CLIP_FALL].speed = fallAnimSpeedRate;
            legacyAnimation[CLIP_HURT].speed = hurtAnimSpeedRate;
            legacyAnimation[CLIP_DEAD].speed = deadAnimSpeedRate;
            legacyAnimation[CLIP_PICKUP].speed = pickupAnimSpeedRate;
            // Change state to idle
            CrossFadeLegacyAnimation(CLIP_IDLE, 0f, WrapMode.Loop);
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

            WeaponAnimations weaponAnimations;
            TryGetWeaponAnimations(weaponItem.WeaponType.DataId, out weaponAnimations);

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
                weaponAnimations.landedClip,
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
                weaponAnimations.landedAnimSpeedRate,
                weaponAnimations.hurtAnimSpeedRate,
                weaponAnimations.deadAnimSpeedRate,
                weaponAnimations.pickupAnimSpeedRate);
        }

        public override void PlayMoveAnimation()
        {
            if (isDead)
                CrossFadeLegacyAnimation(CLIP_DEAD, deadClipFadeLength, WrapMode.Once);
            else
            {
                if (legacyAnimation.GetClip(CLIP_ACTION) != null && legacyAnimation.IsPlaying(CLIP_ACTION))
                    return;
                if (legacyAnimation.GetClip(CLIP_CAST_SKILL) != null && legacyAnimation.IsPlaying(CLIP_CAST_SKILL))
                    return;

                if (movementState.Has(MovementState.IsUnderWater))
                {
                    CrossFadeMoveAnimaton(CLIP_SWIM_IDLE, CLIP_SWIM_MOVE, CLIP_SWIM_MOVE_BACKWARD, CLIP_SWIM_MOVE_LEFT, CLIP_SWIM_MOVE_RIGHT,
                        CLIP_SWIM_MOVE_FORWARD_LEFT, CLIP_SWIM_MOVE_FORWARD_RIGHT, CLIP_SWIM_MOVE_BACKWARD_LEFT, CLIP_SWIM_MOVE_BACKWARD_RIGHT);
                }
                else if (!movementState.Has(MovementState.IsGrounded))
                {
                    CrossFadeLegacyAnimation(CLIP_FALL, fallClipFadeLength, WrapMode.Loop);
                    isLanded = false;
                }
                else
                {
                    if (!isLanded)
                    {
                        PlayLandedAnimation();
                        isLanded = true;
                    }
                    switch (extraMovementState)
                    {
                        case ExtraMovementState.IsSprinting:
                            CrossFadeMoveAnimaton(CLIP_IDLE, CLIP_SPRINT, CLIP_SPRINT_BACKWARD, CLIP_SPRINT_LEFT, CLIP_SPRINT_RIGHT,
                                CLIP_SPRINT_FORWARD_LEFT, CLIP_SPRINT_FORWARD_RIGHT, CLIP_SPRINT_BACKWARD_LEFT, CLIP_SPRINT_BACKWARD_RIGHT);
                            break;
                        case ExtraMovementState.IsWalking:
                            CrossFadeMoveAnimaton(CLIP_IDLE, CLIP_WALK, CLIP_WALK_BACKWARD, CLIP_WALK_LEFT, CLIP_WALK_RIGHT,
                                CLIP_WALK_FORWARD_LEFT, CLIP_WALK_FORWARD_RIGHT, CLIP_WALK_BACKWARD_LEFT, CLIP_WALK_BACKWARD_RIGHT);
                            break;
                        case ExtraMovementState.IsCrouching:
                            CrossFadeMoveAnimaton(CLIP_CROUCH_IDLE, CLIP_CROUCH_MOVE, CLIP_CROUCH_MOVE_BACKWARD, CLIP_CROUCH_MOVE_LEFT, CLIP_CROUCH_MOVE_RIGHT,
                                CLIP_CROUCH_MOVE_FORWARD_LEFT, CLIP_CROUCH_MOVE_FORWARD_RIGHT, CLIP_CROUCH_MOVE_BACKWARD_LEFT, CLIP_CROUCH_MOVE_BACKWARD_RIGHT);
                            break;
                        case ExtraMovementState.IsCrawling:
                            CrossFadeMoveAnimaton(CLIP_CRAWL_IDLE, CLIP_CRAWL_MOVE, CLIP_CRAWL_MOVE_BACKWARD, CLIP_CRAWL_MOVE_LEFT, CLIP_CRAWL_MOVE_RIGHT,
                                CLIP_CRAWL_MOVE_FORWARD_LEFT, CLIP_CRAWL_MOVE_FORWARD_RIGHT, CLIP_CRAWL_MOVE_BACKWARD_LEFT, CLIP_CRAWL_MOVE_BACKWARD_RIGHT);
                            break;
                        default:
                            CrossFadeMoveAnimaton(CLIP_IDLE, CLIP_MOVE, CLIP_MOVE_BACKWARD, CLIP_MOVE_LEFT, CLIP_MOVE_RIGHT,
                                CLIP_MOVE_FORWARD_LEFT, CLIP_MOVE_FORWARD_RIGHT, CLIP_MOVE_BACKWARD_LEFT, CLIP_MOVE_BACKWARD_RIGHT);
                            break;
                    }
                }
            }
        }

        private void CrossFadeMoveAnimaton(string clipIdle,
        string clipMove, string clipMoveBackward, string clipMoveLeft, string clipMoveRight,
        string clipMoveForwardLeft, string clipMoveForwardRight,
        string clipMoveBackwardLeft, string clipMoveBackwardRight)
        {
            // Forward Right
            if (movementState.Has(MovementState.Forward) && movementState.Has(MovementState.Right))
                CrossFadeLegacyAnimation(clipMoveForwardRight, moveClipFadeLength, WrapMode.Loop);
            // Forward Left
            else if (movementState.Has(MovementState.Forward) && movementState.Has(MovementState.Left))
                CrossFadeLegacyAnimation(clipMoveForwardLeft, moveClipFadeLength, WrapMode.Loop);
            // Backward Right
            else if (movementState.Has(MovementState.Backward) && movementState.Has(MovementState.Right))
                CrossFadeLegacyAnimation(clipMoveBackwardRight, moveClipFadeLength, WrapMode.Loop);
            // Backward Left
            else if (movementState.Has(MovementState.Backward) && movementState.Has(MovementState.Left))
                CrossFadeLegacyAnimation(clipMoveBackwardLeft, moveClipFadeLength, WrapMode.Loop);
            // Forward
            else if (movementState.Has(MovementState.Forward))
                CrossFadeLegacyAnimation(clipMove, moveClipFadeLength, WrapMode.Loop);
            // Backward
            else if (movementState.Has(MovementState.Backward))
                CrossFadeLegacyAnimation(clipMoveBackward, moveClipFadeLength, WrapMode.Loop);
            // Right
            else if (movementState.Has(MovementState.Right))
                CrossFadeLegacyAnimation(clipMoveRight, moveClipFadeLength, WrapMode.Loop);
            // Left
            else if (movementState.Has(MovementState.Left))
                CrossFadeLegacyAnimation(clipMoveLeft, moveClipFadeLength, WrapMode.Loop);
            // Idle
            else
                CrossFadeLegacyAnimation(clipIdle, idleClipFadeLength, WrapMode.Loop);
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

        public override void PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1f)
        {
            StartedActionCoroutine(StartCoroutine(PlayActionAnimation_LegacyAnimation(animActionType, dataId, index, playSpeedMultiplier)));
        }

        private IEnumerator PlayActionAnimation_LegacyAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier)
        {
            isDoingAction = true;
            // If animator is not null, play the action animation
            ActionAnimation tempActionAnimation = GetActionAnimation(animActionType, dataId, index);
            if (tempActionAnimation.clip != null)
            {
                if (legacyAnimation.GetClip(CLIP_ACTION) != null)
                    legacyAnimation.RemoveClip(CLIP_ACTION);
                legacyAnimation.AddClip(tempActionAnimation.clip, CLIP_ACTION);
                legacyAnimation[CLIP_ACTION].speed = playSpeedMultiplier;
            }
            AudioManager.PlaySfxClipAtAudioSource(tempActionAnimation.GetRandomAudioClip(), genericAudioSource);
            if (tempActionAnimation.clip != null)
                CrossFadeLegacyAnimation(CLIP_ACTION, actionClipFadeLength, WrapMode.Once);
            // Waits by current transition + clip duration before end animation
            yield return new WaitForSecondsRealtime(tempActionAnimation.GetClipLength() / playSpeedMultiplier);
            if (tempActionAnimation.clip != null)
                CrossFadeLegacyAnimation(CLIP_IDLE, idleClipFadeLength, WrapMode.Loop);
            // Waits by current transition + extra duration before end playing animation state
            yield return new WaitForSecondsRealtime(tempActionAnimation.GetExtendDuration() / playSpeedMultiplier);
            isDoingAction = false;
        }

        public override void PlaySkillCastClip(int dataId, float duration)
        {
            StartedActionCoroutine(StartCoroutine(PlaySkillCastClip_LegacyAnimation(dataId, duration)));
        }

        private IEnumerator PlaySkillCastClip_LegacyAnimation(int dataId, float duration)
        {
            isDoingAction = true;
            AnimationClip castClip = GetSkillCastClip(dataId);
            if (legacyAnimation.GetClip(CLIP_CAST_SKILL) != null)
                legacyAnimation.RemoveClip(CLIP_CAST_SKILL);
            legacyAnimation.AddClip(castClip, CLIP_CAST_SKILL);
            CrossFadeLegacyAnimation(CLIP_CAST_SKILL, actionClipFadeLength, WrapMode.Loop);
            yield return new WaitForSecondsRealtime(duration);
            CrossFadeLegacyAnimation(CLIP_IDLE, idleClipFadeLength, WrapMode.Loop);
            isDoingAction = false;
        }

        public override void PlayWeaponChargeClip(int dataId, bool isLeftHand)
        {
            isDoingAction = true;
            AnimationClip chargeClip = isLeftHand ? GetLeftHandWeaponChargeClip(dataId) : GetRightHandWeaponChargeClip(dataId);
            if (legacyAnimation.GetClip(CLIP_WEAPON_CHARGE) != null)
                legacyAnimation.RemoveClip(CLIP_WEAPON_CHARGE);
            legacyAnimation.AddClip(chargeClip, CLIP_WEAPON_CHARGE);
            CrossFadeLegacyAnimation(CLIP_WEAPON_CHARGE, actionClipFadeLength, WrapMode.Once);
        }

        public override void StopActionAnimation()
        {
            CrossFadeLegacyAnimation(CLIP_IDLE, idleClipFadeLength, WrapMode.Loop);
            isDoingAction = false;
        }

        public override void StopSkillCastAnimation()
        {
            CrossFadeLegacyAnimation(CLIP_IDLE, idleClipFadeLength, WrapMode.Loop);
            isDoingAction = false;
        }

        public override void StopWeaponChargeAnimation()
        {
            CrossFadeLegacyAnimation(CLIP_IDLE, idleClipFadeLength, WrapMode.Loop);
            isDoingAction = false;
        }

        public override void PlayHitAnimation()
        {
            if (isDoingAction)
                return;
            if (legacyAnimation.GetClip(CLIP_HURT) == null)
                return;
            CrossFadeLegacyAnimation(CLIP_HURT, hurtClipFadeLength, WrapMode.Once);
        }

        public override float GetJumpAnimationDuration()
        {
            if (legacyAnimation.GetClip(CLIP_JUMP) == null)
                return 0f;
            return legacyAnimation.GetClip(CLIP_JUMP).length / legacyAnimation[CLIP_JUMP].speed;
        }

        public override void PlayJumpAnimation()
        {
            if (legacyAnimation.GetClip(CLIP_JUMP) == null)
                return;
            CrossFadeLegacyAnimation(CLIP_JUMP, jumpClipFadeLength, WrapMode.Once);
        }

        public override void PlayPickupAnimation()
        {
            if (isDoingAction)
                return;
            if (legacyAnimation.GetClip(CLIP_PICKUP) == null)
                return;
            CrossFadeLegacyAnimation(CLIP_PICKUP, pickupClipFadeLength, WrapMode.Once);
        }

        public void PlayLandedAnimation()
        {
            if (legacyAnimation.GetClip(CLIP_LANDED) == null)
                return;
            CrossFadeLegacyAnimation(CLIP_LANDED, landClipFadeLength, WrapMode.Once);
        }
    }
}
