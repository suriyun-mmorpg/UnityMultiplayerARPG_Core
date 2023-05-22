using System.Collections.Generic;
using Cysharp.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MultiplayerARPG.GameData.Model.Playables
{
    /// <summary>
    /// NOTE: Set its name to default playable behaviour, in the future I might make it able to customize character model's playable behaviour
    /// </summary>
    public partial class AnimationPlayableBehaviour : PlayableBehaviour
    {
        public static readonly AnimationClip EmptyClip = new AnimationClip();
        public static readonly AvatarMask EmptyMask = new AvatarMask();

        public const int BASE_LAYER = 0;
        public const int LEFT_HAND_WIELDING_LAYER = 1;
        public const int ACTION_LAYER = 2;
        public const string SHILED_WEAPON_TYPE_ID = "<SHIELD>";

        private interface IStateInfo
        {
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
            public float playElapsed = 0f;
            public float clipSpeed = 0f;
            public float clipLength = 0f;
            public bool isMoving = false;
            public AnimationClip previousClip;

            public bool HasChanges { get; set; } = true;
            public bool ForcePlay { get; set; } = false;

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

        private class CacheData
        {
            internal readonly HashSet<string> WeaponTypeIds = new HashSet<string>();
            internal readonly HashSet<string> LeftHandWeaponTypeIds = new HashSet<string>();
            internal readonly Dictionary<string, BaseStateInfo> BaseStates = new Dictionary<string, BaseStateInfo>();
            internal readonly Dictionary<string, LeftHandWieldingStateInfo> LeftHandWieldingStates = new Dictionary<string, LeftHandWieldingStateInfo>();

            internal CacheData(PlayableCharacterModel characterModel)
            {
                // Setup clips by settings in character model
                // Default
                SetupDefaultAnimations(characterModel.defaultAnimations);
                int i;
                // Clips based on equipped weapons
                for (i = 0; i < characterModel.weaponAnimations.Length; ++i)
                {
                    SetupWeaponAnimations(characterModel.weaponAnimations[i]);
                }
                // Clips based on equipped weapons in left-hand
                for (i = 0; i < characterModel.leftHandWeaponAnimations.Length; ++i)
                {
                    SetupLeftHandWieldingWeaponAnimations(characterModel.leftHandWeaponAnimations[i]);
                }
                // Clips based on equipped shield in left-hand
                SetupLeftHandWieldingWeaponAnimations(characterModel.leftHandShieldAnimations, SHILED_WEAPON_TYPE_ID);
                // Setup from weapon data
                List<WeaponType> weaponTypes = new List<WeaponType>(GameInstance.WeaponTypes.Values);
                for (i = 0; i < weaponTypes.Count; ++i)
                {
                    if (weaponTypes[i].PlayableCharacterModelSettings.applyWeaponAnimations)
                    {
                        WeaponAnimations weaponAnimations = weaponTypes[i].PlayableCharacterModelSettings.weaponAnimations;
                        weaponAnimations.weaponType = weaponTypes[i];
                        SetupWeaponAnimations(weaponAnimations);
                    }
                    if (weaponTypes[i].PlayableCharacterModelSettings.applyLeftHandWeaponAnimations)
                    {
                        WieldWeaponAnimations weaponAnimations = weaponTypes[i].PlayableCharacterModelSettings.leftHandWeaponAnimations;
                        weaponAnimations.weaponType = weaponTypes[i];
                        SetupLeftHandWieldingWeaponAnimations(weaponAnimations);
                    }
                }
            }

            private void SetupDefaultAnimations(DefaultAnimations defaultAnimations)
            {
                SetBaseState(CLIP_IDLE, defaultAnimations.idleState);
                SetMoveStates(string.Empty, string.Empty, defaultAnimations.moveStates);
                SetMoveStates(string.Empty, MOVE_TYPE_SPRINT, defaultAnimations.sprintStates);
                SetMoveStates(string.Empty, MOVE_TYPE_WALK, defaultAnimations.walkStates);
                SetBaseState(CLIP_CROUCH_IDLE, defaultAnimations.crouchIdleState);
                SetMoveStates(string.Empty, MOVE_TYPE_CROUCH, defaultAnimations.crouchMoveStates);
                SetBaseState(CLIP_CRAWL_IDLE, defaultAnimations.crawlIdleState);
                SetMoveStates(string.Empty, MOVE_TYPE_CRAWL, defaultAnimations.crawlMoveStates);
                SetBaseState(CLIP_SWIM_IDLE, defaultAnimations.swimIdleState);
                SetMoveStates(string.Empty, MOVE_TYPE_SWIM, defaultAnimations.swimMoveStates);
                SetBaseState(CLIP_JUMP, defaultAnimations.jumpState);
                SetBaseState(CLIP_FALL, defaultAnimations.fallState);
                SetBaseState(CLIP_LANDED, defaultAnimations.landedState);
                SetBaseState(CLIP_DEAD, defaultAnimations.deadState);
            }

            private void SetupWeaponAnimations(WeaponAnimations weaponAnimations, string overrideWeaponTypeId = "")
            {
                bool emptyOverrideId = string.IsNullOrEmpty(overrideWeaponTypeId);
                if (emptyOverrideId && weaponAnimations.weaponType == null)
                    return;
                string weaponTypeId = emptyOverrideId ? weaponAnimations.weaponType.Id : overrideWeaponTypeId;
                if (WeaponTypeIds.Contains(weaponTypeId))
                    return;
                WeaponTypeIds.Add(weaponTypeId);
                SetBaseState(ZString.Concat(weaponTypeId, CLIP_IDLE), weaponAnimations.idleState);
                SetMoveStates(weaponTypeId, string.Empty, weaponAnimations.moveStates);
                SetMoveStates(weaponTypeId, MOVE_TYPE_SPRINT, weaponAnimations.sprintStates);
                SetMoveStates(weaponTypeId, MOVE_TYPE_WALK, weaponAnimations.walkStates);
                SetBaseState(ZString.Concat(weaponTypeId, CLIP_CROUCH_IDLE), weaponAnimations.crouchIdleState);
                SetMoveStates(weaponTypeId, MOVE_TYPE_CROUCH, weaponAnimations.crouchMoveStates);
                SetBaseState(ZString.Concat(weaponTypeId, CLIP_CRAWL_IDLE), weaponAnimations.crawlIdleState);
                SetMoveStates(weaponTypeId, MOVE_TYPE_CRAWL, weaponAnimations.crawlMoveStates);
                SetBaseState(ZString.Concat(weaponTypeId, CLIP_SWIM_IDLE), weaponAnimations.swimIdleState);
                SetMoveStates(weaponTypeId, MOVE_TYPE_SWIM, weaponAnimations.swimMoveStates);
                SetBaseState(ZString.Concat(weaponTypeId, CLIP_JUMP), weaponAnimations.jumpState);
                SetBaseState(ZString.Concat(weaponTypeId, CLIP_FALL), weaponAnimations.fallState);
                SetBaseState(ZString.Concat(weaponTypeId, CLIP_LANDED), weaponAnimations.landedState);
                SetBaseState(ZString.Concat(weaponTypeId, CLIP_DEAD), weaponAnimations.deadState);
            }

            private void SetupLeftHandWieldingWeaponAnimations(WieldWeaponAnimations weaponAnimations, string overrideWeaponTypeId = "")
            {
                bool emptyOverrideId = string.IsNullOrEmpty(overrideWeaponTypeId);
                if (emptyOverrideId && weaponAnimations.weaponType == null)
                    return;
                string weaponTypeId = emptyOverrideId ? weaponAnimations.weaponType.Id : overrideWeaponTypeId;
                if (LeftHandWeaponTypeIds.Contains(weaponTypeId))
                    return;
                LeftHandWeaponTypeIds.Add(weaponTypeId);
                SetLeftHandWieldingState(ZString.Concat(weaponTypeId, CLIP_IDLE), weaponAnimations.idleState);
                SetLeftHandWieldingMoveStates(weaponTypeId, string.Empty, weaponAnimations.moveStates);
                SetLeftHandWieldingMoveStates(weaponTypeId, MOVE_TYPE_SPRINT, weaponAnimations.sprintStates);
                SetLeftHandWieldingMoveStates(weaponTypeId, MOVE_TYPE_WALK, weaponAnimations.walkStates);
                SetLeftHandWieldingState(ZString.Concat(weaponTypeId, CLIP_CROUCH_IDLE), weaponAnimations.crouchIdleState);
                SetLeftHandWieldingMoveStates(weaponTypeId, MOVE_TYPE_CROUCH, weaponAnimations.crouchMoveStates);
                SetLeftHandWieldingState(ZString.Concat(weaponTypeId, CLIP_CRAWL_IDLE), weaponAnimations.crawlIdleState);
                SetLeftHandWieldingMoveStates(weaponTypeId, MOVE_TYPE_CRAWL, weaponAnimations.crawlMoveStates);
                SetLeftHandWieldingState(ZString.Concat(weaponTypeId, CLIP_SWIM_IDLE), weaponAnimations.swimIdleState);
                SetLeftHandWieldingMoveStates(weaponTypeId, MOVE_TYPE_SWIM, weaponAnimations.swimMoveStates);
                SetLeftHandWieldingState(ZString.Concat(weaponTypeId, CLIP_JUMP), weaponAnimations.jumpState);
                SetLeftHandWieldingState(ZString.Concat(weaponTypeId, CLIP_FALL), weaponAnimations.fallState);
                SetLeftHandWieldingState(ZString.Concat(weaponTypeId, CLIP_LANDED), weaponAnimations.landedState);
                SetLeftHandWieldingState(ZString.Concat(weaponTypeId, CLIP_DEAD), weaponAnimations.deadState);
            }

            private void SetMoveStates(string weaponTypeId, string moveType, MoveStates moveStates)
            {
                SetBaseState(ZString.Concat(weaponTypeId, DIR_FORWARD, moveType), moveStates.forwardState);
                SetBaseState(ZString.Concat(weaponTypeId, DIR_BACKWARD, moveType), moveStates.backwardState);
                SetBaseState(ZString.Concat(weaponTypeId, DIR_LEFT, moveType), moveStates.leftState);
                SetBaseState(ZString.Concat(weaponTypeId, DIR_RIGHT, moveType), moveStates.rightState);
                SetBaseState(ZString.Concat(weaponTypeId, DIR_FORWARD, DIR_LEFT, moveType), moveStates.forwardLeftState);
                SetBaseState(ZString.Concat(weaponTypeId, DIR_FORWARD, DIR_RIGHT, moveType), moveStates.forwardRightState);
                SetBaseState(ZString.Concat(weaponTypeId, DIR_BACKWARD, DIR_LEFT, moveType), moveStates.backwardLeftState);
                SetBaseState(ZString.Concat(weaponTypeId, DIR_BACKWARD, DIR_RIGHT, moveType), moveStates.backwardRightState);
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
                BaseStates[id] = new BaseStateInfo()
                {
                    State = state,
                };
            }

            private void SetLeftHandWieldingMoveStates(string weaponTypeId, string moveType, WieldMoveStates moveStates)
            {
                SetLeftHandWieldingState(ZString.Concat(weaponTypeId, DIR_FORWARD, moveType), moveStates.forwardState);
                SetLeftHandWieldingState(ZString.Concat(weaponTypeId, DIR_BACKWARD, moveType), moveStates.backwardState);
                SetLeftHandWieldingState(ZString.Concat(weaponTypeId, DIR_LEFT, moveType), moveStates.leftState);
                SetLeftHandWieldingState(ZString.Concat(weaponTypeId, DIR_RIGHT, moveType), moveStates.rightState);
                SetLeftHandWieldingState(ZString.Concat(weaponTypeId, DIR_FORWARD, DIR_LEFT, moveType), moveStates.forwardLeftState);
                SetLeftHandWieldingState(ZString.Concat(weaponTypeId, DIR_FORWARD, DIR_RIGHT, moveType), moveStates.forwardRightState);
                SetLeftHandWieldingState(ZString.Concat(weaponTypeId, DIR_BACKWARD, DIR_LEFT, moveType), moveStates.backwardLeftState);
                SetLeftHandWieldingState(ZString.Concat(weaponTypeId, DIR_BACKWARD, DIR_RIGHT, moveType), moveStates.backwardRightState);
            }

            private void SetLeftHandWieldingState(string id, ActionState state)
            {
                if (state.clip == null)
                    return;
                LeftHandWieldingStates[id] = new LeftHandWieldingStateInfo()
                {
                    State = state,
                };
            }
        }
        private static readonly Dictionary<int, CacheData> Caches = new Dictionary<int, CacheData>();
        private CacheData Cache
        {
            get { return Caches[CharacterModel.Id]; }
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
        private float moveAnimationSpeedMultiplier = 1f;
        private bool readyToPlay = false;

        public void Setup(PlayableCharacterModel characterModel)
        {
            CharacterModel = characterModel;
            if (!Caches.ContainsKey(characterModel.Id))
                Caches[characterModel.Id] = new CacheData(characterModel);
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
            BaseLayerMixer = AnimationMixerPlayable.Create(Graph, 0);
            Graph.Connect(BaseLayerMixer, 0, LayerMixer, BASE_LAYER);
            LayerMixer.SetInputWeight(BASE_LAYER, 1);

            // Create and connect left-hand wielding layer mixer to layer mixer
            LeftHandWieldingLayerMixer = AnimationMixerPlayable.Create(Graph, 0);
            Graph.Connect(LeftHandWieldingLayerMixer, 0, LayerMixer, LEFT_HAND_WIELDING_LAYER);
            LayerMixer.SetInputWeight(LEFT_HAND_WIELDING_LAYER, 0);

            readyToPlay = true;
        }

        private string GetPlayingStateId<T>(string weaponTypeId, Dictionary<string, T> stateInfos, StateUpdateData stateUpdateData) where T : IStateInfo
        {
            stateUpdateData.IsDead = CharacterModel.IsDead;
            stateUpdateData.MovementState = CharacterModel.MovementState;
            stateUpdateData.ExtraMovementState = CharacterModel.ExtraMovementState;

            if (!stateUpdateData.HasChanges)
                return stateUpdateData.playingStateId;

            if (stateUpdateData.IsDead)
            {
                stateUpdateData.PlayingJumpState = PlayingJumpState.None;
                // Get dead state by weapon type
                string stateId = ZString.Concat(weaponTypeId, CLIP_DEAD);
                // State not found, use dead state from default animations
                if (!stateInfos.ContainsKey(stateId))
                    stateId = CLIP_DEAD;
                return stateId;
            }
            else if (stateUpdateData.PlayingJumpState == PlayingJumpState.Starting)
            {
                stateUpdateData.PlayingJumpState = PlayingJumpState.Playing;
                stateUpdateData.IsPreviouslyGrounded = false;
                stateUpdateData.ForcePlay = true;
                // Get jump state by weapon type
                string stateId = ZString.Concat(weaponTypeId, CLIP_JUMP);
                // State not found, use jump state from default animations
                if (!stateInfos.ContainsKey(stateId))
                    stateId = CLIP_JUMP;
                return stateId;
            }
            else if (stateUpdateData.MovementState.Has(MovementState.IsUnderWater) || stateUpdateData.MovementState.Has(MovementState.IsGrounded))
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
                    string stateId = ZString.Concat(weaponTypeId, CLIP_LANDED);
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
                Utf16ValueStringBuilder stringBuilder = ZString.CreateStringBuilder(true);
                bool movingForward = stateUpdateData.MovementState.Has(MovementState.Forward);
                bool movingBackward = stateUpdateData.MovementState.Has(MovementState.Backward);
                bool movingLeft = stateUpdateData.MovementState.Has(MovementState.Left);
                bool movingRight = stateUpdateData.MovementState.Has(MovementState.Right);
                stateUpdateData.isMoving = (movingForward || movingBackward || movingLeft || movingRight) && moveAnimationSpeedMultiplier > 0f;
                if (stateUpdateData.isMoving)
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
                    if (!stateUpdateData.isMoving)
                        stringBuilder.Append(CLIP_SWIM_IDLE);
                    else
                        stringBuilder.Append(MOVE_TYPE_SWIM);
                }
                else
                {
                    switch (stateUpdateData.ExtraMovementState)
                    {
                        case ExtraMovementState.IsSprinting:
                            if (!stateUpdateData.isMoving)
                                stringBuilder.Append(CLIP_IDLE);
                            else
                                stringBuilder.Append(MOVE_TYPE_SPRINT);
                            break;
                        case ExtraMovementState.IsWalking:
                            if (!stateUpdateData.isMoving)
                                stringBuilder.Append(CLIP_IDLE);
                            else
                                stringBuilder.Append(MOVE_TYPE_WALK);
                            break;
                        case ExtraMovementState.IsCrouching:
                            if (!stateUpdateData.isMoving)
                                stringBuilder.Append(CLIP_CROUCH_IDLE);
                            else
                                stringBuilder.Append(MOVE_TYPE_CROUCH);
                            break;
                        case ExtraMovementState.IsCrawling:
                            if (!stateUpdateData.isMoving)
                                stringBuilder.Append(CLIP_CRAWL_IDLE);
                            else
                                stringBuilder.Append(MOVE_TYPE_CRAWL);
                            break;
                        default:
                            if (!stateUpdateData.isMoving)
                                stringBuilder.Append(CLIP_IDLE);
                            break;
                    }
                }
                // This is state ID without current weapon type ID
                string stateWithoutWeaponTypeId = stringBuilder.ToString();
                stringBuilder.Dispose();
                string stateWithWeaponTypeId = ZString.Concat(weaponTypeId, stateWithoutWeaponTypeId);
                // State with weapon type found, use it
                if (stateInfos.ContainsKey(stateWithWeaponTypeId))
                    return stateWithWeaponTypeId;
                // State with weapon type not found, try use state without weapon type
                if (stateInfos.ContainsKey(stateWithoutWeaponTypeId))
                    return stateWithoutWeaponTypeId;
                // State with weapon type and state without weapon type not found, try use state with weapon type but without move type
                stateWithWeaponTypeId = ZString.Concat(weaponTypeId, stateWithoutWeaponIdAndMoveType);
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
                string stateId = ZString.Concat(weaponTypeId, CLIP_FALL);
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
                playingStateId = ZString.Concat(stateUpdateData.WeaponTypeId, CLIP_IDLE);
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

            if (!stateUpdateData.playingStateId.Equals(playingStateId) || stateUpdateData.ForcePlay)
            {
                stateUpdateData.playingStateId = playingStateId;
                stateUpdateData.ForcePlay = false;

                // Play new state
                int inputCount = mixer.GetInputCount();
                AnimationClip newClip = stateInfos[playingStateId].GetClip();
                if (newClip != stateUpdateData.previousClip)
                {
                    inputCount += 1;
                    mixer.SetInputCount(inputCount);
                    AnimationClipPlayable playable = AnimationClipPlayable.Create(Graph, newClip);
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
                    // Reset play elapsed
                    stateUpdateData.playElapsed = 0f;
                }

                // Get input port from new playing state ID
                stateUpdateData.inputPort = inputCount - 1;

                // Set avatar mask
                AvatarMask avatarMask = stateInfos[playingStateId].GetAvatarMask();
                if (avatarMask == null)
                    avatarMask = EmptyMask;
                LayerMixer.SetLayerMaskFromAvatarMask(layer, avatarMask);

                // Set clip info 
                stateUpdateData.clipSpeed = stateInfos[playingStateId].GetSpeed(moveAnimationSpeedMultiplier > 0f ? moveAnimationSpeedMultiplier : 1f);
                // Set transition duration
                stateUpdateData.transitionDuration = stateInfos[playingStateId].GetTransitionDuration();
                if (stateUpdateData.transitionDuration <= 0f)
                    stateUpdateData.transitionDuration = CharacterModel.transitionDuration;
                stateUpdateData.transitionDuration /= stateUpdateData.clipSpeed;
                mixer.GetInput(stateUpdateData.inputPort).Play();
                stateUpdateData.clipLength = stateInfos[playingStateId].GetClipLength(1);
                stateUpdateData.previousClip = newClip;

                // Set layer additive
                LayerMixer.SetLayerAdditive(layer, stateInfos[playingStateId].IsAdditive());
            }
        }

        private void UpdateState(AnimationMixerPlayable mixer, StateUpdateData stateUpdateData, float deltaTime, bool isLeftHand)
        {
            int inputCount = mixer.GetInputCount();
            if (inputCount == 0)
                return;

            mixer.GetInput(stateUpdateData.inputPort).SetSpeed(IsFreeze ? 0 : stateUpdateData.clipSpeed);

            float weight;
            float weightUpdate;
            bool transitionEnded = false;
            if (CharacterModel.IsDead && Time.unscaledTime - CharacterModel.SwitchedTime < 1f)
            {
                // Play dead animation at end frame immediately
                mixer.GetInput(stateUpdateData.inputPort).SetTime(Cache.BaseStates[stateUpdateData.playingStateId].State.clip.length);
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
                // Update left-hand weight
                if (isLeftHand)
                    LayerMixer.SetInputWeight(LEFT_HAND_WIELDING_LAYER, 0f);
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

                // Update left-hand weight
                if (isLeftHand)
                {
                    // TODO: May set weight smoothly
                    if (string.IsNullOrEmpty(stateUpdateData.WeaponTypeId))
                        LayerMixer.SetInputWeight(LEFT_HAND_WIELDING_LAYER, 0f);
                    else
                        LayerMixer.SetInputWeight(LEFT_HAND_WIELDING_LAYER, 1f);
                }
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

            if (!Mathf.Approximately(moveAnimationSpeedMultiplier, CharacterModel.MoveAnimationSpeedMultiplier))
            {
                moveAnimationSpeedMultiplier = CharacterModel.MoveAnimationSpeedMultiplier;
                baseStateUpdateData.ForcePlay = true;
                leftHandWieldingStateUpdateData.ForcePlay = true;
            }

            #region Update base state and left-hand wielding
            if (!IsFreeze)
            {
                PrepareForNewState(BaseLayerMixer, BASE_LAYER, Cache.BaseStates, baseStateUpdateData);
                PrepareForNewState(LeftHandWieldingLayerMixer, LEFT_HAND_WIELDING_LAYER, Cache.LeftHandWieldingStates, leftHandWieldingStateUpdateData);
            }

            UpdateState(BaseLayerMixer, baseStateUpdateData, info.deltaTime, false);
            UpdateState(LeftHandWieldingLayerMixer, leftHandWieldingStateUpdateData, info.deltaTime, true);
            #endregion

            #region Update action state
            if (playingActionState == PlayingActionState.None)
                return;

            if (CharacterModel.IsDead && playingActionState != PlayingActionState.Stopping)
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

        public void SetEquipWeapons(IWeaponItem rightHand, IWeaponItem leftHand, IShieldItem leftHandShield)
        {
            baseStateUpdateData.WeaponTypeId = string.Empty;
            if (rightHand != null && Cache.WeaponTypeIds.Contains(rightHand.WeaponType.Id))
                baseStateUpdateData.WeaponTypeId = rightHand.WeaponType.Id;

            leftHandWieldingStateUpdateData.WeaponTypeId = string.Empty;
            if (leftHand != null && Cache.LeftHandWeaponTypeIds.Contains(leftHand.WeaponType.Id))
                leftHandWieldingStateUpdateData.WeaponTypeId = leftHand.WeaponType.Id;

            if (leftHandShield != null && Cache.LeftHandWeaponTypeIds.Contains(SHILED_WEAPON_TYPE_ID))
                leftHandWieldingStateUpdateData.WeaponTypeId = SHILED_WEAPON_TYPE_ID;
        }

        public void PlayJump()
        {
            baseStateUpdateData.PlayingJumpState = PlayingJumpState.Starting;
            leftHandWieldingStateUpdateData.PlayingJumpState = PlayingJumpState.Starting;
        }

        /// <summary>
        /// Order it to play action animation by action state, return calculated animation length
        /// </summary>
        /// <param name="actionState"></param>
        /// <param name="speedRate"></param>
        /// <param name="duration"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
        public float PlayAction(ActionState actionState, float speedRate, float duration = 0f, bool loop = false)
        {
            if (IsFreeze || CharacterModel.IsDead)
                return 0f;

            // Destroy playing state
            if (ActionLayerMixer.IsValid())
                ActionLayerMixer.Destroy();

            ActionLayerMixer = AnimationMixerPlayable.Create(Graph, 1);
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

            return actionClipLength;
        }

        public void StopAction()
        {
            if (playingActionState == PlayingActionState.Playing ||
                playingActionState == PlayingActionState.Looping)
                playingActionState = PlayingActionState.Stopping;
        }
    }
}
