using Cysharp.Threading.Tasks;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MultiplayerARPG
{
    public partial class BuiltInEntityMovementFunctions3D
    {
        protected const int TICK_COUNT_FOR_INTERPOLATION = 1;
        protected const int FORCE_GROUNDED_FRAMES_AFTER_TELEPORT = 3;
        protected const float MIN_MAGNITUDE_TO_DETERMINE_MOVING = 0.01f;
        protected const float MIN_DIRECTION_SQR_MAGNITUDE = 0.0001f;
        protected const float MIN_DISTANCE_TO_TELEPORT = 0.1f;

        [Header("Network Settings")]
        public MovementSecure movementSecure = MovementSecure.NotSecure;
        [Tooltip("If distance between current frame and previous frame is greater than this value, then it will determine that changes occurs and will sync transform later")]
        [Min(0.01f)]
        public float positionThreshold = 0.01f;
        [Tooltip("If angle between current frame and previous frame is greater than this value, then it will determine that changes occurs and will sync transform later")]
        [Min(0.01f)]
        public float eulerAnglesThreshold = 1f;
        [Tooltip("Keep alive ticks before it is stop syncing (after has no changes)")]
        public int keepAliveTicks = 10;
        [Tooltip("If distance between two interpolating positions more than this value, it will change position to target position immediately")]
        [Min(0.01f)]
        public float interpSnapThreshold = 2f;

        [Header("Movement AI")]
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;

        [Header("Movement Settings")]
        public float jumpHeight = 2f;
        public ApplyJumpForceMode applyJumpForceMode = ApplyJumpForceMode.ApplyImmediately;
        public float applyJumpForceFixedDuration;
        public float gravity = 9.81f;
        public float maxFallVelocity = 40f;
        public float groundedVerticalVelocity = 0f;
        public bool doNotChangeVelocityWhileAirborne;

        [Header("Stand Speed Rate")]
        public float standForwardMoveSpeedRate = 1f;
        public float standForwardSideMoveSpeedRate = 1f;
        public float standSideMoveSpeedRate = 1f;
        public float standBackwardSideMoveSpeedRate = 0.75f;
        public float standBackwardMoveSpeedRate = 0.75f;

        [Header("Crouch Speed Rate")]
        public float crouchForwardMoveSpeedRate = 1f;
        public float crouchForwardSideMoveSpeedRate = 1f;
        public float crouchSideMoveSpeedRate = 1f;
        public float crouchBackwardSideMoveSpeedRate = 0.75f;
        public float crouchBackwardMoveSpeedRate = 0.75f;

        [Header("Crawl Speed Rate")]
        public float crawlForwardMoveSpeedRate = 1f;
        public float crawlForwardSideMoveSpeedRate = 1f;
        public float crawlSideMoveSpeedRate = 1f;
        public float crawlBackwardSideMoveSpeedRate = 0.75f;
        public float crawlBackwardMoveSpeedRate = 0.75f;

        [Header("Swim Speed Rate")]
        public float swimForwardMoveSpeedRate = 1f;
        public float swimForwardSideMoveSpeedRate = 1f;
        public float swimSideMoveSpeedRate = 1f;
        public float swimBackwardSideMoveSpeedRate = 0.75f;
        public float swimBackwardMoveSpeedRate = 0.75f;

        [Header("Pausing")]
        public float landedPauseMovementDuration = 0f;
        public float beforeCrawlingPauseMovementDuration = 0f;
        public float afterCrawlingPauseMovementDuration = 0f;

        [Header("Swimming")]
        [Range(0.1f, 1f)]
        public float underWaterThreshold = 0.75f;
        public bool autoSwimToSurface;

        [Header("Dashing")]
        public EntityMovementForceApplierData dashingForceApplier;

        [Header("Root Motion Settings")]
        public bool alwaysUseRootMotion;
        public bool useRootMotionForMovement;
        public bool useRootMotionForAirMovement;
        public bool useRootMotionForJump;
        public bool useRootMotionForFall;
        public bool useRootMotionUnderWater;
        public bool useRootMotionClimbing;
        public float rootMotionGroundedVerticalVelocity = 0f;

        public BaseGameEntity Entity { get; protected set; }
        public LiteNetLibGameManager Manager => Entity.Manager;
        public CharacterLadderComponent LadderComponent { get; protected set; }
        public bool IsServer => Entity.IsServer;
        public bool IsClient => Entity.IsClient;
        public bool IsOwnerClient => Entity.IsOwnerClient;
        public bool IsOwnedByServer => Entity.IsOwnedByServer;
        public bool IsOwnerClientOrOwnedByServer => Entity.IsOwnerClientOrOwnedByServer;
        public GameInstance CurrentGameInstance => Entity.CurrentGameInstance;
        public BaseGameplayRule CurrentGameplayRule => Entity.CurrentGameplayRule;
        public BaseGameNetworkManager CurrentGameManager => Entity.CurrentGameManager;
        public Transform EntityTransform => Entity.EntityTransform;
        public Animator Animator { get; protected set; }
        public IBuiltInEntityMovement3D EntityMovement { get; protected set; }
        public IEntityTeleportPreparer TeleportPreparer { get; protected set; }
        public bool IsPreparingToTeleport => TeleportPreparer != null && TeleportPreparer.IsPreparingToTeleport;

        public float StoppingDistance
        {
            get { return stoppingDistance; }
        }
        public MovementState MovementState { get; protected set; }
        public ExtraMovementState ExtraMovementState { get; protected set; }
        public DirectionVector2 Direction2D { get { return Vector2.down; } set { } }
        public float CurrentMoveSpeed { get; protected set; }
        public Queue<Vector3> NavPaths { get; protected set; }
        public bool HasNavPaths
        {
            get { return NavPaths != null && NavPaths.Count > 0; }
        }
        public bool IsGrounded { get; protected set; } = true;
        public bool IsAirborne { get; protected set; } = false;
        public bool IsUnderWater { get; protected set; } = false;
        public bool IsClimbing { get; protected set; } = false;

        // Inputs
        protected MovementInputData3D _currentInput;
        protected bool _willResetInput = false;
        protected SortedList<uint, MovementInputData3D> _inputBuffers = new SortedList<uint, MovementInputData3D>();
        protected SortedList<uint, MovementSyncData3D> _syncBuffers = new SortedList<uint, MovementSyncData3D>();
        protected SortedList<uint, MovementSyncData3D> _interpBuffers = new SortedList<uint, MovementSyncData3D>();
        protected LogicUpdater _logicUpdater = null;
        protected uint _simTick = 0;
        protected uint _interpTick = 0;
        public uint RenderTick => _interpTick - TICK_COUNT_FOR_INTERPOLATION;

        // Syncing/Interpolating
        protected MovementSyncData3D _prevSyncData;
        protected MovementSyncData3D _interpFromData;
        protected MovementSyncData3D _interpToData;
        protected uint _prevInterpFromTick;
        protected float _startInterpTime;
        protected float _endInterpTime;

        // Teleportation
        protected int _lastTeleportFrame;
        protected MovementTeleportState _serverTeleportState;
        protected uint _teleportRespondedTick;
        protected MovementTeleportState _clientTeleportState;

        // State simulate codes
        protected float _verticalVelocity;
        protected Vector3 _velocityBeforeAirborne;
        protected Collider _waterCollider;
        protected byte _underWaterFrameCount;
        protected Transform _groundedTransform;
        protected Vector3 _previousPlatformPosition;
        protected Vector3 _previousPosition;
        protected Vector3 _previousMovement;
        protected bool _previouslyGrounded = false;
        protected bool _previouslyAirborne = false;
        protected ExtraMovementState _previouslyExtraMovementState;

        // Move simulate codes
        protected Vector3? _prevPointClickPosition = null;
        protected float _pauseMovementCountDown;
        protected Vector3 _moveDirection;
        protected bool _isJumping;
        protected bool _isDashing;

        // Force simulation
        protected readonly List<EntityMovementForceApplier> _movementForceAppliers = new List<EntityMovementForceApplier>();
        protected IEntityMovementForceUpdateListener[] _forceUpdateListeners;

        // Jump simulate codes
        protected bool _applyingJumpForce;
        protected float _applyJumpForceCountDown;
        protected ExtraMovementState _extraMovementStateWhenJump;

        // Turn simulate codes
        protected bool _lookRotationApplied;
        protected float _yAngle;
        protected float _yTurnSpeed;

        public BuiltInEntityMovementFunctions3D(BaseGameEntity entity, Animator animator, IBuiltInEntityMovement3D entityMovement)
        {
            Entity = entity;
            LadderComponent = entity.GetComponent<CharacterLadderComponent>();
            Animator = animator;
            EntityMovement = entityMovement;
            TeleportPreparer = entity.GetComponent<IEntityTeleportPreparer>();
            _forceUpdateListeners = entity.GetComponents<IEntityMovementForceUpdateListener>();
            _yAngle = EntityTransform.eulerAngles.y;
            _lookRotationApplied = true;
            _currentInput = new MovementInputData3D();
            StopMoveFunction();
        }

        public void EntityStart()
        {
            _clientTeleportState = MovementTeleportState.Responding;
            _yAngle = EntityTransform.eulerAngles.y;
            _verticalVelocity = 0;
            _lastTeleportFrame = Time.frameCount;
            _previousPosition = EntityTransform.position;
        }

        public void ComponentEnabled()
        {
            _yAngle = EntityTransform.eulerAngles.y;
            _verticalVelocity = 0;
            _lastTeleportFrame = Time.frameCount;
            _previousPosition = EntityTransform.position;
        }

        public void OnSetOwnerClient(bool isOwnerClient)
        {
            NavPaths = null;
            ResetBuffersAndStates();
            // Force setup sim tick
            if (IsOwnerClientOrOwnedByServer)
                _simTick = Manager.LocalTick;
        }

        public void EntityOnIdentityInitialize()
        {
            if (_logicUpdater == null)
            {
                _logicUpdater = Manager.LogicUpdater;
                _logicUpdater.OnTick += LogicUpdater_OnTick;
            }
            _interpFromData = _interpToData = new MovementSyncData3D()
            {
                Position = EntityTransform.position,
                Rotation = EntityTransform.eulerAngles.y,
                MovementState = MovementState,
                ExtraMovementState = ExtraMovementState,
            };
            ResetBuffersAndStates();
        }

        protected void ResetBuffersAndStates()
        {
            _inputBuffers.Clear();
            _syncBuffers.Clear();
            _interpBuffers.Clear();
            ClearInterpolationTick();
            ClearSimulationTick();
            // Setup data for syncing determining
            _prevSyncData = new MovementSyncData3D()
            {
                Tick = Manager.LocalTick,
                Position = EntityTransform.position,
                MovementState = MovementState,
                ExtraMovementState = ExtraMovementState,
                Rotation = EntityTransform.eulerAngles.y,
            };
            _yAngle = EntityTransform.eulerAngles.y;
            _lookRotationApplied = true;
        }

        public void EntityOnDestroy()
        {
            if (_logicUpdater != null)
                _logicUpdater.OnTick -= LogicUpdater_OnTick;
        }

        protected void LogicUpdater_OnTick(LogicUpdater updater)
        {
            // Tick count for interpolation
            _simTick++;
            _interpTick++;

            // Manage only owned objects
            if (!IsOwnerClientOrOwnedByServer)
                return;

            // Storing sync buffers, server will send to other clients, owner client will send to server
            if (IsServer || (IsOwnerClient && movementSecure == MovementSecure.NotSecure))
            {
                float rotation = EntityTransform.eulerAngles.y;
                MovementSyncData3D syncData = _prevSyncData;
                bool changed =
                    Vector3.Distance(EntityTransform.position, syncData.Position) > positionThreshold ||
                    MovementState != syncData.MovementState || ExtraMovementState != syncData.ExtraMovementState ||
                    Mathf.Abs(rotation - syncData.Rotation) > eulerAnglesThreshold;
                bool keepAlive = updater.LocalTick - syncData.Tick <= keepAliveTicks;

                if (!changed && !keepAlive)
                {
                    // No changes, not going to keep alive, clear sync buffers
                    _syncBuffers.Clear();
                    return;
                }

                if (changed)
                {
                    syncData.Tick = updater.LocalTick;
                    syncData.Position = EntityTransform.position;
                    syncData.MovementState = MovementState;
                    syncData.ExtraMovementState = ExtraMovementState;
                    syncData.Rotation = rotation;
                }
                _prevSyncData = syncData;

                syncData.Tick = updater.LocalTick;
                // Stored buffers will be send later
                StoreSyncBuffer(syncData, 3);
            }

            _currentInput.Tick = _simTick - 1;
            StoreInputBuffer(_currentInput, 3);
            _willResetInput = true;
        }

        public void OnAnimatorMove()
        {
            if (!Animator)
                return;

            if (alwaysUseRootMotion || Entity.ShouldUseRootMotion)
            {
                // Always use root motion
                Animator.ApplyBuiltinRootMotion();
                return;
            }

            if (MovementState.Has(MovementState.IsGrounded) && useRootMotionForMovement)
                Animator.ApplyBuiltinRootMotion();
            if (!MovementState.Has(MovementState.IsGrounded) && useRootMotionForAirMovement)
                Animator.ApplyBuiltinRootMotion();
            if (MovementState.Has(MovementState.IsUnderWater) && useRootMotionUnderWater)
                Animator.ApplyBuiltinRootMotion();
            if (MovementState.Has(MovementState.IsClimbing) && useRootMotionClimbing)
                Animator.ApplyBuiltinRootMotion();
        }

        public void StopMove()
        {
            if (Entity.IsOwnerClientOrOwnedByServer)
            {
                // Send movement input to server, then server will apply movement and sync transform to clients
                uint tick = Manager.LocalTick;
                _inputBuffers[tick] = new MovementInputData3D()
                {
                    Tick = tick,
                    IsStopped = true,
                    IsPointClick = false,
                };
            }
            StopMoveFunction();
        }

        public void StopMoveFunction()
        {
            NavPaths = null;
            MovementState movementState = MovementState;
            movementState &= ~MovementState.Forward;
            movementState &= ~MovementState.Backward;
            movementState &= ~MovementState.Right;
            movementState &= ~MovementState.Left;
            movementState &= ~MovementState.Up;
            movementState &= ~MovementState.Down;
            MovementState = movementState;
        }

        public void KeyMovement(Vector3 moveDirection, MovementState movementState)
        {
            if (!Entity.IsOwnerClientOrOwnedByServer)
                return;

            if (moveDirection.sqrMagnitude > MIN_DIRECTION_SQR_MAGNITUDE)
                _currentInput.IsPointClick = false;
            bool hasIsJump = movementState.Has(MovementState.IsJump);
            bool hasIsDash = movementState.Has(MovementState.IsDash);
            bool prevHasIsJump = _currentInput.MovementState.Has(MovementState.IsJump);
            bool prevHasIsDash = _currentInput.MovementState.Has(MovementState.IsDash);
            if (!_currentInput.MovementState.HasDirectionMovement())
                _currentInput.MovementState = movementState;
            if (hasIsJump || prevHasIsJump)
                _currentInput.MovementState |= MovementState.IsJump;
            if (hasIsDash || prevHasIsDash)
                _currentInput.MovementState |= MovementState.IsDash;
            if (_currentInput.MoveDirection.ToVector3().sqrMagnitude <= MIN_DIRECTION_SQR_MAGNITUDE)
                _currentInput.MoveDirection = moveDirection;
            if (_currentInput.LookDirection.ToVector3().sqrMagnitude <= MIN_DIRECTION_SQR_MAGNITUDE)
                _currentInput.LookDirection = moveDirection;
        }

        public void PointClickMovement(Vector3 position)
        {
            if (!Entity.IsOwnerClientOrOwnedByServer)
                return;
            _currentInput.IsPointClick = true;
            _currentInput.Position = position;
            _currentInput.MoveDirection = Vector3.zero;
            _currentInput.LookDirection = Vector3.zero;
        }

        public void SetExtraMovementState(ExtraMovementState extraMovementState)
        {
            if (!Entity.IsOwnerClientOrOwnedByServer)
                return;
            if (_currentInput.ExtraMovementState == ExtraMovementState.None)
                _currentInput.ExtraMovementState = extraMovementState;
        }

        public void SetLookRotation(Quaternion rotation, bool immediately)
        {
            if (!Entity.IsOwnerClientOrOwnedByServer)
                return;
            Vector3 lookDirection = rotation * Vector3.forward;
            if (LadderComponent && LadderComponent.ClimbingLadder)
            {
                // Turn to the ladder
                lookDirection = -LadderComponent.ClimbingLadder.ForwardWithYAngleOffsets;
            }
            _currentInput.LookDirection = lookDirection;
            _lookRotationApplied = false;
            if (immediately && Entity.CanTurn())
                TurnImmediately(rotation.eulerAngles.y);
        }

        public Quaternion GetLookRotation()
        {
            return Quaternion.Euler(0f, EntityTransform.eulerAngles.y, 0f);
        }

        public void SetSmoothTurnSpeed(float turnDuration)
        {
            _yTurnSpeed = turnDuration;
        }

        public float GetSmoothTurnSpeed()
        {
            return _yTurnSpeed;
        }

        public async void Teleport(Vector3 position, Quaternion rotation, bool stillMoveAfterTeleport)
        {
            if (!IsServer)
            {
                Logging.LogWarning(nameof(BuiltInEntityMovementFunctions3D), $"Teleport function shouldn't be called at client [{Entity.name}]");
                return;
            }
            if (_serverTeleportState != MovementTeleportState.None)
            {
                // Still waiting for teleport responding
                return;
            }
            await OnTeleport(position, rotation, stillMoveAfterTeleport);
        }

        protected void SetupSimulationTick(uint simTick)
        {
            if (_simTick > simTick && _simTick - simTick > 2)
                _simTick = simTick;
            if (simTick > _simTick && simTick - _simTick > 2)
                _simTick = simTick;
        }

        protected void ClearSimulationTick()
        {
            _simTick = 0;
        }

        protected void SetupInterpolationTick(uint interpTick)
        {
            if (_interpTick > interpTick && _interpTick - interpTick > 2)
                _interpTick = interpTick;
            if (interpTick > _interpTick && interpTick - _interpTick > 2)
                _interpTick = interpTick;
        }

        protected void ClearInterpolationTick()
        {
            _interpTick = 0;
        }

        public void ApplyForce(ApplyMovementForceMode mode, Vector3 direction, ApplyMovementForceSourceType sourceType, int sourceDataId, int sourceLevel, float force, float deceleration, float duration)
        {
            if (!IsServer)
                return;
            if (mode.IsReplaceMovement())
            {
                // Can have only one replace movement force applier, so remove stored ones
                _movementForceAppliers.RemoveReplaceMovementForces();
            }
            _movementForceAppliers.Add(new EntityMovementForceApplier()
                .Apply(mode, direction, sourceType, sourceDataId, sourceLevel, force, deceleration, duration));
        }

        public EntityMovementForceApplier FindForceByActionKey(ApplyMovementForceSourceType sourceType, int sourceDataId)
        {
            return _movementForceAppliers.FindBySource(sourceType, sourceDataId);
        }

        public void ClearAllForces()
        {
            if (!IsServer)
                return;
            _movementForceAppliers.Clear();
        }

        /// <summary>
        /// Calculates the velocity required to move the character to the target position over a specific deltaTime.
        /// Useful for when you wish to work with positions rather than velocities in the UpdateVelocity callback 
        /// </summary>
        public Vector3 GetVelocityForMovePosition(Vector3 fromPosition, Vector3 toPosition, float deltaTime)
        {
            return GetVelocityFromMovement(toPosition - fromPosition, deltaTime);
        }

        public Vector3 GetVelocityFromMovement(Vector3 movement, float deltaTime)
        {
            if (deltaTime <= 0f)
                return Vector3.zero;

            return movement / deltaTime;
        }

        public bool WaterCheck(Collider waterCollider)
        {
            if (waterCollider == null)
            {
                // Not in water
                return false;
            }
            bool isUnderWater = Entity.EntityTransform.position.y < TargetWaterSurfaceY(waterCollider) + 0.01f;
            if (isUnderWater)
            {
                if (_underWaterFrameCount < 3)
                    _underWaterFrameCount++;
                return _underWaterFrameCount >= 3;
            }
            else
            {
                _underWaterFrameCount = 0;
                return false;
            }
        }

        public float TargetWaterSurfaceY(Collider waterCollider)
        {
            Bounds movementBounds = EntityMovement.GetMovementBounds();
            float result = waterCollider.bounds.max.y - (underWaterThreshold * movementBounds.size.y);
            return result;
        }

        public void EntityUpdate(float deltaTime)
        {
            // Simulate movement by inputs if it can predict movement
            if (CanSimulateMovement())
            {
                SimulateMovementFromInput(deltaTime);
                if (_willResetInput)
                {
                    _willResetInput = false;
                    _currentInput = new MovementInputData3D()
                    {
                        IsPointClick = _currentInput.IsPointClick,
                        Position = _currentInput.Position,
                    };
                }
            }
            else
            {
                InterpolateTransform();
            }
        }

        protected void SimulateMovementFromInput(float deltaTime)
        {
            if (IsServer && _serverTeleportState != MovementTeleportState.None)
            {
                // Waiting for client teleport confirmation
                return;
            }

            MovementInputData3D inputData;
            if (IsOwnerClientOrOwnedByServer)
            {
                inputData = _currentInput;
            }
            else if (!TryGetInputBuffer(out inputData))
            {
                inputData = new MovementInputData3D()
                {
                    Tick = 0,
                    IsStopped = false,
                    IsPointClick = false,
                    Position = Vector3.zero,
                    MovementState = MovementState.None,
                    ExtraMovementState = ExtraMovementState,
                    MoveDirection = Vector3.zero,
                    LookDirection = EntityTransform.forward,
                };
                if (_prevPointClickPosition.HasValue)
                {
                    inputData.IsPointClick = true;
                    inputData.Position = _prevPointClickPosition.Value;
                }
            }

            UpdateMovement(deltaTime, ref inputData);
            UpdateRotation(deltaTime, ref inputData);
        }

        protected void InterpolateTransform()
        {
            if (IsServer && _serverTeleportState != MovementTeleportState.None)
            {
                // Waiting for client teleport confirmation
                return;
            }

            if (_interpBuffers.Count < 2)
            {
                // Not ready for interpolation
                _prevInterpFromTick = 0;
                return;
            }

            float currentTime = Time.time;
            uint renderTick = RenderTick;

            // Find two ticks around renderTick
            uint interpFromTick = 0;
            uint interpToTick = 0;

            for (int i = 0; i < _interpBuffers.Count - 1; ++i)
            {
                uint tick1 = _interpBuffers.Keys[i];
                uint tick2 = _interpBuffers.Keys[i + 1];
                MovementSyncData3D data1 = _interpBuffers[tick1];
                MovementSyncData3D data2 = _interpBuffers[tick2];

                if (tick1 <= renderTick && renderTick <= tick2)
                {
                    // TODO: Speed hack checking here
                    interpFromTick = tick1;
                    interpToTick = tick2;
                    _interpFromData = data1;
                    _interpToData = data2;
                    if (_prevInterpFromTick != interpFromTick)
                    {
                        _startInterpTime = currentTime;
                        _endInterpTime = currentTime + (_logicUpdater.DeltaTimeF * (tick2 - tick1));
                        _prevInterpFromTick = interpFromTick;
                    }
                    break;
                }
            }

            if (_interpToData.Tick < 2)
                return;

            float t = Mathf.InverseLerp(_startInterpTime, _endInterpTime, currentTime);
            Vector3 position = Vector3.Lerp(_interpFromData.Position, _interpToData.Position, t);
            EntityTransform.position = position;
            float rotation = Mathf.LerpAngle(_interpFromData.Rotation, _interpToData.Rotation, t);
            EntityTransform.rotation = Quaternion.Euler(0f, rotation, 0f);
            MovementState = t < 0.75f ? _interpFromData.MovementState : _interpToData.MovementState;
            ExtraMovementState = t < 0.75f ? _interpFromData.ExtraMovementState : _interpToData.ExtraMovementState;
        }

        protected bool TryGetInputBuffer(out MovementInputData3D inputData, byte maxLookback = 2)
        {
            for (byte i = 0; i <= maxLookback; ++i)
            {
                if (_simTick > 0 && _simTick <= i)
                {
                    // Not able to look back
                    break;
                }
                if (_inputBuffers.TryGetValue(_simTick - i, out inputData))
                    return true;
            }
            inputData = default;
            return false;
        }

        protected void StoreInputBuffer(MovementInputData3D entry, int maxBuffers)
        {
            if (!_inputBuffers.ContainsKey(entry.Tick))
            {
                _inputBuffers.Add(entry.Tick, entry);
            }
            // Prune old ticks (keep last N)
            while (_inputBuffers.Count > maxBuffers)
            {
                _inputBuffers.RemoveAt(0);
            }
        }

        protected void StoreInputBuffers(MovementInputData3D[] data, int size, uint acceptTick, int maxBuffers)
        {
            for (int i = 0; i < size; ++i)
            {
                MovementInputData3D entry = data[i];
                if (entry.Tick < acceptTick)
                {
                    // Don't accept this tick
                    continue;
                }
                if (_inputBuffers.ContainsKey(entry.Tick))
                {
                    // This tick is already stored
                    continue;
                }
                _inputBuffers.Add(entry.Tick, entry);
            }
            // Prune old ticks (keep last N)
            while (_inputBuffers.Count > maxBuffers)
            {
                _inputBuffers.RemoveAt(0);
            }
        }

        protected void StoreSyncBuffer(MovementSyncData3D entry, int maxBuffers)
        {
            if (!_syncBuffers.ContainsKey(entry.Tick))
            {
                _syncBuffers.Add(entry.Tick, entry);
            }
            // Prune old ticks (keep last N)
            while (_syncBuffers.Count > maxBuffers)
            {
                _syncBuffers.RemoveAt(0);
            }
        }

        protected void StoreInterpolateBuffers(MovementSyncData3D[] data, int size, uint acceptTick, int maxBuffers)
        {
            for (int i = 0; i < size; ++i)
            {
                MovementSyncData3D entry = data[i];
                if (entry.Tick < acceptTick)
                {
                    // Don't accept this tick
                    continue;
                }
                if (_interpBuffers.ContainsKey(entry.Tick))
                {
                    // This tick is already stored
                    continue;
                }
                _interpBuffers.Add(entry.Tick, entry);
            }
            // Prune old ticks (keep last N)
            while (_interpBuffers.Count > maxBuffers)
            {
                _interpBuffers.RemoveAt(0);
            }
        }

        public void UpdateMovement(float deltaTime, ref MovementInputData3D inputData)
        {
            if (IsPreparingToTeleport)
                return;

            if (inputData.IsStopped)
            {
                StopMoveFunction();
                return;
            }

            _moveDirection = Vector3.zero;
            _isJumping = inputData.MovementState.Has(MovementState.IsJump);
            _isDashing = inputData.MovementState.Has(MovementState.IsDash);
            IsUnderWater = WaterCheck(_waterCollider);
            IsClimbing = LadderComponent && LadderComponent.ClimbingLadder;
            IsGrounded = EntityMovement.GroundCheck();
            IsAirborne = !IsGrounded && !IsUnderWater && !IsClimbing && EntityMovement.AirborneCheck();

            // Underwater state, movement state must be setup here to make it able to calculate move speed properly
            if (IsClimbing)
                inputData.MovementState |= MovementState.IsClimbing;
            else if (IsUnderWater)
                inputData.MovementState |= MovementState.IsUnderWater;

            if (IsAirborne || IsClimbing || IsUnderWater)
            {
                if (inputData.ExtraMovementState == ExtraMovementState.IsCrouching || inputData.ExtraMovementState == ExtraMovementState.IsCrawling)
                    inputData.ExtraMovementState = ExtraMovementState.None;
            }

            if (IsClimbing)
                UpdateClimbMovement(deltaTime, ref inputData);
            else
                UpdateGenericMovement(deltaTime, ref inputData);

            // Re-setup movement state here to make sure it is correct
            inputData.MovementState = _moveDirection.sqrMagnitude > MIN_DIRECTION_SQR_MAGNITUDE ? inputData.MovementState : MovementState.None;
            if (IsUnderWater)
                inputData.MovementState |= MovementState.IsUnderWater;
            if (IsGrounded || !IsAirborne || Time.frameCount - _lastTeleportFrame < FORCE_GROUNDED_FRAMES_AFTER_TELEPORT)
                inputData.MovementState |= MovementState.IsGrounded;
            if (_isJumping)
                inputData.MovementState |= MovementState.IsJump;
            // Update movement state
            MovementState = inputData.MovementState;
            // Update extra movement state
            ExtraMovementState = Entity.ValidateExtraMovementState(MovementState, inputData.ExtraMovementState);
            if (_isJumping || IsAirborne)
                ExtraMovementState = _extraMovementStateWhenJump;

            // Prepare previous states will being used in next frames
            _previouslyGrounded = IsGrounded;
            _previouslyAirborne = IsAirborne;
            _previousPosition = EntityTransform.position;
            _previouslyExtraMovementState = ExtraMovementState;
            _isJumping = false;
            _isDashing = false;
        }

        protected void UpdateClimbMovement(float deltaTime, ref MovementInputData3D inputData)
        {
            if (IsPreparingToTeleport)
                return;

            Vector3 tempPredictPosition;
            Vector3 tempCurrentPosition = EntityTransform.position;
            // Prepare movement speed
            inputData.ExtraMovementState = Entity.ValidateExtraMovementState(inputData.MovementState, inputData.ExtraMovementState);
            float tempEntityMoveSpeed = Entity.GetMoveSpeed(inputData.MovementState, inputData.ExtraMovementState);
            float tempMaxMoveSpeed = tempEntityMoveSpeed;
            CurrentMoveSpeed = tempMaxMoveSpeed;

            float currentTime = Time.unscaledTime;
            Vector3 tempMoveVelocity;
            switch (LadderComponent.EnterExitState)
            {
                case EnterExitState.Enter:
                case EnterExitState.Exit:
                    // Enter or exit
                    Vector3 tempPosition;
                    if (LadderComponent.EnterOrExitDuration > 0f)
                        tempPosition = Vector3.Lerp(LadderComponent.EnterOrExitFromPosition, LadderComponent.EnterOrExitToPosition, currentTime - LadderComponent.EnterOrExitTime / LadderComponent.EnterOrExitDuration);
                    else
                        tempPosition = LadderComponent.EnterOrExitToPosition;
                    tempMoveVelocity = GetVelocityForMovePosition(tempCurrentPosition, tempPosition, deltaTime);
                    break;
                case EnterExitState.ConfirmAwaiting:
                    tempMoveVelocity = Vector3.zero;
                    break;
                default:
                    if (inputData.MovementState.Has(MovementState.Up))
                        _moveDirection.y = 1f;
                    else if (inputData.MovementState.Has(MovementState.Down))
                        _moveDirection.y = -1f;

                    if (Mathf.Approximately(_moveDirection.y, 0f))
                        return;

                    tempMoveVelocity = GetVelocityForMovePosition(tempCurrentPosition,
                        LadderComponent.ClimbingLadder.ClosestPointOnLadderSegment(tempCurrentPosition, EntityMovement.GetMovementBounds().extents.z, out float segmentState), deltaTime) +
                        LadderComponent.ClimbingLadder.Up * _moveDirection.y * CurrentMoveSpeed;

                    if (Mathf.Abs(segmentState) > 0.05f)
                    {
                        if (segmentState > 0 && _moveDirection.y > 0f)
                        {
                            // Exit (top)
                            //tempMoveVelocity = GetVelocityForMovePosition(tempCurrentPosition, LadderComponent.ClimbingLadder.topExitTransform.position, deltaTime);

                            LadderComponent.EnterExitState = EnterExitState.ConfirmAwaiting;
                            LadderComponent.CallCmdExitLadder(LadderEntranceType.Top);
                        }
                        // If we're lower than the ladder bottom point
                        else if (segmentState < 0 && _moveDirection.y < 0f)
                        {
                            // Exit (bottom)
                            //tempMoveVelocity = GetVelocityForMovePosition(tempCurrentPosition, LadderComponent.ClimbingLadder.bottomExitTransform.position, deltaTime);

                            LadderComponent.EnterExitState = EnterExitState.ConfirmAwaiting;
                            LadderComponent.CallCmdExitLadder(LadderEntranceType.Bottom);
                        }
                    }
                    break;
            }
            // Move
            tempPredictPosition = tempCurrentPosition + (tempMoveVelocity * deltaTime);
            _previousMovement = tempMoveVelocity * deltaTime;
            EntityMovement.Move(inputData.MovementState, inputData.ExtraMovementState, _previousMovement, deltaTime);
        }

        protected void UpdateGenericMovement(float deltaTime, ref MovementInputData3D inputData)
        {
            if (IsPreparingToTeleport)
                return;

            float tempSqrMagnitude;
            float tempPredictSqrMagnitude;
            Vector3 tempPredictPosition;
            float tempTargetDistance = 0f;
            Vector3 tempHorizontalMoveDirection = Vector3.zero;
            Vector3 tempMoveVelocity = Vector3.zero;
            Vector3 tempCurrentPosition = EntityTransform.position;
            Vector3 tempTargetPosition = tempCurrentPosition;
            bool forceUseRootMotion = alwaysUseRootMotion || Entity.ShouldUseRootMotion;
            Vector3 tempInputDirection = Vector3.zero;

            if (!inputData.IsPointClick)
            {
                tempInputDirection = inputData.MoveDirection;
                NavPaths = null;
            }

            if (inputData.IsPointClick && (!_prevPointClickPosition.HasValue || Vector3.Distance(_prevPointClickPosition.Value, inputData.Position) > 0.01f))
            {
                _prevPointClickPosition = inputData.Position;
                SetMovePaths(inputData.Position, true);
            }

            if (HasNavPaths)
            {
                // Set `tempTargetPosition` and `tempCurrentPosition`
                tempTargetPosition = NavPaths.Peek();
                _moveDirection = (tempTargetPosition - tempCurrentPosition).normalized;
                tempTargetDistance = Vector3.Distance(tempTargetPosition.GetXZ(), tempCurrentPosition.GetXZ());
                bool shouldStop = tempTargetDistance < StoppingDistance;
                if (shouldStop)
                {
                    NavPaths.Dequeue();
                    if (!HasNavPaths)
                    {
                        StopMoveFunction();
                        _moveDirection = Vector3.zero;
                    }
                    else
                    {
                        if (!inputData.MovementState.Has(MovementState.Forward))
                            inputData.MovementState |= MovementState.Forward;
                    }
                }
                else
                {
                    if (!inputData.MovementState.Has(MovementState.Forward))
                        inputData.MovementState |= MovementState.Forward;
                }
            }
            else if (tempInputDirection.sqrMagnitude > MIN_DIRECTION_SQR_MAGNITUDE)
            {
                _moveDirection = tempInputDirection.normalized;
                tempTargetPosition = tempCurrentPosition + _moveDirection;
            }

            if (HasNavPaths && _lookRotationApplied && _moveDirection.sqrMagnitude > MIN_DIRECTION_SQR_MAGNITUDE)
            {
                // Turn character by move direction
                if (Entity.CanTurn())
                    inputData.LookDirection = _moveDirection;
            }

            if (!Entity.CanMove())
            {
                _moveDirection = Vector3.zero;
                _isJumping = false;
                _applyingJumpForce = false;
                _isDashing = false;
            }

            if (_isJumping && (_applyingJumpForce || !Entity.CanJump() || !Entity.AllowToJump()))
            {
                _isJumping = false;
            }

            if (_isDashing && (!Entity.CanDash() || !Entity.AllowToDash()))
            {
                _isDashing = false;
            }

            // Prepare movement speed
            inputData.ExtraMovementState = Entity.ValidateExtraMovementState(inputData.MovementState, inputData.ExtraMovementState);
            float tempEntityMoveSpeed = _applyingJumpForce ? 0f : Entity.GetMoveSpeed(inputData.MovementState, inputData.ExtraMovementState);
            float tempMaxMoveSpeed = tempEntityMoveSpeed;

            // Calculate vertical velocity by gravity
            if (!IsGrounded && !IsUnderWater)
            {
                if (!useRootMotionForFall && !forceUseRootMotion)
                {
                    float gravity = CalculateGravity();
                    float maxFallVelocity = CalculateMaxFallVelocity();
                    _verticalVelocity -= gravity * deltaTime;
                    if (_verticalVelocity < -maxFallVelocity)
                        _verticalVelocity = -maxFallVelocity;
                }
                else
                {
                    _verticalVelocity = rootMotionGroundedVerticalVelocity;
                }
            }
            else
            {
                // Not falling set verical velocity to 0
                _verticalVelocity = groundedVerticalVelocity;
            }

            // Jumping
            if (_pauseMovementCountDown <= 0f && _isJumping)
            {
                _extraMovementStateWhenJump = inputData.ExtraMovementState;
                if (CanSimulateMovement())
                    Entity.CallRpcPlayJumpAnimation();
                _applyingJumpForce = true;
                _applyJumpForceCountDown = 0f;
                switch (applyJumpForceMode)
                {
                    case ApplyJumpForceMode.ApplyAfterFixedDuration:
                        _applyJumpForceCountDown = applyJumpForceFixedDuration;
                        break;
                    case ApplyJumpForceMode.ApplyAfterJumpDuration:
                        if (Entity.Model is IJumppableModel jumppableModel)
                            _applyJumpForceCountDown = jumppableModel.GetJumpAnimationDuration();
                        break;
                }
            }

            if (_applyingJumpForce)
            {
                _applyJumpForceCountDown -= deltaTime;
                if (_applyJumpForceCountDown <= 0f)
                {
                    IsGrounded = false;
                    _applyingJumpForce = false;
                    float jumpForceVerticalVelocity = CalculateJumpVerticalSpeed();
                    if (!useRootMotionForJump && !forceUseRootMotion)
                    {
                        _verticalVelocity = jumpForceVerticalVelocity;
                    }
                    EntityMovement.OnJumpForceApplied(jumpForceVerticalVelocity);
                    Entity.OnJumpForceApplied(jumpForceVerticalVelocity);
                }
            }

            // Updating horizontal movement (WASD inputs)
            if (!IsAirborne)
                _velocityBeforeAirborne = Vector3.zero;

            // Dashing
            if (_pauseMovementCountDown <= 0f && IsGrounded && _isDashing)
            {
                // Can have only one replace movement force applier, so remove stored ones
                _movementForceAppliers.RemoveReplaceMovementForces();
                _movementForceAppliers.Add(new EntityMovementForceApplier().Apply(
                    ApplyMovementForceMode.Dash, EntityTransform.forward, ApplyMovementForceSourceType.None, 0, 0, dashingForceApplier));
            }

            // Apply Forces
            _forceUpdateListeners.OnPreUpdateForces(_movementForceAppliers);
            _movementForceAppliers.UpdateForces(deltaTime,
                Entity.GetMoveSpeed(MovementState.Forward, ExtraMovementState.None),
                out Vector3 forceMotion, out EntityMovementForceApplier replaceMovementForceApplier);
            _forceUpdateListeners.OnPostUpdateForces(_movementForceAppliers);

            // Replace player's movement by this
            if (replaceMovementForceApplier != null)
            {
                // Still dashing to add dash to movement state
                if (replaceMovementForceApplier.Mode == ApplyMovementForceMode.Dash)
                    inputData.MovementState |= MovementState.IsDash;
                // Force turn to dashed direction
                _moveDirection = replaceMovementForceApplier.Direction;
                inputData.LookDirection = _moveDirection;
                // Change move speed to dash force
                tempMaxMoveSpeed = replaceMovementForceApplier.CurrentSpeed;
            }

            // Movement updating
            if (_pauseMovementCountDown <= 0f && _moveDirection.sqrMagnitude > MIN_DIRECTION_SQR_MAGNITUDE && (!IsAirborne || !doNotChangeVelocityWhileAirborne || !IsOwnerClientOrOwnedByServer))
            {
                // Calculate only horizontal move direction
                tempHorizontalMoveDirection = _moveDirection;
                tempHorizontalMoveDirection.y = 0;
                tempHorizontalMoveDirection.Normalize();

                // Get angle between move direction and forward
                float moveAngle = Vector3.Angle(tempHorizontalMoveDirection, EntityTransform.forward);

                // Start with base speed
                float moveSpeedRate = 1f;

                // Forward (0-34)
                if (moveAngle < 35f)
                {
                    if (IsUnderWater)
                    {
                        moveSpeedRate = swimForwardMoveSpeedRate;
                    }
                    else
                    {
                        switch (inputData.ExtraMovementState)
                        {
                            case ExtraMovementState.IsCrawling:
                                moveSpeedRate = crawlForwardMoveSpeedRate;
                                break;
                            case ExtraMovementState.IsCrouching:
                                moveSpeedRate = crouchForwardMoveSpeedRate;
                                break;
                            default:
                                moveSpeedRate = standForwardMoveSpeedRate;
                                break;
                        }
                    }
                }
                // Forward-side (diagonal, 35-55)
                else if (moveAngle >= 35f && moveAngle <= 55f)
                {
                    if (IsUnderWater)
                    {
                        moveSpeedRate = swimForwardSideMoveSpeedRate;
                    }
                    else
                    {
                        switch (inputData.ExtraMovementState)
                        {
                            case ExtraMovementState.IsCrawling:
                                moveSpeedRate = crawlForwardSideMoveSpeedRate;
                                break;
                            case ExtraMovementState.IsCrouching:
                                moveSpeedRate = crouchForwardSideMoveSpeedRate;
                                break;
                            default:
                                moveSpeedRate = standForwardSideMoveSpeedRate;
                                break;
                        }
                    }
                }
                // Pure side (56-124)
                else if (moveAngle > 55f && moveAngle < 125f)
                {
                    if (IsUnderWater)
                    {
                        moveSpeedRate = swimSideMoveSpeedRate;
                    }
                    else
                    {
                        switch (inputData.ExtraMovementState)
                        {
                            case ExtraMovementState.IsCrawling:
                                moveSpeedRate = crawlSideMoveSpeedRate;
                                break;
                            case ExtraMovementState.IsCrouching:
                                moveSpeedRate = crouchSideMoveSpeedRate;
                                break;
                            default:
                                moveSpeedRate = standSideMoveSpeedRate;
                                break;
                        }
                    }
                }
                // Backward-side (diagonal, 125-145)
                else if (moveAngle >= 125f && moveAngle <= 145f)
                {
                    if (IsUnderWater)
                    {
                        moveSpeedRate = swimBackwardSideMoveSpeedRate;
                    }
                    else
                    {
                        switch (inputData.ExtraMovementState)
                        {
                            case ExtraMovementState.IsCrawling:
                                moveSpeedRate = crawlBackwardSideMoveSpeedRate;
                                break;
                            case ExtraMovementState.IsCrouching:
                                moveSpeedRate = crouchBackwardSideMoveSpeedRate;
                                break;
                            default:
                                moveSpeedRate = standBackwardSideMoveSpeedRate;
                                break;
                        }
                    }
                }
                // Backward (146-180)
                else if (moveAngle > 145f)
                {
                    if (IsUnderWater)
                    {
                        moveSpeedRate = swimBackwardMoveSpeedRate;
                    }
                    else
                    {
                        switch (inputData.ExtraMovementState)
                        {
                            case ExtraMovementState.IsCrawling:
                                moveSpeedRate = crawlBackwardMoveSpeedRate;
                                break;
                            case ExtraMovementState.IsCrouching:
                                moveSpeedRate = crouchBackwardMoveSpeedRate;
                                break;
                            default:
                                moveSpeedRate = standBackwardMoveSpeedRate;
                                break;
                        }
                    }
                }
                CurrentMoveSpeed = tempMaxMoveSpeed * moveSpeedRate;

                // NOTE: `tempTargetPosition` and `tempCurrentPosition` were set above
                tempSqrMagnitude = (tempTargetPosition - tempCurrentPosition).sqrMagnitude;
                tempPredictPosition = tempCurrentPosition + (tempHorizontalMoveDirection * CurrentMoveSpeed * deltaTime);
                tempPredictSqrMagnitude = (tempPredictPosition - tempCurrentPosition).sqrMagnitude;
                if (HasNavPaths)
                {
                    // Check `tempSqrMagnitude` against the `tempPredictSqrMagnitude`
                    // if `tempPredictSqrMagnitude` is greater than `tempSqrMagnitude`,
                    // rigidbody will reaching target and character is moving pass it,
                    // so adjust move speed by distance and time (with physic formula: v=s/t)
                    if (tempPredictSqrMagnitude >= tempSqrMagnitude && tempTargetDistance > 0f)
                        CurrentMoveSpeed *= tempTargetDistance / deltaTime / CurrentMoveSpeed;
                    if (CurrentMoveSpeed < 0.01f || tempTargetDistance <= 0f)
                        CurrentMoveSpeed = 0f;
                }
                tempMoveVelocity = tempHorizontalMoveDirection * CurrentMoveSpeed;
                _velocityBeforeAirborne = tempMoveVelocity;
            }

            // Pause movement updating
            if (IsOwnerClientOrOwnedByServer)
            {
                if (IsGrounded && _previouslyAirborne)
                {
                    _pauseMovementCountDown = landedPauseMovementDuration;
                }
                else if (IsGrounded && _previouslyExtraMovementState != ExtraMovementState.IsCrawling && inputData.ExtraMovementState == ExtraMovementState.IsCrawling)
                {
                    _pauseMovementCountDown = beforeCrawlingPauseMovementDuration;
                }
                else if (IsGrounded && _previouslyExtraMovementState == ExtraMovementState.IsCrawling && inputData.ExtraMovementState != ExtraMovementState.IsCrawling)
                {
                    _pauseMovementCountDown = afterCrawlingPauseMovementDuration;
                }
                else
                {
                    if (_pauseMovementCountDown > 0f)
                        _pauseMovementCountDown -= deltaTime;
                }
                if (_pauseMovementCountDown > 0f)
                {
                    // Remove movement from movestate while pausing movement
                    inputData.MovementState &= ~(MovementState.Forward | MovementState.Backward | MovementState.Left | MovementState.Right);
                }
            }

            // Move by velocity before jump
            if (IsAirborne && doNotChangeVelocityWhileAirborne)
                tempMoveVelocity = _velocityBeforeAirborne;
            // Updating vertical movement (Fall, WASD inputs under water)
            if (IsUnderWater)
            {
                CurrentMoveSpeed = tempMaxMoveSpeed;

                // Move up to surface while under water
                if (autoSwimToSurface || inputData.MovementState.Has(MovementState.Up) || inputData.MovementState.Has(MovementState.Down) || Mathf.Abs(_moveDirection.y) > 0)
                {
                    if (autoSwimToSurface || inputData.MovementState.Has(MovementState.Up))
                        _moveDirection.y = 1f;
                    else if (inputData.MovementState.Has(MovementState.Down))
                        _moveDirection.y = -1f;
                    tempTargetPosition = Vector3.up * TargetWaterSurfaceY(_waterCollider);
                    tempCurrentPosition = Vector3.up * EntityTransform.position.y;
                    tempTargetDistance = Vector3.Distance(tempTargetPosition, tempCurrentPosition);
                    tempSqrMagnitude = (tempTargetPosition - tempCurrentPosition).sqrMagnitude;
                    tempPredictPosition = tempCurrentPosition + (Vector3.up * _moveDirection.y * CurrentMoveSpeed * deltaTime);
                    if (_moveDirection.y > 0f)
                    {
                        tempPredictSqrMagnitude = (tempPredictPosition - tempCurrentPosition).sqrMagnitude;
                        // Check `tempSqrMagnitude` against the `tempPredictSqrMagnitude`
                        // if `tempPredictSqrMagnitude` is greater than `tempSqrMagnitude`,
                        // rigidbody will reaching target and character is moving pass it,
                        // so adjust move speed by distance and time (with physic formula: v=s/t)
                        if (tempPredictSqrMagnitude >= tempSqrMagnitude && tempTargetDistance > 0f)
                            CurrentMoveSpeed *= tempTargetDistance / deltaTime / CurrentMoveSpeed;
                        if (CurrentMoveSpeed < 0.01f || tempTargetDistance <= 0f)
                        {
                            CurrentMoveSpeed = 0f;
                            // Force set move direction y to 0 to prevent swim move animation playing
                            if (autoSwimToSurface || inputData.MovementState.Has(MovementState.Up))
                                _moveDirection.y = 0f;
                        }
                    }
                    tempMoveVelocity.y = _moveDirection.y * CurrentMoveSpeed;
                }
            }
            else
            {
                // Update velocity while not under water
                tempMoveVelocity.y = _verticalVelocity;
            }

            // Don't applies velocity while using root motion
            if ((IsGrounded && (forceUseRootMotion || useRootMotionForMovement)) ||
                (IsAirborne && (forceUseRootMotion || useRootMotionForAirMovement)) ||
                (!IsGrounded && !IsAirborne && (forceUseRootMotion || useRootMotionForMovement)) ||
                (IsUnderWater && (forceUseRootMotion || useRootMotionUnderWater)))
            {
                tempMoveVelocity.x = 0;
                tempMoveVelocity.z = 0;
            }

            Vector3 platformMotion = Vector3.zero;
            if (!IsUnderWater)
            {
                // Apply platform motion
                if (_groundedTransform != null)
                {
                    platformMotion = (_groundedTransform.position - _previousPlatformPosition) / deltaTime;
                    _previousPlatformPosition = _groundedTransform.position;
                }
            }
            Vector3 snapToGroundMotion = EntityMovement.GetSnapToGroundMotion(tempMoveVelocity, platformMotion, forceMotion);
            _previousMovement = ((tempMoveVelocity + platformMotion + forceMotion) * deltaTime) + snapToGroundMotion;
            if (Entity.IsOwnerClientOrOwnedByServer && LadderComponent &&
                LadderComponent.TriggeredLadderEntry && !LadderComponent.ClimbingLadder &&
                LadderComponent.EnterExitState == EnterExitState.None &&
                LadderComponent.EnterExitState != EnterExitState.ConfirmAwaiting &&
                tempHorizontalMoveDirection.sqrMagnitude > 0f)
            {
                Vector3 dirToLadder = (LadderComponent.TriggeredLadderEntry.TipTransform.position.GetXZ() - Entity.EntityTransform.position.GetXZ()).normalized;
                float angle = Vector3.Angle(tempHorizontalMoveDirection, dirToLadder);
                if (angle < 15f)
                {
                    LadderComponent.EnterExitState = EnterExitState.ConfirmAwaiting;
                    LadderComponent.CallCmdEnterLadder();
                }
            }
            EntityMovement.Move(inputData.MovementState, inputData.ExtraMovementState, _previousMovement, deltaTime);
        }

        public void UpdateRotation(float deltaTime, ref MovementInputData3D inputData)
        {
            if (!Entity.CanTurn())
                return;

            Vector3 lookDirection = inputData.LookDirection;

            // Ignore zero direction to avoid errors
            if (lookDirection.sqrMagnitude > MIN_DIRECTION_SQR_MAGNITUDE)
            {
                // Get yaw from the look direction
                float targetYAngle = Quaternion.LookRotation(lookDirection, Vector3.up).eulerAngles.y;

                if (_yTurnSpeed <= 0f)
                {
                    _yAngle = targetYAngle;
                }
                else if (Mathf.Abs(Mathf.DeltaAngle(_yAngle, targetYAngle)) > 1f)
                {
                    // Smooth rotation towards target angle
                    _yAngle = Mathf.LerpAngle(_yAngle, targetYAngle, _yTurnSpeed * deltaTime);
                }
            }

            _lookRotationApplied = true;
            RotateY();
        }

        public void FixSwimUpPosition(float deltaTime)
        {
            if (!CanSimulateMovement())
                return;

            if (!IsGrounded && IsUnderWater && _previousMovement.y > 0f)
            {
                Vector3 tempTargetPosition = Vector3.up * TargetWaterSurfaceY(_waterCollider);
                if (Mathf.Abs(EntityTransform.position.y - tempTargetPosition.y) < 0.05f)
                    EntityMovement.SetPosition(new Vector3(EntityTransform.position.x, tempTargetPosition.y, EntityTransform.position.z));
            }
        }

        protected void RotateY()
        {
            EntityMovement.RotateY(_yAngle);
        }

        protected void SetMovePaths(Vector3 position, bool useNavMesh)
        {
            if (useNavMesh)
            {
                NavMeshPath navPath = new NavMeshPath();
                if (NavMesh.SamplePosition(position, out NavMeshHit navHit, 5f, NavMesh.AllAreas) &&
                    NavMesh.CalculatePath(EntityTransform.position, navHit.position, NavMesh.AllAreas, navPath))
                {
                    NavPaths = new Queue<Vector3>(navPath.corners);
                    // Dequeue first path it's not require for future movement
                    NavPaths.Dequeue();
                }
            }
            else
            {
                // If not use nav mesh, just move to position by direction
                NavPaths = new Queue<Vector3>();
                NavPaths.Enqueue(position);
            }
        }

        protected float CalculateMaxFallVelocity()
        {
            return maxFallVelocity * Entity.GetGravityRate();
        }

        protected float CalculateGravity()
        {
            return gravity * Entity.GetGravityRate();
        }

        protected float CalculateJumpVerticalSpeed()
        {
            // From the jump height and gravity we deduce the upwards speed 
            // for the character to reach at the apex.
            return Mathf.Sqrt(2f * (jumpHeight + Entity.GetJumpHeight()) * CalculateGravity());
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicLayers.Water)
            {
                // Enter water
                _waterCollider = other;
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == PhysicLayers.Water && _waterCollider == other)
            {
                // Exit water
                _waterCollider = null;
            }
        }

        public void OnControllerColliderHit(Vector3 hitPoint, Transform hitTransform)
        {
            if (IsGrounded)
            {
                if (EntityTransform.position.y >= hitPoint.y)
                {
                    _groundedTransform = hitTransform;
                    _previousPlatformPosition = _groundedTransform.position;
                    return;
                }
            }
            _groundedTransform = null;
        }

        public bool WriteClientState(uint writeTick, NetDataWriter writer, out bool shouldSendReliably)
        {
            if (_clientTeleportState.Has(MovementTeleportState.Responding))
            {
                shouldSendReliably = true;
                writer.Put((byte)_clientTeleportState);
                _clientTeleportState = MovementTeleportState.None;
                return true;
            }
            shouldSendReliably = false;
            switch (movementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    if (_inputBuffers.Count == 0 && _clientTeleportState == MovementTeleportState.None)
                        return false;
                    writer.Put((byte)_clientTeleportState);
                    writer.Put((byte)_inputBuffers.Count);
                    for (int i = 0; i < _inputBuffers.Count; ++i)
                    {
                        writer.Put(_inputBuffers.Values[i]);
                    }
                    return true;
                default:
                    if (_syncBuffers.Count == 0 && _clientTeleportState == MovementTeleportState.None)
                        return false;
                    writer.Put((byte)_clientTeleportState);
                    writer.Put((byte)_syncBuffers.Count);
                    for (int i = 0; i < _syncBuffers.Count; ++i)
                    {
                        writer.Put(_syncBuffers.Values[i]);
                    }
                    return true;
            }
        }

        public bool WriteServerState(uint writeTick, NetDataWriter writer, out bool shouldSendReliably)
        {
            if (_serverTeleportState.Has(MovementTeleportState.Requesting))
            {
                _syncBuffers.Clear();
                shouldSendReliably = true;
                writer.Put((byte)_serverTeleportState);
                writer.Put(EntityTransform.position.x);
                writer.Put(EntityTransform.position.y);
                writer.Put(EntityTransform.position.z);
                writer.PutPackedUShort(Mathf.FloatToHalf(EntityTransform.eulerAngles.y));
                if (!IsOwnerClientOrOwnedByServer)
                    _serverTeleportState = MovementTeleportState.WaitingForResponse;
                else
                    _serverTeleportState = MovementTeleportState.None;
                return true;
            }
            shouldSendReliably = false;
            if (_syncBuffers.Count == 0 && _serverTeleportState == MovementTeleportState.None)
                return false;
            writer.Put((byte)_serverTeleportState);
            writer.Put((byte)_syncBuffers.Count);
            for (int i = 0; i < _syncBuffers.Count; ++i)
            {
                writer.Put(_syncBuffers.Values[i]);
            }
            return true;
        }

        public void ReadClientStateAtServer(uint peerTick, NetDataReader reader)
        {
            MovementTeleportState movementTeleportState = (MovementTeleportState)reader.GetByte();
            if (movementTeleportState.Has(MovementTeleportState.Responding))
            {
                _teleportRespondedTick = peerTick;
                _serverTeleportState = MovementTeleportState.Responded;
                return;
            }
            if (_serverTeleportState.Has(MovementTeleportState.WaitingForResponse))
            {
                // Wait for teleportation confirmation
                return;
            }
            if (_serverTeleportState == MovementTeleportState.Responded)
                ResetBuffersAndStates();
            byte size;
            int maxBuffers;
            switch (movementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    size = reader.GetByte();
                    if (size == 0)
                    {
                        _serverTeleportState = MovementTeleportState.None;
                        return;
                    }
                    MovementInputData3D[] inputBuffers = ArrayPool<MovementInputData3D>.Shared.Rent(size);
                    for (byte i = 0; i < size; ++i)
                    {
                        inputBuffers[i] = reader.Get<MovementInputData3D>();
                    }
                    if (!IsOwnerClient)
                    {
                        maxBuffers = _serverTeleportState == MovementTeleportState.Responded ? 1 : 30;
                        StoreInputBuffers(inputBuffers, size, _teleportRespondedTick, maxBuffers);
                        uint simTick = _inputBuffers.Keys[_inputBuffers.Count - 1];
                        if (Entity.Player != null)
                            simTick += LogicUpdater.TimeToTick(Entity.Player.Rtt / 2, _logicUpdater.DeltaTime);
                        SetupSimulationTick(simTick);
                    }
                    ArrayPool<MovementInputData3D>.Shared.Return(inputBuffers);
                    _serverTeleportState = MovementTeleportState.None;
                    break;
                default:
                    size = reader.GetByte();
                    if (size == 0)
                    {
                        _serverTeleportState = MovementTeleportState.None;
                        return;
                    }
                    MovementSyncData3D[] interpoationBuffers = ArrayPool<MovementSyncData3D>.Shared.Rent(size);
                    for (byte i = 0; i < size; ++i)
                    {
                        interpoationBuffers[i] = reader.Get<MovementSyncData3D>();
                        StoreSyncBuffer(interpoationBuffers[i], 3);
                    }
                    maxBuffers = _serverTeleportState == MovementTeleportState.Responded ? 1 : 30;
                    StoreInterpolateBuffers(interpoationBuffers, size, _teleportRespondedTick, maxBuffers);
                    uint interpTick = _interpBuffers.Keys[_interpBuffers.Count - 1];
                    if (Entity.Player != null)
                        interpTick += LogicUpdater.TimeToTick(Entity.Player.Rtt / 2, _logicUpdater.DeltaTime);
                    SetupInterpolationTick(interpTick);
                    ArrayPool<MovementSyncData3D>.Shared.Return(interpoationBuffers);
                    _serverTeleportState = MovementTeleportState.None;
                    // Broadcast to other clients immediately
                    Entity.SendServerState(_logicUpdater.LocalTick);
                    break;
            }
        }

        public async void ReadServerStateAtClient(uint peerTick, NetDataReader reader)
        {
            MovementTeleportState movementTeleportState = (MovementTeleportState)reader.GetByte();
            if (movementTeleportState.Has(MovementTeleportState.Requesting))
            {
                Vector3 position = new Vector3(
                    reader.GetFloat(),
                    reader.GetFloat(),
                    reader.GetFloat());
                float rotation = Mathf.HalfToFloat(reader.GetPackedUShort());
                bool stillMoveAfterTeleport = movementTeleportState.Has(MovementTeleportState.StillMoveAfterTeleport);
                if (!IsServer)
                {
                    _interpBuffers.Clear();
                    await OnTeleport(position, Quaternion.Euler(0f, rotation, 0f), stillMoveAfterTeleport);
                }
                return;
            }
            byte size = reader.GetByte();
            if (size == 0)
                return;
            MovementSyncData3D[] interpoationBuffers = ArrayPool<MovementSyncData3D>.Shared.Rent(size);
            for (byte i = 0; i < size; ++i)
            {
                interpoationBuffers[i] = reader.Get<MovementSyncData3D>();
            }
            if (!IsServer)
            {
                StoreInterpolateBuffers(interpoationBuffers, size, 0, 30);
                SetupInterpolationTick(_interpBuffers.Keys[_interpBuffers.Count - 1]);
            }
            ArrayPool<MovementSyncData3D>.Shared.Return(interpoationBuffers);
        }

        protected async UniTask OnTeleport(Vector3 position, Quaternion rotation, bool stillMoveAfterTeleport)
        {
            if (Vector3.Distance(position, EntityTransform.position) <= MIN_DISTANCE_TO_TELEPORT)
            {
                // Too close to teleport
                return;
            }
            // Prepare before move
            _verticalVelocity = 0;
            if (!stillMoveAfterTeleport)
            {
                NavPaths = null;
            }
            if (IsServer && !IsOwnerClientOrOwnedByServer)
            {
                _serverTeleportState = MovementTeleportState.WaitingForResponse;
            }
            if (TeleportPreparer != null)
            {
                await TeleportPreparer.PrepareToTeleport(position, rotation);
            }
            // Move character to target position
            _verticalVelocity = 0;
            if (!stillMoveAfterTeleport)
            {
                NavPaths = null;
            }
            _lastTeleportFrame = Time.frameCount;
            EntityTransform.position = position;
            EntityMovement.SetPosition(position);
            CurrentGameManager.ShouldPhysicSyncTransforms = true;
            TurnImmediately(rotation.eulerAngles.y);
            _previousPosition = EntityTransform.position;
            // Prepare teleporation states
            if (IsServer && !IsOwnerClient)
            {
                _serverTeleportState = MovementTeleportState.Requesting;
                if (stillMoveAfterTeleport)
                    _serverTeleportState |= MovementTeleportState.StillMoveAfterTeleport;
                if (!IsOwnedByServer)
                    _serverTeleportState |= MovementTeleportState.WaitingForResponse;
            }
            if (!IsServer && IsOwnerClient)
            {
                _clientTeleportState = MovementTeleportState.Responding;
            }
        }

        public bool CanSimulateMovement()
        {
            switch (movementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    return Entity.IsServer || Entity.IsOwnerClientOrOwnedByServer;
                default:
                    return Entity.IsOwnerClientOrOwnedByServer;
            }
        }

        public bool UseRootMotion()
        {
            return alwaysUseRootMotion || useRootMotionForMovement || useRootMotionForAirMovement || useRootMotionForJump || useRootMotionForFall || useRootMotionUnderWater || useRootMotionClimbing;
        }

        public void TurnImmediately(float yAngle)
        {
            _yAngle = yAngle;
            RotateY();
        }

        public async UniTask WaitClientTeleportConfirm()
        {
            while (this != null && _serverTeleportState != MovementTeleportState.None)
            {
                await UniTask.Delay(100);
            }
        }

        public bool IsWaitingClientTeleportConfirm()
        {
            return _serverTeleportState != MovementTeleportState.None;
        }
    }
}
