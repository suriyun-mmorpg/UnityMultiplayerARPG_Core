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
            SetupComponent();
            base.Awake();
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
            Item weaponItem = GameInstance.Singleton.DefaultWeaponItem;
            if (equipWeapons.rightHand.NotEmptySlot() && equipWeapons.rightHand.GetWeaponItem() != null)
                weaponItem = equipWeapons.rightHand.GetWeaponItem();
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
                if (!movementState.HasFlag(MovementState.IsGrounded))
                    CrossFadeLegacyAnimation(CLIP_FALL, fallClipFadeLength, WrapMode.Loop);
                else
                {
                    // Forward Right
                    if (movementState.HasFlag(MovementState.Forward) && movementState.HasFlag(MovementState.Right))
                        CrossFadeLegacyAnimation(CLIP_MOVE_FORWARD_RIGHT, moveClipFadeLength, WrapMode.Loop);
                    // Forward Left
                    else if (movementState.HasFlag(MovementState.Forward) && movementState.HasFlag(MovementState.Left))
                        CrossFadeLegacyAnimation(CLIP_MOVE_FORWARD_LEFT, moveClipFadeLength, WrapMode.Loop);
                    // Backward Right
                    else if (movementState.HasFlag(MovementState.Backward) && movementState.HasFlag(MovementState.Right))
                        CrossFadeLegacyAnimation(CLIP_MOVE_BACKWARD_RIGHT, moveClipFadeLength, WrapMode.Loop);
                    // Backward Left
                    else if (movementState.HasFlag(MovementState.Backward) && movementState.HasFlag(MovementState.Left))
                        CrossFadeLegacyAnimation(CLIP_MOVE_BACKWARD_LEFT, moveClipFadeLength, WrapMode.Loop);
                    // Forward
                    else if (movementState.HasFlag(MovementState.Forward))
                        CrossFadeLegacyAnimation(CLIP_MOVE, moveClipFadeLength, WrapMode.Loop);
                    // Backward
                    else if (movementState.HasFlag(MovementState.Backward))
                        CrossFadeLegacyAnimation(CLIP_MOVE_BACKWARD, moveClipFadeLength, WrapMode.Loop);
                    // Right
                    else if (movementState.HasFlag(MovementState.Right))
                        CrossFadeLegacyAnimation(CLIP_MOVE_RIGHT, moveClipFadeLength, WrapMode.Loop);
                    // Left
                    else if (movementState.HasFlag(MovementState.Left))
                        CrossFadeLegacyAnimation(CLIP_MOVE_LEFT, moveClipFadeLength, WrapMode.Loop);
                    // Idle
                    else
                        CrossFadeLegacyAnimation(CLIP_IDLE, idleClipFadeLength, WrapMode.Loop);
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
