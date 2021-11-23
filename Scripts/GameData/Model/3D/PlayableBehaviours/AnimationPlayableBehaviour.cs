using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MultiplayerARPG.GameData.Model.Playables
{
    /// <summary>
    /// NOTE: Set its name to default playable behaviour, in the future I might make it able to customize character model's playable behaviour
    /// </summary>
    public class AnimationPlayableBehaviour : PlayableBehaviour
    {
        public static readonly AnimationClip EmptyClip = new AnimationClip();
        public static readonly AvatarMask EmptyMask = new AvatarMask();

        private struct BaseStateInfo
        {
            public int inputPort;
            public AnimState state;
            public float GetSpeed(float rate)
            {
                return (state.animSpeedRate > 0 ? state.animSpeedRate : 1) * rate;
            }

            public float GetClipLength(float rate)
            {
                return state.clip.length / GetSpeed(rate);
            }
        }

        private enum PlayingJumpState
        {
            None,
            Starting,
            Playing,
        }

        public enum PlayingActionState
        {
            None,
            Playing,
            Stopping,
        }

        // Clip name variables
        // Move direction
        public const string DIR_FORWARD = "Forward";
        public const string DIR_BACKWARD = "Backward";
        public const string DIR_LEFT = "Left";
        public const string DIR_RIGHT = "Right";
        // Move
        public const string CLIP_IDLE = "__Idle";
        public const string MOVE_TYPE_SPRINT = "__Sprint";
        public const string MOVE_TYPE_WALK = "__Walk";
        // Crouch
        public const string CLIP_CROUCH_IDLE = "__CrouchIdle";
        public const string MOVE_TYPE_CROUCH = "__CrouchMove";
        // Crawl
        public const string CLIP_CRAWL_IDLE = "__CrawlIdle";
        public const string MOVE_TYPE_CRAWL = "__CrawlMove";
        // Swim
        public const string CLIP_SWIM_IDLE = "__SwimIdle";
        public const string MOVE_TYPE_SWIM = "__SwimMove";
        // Other
        public const string CLIP_JUMP = "__Jump";
        public const string CLIP_FALL = "__Fall";
        public const string CLIP_LANDED = "__Landed";
        public const string CLIP_HURT = "__Hurt";
        public const string CLIP_DEAD = "__Dead";
        public const string CLIP_ACTION = "__Action";
        public const string CLIP_CAST_SKILL = "__CastSkill";
        public const string CLIP_WEAPON_CHARGE = "__WeaponCharge";
        public const string CLIP_PICKUP = "__Pickup";

        public Playable Self { get; private set; }
        public PlayableGraph Graph { get; private set; }
        public AnimationLayerMixerPlayable LayerMixer { get; private set; }
        public AnimationMixerPlayable BaseLayerMixer { get; private set; }
        public AnimationMixerPlayable ActionLayerMixer { get; private set; }
        public PlayableCharacterModel CharacterModel { get; private set; }
        public bool IsFreeze { get; set; }

        private string currentWeaponTypeId = string.Empty;
        private string previousStateId = string.Empty;
        private string playingStateId = string.Empty;
        private PlayingJumpState playingJumpState = PlayingJumpState.None;
        private PlayingActionState playingActionState = PlayingActionState.None;
        private bool isPreviouslyGrounded = true;
        private bool playingLandedState = false;
        private int baseInputPort = 0;
        private float baseTransitionDuration = 0f;
        private float baseClipLength = 0f;
        private float basePlayElapsed = 0f;
        private float actionTransitionDuration = 0f;
        private float actionClipLength = 0f;
        private float actionPlayElapsed = 0f;
        private AnimActionType animActionType = AnimActionType.None;
        private int animDataId = 0;
        private int actionAnimIndex = 0;
        private float actionPlaySpeedMultiplier = 0;
        private float skillCastDuration = 0f;
        private bool isLeftHand = false;
        private float baseLayerClipSpeed = 0f;
        private float actionLayerClipSpeed = 0f;
        private readonly StringBuilder stringBuilder = new StringBuilder();
        private readonly HashSet<string> weaponTypeIds = new HashSet<string>();
        private readonly Dictionary<string, BaseStateInfo> baseStates = new Dictionary<string, BaseStateInfo>();
        private int baseLayerInputPortCount = 0;
        private bool readyToPlay = false;

        public void Setup(PlayableCharacterModel characterModel)
        {
            CharacterModel = characterModel;
            // Setup clips by settings in character model
            // Default
            SetupDefaultAnimations(characterModel.defaultAnimations);
            // Clips based on equipped weapons
            for (int i = 0; i < characterModel.weaponAnimations.Length; ++i)
            {
                SetupWeaponAnimations(characterModel.weaponAnimations[i]);
            }
        }

        public override void OnPlayableCreate(Playable playable)
        {
            Self = playable;
            Self.SetInputCount(1);
            Self.SetInputWeight(0, 1);

            Graph = playable.GetGraph();
            // Create and connect layer mixer to graph
            LayerMixer = AnimationLayerMixerPlayable.Create(Graph, 2);
            Graph.Connect(LayerMixer, 0, Self, 0);

            // Create and connect base layer mixer to layer mixer
            BaseLayerMixer = AnimationMixerPlayable.Create(Graph, 0, true);
            Graph.Connect(BaseLayerMixer, 0, LayerMixer, 0);
            LayerMixer.SetInputWeight(0, 1);

            // Connect to states
            BaseLayerMixer.SetInputCount(baseLayerInputPortCount);
            foreach (BaseStateInfo stateInfo in baseStates.Values)
            {
                AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(Graph, stateInfo.state.clip);
                clipPlayable.SetApplyFootIK(stateInfo.state.applyFootIk);
                clipPlayable.SetApplyPlayableIK(stateInfo.state.applyPlayableIk);
                Graph.Connect(clipPlayable, 0, BaseLayerMixer, stateInfo.inputPort);
            }
            BaseLayerMixer.SetInputWeight(0, 1);
            readyToPlay = true;
        }

        private void SetupDefaultAnimations(DefaultAnimations defaultAnimations)
        {
            SetBaseState(stringBuilder.Clear().Append(CLIP_IDLE).ToString(), defaultAnimations.idleState);
            SetMoveStates(string.Empty, string.Empty, defaultAnimations.moveStates);
            SetMoveStates(string.Empty, MOVE_TYPE_SPRINT, defaultAnimations.sprintStates);
            SetMoveStates(string.Empty, MOVE_TYPE_WALK, defaultAnimations.walkStates);
            SetBaseState(stringBuilder.Clear().Append(CLIP_CROUCH_IDLE).ToString(), defaultAnimations.crouchIdleState);
            SetMoveStates(string.Empty, MOVE_TYPE_CROUCH, defaultAnimations.crouchMoveStates);
            SetBaseState(stringBuilder.Clear().Append(CLIP_CRAWL_IDLE).ToString(), defaultAnimations.crawlIdleState);
            SetMoveStates(string.Empty, MOVE_TYPE_CRAWL, defaultAnimations.crawlMoveStates);
            SetBaseState(stringBuilder.Clear().Append(CLIP_SWIM_IDLE).ToString(), defaultAnimations.swimIdleState);
            SetMoveStates(string.Empty, MOVE_TYPE_SWIM, defaultAnimations.swimMoveStates);
            SetBaseState(stringBuilder.Clear().Append(CLIP_JUMP).ToString(), defaultAnimations.jumpState);
            SetBaseState(stringBuilder.Clear().Append(CLIP_FALL).ToString(), defaultAnimations.fallState);
            SetBaseState(stringBuilder.Clear().Append(CLIP_LANDED).ToString(), defaultAnimations.landedState);
            SetBaseState(stringBuilder.Clear().Append(CLIP_DEAD).ToString(), defaultAnimations.deadState);
        }

        private void SetupWeaponAnimations(WeaponAnimations weaponAnimations)
        {
            if (weaponAnimations.weaponType == null)
                return;
            weaponTypeIds.Add(weaponAnimations.weaponType.Id);
            SetBaseState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_IDLE).ToString(), weaponAnimations.idleState);
            SetMoveStates(weaponAnimations.weaponType.Id, string.Empty, weaponAnimations.moveStates);
            SetMoveStates(weaponAnimations.weaponType.Id, MOVE_TYPE_SPRINT, weaponAnimations.sprintStates);
            SetMoveStates(weaponAnimations.weaponType.Id, MOVE_TYPE_WALK, weaponAnimations.walkStates);
            SetBaseState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_CROUCH_IDLE).ToString(), weaponAnimations.crouchIdleState);
            SetMoveStates(weaponAnimations.weaponType.Id, MOVE_TYPE_CROUCH, weaponAnimations.crouchMoveStates);
            SetBaseState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_CRAWL_IDLE).ToString(), weaponAnimations.crawlIdleState);
            SetMoveStates(weaponAnimations.weaponType.Id, MOVE_TYPE_CRAWL, weaponAnimations.crawlMoveStates);
            SetBaseState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_SWIM_IDLE).ToString(), weaponAnimations.swimIdleState);
            SetMoveStates(weaponAnimations.weaponType.Id, MOVE_TYPE_SWIM, weaponAnimations.swimMoveStates);
            SetBaseState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_JUMP).ToString(), weaponAnimations.jumpState);
            SetBaseState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_FALL).ToString(), weaponAnimations.fallState);
            SetBaseState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_LANDED).ToString(), weaponAnimations.landedState);
            SetBaseState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_DEAD).ToString(), weaponAnimations.deadState);
        }

        private void SetMoveStates(string weaponTypeId, string moveType, MoveStates moveStates)
        {
            SetBaseState(stringBuilder.Clear().Append(weaponTypeId).Append(DIR_FORWARD).Append(moveType).ToString(), moveStates.forwardState);
            SetBaseState(stringBuilder.Clear().Append(weaponTypeId).Append(DIR_BACKWARD).Append(moveType).ToString(), moveStates.backwardState);
            SetBaseState(stringBuilder.Clear().Append(weaponTypeId).Append(DIR_LEFT).Append(moveType).ToString(), moveStates.leftState);
            SetBaseState(stringBuilder.Clear().Append(weaponTypeId).Append(DIR_RIGHT).Append(moveType).ToString(), moveStates.rightState);
            SetBaseState(stringBuilder.Clear().Append(weaponTypeId).Append(DIR_FORWARD).Append(DIR_LEFT).Append(moveType).ToString(), moveStates.forwardLeftState);
            SetBaseState(stringBuilder.Clear().Append(weaponTypeId).Append(DIR_FORWARD).Append(DIR_RIGHT).Append(moveType).ToString(), moveStates.forwardRightState);
            SetBaseState(stringBuilder.Clear().Append(weaponTypeId).Append(DIR_BACKWARD).Append(DIR_LEFT).Append(moveType).ToString(), moveStates.backwardLeftState);
            SetBaseState(stringBuilder.Clear().Append(weaponTypeId).Append(DIR_BACKWARD).Append(DIR_RIGHT).Append(moveType).ToString(), moveStates.backwardRightState);
        }

        private void SetBaseState(string id, AnimState state)
        {
            if (state.clip == null)
            {
                if (!id.Equals(CLIP_IDLE))
                    return;
                // Idle clip is empty, use `EmptyClip`
                state.clip = EmptyClip;
            }
            baseStates[id] = new BaseStateInfo()
            {
                inputPort = baseLayerInputPortCount++,
                state = state,
            };
        }

        private string GetPlayingStateId()
        {
            if (CharacterModel.isDead)
            {
                playingJumpState = PlayingJumpState.None;
                // Get dead state by weapon type
                string stateId = stringBuilder.Clear().Append(currentWeaponTypeId).Append(CLIP_DEAD).ToString();
                // State not found, use dead state from default animations
                if (!baseStates.ContainsKey(stateId))
                    stateId = CLIP_DEAD;
                return stateId;
            }
            else if (playingJumpState == PlayingJumpState.Starting)
            {
                playingJumpState = PlayingJumpState.Playing;
                isPreviouslyGrounded = false;
                // Get jump state by weapon type
                string stateId = stringBuilder.Clear().Append(currentWeaponTypeId).Append(CLIP_JUMP).ToString();
                // State not found, use jump state from default animations
                if (!baseStates.ContainsKey(stateId))
                    stateId = CLIP_JUMP;
                return stateId;
            }
            else if (CharacterModel.movementState.Has(MovementState.IsUnderWater) || CharacterModel.movementState.Has(MovementState.IsGrounded))
            {
                if (playingLandedState || playingJumpState == PlayingJumpState.Playing)
                {
                    // Don't change state because character is just landed, landed animation has to be played before change to move state
                    return playingStateId;
                }
                if (CharacterModel.movementState.Has(MovementState.IsGrounded) && !isPreviouslyGrounded)
                {
                    isPreviouslyGrounded = true;
                    // Get landed state by weapon type
                    string stateId = stringBuilder.Clear().Append(currentWeaponTypeId).Append(CLIP_LANDED).ToString();
                    // State not found, use landed state from default animations
                    if (!baseStates.ContainsKey(stateId))
                        stateId = CLIP_LANDED;
                    // State found, use this state Id. If it not, use move state
                    if (baseStates.ContainsKey(stateId))
                    {
                        playingLandedState = true;
                        return stateId;
                    }
                }
                // Get movement state
                stringBuilder.Clear();
                bool movingForward = CharacterModel.movementState.Has(MovementState.Forward);
                bool movingBackward = CharacterModel.movementState.Has(MovementState.Backward);
                bool movingLeft = CharacterModel.movementState.Has(MovementState.Left);
                bool movingRight = CharacterModel.movementState.Has(MovementState.Right);
                bool moving = (movingForward || movingBackward || movingLeft || movingRight) && CharacterModel.moveAnimationSpeedMultiplier > 0f;
                if (moving)
                {
                    if (movingForward)
                        stringBuilder.Append(DIR_FORWARD);
                    else if (movingBackward)
                        stringBuilder.Append(DIR_BACKWARD);
                    if (movingLeft)
                        stringBuilder.Append(DIR_LEFT);
                    else if (movingRight)
                        stringBuilder.Append(DIR_RIGHT);
                }
                // Set state without move type, it will be used if state with move type not found
                string stateWithoutMoveType = stringBuilder.ToString();
                if (CharacterModel.movementState.Has(MovementState.IsUnderWater))
                {
                    if (!moving)
                        stringBuilder.Append(CLIP_SWIM_IDLE);
                    else
                        stringBuilder.Append(MOVE_TYPE_SWIM);
                }
                else
                {
                    switch (CharacterModel.extraMovementState)
                    {
                        case ExtraMovementState.IsSprinting:
                            if (!moving)
                                stringBuilder.Append(CLIP_IDLE);
                            else
                                stringBuilder.Append(MOVE_TYPE_SPRINT);
                            break;
                        case ExtraMovementState.IsWalking:
                            if (!moving)
                                stringBuilder.Append(CLIP_IDLE);
                            else
                                stringBuilder.Append(MOVE_TYPE_WALK);
                            break;
                        case ExtraMovementState.IsCrouching:
                            if (!moving)
                                stringBuilder.Append(CLIP_CROUCH_IDLE);
                            else
                                stringBuilder.Append(MOVE_TYPE_CROUCH);
                            break;
                        case ExtraMovementState.IsCrawling:
                            if (!moving)
                                stringBuilder.Append(CLIP_CRAWL_IDLE);
                            else
                                stringBuilder.Append(MOVE_TYPE_CRAWL);
                            break;
                        default:
                            if (!moving)
                                stringBuilder.Append(CLIP_IDLE);
                            break;
                    }
                }
                // This is state ID with out current weapon type ID
                string stateWithoutWeaponTypeId = stringBuilder.ToString();
                string stateWithWeaponTypeId = stringBuilder.Clear().Append(currentWeaponTypeId).Append(stateWithoutWeaponTypeId).ToString();
                // State not found, use fall state from default animations
                if (baseStates.ContainsKey(stateWithWeaponTypeId))
                    return stateWithWeaponTypeId;
                if (baseStates.ContainsKey(stateWithoutWeaponTypeId))
                    return stateWithoutWeaponTypeId;
                // If state still not found, find state without move type
                stateWithWeaponTypeId = stringBuilder.Clear().Append(currentWeaponTypeId).Append(stateWithoutMoveType).ToString();
                if (baseStates.ContainsKey(stateWithWeaponTypeId))
                    return stateWithWeaponTypeId;
                return stateWithoutMoveType;
            }
            else if (playingJumpState == PlayingJumpState.Playing)
            {
                // Don't change state because character is jumping, it will change to fall when jump animation played
                return playingStateId;
            }
            else
            {
                isPreviouslyGrounded = false;
                // Get fall state by weapon type
                string stateId = stringBuilder.Clear().Append(currentWeaponTypeId).Append(CLIP_FALL).ToString();
                // State not found, use fall state from default animations
                if (!baseStates.ContainsKey(stateId))
                    stateId = CLIP_FALL;
                return stateId;
            }
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (!readyToPlay)
                return;

            #region Update base state
            if (!IsFreeze)
            {
                // Change state only when previous animation weight >= 1f
                if (BaseLayerMixer.GetInputWeight(baseInputPort) >= 1f)
                {
                    string playingStateId = GetPlayingStateId();
                    if (!this.playingStateId.Equals(playingStateId))
                    {
                        this.playingStateId = playingStateId;
                        // State not found, use idle state
                        if (!baseStates.ContainsKey(playingStateId))
                            playingStateId = stringBuilder.Clear().Append(currentWeaponTypeId).Append(CLIP_IDLE).ToString();
                        // State still not found, use idle state from default animations
                        if (!baseStates.ContainsKey(playingStateId))
                            playingStateId = CLIP_IDLE;
                        // Get previous clip to continue playing it
                        if (string.IsNullOrEmpty(previousStateId))
                            previousStateId = playingStateId;
                        // Get input port from new playing state ID
                        baseInputPort = baseStates[playingStateId].inputPort;
                        // Set clip info 
                        baseLayerClipSpeed = baseStates[playingStateId].GetSpeed(1);
                        // Set transition duration
                        baseTransitionDuration = baseStates[playingStateId].state.transitionDuration;
                        if (baseTransitionDuration <= 0f)
                            baseTransitionDuration = CharacterModel.transitionDuration;
                        baseTransitionDuration /= baseLayerClipSpeed;
                        BaseLayerMixer.GetInput(baseInputPort).Play();
                        baseClipLength = baseStates[playingStateId].GetClipLength(1);
                        // Set layer additive
                        LayerMixer.SetLayerAdditive(0, baseStates[playingStateId].state.isAdditive);
                        // Reset play elapsed
                        basePlayElapsed = 0f;
                        // Set previous state Id for next state change updating
                        previousStateId = playingStateId;
                    }
                }
            }

            // Update freezing state
            BaseLayerMixer.GetInput(baseInputPort).SetSpeed(IsFreeze ? 0 : baseLayerClipSpeed);

            // Update transition
            float weight;
            float weightUpdate = info.deltaTime / baseTransitionDuration;
            int inputCount = BaseLayerMixer.GetInputCount();
            for (int i = 0; i < inputCount; ++i)
            {
                weight = BaseLayerMixer.GetInputWeight(i);
                if (i != baseInputPort)
                {
                    weight -= weightUpdate;
                    if (weight < 0f)
                        weight = 0f;
                }
                else
                {
                    weight += weightUpdate;
                    if (weight > 1f)
                        weight = 1f;
                }
                BaseLayerMixer.SetInputWeight(i, weight);
                if (weight <= 0)
                {
                    BaseLayerMixer.GetInput(i).Pause();
                    BaseLayerMixer.GetInput(i).SetTime(0);
                }
            }

            // Update playing state
            basePlayElapsed += info.deltaTime;

            // It will change state to fall in next frame
            if (playingJumpState == PlayingJumpState.Playing && basePlayElapsed >= baseClipLength)
                playingJumpState = PlayingJumpState.None;

            // It will change state to movement in next frame
            if (playingLandedState && basePlayElapsed >= baseClipLength)
                playingLandedState = false;
            #endregion

            #region Update action state
            if (playingActionState == PlayingActionState.None)
                return;

            if (CharacterModel.isDead && playingActionState != PlayingActionState.Stopping)
            {
                // Character dead, stop action animation
                playingActionState = PlayingActionState.Stopping;
            }

            // Update freezing state
            ActionLayerMixer.GetInput(0).SetSpeed(IsFreeze ? 0 : actionLayerClipSpeed);

            // Update transition
            weightUpdate = info.deltaTime / actionTransitionDuration;
            weight = LayerMixer.GetInputWeight(1);
            switch (playingActionState)
            {
                case PlayingActionState.Playing:
                    weight += weightUpdate;
                    if (weight > 1f)
                        weight = 1f;
                    break;
                case PlayingActionState.Stopping:
                    weight -= weightUpdate;
                    if (weight < 0f)
                        weight = 0f;
                    break;
            }
            LayerMixer.SetInputWeight(1, weight);

            // Update playing state
            actionPlayElapsed += info.deltaTime;

            // Stopped
            if (weight <= 0f)
            {
                playingActionState = PlayingActionState.None;
                if (ActionLayerMixer.IsValid())
                    ActionLayerMixer.Destroy();
                return;
            }

            // Animation end, transition to idle
            if (actionPlayElapsed >= actionClipLength && playingActionState == PlayingActionState.Playing)
            {
                playingActionState = PlayingActionState.Stopping;
            }
            #endregion
        }

        public void SetPlayingWeaponTypeId(IWeaponItem weaponItem)
        {
            currentWeaponTypeId = string.Empty;
            if (weaponItem != null && weaponTypeIds.Contains(weaponItem.WeaponType.Id))
                currentWeaponTypeId = weaponItem.WeaponType.Id;
        }

        public void PlayJump()
        {
            playingJumpState = PlayingJumpState.Starting;
        }

        public void PlayAction(ActionState actionState, float speedRate, float duration = 0f)
        {
            if (IsFreeze || CharacterModel.isDead)
                return;

            // Destroy playing state
            if (ActionLayerMixer.IsValid())
                ActionLayerMixer.Destroy();

            ActionLayerMixer = AnimationMixerPlayable.Create(Graph, 1, true);
            Graph.Connect(ActionLayerMixer, 0, LayerMixer, 1);
            LayerMixer.SetInputWeight(1, 0f);

            AnimationClip clip = actionState.clip != null ? actionState.clip : EmptyClip;
            AnimationClipPlayable playable = AnimationClipPlayable.Create(Graph, clip);
            playable.SetApplyFootIK(actionState.applyFootIk);
            playable.SetApplyPlayableIK(actionState.applyPlayableIk);
            Graph.Connect(playable, 0, ActionLayerMixer, 0);
            ActionLayerMixer.SetInputWeight(0, 1f);

            // Set avatar mask
            AvatarMask avatarMask = actionState.avatarMask;
            if (avatarMask == null)
                avatarMask = CharacterModel.actionAvatarMask;
            if (avatarMask == null)
                avatarMask = EmptyMask;
            LayerMixer.SetLayerMaskFromAvatarMask(1, avatarMask);

            // Set clip info
            actionLayerClipSpeed = (actionState.animSpeedRate > 0f ? actionState.animSpeedRate : 1f) * speedRate;
            // Set transition duration
            actionTransitionDuration = actionState.transitionDuration;
            if (actionTransitionDuration <= 0f)
                actionTransitionDuration = CharacterModel.transitionDuration;
            actionTransitionDuration /= actionLayerClipSpeed;
            // Set clip length
            ActionLayerMixer.GetInput(0).SetTime(0f);
            actionClipLength = (duration > 0f ? duration : clip.length) / actionLayerClipSpeed;
            // Set layer additive
            LayerMixer.SetLayerAdditive(1, actionState.isAdditive);
            // Reset play elapsed
            actionPlayElapsed = 0f;

            playingActionState = PlayingActionState.Playing;
        }

        public void StopAction()
        {
            if (playingActionState == PlayingActionState.Playing)
                playingActionState = PlayingActionState.Stopping;
        }
    }
}