using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
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

    public class CharacterModel : MonoBehaviour
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

        [SerializeField]
        private int dataId;
        public int DataId { get { return dataId; } }

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
        [Header("Equipment Containers")]
        [SerializeField]
        private EquipmentModelContainer[] equipmentContainers;
        [Header("Effect Containers")]
        [SerializeField]
        private EffectContainer[] effectContainers;
        
        protected GameInstance gameInstance { get { return GameInstance.Singleton; } }

        private Transform cacheTransform;
        public Transform CacheTransform
        {
            get
            {
                if (cacheTransform == null)
                    cacheTransform = GetComponent<Transform>();
                return cacheTransform;
            }
        }

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

        private Dictionary<string, EquipmentModelContainer> cacheEquipmentModelContainers = null;
        /// <summary>
        /// Dictionary[equipSocket(String), container(EquipmentModelContainer)]
        /// </summary>
        public Dictionary<string, EquipmentModelContainer> CacheEquipmentModelContainers
        {
            get
            {
                if (cacheEquipmentModelContainers == null)
                {
                    cacheEquipmentModelContainers = new Dictionary<string, EquipmentModelContainer>();
                    foreach (var equipmentContainer in equipmentContainers)
                    {
                        if (equipmentContainer.transform != null && !cacheEquipmentModelContainers.ContainsKey(equipmentContainer.equipSocket))
                            cacheEquipmentModelContainers[equipmentContainer.equipSocket] = equipmentContainer;
                    }
                }
                return cacheEquipmentModelContainers;
            }
        }

        private Dictionary<string, EffectContainer> cacheEffectContainers = null;
        /// <summary>
        /// Dictionary[effectSocket(String), container(CharacterModelContainer)]
        /// </summary>
        public Dictionary<string, EffectContainer> CacheEffectContainers
        {
            get
            {
                if (cacheEffectContainers == null)
                {
                    cacheEffectContainers = new Dictionary<string, EffectContainer>();
                    foreach (var effectContainer in effectContainers)
                    {
                        if (effectContainer.transform != null && !cacheEffectContainers.ContainsKey(effectContainer.effectSocket))
                            cacheEffectContainers[effectContainer.effectSocket] = effectContainer;
                    }
                }
                return cacheEffectContainers;
            }
        }

        /// <summary>
        /// Dictionary[equipPosition(String), Dictionary[equipSocket(String), model(GameObject)]]
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, GameObject>> cacheModels = new Dictionary<string, Dictionary<string, GameObject>>();

        /// <summary>
        /// Dictionary[equipPosition(String), List[effect(GameEffect)]]
        /// </summary>
        private readonly Dictionary<string, List<GameEffect>> cacheEffects = new Dictionary<string, List<GameEffect>>();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying && dataId != name.GenerateHashId())
            {
                dataId = name.GenerateHashId();
                EditorUtility.SetDirty(gameObject);
            }
        }
#endif

        private void CreateCacheModel(string equipPosition, Dictionary<string, GameObject> models)
        {
            DestroyCacheModel(equipPosition);
            if (models == null)
                return;
            foreach (var model in models)
            {
                EquipmentModelContainer container;
                if (!CacheEquipmentModelContainers.TryGetValue(model.Key, out container))
                    continue;
                if (container.defaultModel != null)
                    container.defaultModel.SetActive(false);
            }
            cacheModels[equipPosition] = models;
        }

        private void DestroyCacheModel(string equipPosition)
        {
            Dictionary<string, GameObject> oldModels;
            if (!string.IsNullOrEmpty(equipPosition) && cacheModels.TryGetValue(equipPosition, out oldModels) && oldModels != null)
            {
                foreach (var model in oldModels)
                {
                    Destroy(model.Value);
                    EquipmentModelContainer container;
                    if (!CacheEquipmentModelContainers.TryGetValue(model.Key, out container))
                        continue;
                    if (container.defaultModel != null)
                        container.defaultModel.SetActive(true);
                }
                cacheModels.Remove(equipPosition);
            }
        }

        public void SetEquipWeapons(EquipWeapons equipWeapons)
        {
            var rightHandWeapon = equipWeapons.rightHand.GetWeaponItem();
            var leftHandWeapon = equipWeapons.leftHand.GetWeaponItem();
            var leftHandShield = equipWeapons.leftHand.GetShieldItem();

            // Clear equipped item models
            var keepingKeys = new List<string>();
            if (rightHandWeapon != null)
                keepingKeys.Add(GameDataConst.EQUIP_POSITION_RIGHT_HAND);
            if (leftHandWeapon != null || leftHandShield != null)
                keepingKeys.Add(GameDataConst.EQUIP_POSITION_LEFT_HAND);

            var keys = new List<string>(cacheModels.Keys);
            foreach (var key in keys)
            {
                if (!keepingKeys.Contains(key) &&
                    (key.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND) ||
                    key.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND)))
                    DestroyCacheModel(key);
            }

            if (rightHandWeapon != null)
                InstantiateEquipModel(GameDataConst.EQUIP_POSITION_RIGHT_HAND, rightHandWeapon.equipmentModels);
            if (leftHandWeapon != null)
                InstantiateEquipModel(GameDataConst.EQUIP_POSITION_LEFT_HAND, leftHandWeapon.subEquipmentModels);
            if (leftHandShield != null)
                InstantiateEquipModel(GameDataConst.EQUIP_POSITION_LEFT_HAND, leftHandShield.equipmentModels);
        }

        public void SetEquipItems(IList<CharacterItem> equipItems)
        {
            // Clear equipped item models
            var keepingKeys = new List<string>();
            foreach (var equipItem in equipItems)
            {
                var armorItem = equipItem.GetArmorItem();
                if (armorItem != null)
                    keepingKeys.Add(armorItem.EquipPosition);
            }

            var keys = new List<string>(cacheModels.Keys);
            foreach (var key in keys)
            {
                if (!keepingKeys.Contains(key) &&
                    !key.Equals(GameDataConst.EQUIP_POSITION_RIGHT_HAND) &&
                    !key.Equals(GameDataConst.EQUIP_POSITION_LEFT_HAND))
                    DestroyCacheModel(key);
            }

            foreach (var equipItem in equipItems)
            {
                var armorItem = equipItem.GetArmorItem();
                if (armorItem == null)
                    continue;
                var equipPosition = armorItem.EquipPosition;
                if (keepingKeys.Contains(equipPosition))
                    InstantiateEquipModel(equipPosition, armorItem.equipmentModels);
            }
        }

        public void InstantiateEquipModel(string equipPosition, EquipmentModel[] equipmentModels)
        {
            if (equipmentModels == null || equipmentModels.Length == 0)
                return;
            var models = new Dictionary<string, GameObject>();
            foreach (var equipmentModel in equipmentModels)
            {
                var equipSocket = equipmentModel.equipSocket;
                var model = equipmentModel.model;
                if (string.IsNullOrEmpty(equipSocket) || model == null)
                    continue;
                EquipmentModelContainer container;
                if (!CacheEquipmentModelContainers.TryGetValue(equipSocket, out container))
                    continue;
                var newModel = Instantiate(model, container.transform);
                newModel.transform.localPosition = Vector3.zero;
                newModel.transform.localEulerAngles = Vector3.zero;
                newModel.transform.localScale = Vector3.one;
                newModel.gameObject.SetActive(true);
                newModel.gameObject.layer = gameInstance.characterLayer;
                newModel.RemoveComponentsInChildren<Collider>(false);
                var skinnedMesh = newModel.GetComponentInChildren<SkinnedMeshRenderer>();
                if (skinnedMesh != null && skinnedMeshRenderer != null)
                {
                    skinnedMesh.bones = skinnedMeshRenderer.bones;
                    skinnedMesh.rootBone = skinnedMeshRenderer.rootBone;
                }
                models.Add(equipSocket, newModel);
            }
            CreateCacheModel(equipPosition, models);
        }

        private void CreateCacheEffect(string buffId, List<GameEffect> effects)
        {
            DestroyCacheEffect(buffId);
            if (effects == null)
                return;
            cacheEffects[buffId] = effects;
        }

        private void DestroyCacheEffect(string buffId)
        {
            List<GameEffect> oldEffects;
            if (!string.IsNullOrEmpty(buffId) && cacheEffects.TryGetValue(buffId, out oldEffects) && oldEffects != null)
            {
                foreach (var effect in oldEffects)
                {
                    effect.DestroyEffect();
                }
                cacheEffects.Remove(buffId);
            }
        }

        public void SetBuffs(IList<CharacterBuff> buffs)
        {
            var keepingKeys = new List<string>();
            var addingKeys = new List<string>();
            foreach (var buff in buffs)
            {
                var buffId = buff.id;
                keepingKeys.Add(buffId);
                addingKeys.Add(buffId);
            }

            var keys = new List<string>(cacheEffects.Keys);
            foreach (var key in keys)
            {
                if (!keepingKeys.Contains(key))
                    DestroyCacheEffect(key);
                else
                    addingKeys.Remove(key);
            }

            foreach (var buff in buffs)
            {
                var buffId = buff.id;
                if (addingKeys.Contains(buffId))
                {
                    var buffData = buff.GetBuff();
                    InstantiateBuffEffect(buffId, buffData.effects);
                }
            }
        }

        public List<GameEffect> InstantiateEffect(GameEffect[] effects)
        {
            if (effects == null || effects.Length == 0)
                return new List<GameEffect>();
            var newEffects = new List<GameEffect>();
            foreach (var effect in effects)
            {
                if (effect == null)
                    continue;
                var effectSocket = effect.effectSocket;
                if (string.IsNullOrEmpty(effectSocket))
                    continue;
                EffectContainer container;
                if (!CacheEffectContainers.TryGetValue(effectSocket, out container))
                    continue;
                var newEffect = effect.InstantiateTo(null);
                newEffect.followingTarget = container.transform;
                newEffect.CacheTransform.position = newEffect.followingTarget.position;
                newEffect.CacheTransform.rotation = newEffect.followingTarget.rotation;
                newEffect.gameObject.SetActive(true);
                newEffect.gameObject.layer = gameInstance.characterLayer;
                newEffects.Add(newEffect);
            }
            return newEffects;
        }

        public void InstantiateBuffEffect(string buffId, GameEffect[] buffEffects)
        {
            if (buffEffects == null || buffEffects.Length == 0)
                return;
            var effects = InstantiateEffect(buffEffects);
            CreateCacheEffect(buffId, effects);
        }

        public void UpdateAnimation(bool isDead, Vector3 moveVelocity, float playMoveSpeedMultiplier = 1f)
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
                    else if (moveMagnitude < legacyAnimationData.magnitudeToPlayMoveClip)
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

        public Coroutine PlayActionAnimation(int actionId, AnimActionType animActionType, float playSpeedMultiplier = 1f)
        {
            if (animatorType == AnimatorType.LegacyAnimtion)
                return StartCoroutine(PlayActionAnimation_LegacyAnimation(actionId, animActionType, playSpeedMultiplier));
            return StartCoroutine(PlayActionAnimation_Animator(actionId, animActionType, playSpeedMultiplier));
        }

        private IEnumerator PlayActionAnimation_Animator(int actionId, AnimActionType animActionType, float playSpeedMultiplier)
        {
            // If animator is not null, play the action animation
            ActionAnimation actionAnimation;
            AnimationClip clip;
            float triggerDuration;
            float extraDuration;
            AudioClip audioClip;
            if (GameInstance.ActionAnimations.TryGetValue(actionId, out actionAnimation) &&
                actionAnimation.GetData(this, out clip, out triggerDuration, out extraDuration, out audioClip))
            {
                CacheAnimator.SetBool(ANIM_DO_ACTION, false);
                CacheAnimatorController[ANIM_STATE_ACTION_CLIP] = clip;
                if (audioClip != null)
                    AudioSource.PlayClipAtPoint(audioClip, CacheTransform.position, AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level);
                CacheAnimator.SetFloat(ANIM_ACTION_CLIP_MULTIPLIER, playSpeedMultiplier);
                CacheAnimator.SetBool(ANIM_DO_ACTION, true);
                // Waits by current transition + clip duration before end animation
                var waitDelay = CacheAnimator.GetAnimatorTransitionInfo(0).duration + (clip.length / playSpeedMultiplier);
                yield return new WaitForSecondsRealtime(waitDelay);
                CacheAnimator.SetBool(ANIM_DO_ACTION, false);
                // Waits by current transition + extra duration before end playing animation state
                waitDelay = CacheAnimator.GetAnimatorTransitionInfo(0).duration + (extraDuration / playSpeedMultiplier);
                yield return new WaitForSecondsRealtime(waitDelay);
            }
        }

        private IEnumerator PlayActionAnimation_LegacyAnimation(int actionId, AnimActionType animActionType, float playSpeedMultiplier)
        {
            // If animator is not null, play the action animation
            ActionAnimation actionAnimation;
            AnimationClip clip;
            float triggerDuration;
            float extraDuration;
            AudioClip audioClip;
            if (GameInstance.ActionAnimations.TryGetValue(actionId, out actionAnimation) &&
                actionAnimation.GetData(this, out clip, out triggerDuration, out extraDuration, out audioClip))
            {
                if (CacheAnimation.GetClip(LEGACY_CLIP_ACTION) != null)
                    CacheAnimation.RemoveClip(LEGACY_CLIP_ACTION);
                CacheAnimation.AddClip(clip, LEGACY_CLIP_ACTION);
                if (audioClip != null)
                    AudioSource.PlayClipAtPoint(audioClip, CacheTransform.position, AudioManager.Singleton == null ? 1f : AudioManager.Singleton.sfxVolumeSetting.Level);
                CrossFadeLegacyAnimation(LEGACY_CLIP_ACTION, legacyAnimationData.actionClipFadeLength);
                // Waits by current transition + clip duration before end animation
                var waitDelay = clip.length / playSpeedMultiplier;
                yield return new WaitForSecondsRealtime(waitDelay);
                CrossFadeLegacyAnimation(legacyAnimationData.idleClip, legacyAnimationData.idleClipFadeLength);
                // Waits by current transition + extra duration before end playing animation state
                waitDelay = extraDuration / playSpeedMultiplier;
                yield return new WaitForSecondsRealtime(waitDelay);
            }
        }

        public void PlayHurtAnimation()
        {
            if (animatorType == AnimatorType.LegacyAnimtion)
            {
                CrossFadeLegacyAnimation(legacyAnimationData.hurtClip, legacyAnimationData.hurtClipFadeLength);
                return;
            }
            CacheAnimator.ResetTrigger(ANIM_HURT);
            CacheAnimator.SetTrigger(ANIM_HURT);
        }

        public void PlayJumpAnimation()
        {
            if (animatorType == AnimatorType.LegacyAnimtion)
            {
                CrossFadeLegacyAnimation(legacyAnimationData.jumpClip, legacyAnimationData.jumpClipFadeLength);
                return;
            }
            CacheAnimator.ResetTrigger(ANIM_JUMP);
            CacheAnimator.SetTrigger(ANIM_JUMP);
        }
    }

    [System.Serializable]
    public struct EquipmentModelContainer
    {
        public string equipSocket;
        public GameObject defaultModel;
        public Transform transform;
    }

    [System.Serializable]
    public struct EffectContainer
    {
        public string effectSocket;
        public Transform transform;
    }
}
