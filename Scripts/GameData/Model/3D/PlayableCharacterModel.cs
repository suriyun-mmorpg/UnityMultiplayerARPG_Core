using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MultiplayerARPG.GameData.Model.Playables
{
    public class PlayableCharacterModel : BaseCharacterModel
    {
        [Header("Renderer")]
        [Tooltip("This will be used to apply bone weights when equip an equipments")]
        public SkinnedMeshRenderer skinnedMeshRenderer;

        [Header("Animations")]
        [Tooltip("If `avatarMask` in action state settings is `null`, it will use this value")]
        public AvatarMask actionAvatarMask;
        [Tooltip("If `transitionDuration` in state settings is <= 0, it will use this value")]
        public float transitionDuration;
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

        protected override void Awake()
        {
            base.Awake();
            PrepareMissingMovementAnimations();
            Template = new AnimationPlayableBehaviour();
            Template.Setup(this);
        }

        internal override void OnSwitchingToAnotherModel()
        {
            if (Graph.IsValid())
                Graph.Destroy();
        }

        internal override void OnSwitchedToThisModel()
        {
            Graph = PlayableGraph.Create();
            Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            ScriptPlayable<AnimationPlayableBehaviour> playable = ScriptPlayable<AnimationPlayableBehaviour>.Create(Graph, Template, 1);
            Behaviour = playable.GetBehaviour();
            AnimationPlayableOutput output = AnimationPlayableOutput.Create(Graph, "Output", GetComponent<Animator>());
            output.SetSourcePlayable(playable);
            Graph.Play();
        }

        protected AnimState SetClipFromDefaultStateIfEmpty(AnimState defaultState, AnimState targetState)
        {
            if (targetState.clip == null)
                targetState.clip = defaultState.clip;
            return targetState;
        }

        protected MoveStates SetStatesFromDefaultStatesIfEmpty(MoveStates targetStates)
        {
            targetStates.forwardLeftState = SetClipFromDefaultStateIfEmpty(targetStates.forwardState, targetStates.forwardLeftState);
            targetStates.forwardRightState = SetClipFromDefaultStateIfEmpty(targetStates.forwardState, targetStates.forwardRightState);
            targetStates.leftState = SetClipFromDefaultStateIfEmpty(targetStates.forwardState, targetStates.leftState);
            targetStates.rightState = SetClipFromDefaultStateIfEmpty(targetStates.forwardState, targetStates.rightState);
            targetStates.backwardState = SetClipFromDefaultStateIfEmpty(targetStates.forwardState, targetStates.backwardState);
            targetStates.backwardLeftState = SetClipFromDefaultStateIfEmpty(targetStates.backwardState, targetStates.backwardLeftState);
            targetStates.backwardRightState = SetClipFromDefaultStateIfEmpty(targetStates.backwardState, targetStates.backwardRightState);
            return targetStates;
        }

        protected MoveStates SetStatesFromDefaultStatesIfEmpty(MoveStates defaultStates, MoveStates targetStates)
        {
            targetStates.forwardState = SetClipFromDefaultStateIfEmpty(defaultStates.forwardState, targetStates.forwardState);
            targetStates.forwardLeftState = SetClipFromDefaultStateIfEmpty(targetStates.forwardState, targetStates.forwardLeftState);
            targetStates.forwardRightState = SetClipFromDefaultStateIfEmpty(targetStates.forwardState, targetStates.forwardRightState);
            targetStates.leftState = SetClipFromDefaultStateIfEmpty(targetStates.forwardState, targetStates.leftState);
            targetStates.rightState = SetClipFromDefaultStateIfEmpty(targetStates.forwardState, targetStates.rightState);
            targetStates.backwardState = SetClipFromDefaultStateIfEmpty(targetStates.forwardState, targetStates.backwardState);
            targetStates.backwardLeftState = SetClipFromDefaultStateIfEmpty(targetStates.backwardState, targetStates.backwardLeftState);
            targetStates.backwardRightState = SetClipFromDefaultStateIfEmpty(targetStates.backwardState, targetStates.backwardRightState);
            return targetStates;
        }


        protected void PrepareMissingMovementAnimations()
        {
            DefaultAnimations tempDefaultAnimations = defaultAnimations;
            // Move
            tempDefaultAnimations.moveStates = SetStatesFromDefaultStatesIfEmpty(tempDefaultAnimations.moveStates);
            // Sprint
            tempDefaultAnimations.sprintStates = SetStatesFromDefaultStatesIfEmpty(tempDefaultAnimations.moveStates, tempDefaultAnimations.sprintStates);
            // Walk
            tempDefaultAnimations.walkStates = SetStatesFromDefaultStatesIfEmpty(tempDefaultAnimations.moveStates, tempDefaultAnimations.walkStates);
            // Crouch
            tempDefaultAnimations.crouchMoveStates = SetStatesFromDefaultStatesIfEmpty(tempDefaultAnimations.moveStates, tempDefaultAnimations.crouchMoveStates);
            // Crawl
            tempDefaultAnimations.crawlMoveStates = SetStatesFromDefaultStatesIfEmpty(tempDefaultAnimations.moveStates, tempDefaultAnimations.crawlMoveStates);
            // Swim
            tempDefaultAnimations.swimMoveStates = SetStatesFromDefaultStatesIfEmpty(tempDefaultAnimations.moveStates, tempDefaultAnimations.swimMoveStates);
            // Apply
            defaultAnimations = tempDefaultAnimations;

            // Weapon Animations
            if (weaponAnimations != null && weaponAnimations.Length > 0)
            {
                WeaponAnimations tempWeaponAnimations;
                for (int i = 0; i < weaponAnimations.Length; ++i)
                {
                    tempWeaponAnimations = weaponAnimations[i];
                    // Move
                    tempWeaponAnimations.moveStates = SetStatesFromDefaultStatesIfEmpty(tempDefaultAnimations.moveStates, tempWeaponAnimations.moveStates);
                    // Sprint
                    tempWeaponAnimations.sprintStates = SetStatesFromDefaultStatesIfEmpty(tempWeaponAnimations.moveStates, tempWeaponAnimations.sprintStates);
                    // Walk
                    tempWeaponAnimations.walkStates = SetStatesFromDefaultStatesIfEmpty(tempWeaponAnimations.moveStates, tempWeaponAnimations.walkStates);
                    // Crouch
                    tempWeaponAnimations.crouchMoveStates = SetStatesFromDefaultStatesIfEmpty(tempWeaponAnimations.moveStates, tempWeaponAnimations.crouchMoveStates);
                    // Crawl
                    tempWeaponAnimations.crawlMoveStates = SetStatesFromDefaultStatesIfEmpty(tempWeaponAnimations.moveStates, tempWeaponAnimations.crawlMoveStates);
                    // Swim
                    tempWeaponAnimations.swimMoveStates = SetStatesFromDefaultStatesIfEmpty(tempWeaponAnimations.moveStates, tempWeaponAnimations.swimMoveStates);
                    // Apply
                    weaponAnimations[i] = tempWeaponAnimations;
                }
            }
        }

        private void OnDestroy()
        {
            if (Graph.IsValid())
                Graph.Destroy();
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
                    tempActionAnimation = GetRightHandAttackAnimations(dataId)[index];
                    break;
                case AnimActionType.AttackLeftHand:
                    tempActionAnimation = GetLeftHandAttackAnimations(dataId)[index];
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
            StartedActionCoroutine(StartCoroutine(PlaySkillCastClip_Animator(dataId, duration)));
        }

        private IEnumerator PlaySkillCastClip_Animator(int dataId, float duration)
        {
            ActionState castState = GetSkillCastState(dataId);
            bool hasClip = castState.clip != null;
            if (hasClip)
                Behaviour.PlayAction(castState, 1f);
            // Waits by skill cast duration
            yield return new WaitForSecondsRealtime(duration);
            // Stop casting skill animation
            if (hasClip)
                Behaviour.StopAction();
        }

        public override void StopSkillCastAnimation()
        {
            Behaviour.StopAction();
        }
        #endregion

        #region Action animations
        public override void PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1)
        {
            StartedActionCoroutine(StartCoroutine(PlayActionAnimation_Animator(animActionType, dataId, index, playSpeedMultiplier)));
        }

        private IEnumerator PlayActionAnimation_Animator(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier)
        {
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
        }

        public override void StopActionAnimation()
        {
            Behaviour.StopAction();
        }
        #endregion

        #region Weapon charge animations
        public override void PlayWeaponChargeClip(int dataId, bool isLeftHand)
        {
            WeaponAnimations weaponAnimations;
            if (TryGetWeaponAnimations(dataId, out weaponAnimations))
            {
                if (isLeftHand)
                    Behaviour.PlayAction(weaponAnimations.leftHandChargeState, 1f);
                else
                    Behaviour.PlayAction(weaponAnimations.rightHandChargeState, 1f);
            }
            else
            {
                if (isLeftHand)
                    Behaviour.PlayAction(defaultAnimations.leftHandChargeState, 1f);
                else
                    Behaviour.PlayAction(defaultAnimations.rightHandChargeState, 1f);
            }
        }

        public override void StopWeaponChargeAnimation()
        {
            Behaviour.StopAction();
        }
        #endregion

        #region Other animations
        public override void PlayMoveAnimation()
        {
            // Do nothing, animation playable behaviour will do it
        }

        public override void PlayHitAnimation()
        {
            if (actionCoroutine == null)
                return;
            WeaponAnimations weaponAnimations;
            if (equippedWeaponType != null && TryGetWeaponAnimations(equippedWeaponType.DataId, out weaponAnimations))
                Behaviour.PlayAction(weaponAnimations.hurtState, 1f);
            else
                Behaviour.PlayAction(defaultAnimations.hurtState, 1f);
        }

        public override void PlayJumpAnimation()
        {
            Behaviour.PlayJump();
        }

        public override void PlayPickupAnimation()
        {
            if (actionCoroutine == null)
                return;
            WeaponAnimations weaponAnimations;
            if (equippedWeaponType != null && TryGetWeaponAnimations(equippedWeaponType.DataId, out weaponAnimations))
                Behaviour.PlayAction(weaponAnimations.pickupState, 1f);
            else
                Behaviour.PlayAction(defaultAnimations.pickupState, 1f);
        }
        #endregion

        protected Coroutine StartedActionCoroutine(Coroutine coroutine)
        {
            StopActionCoroutine();
            actionCoroutine = coroutine;
            return actionCoroutine;
        }

        protected void StopActionCoroutine()
        {
            if (actionCoroutine != null)
                StopCoroutine(actionCoroutine);
        }
    }
}
