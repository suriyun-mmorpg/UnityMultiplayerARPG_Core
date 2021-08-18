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
        [Tooltip("If `avatarMask` in action clip settings is `null`, it will use this value")]
        public AvatarMask actionAvatarMask;
        [Tooltip("If `transitionDuration` in clip settings is <= 0, it will use this value")]
        public float transitionDuration;
        public DefaultAnimations defaultAnimations;
        [ArrayElementTitle("weaponType")]
        public WeaponAnimations[] weaponAnimations;
        [ArrayElementTitle("skill")]
        public SkillAnimations[] skillAnimations;

        public PlayableGraph Graph { get; protected set; }
        public AnimationPlayableBehaviour Behaviour { get; protected set; }

        protected override void Awake()
        {
            base.Awake();
            PrepareMissingMovementAnimations();
            Graph = PlayableGraph.Create();
            Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            AnimationPlayableBehaviour template = new AnimationPlayableBehaviour();
            template.Setup(this);
            ScriptPlayable<AnimationPlayableBehaviour> playable = ScriptPlayable<AnimationPlayableBehaviour>.Create(Graph, template, 1);
            Behaviour = playable.GetBehaviour();
            AnimationPlayableOutput output = AnimationPlayableOutput.Create(Graph, "Output", GetComponent<Animator>());
            output.SetSourcePlayable(playable);
        }

        internal override void OnSwitchingToAnotherModel()
        {
            if (Graph.IsValid() && Graph.IsPlaying())
                Graph.Stop();
        }

        internal override void OnSwitchedToThisModel()
        {
            if (Graph.IsValid() && !Graph.IsPlaying())
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
            Graph.Destroy();
        }

        #region Right-hand animations
        public override bool GetRightHandAttackAnimation(int dataId, int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }

        public override bool GetRightHandReloadAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }

        public override bool GetRandomRightHandAttackAnimation(int dataId, int randomSeed, out int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Left-hand animations
        public override bool GetLeftHandAttackAnimation(int dataId, int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }

        public override bool GetLeftHandReloadAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }

        public override bool GetRandomLeftHandAttackAnimation(int dataId, int randomSeed, out int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Skill animations
        public override bool GetSkillActivateAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            throw new System.NotImplementedException();
        }

        public override SkillActivateAnimationType UseSkillActivateAnimationType(int dataId)
        {
            throw new System.NotImplementedException();
        }

        public override Coroutine PlaySkillCastClip(int dataId, float duration)
        {
            throw new System.NotImplementedException();
        }

        public override void StopSkillCastAnimation()
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Action animations
        public override Coroutine PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1)
        {
            throw new System.NotImplementedException();
        }

        public override void StopActionAnimation()
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Weapon charge animations
        public override void PlayWeaponChargeClip(int dataId, bool isLeftHand)
        {
            throw new System.NotImplementedException();
        }

        public override void StopWeaponChargeAnimation()
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Other animations
        public override void PlayMoveAnimation()
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}
