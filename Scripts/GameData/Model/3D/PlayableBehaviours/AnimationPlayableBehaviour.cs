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

        public const int BASE_LAYER = 0;
        public const int LEFT_HAND_WIELDING_LAYER = 1;
        public const int ACTION_LAYER = 2;

        private interface IStateInfo
        {
            public int InputPort { get; set; }
            public float GetSpeed(float rate);
            public float GetClipLength(float rate);
            public AnimationClip GetClip();
            public float GetTransitionDuration();
            public bool IsAdditive();
            public bool ApplyFootIk();
            public bool ApplyPlayableIk();
            public AvatarMask GetAvatarMask();
        }

        private struct BaseStateInfo : IStateInfo
        {
            public int InputPort { get; set; }
            public AnimState State { get; set; }
            public float GetSpeed(float rate)
            {
                return (State.animSpeedRate > 0 ? State.animSpeedRate : 1) * rate;
            }

            public float GetClipLength(float rate)
            {
                return State.clip.length / GetSpeed(rate);
            }

            public AnimationClip GetClip()
            {
                return State.clip;
            }

            public float GetTransitionDuration()
            {
                return State.transitionDuration;
            }

            public bool IsAdditive()
            {
                return State.isAdditive;
            }

            public bool ApplyFootIk()
            {
                return State.applyFootIk;
            }

            public bool ApplyPlayableIk()
            {
                return State.applyPlayableIk;
            }

            public AvatarMask GetAvatarMask()
            {
                return null;
            }
        }

        private struct LeftHandWieldingStateInfo : IStateInfo
        {
            public int InputPort { get; set; }
            public ActionState State { get; set; }
            public float GetSpeed(float rate)
            {
                return (State.animSpeedRate > 0 ? State.animSpeedRate : 1) * rate;
            }

            public float GetClipLength(float rate)
            {
                return State.clip.length / GetSpeed(rate);
            }

            public AnimationClip GetClip()
            {
                return State.clip;
            }

            public float GetTransitionDuration()
            {
                return State.transitionDuration;
            }

            public bool IsAdditive()
            {
                return State.isAdditive;
            }

            public bool ApplyFootIk()
            {
                return State.applyFootIk;
            }

            public bool ApplyPlayableIk()
            {
                return State.applyPlayableIk;
            }

            public AvatarMask GetAvatarMask()
            {
                return State.avatarMask;
            }
        }

        private enum PlayingJumpState
        {
            None,
            Starting,
            Playing,
        }

        private enum PlayingActionState
        {
            None,
            Playing,
            Stopping,
            Looping,
        }

        private class StateUpdateData
        {
            public string playingStateId = string.Empty;
            public int inputPort = 0;
            public float transitionDuration = 0f;
            public float clipLength = 0f;
            public float playElapsed = 0f;
            public float clipSpeed = 0f;

            public bool HasChanges { get; set; } = true;

            private string _weaponTypeId;
            public string WeaponTypeId
            {
                get { return _weaponTypeId; }
                set
                {
                    if (_weaponTypeId == value)
                        return;
                    _weaponTypeId = value;
                    HasChanges = true;
                }
            }

            private bool _isDead;
            public bool IsDead
            {
                get { return _isDead; }
                set
                {
                    if (_isDead == value)
                        return;
                    _isDead = value;
                    HasChanges = true;
                }
            }

            private MovementState _movementState;
            public MovementState MovementState
            {
                get { return _movementState; }
                set
                {
                    if (_movementState == value)
                        return;
                    _movementState = value;
                    HasChanges = true;
                }
            }

            private ExtraMovementState _extraMovementState;
            public ExtraMovementState ExtraMovementState
            {
                get { return _extraMovementState; }
                set
                {
                    if (_extraMovementState == value)
                        return;
                    _extraMovementState = value;
                    HasChanges = true;
                }
            }

            private PlayingJumpState _playingJumpState = PlayingJumpState.None;
            public PlayingJumpState PlayingJumpState
            {
                get { return _playingJumpState; }
                set
                {
                    if (_playingJumpState == value)
                        return;
                    _playingJumpState = value;
                    HasChanges = true;
                }
            }

            private bool _isPreviouslyGrounded = true;
            public bool IsPreviouslyGrounded
            {
                get { return _isPreviouslyGrounded; }
                set
                {
                    if (_isPreviouslyGrounded == value)
                        return;
                    _isPreviouslyGrounded = value;
                    HasChanges = true;
                }
            }

            private bool _playingLandedState = false;
            public bool PlayingLandedState
            {
                get { return _playingLandedState; }
                set
                {
                    if (_playingLandedState == value)
                        return;
                    _playingLandedState = value;
                    HasChanges = true;
                }
            }
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

        public Playable Self { get; private set; }
        public PlayableGraph Graph { get; private set; }
        public AnimationLayerMixerPlayable LayerMixer { get; private set; }
        public AnimationMixerPlayable BaseLayerMixer { get; private set; }
        public AnimationMixerPlayable LeftHandWieldingLayerMixer { get; private set; }
        public AnimationMixerPlayable ActionLayerMixer { get; private set; }
        public PlayableCharacterModel CharacterModel { get; private set; }
        public bool IsFreeze { get; set; }

        private readonly StateUpdateData baseStateUpdateData = new StateUpdateData();
        private readonly StateUpdateData leftHandWieldingStateUpdateData = new StateUpdateData();
        private PlayingActionState playingActionState = PlayingActionState.None;
        private float actionTransitionDuration = 0f;
        private float actionClipLength = 0f;
        private float actionPlayElapsed = 0f;
        private float actionLayerClipSpeed = 0f;
        private readonly StringBuilder stringBuilder = new StringBuilder();
        private readonly HashSet<string> weaponTypeIds = new HashSet<string>();
        private readonly HashSet<string> leftHandWeaponTypeIds = new HashSet<string>();
        private readonly Dictionary<string, BaseStateInfo> baseStates = new Dictionary<string, BaseStateInfo>();
        private readonly Dictionary<string, LeftHandWieldingStateInfo> leftHandWieldingStates = new Dictionary<string, LeftHandWieldingStateInfo>();
        private int baseLayerInputPortCount = 0;
        private int leftHandWieldingLayerInputPortCount = 0;
        private bool readyToPlay = false;
        private float awakenTime;

        public void Setup(PlayableCharacterModel characterModel)
        {
            CharacterModel = characterModel;
            awakenTime = Time.unscaledTime;
            // Setup clips by settings in character model
            // Default
            SetupDefaultAnimations(characterModel.defaultAnimations);
            // Clips based on equipped weapons
            for (int i = 0; i < characterModel.weaponAnimations.Length; ++i)
            {
                SetupWeaponAnimations(characterModel.weaponAnimations[i]);
            }
            // Clips based on equipped weapons in left-hand
            for (int i = 0; i < characterModel.leftHandWieldingWeaponAnimations.Length; ++i)
            {
                SetupLeftHandWieldingWeaponAnimations(characterModel.leftHandWieldingWeaponAnimations[i]);
            }
        }

        public override void OnPlayableCreate(Playable playable)
        {
            Self = playable;
            Self.SetInputCount(1);
            Self.SetInputWeight(0, 1);

            Graph = playable.GetGraph();
            // Create and connect layer mixer to graph
            // 0 - Base state
            // 1 - Left-hand wielding state
            // 2 - Action state
            LayerMixer = AnimationLayerMixerPlayable.Create(Graph, 3);
            Graph.Connect(LayerMixer, 0, Self, 0);

            // Create and connect base layer mixer to layer mixer
            BaseLayerMixer = AnimationMixerPlayable.Create(Graph, 0, true);
            Graph.Connect(BaseLayerMixer, 0, LayerMixer, BASE_LAYER);
            LayerMixer.SetInputWeight(BASE_LAYER, 1);

            // Create and connect left-hand wielding layer mixer to layer mixer
            LeftHandWieldingLayerMixer = AnimationMixerPlayable.Create(Graph, 0, true);
            Graph.Connect(LeftHandWieldingLayerMixer, 0, LayerMixer, LEFT_HAND_WIELDING_LAYER);
            LayerMixer.SetInputWeight(LEFT_HAND_WIELDING_LAYER, 0);

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

        private void SetupLeftHandWieldingWeaponAnimations(WieldWeaponAnimations weaponAnimations)
        {
            if (weaponAnimations.weaponType == null)
                return;
            leftHandWeaponTypeIds.Add(weaponAnimations.weaponType.Id);
            SetLeftHandWieldingState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_IDLE).ToString(), weaponAnimations.idleState);
            SetLeftHandWieldingMoveStates(weaponAnimations.weaponType.Id, string.Empty, weaponAnimations.moveStates);
            SetLeftHandWieldingMoveStates(weaponAnimations.weaponType.Id, MOVE_TYPE_SPRINT, weaponAnimations.sprintStates);
            SetLeftHandWieldingMoveStates(weaponAnimations.weaponType.Id, MOVE_TYPE_WALK, weaponAnimations.walkStates);
            SetLeftHandWieldingState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_CROUCH_IDLE).ToString(), weaponAnimations.crouchIdleState);
            SetLeftHandWieldingMoveStates(weaponAnimations.weaponType.Id, MOVE_TYPE_CROUCH, weaponAnimations.crouchMoveStates);
            SetLeftHandWieldingState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_CRAWL_IDLE).ToString(), weaponAnimations.crawlIdleState);
            SetLeftHandWieldingMoveStates(weaponAnimations.weaponType.Id, MOVE_TYPE_CRAWL, weaponAnimations.crawlMoveStates);
            SetLeftHandWieldingState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_SWIM_IDLE).ToString(), weaponAnimations.swimIdleState);
            SetLeftHandWieldingMoveStates(weaponAnimations.weaponType.Id, MOVE_TYPE_SWIM, weaponAnimations.swimMoveStates);
            SetLeftHandWieldingState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_JUMP).ToString(), weaponAnimations.jumpState);
            SetLeftHandWieldingState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_FALL).ToString(), weaponAnimations.fallState);
            SetLeftHandWieldingState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_LANDED).ToString(), weaponAnimations.landedState);
            SetLeftHandWieldingState(stringBuilder.Clear().Append(weaponAnimations.weaponType.Id).Append(CLIP_DEAD).ToString(), weaponAnimations.deadState);
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
                if (id.Equals(CLIP_IDLE))
                {
                    // Idle clip is empty, use `EmptyClip`
                    state.clip = EmptyClip;
                }
                return;
            }
            baseStates[id] = new BaseStateInfo()
            {
                InputPort = baseLayerInputPortCount++,
                State = state,
            };
        }

        private void SetLeftHandWieldingMoveStates(string weaponTypeId, string moveType, WieldMoveStates moveStates)
        {
            SetLeftHandWieldingState(stringBuilder.Clear().Append(weaponTypeId).Append(DIR_FORWARD).Append(moveType).ToString(), moveStates.forwardState);
            SetLeftHandWieldingState(stringBuilder.Clear().Append(weaponTypeId).Append(DIR_BACKWARD).Append(moveType).ToString(), moveStates.backwardState);
            SetLeftHandWieldingState(stringBuilder.Clear().Append(weaponTypeId).Append(DIR_LEFT).Append(moveType).ToString(), moveStates.leftState);
            SetLeftHandWieldingState(stringBuilder.Clear().Append(weaponTypeId).Append(DIR_RIGHT).Append(moveType).ToString(), moveStates.rightState);
            SetLeftHandWieldingState(stringBuilder.Clear().Append(weaponTypeId).Append(DIR_FORWARD).Append(DIR_LEFT).Append(moveType).ToString(), moveStates.forwardLeftState);
            SetLeftHandWieldingState(stringBuilder.Clear().Append(weaponTypeId).Append(DIR_FORWARD).Append(DIR_RIGHT).Append(moveType).ToString(), moveStates.forwardRightState);
            SetLeftHandWieldingState(stringBuilder.Clear().Append(weaponTypeId).Append(DIR_BACKWARD).Append(DIR_LEFT).Append(moveType).ToString(), moveStates.backwardLeftState);
            SetLeftHandWieldingState(stringBuilder.Clear().Append(weaponTypeId).Append(DIR_BACKWARD).Append(DIR_RIGHT).Append(moveType).ToString(), moveStates.backwardRightState);
        }

        private void SetLeftHandWieldingState(string id, ActionState state)
        {
            if (state.clip == null)
                return;
            leftHandWieldingStates[id] = new LeftHandWieldingStateInfo()
            {
                InputPort = leftHandWieldingLayerInputPortCount++,
                State = state,
            };
        }

        private string GetPlayingStateId<T>(string weaponTypeId, Dictionary<string, T> stateInfos, StateUpdateData stateUpdateData) where T : IStateInfo
        {
            stateUpdateData.IsDead = CharacterModel.isDead;
            stateUpdateData.MovementState = CharacterModel.movementState;
            stateUpdateData.ExtraMovementState = CharacterModel.extraMovementState;

            if (!stateUpdateData.HasChanges)
                return stateUpdateData.playingStateId;

            if (stateUpdateData.IsDead)
            {
                stateUpdateData.PlayingJumpState = PlayingJumpState.None;
                // Get dead state by weapon type
                string stateId = stringBuilder.Clear().Append(weaponTypeId).Append(CLIP_DEAD).ToString();
                // State not found, use dead state from default animations
                if (!stateInfos.ContainsKey(stateId))
                    stateId = CLIP_DEAD;
                return stateId;
            }
            else if (stateUpdateData.PlayingJumpState == PlayingJumpState.Starting)
            {
                stateUpdateData.PlayingJumpState = PlayingJumpState.Playing;
                stateUpdateData.IsPreviouslyGrounded = false;
                // Get jump state by weapon type
                string stateId = stringBuilder.Clear().Append(weaponTypeId).Append(CLIP_JUMP).ToString();
                // State not found, use jump state from default animations
                if (!stateInfos.ContainsKey(stateId))
                    stateId = CLIP_JUMP;
                return stateId;
            }
            else if (stateUpdateData.MovementState.Has(MovementState.IsUnderWater) || CharacterModel.movementState.Has(MovementState.IsGrounded))
            {
                if (stateUpdateData.PlayingLandedState || stateUpdateData.PlayingJumpState == PlayingJumpState.Playing)
                {
                    // Don't change state because character is just landed, landed animation has to be played before change to move state
                    return stateUpdateData.playingStateId;
                }
                if (stateUpdateData.MovementState.Has(MovementState.IsGrounded) && !stateUpdateData.IsPreviouslyGrounded)
                {
                    stateUpdateData.IsPreviouslyGrounded = true;
                    // Get landed state by weapon type
                    string stateId = stringBuilder.Clear().Append(weaponTypeId).Append(CLIP_LANDED).ToString();
                    // State not found, use landed state from default animations
                    if (!stateInfos.ContainsKey(stateId))
                        stateId = CLIP_LANDED;
                    // State found, use this state Id. If it not, use move state
                    if (stateInfos.ContainsKey(stateId))
                    {
                        stateUpdateData.PlayingLandedState = true;
                        return stateId;
                    }
                }
                // Get movement state
                stringBuilder.Clear();
                bool movingForward = stateUpdateData.MovementState.Has(MovementState.Forward);
                bool movingBackward = stateUpdateData.MovementState.Has(MovementState.Backward);
                bool movingLeft = stateUpdateData.MovementState.Has(MovementState.Left);
                bool movingRight = stateUpdateData.MovementState.Has(MovementState.Right);
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
                string stateWithoutWeaponIdAndMoveType = stringBuilder.ToString();
                if (stateUpdateData.MovementState.Has(MovementState.IsUnderWater))
                {
                    if (!moving)
                        stringBuilder.Append(CLIP_SWIM_IDLE);
                    else
                        stringBuilder.Append(MOVE_TYPE_SWIM);
                }
                else
                {
                    switch (stateUpdateData.ExtraMovementState)
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
                // This is state ID without current weapon type ID
                string stateWithoutWeaponTypeId = stringBuilder.ToString();
                string stateWithWeaponTypeId = stringBuilder.Clear().Append(weaponTypeId).Append(stateWithoutWeaponTypeId).ToString();
                // State with weapon type found, use it
                if (stateInfos.ContainsKey(stateWithWeaponTypeId))
                    return stateWithWeaponTypeId;
                // State with weapon type not found, try use state without weapon type
                if (stateInfos.ContainsKey(stateWithoutWeaponTypeId))
                    return stateWithoutWeaponTypeId;
                // State with weapon type and state without weapon type not found, try use state with weapon type but without move type
                stateWithWeaponTypeId = stringBuilder.Clear().Append(weaponTypeId).Append(stateWithoutWeaponIdAndMoveType).ToString();
                if (stateInfos.ContainsKey(stateWithWeaponTypeId))
                    return stateWithWeaponTypeId;
                // State still not found, use state without weapon type and move type
                return stateWithoutWeaponIdAndMoveType;
            }
            else if (stateUpdateData.PlayingJumpState == PlayingJumpState.Playing)
            {
                // Don't change state because character is jumping, it will change to fall when jump animation played
                return stateUpdateData.playingStateId;
            }
            else
            {
                stateUpdateData.IsPreviouslyGrounded = false;
                // Get fall state by weapon type
                string stateId = stringBuilder.Clear().Append(weaponTypeId).Append(CLIP_FALL).ToString();
                // State not found, use fall state from default animations
                if (!stateInfos.ContainsKey(stateId))
                    stateId = CLIP_FALL;
                return stateId;
            }
        }

        private void PrepareForNewState<T>(AnimationMixerPlayable mixer, uint layer, Dictionary<string, T> stateInfos, StateUpdateData stateUpdateData) where T : IStateInfo
        {
            // No animation states?
            if (stateInfos.Count == 0)
                return;

            // Change state only when previous animation weight >= 1f
            if (mixer.GetInputCount() > 0 && mixer.GetInputWeight(stateUpdateData.inputPort) < 1f)
                return;

            string playingStateId = GetPlayingStateId(stateUpdateData.WeaponTypeId, stateInfos, stateUpdateData);
            // State not found, use idle state (with weapon type)
            if (!stateInfos.ContainsKey(playingStateId))
                playingStateId = stringBuilder.Clear().Append(stateUpdateData.WeaponTypeId).Append(CLIP_IDLE).ToString();
            // State still not found, use idle state from default states (without weapon type)
            if (!stateInfos.ContainsKey(playingStateId))
                playingStateId = CLIP_IDLE;
            // State not found, no idle state? don't play new animation
            if (!stateInfos.ContainsKey(playingStateId))
            {
                // Reset play elapsed
                stateUpdateData.playElapsed = 0f;
                return;
            }

            if (!stateUpdateData.playingStateId.Equals(playingStateId))
            {
                stateUpdateData.playingStateId = playingStateId;

                // Play new state
                int inputCount = mixer.GetInputCount() + 1;
                mixer.SetInputCount(inputCount);
                AnimationClipPlayable playable = AnimationClipPlayable.Create(Graph, stateInfos[playingStateId].GetClip());
                playable.SetApplyFootIK(stateInfos[playingStateId].ApplyFootIk());
                playable.SetApplyPlayableIK(stateInfos[playingStateId].ApplyPlayableIk());
                Graph.Connect(playable, 0, mixer, inputCount - 1);
                if (inputCount > 1)
                {
                    // Set weight to 0 for transition
                    mixer.SetInputWeight(inputCount - 1, 0f);
                }
                else
                {
                    // Set weight to 1 for immediately playing
                    mixer.SetInputWeight(inputCount - 1, 1f);
                }

                // Get input port from new playing state ID
                stateUpdateData.inputPort = inputCount - 1;

                // Set avatar mask
                AvatarMask avatarMask = stateInfos[playingStateId].GetAvatarMask();
                if (avatarMask == null)
                    avatarMask = EmptyMask;
                LayerMixer.SetLayerMaskFromAvatarMask(layer, avatarMask);

                // Set clip info 
                stateUpdateData.clipSpeed = stateInfos[playingStateId].GetSpeed(1);
                // Set transition duration
                stateUpdateData.transitionDuration = stateInfos[playingStateId].GetTransitionDuration();
                if (stateUpdateData.transitionDuration <= 0f)
                    stateUpdateData.transitionDuration = CharacterModel.transitionDuration;
                stateUpdateData.transitionDuration /= stateUpdateData.clipSpeed;
                mixer.GetInput(stateUpdateData.inputPort).Play();
                stateUpdateData.clipLength = stateInfos[playingStateId].GetClipLength(1);

                // Set layer additive
                LayerMixer.SetLayerAdditive(layer, stateInfos[playingStateId].IsAdditive());

                // Reset play elapsed
                stateUpdateData.playElapsed = 0f;
            }
        }

        private void UpdateState(AnimationMixerPlayable mixer, StateUpdateData stateUpdateData, float deltaTime)
        {
            int inputCount = mixer.GetInputCount();
            if (inputCount == 0)
                return;

            mixer.GetInput(stateUpdateData.inputPort).SetSpeed(IsFreeze ? 0 : stateUpdateData.clipSpeed);

            float weight;
            float weightUpdate;
            bool transitionEnded = false;
            if (CharacterModel.isDead && Time.unscaledTime - awakenTime < 1f)
            {
                // Play dead animation at end frame immediately
                mixer.GetInput(stateUpdateData.inputPort).SetTime(baseStates[stateUpdateData.playingStateId].State.clip.length);
                for (int i = 0; i < inputCount; ++i)
                {
                    if (i != stateUpdateData.inputPort)
                    {
                        mixer.SetInputWeight(i, 0f);
                    }
                    else
                    {
                        mixer.SetInputWeight(i, 1f);
                        transitionEnded = true;
                    }
                }
            }
            else
            {
                // Update transition
                weightUpdate = deltaTime / stateUpdateData.transitionDuration;
                for (int i = 0; i < inputCount; ++i)
                {
                    weight = mixer.GetInputWeight(i);
                    if (i != stateUpdateData.inputPort)
                    {
                        weight -= weightUpdate;
                        if (weight < 0f)
                            weight = 0f;
                    }
                    else
                    {
                        weight += weightUpdate;
                        if (weight > 1f)
                        {
                            weight = 1f;
                            transitionEnded = true;
                        }
                    }
                    mixer.SetInputWeight(i, weight);
                }

                // Update playing state
                stateUpdateData.playElapsed += deltaTime;

                // It will change state to fall in next frame
                if (stateUpdateData.PlayingJumpState == PlayingJumpState.Playing && stateUpdateData.playElapsed >= stateUpdateData.clipLength)
                    stateUpdateData.PlayingJumpState = PlayingJumpState.None;

                // It will change state to movement in next frame
                if (stateUpdateData.PlayingLandedState && stateUpdateData.playElapsed >= stateUpdateData.clipLength)
                    stateUpdateData.PlayingLandedState = false;
            }

            if (inputCount > 1 && transitionEnded)
            {
                // Disconnect and destroy all input except the last one
                Playable tempPlayable;
                for (int i = 0; i < inputCount - 1; ++i)
                {
                    tempPlayable = mixer.GetInput(i);
                    Graph.Disconnect(mixer, i);
                    if (tempPlayable.IsValid())
                        tempPlayable.Destroy();
                }
                // Get last input connect to mixer at index-0
                tempPlayable = mixer.GetInput(inputCount - 1);
                Graph.Disconnect(mixer, inputCount - 1);
                Graph.Connect(tempPlayable, 0, mixer, 0);
                mixer.SetInputCount(1);
                stateUpdateData.inputPort = 0;
            }
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (!readyToPlay)
                return;

            #region Update base state and left-hand wielding
            if (!IsFreeze)
            {
                PrepareForNewState(BaseLayerMixer, BASE_LAYER, baseStates, baseStateUpdateData);
                PrepareForNewState(LeftHandWieldingLayerMixer, LEFT_HAND_WIELDING_LAYER, leftHandWieldingStates, leftHandWieldingStateUpdateData);
            }

            UpdateState(BaseLayerMixer, baseStateUpdateData, info.deltaTime);
            UpdateState(LeftHandWieldingLayerMixer, leftHandWieldingStateUpdateData, info.deltaTime);
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
            float weightUpdate = info.deltaTime / actionTransitionDuration;
            float weight = LayerMixer.GetInputWeight(ACTION_LAYER);
            switch (playingActionState)
            {
                case PlayingActionState.Playing:
                case PlayingActionState.Looping:
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
            LayerMixer.SetInputWeight(ACTION_LAYER, weight);

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

        public void SetPlayingWeaponTypeId(IWeaponItem rightHand, IWeaponItem leftHand)
        {
            baseStateUpdateData.WeaponTypeId = string.Empty;
            if (rightHand != null && weaponTypeIds.Contains(rightHand.WeaponType.Id))
                baseStateUpdateData.WeaponTypeId = rightHand.WeaponType.Id;

            leftHandWieldingStateUpdateData.WeaponTypeId = string.Empty;
            if (leftHand != null && leftHandWeaponTypeIds.Contains(leftHand.WeaponType.Id))
                leftHandWieldingStateUpdateData.WeaponTypeId = leftHand.WeaponType.Id;
        }

        public void PlayJump()
        {
            baseStateUpdateData.PlayingJumpState = PlayingJumpState.Starting;
            leftHandWieldingStateUpdateData.PlayingJumpState = PlayingJumpState.Starting;
        }

        public void PlayAction(ActionState actionState, float speedRate, float duration = 0f, bool loop = false)
        {
            if (IsFreeze || CharacterModel.isDead)
                return;

            // Destroy playing state
            if (ActionLayerMixer.IsValid())
                ActionLayerMixer.Destroy();

            ActionLayerMixer = AnimationMixerPlayable.Create(Graph, 1, true);
            Graph.Connect(ActionLayerMixer, 0, LayerMixer, ACTION_LAYER);
            LayerMixer.SetInputWeight(ACTION_LAYER, 0f);

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
            LayerMixer.SetLayerMaskFromAvatarMask(ACTION_LAYER, avatarMask);

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
            LayerMixer.SetLayerAdditive(ACTION_LAYER, actionState.isAdditive);
            // Reset play elapsed
            actionPlayElapsed = 0f;

            if (loop)
                playingActionState = PlayingActionState.Looping;
            else
                playingActionState = PlayingActionState.Playing;
        }

        public void StopAction()
        {
            if (playingActionState == PlayingActionState.Playing ||
                playingActionState == PlayingActionState.Looping)
                playingActionState = PlayingActionState.Stopping;
        }
    }
}
