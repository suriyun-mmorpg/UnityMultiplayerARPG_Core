using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public float hurtClipFadeLength = 0.1f;
        public float deadClipFadeLength = 0.1f;

        [Header("Relates Components")]
        public Animation legacyAnimation;

        // Private state validater
        private bool isSetupComponent;
        private bool isPlayingActionAnimation;
        private string lastFadedLegacyClipName;

        protected override void Awake()
        {
            base.Awake();
            SetupComponent();
        }

        private void SetupComponent()
        {
            if (isSetupComponent)
                return;
            isSetupComponent = true;
            if (legacyAnimation == null)
                legacyAnimation = GetComponentInChildren<Animation>();
            legacyAnimation.AddClip(defaultAnimations.idleClip, CLIP_IDLE);
            legacyAnimation.AddClip(defaultAnimations.moveClip, CLIP_MOVE);
            legacyAnimation.AddClip(defaultAnimations.moveBackwardClip, CLIP_MOVE_BACKWARD);
            legacyAnimation.AddClip(defaultAnimations.moveLeftClip, CLIP_MOVE_LEFT);
            legacyAnimation.AddClip(defaultAnimations.moveRightClip, CLIP_MOVE_RIGHT);
            legacyAnimation.AddClip(defaultAnimations.moveForwardLeftClip, CLIP_MOVE_FORWARD_LEFT);
            legacyAnimation.AddClip(defaultAnimations.moveForwardRightClip, CLIP_MOVE_FORWARD_RIGHT);
            legacyAnimation.AddClip(defaultAnimations.moveBackwardLeftClip, CLIP_MOVE_BACKWARD_LEFT);
            legacyAnimation.AddClip(defaultAnimations.moveBackwardRightClip, CLIP_MOVE_BACKWARD_RIGHT);
            legacyAnimation.AddClip(defaultAnimations.sprintClip, CLIP_SPRINT);
            legacyAnimation.AddClip(defaultAnimations.sprintBackwardClip, CLIP_SPRINT_BACKWARD);
            legacyAnimation.AddClip(defaultAnimations.sprintLeftClip, CLIP_SPRINT_LEFT);
            legacyAnimation.AddClip(defaultAnimations.sprintRightClip, CLIP_SPRINT_RIGHT);
            legacyAnimation.AddClip(defaultAnimations.sprintForwardLeftClip, CLIP_SPRINT_FORWARD_LEFT);
            legacyAnimation.AddClip(defaultAnimations.sprintForwardRightClip, CLIP_SPRINT_FORWARD_RIGHT);
            legacyAnimation.AddClip(defaultAnimations.sprintBackwardLeftClip, CLIP_SPRINT_BACKWARD_LEFT);
            legacyAnimation.AddClip(defaultAnimations.sprintBackwardRightClip, CLIP_SPRINT_BACKWARD_RIGHT);
            legacyAnimation.AddClip(defaultAnimations.crouchIdleClip, CLIP_CROUCH_IDLE);
            legacyAnimation.AddClip(defaultAnimations.crouchMoveClip, CLIP_CROUCH_MOVE);
            legacyAnimation.AddClip(defaultAnimations.crouchMoveBackwardClip, CLIP_CROUCH_MOVE_BACKWARD);
            legacyAnimation.AddClip(defaultAnimations.crouchMoveLeftClip, CLIP_CROUCH_MOVE_LEFT);
            legacyAnimation.AddClip(defaultAnimations.crouchMoveRightClip, CLIP_CROUCH_MOVE_RIGHT);
            legacyAnimation.AddClip(defaultAnimations.crouchMoveForwardLeftClip, CLIP_CROUCH_MOVE_FORWARD_LEFT);
            legacyAnimation.AddClip(defaultAnimations.crouchMoveForwardRightClip, CLIP_CROUCH_MOVE_FORWARD_RIGHT);
            legacyAnimation.AddClip(defaultAnimations.crouchMoveBackwardLeftClip, CLIP_CROUCH_MOVE_BACKWARD_LEFT);
            legacyAnimation.AddClip(defaultAnimations.crouchMoveBackwardRightClip, CLIP_CROUCH_MOVE_BACKWARD_RIGHT);
            legacyAnimation.AddClip(defaultAnimations.crawlIdleClip, CLIP_CRAWL_IDLE);
            legacyAnimation.AddClip(defaultAnimations.crawlMoveClip, CLIP_CRAWL_MOVE);
            legacyAnimation.AddClip(defaultAnimations.crawlMoveBackwardClip, CLIP_CRAWL_MOVE_BACKWARD);
            legacyAnimation.AddClip(defaultAnimations.crawlMoveLeftClip, CLIP_CRAWL_MOVE_LEFT);
            legacyAnimation.AddClip(defaultAnimations.crawlMoveRightClip, CLIP_CRAWL_MOVE_RIGHT);
            legacyAnimation.AddClip(defaultAnimations.crawlMoveForwardLeftClip, CLIP_CRAWL_MOVE_FORWARD_LEFT);
            legacyAnimation.AddClip(defaultAnimations.crawlMoveForwardRightClip, CLIP_CRAWL_MOVE_FORWARD_RIGHT);
            legacyAnimation.AddClip(defaultAnimations.crawlMoveBackwardLeftClip, CLIP_CRAWL_MOVE_BACKWARD_LEFT);
            legacyAnimation.AddClip(defaultAnimations.crawlMoveBackwardRightClip, CLIP_CRAWL_MOVE_BACKWARD_RIGHT);
            legacyAnimation.AddClip(defaultAnimations.swimIdleClip, CLIP_SWIM_IDLE);
            legacyAnimation.AddClip(defaultAnimations.swimMoveClip, CLIP_SWIM_MOVE);
            legacyAnimation.AddClip(defaultAnimations.swimMoveBackwardClip, CLIP_SWIM_MOVE_BACKWARD);
            legacyAnimation.AddClip(defaultAnimations.swimMoveLeftClip, CLIP_SWIM_MOVE_LEFT);
            legacyAnimation.AddClip(defaultAnimations.swimMoveRightClip, CLIP_SWIM_MOVE_RIGHT);
            legacyAnimation.AddClip(defaultAnimations.swimMoveForwardLeftClip, CLIP_SWIM_MOVE_FORWARD_LEFT);
            legacyAnimation.AddClip(defaultAnimations.swimMoveForwardRightClip, CLIP_SWIM_MOVE_FORWARD_RIGHT);
            legacyAnimation.AddClip(defaultAnimations.swimMoveBackwardLeftClip, CLIP_SWIM_MOVE_BACKWARD_LEFT);
            legacyAnimation.AddClip(defaultAnimations.swimMoveBackwardRightClip, CLIP_SWIM_MOVE_BACKWARD_RIGHT);
            legacyAnimation.AddClip(defaultAnimations.jumpClip, CLIP_JUMP);
            legacyAnimation.AddClip(defaultAnimations.fallClip, CLIP_FALL);
            legacyAnimation.AddClip(defaultAnimations.hurtClip, CLIP_HURT);
            legacyAnimation.AddClip(defaultAnimations.deadClip, CLIP_DEAD);
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

            Item weaponItem = equipWeapons.GetRightHandWeaponItem();
            if (weaponItem == null)
                weaponItem = GameInstance.Singleton.DefaultWeaponItem;
            WeaponAnimations weaponAnimations = default(WeaponAnimations);
            GetAnims().CacheWeaponAnimations.TryGetValue(weaponItem.WeaponType.DataId, out weaponAnimations);
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
            if (legacyAnimation.GetClip(CLIP_JUMP) != null)
                legacyAnimation.RemoveClip(CLIP_JUMP);
            if (legacyAnimation.GetClip(CLIP_FALL) != null)
                legacyAnimation.RemoveClip(CLIP_FALL);
            if (legacyAnimation.GetClip(CLIP_HURT) != null)
                legacyAnimation.RemoveClip(CLIP_HURT);
            if (legacyAnimation.GetClip(CLIP_DEAD) != null)
                legacyAnimation.RemoveClip(CLIP_DEAD);
            // Setup generic clips
            legacyAnimation.AddClip(weaponAnimations.idleClip != null ? weaponAnimations.idleClip : defaultAnimations.idleClip, CLIP_IDLE);
            legacyAnimation.AddClip(weaponAnimations.moveClip != null ? weaponAnimations.moveClip : defaultAnimations.moveClip, CLIP_MOVE);
            legacyAnimation.AddClip(weaponAnimations.moveBackwardClip != null ? weaponAnimations.moveBackwardClip : defaultAnimations.moveBackwardClip, CLIP_MOVE_BACKWARD);
            legacyAnimation.AddClip(weaponAnimations.moveLeftClip != null ? weaponAnimations.moveLeftClip : defaultAnimations.moveLeftClip, CLIP_MOVE_LEFT);
            legacyAnimation.AddClip(weaponAnimations.moveRightClip != null ? weaponAnimations.moveRightClip : defaultAnimations.moveRightClip, CLIP_MOVE_RIGHT);
            legacyAnimation.AddClip(weaponAnimations.moveForwardLeftClip != null ? weaponAnimations.moveForwardLeftClip : defaultAnimations.moveForwardLeftClip, CLIP_MOVE_FORWARD_LEFT);
            legacyAnimation.AddClip(weaponAnimations.moveForwardRightClip != null ? weaponAnimations.moveForwardRightClip : defaultAnimations.moveForwardRightClip, CLIP_MOVE_FORWARD_RIGHT);
            legacyAnimation.AddClip(weaponAnimations.moveBackwardLeftClip != null ? weaponAnimations.moveBackwardLeftClip : defaultAnimations.moveBackwardLeftClip, CLIP_MOVE_BACKWARD_LEFT);
            legacyAnimation.AddClip(weaponAnimations.moveBackwardRightClip != null ? weaponAnimations.moveBackwardRightClip : defaultAnimations.moveBackwardRightClip, CLIP_MOVE_BACKWARD_RIGHT);
            legacyAnimation.AddClip(weaponAnimations.sprintClip != null ? weaponAnimations.sprintClip : defaultAnimations.sprintClip, CLIP_SPRINT);
            legacyAnimation.AddClip(weaponAnimations.sprintBackwardClip != null ? weaponAnimations.sprintBackwardClip : defaultAnimations.sprintBackwardClip, CLIP_SPRINT_BACKWARD);
            legacyAnimation.AddClip(weaponAnimations.sprintLeftClip != null ? weaponAnimations.sprintLeftClip : defaultAnimations.sprintLeftClip, CLIP_SPRINT_LEFT);
            legacyAnimation.AddClip(weaponAnimations.sprintRightClip != null ? weaponAnimations.sprintRightClip : defaultAnimations.sprintRightClip, CLIP_SPRINT_RIGHT);
            legacyAnimation.AddClip(weaponAnimations.sprintForwardLeftClip != null ? weaponAnimations.sprintForwardLeftClip : defaultAnimations.sprintForwardLeftClip, CLIP_SPRINT_FORWARD_LEFT);
            legacyAnimation.AddClip(weaponAnimations.sprintForwardRightClip != null ? weaponAnimations.sprintForwardRightClip : defaultAnimations.sprintForwardRightClip, CLIP_SPRINT_FORWARD_RIGHT);
            legacyAnimation.AddClip(weaponAnimations.sprintBackwardLeftClip != null ? weaponAnimations.sprintBackwardLeftClip : defaultAnimations.sprintBackwardLeftClip, CLIP_SPRINT_BACKWARD_LEFT);
            legacyAnimation.AddClip(weaponAnimations.sprintBackwardRightClip != null ? weaponAnimations.sprintBackwardRightClip : defaultAnimations.sprintBackwardRightClip, CLIP_SPRINT_BACKWARD_RIGHT);
            legacyAnimation.AddClip(weaponAnimations.crouchIdleClip != null ? weaponAnimations.crouchIdleClip : defaultAnimations.crouchIdleClip, CLIP_CROUCH_IDLE);
            legacyAnimation.AddClip(weaponAnimations.crouchMoveClip != null ? weaponAnimations.crouchMoveClip : defaultAnimations.crouchMoveClip, CLIP_CROUCH_MOVE);
            legacyAnimation.AddClip(weaponAnimations.crouchMoveBackwardClip != null ? weaponAnimations.crouchMoveBackwardClip : defaultAnimations.crouchMoveBackwardClip, CLIP_CROUCH_MOVE_BACKWARD);
            legacyAnimation.AddClip(weaponAnimations.crouchMoveLeftClip != null ? weaponAnimations.crouchMoveLeftClip : defaultAnimations.crouchMoveLeftClip, CLIP_CROUCH_MOVE_LEFT);
            legacyAnimation.AddClip(weaponAnimations.crouchMoveRightClip != null ? weaponAnimations.crouchMoveRightClip : defaultAnimations.crouchMoveRightClip, CLIP_CROUCH_MOVE_RIGHT);
            legacyAnimation.AddClip(weaponAnimations.crouchMoveForwardLeftClip != null ? weaponAnimations.crouchMoveForwardLeftClip : defaultAnimations.crouchMoveForwardLeftClip, CLIP_CROUCH_MOVE_FORWARD_LEFT);
            legacyAnimation.AddClip(weaponAnimations.crouchMoveForwardRightClip != null ? weaponAnimations.crouchMoveForwardRightClip : defaultAnimations.crouchMoveForwardRightClip, CLIP_CROUCH_MOVE_FORWARD_RIGHT);
            legacyAnimation.AddClip(weaponAnimations.crouchMoveBackwardLeftClip != null ? weaponAnimations.crouchMoveBackwardLeftClip : defaultAnimations.crouchMoveBackwardLeftClip, CLIP_CROUCH_MOVE_BACKWARD_LEFT);
            legacyAnimation.AddClip(weaponAnimations.crouchMoveBackwardRightClip != null ? weaponAnimations.crouchMoveBackwardRightClip : defaultAnimations.crouchMoveBackwardRightClip, CLIP_CROUCH_MOVE_BACKWARD_RIGHT);
            legacyAnimation.AddClip(weaponAnimations.crawlIdleClip != null ? weaponAnimations.crawlIdleClip : defaultAnimations.crawlIdleClip, CLIP_CRAWL_IDLE);
            legacyAnimation.AddClip(weaponAnimations.crawlMoveClip != null ? weaponAnimations.crawlMoveClip : defaultAnimations.crawlMoveClip, CLIP_CRAWL_MOVE);
            legacyAnimation.AddClip(weaponAnimations.crawlMoveBackwardClip != null ? weaponAnimations.crawlMoveBackwardClip : defaultAnimations.crawlMoveBackwardClip, CLIP_CRAWL_MOVE_BACKWARD);
            legacyAnimation.AddClip(weaponAnimations.crawlMoveLeftClip != null ? weaponAnimations.crawlMoveLeftClip : defaultAnimations.crawlMoveLeftClip, CLIP_CRAWL_MOVE_LEFT);
            legacyAnimation.AddClip(weaponAnimations.crawlMoveRightClip != null ? weaponAnimations.crawlMoveRightClip : defaultAnimations.crawlMoveRightClip, CLIP_CRAWL_MOVE_RIGHT);
            legacyAnimation.AddClip(weaponAnimations.crawlMoveForwardLeftClip != null ? weaponAnimations.crawlMoveForwardLeftClip : defaultAnimations.crawlMoveForwardLeftClip, CLIP_CRAWL_MOVE_FORWARD_LEFT);
            legacyAnimation.AddClip(weaponAnimations.crawlMoveForwardRightClip != null ? weaponAnimations.crawlMoveForwardRightClip : defaultAnimations.crawlMoveForwardRightClip, CLIP_CRAWL_MOVE_FORWARD_RIGHT);
            legacyAnimation.AddClip(weaponAnimations.crawlMoveBackwardLeftClip != null ? weaponAnimations.crawlMoveBackwardLeftClip : defaultAnimations.crawlMoveBackwardLeftClip, CLIP_CRAWL_MOVE_BACKWARD_LEFT);
            legacyAnimation.AddClip(weaponAnimations.crawlMoveBackwardRightClip != null ? weaponAnimations.crawlMoveBackwardRightClip : defaultAnimations.crawlMoveBackwardRightClip, CLIP_CRAWL_MOVE_BACKWARD_RIGHT);
            legacyAnimation.AddClip(weaponAnimations.swimIdleClip != null ? weaponAnimations.swimIdleClip : defaultAnimations.swimIdleClip, CLIP_SWIM_IDLE);
            legacyAnimation.AddClip(weaponAnimations.swimMoveClip != null ? weaponAnimations.swimMoveClip : defaultAnimations.swimMoveClip, CLIP_SWIM_MOVE);
            legacyAnimation.AddClip(weaponAnimations.swimMoveBackwardClip != null ? weaponAnimations.swimMoveBackwardClip : defaultAnimations.swimMoveBackwardClip, CLIP_SWIM_MOVE_BACKWARD);
            legacyAnimation.AddClip(weaponAnimations.swimMoveLeftClip != null ? weaponAnimations.swimMoveLeftClip : defaultAnimations.swimMoveLeftClip, CLIP_SWIM_MOVE_LEFT);
            legacyAnimation.AddClip(weaponAnimations.swimMoveRightClip != null ? weaponAnimations.swimMoveRightClip : defaultAnimations.swimMoveRightClip, CLIP_SWIM_MOVE_RIGHT);
            legacyAnimation.AddClip(weaponAnimations.swimMoveForwardLeftClip != null ? weaponAnimations.swimMoveForwardLeftClip : defaultAnimations.swimMoveForwardLeftClip, CLIP_SWIM_MOVE_FORWARD_LEFT);
            legacyAnimation.AddClip(weaponAnimations.swimMoveForwardRightClip != null ? weaponAnimations.swimMoveForwardRightClip : defaultAnimations.swimMoveForwardRightClip, CLIP_SWIM_MOVE_FORWARD_RIGHT);
            legacyAnimation.AddClip(weaponAnimations.swimMoveBackwardLeftClip != null ? weaponAnimations.swimMoveBackwardLeftClip : defaultAnimations.swimMoveBackwardLeftClip, CLIP_SWIM_MOVE_BACKWARD_LEFT);
            legacyAnimation.AddClip(weaponAnimations.swimMoveBackwardRightClip != null ? weaponAnimations.swimMoveBackwardRightClip : defaultAnimations.swimMoveBackwardRightClip, CLIP_SWIM_MOVE_BACKWARD_RIGHT);
            legacyAnimation.AddClip(weaponAnimations.jumpClip != null ? weaponAnimations.jumpClip : defaultAnimations.jumpClip, CLIP_JUMP);
            legacyAnimation.AddClip(weaponAnimations.fallClip != null ? weaponAnimations.fallClip : defaultAnimations.fallClip, CLIP_FALL);
            legacyAnimation.AddClip(weaponAnimations.hurtClip != null ? weaponAnimations.hurtClip : defaultAnimations.hurtClip, CLIP_HURT);
            legacyAnimation.AddClip(weaponAnimations.deadClip != null ? weaponAnimations.deadClip : defaultAnimations.deadClip, CLIP_DEAD);
            CrossFadeLegacyAnimation(CLIP_IDLE, 0, WrapMode.Loop);
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

                if (movementState.HasFlag(MovementState.IsUnderWater))
                {
                    CrossFadeMoveAnimaton(CLIP_SWIM_IDLE, CLIP_SWIM_MOVE, CLIP_SWIM_MOVE_BACKWARD, CLIP_SWIM_MOVE_LEFT, CLIP_SWIM_MOVE_RIGHT,
                        CLIP_SWIM_MOVE_FORWARD_LEFT, CLIP_SWIM_MOVE_FORWARD_RIGHT, CLIP_SWIM_MOVE_BACKWARD_LEFT, CLIP_SWIM_MOVE_BACKWARD_RIGHT);
                }
                else if (!movementState.HasFlag(MovementState.IsGrounded))
                {
                    CrossFadeLegacyAnimation(CLIP_FALL, fallClipFadeLength, WrapMode.Loop);
                }
                else
                {
                    switch (extraMovementState)
                    {
                        case ExtraMovementState.IsSprinting:
                            CrossFadeMoveAnimaton(CLIP_IDLE, CLIP_SPRINT, CLIP_SPRINT_BACKWARD, CLIP_SPRINT_LEFT, CLIP_SPRINT_RIGHT,
                                CLIP_SPRINT_FORWARD_LEFT, CLIP_SPRINT_FORWARD_RIGHT, CLIP_SPRINT_BACKWARD_LEFT, CLIP_SPRINT_BACKWARD_RIGHT);
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
            if (movementState.HasFlag(MovementState.Forward) && movementState.HasFlag(MovementState.Right))
                CrossFadeLegacyAnimation(clipMoveForwardRight, moveClipFadeLength, WrapMode.Loop);
            // Forward Left
            else if (movementState.HasFlag(MovementState.Forward) && movementState.HasFlag(MovementState.Left))
                CrossFadeLegacyAnimation(clipMoveForwardLeft, moveClipFadeLength, WrapMode.Loop);
            // Backward Right
            else if (movementState.HasFlag(MovementState.Backward) && movementState.HasFlag(MovementState.Right))
                CrossFadeLegacyAnimation(clipMoveBackwardRight, moveClipFadeLength, WrapMode.Loop);
            // Backward Left
            else if (movementState.HasFlag(MovementState.Backward) && movementState.HasFlag(MovementState.Left))
                CrossFadeLegacyAnimation(clipMoveBackwardLeft, moveClipFadeLength, WrapMode.Loop);
            // Forward
            else if (movementState.HasFlag(MovementState.Forward))
                CrossFadeLegacyAnimation(clipMove, moveClipFadeLength, WrapMode.Loop);
            // Backward
            else if (movementState.HasFlag(MovementState.Backward))
                CrossFadeLegacyAnimation(clipMoveBackward, moveClipFadeLength, WrapMode.Loop);
            // Right
            else if (movementState.HasFlag(MovementState.Right))
                CrossFadeLegacyAnimation(clipMoveRight, moveClipFadeLength, WrapMode.Loop);
            // Left
            else if (movementState.HasFlag(MovementState.Left))
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

        public override Coroutine PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1f)
        {
            return StartCoroutine(PlayActionAnimation_LegacyAnimation(animActionType, dataId, index, playSpeedMultiplier));
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
            AudioClip audioClip = tempActionAnimation.GetRandomAudioClip();
            if (audioClip != null)
                AudioSource.PlayClipAtPoint(audioClip, CacheTransform.position, AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level);
            isPlayingActionAnimation = true;
            if (tempActionAnimation.clip != null)
                CrossFadeLegacyAnimation(CLIP_ACTION, actionClipFadeLength, WrapMode.Once);
            // Waits by current transition + clip duration before end animation
            yield return new WaitForSecondsRealtime(tempActionAnimation.GetClipLength() / playSpeedMultiplier);
            if (tempActionAnimation.clip != null)
                CrossFadeLegacyAnimation(CLIP_IDLE, idleClipFadeLength, WrapMode.Loop);
            // Waits by current transition + extra duration before end playing animation state
            yield return new WaitForSecondsRealtime(tempActionAnimation.GetExtraDuration() / playSpeedMultiplier);
            isPlayingActionAnimation = false;
        }

        public override Coroutine PlaySkillCastClip(int dataId, float duration)
        {
            return StartCoroutine(PlaySkillCastClip_LegacyAnimation(dataId, duration));
        }

        private IEnumerator PlaySkillCastClip_LegacyAnimation(int dataId, float duration)
        {
            AnimationClip castClip = GetSkillCastClip(dataId);
            if (legacyAnimation.GetClip(CLIP_CAST_SKILL) != null)
                legacyAnimation.RemoveClip(CLIP_CAST_SKILL);
            legacyAnimation.AddClip(castClip, CLIP_CAST_SKILL);
            CrossFadeLegacyAnimation(CLIP_CAST_SKILL, actionClipFadeLength, WrapMode.Loop);
            yield return new WaitForSecondsRealtime(duration);
            if (!isPlayingActionAnimation)
                CrossFadeLegacyAnimation(CLIP_IDLE, idleClipFadeLength, WrapMode.Loop);
        }

        public override void StopActionAnimation()
        {
            CrossFadeLegacyAnimation(CLIP_IDLE, idleClipFadeLength, WrapMode.Loop);
            isPlayingActionAnimation = false;
        }

        public override void StopSkillCastAnimation()
        {
            CrossFadeLegacyAnimation(CLIP_IDLE, idleClipFadeLength, WrapMode.Loop);
        }

        public override void PlayHitAnimation()
        {
            CrossFadeLegacyAnimation(CLIP_HURT, hurtClipFadeLength, WrapMode.Once);
        }

        public override void PlayJumpAnimation()
        {
            CrossFadeLegacyAnimation(CLIP_JUMP, jumpClipFadeLength, WrapMode.Once);
        }
    }
}
