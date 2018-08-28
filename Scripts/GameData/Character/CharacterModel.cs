using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterModel : BaseCharacterModel
    {
        // Animator variables
        public const string ANIM_STATE_ACTION_CLIP = "_Action";
        public static readonly int ANIM_IS_DEAD = Animator.StringToHash("IsDead");
        public static readonly int ANIM_MOVE_SPEED = Animator.StringToHash("MoveSpeed");
        public static readonly int ANIM_Y_SPEED = Animator.StringToHash("YSpeed");
        public static readonly int ANIM_DO_ACTION = Animator.StringToHash("DoAction");
        public static readonly int ANIM_HURT = Animator.StringToHash("Hurt");
        public static readonly int ANIM_JUMP = Animator.StringToHash("Jump");
        public static readonly int ANIM_MOVE_CLIP_MULTIPLIER = Animator.StringToHash("MoveSpeedMultiplier");
        public static readonly int ANIM_ACTION_CLIP_MULTIPLIER = Animator.StringToHash("ActionSpeedMultiplier");
        // Legacy Animation variables
        public const string LEGACY_CLIP_ACTION = "_Action";

        public enum AnimatorType
        {
            Animator,
            LegacyAnimtion,
        }
        [Header("Animation Component Type")]
        public AnimatorType animatorType;
        [Header("Animator")]
        [SerializeField]
        private RuntimeAnimatorController animatorController;
        [Header("Legacy Animation")]
        [SerializeField]
        private LegacyAnimationData legacyAnimationData;
        [Header("Renderer")]
        [SerializeField]
        private SkinnedMeshRenderer skinnedMeshRenderer;
        [Header("Animations")]
        [SerializeField]
        private ActionAnimation[] defaultAttackAnimations;
        [SerializeField]
        private ActionAnimation[] defaultSkillCastAnimations;
        [SerializeField]
        private WeaponAnimations[] weaponAnimations;
        [SerializeField]
        private SkillCastAnimations[] skillCastAnimations;

        private static Dictionary<int, ActionAnimation[]> cacheRightHandAttackAnimations;
        public Dictionary<int, ActionAnimation[]> CacheRightHandAttackAnimations
        {
            get
            {
                if (cacheRightHandAttackAnimations == null)
                {
                    cacheRightHandAttackAnimations = new Dictionary<int, ActionAnimation[]>();
                    foreach (var attackAnimation in weaponAnimations)
                    {
                        if (attackAnimation.weaponType == null) continue;
                        cacheRightHandAttackAnimations[attackAnimation.weaponType.DataId] = attackAnimation.rightHandAttackAnimations;
                    }
                }
                return cacheRightHandAttackAnimations;
            }
        }

        private static Dictionary<int, ActionAnimation[]> cacheLeftHandAttackAnimations;
        public Dictionary<int, ActionAnimation[]> CacheLeftHandAttackAnimations
        {
            get
            {
                if (cacheLeftHandAttackAnimations == null)
                {
                    cacheLeftHandAttackAnimations = new Dictionary<int, ActionAnimation[]>();
                    foreach (var attackAnimation in weaponAnimations)
                    {
                        if (attackAnimation.weaponType == null) continue;
                        cacheLeftHandAttackAnimations[attackAnimation.weaponType.DataId] = attackAnimation.rightHandAttackAnimations;
                    }
                }
                return cacheLeftHandAttackAnimations;
            }
        }

        private static Dictionary<int, ActionAnimation[]> cacheSkillCastAnimations;
        public Dictionary<int, ActionAnimation[]> CacheSkillCastAnimations
        {
            get
            {
                if (cacheSkillCastAnimations == null)
                {
                    cacheSkillCastAnimations = new Dictionary<int, ActionAnimation[]>();
                    foreach (var skillCastAnimation in skillCastAnimations)
                    {
                        if (skillCastAnimation.skill == null) continue;
                        cacheSkillCastAnimations[skillCastAnimation.skill.DataId] = skillCastAnimation.castAnimations;
                    }
                }
                return cacheSkillCastAnimations;
            }
        }

        // Optimize garbage collection
        protected ActionAnimation tempActionAnimation;
        protected ActionAnimation[] tempActionAnimations;

        private Animator cacheAnimator;
        public Animator CacheAnimator
        {
            get
            {
                if (cacheAnimator == null)
                {
                    cacheAnimator = GetComponent<Animator>();
                    cacheAnimator.runtimeAnimatorController = CacheAnimatorController;
                }
                return cacheAnimator;
            }
        }

        private AnimatorOverrideController cacheAnimatorController;
        public AnimatorOverrideController CacheAnimatorController
        {
            get
            {
                if (cacheAnimatorController == null)
                    cacheAnimatorController = new AnimatorOverrideController(animatorController);
                return cacheAnimatorController;
            }
        }

        private Animation cacheAnimation;
        public Animation CacheAnimation
        {
            get
            {
                if (cacheAnimation == null)
                {
                    cacheAnimation = GetComponent<Animation>();
                    cacheAnimation.AddClip(legacyAnimationData.idleClip, legacyAnimationData.idleClip.name);
                    cacheAnimation.AddClip(legacyAnimationData.moveClip, legacyAnimationData.moveClip.name);
                    cacheAnimation.AddClip(legacyAnimationData.jumpClip, legacyAnimationData.jumpClip.name);
                    cacheAnimation.AddClip(legacyAnimationData.fallClip, legacyAnimationData.fallClip.name);
                    cacheAnimation.AddClip(legacyAnimationData.hurtClip, legacyAnimationData.hurtClip.name);
                    cacheAnimation.AddClip(legacyAnimationData.deadClip, legacyAnimationData.deadClip.name);
                }
                return cacheAnimation;
            }
        }

        public override void AddingNewModel(GameObject newModel)
        {
            base.AddingNewModel(newModel);
            var skinnedMesh = newModel.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMesh != null && skinnedMeshRenderer != null)
            {
                skinnedMesh.bones = skinnedMeshRenderer.bones;
                skinnedMesh.rootBone = skinnedMeshRenderer.rootBone;
            }
        }

        public override void UpdateAnimation(bool isDead, Vector3 moveVelocity, float playMoveSpeedMultiplier = 1f)
        {
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

        #region Update Animation Functions
        private void UpdateAnimation_Animator(bool isDead, Vector3 moveVelocity, float playMoveSpeedMultiplier)
        {
            if (!CacheAnimator.gameObject.activeInHierarchy)
                return;
            if (isDead && CacheAnimator.GetBool(ANIM_DO_ACTION))
            {
                // Force set to none action when dead
                CacheAnimator.SetBool(ANIM_DO_ACTION, false);
            }
            CacheAnimator.SetFloat(ANIM_MOVE_SPEED, isDead ? 0 : new Vector3(moveVelocity.x, 0, moveVelocity.z).magnitude);
            CacheAnimator.SetFloat(ANIM_MOVE_CLIP_MULTIPLIER, playMoveSpeedMultiplier);
            CacheAnimator.SetFloat(ANIM_Y_SPEED, moveVelocity.y);
            CacheAnimator.SetBool(ANIM_IS_DEAD, isDead);
        }

        private void UpdateAnimation_LegacyAnimation(bool isDead, Vector3 moveVelocity, float playMoveSpeedMultiplier)
        {
            if (isDead)
                CrossFadeLegacyAnimation(legacyAnimationData.deadClip, legacyAnimationData.deadClipFadeLength);
            else
            {
                if (CacheAnimation.IsPlaying(LEGACY_CLIP_ACTION))
                    return;
                var ySpeed = moveVelocity.y;
                if (ySpeed < legacyAnimationData.ySpeedToPlayFallClip)
                    CrossFadeLegacyAnimation(legacyAnimationData.fallClip, legacyAnimationData.fallClipFadeLength);
                else
                {
                    var moveMagnitude = new Vector3(moveVelocity.x, 0, moveVelocity.z).magnitude;
                    if (moveMagnitude > legacyAnimationData.magnitudeToPlayMoveClip)
                        CrossFadeLegacyAnimation(legacyAnimationData.moveClip, legacyAnimationData.moveClipFadeLength);
                    else
                        CrossFadeLegacyAnimation(legacyAnimationData.idleClip, legacyAnimationData.idleClipFadeLength);
                }
            }
        }

        private void CrossFadeLegacyAnimation(AnimationClip clip, float fadeLength)
        {
            CrossFadeLegacyAnimation(clip.name, fadeLength);
        }

        private void CrossFadeLegacyAnimation(string clipName, float fadeLength)
        {
            if (!CacheAnimation.IsPlaying(clipName))
                CacheAnimation.CrossFade(clipName, fadeLength);
        }
        #endregion

        public override Coroutine PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1f)
        {
            if (animatorType == AnimatorType.LegacyAnimtion)
                return StartCoroutine(PlayActionAnimation_LegacyAnimation(animActionType, dataId, index, playSpeedMultiplier));
            return StartCoroutine(PlayActionAnimation_Animator(animActionType, dataId, index, playSpeedMultiplier));
        }

        #region Action Animation Functions
        private IEnumerator PlayActionAnimation_Animator(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier)
        {
            // If animator is not null, play the action animation
            tempActionAnimation = GetActionAnimation(animActionType, dataId, index);
            if (tempActionAnimation != null && tempActionAnimation.clip != null)
            {
                CacheAnimator.SetBool(ANIM_DO_ACTION, false);
                CacheAnimatorController[ANIM_STATE_ACTION_CLIP] = tempActionAnimation.clip;
                var audioClip = tempActionAnimation.GetRandomAudioClip();
                if (audioClip != null)
                    AudioSource.PlayClipAtPoint(audioClip, CacheTransform.position, AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level);
                CacheAnimator.SetFloat(ANIM_ACTION_CLIP_MULTIPLIER, playSpeedMultiplier);
                CacheAnimator.SetBool(ANIM_DO_ACTION, true);
                // Waits by current transition + clip duration before end animation
                yield return new WaitForSecondsRealtime(CacheAnimator.GetAnimatorTransitionInfo(0).duration + (tempActionAnimation.GetClipLength() / playSpeedMultiplier));
                CacheAnimator.SetBool(ANIM_DO_ACTION, false);
                // Waits by current transition + extra duration before end playing animation state
                yield return new WaitForSecondsRealtime(CacheAnimator.GetAnimatorTransitionInfo(0).duration + (tempActionAnimation.GetExtraDuration() / playSpeedMultiplier));
            }
        }

        private IEnumerator PlayActionAnimation_LegacyAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier)
        {
            // If animator is not null, play the action animation
            tempActionAnimation = GetActionAnimation(animActionType, dataId, index);
            if (tempActionAnimation != null && tempActionAnimation.clip != null)
            {
                if (CacheAnimation.GetClip(LEGACY_CLIP_ACTION) != null)
                    CacheAnimation.RemoveClip(LEGACY_CLIP_ACTION);
                CacheAnimation.AddClip(tempActionAnimation.clip, LEGACY_CLIP_ACTION);
                var audioClip = tempActionAnimation.GetRandomAudioClip();
                if (audioClip != null)
                    AudioSource.PlayClipAtPoint(audioClip, CacheTransform.position, AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level);
                CrossFadeLegacyAnimation(LEGACY_CLIP_ACTION, legacyAnimationData.actionClipFadeLength);
                // Waits by current transition + clip duration before end animation
                yield return new WaitForSecondsRealtime(tempActionAnimation.GetClipLength() / playSpeedMultiplier);
                CrossFadeLegacyAnimation(legacyAnimationData.idleClip, legacyAnimationData.idleClipFadeLength);
                // Waits by current transition + extra duration before end playing animation state
                yield return new WaitForSecondsRealtime(tempActionAnimation.GetExtraDuration() / playSpeedMultiplier);
            }
        }
        #endregion

        public override void PlayHurtAnimation()
        {
            if (animatorType == AnimatorType.LegacyAnimtion)
            {
                CrossFadeLegacyAnimation(legacyAnimationData.hurtClip, legacyAnimationData.hurtClipFadeLength);
                return;
            }
            CacheAnimator.ResetTrigger(ANIM_HURT);
            CacheAnimator.SetTrigger(ANIM_HURT);
        }

        public override void PlayJumpAnimation()
        {
            if (animatorType == AnimatorType.LegacyAnimtion)
            {
                CrossFadeLegacyAnimation(legacyAnimationData.jumpClip, legacyAnimationData.jumpClipFadeLength);
                return;
            }
            CacheAnimator.ResetTrigger(ANIM_JUMP);
            CacheAnimator.SetTrigger(ANIM_JUMP);
        }

        #region Animation data helpers
        public ActionAnimation GetActionAnimation(AnimActionType animActionType, int dataId, int index)
        {
            tempActionAnimation = null;
            switch (animActionType)
            {
                case AnimActionType.AttackRightHand:
                    tempActionAnimation = GetRightHandAttackAnimations(dataId)[index];
                    break;
                case AnimActionType.AttackLeftHand:
                    tempActionAnimation = GetLeftHandAttackAnimations(dataId)[index];
                    break;
                case AnimActionType.Skill:
                    tempActionAnimation = GetSkillCastAnimations(dataId)[index];
                    break;
            }
            return tempActionAnimation;
        }

        public ActionAnimation[] GetRightHandAttackAnimations(WeaponType weaponType)
        {
            return GetRightHandAttackAnimations(weaponType.DataId);
        }

        public ActionAnimation[] GetRightHandAttackAnimations(int dataId)
        {
            if (CacheRightHandAttackAnimations.ContainsKey(dataId))
                return CacheRightHandAttackAnimations[dataId];
            return defaultAttackAnimations;
        }

        public ActionAnimation[] GetLeftHandAttackAnimations(WeaponType weaponType)
        {
            return GetLeftHandAttackAnimations(weaponType.DataId);
        }

        public ActionAnimation[] GetLeftHandAttackAnimations(int dataId)
        {
            if (CacheLeftHandAttackAnimations.ContainsKey(dataId))
                return CacheLeftHandAttackAnimations[dataId];
            return defaultAttackAnimations;
        }

        public ActionAnimation[] GetSkillCastAnimations(Skill skill)
        {
            return GetSkillCastAnimations(skill.DataId);
        }

        public ActionAnimation[] GetSkillCastAnimations(int dataId)
        {
            if (CacheSkillCastAnimations.ContainsKey(dataId))
                return CacheSkillCastAnimations[dataId];
            return defaultSkillCastAnimations;
        }

        public override bool GetRandomRightHandAttackAnimation(
            int dataId,
            out int animationIndex,
            out float triggerDuration,
            out float totalDuration)
        {
            tempActionAnimations = GetRightHandAttackAnimations(dataId);
            animationIndex = 0;
            triggerDuration = 0f;
            totalDuration = 0f;
            if (tempActionAnimations.Length == 0) return false;
            animationIndex = Random.Range(0, tempActionAnimations.Length);
            triggerDuration = tempActionAnimations[animationIndex].GetTriggerDuration();
            totalDuration = tempActionAnimations[animationIndex].GetTotalDuration();
            return true;
        }

        public override bool GetRandomLeftHandAttackAnimation(
            int dataId,
            out int animationIndex,
            out float triggerDuration,
            out float totalDuration)
        {
            tempActionAnimations = GetLeftHandAttackAnimations(dataId);
            animationIndex = 0;
            triggerDuration = 0f;
            totalDuration = 0f;
            if (tempActionAnimations.Length == 0) return false;
            animationIndex = Random.Range(0, tempActionAnimations.Length);
            triggerDuration = tempActionAnimations[animationIndex].GetTriggerDuration();
            totalDuration = tempActionAnimations[animationIndex].GetTotalDuration();
            return true;
        }

        public override bool GetRandomSkillCastAnimation(
            int dataId,
            out int animationIndex,
            out float triggerDuration,
            out float totalDuration)
        {
            tempActionAnimations = GetSkillCastAnimations(dataId);
            animationIndex = 0;
            triggerDuration = 0f;
            totalDuration = 0f;
            if (tempActionAnimations.Length == 0) return false;
            animationIndex = Random.Range(0, tempActionAnimations.Length);
            triggerDuration = tempActionAnimations[animationIndex].GetTriggerDuration();
            totalDuration = tempActionAnimations[animationIndex].GetTotalDuration();
            return true;
        }

        public override bool HasSkillCastAnimations(int dataId)
        {
            tempActionAnimations = GetSkillCastAnimations(dataId);
            return tempActionAnimations != null && tempActionAnimations.Length > 0;
        }
        #endregion
    }

    [System.Serializable]
    public class LegacyAnimationData
    {
        public AnimationClip idleClip;
        public AnimationClip moveClip;
        public AnimationClip jumpClip;
        public AnimationClip fallClip;
        public AnimationClip hurtClip;
        public AnimationClip deadClip;
        public float actionClipFadeLength = 0.1f;
        public float idleClipFadeLength = 0.1f;
        public float moveClipFadeLength = 0.1f;
        public float jumpClipFadeLength = 0.1f;
        public float fallClipFadeLength = 0.1f;
        public float hurtClipFadeLength = 0.1f;
        public float deadClipFadeLength = 0.1f;
        public float magnitudeToPlayMoveClip = 0.1f;
        public float ySpeedToPlayJumpClip = 0.25f;
        public float ySpeedToPlayFallClip = -0.25f;
    }
}
