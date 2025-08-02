using Cysharp.Threading.Tasks;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;

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
    [RequireComponent(typeof(Rigidbody2D))]
    public class RigidBodyEntityMovement2D : BaseNetworkedGameEntityComponent<BaseGameEntity>, IEntityMovementComponent
    {
        public const int TICK_COUNT_FOR_INTERPOLATION = 2;

        public struct InputData : INetSerializable
        {
            public uint Tick;
            public bool IsStopped;
            public bool IsPointClick;
            public Vector2 Position;
            public MovementState MovementState;
            public ExtraMovementState ExtraMovementState;
            public DirectionVector2 Direction2D;

            public void Deserialize(NetDataReader reader)
            {
                Tick = reader.GetPackedUInt();
                IsStopped = reader.GetBool();
                if (IsStopped)
                    return;
                IsPointClick = reader.GetBool();
                if (IsPointClick)
                {
                    Position = new Vector2(
                        reader.GetFloat(),
                        reader.GetFloat());
                }
                else
                {
                    MovementState = (MovementState)reader.GetByte();
                    ExtraMovementState = (ExtraMovementState)reader.GetByte();
                    Direction2D = reader.Get<DirectionVector2>();
                }
            }

            public void Serialize(NetDataWriter writer)
            {
                writer.PutPackedUInt(Tick);
                writer.Put(IsStopped);
                if (IsStopped)
                    return;
                writer.Put(IsPointClick);
                if (IsPointClick)
                {
                    writer.Put(Position.x);
                    writer.Put(Position.y);
                }
                else
                {
                    writer.Put((byte)MovementState);
                    writer.Put((byte)ExtraMovementState);
                    writer.Put(Direction2D);
                }
            }
        }

        public struct SyncData : INetSerializable
        {
            public uint Tick;
            public Vector2 Position;
            public MovementState MovementState;
            public ExtraMovementState ExtraMovementState;
            public DirectionVector2 Direction2D;

            public void Deserialize(NetDataReader reader)
            {
                Tick = reader.GetPackedUInt();
                Position = new Vector2(
                    reader.GetFloat(),
                    reader.GetFloat());
                MovementState = (MovementState)reader.GetByte();
                ExtraMovementState = (ExtraMovementState)reader.GetByte();
                Direction2D = reader.Get<DirectionVector2>();
            }

            public void Serialize(NetDataWriter writer)
            {
                writer.PutPackedUInt(Tick);
                writer.Put(Position.x);
                writer.Put(Position.y);
                writer.Put((byte)MovementState);
                writer.Put((byte)ExtraMovementState);
                writer.Put(Direction2D);
            }
        }

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
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;
        public float StoppingDistance => stoppingDistance;

        [Header("Dashing")]
        public EntityMovementForceApplierData dashingForceApplier = EntityMovementForceApplierData.CreateDefault();

        public Rigidbody2D CacheRigidbody2D { get; private set; }
        public MovementState MovementState { get; protected set; }
        public ExtraMovementState ExtraMovementState { get; protected set; }
        public DirectionVector2 Direction2D { get; set; }
        public float CurrentMoveSpeed { get; private set; }
        public Queue<Vector2> NavPaths { get; protected set; }
        public bool HasNavPaths
        {
            get { return NavPaths != null && NavPaths.Count > 0; }
        }

        // Input codes
        private SortedList<uint, InputData> _inputBuffers = new SortedList<uint, InputData>();
        private SortedList<uint, SyncData> _syncBuffers = new SortedList<uint, SyncData>();
        private SortedList<uint, SyncData> _interpBuffers = new SortedList<uint, SyncData>();
        private bool _hasSimTick = false;
        private uint _simTick = 0;
        private bool _hasInterpTick = false;
        private uint _interpTick = 0;
        public uint RenderTick => _interpTick - TICK_COUNT_FOR_INTERPOLATION;

        protected bool _isServerWaitingTeleportConfirm;
        protected SyncData _prevSyncData;
        protected SyncData _interpFromSyncData;
        protected SyncData _interpToSyncData;
        protected float _startInterpTime;
        protected float _endInterpTime;

        // Force simulation
        protected readonly List<EntityMovementForceApplier> _movementForceAppliers = new List<EntityMovementForceApplier>();
        protected IEntityMovementForceUpdateListener[] _forceUpdateListeners;

        // Interpolation Data
        private Vector2? _prevPointClickPosition = null;
        private uint _prevInterpFromTick;

        public bool CanSimulateMovement()
        {
            return Entity.IsOwnerClient || (Entity.IsOwnerClientOrOwnedByServer && movementSecure == MovementSecure.NotSecure) || (Entity.IsServer && movementSecure == MovementSecure.ServerAuthoritative);
        }

        protected void SetupSimulationTick(uint tick)
        {
            if (_hasSimTick)
                return;
            _hasSimTick = true;
            _simTick = tick;
        }

        protected void ClearSimulationTick()
        {
            _hasSimTick = false;
            _simTick = 0;
        }

        protected void SetupInterpolationTick(uint tick)
        {
            if (_hasInterpTick)
                return;
            _hasInterpTick = true;
            _interpTick = tick;
        }

        protected void ClearInterpolationTick()
        {
            _hasInterpTick = false;
            _interpTick = 0;
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
            return true;
        }

        public Bounds GetMovementBounds()
        {
            return GameplayUtils.MakeLocalBoundsByCollider(transform);
        }

        protected virtual void SetMovePaths(Vector2 position, bool useNavMesh)
        {
            // TODO: Implement nav mesh
            NavPaths = new Queue<Vector2>();
            NavPaths.Enqueue(position);
        }

        public virtual void SetLookRotation(Quaternion rotation, bool immediately)
        {
            if (!Entity.CanMove() || !Entity.CanTurn())
                return;
            if (CanSimulateMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                Direction2D = (Vector2)(rotation * Vector3.forward);
            }
        }

        public Quaternion GetLookRotation()
        {
            return Quaternion.LookRotation(Direction2D);
        }

        public void SetSmoothTurnSpeed(float speed)
        {
            // 2D, do nothing
        }

        public float GetSmoothTurnSpeed()
        {
            // 2D, do nothing
            return 0f;
        }

        public virtual void KeyMovement(Vector3 moveDirection, MovementState movementState)
        {
            if (!Entity.CanMove())
                return;
            if (!CanSimulateMovement())
                return;
            uint tick = Manager.LocalTick;
            if (_inputBuffers.Count > 0)
            {
                uint prevTick = _inputBuffers.Keys[_inputBuffers.Count - 1];
                if (prevTick == tick)
                    return;
                if (_inputBuffers.TryGetValue(prevTick, out InputData prevInput) &&
                    prevInput.IsPointClick && moveDirection.sqrMagnitude <= 0f)
                {
                    prevInput.Tick = tick;
                    StoreInputBuffer(prevInput);
                    return;
                }
            }
            StoreInputBuffer(new InputData()
            {
                Tick = tick,
                IsPointClick = false,
                MovementState = movementState,
                Direction2D = moveDirection,
            });
        }

        public virtual void PointClickMovement(Vector3 position)
        {
            if (!Entity.CanMove())
                return;
            if (!CanSimulateMovement())
                return;
            uint tick = Manager.LocalTick;
            _inputBuffers.Remove(tick);
            StoreInputBuffer(new InputData()
            {
                Tick = tick,
                IsPointClick = true,
                Position = position,
            });
        }

        public void SetExtraMovementState(ExtraMovementState extraMovementState)
        {
            if (!Entity.CanMove())
                return;
            if (!CanSimulateMovement())
                return;
            uint tick = Manager.LocalTick;
            if (!_inputBuffers.TryGetValue(tick, out InputData inputData))
                return;
            if (inputData.ExtraMovementState != ExtraMovementState.None)
                return;
            inputData.ExtraMovementState = extraMovementState;
            _inputBuffers[tick] = inputData;
        }

        public void StopMove()
        {
            if (movementSecure == MovementSecure.ServerAuthoritative)
            {
                // Send movement input to server, then server will apply movement and sync transform to clients
                uint tick = Manager.LocalTick;
                _inputBuffers[tick] = new InputData()
                {
                    Tick = tick,
                    IsStopped = true,
                };
            }
            StopMoveFunction();
        }

        private void StopMoveFunction()
        {
            NavPaths = null;
#if UNITY_6000_0_OR_NEWER
            CacheRigidbody2D.linearVelocity = Vector2.zero;
#else
            CacheRigidbody2D.velocity = Vector2.zero;
#endif
            MovementState = MovementState.IsGrounded;
            ExtraMovementState = ExtraMovementState.None;
        }

        public void Teleport(Vector3 position, Quaternion rotation, bool stillMoveAfterTeleport)
        {
            Debug.LogError("Implement this later");
        }

        public async UniTask WaitClientTeleportConfirm()
        {
            return;
            // TODO: Implement this later
            while (this != null && _isServerWaitingTeleportConfirm)
            {
                await UniTask.Delay(1000);
            }
        }

        public override void EntityAwake()
        {
            // Prepare rigidbody component
            CacheRigidbody2D = gameObject.GetOrAddComponent<Rigidbody2D>();
            _forceUpdateListeners = gameObject.GetComponents<IEntityMovementForceUpdateListener>();
            // Disable unused component
            LiteNetLibTransform disablingComp = gameObject.GetComponent<LiteNetLibTransform>();
            if (disablingComp != null)
            {
                Logging.LogWarning(nameof(RigidBodyEntityMovement2D), "You can remove `LiteNetLibTransform` component from game entity, it's not being used anymore [" + name + "]");
                disablingComp.enabled = false;
            }
            // Setup
            CacheRigidbody2D.simulated = false;
            CacheRigidbody2D.gravityScale = 0;
            CacheRigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            StopMoveFunction();
        }

        public override void OnSetOwnerClient(bool isOwnerClient)
        {
            CacheRigidbody2D.simulated = CanSimulateMovement();
            ClearInterpolationTick();
            ClearSimulationTick();
            // Setup data for syncing determining
            SyncData syncData = _prevSyncData;
            syncData.Tick = Manager.LocalTick;
            syncData.Position = EntityTransform.position;
            syncData.MovementState = MovementState;
            syncData.ExtraMovementState = ExtraMovementState;
            syncData.Direction2D = Direction2D;
            _prevSyncData = syncData;
            // Force setup sim tick
            _hasSimTick = true;
            _simTick = Manager.LocalTick;
        }

        public override void EntityStart()
        {
            if (Manager != null)
                Manager.LogicUpdater.OnTick += LogicUpdater_OnTick;
        }

        public override void EntityOnDestroy()
        {
            if (Manager != null)
                Manager.LogicUpdater.OnTick -= LogicUpdater_OnTick;
        }

        private void LogicUpdater_OnTick(LogicUpdater updater)
        {
            _simTick++;
            _interpTick++;

            // Storing sync buffers, server will send to other clients, owner client will send to server
            if (IsServer || (IsOwnerClient && movementSecure == MovementSecure.NotSecure))
            {
                SyncData syncData = _prevSyncData;
                bool changed =
                    Vector2.Distance(EntityTransform.position, syncData.Position) > positionThreshold ||
                    MovementState != syncData.MovementState || ExtraMovementState != syncData.ExtraMovementState ||
                    Quaternion.Angle(Quaternion.LookRotation(Vector3.forward, Direction2D), Quaternion.LookRotation(Vector3.forward, syncData.Direction2D)) > eulerAnglesThreshold;
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
                    syncData.Direction2D = Direction2D;
                }
                _prevSyncData = syncData;

                syncData.Tick = updater.LocalTick;
                // Stored buffers will be send later
                StoreSyncBuffer(syncData);
            }
        }

        public override void EntityUpdate()
        {
            // Simulate movement by inputs if it can predict movement
            if (CanSimulateMovement())
            {
                SimulateMovementFromInput(Time.deltaTime);
            }
            else
            {
                InterpolateTransform();
            }
        }

        private void SimulateMovementFromInput(float deltaTime)
        {
            if (!_inputBuffers.TryGetValue(_simTick, out InputData inputData))
                return;

            if (inputData.IsStopped)
            {
                StopMoveFunction();
                return;
            }

            bool isDashing = false;
            float tempSqrMagnitude;
            float tempPredictSqrMagnitude;
            float tempTargetDistance;
            float tempEntityMoveSpeed;
            float tempMaxMoveSpeed;
            Vector2 tempInputDirection;
            Vector2 tempMoveDirection;
            Vector2 tempMoveVelocity;
            Vector2 tempCurrentPosition;
            Vector2 tempTargetPosition;
            Vector2 tempPredictPosition;

            tempCurrentPosition = EntityTransform.position;
            tempInputDirection = Vector3.zero;
            tempMoveVelocity = Vector3.zero;
            tempMoveDirection = Vector2.zero;
            tempTargetDistance = 0f;

            if (!inputData.IsPointClick)
                tempInputDirection = inputData.Direction2D;

            if (inputData.IsPointClick && (!_prevPointClickPosition.HasValue || Vector3.Distance(_prevPointClickPosition.Value, inputData.Position) > 0.01f))
            {
                SetMovePaths(inputData.Position, true);
            }
            else if (tempInputDirection.sqrMagnitude > 0f)
            {
                NavPaths = null;
            }
            MovementState = inputData.MovementState;
            ExtraMovementState = inputData.ExtraMovementState;
            isDashing = inputData.MovementState.Has(MovementState.IsDash);

            if (HasNavPaths)
            {
                // Set `tempTargetPosition` and `tempCurrentPosition`
                tempTargetPosition = NavPaths.Peek();
                tempMoveDirection = (tempTargetPosition - tempCurrentPosition).normalized;
                tempTargetDistance = Vector2.Distance(tempTargetPosition, tempCurrentPosition);
                float stoppingDistance = StoppingDistance;
                bool shouldStop = tempTargetDistance < stoppingDistance;
                if (shouldStop)
                {
                    NavPaths.Dequeue();
                    if (!HasNavPaths)
                    {
                        StopMoveFunction();
                        tempMoveDirection = Vector2.zero;
                    }
                    else
                    {
                        if (!MovementState.Has(MovementState.Forward))
                            MovementState |= MovementState.Forward;
                    }
                }
                else
                {
                    if (!MovementState.Has(MovementState.Forward))
                        MovementState |= MovementState.Forward;
                    // Turn character to destination
                    Direction2D = tempMoveDirection;
                }
            }
            else if (tempInputDirection.sqrMagnitude > 0f)
            {
                tempMoveDirection = tempInputDirection;
                tempTargetPosition = tempCurrentPosition + tempMoveDirection;
            }
            else
            {
                tempTargetPosition = tempCurrentPosition;
                StopMove();
            }

            if (!Entity.CanMove())
            {
                tempMoveDirection = Vector2.zero;
            }

            if (!Entity.CanDash())
            {
                isDashing = false;
            }

            // Prepare movement speed
            tempEntityMoveSpeed = Entity.GetMoveSpeed();
            tempMaxMoveSpeed = tempEntityMoveSpeed;

            // Dashing
            if (isDashing)
            {
                // Can have only one replace movement force applier, so remove stored ones
                _movementForceAppliers.RemoveReplaceMovementForces();
                _movementForceAppliers.Add(new EntityMovementForceApplier().Apply(
                    ApplyMovementForceMode.Dash, Direction2D, ApplyMovementForceSourceType.None, 0, 0, dashingForceApplier));
            }

            // Apply Forces
            _forceUpdateListeners.OnPreUpdateForces(_movementForceAppliers);
            _movementForceAppliers.UpdateForces(Time.deltaTime,
                Entity.GetMoveSpeed(MovementState.Forward, ExtraMovementState.None),
                out Vector3 forceMotion, out EntityMovementForceApplier replaceMovementForceApplier);
            _forceUpdateListeners.OnPostUpdateForces(_movementForceAppliers);

            // Replace player's movement by this
            if (replaceMovementForceApplier != null)
            {
                // Still dashing to add dash to movement state
                if (replaceMovementForceApplier.Mode == ApplyMovementForceMode.Dash)
                    MovementState |= MovementState.IsDash;
                // Force turn to dashed direction
                tempMoveDirection = replaceMovementForceApplier.Direction;
                Direction2D = tempMoveDirection;
                // Change move speed to dash force
                tempMaxMoveSpeed = replaceMovementForceApplier.CurrentSpeed;
            }

            // Updating horizontal movement (WASD inputs)
            if (tempMoveDirection.sqrMagnitude > 0f)
            {
                // If character move backward
                CurrentMoveSpeed = tempMaxMoveSpeed;

                // NOTE: `tempTargetPosition` and `tempCurrentPosition` were set above
                tempSqrMagnitude = (tempTargetPosition - tempCurrentPosition).sqrMagnitude;
                tempPredictPosition = tempCurrentPosition + (tempMoveDirection * CurrentMoveSpeed * deltaTime);
                tempPredictSqrMagnitude = (tempPredictPosition - tempCurrentPosition).sqrMagnitude;
                if (HasNavPaths)
                {
                    // Check `tempSqrMagnitude` against the `tempPredictSqrMagnitude`
                    // if `tempPredictSqrMagnitude` is greater than `tempSqrMagnitude`,
                    // rigidbody will reaching target and character is moving pass it,
                    // so adjust move speed by distance and time (with physic formula: v=s/t)
                    if (tempPredictSqrMagnitude >= tempSqrMagnitude && tempTargetDistance > 0f)
                        CurrentMoveSpeed *= tempTargetDistance / deltaTime / CurrentMoveSpeed;
                }
                tempMoveVelocity = tempMoveDirection * CurrentMoveSpeed;
            }
#if UNITY_6000_0_OR_NEWER
            CacheRigidbody2D.linearVelocity = tempMoveVelocity + new Vector2(forceMotion.x, forceMotion.y);
#else
            CacheRigidbody2D.velocity = tempMoveVelocity + new Vector2(forceMotion.x, forceMotion.y);
#endif
        }

        private void InterpolateTransform()
        {
            if (!_hasInterpTick || _interpBuffers.Count < 2)
            {
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
                SyncData data1 = _interpBuffers[tick1];
                SyncData data2 = _interpBuffers[tick2];

                if (tick1 <= renderTick && renderTick <= tick2)
                {
                    interpFromTick = tick1;
                    interpToTick = tick2;
                    _interpFromSyncData = data1;
                    _interpToSyncData = data2;
                    if (_prevInterpFromTick != interpFromTick)
                    {
                        _startInterpTime = currentTime;
                        _endInterpTime = currentTime + (Manager.LogicUpdater.DeltaTimeF * (tick2 - tick1));
                        _prevInterpFromTick = interpFromTick;
                    }
                    break;
                }
            }

            float t = Mathf.InverseLerp(_startInterpTime, _endInterpTime, currentTime);
            EntityTransform.position = Vector2.Lerp(_interpFromSyncData.Position, _interpToSyncData.Position, t);
            // CacheRigidbody2D.MovePosition(Vector2.Lerp(_interpFromPosition, _interpToPosition, t));
            Direction2D = Vector2.Lerp(_interpFromSyncData.Direction2D, _interpToSyncData.Direction2D, t).normalized;
            MovementState = t < 0.75f ? _interpFromSyncData.MovementState : _interpToSyncData.MovementState;
            ExtraMovementState = t < 0.75f ? _interpFromSyncData.ExtraMovementState : _interpToSyncData.ExtraMovementState;
        }

        private void StoreInputBuffer(InputData entry, int maxBuffers = 3)
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

        private void StoreInputBuffers(InputData[] data, int size, int maxBuffers = 3)
        {
            for (int i = 0; i < size; ++i)
            {
                InputData entry = data[i];
                if (_inputBuffers.ContainsKey(entry.Tick))
                    continue;
                _inputBuffers.Add(entry.Tick, entry);
            }
            // Prune old ticks (keep last N)
            while (_inputBuffers.Count > maxBuffers)
            {
                _inputBuffers.RemoveAt(0);
            }
        }

        private void StoreSyncBuffer(SyncData entry, int maxBuffers = 3)
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

        private void StoreInterpolateBuffers(SyncData[] data, int size, int maxBuffers = 3)
        {
            for (int i = 0; i < size; ++i)
            {
                SyncData entry = data[i];
                if (_interpBuffers.ContainsKey(entry.Tick))
                    continue;
                _interpBuffers.Add(entry.Tick, entry);
            }
            // Prune old ticks (keep last N)
            while (_interpBuffers.Count > maxBuffers)
            {
                _interpBuffers.RemoveAt(0);
            }
        }

        public bool WriteClientState(long writeTimestamp, NetDataWriter writer, out bool shouldSendReliably)
        {
            shouldSendReliably = false;
            switch (movementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    if (_inputBuffers.Count == 0)
                        return false;
                    writer.Put((byte)_inputBuffers.Count);
                    for (int i = 0; i < _inputBuffers.Count; ++i)
                    {
                        writer.Put(_inputBuffers.Values[i]);
                    }
                    return true;
                default:
                    if (_syncBuffers.Count == 0)
                        return false;
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
            shouldSendReliably = false;
            if (_syncBuffers.Count == 0)
                return false;
            writer.Put((byte)_syncBuffers.Count);
            for (int i = 0; i < _syncBuffers.Count; ++i)
            {
                writer.Put(_syncBuffers.Values[i]);
            }
            return true;
        }

        public void ReadClientStateAtServer(long peerTimestamp, NetDataReader reader)
        {
            byte size;
            switch (movementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    size = reader.GetByte();
                    if (size == 0)
                        return;
                    InputData[] inputBuffers = ArrayPool<InputData>.Shared.Rent(size);
                    for (byte i = 0; i < size; ++i)
                    {
                        inputBuffers[i] = reader.Get<InputData>();
                    }
                    if (!IsOwnerClient)
                    {
                        StoreInputBuffers(inputBuffers, size);
                        SetupSimulationTick(_inputBuffers.Keys[_inputBuffers.Count - 1]);
                    }
                    ArrayPool<InputData>.Shared.Return(inputBuffers);
                    break;
                default:
                    size = reader.GetByte();
                    if (size == 0)
                        return;
                    SyncData[] interpoationBuffers = ArrayPool<SyncData>.Shared.Rent(size);
                    for (byte i = 0; i < size; ++i)
                    {
                        interpoationBuffers[i] = reader.Get<SyncData>();
                    }
                    StoreInterpolateBuffers(interpoationBuffers, size, 30);
                    SetupInterpolationTick(_interpBuffers.Keys[_interpBuffers.Count - 1]);
                    ArrayPool<SyncData>.Shared.Return(interpoationBuffers);
                    break;
            }
        }

        public void ReadServerStateAtClient(long peerTimestamp, NetDataReader reader)
        {
            byte size = reader.GetByte();
            if (size == 0)
                return;
            SyncData[] interpoationBuffers = ArrayPool<SyncData>.Shared.Rent(size);
            for (byte i = 0; i < size; ++i)
            {
                interpoationBuffers[i] = reader.Get<SyncData>();
            }
            if (!IsServer)
            {
                StoreInterpolateBuffers(interpoationBuffers, size, 30);
                SetupInterpolationTick(_interpBuffers.Keys[_interpBuffers.Count - 1]);
            }
            ArrayPool<SyncData>.Shared.Return(interpoationBuffers);
        }
    }
}