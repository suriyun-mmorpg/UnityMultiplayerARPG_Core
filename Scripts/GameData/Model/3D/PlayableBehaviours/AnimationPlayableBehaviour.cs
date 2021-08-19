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
        private struct BaseStateInfo
        {
            public int inputPort;
            public AnimState state;
            public float GetSpeed(float rate)
            {
                return state.animSpeedRate * rate;
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
        public const string CLIP_MOVE = "__Move";
        public const string CLIP_SPRINT = "__Sprint";
        public const string CLIP_WALK = "__Walk";
        // Crouch
        public const string CLIP_CROUCH_IDLE = "__CrouchIdle";
        public const string CLIP_CROUCH_MOVE = "__CrouchMove";
        // Crawl
        public const string CLIP_CRAWL_IDLE = "__CrawlIdle";
        public const string CLIP_CRAWL_MOVE = "__CrawlMove";
        // Swim
        public const string CLIP_SWIM_IDLE = "__SwimIdle";
        public const string CLIP_SWIM_MOVE = "__SwimMove";
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

        private string currentWeaponTypeId = string.Empty;
        private string playingStateId = string.Empty;
        private PlayingJumpState jumpState = PlayingJumpState.None;
        private PlayingActionState actionState = PlayingActionState.None;
        private bool isPreviouslyGrounded = true;
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
        private readonly StringBuilder stringBuilder = new StringBuilder();
        private readonly HashSet<string> weaponTypeIds = new HashSet<string>();
        private readonly Dictionary<string, BaseStateInfo> states = new Dictionary<string, BaseStateInfo>();

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
        }

        public void Setup(PlayableCharacterModel characterModel)
        {
            CharacterModel = characterModel;
            states.Clear();
            // Setup clips by settings in character model
            // Default
            SetupDefaultAnimations(characterModel.defaultAnimations);
            // Clips based on equipped weapons
            for (int i = 0; i < characterModel.weaponAnimations.Length; ++i)
            {
                SetupWeaponAnimations(characterModel.weaponAnimations[i]);
            }
        }

        private void SetupDefaultAnimations(DefaultAnimations defaultAnimations)
        {
            SetBaseState(stringBuilder.Clear().Append(CLIP_IDLE).ToString(), defaultAnimations.idleState);
            SetMoveStates(string.Empty, CLIP_MOVE, defaultAnimations.moveStates);
            SetMoveStates(string.Empty, CLIP_SPRINT, defaultAnimations.sprintStates);
            SetMoveStates(string.Empty, CLIP_WALK, defaultAnimations.walkStates);
            SetBaseState(stringBuilder.Clear().Append(CLIP_CROUCH_IDLE).ToString(), defaultAnimations.crouchIdleState);
            SetMoveStates(string.Empty, CLIP_CROUCH_MOVE, defaultAnimations.crouchMoveStates);
            SetBaseState(stringBuilder.Clear().Append(CLIP_CRAWL_IDLE).ToString(), defaultAnimations.crawlIdleState);
            SetMoveStates(string.Empty, CLIP_CRAWL_MOVE, defaultAnimations.crawlMoveStates);
            SetBaseState(stringBuilder.Clear().Append(CLIP_SWIM_IDLE).ToString(), defaultAnimations.swimIdleState);
            SetMoveStates(string.Empty, CLIP_SWIM_MOVE, defaultAnimations.swimMoveStates);
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
            SetMoveStates(weaponAnimations.weaponType.Id, CLIP_MOVE, weaponAnimations.moveStates);
            SetMoveStates(weaponAnimations.weaponType.Id, CLIP_SPRINT, weaponAnimations.sprintStates);
            SetMoveStates(weaponAnimations.weaponType.Id, CLIP_WALK, weaponAnimations.walkStates);
            SetBaseState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_CROUCH_IDLE).ToString(), weaponAnimations.crouchIdleState);
            SetMoveStates(weaponAnimations.weaponType.Id, CLIP_CROUCH_MOVE, weaponAnimations.crouchMoveStates);
            SetBaseState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_CRAWL_IDLE).ToString(), weaponAnimations.crawlIdleState);
            SetMoveStates(weaponAnimations.weaponType.Id, CLIP_CRAWL_MOVE, weaponAnimations.crawlMoveStates);
            SetBaseState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_SWIM_IDLE).ToString(), weaponAnimations.swimIdleState);
            SetMoveStates(weaponAnimations.weaponType.Id, CLIP_SWIM_MOVE, weaponAnimations.swimMoveStates);
            SetBaseState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_JUMP).ToString(), weaponAnimations.jumpState);
            SetBaseState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_FALL).ToString(), weaponAnimations.fallState);
            SetBaseState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_LANDED).ToString(), weaponAnimations.landedState);
            SetBaseState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_DEAD).ToString(), weaponAnimations.deadState);
        }

        private void SetMoveStates(string weaponTypeId, string moveType, MoveStates moveStates)
        {
            SetBaseState(stringBuilder.Clear().Append(weaponTypeId).Append(moveType).Append(DIR_FORWARD).ToString(), moveStates.forwardState);
            SetBaseState(stringBuilder.Clear().Append(weaponTypeId).Append(moveType).Append(DIR_BACKWARD).ToString(), moveStates.backwardState);
            SetBaseState(stringBuilder.Clear().Append(weaponTypeId).Append(moveType).Append(DIR_LEFT).ToString(), moveStates.leftState);
            SetBaseState(stringBuilder.Clear().Append(weaponTypeId).Append(moveType).Append(DIR_RIGHT).ToString(), moveStates.rightState);
            SetBaseState(stringBuilder.Clear().Append(weaponTypeId).Append(moveType).Append(DIR_FORWARD).Append(DIR_LEFT).ToString(), moveStates.forwardLeftState);
            SetBaseState(stringBuilder.Clear().Append(weaponTypeId).Append(moveType).Append(DIR_FORWARD).Append(DIR_RIGHT).ToString(), moveStates.forwardRightState);
            SetBaseState(stringBuilder.Clear().Append(weaponTypeId).Append(moveType).Append(DIR_BACKWARD).Append(DIR_LEFT).ToString(), moveStates.backwardLeftState);
            SetBaseState(stringBuilder.Clear().Append(weaponTypeId).Append(moveType).Append(DIR_BACKWARD).Append(DIR_RIGHT).ToString(), moveStates.backwardRightState);
        }

        private void SetBaseState(string id, AnimState state)
        {
            AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(Graph, state.clip);
            int inputPort = BaseLayerMixer.GetInputCount();
            BaseLayerMixer.SetInputCount(inputPort + 1);
            Graph.Connect(clipPlayable, 0, BaseLayerMixer, inputPort);
            states[id] = new BaseStateInfo()
            {
                inputPort = inputPort,
                state = state,
            };
        }

        private string GetPlayingStateId()
        {
            stringBuilder.Clear();
            stringBuilder.Append(currentWeaponTypeId);
            if (CharacterModel.isDead)
            {
                jumpState = PlayingJumpState.None;
                stringBuilder.Append(CLIP_DEAD);
                return stringBuilder.ToString();
            }
            else if (jumpState == PlayingJumpState.Starting)
            {
                jumpState = PlayingJumpState.Playing;
                isPreviouslyGrounded = false;
                stringBuilder.Append(CLIP_JUMP);
                return stringBuilder.ToString();
            }
            else if (!CharacterModel.movementState.HasFlag(MovementState.IsGrounded))
            {
                isPreviouslyGrounded = false;
                stringBuilder.Append(CLIP_FALL);
                return stringBuilder.ToString();
            }
            else if (jumpState == PlayingJumpState.Playing)
            {
                // Don't change state because character is jumping, it will change to idle when jump animation played
                return playingStateId;
            }
            else
            {
                if (!isPreviouslyGrounded)
                {
                    isPreviouslyGrounded = true;
                    stringBuilder.Append(CLIP_LANDED);
                    return stringBuilder.ToString();
                }
                bool movingForward = CharacterModel.movementState.HasFlag(MovementState.Forward);
                bool movingBackward = CharacterModel.movementState.HasFlag(MovementState.Backward);
                bool movingLeft = CharacterModel.movementState.HasFlag(MovementState.Left);
                bool movingRight = CharacterModel.movementState.HasFlag(MovementState.Right);
                bool moving = movingForward || movingBackward || movingLeft || movingRight;
                if (CharacterModel.movementState.HasFlag(MovementState.IsUnderWater))
                {
                    if (moving)
                        stringBuilder.Append(CLIP_SWIM_MOVE);
                    else
                        stringBuilder.Append(CLIP_SWIM_IDLE);
                }
                else
                {
                    switch (CharacterModel.extraMovementState)
                    {
                        case ExtraMovementState.IsSprinting:
                            if (moving)
                                stringBuilder.Append(CLIP_SPRINT);
                            else
                                stringBuilder.Append(CLIP_IDLE);
                            break;
                        case ExtraMovementState.IsWalking:
                            if (moving)
                                stringBuilder.Append(CLIP_WALK);
                            else
                                stringBuilder.Append(CLIP_IDLE);
                            break;
                        case ExtraMovementState.IsCrouching:
                            if (moving)
                                stringBuilder.Append(CLIP_CROUCH_MOVE);
                            else
                                stringBuilder.Append(CLIP_CROUCH_IDLE);
                            break;
                        case ExtraMovementState.IsCrawling:
                            if (moving)
                                stringBuilder.Append(CLIP_CRAWL_MOVE);
                            else
                                stringBuilder.Append(CLIP_CRAWL_IDLE);
                            break;
                        default:
                            if (moving)
                                stringBuilder.Append(CLIP_MOVE);
                            else
                                stringBuilder.Append(CLIP_IDLE);
                            break;
                    }
                }
                if (movingForward)
                    stringBuilder.Append(DIR_FORWARD);
                else if (movingBackward)
                    stringBuilder.Append(DIR_BACKWARD);
                if (movingLeft)
                    stringBuilder.Append(DIR_LEFT);
                else if (movingRight)
                    stringBuilder.Append(DIR_RIGHT);
                return stringBuilder.ToString();
            }
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            #region Update base state
            string playingStateId = GetPlayingStateId();
            if (!this.playingStateId.Equals(playingStateId))
            {
                this.playingStateId = playingStateId;
                baseInputPort = states[playingStateId].inputPort;
                // Set clip info 
                float speed = states[playingStateId].GetSpeed(1);
                // Set transition duration
                baseTransitionDuration = states[playingStateId].state.transitionDuration;
                if (baseTransitionDuration <= 0f)
                    baseTransitionDuration = CharacterModel.transitionDuration;
                baseTransitionDuration *= speed;
                // Set clip length
                Playable clipPlayable = BaseLayerMixer.GetInput(baseInputPort);
                clipPlayable.SetSpeed(speed);
                baseClipLength = states[playingStateId].GetClipLength(1);
                // Reset play elapsed
                basePlayElapsed = 0f;
            }

            // Update transition
            float weightUpdate = info.deltaTime / baseTransitionDuration;
            int inputCount = BaseLayerMixer.GetInputCount();
            for (int i = 0; i < inputCount; ++i)
            {
                float weight = BaseLayerMixer.GetInputWeight(i);
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
            }

            // Update playing state
            basePlayElapsed += info.deltaTime;

            // It will change state to fall in next frame
            if (jumpState == PlayingJumpState.Playing && basePlayElapsed >= baseClipLength)
                jumpState = PlayingJumpState.None;
            #endregion

            #region Update action state
            if (actionState != PlayingActionState.None)
                weightUpdate = info.deltaTime / actionTransitionDuration;

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
            jumpState = PlayingJumpState.Starting;
        }

        public void PlayAction(ActionState actionState, float speedRate)
        {
            // Destroy playing state
            if (ActionLayerMixer.IsValid())
                ActionLayerMixer.Destroy();

            ActionLayerMixer = AnimationMixerPlayable.Create(Graph, 1, true);
            Graph.Connect(ActionLayerMixer, 0, LayerMixer, 1);
            LayerMixer.SetInputWeight(1, 0);

            AnimationClipPlayable playable = AnimationClipPlayable.Create(Graph, actionState.clip);
            Graph.Connect(playable, 0, ActionLayerMixer, 0);

            // Set avatar mask
            AvatarMask avatarMask = actionState.avatarMask;
            if (avatarMask == null)
                avatarMask = CharacterModel.actionAvatarMask;
            LayerMixer.SetLayerMaskFromAvatarMask(1, avatarMask);

            // Set clip info
            float speed = actionState.animSpeedRate * speedRate;
            // Set transition duration
            actionTransitionDuration = actionState.transitionDuration;
            if (actionTransitionDuration <= 0f)
                actionTransitionDuration = CharacterModel.transitionDuration;
            actionTransitionDuration *= speed;
            // Set clip length
            Playable clipPlayable = BaseLayerMixer.GetInput(baseInputPort);
            clipPlayable.SetSpeed(speed);
            actionClipLength = actionState.clip.length * speed;
            // Reset play elapsed
            actionPlayElapsed = 0f;
        }

        public void StopAction()
        {
            if (actionState == PlayingActionState.Playing)
                actionState = PlayingActionState.Stopping;
        }
    }
}