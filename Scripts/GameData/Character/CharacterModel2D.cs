using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterModel2D : CharacterModel
    {
        public CharacterAnimation2D idleAnimation;
        public CharacterAnimation2D moveAnimation;
        public CharacterAnimation2D deadAnimation;
        public CharacterAnimation2D defaultAttackAnimation;
        public CharacterAnimation2D defaultSkillCastAnimation;
        public WeaponAttack2D[] attackAnimations;
        public SkillCast2D[] skillCastAnimations;

        private static readonly Dictionary<int, Dictionary<int, CharacterAnimation2D>> cacheAttackAnimations = new Dictionary<int, Dictionary<int, CharacterAnimation2D>>();
        private static readonly Dictionary<int, Dictionary<int, CharacterAnimation2D>> cacheSkillCastAnimations = new Dictionary<int, Dictionary<int, CharacterAnimation2D>>();

        public Dictionary<int, CharacterAnimation2D> CacheAttackAnimations
        {
            get
            {
                if (!cacheAttackAnimations.ContainsKey(DataId))
                {
                    cacheAttackAnimations.Add(DataId, new Dictionary<int, CharacterAnimation2D>());
                    foreach (var attackAnimation in attackAnimations)
                    {
                        if (attackAnimation.weaponType == null) continue;
                        cacheAttackAnimations[DataId][attackAnimation.weaponType.DataId] = attackAnimation.animation;
                    }
                }
                return cacheAttackAnimations[DataId];
            }
        }

        public Dictionary<int, CharacterAnimation2D> CacheSkillCastAnimations
        {
            get
            {
                if (!cacheSkillCastAnimations.ContainsKey(DataId))
                {
                    cacheSkillCastAnimations.Add(DataId, new Dictionary<int, CharacterAnimation2D>());
                    foreach (var skillCastAnimation in skillCastAnimations)
                    {
                        if (skillCastAnimation.skill == null) continue;
                        cacheSkillCastAnimations[DataId][skillCastAnimation.skill.DataId] = skillCastAnimation.animation;
                    }
                }
                return cacheSkillCastAnimations[DataId];
            }
        }

        private DirectionType currentDirection = DirectionType.Down;
        private AnimationClip playingClip;

        public void Play2DAnim_Animator(AnimationClip clip)
        {
            if (playingClip == null || playingClip == clip) return;
            CacheAnimator.enabled = false;
            CacheAnimatorController[ANIM_STATE_ACTION_CLIP] = clip;
            CacheAnimator.enabled = true;
        }

        public void Play2DAnim_Animation(AnimationClip clip)
        {
            if (playingClip == null || playingClip == clip) return;
            if (CacheAnimation.GetClip(LEGACY_CLIP_ACTION) != null)
                CacheAnimation.RemoveClip(LEGACY_CLIP_ACTION);
            CacheAnimation.AddClip(clip, LEGACY_CLIP_ACTION);
            CacheAnimation.Play(LEGACY_CLIP_ACTION);
        }

        private void UpdateDirection(Vector3 moveVelocity)
        {
            if (moveVelocity.magnitude > 0f)
            {
                var normalized = moveVelocity.normalized;
                if (Mathf.Abs(normalized.x) >= Mathf.Abs(normalized.y))
                {
                    if (normalized.x < 0) currentDirection = DirectionType.Left;
                    if (normalized.x > 0) currentDirection = DirectionType.Right;
                }
                else
                {
                    if (normalized.y < 0) currentDirection = DirectionType.Down;
                    if (normalized.y > 0) currentDirection = DirectionType.Up;
                }
            }
        }

        public override void UpdateAnimation(bool isDead, Vector3 moveVelocity, float playMoveSpeedMultiplier = 1)
        {
            UpdateDirection(moveVelocity);
            switch (animatorType)
            {
                case AnimatorType.Animator:
                    UpdateAnimation_Animator(isDead, moveVelocity, playMoveSpeedMultiplier);
                    break;
                case AnimatorType.LegacyAnimtion:
                    UpdateAnimation_LegacyAnimation(isDead, moveVelocity, playMoveSpeedMultiplier);
                    break;
            }
        }

        private void UpdateAnimation_Animator(bool isDead, Vector3 moveVelocity, float playMoveSpeedMultiplier)
        {
            if (isDead)
                Play2DAnim_Animator(deadAnimation.GetClipByDirection(currentDirection));
            else
            {
                if (moveVelocity.magnitude > 0)
                    Play2DAnim_Animator(moveAnimation.GetClipByDirection(currentDirection));
                else
                    Play2DAnim_Animator(idleAnimation.GetClipByDirection(currentDirection));
            }
        }

        private void UpdateAnimation_LegacyAnimation(bool isDead, Vector3 moveVelocity, float playMoveSpeedMultiplier)
        {
            if (isDead)
                Play2DAnim_Animation(deadAnimation.GetClipByDirection(currentDirection));
            else
            {
                if (moveVelocity.magnitude > 0)
                    Play2DAnim_Animation(moveAnimation.GetClipByDirection(currentDirection));
                else
                    Play2DAnim_Animation(idleAnimation.GetClipByDirection(currentDirection));
            }
        }

        public override Coroutine PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1)
        {
            if (animatorType == AnimatorType.LegacyAnimtion)
                return StartCoroutine(PlayActionAnimation_LegacyAnimation(animActionType, dataId, index, playSpeedMultiplier));
            return StartCoroutine(PlayActionAnimation_Animator(animActionType, dataId, index, playSpeedMultiplier));
        }

        private CharacterAnimation2D GetActionAnimation(AnimActionType animActionType, int dataId)
        {
            CharacterAnimation2D animation2D = null;
            switch (animActionType)
            {
                case AnimActionType.AttackRightHand:
                case AnimActionType.AttackLeftHand:
                case AnimActionType.MonsterAttack:
                    if (!CacheAttackAnimations.TryGetValue(dataId, out animation2D))
                        animation2D = defaultAttackAnimation;
                    break;
                case AnimActionType.Skill:
                    if (!CacheSkillCastAnimations.TryGetValue(dataId, out animation2D))
                        animation2D = defaultSkillCastAnimation;
                    break;
            }
            return animation2D;
        }

        private IEnumerator PlayActionAnimation_Animator(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier)
        {
            // If animator is not null, play the action animation
            var animation = GetActionAnimation(animActionType, dataId);
            if (animation != null)
            {
                AnimationClip clip = animation.GetClipByDirection(currentDirection);
                if (clip != null)
                {
                    // Waits by current transition + clip duration before end animation
                    Play2DAnim_Animator(clip);
                    yield return new WaitForSecondsRealtime(clip.length / playSpeedMultiplier);
                    Play2DAnim_Animator(idleAnimation.GetClipByDirection(currentDirection));
                    yield return new WaitForSecondsRealtime(animation.extraDuration / playSpeedMultiplier);
                }
            }
        }

        private IEnumerator PlayActionAnimation_LegacyAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier)
        {
            // If animator is not null, play the action animation
            var animation = GetActionAnimation(animActionType, dataId);
            if (animation != null)
            {
                AnimationClip clip = animation.GetClipByDirection(currentDirection);
                if (clip != null)
                {
                    // Waits by current transition + clip duration before end animation
                    Play2DAnim_Animation(clip);
                    yield return new WaitForSecondsRealtime(clip.length / playSpeedMultiplier);
                    Play2DAnim_Animation(idleAnimation.GetClipByDirection(currentDirection));
                    yield return new WaitForSecondsRealtime(animation.extraDuration / playSpeedMultiplier);
                }
            }
        }

        public override void PlayHurtAnimation()
        {
            // TODO: 2D may just play blink red color
        }

        public override void PlayJumpAnimation()
        {
            // TODO: 2D may able to jump
        }
    }
}
