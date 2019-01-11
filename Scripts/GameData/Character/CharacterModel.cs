using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterModel : BaseCharacterModel
    {
        // Animator variables
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
        public RuntimeAnimatorController animatorController;
        public DefaultAnimatorData defaultAnimatorData = new DefaultAnimatorData()
        {
            idleClip = null,
            moveClip = null,
            jumpClip = null,
            fallClip = null,
            hurtClip = null,
            deadClip = null,
            actionClip = null,
        };
        [Header("Legacy Animation")]
        public LegacyAnimationData legacyAnimationData = new LegacyAnimationData()
        {
            idleClip = null,
            moveClip = null,
            jumpClip = null,
            fallClip = null,
            hurtClip = null,
            deadClip = null,
            actionClipFadeLength = 0.1f,
            idleClipFadeLength = 0.1f,
            moveClipFadeLength = 0.1f,
            jumpClipFadeLength = 0.1f,
            fallClipFadeLength = 0.1f,
            hurtClipFadeLength = 0.1f,
            deadClipFadeLength = 0.1f,
            magnitudeToPlayMoveClip = 0.1f,
            ySpeedToPlayJumpClip = 0.25f,
            ySpeedToPlayFallClip = -0.25f,
        };
        [Header("Renderer")]
        public SkinnedMeshRenderer skinnedMeshRenderer;
        [Header("Animations")]
        public ActionAnimation[] defaultAttackAnimations;
        public ActionAnimation[] defaultSkillCastAnimations;
        public WeaponAnimations[] weaponAnimations;
        public SkillCastAnimations[] skillCastAnimations;

        private string defaultIdleClipName;
        private string defaultMoveClipName;
        private string defaultJumpClipName;
        private string defaultFallClipName;
        private string defaultHurtClipName;
        private string defaultDeadClipName;
        private string defaultActionClipName;

        private static Dictionary<int, WeaponAnimations> cacheWeaponAnimations;
        public Dictionary<int, WeaponAnimations> CacheWeaponAnimations
        {
            get
            {
                if (cacheWeaponAnimations == null)
                {
                    cacheWeaponAnimations = new Dictionary<int, WeaponAnimations>();
                    foreach (WeaponAnimations weaponAnimation in weaponAnimations)
                    {
                        if (weaponAnimation.weaponType == null) continue;
                        cacheWeaponAnimations[weaponAnimation.weaponType.DataId] = weaponAnimation;
                    }
                }
                return cacheWeaponAnimations;
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
                    foreach (SkillCastAnimations skillCastAnimation in skillCastAnimations)
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
                {
                    cacheAnimatorController = new AnimatorOverrideController(animatorController);
                    defaultIdleClipName = defaultAnimatorData.idleClip != null ? defaultAnimatorData.idleClip.name : string.Empty;
                    defaultMoveClipName = defaultAnimatorData.moveClip != null ? defaultAnimatorData.moveClip.name : string.Empty;
                    defaultJumpClipName = defaultAnimatorData.jumpClip != null ? defaultAnimatorData.jumpClip.name : string.Empty;
                    defaultFallClipName = defaultAnimatorData.fallClip != null ? defaultAnimatorData.fallClip.name : string.Empty;
                    defaultHurtClipName = defaultAnimatorData.hurtClip != null ? defaultAnimatorData.hurtClip.name : string.Empty;
                    defaultDeadClipName = defaultAnimatorData.deadClip != null ? defaultAnimatorData.deadClip.name : string.Empty;
                    defaultActionClipName = defaultAnimatorData.actionClip != null ? defaultAnimatorData.actionClip.name : string.Empty;
                }
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

        public override void SetEquipWeapons(EquipWeapons equipWeapons)
        {
            base.SetEquipWeapons(equipWeapons);
            if (equipWeapons.rightHand.IsValid() && equipWeapons.rightHand.GetWeaponItem() != null)
            {
                WeaponAnimations weaponAnimations;
                if (CacheWeaponAnimations.TryGetValue(equipWeapons.rightHand.GetWeaponItem().WeaponType.DataId, out weaponAnimations))
                {
                    switch (animatorType)
                    {
                        case AnimatorType.Animator:
                            SetupGenericClips_Animator(
                                weaponAnimations.idleClip,
                                weaponAnimations.moveClip,
                                weaponAnimations.jumpClip,
                                weaponAnimations.fallClip,
                                weaponAnimations.hurtClip,
                                weaponAnimations.deadClip);
                            break;
                        case AnimatorType.LegacyAnimtion:
                            SetupGenericClips_LegacyAnimation(
                                weaponAnimations.idleClip,
                                weaponAnimations.moveClip,
                                weaponAnimations.jumpClip,
                                weaponAnimations.fallClip,
                                weaponAnimations.hurtClip,
                                weaponAnimations.deadClip);
                            break;
                    }
                }
                else
                {
                    switch (animatorType)
                    {
                        case AnimatorType.Animator:
                            SetupGenericClips_Animator(null, null, null, null, null, null);
                            break;
                        case AnimatorType.LegacyAnimtion:
                            SetupGenericClips_LegacyAnimation(null, null, null, null, null, null);
                            break;
                    }
                }
            }
        }

        private void SetupGenericClips_Animator(
            AnimationClip idleClip,
            AnimationClip moveClip,
            AnimationClip jumpClip,
            AnimationClip fallClip,
            AnimationClip hurtClip,
            AnimationClip deadClip)
        {
            if (idleClip == null)
                idleClip = defaultAnimatorData.idleClip;
            if (moveClip == null)
                moveClip = defaultAnimatorData.moveClip;
            if (jumpClip == null)
                jumpClip = defaultAnimatorData.jumpClip;
            if (fallClip == null)
                fallClip = defaultAnimatorData.fallClip;
            if (hurtClip == null)
                hurtClip = defaultAnimatorData.hurtClip;
            if (deadClip == null)
                deadClip = defaultAnimatorData.deadClip;
            // Setup generic clips
            if (!string.IsNullOrEmpty(defaultIdleClipName))
                CacheAnimatorController[defaultIdleClipName] = idleClip;
            if (!string.IsNullOrEmpty(defaultMoveClipName))
                CacheAnimatorController[defaultMoveClipName] = moveClip;
            if (!string.IsNullOrEmpty(defaultJumpClipName))
                CacheAnimatorController[defaultJumpClipName] = jumpClip;
            if (!string.IsNullOrEmpty(defaultFallClipName))
                CacheAnimatorController[defaultFallClipName] = fallClip;
            if (!string.IsNullOrEmpty(defaultHurtClipName))
                CacheAnimatorController[defaultHurtClipName] = hurtClip;
            if (!string.IsNullOrEmpty(defaultDeadClipName))
                CacheAnimatorController[defaultDeadClipName] = deadClip;
        }

        private void SetupGenericClips_LegacyAnimation(
            AnimationClip idleClip,
            AnimationClip moveClip,
            AnimationClip jumpClip,
            AnimationClip fallClip,
            AnimationClip hurtClip,
            AnimationClip deadClip)
        {
            if (idleClip == null)
                idleClip = legacyAnimationData.idleClip;
            if (moveClip == null)
                moveClip = legacyAnimationData.moveClip;
            if (jumpClip == null)
                jumpClip = legacyAnimationData.jumpClip;
            if (fallClip == null)
                fallClip = legacyAnimationData.fallClip;
            if (hurtClip == null)
                hurtClip = legacyAnimationData.hurtClip;
            if (deadClip == null)
                deadClip = legacyAnimationData.deadClip;
            // Remove clips
            cacheAnimation.RemoveClip(legacyAnimationData.idleClip.name);
            cacheAnimation.RemoveClip(legacyAnimationData.moveClip.name);
            cacheAnimation.RemoveClip(legacyAnimationData.jumpClip.name);
            cacheAnimation.RemoveClip(legacyAnimationData.fallClip.name);
            cacheAnimation.RemoveClip(legacyAnimationData.hurtClip.name);
            cacheAnimation.RemoveClip(legacyAnimationData.deadClip.name);
            // Setup generic clips
            cacheAnimation.AddClip(idleClip, legacyAnimationData.idleClip.name);
            cacheAnimation.AddClip(moveClip, legacyAnimationData.moveClip.name);
            cacheAnimation.AddClip(jumpClip, legacyAnimationData.jumpClip.name);
            cacheAnimation.AddClip(fallClip, legacyAnimationData.fallClip.name);
            cacheAnimation.AddClip(hurtClip, legacyAnimationData.hurtClip.name);
            cacheAnimation.AddClip(deadClip, legacyAnimationData.deadClip.name);
        }

        public override void AddingNewModel(GameObject newModel)
        {
            base.AddingNewModel(newModel);
            SkinnedMeshRenderer skinnedMesh = newModel.GetComponentInChildren<SkinnedMeshRenderer>();
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
                float ySpeed = moveVelocity.y;
                if (ySpeed < legacyAnimationData.ySpeedToPlayFallClip)
                    CrossFadeLegacyAnimation(legacyAnimationData.fallClip, legacyAnimationData.fallClipFadeLength);
                else
                {
                    float moveMagnitude = new Vector3(moveVelocity.x, 0, moveVelocity.z).magnitude;
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
            if (tempActionAnimation.clip != null)
            {
                CacheAnimator.SetBool(ANIM_DO_ACTION, false);
                CacheAnimatorController[defaultActionClipName] = tempActionAnimation.clip;
                AudioClip audioClip = tempActionAnimation.GetRandomAudioClip();
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
            if (tempActionAnimation.clip != null)
            {
                if (CacheAnimation.GetClip(LEGACY_CLIP_ACTION) != null)
                    CacheAnimation.RemoveClip(LEGACY_CLIP_ACTION);
                CacheAnimation.AddClip(tempActionAnimation.clip, LEGACY_CLIP_ACTION);
                AudioClip audioClip = tempActionAnimation.GetRandomAudioClip();
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
            tempActionAnimation = default(ActionAnimation);
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
            if (CacheWeaponAnimations.ContainsKey(dataId) &&
                CacheWeaponAnimations[dataId].rightHandAttackAnimations != null)
                return CacheWeaponAnimations[dataId].rightHandAttackAnimations;
            return defaultAttackAnimations;
        }

        public ActionAnimation[] GetLeftHandAttackAnimations(WeaponType weaponType)
        {
            return GetLeftHandAttackAnimations(weaponType.DataId);
        }

        public ActionAnimation[] GetLeftHandAttackAnimations(int dataId)
        {
            if (CacheWeaponAnimations.ContainsKey(dataId) &&
                CacheWeaponAnimations[dataId].leftHandAttackAnimations != null)
                return CacheWeaponAnimations[dataId].leftHandAttackAnimations;
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
    public struct LegacyAnimationData
    {
        public AnimationClip idleClip;
        public AnimationClip moveClip;
        public AnimationClip jumpClip;
        public AnimationClip fallClip;
        public AnimationClip hurtClip;
        public AnimationClip deadClip;
        public float actionClipFadeLength;
        public float idleClipFadeLength;
        public float moveClipFadeLength;
        public float jumpClipFadeLength;
        public float fallClipFadeLength;
        public float hurtClipFadeLength;
        public float deadClipFadeLength;
        public float magnitudeToPlayMoveClip;
        public float ySpeedToPlayJumpClip;
        public float ySpeedToPlayFallClip;
    }

    [System.Serializable]
    public struct DefaultAnimatorData
    {
        public AnimationClip idleClip;
        public AnimationClip moveClip;
        public AnimationClip jumpClip;
        public AnimationClip fallClip;
        public AnimationClip hurtClip;
        public AnimationClip deadClip;
        public AnimationClip actionClip;
    }
}
