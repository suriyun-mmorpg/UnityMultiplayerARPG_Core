using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [ExecuteInEditMode]
    public class CharacterModel2D : BaseCharacterModel, ICharacterModel2D
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
        public CharacterAnimation2D defaultSkillCastClip2D;
        public ActionAnimation2D defaultSkillActivateAnimation2D;
        public ActionAnimation2D defaultReloadAnimation2D;
        public WeaponAnimations2D[] weaponAnimations2D;
        public SkillAnimations2D[] skillAnimations2D;

        // Deprecated
        public ActionAnimation2D defaultSkillCastAnimation2D;
        public SkillCastAnimations2D[] skillCastAnimations2D;

        public float magnitudeToPlayMoveClip = 0.1f;
        [Header("Sample 2D Animations")]
        public SampleAnimation sampleAnimation = SampleAnimation.Idle;
        public DirectionType2D sampleDirection = DirectionType2D.Down;
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

        private Dictionary<int, ActionAnimation2D> cacheRightHandReloadAnimations;
        public Dictionary<int, ActionAnimation2D> CacheRightHandReloadAnimations
        {
            get
            {
                if (cacheRightHandReloadAnimations == null)
                {
                    cacheRightHandReloadAnimations = new Dictionary<int, ActionAnimation2D>();
                    foreach (WeaponAnimations2D attackAnimation in weaponAnimations2D)
                    {
                        if (attackAnimation.weaponType == null) continue;
                        cacheRightHandReloadAnimations[attackAnimation.weaponType.DataId] = attackAnimation.rightHandReloadAnimation;
                    }
                }
                return cacheRightHandReloadAnimations;
            }
        }

        private Dictionary<int, ActionAnimation2D> cacheLeftHandReloadAnimations;
        public Dictionary<int, ActionAnimation2D> CacheLeftHandReloadAnimations
        {
            get
            {
                if (cacheLeftHandReloadAnimations == null)
                {
                    cacheLeftHandReloadAnimations = new Dictionary<int, ActionAnimation2D>();
                    foreach (WeaponAnimations2D attackAnimation in weaponAnimations2D)
                    {
                        if (attackAnimation.weaponType == null) continue;
                        cacheLeftHandReloadAnimations[attackAnimation.weaponType.DataId] = attackAnimation.leftHandReloadAnimation;
                    }
                }
                return cacheLeftHandReloadAnimations;
            }
        }

        private Dictionary<int, SkillAnimations2D> cacheSkillAnimations;
        public Dictionary<int, SkillAnimations2D> CacheSkillAnimations
        {
            get
            {
                if (cacheSkillAnimations == null)
                {
                    cacheSkillAnimations = new Dictionary<int, SkillAnimations2D>();
                    foreach (SkillAnimations2D skillAnimation in skillAnimations2D)
                    {
                        if (skillAnimation.skill == null) continue;
                        cacheSkillAnimations[skillAnimation.skill.DataId] = skillAnimation;
                    }
                }
                return cacheSkillAnimations;
            }
        }

        public DirectionType2D CurrentDirectionType { get; set; }

        private AnimationClip2D playingAnim = null;
        private int currentFrame = 0;
        private bool playing = false;
        private bool playingAction = false;
        private float secsPerFrame = 0;
        private float nextFrameTime = 0;
        private SampleAnimation? dirtySampleAnimation;
        private DirectionType2D? dirtySampleType;

        private void Awake()
        {
            MigrateSkillCastAnimations();
        }

        private void Start()
        {
            Play(idleAnimation2D, DirectionType2D.Down);
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
            if (defaultSkillCastAnimation2D != null)
            {
                hasChanges = true;
                defaultSkillActivateAnimation2D = defaultSkillCastAnimation2D;
                defaultSkillCastAnimation2D = null;
            }

            if (skillCastAnimations2D != null &&
                skillCastAnimations2D.Length > 0)
            {
                hasChanges = true;
                skillAnimations2D = new SkillAnimations2D[skillCastAnimations2D.Length];
                for (int i = 0; i < skillCastAnimations2D.Length; ++i)
                {
                    SkillAnimations2D data = new SkillAnimations2D();
                    data.skill = skillCastAnimations2D[i].skill;
                    if (skillCastAnimations2D[i].animation != null)
                        data.activateAnimation = skillCastAnimations2D[i].animation;
                    skillAnimations2D[i] = data;
                }
                //skillCastAnimations2D = null;
            }
            return hasChanges;
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

        public void Play(CharacterAnimation2D animation, DirectionType2D directionType)
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

        public override void UpdateAnimation(bool isDead, MovementState movementState, float playMoveSpeedMultiplier = 1)
        {
            if (playingAction)
                return;

            if (isDead)
            {
                Play(deadAnimation2D, CurrentDirectionType);
            }
            else
            {
                if (movementState.HasFlag(MovementState.Forward) ||
                    movementState.HasFlag(MovementState.Backward) ||
                    movementState.HasFlag(MovementState.Right) ||
                    movementState.HasFlag(MovementState.Left))
                    Play(moveAnimation2D, CurrentDirectionType);
                else
                    Play(idleAnimation2D, CurrentDirectionType);
            }
        }

        private ActionAnimation2D GetActionAnimation(AnimActionType animActionType, int dataId)
        {
            ActionAnimation2D animation2D = null;
            SkillAnimations2D skillAnimations2D;
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
                    if (!CacheSkillAnimations.TryGetValue(dataId, out skillAnimations2D))
                        animation2D = defaultSkillActivateAnimation2D;
                    else
                        animation2D = skillAnimations2D.activateAnimation;
                    break;
                case AnimActionType.ReloadRightHand:
                    if (!CacheRightHandReloadAnimations.TryGetValue(dataId, out animation2D))
                        animation2D = defaultReloadAnimation2D;
                    break;
                case AnimActionType.ReloadLeftHand:
                    if (!CacheLeftHandReloadAnimations.TryGetValue(dataId, out animation2D))
                        animation2D = defaultReloadAnimation2D;
                    break;
            }
            return animation2D;
        }

        public override Coroutine PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1)
        {
            return StartCoroutine(PlayActionAnimationRoutine(animActionType, dataId, index, playSpeedMultiplier));
        }

        IEnumerator PlayActionAnimationRoutine(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier)
        {
            // If animator is not null, play the action animation
            ActionAnimation2D animation2D = GetActionAnimation(animActionType, dataId);
            if (animation2D != null)
            {
                AnimationClip2D anim = animation2D.GetClipByDirection(CurrentDirectionType);
                if (anim != null)
                {
                    playingAction = true;
                    AudioClip audioClip = animation2D.GetRandomAudioClip();
                    if (audioClip != null)
                        AudioSource.PlayClipAtPoint(audioClip, CacheTransform.position, AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level);
                    // Waits by current transition + clip duration before end animation
                    Play(anim);
                    yield return new WaitForSecondsRealtime(anim.duration / playSpeedMultiplier);
                    Play(idleAnimation2D, CurrentDirectionType);
                    yield return new WaitForSecondsRealtime(animation2D.extraDuration / playSpeedMultiplier);
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
            CharacterAnimation2D animation2D;
            SkillAnimations2D skillAnimations2D;
            if (!CacheSkillAnimations.TryGetValue(dataId, out skillAnimations2D))
                animation2D = defaultSkillActivateAnimation2D;
            else
                animation2D = skillAnimations2D.castAnimation;

            if (animation2D != null)
            {
                AnimationClip2D anim = animation2D.GetClipByDirection(CurrentDirectionType);
                if (anim != null)
                {
                    playingAction = true;
                    Play(anim);
                    yield return new WaitForSecondsRealtime(duration);
                    Play(idleAnimation2D, CurrentDirectionType);
                    playingAction = false;
                }
            }
        }

        public override void StopActionAnimation()
        {
            playingAction = false;
        }

        public override void StopSkillCastAnimation()
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
            AnimationClip2D clip = animation2D.GetClipByDirection(CurrentDirectionType);
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
            AnimationClip2D clip = animation2D.GetClipByDirection(CurrentDirectionType);
            if (clip == null) return false;
            triggerDuration = clip.duration * animation2D.triggerDurationRate;
            totalDuration = clip.duration + animation2D.extraDuration;
            return true;
        }

        public override bool GetSkillActivateAnimation(int dataId, out float triggerDuration, out float totalDuration)
        {
            ActionAnimation2D animation2D;
            SkillAnimations2D skillAnimations2D;
            if (!CacheSkillAnimations.TryGetValue(dataId, out skillAnimations2D))
                animation2D = defaultSkillActivateAnimation2D;
            else
                animation2D = skillAnimations2D.activateAnimation;
            triggerDuration = 0f;
            totalDuration = 0f;
            if (animation2D == null) return false;
            AnimationClip2D clip = animation2D.GetClipByDirection(CurrentDirectionType);
            if (clip == null) return false;
            triggerDuration = clip.duration * animation2D.triggerDurationRate;
            totalDuration = clip.duration + animation2D.extraDuration;
            return true;
        }

        public override bool GetRightHandReloadAnimation(int dataId, out float triggerDuration, out float totalDuration)
        {
            ActionAnimation2D animation2D = null;
            if (!CacheRightHandReloadAnimations.TryGetValue(dataId, out animation2D))
                animation2D = defaultReloadAnimation2D;
            triggerDuration = 0f;
            totalDuration = 0f;
            if (animation2D == null) return false;
            AnimationClip2D clip = animation2D.GetClipByDirection(CurrentDirectionType);
            if (clip == null) return false;
            triggerDuration = clip.duration * animation2D.triggerDurationRate;
            totalDuration = clip.duration + animation2D.extraDuration;
            return true;
        }

        public override bool GetLeftHandReloadAnimation(int dataId, out float triggerDuration, out float totalDuration)
        {
            ActionAnimation2D animation2D = null;
            if (!CacheLeftHandReloadAnimations.TryGetValue(dataId, out animation2D))
                animation2D = defaultReloadAnimation2D;
            triggerDuration = 0f;
            totalDuration = 0f;
            if (animation2D == null) return false;
            AnimationClip2D clip = animation2D.GetClipByDirection(CurrentDirectionType);
            if (clip == null) return false;
            triggerDuration = clip.duration * animation2D.triggerDurationRate;
            totalDuration = clip.duration + animation2D.extraDuration;
            return true;
        }

        public override SkillActivateAnimationType UseSkillActivateAnimationType(int dataId)
        {
            if (!CacheSkillAnimations.ContainsKey(dataId))
                return SkillActivateAnimationType.UseActivateAnimation;
            return CacheSkillAnimations[dataId].activateAnimationType;
        }
    }
}
