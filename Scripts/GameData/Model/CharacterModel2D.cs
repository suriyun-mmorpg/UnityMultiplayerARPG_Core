using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [ExecuteInEditMode]
    public class CharacterModel2D : BaseCharacterModel
    {
        public enum SampleAnimation
        {
            Idle,
            Move,
            Dead,
            DefaultAttack,
            DefaultSkillCast,

        }
        [Header("2D Animations")]
        public SpriteRenderer spriteRenderer;
        public CharacterAnimation2D idleAnimation2D;
        public CharacterAnimation2D moveAnimation2D;
        public CharacterAnimation2D deadAnimation2D;
        public ActionAnimation2D defaultAttackAnimation2D;
        public ActionAnimation2D defaultSkillCastAnimation2D;
        public ActionAnimation2D defaultReloadAnimation2D;
        public WeaponAnimations2D[] weaponAnimations2D;
        public SkillCastAnimations2D[] skillCastAnimations2D;
        public float magnitudeToPlayMoveClip = 0.1f;
        [Header("Sample 2D Animations")]
        public SampleAnimation sampleAnimation = SampleAnimation.Idle;
        public DirectionType sampleDirection = DirectionType.Down;
        public bool sampleAlwaysLoop = true;

        private Dictionary<int, ActionAnimation2D> cacheRightHandAttackAnimations;
        public Dictionary<int, ActionAnimation2D> CacheRightHandAttackAnimations
        {
            get
            {
                if (cacheRightHandAttackAnimations == null)
                {
                    cacheRightHandAttackAnimations = new Dictionary<int, ActionAnimation2D>();
                    foreach (WeaponAnimations2D attackAnimation in weaponAnimations2D)
                    {
                        if (attackAnimation.weaponType == null) continue;
                        cacheRightHandAttackAnimations[attackAnimation.weaponType.DataId] = attackAnimation.rightHandAttackAnimation;
                    }
                }
                return cacheRightHandAttackAnimations;
            }
        }

        private Dictionary<int, ActionAnimation2D> cacheLeftHandAttackAnimations;
        public Dictionary<int, ActionAnimation2D> CacheLeftHandAttackAnimations
        {
            get
            {
                if (cacheLeftHandAttackAnimations == null)
                {
                    cacheLeftHandAttackAnimations = new Dictionary<int, ActionAnimation2D>();
                    foreach (WeaponAnimations2D attackAnimation in weaponAnimations2D)
                    {
                        if (attackAnimation.weaponType == null) continue;
                        cacheLeftHandAttackAnimations[attackAnimation.weaponType.DataId] = attackAnimation.leftHandAttackAnimation;
                    }
                }
                return cacheLeftHandAttackAnimations;
            }
        }

        private Dictionary<int, ActionAnimation2D> cacheReloadAnimations;
        public Dictionary<int, ActionAnimation2D> CacheReloadAnimations
        {
            get
            {
                if (cacheReloadAnimations == null)
                {
                    cacheReloadAnimations = new Dictionary<int, ActionAnimation2D>();
                    foreach (WeaponAnimations2D attackAnimation in weaponAnimations2D)
                    {
                        if (attackAnimation.weaponType == null) continue;
                        cacheReloadAnimations[attackAnimation.weaponType.DataId] = attackAnimation.reloadAnimation;
                    }
                }
                return cacheReloadAnimations;
            }
        }

        private Dictionary<int, ActionAnimation2D> cacheSkillCastAnimations;
        public Dictionary<int, ActionAnimation2D> CacheSkillCastAnimations
        {
            get
            {
                if (cacheSkillCastAnimations == null)
                {
                    cacheSkillCastAnimations = new Dictionary<int, ActionAnimation2D>();
                    foreach (SkillCastAnimations2D skillCastAnimation in skillCastAnimations2D)
                    {
                        if (skillCastAnimation.skill == null) continue;
                        cacheSkillCastAnimations[skillCastAnimation.skill.DataId] = skillCastAnimation.animation;
                    }
                }
                return cacheSkillCastAnimations;
            }
        }
        
        [HideInInspector, System.NonSerialized]
        public DirectionType currentDirectionType = DirectionType.Down;
        private AnimationClip2D playingAnim = null;
        private int currentFrame = 0;
        private bool playing = false;
        private bool playingAction = false;
        private float secsPerFrame = 0;
        private float nextFrameTime = 0;
        private SampleAnimation? dirtySampleAnimation;
        private DirectionType? dirtySampleType;

        private void Start()
        {
            Play(idleAnimation2D, DirectionType.Down);
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.update += EditorUpdate;
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
#endif
        }

        void EditorUpdate()
        {
            if (!Application.isPlaying)
            {
                UpdateSample();
                Update();
            }
        }

        void Update()
        {
            if (spriteRenderer == null)
                return;
#if UNITY_EDITOR
            if (!Application.isPlaying && sampleAlwaysLoop)
                playing = true;
#endif
            // Increase next frame time while pause
            if (!playing)
            {
                nextFrameTime += Time.deltaTime;
                return;
            }
            // Is is time to play next frame?
            float time = Time.realtimeSinceStartup;
            if (time < nextFrameTime)
                return;
            // Play next frame
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
            nextFrameTime = time + secsPerFrame;
        }

        private void UpdateSample()
        {
            if (dirtySampleAnimation.HasValue &&
                dirtySampleAnimation.Value == sampleAnimation &&
                dirtySampleType.HasValue &&
                dirtySampleType.Value == sampleDirection)
                return;
            dirtySampleAnimation = sampleAnimation;
            dirtySampleType = sampleDirection;
            switch (sampleAnimation)
            {
                case SampleAnimation.Idle:
                    Play(idleAnimation2D, sampleDirection);
                    break;
                case SampleAnimation.Move:
                    Play(moveAnimation2D, sampleDirection);
                    break;
                case SampleAnimation.Dead:
                    Play(deadAnimation2D, sampleDirection);
                    break;
                case SampleAnimation.DefaultAttack:
                    Play(defaultAttackAnimation2D, sampleDirection);
                    break;
                case SampleAnimation.DefaultSkillCast:
                    Play(defaultSkillCastAnimation2D, sampleDirection);
                    break;
            }
        }

        public void Play(CharacterAnimation2D animation, DirectionType directionType)
        {
            if (animation == null)
                return;
            Play(animation.GetClipByDirection(directionType));
        }

        public void Play(AnimationClip2D anim)
        {
            if (anim == playingAnim)
                return;

            playingAnim = anim;
            spriteRenderer.flipX = anim.flipX;
            spriteRenderer.flipY = anim.flipY;
            secsPerFrame = 1f / anim.framesPerSec;
            currentFrame = -1;
            playing = true;
            nextFrameTime = Time.realtimeSinceStartup + secsPerFrame;
        }

        public void Stop()
        {
            playing = false;
        }

        public void Resume()
        {
            playing = true;
        }

        public override void UpdateAnimation(bool isDead, MovementFlag movementState, float playMoveSpeedMultiplier = 1)
        {
            if (playingAction)
                return;
            if (isDead)
                Play(deadAnimation2D, currentDirectionType);
            else
            {
                if (movementState.HasFlag(MovementFlag.Forward) ||
                    movementState.HasFlag(MovementFlag.Backward) ||
                    movementState.HasFlag(MovementFlag.Right) ||
                    movementState.HasFlag(MovementFlag.Left))
                    Play(moveAnimation2D, currentDirectionType);
                else
                    Play(idleAnimation2D, currentDirectionType);
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
                    if (!CacheRightHandAttackAnimations.TryGetValue(dataId, out animation2D))
                        animation2D = defaultAttackAnimation2D;
                    break;
                case AnimActionType.AttackLeftHand:
                    if (!CacheLeftHandAttackAnimations.TryGetValue(dataId, out animation2D))
                        animation2D = defaultAttackAnimation2D;
                    break;
                case AnimActionType.Skill:
                    if (!CacheSkillCastAnimations.TryGetValue(dataId, out animation2D))
                        animation2D = defaultSkillCastAnimation2D;
                    break;
                case AnimActionType.Reload:
                    if (!CacheReloadAnimations.TryGetValue(dataId, out animation2D))
                        animation2D = defaultReloadAnimation2D;
                    break;
            }
            return animation2D;
        }

        IEnumerator PlayActionAnimationRoutine(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier)
        {
            // If animator is not null, play the action animation
            ActionAnimation2D animation = GetActionAnimation(animActionType, dataId);
            if (animation != null)
            {
                AnimationClip2D anim = animation.GetClipByDirection(currentDirectionType);
                if (anim != null)
                {
                    playingAction = true;
                    AudioClip audioClip = animation.GetRandomAudioClip();
                    if (audioClip != null)
                        AudioSource.PlayClipAtPoint(audioClip, CacheTransform.position, AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level);
                    // Waits by current transition + clip duration before end animation
                    Play(anim);
                    yield return new WaitForSecondsRealtime(anim.duration / playSpeedMultiplier);
                    Play(idleAnimation2D, currentDirectionType);
                    yield return new WaitForSecondsRealtime(animation.extraDuration / playSpeedMultiplier);
                    playingAction = false;
                }
            }
        }

        public override Coroutine PlaySkillCastClip(int dataId, float duration)
        {
            return StartCoroutine(PlaySkillCastClipRoutine(dataId, duration));
        }

        IEnumerator PlaySkillCastClipRoutine(int dataId, float duration)
        {
            playingAction = true;
            yield return new WaitForSecondsRealtime(duration);
            playingAction = false;
        }

        public override void StopActionAnimation()
        {
            playingAction = false;
        }

        public override void PlayHurtAnimation()
        {
            // TODO: 2D may just play blink red color
        }

        public override void PlayJumpAnimation()
        {
            // TODO: 2D may able to jump
        }

        public override bool GetRandomRightHandAttackAnimation(int dataId, out int animationIndex, out float triggerDuration, out float totalDuration)
        {
            ActionAnimation2D animation2D = null;
            if (!CacheRightHandAttackAnimations.TryGetValue(dataId, out animation2D))
                animation2D = defaultAttackAnimation2D;
            animationIndex = 0;
            triggerDuration = 0f;
            totalDuration = 0f;
            if (animation2D == null) return false;
            AnimationClip2D clip = animation2D.GetClipByDirection(currentDirectionType);
            if (clip == null) return false;
            triggerDuration = clip.duration * animation2D.triggerDurationRate;
            totalDuration = clip.duration + animation2D.extraDuration;
            return true;
        }

        public override bool GetRandomLeftHandAttackAnimation(int dataId, out int animationIndex, out float triggerDuration, out float totalDuration)
        {
            ActionAnimation2D animation2D = null;
            if (!CacheLeftHandAttackAnimations.TryGetValue(dataId, out animation2D))
                animation2D = defaultAttackAnimation2D;
            animationIndex = 0;
            triggerDuration = 0f;
            totalDuration = 0f;
            if (animation2D == null) return false;
            AnimationClip2D clip = animation2D.GetClipByDirection(currentDirectionType);
            if (clip == null) return false;
            triggerDuration = clip.duration * animation2D.triggerDurationRate;
            totalDuration = clip.duration + animation2D.extraDuration;
            return true;
        }

        public override bool GetSkillActivateAnimation(int dataId, out float triggerDuration, out float totalDuration)
        {
            ActionAnimation2D animation2D = null;
            if (!CacheSkillCastAnimations.TryGetValue(dataId, out animation2D))
                animation2D = defaultSkillCastAnimation2D;
            triggerDuration = 0f;
            totalDuration = 0f;
            if (animation2D == null) return false;
            AnimationClip2D clip = animation2D.GetClipByDirection(currentDirectionType);
            if (clip == null) return false;
            triggerDuration = clip.duration * animation2D.triggerDurationRate;
            totalDuration = clip.duration + animation2D.extraDuration;
            return true;
        }

        public override bool GetReloadAnimation(int dataId, out float triggerDuration, out float totalDuration)
        {
            ActionAnimation2D animation2D = null;
            if (!CacheReloadAnimations.TryGetValue(dataId, out animation2D))
                animation2D = defaultReloadAnimation2D;
            triggerDuration = 0f;
            totalDuration = 0f;
            if (animation2D == null) return false;
            AnimationClip2D clip = animation2D.GetClipByDirection(currentDirectionType);
            if (clip == null) return false;
            triggerDuration = clip.duration * animation2D.triggerDurationRate;
            totalDuration = clip.duration + animation2D.extraDuration;
            return true;
        }

        public override bool HasSkillAnimations(int dataId)
        {
            ActionAnimation2D animation2D = null;
            if (!CacheSkillCastAnimations.TryGetValue(dataId, out animation2D))
                animation2D = defaultSkillCastAnimation2D;
            if (animation2D == null) return false;
            AnimationClip2D clip = animation2D.GetClipByDirection(currentDirectionType);
            if (clip == null) return false;
            return true;
        }
    }
}
