using System.Buffers;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LiteNetLib.Utils;
using LiteNetLibManager;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.AI;

namespace MultiplayerARPG
{
    /// <summary>
    /// If `movementSecure` is `NotSecure`
    /// - Owner client will simulate movement (stored in `_inputBuffers`, storing in `KeyMovement` and `PointClickMovement` functions)
    /// - Owner client will sync transform to server (stored in `_syncBuffers`, storing in `LogicUpdater_OnTick` function)
    /// - Server will sync transform to clients
    /// - Server and other clients will interpolate transform by synced buffers (stored in `_syncBuffers`)
    /// If `movementSecure` is `ServerAuthoritative`
    /// - Owner client will simulate movement (stored in `_inputBuffers`, storing in `KeyMovement` and `PointClickMovement` functions)
    /// - Owner client will send inputs to server
    /// - Server will simulate movement (stored in `_inputBuffers`)
    /// - Server will sync transform to clients
    /// - Other clients will interpolate transform by synced buffers (stored in `_syncBuffers`)
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshEntityMovement : BaseNetworkedGameEntityComponent<BaseGameEntity>, IEntityMovementComponent
    {
        protected const int TICK_COUNT_FOR_INTERPOLATION = 2;
        protected const float MIN_MAGNITUDE_TO_DETERMINE_MOVING = 0.01f;
        protected const float MIN_DIRECTION_SQR_MAGNITUDE = 0.0001f;
        protected const float MIN_DISTANCE_TO_TELEPORT = 0.1f;
        protected static readonly ProfilerMarker s_UpdateProfilerMarker = new ProfilerMarker("NavMeshEntityMovement - Update");

        [Header("Networking Settings")]
        public MovementSecure movementSecure = MovementSecure.NotSecure;
        [Tooltip("If distance between current frame and previous frame is greater than this value, then it will determine that changes occurs and will sync transform later")]
        [Min(0.01f)]
        public float positionThreshold = 0.01f;
        [Tooltip("If angle between current frame and previous frame is greater than this value, then it will determine that changes occurs and will sync transform later")]
        [Min(0.01f)]
        public float eulerAnglesThreshold = 1f;
        [Tooltip("Keep alive ticks before it is stop syncing (after has no changes)")]
        public int keepAliveTicks = 10;

        [Header("Movement Settings")]
        public ObstacleAvoidanceType obstacleAvoidanceWhileMoving = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        public ObstacleAvoidanceType obstacleAvoidanceWhileStationary = ObstacleAvoidanceType.NoObstacleAvoidance;

        [Header("Dashing")]
        public EntityMovementForceApplierData dashingForceApplier = EntityMovementForceApplierData.CreateDefault();

        public NavMeshAgent CacheNavMeshAgent { get; protected set; }
        public IEntityTeleportPreparer TeleportPreparer { get; protected set; }
        public bool IsPreparingToTeleport { get { return TeleportPreparer != null && TeleportPreparer.IsPreparingToTeleport; } }
        public float StoppingDistance
        {
            get { return CacheNavMeshAgent.stoppingDistance; }
        }
        public MovementState MovementState { get; protected set; }
        public ExtraMovementState ExtraMovementState { get; protected set; }
        public DirectionVector2 Direction2D { get { return Vector2.down; } set { } }
        public float CurrentMoveSpeed { get { return CacheNavMeshAgent.isStopped ? 0f : CacheNavMeshAgent.speed; } }

        // Inputs
        protected MovementInputData3D _currentInput;
        protected int? _resetInputFrame = null;
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
        protected MovementTeleportState _serverTeleportState;
        protected MovementTeleportState _clientTeleportState;

        // Move simulate codes
        protected Vector3? _prevPointClickPosition = null;

        // Force simulation
        protected readonly List<EntityMovementForceApplier> _movementForceAppliers = new List<EntityMovementForceApplier>();
        protected IEntityMovementForceUpdateListener[] _forceUpdateListeners;

        // Turn simulate codes
        protected bool _lookRotationApplied;
        protected float _yAngle;
        protected float _yTurnSpeed;

        public override void EntityAwake()
        {
            // Prepare nav mesh agent component
            CacheNavMeshAgent = gameObject.GetOrAddComponent<NavMeshAgent>();
            TeleportPreparer = gameObject.GetComponent<IEntityTeleportPreparer>();
            _forceUpdateListeners = gameObject.GetComponents<IEntityMovementForceUpdateListener>();
            // Disable unused component
            LiteNetLibTransform disablingComp = gameObject.GetComponent<LiteNetLibTransform>();
            if (disablingComp != null)
            {
                Logging.LogWarning(nameof(NavMeshEntityMovement), "You can remove `LiteNetLibTransform` component from game entity, it's not being used anymore [" + name + "]");
                disablingComp.enabled = false;
            }
            // Setup
            Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
            if (rigidBody != null)
            {
                rigidBody.useGravity = false;
                rigidBody.isKinematic = true;
            }
            _currentInput = new MovementInputData3D();
            StopMoveFunction();
        }

        public override void EntityStart()
        {
            _clientTeleportState = MovementTeleportState.Responding;
        }

        public override void OnSetOwnerClient(bool isOwnerClient)
        {
            CacheNavMeshAgent.enabled = CanSimulateMovement();
            ResetBuffersAndStates();
            // Force setup sim tick
            if (IsOwnerClientOrOwnedByServer)
                _simTick = Manager.LocalTick;
        }

        public override void ComponentOnEnable()
        {
            CacheNavMeshAgent.enabled = CanSimulateMovement();
        }

        public override void ComponentOnDisable()
        {
            CacheNavMeshAgent.enabled = false;
        }

        public override void OnIdentityInitialize()
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

        public override void EntityOnDestroy()
        {
            if (_logicUpdater != null)
                _logicUpdater.OnTick -= LogicUpdater_OnTick;
        }

        protected void LogicUpdater_OnTick(LogicUpdater updater)
        {
            _simTick++;
            _interpTick++;

            // Storing sync buffers, server will send to other clients, owner client will send to server
            if (IsServer || (IsOwnerClient && movementSecure == MovementSecure.NotSecure))
            {
                float rotation = EntityTransform.eulerAngles.y;
                MovementSyncData3D syncData = _prevSyncData;
                bool changed =
                    Vector3.Distance(EntityTransform.position, syncData.Position) > positionThreshold ||
                    MovementState != syncData.MovementState || ExtraMovementState != syncData.ExtraMovementState ||
                    Mathf.Abs(rotation - syncData.Rotation) > eulerAnglesThreshold;
                bool keepAlive = updater.LocalTick - syncData.Tick >= keepAliveTicks;

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
                StoreSyncBuffer(syncData);
            }

            if (IsOwnerClientOrOwnedByServer)
            {
                _currentInput.Tick = _simTick - 1;
                StoreInputBuffer(_currentInput);
                _resetInputFrame = Time.frameCount;
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

        public bool FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
        {
            result = fromPosition;
            float findDist = 1f;
            NavMeshHit navHit;
            while (!NavMesh.SamplePosition(fromPosition, out navHit, findDist, NavMesh.AllAreas))
            {
                findDist += 1f;
                if (findDist > findDistance)
                    return false;
            }
            result = navHit.position;
            return true;
        }

        public Bounds GetMovementBounds()
        {
            Vector3 agentPosition = transform.position;
            Vector3 lossyScale = transform.lossyScale;

            // Calculate the scaled extents using lossy scale
            float scaledRadius = CacheNavMeshAgent.radius * Mathf.Max(lossyScale.x, lossyScale.z);
            float scaledHeight = CacheNavMeshAgent.height * lossyScale.y;
            float baseOffset = CacheNavMeshAgent.baseOffset * lossyScale.y;

            // Adjust the center to include the baseOffset and scale
            Vector3 center = new Vector3(agentPosition.x, agentPosition.y + baseOffset + (scaledHeight * 0.5f), agentPosition.z);
            Vector3 size = new Vector3(scaledRadius * 2, scaledHeight, scaledRadius * 2);
            return new Bounds(center, size);
        }

        protected void SetMovePaths(Vector3 position)
        {
            if (!Entity.CanMove())
                return;
            CacheNavMeshAgent.updatePosition = true;
            CacheNavMeshAgent.updateRotation = false;
            if (CacheNavMeshAgent.isOnNavMesh)
            {
                CacheNavMeshAgent.isStopped = false;
                NavMeshPath path = new NavMeshPath();
                NavMesh.CalculatePath(EntityTransform.position, position, CacheNavMeshAgent.areaMask, path);
                CacheNavMeshAgent.SetPath(path);
            }
        }

        public void SetSmoothTurnSpeed(float turnDuration)
        {
            _yTurnSpeed = turnDuration;
        }

        public float GetSmoothTurnSpeed()
        {
            return _yTurnSpeed;
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

        public virtual void PointClickMovement(Vector3 position)
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
            _currentInput.LookDirection = lookDirection;
            _lookRotationApplied = false;
            if (immediately && Entity.CanTurn())
                TurnImmediately(rotation.eulerAngles.y);
        }

        public Quaternion GetLookRotation()
        {
            return Quaternion.Euler(0f, EntityTransform.eulerAngles.y, 0f);
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

        protected void StopMoveFunction()
        {
            CacheNavMeshAgent.updatePosition = false;
            CacheNavMeshAgent.updateRotation = false;
            if (CacheNavMeshAgent.isOnNavMesh)
                CacheNavMeshAgent.isStopped = true;
            MovementState = MovementState.IsGrounded;
            ExtraMovementState = ExtraMovementState.None;
        }

        public async void Teleport(Vector3 position, Quaternion rotation, bool stillMoveAfterTeleport)
        {
            if (!IsServer)
            {
                Logging.LogWarning(nameof(NavMeshEntityMovement), $"Teleport function shouldn't be called at client {name}");
                return;
            }
            if (_serverTeleportState.Has(MovementTeleportState.WaitingForResponse))
            {
                // Still waiting for teleport responding
                return;
            }
            await OnTeleport(position, rotation, stillMoveAfterTeleport);
        }

        protected virtual async UniTask OnTeleport(Vector3 position, Quaternion rotation, bool stillMoveAfterTeleport)
        {
            if (Vector3.Distance(position, EntityTransform.position) <= MIN_DISTANCE_TO_TELEPORT)
            {
                // Too close to teleport
                return;
            }
            // Prepare before move
            if (IsServer && !IsOwnerClientOrOwnedByServer)
            {
                _serverTeleportState = MovementTeleportState.WaitingForResponse;
            }
            if (TeleportPreparer != null)
            {
                await TeleportPreparer.PrepareToTeleport(position, rotation);
            }
            // Move character to target position
            Vector3 beforeWarpDest = CacheNavMeshAgent.destination;
            CacheNavMeshAgent.Warp(position);
            if (!stillMoveAfterTeleport && CacheNavMeshAgent.isOnNavMesh)
            {
                CacheNavMeshAgent.isStopped = true;
            }
            if (stillMoveAfterTeleport && CacheNavMeshAgent.isOnNavMesh)
            {
                CacheNavMeshAgent.SetDestination(beforeWarpDest);
            }
            TurnImmediately(rotation.eulerAngles.y);
            // Prepare teleporation states
            if (IsServer && !IsOwnerClientOrOwnedByServer)
            {
                _serverTeleportState = MovementTeleportState.Requesting;
                if (stillMoveAfterTeleport)
                    _serverTeleportState |= MovementTeleportState.StillMoveAfterTeleport;
                _serverTeleportState |= MovementTeleportState.WaitingForResponse;
            }
            if (!IsServer && IsOwnerClient)
            {
                _clientTeleportState = MovementTeleportState.Responding;
            }
        }

        public async UniTask WaitClientTeleportConfirm()
        {
            while (this != null && _serverTeleportState.Has(MovementTeleportState.WaitingForResponse))
            {
                await UniTask.Delay(100);
            }
        }

        public bool IsWaitingClientTeleportConfirm()
        {
            return _serverTeleportState.Has(MovementTeleportState.WaitingForResponse);
        }

        protected float GetPathRemainingDistance()
        {
            if (CacheNavMeshAgent.pathPending ||
                CacheNavMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid ||
                CacheNavMeshAgent.path.corners.Length == 0)
                return -1f;

            float distance = 0.0f;
            for (int i = 0; i < CacheNavMeshAgent.path.corners.Length - 1; ++i)
            {
                distance += Vector3.Distance(CacheNavMeshAgent.path.corners[i], CacheNavMeshAgent.path.corners[i + 1]);
            }

            return distance;
        }

        public override void EntityUpdate()
        {
            // Simulate movement by inputs if it can predict movement
            if (CanSimulateMovement())
            {
                SimulateMovementFromInput(Time.deltaTime);
                if (_resetInputFrame.HasValue && Time.frameCount >= _resetInputFrame.Value)
                {
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
            if (IsServer && _serverTeleportState.Has(MovementTeleportState.WaitingForResponse))
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
            if (IsServer && _serverTeleportState.Has(MovementTeleportState.WaitingForResponse))
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

            float t = Mathf.InverseLerp(_startInterpTime, _endInterpTime, currentTime);
            EntityTransform.position = Vector3.Lerp(_interpFromData.Position, _interpToData.Position, t);
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

        protected void StoreInputBuffer(MovementInputData3D entry, int maxBuffers = 3)
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

        protected void StoreInputBuffers(MovementInputData3D[] data, int size, int maxBuffers = 3)
        {
            for (int i = 0; i < size; ++i)
            {
                MovementInputData3D entry = data[i];
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

        protected void StoreSyncBuffer(MovementSyncData3D entry, int maxBuffers = 3)
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

        protected void StoreInterpolateBuffers(MovementSyncData3D[] data, int size, int maxBuffers = 3)
        {
            for (int i = 0; i < size; ++i)
            {
                MovementSyncData3D entry = data[i];
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

            bool isDashing = inputData.MovementState.Has(MovementState.IsDash);
            Vector3 tempInputDirection = Vector3.zero;

            if (!inputData.IsPointClick)
            {
                tempInputDirection = inputData.MoveDirection;
            }

            if (inputData.IsPointClick && (!_prevPointClickPosition.HasValue || Vector3.Distance(_prevPointClickPosition.Value, inputData.Position) > 0.01f))
            {
                _prevPointClickPosition = inputData.Position;
                SetMovePaths(inputData.Position);
            }

            MovementState = inputData.MovementState;
            ExtraMovementState = this.ValidateExtraMovementState(MovementState, inputData.ExtraMovementState);
            isDashing = inputData.MovementState.Has(MovementState.IsDash);

            ApplyMovementForceMode replaceMovementForceApplierMode = ApplyMovementForceMode.Default;
            // Update force applying
            // Dashing
            if (isDashing)
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
                replaceMovementForceApplierMode = replaceMovementForceApplier.Mode;
                // Force turn to dashed direction
                inputData.LookDirection = replaceMovementForceApplier.Direction;
                // Change move speed to dash force
                if (CacheNavMeshAgent.hasPath)
                {
                    CacheNavMeshAgent.isStopped = true;
                }
                if (CacheNavMeshAgent.isOnNavMesh)
                {
                    CacheNavMeshAgent.Move(replaceMovementForceApplier.CurrentSpeed * replaceMovementForceApplier.Direction * deltaTime);
                }
            }

            if (forceMotion.sqrMagnitude > MIN_DIRECTION_SQR_MAGNITUDE && CacheNavMeshAgent.isOnNavMesh)
            {
                CacheNavMeshAgent.Move(forceMotion * deltaTime);
            }

            bool isStationary = !CacheNavMeshAgent.isOnNavMesh || CacheNavMeshAgent.isStopped || GetPathRemainingDistance() <= CacheNavMeshAgent.stoppingDistance;

            CacheNavMeshAgent.obstacleAvoidanceType = isStationary ? obstacleAvoidanceWhileStationary : obstacleAvoidanceWhileMoving;
            MovementState = MovementState.IsGrounded;

            if (replaceMovementForceApplierMode == ApplyMovementForceMode.Dash)
                MovementState |= MovementState.IsDash;

            if (!replaceMovementForceApplierMode.IsReplaceMovement())
            {
                if (tempInputDirection.sqrMagnitude > MIN_DIRECTION_SQR_MAGNITUDE)
                {
                    // Moving by WASD keys
                    MovementState |= MovementState.Forward;
                    ExtraMovementState = this.ValidateExtraMovementState(MovementState, inputData.ExtraMovementState);
                    CacheNavMeshAgent.speed = Entity.GetMoveSpeed(MovementState, ExtraMovementState);
                    CacheNavMeshAgent.updatePosition = true;
                    CacheNavMeshAgent.updateRotation = false;
                    if (CacheNavMeshAgent.isOnNavMesh)
                        CacheNavMeshAgent.isStopped = true;
                    CacheNavMeshAgent.Move(tempInputDirection * CacheNavMeshAgent.speed * deltaTime);
                    // Turn character to destination
                    if (_lookRotationApplied && Entity.CanTurn())
                        inputData.LookDirection = tempInputDirection;
                }
                else
                {
                    // Moving by clicked position
                    bool isMoving = CacheNavMeshAgent.velocity.magnitude > MIN_MAGNITUDE_TO_DETERMINE_MOVING;
                    MovementState |= isMoving ? MovementState.Forward : MovementState.None;
                    ExtraMovementState = this.ValidateExtraMovementState(MovementState, inputData.ExtraMovementState);
                    CacheNavMeshAgent.speed = Entity.GetMoveSpeed(MovementState, ExtraMovementState);
                    // Turn character to destination
                    if (isMoving && _lookRotationApplied && Entity.CanTurn())
                        inputData.LookDirection = CacheNavMeshAgent.velocity.normalized;
                }
            }
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

        protected void RotateY()
        {
            EntityTransform.rotation = Quaternion.Euler(0f, _yAngle, 0f);
        }

        public bool WriteClientState(long writeTimestamp, NetDataWriter writer, out bool shouldSendReliably)
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
                    if (_inputBuffers.Count == 0)
                        return false;
                    writer.Put((byte)_clientTeleportState);
                    writer.Put((byte)_inputBuffers.Count);
                    for (int i = 0; i < _inputBuffers.Count; ++i)
                    {
                        writer.Put(_inputBuffers.Values[i]);
                    }
                    return true;
                default:
                    if (_syncBuffers.Count == 0)
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

        public bool WriteServerState(long writeTimestamp, NetDataWriter writer, out bool shouldSendReliably)
        {
            if (_serverTeleportState.Has(MovementTeleportState.Requesting))
            {
                shouldSendReliably = true;
                writer.Put((byte)_serverTeleportState);
                writer.Put(EntityTransform.position.x);
                writer.Put(EntityTransform.position.y);
                writer.Put(EntityTransform.position.z);
                writer.PutPackedUShort(Mathf.FloatToHalf(EntityTransform.eulerAngles.y));
                _serverTeleportState = MovementTeleportState.WaitingForResponse;
                return true;
            }
            shouldSendReliably = false;
            if (_syncBuffers.Count == 0)
                return false;
            writer.Put((byte)_serverTeleportState);
            writer.Put((byte)_syncBuffers.Count);
            for (int i = 0; i < _syncBuffers.Count; ++i)
            {
                writer.Put(_syncBuffers.Values[i]);
            }
            return true;
        }

        public void ReadClientStateAtServer(long peerTimestamp, NetDataReader reader)
        {
            MovementTeleportState movementTeleportState = (MovementTeleportState)reader.GetByte();
            if (movementTeleportState.Has(MovementTeleportState.Responding))
            {
                _serverTeleportState = MovementTeleportState.None;
                return;
            }
            byte size;
            switch (movementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    size = reader.GetByte();
                    if (size == 0)
                        return;
                    MovementInputData3D[] inputBuffers = ArrayPool<MovementInputData3D>.Shared.Rent(size);
                    for (byte i = 0; i < size; ++i)
                    {
                        inputBuffers[i] = reader.Get<MovementInputData3D>();
                    }
                    if (!IsOwnerClient)
                    {
                        StoreInputBuffers(inputBuffers, size, 30);
                        uint simTick = _inputBuffers.Keys[_inputBuffers.Count - 1];
                        if (Player != null)
                            simTick += LogicUpdater.TimeToTick(Player.Rtt / 2, _logicUpdater.DeltaTime);
                        SetupSimulationTick(simTick);
                    }
                    ArrayPool<MovementInputData3D>.Shared.Return(inputBuffers);
                    break;
                default:
                    size = reader.GetByte();
                    if (size == 0)
                        return;
                    MovementSyncData3D[] interpoationBuffers = ArrayPool<MovementSyncData3D>.Shared.Rent(size);
                    for (byte i = 0; i < size; ++i)
                    {
                        interpoationBuffers[i] = reader.Get<MovementSyncData3D>();
                    }
                    StoreInterpolateBuffers(interpoationBuffers, size, 30);
                    uint interpTick = _interpBuffers.Keys[_interpBuffers.Count - 1];
                    if (Player != null)
                        interpTick += LogicUpdater.TimeToTick(Player.Rtt / 2, _logicUpdater.DeltaTime);
                    SetupInterpolationTick(interpTick);
                    ArrayPool<MovementSyncData3D>.Shared.Return(interpoationBuffers);
                    break;
            }
        }

        public async void ReadServerStateAtClient(long peerTimestamp, NetDataReader reader)
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
                    await OnTeleport(position, Quaternion.Euler(0f, rotation, 0f), stillMoveAfterTeleport);
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
                StoreInterpolateBuffers(interpoationBuffers, size, 30);
                SetupInterpolationTick(_interpBuffers.Keys[_interpBuffers.Count - 1]);
            }
            ArrayPool<MovementSyncData3D>.Shared.Return(interpoationBuffers);
        }

        protected virtual Vector3 GetMoveablePosition(Vector3 oldPos, Vector3 newPos, float clientMoveDist, float moveableDist)
        {
            Vector3 dir = (newPos.GetXZ() - oldPos.GetXZ()).normalized;
            Vector3 deltaMove = dir * Mathf.Min(clientMoveDist, moveableDist);
            return oldPos + deltaMove;
        }

        public void TurnImmediately(float yAngle)
        {
            _yAngle = yAngle;
            RotateY();
        }

        public bool AllowToJump()
        {
            return false;
        }

        public bool AllowToDash()
        {
            return true;
        }

        public bool AllowToCrouch()
        {
            return true;
        }

        public bool AllowToCrawl()
        {
            return true;
        }
    }
}