using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [ExecuteInEditMode]
    public class CharacterModel2D :
        BaseCharacterModelWithCacheAnims<WeaponAnimations2D, SkillAnimations2D, VehicleAnimations2D>,
        ICharacterModel2D
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
        public VehicleAnimations2D[] vehicleAnimations2D;

        public float magnitudeToPlayMoveClip = 0.1f;
        [Header("Sample 2D Animations")]
        public SampleAnimation sampleAnimation = SampleAnimation.Idle;
        public DirectionType2D sampleDirection = DirectionType2D.Down;
        public bool sampleAlwaysLoop = true;

        public DirectionType2D CurrentDirectionType { get; set; }

        private AnimationClip2D playingAnim = null;
        private int currentFrame = 0;
        private bool playing = false;
        private bool playingAction = false;
        private float secsPerFrame = 0;
        private float nextFrameTime = 0;
        private SampleAnimation? dirtySampleAnimation;
        private DirectionType2D? dirtySampleType;

        protected override void Awake()
        {
            base.Awake();
            Play(idleAnimation2D, DirectionType2D.Down);
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
                    Play(defaultSkillCastClip2D, sampleDirection);
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

        public override void UpdateMovementAnimation(bool isDead, MovementState movementState, float playMoveSpeedMultiplier = 1)
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
            WeaponAnimations2D weaponAnimations2D;
            SkillAnimations2D skillAnimations2D;
            switch (animActionType)
            {
                case AnimActionType.AttackRightHand:
                    if (!GetAnims().CacheWeaponAnimations.TryGetValue(dataId, out weaponAnimations2D))
                        animation2D = defaultAttackAnimation2D;
                    else
                        animation2D = weaponAnimations2D.rightHandAttackAnimation;
                    break;
                case AnimActionType.AttackLeftHand:
                    if (!GetAnims().CacheWeaponAnimations.TryGetValue(dataId, out weaponAnimations2D))
                        animation2D = defaultAttackAnimation2D;
                    else
                        animation2D = weaponAnimations2D.leftHandAttackAnimation;
                    break;
                case AnimActionType.Skill:
                    if (!GetAnims().CacheSkillAnimations.TryGetValue(dataId, out skillAnimations2D))
                        animation2D = defaultSkillActivateAnimation2D;
                    else
                        animation2D = skillAnimations2D.activateAnimation;
                    break;
                case AnimActionType.ReloadRightHand:
                    if (!GetAnims().CacheWeaponAnimations.TryGetValue(dataId, out weaponAnimations2D))
                        animation2D = defaultReloadAnimation2D;
                    else
                        animation2D = weaponAnimations2D.rightHandReloadAnimation;
                    break;
                case AnimActionType.ReloadLeftHand:
                    if (!GetAnims().CacheWeaponAnimations.TryGetValue(dataId, out weaponAnimations2D))
                        animation2D = defaultReloadAnimation2D;
                    else
                        animation2D = weaponAnimations2D.leftHandReloadAnimation;
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
            CharacterAnimation2D animation2D = defaultSkillActivateAnimation2D;
            SkillAnimations2D skillAnims;
            if (GetAnims().CacheSkillAnimations.TryGetValue(dataId, out skillAnims))
                animation2D = skillAnims.castAnimation;

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

        public override bool GetRandomRightHandAttackAnimation(int dataId, out int animationIndex, out float triggerDuration, out float totalDuration)
        {
            ActionAnimation2D animation2D = defaultAttackAnimation2D;
            WeaponAnimations2D weaponAnims;
            if (GetAnims().CacheWeaponAnimations.TryGetValue(dataId, out weaponAnims))
                animation2D = weaponAnims.rightHandReloadAnimation;
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
            ActionAnimation2D animation2D = defaultAttackAnimation2D;
            WeaponAnimations2D weaponAnims;
            if (GetAnims().CacheWeaponAnimations.TryGetValue(dataId, out weaponAnims))
                animation2D = weaponAnims.leftHandAttackAnimation;
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
            ActionAnimation2D animation2D = defaultSkillActivateAnimation2D;
            SkillAnimations2D skillAnims;
            if (GetAnims().CacheSkillAnimations.TryGetValue(dataId, out skillAnims))
                animation2D = skillAnims.activateAnimation;
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
            ActionAnimation2D animation2D = defaultReloadAnimation2D;
            WeaponAnimations2D weaponAnims;
            if (GetAnims().CacheWeaponAnimations.TryGetValue(dataId, out weaponAnims))
                animation2D = weaponAnims.rightHandReloadAnimation;
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
            ActionAnimation2D animation2D = defaultReloadAnimation2D;
            WeaponAnimations2D weaponAnims;
            if (GetAnims().CacheWeaponAnimations.TryGetValue(dataId, out weaponAnims))
                animation2D = weaponAnims.leftHandReloadAnimation;
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
            if (!GetAnims().CacheSkillAnimations.ContainsKey(dataId))
                return SkillActivateAnimationType.UseActivateAnimation;
            return GetAnims().CacheSkillAnimations[dataId].activateAnimationType;
        }

        protected override WeaponAnimations2D[] GetWeaponAnims()
        {
            return weaponAnimations2D;
        }

        protected override SkillAnimations2D[] GetSkillAnims()
        {
            return skillAnimations2D;
        }

        protected override VehicleAnimations2D[] GetVehicleAnims()
        {
            return vehicleAnimations2D;
        }
    }
}
