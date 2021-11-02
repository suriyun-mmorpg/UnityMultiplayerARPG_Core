using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MultiplayerARPG.GameData.Model.Playables
{
    public class PlayableCharacterModel : BaseCharacterModel
    {
        [Header("Relates Components")]
        [Tooltip("It will find `Animator` component on automatically if this is NULL")]
        public Animator animator;

        [Header("Renderer")]
        [Tooltip("This will be used to apply bone weights when equip an equipments")]
        public SkinnedMeshRenderer skinnedMeshRenderer;

        [Header("Animations")]
        [Tooltip("If `avatarMask` in action state settings is `null`, it will use this value")]
        public AvatarMask actionAvatarMask;
        [Tooltip("If `transitionDuration` in state settings is <= 0, it will use this value")]
        public float transitionDuration = 0.1f;
        public DefaultAnimations defaultAnimations;
        [ArrayElementTitle("weaponType")]
        public WeaponAnimations[] weaponAnimations;
        [ArrayElementTitle("skill")]
        public SkillAnimations[] skillAnimations;

        public PlayableGraph Graph { get; protected set; }
        public AnimationPlayableBehaviour Template { get; protected set; }
        public AnimationPlayableBehaviour Behaviour { get; protected set; }

        protected WeaponType equippedWeaponType = null;
        protected Coroutine actionCoroutine = null;
        protected bool isDoingAction = false;

        protected override void Awake()
        {
            base.Awake();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            Template = new AnimationPlayableBehaviour();
            Template.Setup(this);
            CreateGraph();
        }

        private void Start()
        {
            if (!IsMainModel)
                Graph.Stop();
        }

        protected void CreateGraph()
        {
            Graph = PlayableGraph.Create($"{name}.PlayableCharacterModel");
            Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            ScriptPlayable<AnimationPlayableBehaviour> playable = ScriptPlayable<AnimationPlayableBehaviour>.Create(Graph, Template, 1);
            Behaviour = playable.GetBehaviour();
            AnimationPlayableOutput output = AnimationPlayableOutput.Create(Graph, "Output", animator);
            output.SetSourcePlayable(playable);
            Graph.Play();
        }

        protected void DestroyGraph()
        {
            if (Graph.IsValid())
                Graph.Destroy();
        }

        internal override void OnSwitchingToAnotherModel()
        {
            if (Graph.IsValid())
                Graph.Stop();
        }

        internal override void OnSwitchedToThisModel()
        {
            if (Graph.IsValid())
                Graph.Play();
        }

        private void OnDestroy()
        {
            DestroyGraph();
        }

        public bool TryGetWeaponAnimations(int dataId, out WeaponAnimations anims)
        {
            return CacheAnimationsManager.SetAndTryGetCacheWeaponAnimations(Id, weaponAnimations, skillAnimations, dataId, out anims);
        }

        public bool TryGetSkillAnimations(int dataId, out SkillAnimations anims)
        {
            return CacheAnimationsManager.SetAndTryGetCacheSkillAnimations(Id, weaponAnimations, skillAnimations, dataId, out anims);
        }

        public ActionAnimation GetActionAnimation(AnimActionType animActionType, int dataId, int index)
        {
            ActionAnimation tempActionAnimation = default;
            switch (animActionType)
            {
                case AnimActionType.AttackRightHand:
                    ActionAnimation[] rightHandAnims = GetRightHandAttackAnimations(dataId);
                    if (index >= rightHandAnims.Length)
                        index = 0;
                    if (index < rightHandAnims.Length)
                        tempActionAnimation = rightHandAnims[index];
                    break;
                case AnimActionType.AttackLeftHand:
                    ActionAnimation[] leftHandAnims = GetLeftHandAttackAnimations(dataId);
                    if (index >= leftHandAnims.Length)
                        index = 0;
                    if (index < leftHandAnims.Length)
                        tempActionAnimation = leftHandAnims[index];
                    break;
                case AnimActionType.SkillRightHand:
                case AnimActionType.SkillLeftHand:
                    tempActionAnimation = GetSkillActivateAnimation(dataId);
                    break;
                case AnimActionType.ReloadRightHand:
                    tempActionAnimation = GetRightHandReloadAnimation(dataId);
                    break;
                case AnimActionType.ReloadLeftHand:
                    tempActionAnimation = GetLeftHandReloadAnimation(dataId);
                    break;
            }
            return tempActionAnimation;
        }

        public override void SetEquipWeapons(EquipWeapons equipWeapons)
        {
            base.SetEquipWeapons(equipWeapons);
            // Get one equipped weapon from right-hand or left-hand
            IWeaponItem weaponItem = equipWeapons.GetRightHandWeaponItem();
            if (weaponItem == null)
                weaponItem = equipWeapons.GetLeftHandWeaponItem();
            // Set equipped weapon type, it will be used to get animations by id
            equippedWeaponType = null;
            if (weaponItem != null)
                equippedWeaponType = weaponItem.WeaponType;
            if (Behaviour != null)
                Behaviour.SetPlayingWeaponTypeId(weaponItem);
        }

        #region Right-hand animations
        public ActionAnimation[] GetRightHandAttackAnimations(int dataId)
        {
            WeaponAnimations anims;
            if (TryGetWeaponAnimations(dataId, out anims) && anims.rightHandAttackAnimations != null)
                return anims.rightHandAttackAnimations;
            return defaultAnimations.rightHandAttackAnimations;
        }

        public ActionAnimation GetRightHandReloadAnimation(int dataId)
        {
            WeaponAnimations anims;
            if (TryGetWeaponAnimations(dataId, out anims) && anims.rightHandReloadAnimation.state.clip != null)
                return anims.rightHandReloadAnimation;
            return defaultAnimations.rightHandReloadAnimation;
        }

        public override bool GetRightHandAttackAnimation(int dataId, int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            ActionAnimation[] tempActionAnimations = GetRightHandAttackAnimations(dataId);
            animSpeedRate = 1f;
            triggerDurations = new float[] { 0f };
            totalDuration = 0f;
            if (tempActionAnimations.Length == 0 || animationIndex >= tempActionAnimations.Length) return false;
            animSpeedRate = tempActionAnimations[animationIndex].GetAnimSpeedRate();
            triggerDurations = tempActionAnimations[animationIndex].GetTriggerDurations();
            totalDuration = tempActionAnimations[animationIndex].GetTotalDuration();
            return true;
        }

        public override bool GetRightHandReloadAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            ActionAnimation tempActionAnimation = GetRightHandReloadAnimation(dataId);
            animSpeedRate = tempActionAnimation.GetAnimSpeedRate();
            triggerDurations = tempActionAnimation.GetTriggerDurations();
            totalDuration = tempActionAnimation.GetTotalDuration();
            return true;
        }

        public override bool GetRandomRightHandAttackAnimation(int dataId, int randomSeed, out int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            animationIndex = GenericUtils.RandomInt(randomSeed, 0, GetRightHandAttackAnimations(dataId).Length);
            return GetRightHandAttackAnimation(dataId, animationIndex, out animSpeedRate, out triggerDurations, out totalDuration);
        }
        #endregion

        #region Left-hand animations
        public ActionAnimation[] GetLeftHandAttackAnimations(int dataId)
        {
            WeaponAnimations anims;
            if (TryGetWeaponAnimations(dataId, out anims) && anims.leftHandAttackAnimations != null)
                return anims.leftHandAttackAnimations;
            return defaultAnimations.leftHandAttackAnimations;
        }

        public ActionAnimation GetLeftHandReloadAnimation(int dataId)
        {
            WeaponAnimations anims;
            if (TryGetWeaponAnimations(dataId, out anims) && anims.leftHandReloadAnimation.state.clip != null)
                return anims.leftHandReloadAnimation;
            return defaultAnimations.leftHandReloadAnimation;
        }

        public override bool GetLeftHandAttackAnimation(int dataId, int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            ActionAnimation[] tempActionAnimations = GetLeftHandAttackAnimations(dataId);
            animSpeedRate = 1f;
            triggerDurations = new float[] { 0f };
            totalDuration = 0f;
            if (tempActionAnimations.Length == 0 || animationIndex >= tempActionAnimations.Length) return false;
            animSpeedRate = tempActionAnimations[animationIndex].GetAnimSpeedRate();
            triggerDurations = tempActionAnimations[animationIndex].GetTriggerDurations();
            totalDuration = tempActionAnimations[animationIndex].GetTotalDuration();
            return true;
        }

        public override bool GetLeftHandReloadAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            ActionAnimation tempActionAnimation = GetLeftHandReloadAnimation(dataId);
            animSpeedRate = tempActionAnimation.GetAnimSpeedRate();
            triggerDurations = tempActionAnimation.GetTriggerDurations();
            totalDuration = tempActionAnimation.GetTotalDuration();
            return true;
        }

        public override bool GetRandomLeftHandAttackAnimation(int dataId, int randomSeed, out int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            animationIndex = GenericUtils.RandomInt(randomSeed, 0, GetLeftHandAttackAnimations(dataId).Length);
            return GetLeftHandAttackAnimation(dataId, animationIndex, out animSpeedRate, out triggerDurations, out totalDuration);
        }
        #endregion

        #region Skill animations
        public ActionAnimation GetSkillActivateAnimation(int dataId)
        {
            SkillAnimations anims;
            if (TryGetSkillAnimations(dataId, out anims) && anims.activateAnimation.state.clip != null)
                return anims.activateAnimation;
            return defaultAnimations.skillActivateAnimation;
        }

        public ActionState GetSkillCastState(int dataId)
        {
            SkillAnimations anims;
            if (TryGetSkillAnimations(dataId, out anims) && anims.castState.clip != null)
                return anims.castState;
            return defaultAnimations.skillCastState;
        }

        public override bool GetSkillActivateAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            ActionAnimation tempActionAnimation = GetSkillActivateAnimation(dataId);
            animSpeedRate = tempActionAnimation.GetAnimSpeedRate();
            triggerDurations = tempActionAnimation.GetTriggerDurations();
            totalDuration = tempActionAnimation.GetTotalDuration();
            return true;
        }

        public override SkillActivateAnimationType GetSkillActivateAnimationType(int dataId)
        {
            SkillAnimations anims;
            if (!TryGetSkillAnimations(dataId, out anims))
                return SkillActivateAnimationType.UseActivateAnimation;
            return anims.activateAnimationType;
        }

        public override void PlaySkillCastClip(int dataId, float duration)
        {
            StartedActionCoroutine(StartCoroutine(PlaySkillCastClipRoutine(dataId, duration)));
        }

        private IEnumerator PlaySkillCastClipRoutine(int dataId, float duration)
        {
            isDoingAction = true;
            ActionState castState = GetSkillCastState(dataId);
            bool hasClip = castState.clip != null;
            if (hasClip)
                Behaviour.PlayAction(castState, 1f, duration);
            // Waits by skill cast duration
            yield return new WaitForSecondsRealtime(duration);
            // Stop casting skill animation
            if (hasClip)
                Behaviour.StopAction();
            isDoingAction = false;
        }

        public override void StopSkillCastAnimation()
        {
            Behaviour.StopAction();
            isDoingAction = false;
        }
        #endregion

        #region Action animations
        public override void PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1)
        {
            StartedActionCoroutine(StartCoroutine(PlayActionAnimationRoutine(animActionType, dataId, index, playSpeedMultiplier)));
        }

        private IEnumerator PlayActionAnimationRoutine(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier)
        {
            isDoingAction = true;
            ActionAnimation tempActionAnimation = GetActionAnimation(animActionType, dataId, index);
            AudioManager.PlaySfxClipAtAudioSource(tempActionAnimation.GetRandomAudioClip(), genericAudioSource);
            bool hasClip = tempActionAnimation.state.clip != null;
            if (hasClip)
                Behaviour.PlayAction(tempActionAnimation.state, playSpeedMultiplier);
            // Waits by current transition + clip duration before end animation
            yield return new WaitForSecondsRealtime(tempActionAnimation.GetClipLength() / playSpeedMultiplier);
            // Stop doing action animation
            if (hasClip)
                Behaviour.StopAction();
            // Waits by current transition + extra duration before end playing animation state
            yield return new WaitForSecondsRealtime(tempActionAnimation.GetExtendDuration() / playSpeedMultiplier);
            isDoingAction = false;
        }

        public override void StopActionAnimation()
        {
            Behaviour.StopAction();
            isDoingAction = false;
        }
        #endregion

        #region Weapon charge animations
        public override void PlayWeaponChargeClip(int dataId, bool isLeftHand)
        {
            isDoingAction = true;
            WeaponAnimations weaponAnimations;
            if (TryGetWeaponAnimations(dataId, out weaponAnimations))
            {
                if (isLeftHand && weaponAnimations.leftHandChargeState.clip != null)
                {
                    Behaviour.PlayAction(weaponAnimations.leftHandChargeState, 1f);
                    return;
                }
                if (!isLeftHand && weaponAnimations.rightHandChargeState.clip != null)
                {
                    Behaviour.PlayAction(weaponAnimations.rightHandChargeState, 1f);
                    return;
                }
            }
            if (isLeftHand)
                Behaviour.PlayAction(defaultAnimations.leftHandChargeState, 1f);
            else
                Behaviour.PlayAction(defaultAnimations.rightHandChargeState, 1f);
        }

        public override void StopWeaponChargeAnimation()
        {
            Behaviour.StopAction();
            isDoingAction = false;
        }
        #endregion

        #region Other animations
        public override void PlayMoveAnimation()
        {
            // Do nothing, animation playable behaviour will do it
            if (Behaviour != null)
                Behaviour.IsFreeze = isFreezeAnimation;
        }

        public override void PlayHitAnimation()
        {
            if (isDoingAction)
                return;
            WeaponAnimations weaponAnimations;
            if (equippedWeaponType != null && TryGetWeaponAnimations(equippedWeaponType.DataId, out weaponAnimations) && weaponAnimations.hurtState.clip != null)
            {
                Behaviour.PlayAction(weaponAnimations.hurtState, 1f);
                return;
            }
            if (defaultAnimations.hurtState.clip != null)
                Behaviour.PlayAction(defaultAnimations.hurtState, 1f);
        }

        public override void PlayJumpAnimation()
        {
            Behaviour.PlayJump();
        }

        public override void PlayPickupAnimation()
        {
            if (isDoingAction)
                return;
            WeaponAnimations weaponAnimations;
            if (equippedWeaponType != null && TryGetWeaponAnimations(equippedWeaponType.DataId, out weaponAnimations) && weaponAnimations.pickupState.clip != null)
            {
                Behaviour.PlayAction(weaponAnimations.pickupState, 1f);
                return;
            }
            if (defaultAnimations.pickupState.clip != null)
                Behaviour.PlayAction(defaultAnimations.pickupState, 1f);
        }
        #endregion

        protected Coroutine StartedActionCoroutine(Coroutine coroutine)
        {
            StopActionCoroutine();
            if (actionCoroutine != null)
                StopCoroutine(actionCoroutine);
            actionCoroutine = coroutine;
            isDoingAction = true;
            return actionCoroutine;
        }

        protected void StopActionCoroutine()
        {
            if (actionCoroutine != null)
                StopCoroutine(actionCoroutine);
            actionCoroutine = null;
            isDoingAction = false;
        }
    }
}
