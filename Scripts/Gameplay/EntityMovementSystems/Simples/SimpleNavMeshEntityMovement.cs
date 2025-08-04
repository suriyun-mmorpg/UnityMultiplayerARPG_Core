using Cysharp.Threading.Tasks;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.AI;

namespace MultiplayerARPG
{
    /// <summary>
    /// This one is simple client-authoritative entity movement
    /// Being made to show how to simply implements entity movements by using `LiteNetLibTransform`
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(LiteNetLibTransform))]
    public class SimpleNavMeshEntityMovement : BaseNetworkedGameEntityComponent<BaseGameEntity>, IEntityMovementComponent
    {
        protected static readonly float s_minMagnitudeToDetermineMoving = 0.01f;
        protected static readonly ProfilerMarker s_UpdateProfilerMarker = new ProfilerMarker("SimpleNavMeshEntityMovement - Update");

        [Header("Movement Settings")]
        public ObstacleAvoidanceType obstacleAvoidanceWhileMoving = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        public ObstacleAvoidanceType obstacleAvoidanceWhileStationary = ObstacleAvoidanceType.NoObstacleAvoidance;

        [Header("Dashing")]
        public EntityMovementForceApplierData dashingForceApplier = EntityMovementForceApplierData.CreateDefault();

        public LiteNetLibTransform CacheNetworkedTransform { get; protected set; }
        public NavMeshAgent CacheNavMeshAgent { get; protected set; }
        public float StoppingDistance
        {
            get { return CacheNavMeshAgent.stoppingDistance; }
        }
        public MovementState MovementState { get; protected set; }
        public ExtraMovementState ExtraMovementState { get; protected set; }
        public DirectionVector2 Direction2D { get { return Vector2.down; } set { } }
        public float CurrentMoveSpeed { get { return CacheNavMeshAgent.isStopped ? 0f : CacheNavMeshAgent.speed; } }

        // Inputs
        protected MovementInputData3D _movementInput;

        // Teleportation
        protected MovementTeleportState _serverTeleportState;
        protected MovementTeleportState _clientTeleportState;

        // Force simulation
        protected readonly List<EntityMovementForceApplier> _movementForceAppliers = new List<EntityMovementForceApplier>();
        protected IEntityMovementForceUpdateListener[] _forceUpdateListeners;

        // Turn simulate codes
        protected bool _lookRotationApplied;
        protected float _yAngle;
        protected float _targetYAngle;
        protected float _yTurnSpeed;
        protected float? _remoteTargetYAngle;

        // Interpolation Data
        protected SortedList<uint, System.ValueTuple<MovementState, ExtraMovementState>> _interpExtra = new SortedList<uint, System.ValueTuple<MovementState, ExtraMovementState>>();

        public override void EntityAwake()
        {
            // Prepare nav mesh agent component
            CacheNetworkedTransform = gameObject.GetOrAddComponent<LiteNetLibTransform>();
            CacheNavMeshAgent = gameObject.GetOrAddComponent<NavMeshAgent>();
            _forceUpdateListeners = gameObject.GetComponents<IEntityMovementForceUpdateListener>();
            Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
            if (rigidBody != null)
            {
                rigidBody.useGravity = false;
                rigidBody.isKinematic = true;
            }
            // Setup
            CacheNetworkedTransform.syncByOwnerClient = true;
            CacheNetworkedTransform.onWriteSyncBuffer += CacheNetworkedTransform_onWriteSyncBuffer;
            CacheNetworkedTransform.onReadInterpBuffer += CacheNetworkedTransform_onReadInterpBuffer;
            CacheNetworkedTransform.onInterpolate += CacheNetworkedTransform_onInterpolate;
            CacheNavMeshAgent.enabled = false;
            _yAngle = _targetYAngle = EntityTransform.eulerAngles.y;
            _lookRotationApplied = true;
        }

        public override void EntityStart()
        {
            _clientTeleportState = MovementTeleportState.Responding;
        }

        public override void EntityOnDestroy()
        {
            CacheNetworkedTransform.onWriteSyncBuffer -= CacheNetworkedTransform_onWriteSyncBuffer;
            CacheNetworkedTransform.onReadInterpBuffer -= CacheNetworkedTransform_onReadInterpBuffer;
            CacheNetworkedTransform.onInterpolate -= CacheNetworkedTransform_onInterpolate;
        }

        public override void OnSetOwnerClient(bool isOwnerClient)
        {
            CacheNavMeshAgent.enabled = CanSimulateMovement();
        }

        public override void ComponentOnEnable()
        {
            CacheNavMeshAgent.enabled = CanSimulateMovement();
        }

        public override void ComponentOnDisable()
        {
            CacheNavMeshAgent.enabled = false;
        }

        public bool CanSimulateMovement()
        {
            return Entity.IsOwnerClientOrOwnedByServer;
        }

        protected void CacheNetworkedTransform_onWriteSyncBuffer(NetDataWriter writer, uint tick)
        {
            writer.Put((byte)MovementState);
            writer.Put((byte)ExtraMovementState);
        }

        protected void CacheNetworkedTransform_onReadInterpBuffer(NetDataReader reader, uint tick)
        {
            _interpExtra[tick] = new System.ValueTuple<MovementState, ExtraMovementState>(
                (MovementState)reader.GetByte(),
                (ExtraMovementState)reader.GetByte());
            while (_interpExtra.Count > 30)
            {
                _interpExtra.RemoveAt(0);
            }
        }

        protected void CacheNetworkedTransform_onInterpolate(LiteNetLibTransform.TransformData interpFromData, LiteNetLibTransform.TransformData interpToData, float interpTime)
        {
            if (interpTime <= 0.75f)
            {
                if (_interpExtra.TryGetValue(interpFromData.Tick, out var states))
                {
                    MovementState = states.Item1;
                    ExtraMovementState = states.Item2;
                }
            }
            else
            {
                if (_interpExtra.TryGetValue(interpToData.Tick, out var states))
                {
                    MovementState = states.Item1;
                    ExtraMovementState = states.Item2;
                }
            }
        }

        public void KeyMovement(Vector3 moveDirection, MovementState movementState)
        {
            if (!Entity.CanMove())
                return;
            if (!CanSimulateMovement())
                return;
            MovementInputData3D movementInput = _movementInput;
            if (moveDirection.sqrMagnitude > 0)
            {
                movementInput.IsPointClick = false;
                CacheNavMeshAgent.updatePosition = true;
                CacheNavMeshAgent.updateRotation = false;
                if (CacheNavMeshAgent.isOnNavMesh)
                    CacheNavMeshAgent.isStopped = true;
            }
            movementInput.MoveDirection = moveDirection;
            movementInput.MovementState = movementState;
            _movementInput = movementInput;
        }

        public void PointClickMovement(Vector3 position)
        {
            if (!Entity.CanMove())
                return;
            if (!CanSimulateMovement())
                return;
            // Always apply movement to owner client (it's client prediction for server auth movement)
            SetMovePaths(position);
        }

        public void SetExtraMovementState(ExtraMovementState extraMovementState)
        {
            if (!Entity.CanMove())
                return;
            if (!CanSimulateMovement())
                return;
            MovementInputData3D movementInput = _movementInput;
            movementInput.ExtraMovementState = extraMovementState;
            _movementInput = movementInput;
        }

        public void StopMove()
        {
            StopMoveFunction();
        }

        protected void StopMoveFunction()
        {
            MovementInputData3D movementInput = _movementInput;
            movementInput.MoveDirection = Vector3.zero;
            movementInput.IsPointClick = false;
            _movementInput = movementInput;
            CacheNavMeshAgent.updatePosition = false;
            CacheNavMeshAgent.updateRotation = false;
            if (CacheNavMeshAgent.isOnNavMesh)
                CacheNavMeshAgent.isStopped = true;
        }

        public void SetLookRotation(Quaternion rotation, bool immediately)
        {
            if (!Entity.CanMove() || !Entity.CanTurn())
                return;
            if (CanSimulateMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                _targetYAngle = rotation.eulerAngles.y;
                _lookRotationApplied = false;
                if (immediately)
                    TurnImmediately(_targetYAngle);
            }
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

        public void Teleport(Vector3 position, Quaternion rotation, bool stillMoveAfterTeleport)
        {
            if (!IsServer)
            {
                Logging.LogWarning(nameof(NavMeshEntityMovement), "Teleport function shouldn't be called at client [" + name + "]");
                return;
            }
            OnTeleport(position, rotation.eulerAngles.y, stillMoveAfterTeleport);
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
            if (CanSimulateMovement())
            {
                using (s_UpdateProfilerMarker.Auto())
                {
                    float deltaTime = Time.deltaTime;
                    UpdateMovement(deltaTime);
                    UpdateRotation(deltaTime);
                }
            }
        }

        public void UpdateMovement(float deltaTime)
        {
            // Prepare speed
            CacheNavMeshAgent.speed = Entity.GetMoveSpeed();
            bool isDashing = _movementInput.MovementState.Has(MovementState.IsDash);

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
                _targetYAngle = Quaternion.LookRotation(replaceMovementForceApplier.Direction).eulerAngles.y;
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

            if (forceMotion.magnitude > 0 && CacheNavMeshAgent.isOnNavMesh)
            {
                CacheNavMeshAgent.Move(forceMotion * deltaTime);
            }

            bool isStationary = !CacheNavMeshAgent.isOnNavMesh || CacheNavMeshAgent.isStopped || GetPathRemainingDistance() <= CacheNavMeshAgent.stoppingDistance;

            CacheNavMeshAgent.obstacleAvoidanceType = isStationary ? obstacleAvoidanceWhileStationary : obstacleAvoidanceWhileMoving;
            MovementState = MovementState.IsGrounded;
            if (!replaceMovementForceApplierMode.IsReplaceMovement())
            {
                if (_movementInput.MoveDirection.ToVector3().sqrMagnitude > 0f)
                {
                    // Moving by WASD keys
                    CacheNavMeshAgent.Move(_movementInput.MoveDirection.ToVector3() * CacheNavMeshAgent.speed * deltaTime);
                    MovementState |= MovementState.Forward;
                    // Turn character to destination
                    if (_lookRotationApplied && Entity.CanTurn())
                        _targetYAngle = Quaternion.LookRotation(_movementInput.MoveDirection.ToVector3()).eulerAngles.y;
                }
                else
                {
                    // Moving by clicked position
                    MovementState |= CacheNavMeshAgent.velocity.magnitude > s_minMagnitudeToDetermineMoving ? MovementState.Forward : MovementState.None;
                    // Turn character to destination
                    if (_lookRotationApplied && Entity.CanTurn() && CacheNavMeshAgent.velocity.magnitude > s_minMagnitudeToDetermineMoving)
                        _targetYAngle = Quaternion.LookRotation(CacheNavMeshAgent.velocity.normalized).eulerAngles.y;
                }
            }
            // Update extra movement state
            ExtraMovementState = this.ValidateExtraMovementState(MovementState, _movementInput.ExtraMovementState);

            if (replaceMovementForceApplierMode == ApplyMovementForceMode.Dash)
                MovementState |= MovementState.IsDash;
        }

        public void UpdateRotation(float deltaTime)
        {
            if (_yTurnSpeed <= 0f)
                _yAngle = _targetYAngle;
            else if (Mathf.Abs(_yAngle - _targetYAngle) > 1f)
                _yAngle = Mathf.LerpAngle(_yAngle, _targetYAngle, _yTurnSpeed * deltaTime);
            _lookRotationApplied = true;
            RotateY();
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

        protected void RotateY()
        {
            EntityTransform.eulerAngles = new Vector3(0f, _yAngle, 0f);
        }

        protected void SetMovePaths(Vector3 position)
        {
            if (!Entity.CanMove())
                return;
            MovementInputData3D movementInput = _movementInput;
            movementInput.MoveDirection = Vector3.zero;
            movementInput.IsPointClick = true;
            _movementInput = movementInput;
            CacheNavMeshAgent.updatePosition = true;
            CacheNavMeshAgent.updateRotation = false;
            if (CacheNavMeshAgent.isOnNavMesh)
            {
                CacheNavMeshAgent.isStopped = false;

                NavMeshPath path = new NavMeshPath();
                NavMesh.CalculatePath(transform.position, position, CacheNavMeshAgent.areaMask, path);
                CacheNavMeshAgent.SetPath(path);
            }
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
            return false;
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
            return false;
        }

        public void ReadClientStateAtServer(long peerTimestamp, NetDataReader reader)
        {
            MovementTeleportState movementTeleportState = (MovementTeleportState)reader.GetByte();
            if (movementTeleportState.Has(MovementTeleportState.Responding))
            {
                _serverTeleportState = MovementTeleportState.None;
                return;
            }
        }

        public void ReadServerStateAtClient(long peerTimestamp, NetDataReader reader)
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
                OnTeleport(position, rotation, stillMoveAfterTeleport);
                return;
            }
        }

        protected virtual Vector3 GetMoveablePosition(Vector3 oldPos, Vector3 newPos, float clientMoveDist, float moveableDist)
        {
            Vector3 dir = (newPos.GetXZ() - oldPos.GetXZ()).normalized;
            Vector3 deltaMove = dir * Mathf.Min(clientMoveDist, moveableDist);
            return oldPos + deltaMove;
        }

        protected virtual void OnTeleport(Vector3 position, float yAngle, bool stillMoveAfterTeleport)
        {
            MovementInputData3D movementInput = _movementInput;
            movementInput.MoveDirection = Vector3.zero;
            movementInput.IsPointClick = false;
            _movementInput = movementInput;
            Vector3 beforeWarpDest = CacheNavMeshAgent.destination;
            CacheNavMeshAgent.Warp(position);
            if (!stillMoveAfterTeleport && CacheNavMeshAgent.isOnNavMesh)
                CacheNavMeshAgent.isStopped = true;
            if (stillMoveAfterTeleport && CacheNavMeshAgent.isOnNavMesh)
                CacheNavMeshAgent.SetDestination(beforeWarpDest);
            TurnImmediately(yAngle);
            if (IsServer && !IsOwnedByServer)
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

        public void RemoteTurnSimulation(float yAngle, float deltaTime)
        {
            if (!IsClient)
            {
                // Turn to target immediately
                TurnImmediately(yAngle);
                return;
            }
            // Will turn smoothly later
            _targetYAngle = yAngle;
            _yTurnSpeed = 1f / deltaTime;
        }

        public void TurnImmediately(float yAngle)
        {
            _yAngle = _targetYAngle = yAngle;
            RotateY();
        }

        public void ApplyRemoteTurnAngle()
        {
            if (_remoteTargetYAngle.HasValue)
            {
                _targetYAngle = _remoteTargetYAngle.Value;
                _remoteTargetYAngle = null;
            }
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

        public async UniTask WaitClientTeleportConfirm()
        {
            while (this != null && _serverTeleportState.Has(MovementTeleportState.WaitingForResponse))
            {
                await UniTask.Delay(1000);
            }
        }
    }
}
