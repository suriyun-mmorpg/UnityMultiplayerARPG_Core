using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterModel2D : CharacterModel
    {
        [Header("2D Animations")]
        [SerializeField]
        private SpriteRenderer spriteRenderer;
        [SerializeField]
        private CharacterAnimation2D idleAnimation2D;
        [SerializeField]
        private CharacterAnimation2D moveAnimation2D;
        [SerializeField]
        private CharacterAnimation2D deadAnimation2D;
        [SerializeField]
        private ActionAnimation2D defaultAttackAnimation2D;
        [SerializeField]
        private ActionAnimation2D defaultSkillCastAnimation2D;
        [SerializeField]
        private WeaponAnimations2D[] weaponAnimations2D;
        [SerializeField]
        private SkillCastAnimations2D[] skillCastAnimations2D;

        private Dictionary<int, ActionAnimation2D> cacheWeaponAnimations2D;
        public Dictionary<int, ActionAnimation2D> CacheWeaponAnimations2D
        {
            get
            {
                if (cacheWeaponAnimations2D == null)
                {
                    cacheWeaponAnimations2D = new Dictionary<int, ActionAnimation2D>();
                    foreach (var attackAnimation in weaponAnimations2D)
                    {
                        if (attackAnimation.weaponType == null) continue;
                        cacheWeaponAnimations2D[attackAnimation.weaponType.DataId] = attackAnimation.animation;
                    }
                }
                return cacheWeaponAnimations2D;
            }
        }

        private Dictionary<int, ActionAnimation2D> cacheSkillCastAnimations2D;
        public Dictionary<int, ActionAnimation2D> CacheSkillCastAnimations2D
        {
            get
            {
                if (cacheSkillCastAnimations2D == null)
                {
                    cacheSkillCastAnimations2D = new Dictionary<int, ActionAnimation2D>();
                    foreach (var skillCastAnimation in skillCastAnimations2D)
                    {
                        if (skillCastAnimation.skill == null) continue;
                        cacheSkillCastAnimations2D[skillCastAnimation.skill.DataId] = skillCastAnimation.animation;
                    }
                }
                return cacheSkillCastAnimations2D;
            }
        }

        private DirectionType currentDirection = DirectionType.Down;
        private Anim2D playingAnim;
        private int currentFrame;
        bool playing;
        float secsPerFrame;
        float nextFrameTime;

        void Update()
        {
            if (!playing || Time.time < nextFrameTime || spriteRenderer == null) return;
            currentFrame++;
            if (currentFrame >= playingAnim.frames.Length)
            {
                if (!playingAnim.loop)
                {
                    playing = false;
                    return;
                }
                currentFrame = 0;
            }
            spriteRenderer.sprite = playingAnim.frames[currentFrame];
            nextFrameTime += secsPerFrame;
        }

        public void Play(Anim2D anim)
        {
            playingAnim = anim;

            secsPerFrame = 1f / anim.framesPerSec;
            currentFrame = -1;
            playing = true;
            nextFrameTime = Time.time;
        }

        public void Stop()
        {
            playing = false;
        }

        public void Resume()
        {
            playing = true;
            nextFrameTime = Time.time + secsPerFrame;
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
            if (isDead)
                Play(deadAnimation2D.GetClipByDirection(currentDirection));
            else
            {
                if (moveVelocity.magnitude > 0)
                    Play(moveAnimation2D.GetClipByDirection(currentDirection));
                else
                    Play(idleAnimation2D.GetClipByDirection(currentDirection));
            }
        }

        public override Coroutine PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1)
        {
            return StartCoroutine(PlayActionAnimationRoutine(animActionType, dataId, index, playSpeedMultiplier));
        }

        private ActionAnimation2D GetActionAnimation(AnimActionType animActionType, int dataId)
        {
            ActionAnimation2D animation2D = null;
            switch (animActionType)
            {
                case AnimActionType.AttackRightHand:
                case AnimActionType.AttackLeftHand:
                    if (!CacheWeaponAnimations2D.TryGetValue(dataId, out animation2D))
                        animation2D = defaultAttackAnimation2D;
                    break;
                case AnimActionType.Skill:
                    if (!CacheSkillCastAnimations2D.TryGetValue(dataId, out animation2D))
                        animation2D = defaultSkillCastAnimation2D;
                    break;
            }
            return animation2D;
        }

        IEnumerator PlayActionAnimationRoutine(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier)
        {
            // If animator is not null, play the action animation
            var animation = GetActionAnimation(animActionType, dataId);
            if (animation != null)
            {
                var anim = animation.GetClipByDirection(currentDirection);
                if (anim != null)
                {
                    // Waits by current transition + clip duration before end animation
                    Play(anim);
                    yield return new WaitForSecondsRealtime(anim.duration / playSpeedMultiplier);
                    Play(idleAnimation2D.GetClipByDirection(currentDirection));
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
