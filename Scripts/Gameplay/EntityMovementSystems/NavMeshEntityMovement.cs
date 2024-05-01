using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibManager;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Profiling;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshEntityMovement : BaseNetworkedGameEntityComponent<BaseGameEntity>, IEntityMovementComponent
    {
        protected static readonly long s_lagBuffer = System.TimeSpan.TicksPerMillisecond * 200;
        protected static readonly float s_minMagnitudeToDetermineMoving = 0.01f;
        protected static readonly float s_minDistanceToSimulateMovement = 0.01f;
        protected static readonly float s_timestampToUnityTimeMultiplier = 0.001f;

        [Header("Movement Settings")]
        public ObstacleAvoidanceType obstacleAvoidanceWhileMoving = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        public ObstacleAvoidanceType obstacleAvoidanceWhileStationary = ObstacleAvoidanceType.NoObstacleAvoidance;
        public MovementSecure movementSecure = MovementSecure.NotSecure;

        [Header("Networking Settings")]
        public float snapThreshold = 5.0f;

        public NavMeshAgent CacheNavMeshAgent { get; private set; }
        public float StoppingDistance
        {
            get { return CacheNavMeshAgent.stoppingDistance; }
        }
        public MovementState MovementState { get; protected set; }
        public ExtraMovementState ExtraMovementState { get; protected set; }
        public DirectionVector2 Direction2D { get { return Vector2.down; } set { } }
        public float CurrentMoveSpeed { get { return CacheNavMeshAgent.isStopped ? 0f : CacheNavMeshAgent.speed; } }

        // Input codes
        protected Vector3? _inputDirection;
        protected ExtraMovementState _tempExtraMovementState;
        protected bool _moveByDestination;

        // Move simulate codes
        private readonly List<EntityMovementForceApplier> _movementForceAppliers = new List<EntityMovementForceApplier>();

        // Client state codes
        protected EntityMovementInput _oldInput;
        protected EntityMovementInput _currentInput;

        // State simulate codes
        private float? _lagMoveSpeedRate;

        // Turn simulate codes
        protected bool _lookRotationApplied;
        protected float _yAngle;
        protected float _targetYAngle;
        protected float _yTurnSpeed;

        // Teleport codes
        protected bool _isTeleporting;
        protected bool _stillMoveAfterTeleport;

        // Peers accept codes
        protected long _acceptedPositionTimestamp;
        private MovementState _acceptedMovementStateBeforeStopped;
        private ExtraMovementState _acceptedExtraMovementStateBeforeStopped;

        // Server validate codes
        protected float _lastServerValidateHorDistDiff;
        protected bool _isServerWaitingTeleportConfirm;

        // Client confirm codes
        protected bool _isClientConfirmingTeleport;

        public override void EntityAwake()
        {
            // Prepare nav mesh agent component
            CacheNavMeshAgent = gameObject.GetOrAddComponent<NavMeshAgent>();
            // Disable unused component
            LiteNetLibTransform disablingComp = gameObject.GetComponent<LiteNetLibTransform>();
            if (disablingComp != null)
            {
                Logging.LogWarning(nameof(NavMeshEntityMovement), "You can remove `LiteNetLibTransform` component from game entity, it's not being used anymore [" + name + "]");
                disablingComp.enabled = false;
            }
            Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
            if (rigidBody != null)
            {
                rigidBody.useGravity = false;
                rigidBody.isKinematic = true;
            }
            // Setup
            _yAngle = _targetYAngle = CacheTransform.eulerAngles.y;
            _lookRotationApplied = true;
            StopMoveFunction();
        }

        public override void EntityStart()
        {
            _isClientConfirmingTeleport = true;
        }

        public override void ComponentOnEnable()
        {
            CacheNavMeshAgent.enabled = true;
        }

        public override void ComponentOnDisable()
        {
            CacheNavMeshAgent.enabled = false;
        }

        public void KeyMovement(Vector3 moveDirection, MovementState movementState)
        {
            if (!Entity.CanMove())
                return;
            if (moveDirection.sqrMagnitude <= 0)
            {
                _inputDirection = null;
                return;
            }
            if (CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                _inputDirection = moveDirection;
                _moveByDestination = false;
                CacheNavMeshAgent.updatePosition = true;
                CacheNavMeshAgent.updateRotation = false;
                if (CacheNavMeshAgent.isOnNavMesh)
                    CacheNavMeshAgent.isStopped = true;
            }
        }

        public void PointClickMovement(Vector3 position)
        {
            if (!Entity.CanMove())
                return;
            if (CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                SetMovePaths(position);
            }
        }

        public void SetExtraMovementState(ExtraMovementState extraMovementState)
        {
            if (!Entity.CanMove())
                return;
            if (CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                _tempExtraMovementState = extraMovementState;
            }
        }

        public void StopMove()
        {
            if (movementSecure == MovementSecure.ServerAuthoritative)
            {
                // Send movement input to server, then server will apply movement and sync transform to clients
                _currentInput = Entity.SetInputStop(_currentInput);
            }
            StopMoveFunction();
        }

        private void StopMoveFunction()
        {
            _inputDirection = null;
            _moveByDestination = false;
            CacheNavMeshAgent.updatePosition = false;
            CacheNavMeshAgent.updateRotation = false;
            if (CacheNavMeshAgent.isOnNavMesh)
                CacheNavMeshAgent.isStopped = true;
        }

        public void SetLookRotation(Quaternion rotation)
        {
            if (!Entity.CanMove() || !Entity.CanTurn())
                return;
            if (CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                _targetYAngle = rotation.eulerAngles.y;
                _lookRotationApplied = false;
            }
        }

        public Quaternion GetLookRotation()
        {
            return Quaternion.Euler(0f, CacheTransform.eulerAngles.y, 0f);
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
            _isTeleporting = true;
            _stillMoveAfterTeleport = stillMoveAfterTeleport;
            OnTeleport(position, rotation.eulerAngles.y, stillMoveAfterTeleport);
        }

        public bool FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
        {
            result = fromPosition;
            if (NavMesh.SamplePosition(fromPosition, out NavMeshHit navHit, findDistance, NavMesh.AllAreas))
            {
                result = navHit.position;
                return true;
            }
            return false;
        }

        public void ApplyForce(Vector3 direction, ApplyMovementForceMode mode, float force, float deceleration, float duration)
        {
            if (!IsServer)
                return;
            if (mode.IsReplaceMovement())
            {
                // Can have only one replace movement force applier, so remove stored ones
                _movementForceAppliers.RemoveReplaceMovementForces();
            }
            _movementForceAppliers.Add(new EntityMovementForceApplier()
                .Apply(direction, mode, force, deceleration, duration));
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
            Profiler.BeginSample("NavMeshEntityMovement - Update");
            CacheNavMeshAgent.speed = Entity.GetMoveSpeed();
            float deltaTime = Time.deltaTime;
            bool isStationary = !CacheNavMeshAgent.isOnNavMesh || CacheNavMeshAgent.isStopped || GetPathRemainingDistance() <= CacheNavMeshAgent.stoppingDistance;
            if (CanPredictMovement())
            {
                CacheNavMeshAgent.obstacleAvoidanceType = isStationary ? obstacleAvoidanceWhileStationary : obstacleAvoidanceWhileMoving;

                if (_inputDirection.HasValue)
                {
                    // Moving by WASD keys
                    CacheNavMeshAgent.Move(_inputDirection.Value * CacheNavMeshAgent.speed * deltaTime);
                    MovementState = MovementState.Forward | MovementState.IsGrounded;
                    // Turn character to destination
                    if (_lookRotationApplied && Entity.CanTurn())
                        _targetYAngle = Quaternion.LookRotation(_inputDirection.Value).eulerAngles.y;
                }
                else
                {
                    // Moving by clicked position
                    MovementState = (CacheNavMeshAgent.velocity.magnitude > s_minMagnitudeToDetermineMoving ? MovementState.Forward : MovementState.None) | MovementState.IsGrounded;
                    // Turn character to destination
                    if (_lookRotationApplied && Entity.CanTurn() && CacheNavMeshAgent.velocity.magnitude > s_minMagnitudeToDetermineMoving)
                        _targetYAngle = Quaternion.LookRotation(CacheNavMeshAgent.velocity.normalized).eulerAngles.y;
                }
                // Update extra movement state
                ExtraMovementState = this.ValidateExtraMovementState(MovementState, _tempExtraMovementState);
                // Set current input
                _currentInput = Entity.SetInputMovementState(_currentInput, MovementState);
                _currentInput = Entity.SetInputExtraMovementState(_currentInput, ExtraMovementState);
                if (_inputDirection.HasValue)
                {
                    _currentInput = Entity.SetInputIsKeyMovement(_currentInput, true);
                    _currentInput = Entity.SetInputPosition(_currentInput, CacheTransform.position);
                }
                else if (_moveByDestination)
                {
                    _currentInput = Entity.SetInputIsKeyMovement(_currentInput, false);
                    _currentInput = Entity.SetInputPosition(_currentInput, CacheNavMeshAgent.destination);
                }
            }
            else
            {
                // Disable obstacle avoidance because it won't predict movement, it is just moving to destination without obstacle avoidance
                CacheNavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
                if (CacheNavMeshAgent.velocity.magnitude > s_minMagnitudeToDetermineMoving)
                {
                    MovementState = _acceptedMovementStateBeforeStopped;
                    ExtraMovementState = _acceptedExtraMovementStateBeforeStopped;
                }
                else
                {
                    MovementState = MovementState.IsGrounded;
                    ExtraMovementState = ExtraMovementState.None;
                }
            }
            // Update rotating
            if (_yTurnSpeed <= 0f)
                _yAngle = _targetYAngle;
            else if (Mathf.Abs(_yAngle - _targetYAngle) > 1f)
                _yAngle = Mathf.LerpAngle(_yAngle, _targetYAngle, _yTurnSpeed * deltaTime);
            UpdateRotation();
            _lookRotationApplied = true;
            _currentInput = Entity.SetInputYAngle(_currentInput, CacheTransform.eulerAngles.y);
            Profiler.EndSample();
        }

        private void UpdateRotation()
        {
            CacheTransform.eulerAngles = new Vector3(0f, _yAngle, 0f);
        }

        private void SetMovePaths(Vector3 position)
        {
            if (!Entity.CanMove())
                return;
            _inputDirection = null;
            _moveByDestination = true;
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
            shouldSendReliably = false;
            if (movementSecure == MovementSecure.NotSecure && IsOwnerClient && !IsServer)
            {
                // Sync transform from owner client to server (except it's both owner client and server)
                if (_isClientConfirmingTeleport)
                {
                    shouldSendReliably = true;
                    MovementState |= MovementState.IsTeleport;
                }
                this.ClientWriteSyncTransform3D(writer);
                _isClientConfirmingTeleport = false;
                return true;
            }
            if (movementSecure == MovementSecure.ServerAuthoritative && IsOwnerClient && !IsServer)
            {
                _currentInput = Entity.SetInputExtraMovementState(_currentInput, _tempExtraMovementState);
                if (_isClientConfirmingTeleport)
                {
                    shouldSendReliably = true;
                    _currentInput.MovementState |= MovementState.IsTeleport;
                }
                if (Entity.DifferInputEnoughToSend(_oldInput, _currentInput, out EntityMovementInputState inputState))
                {
                    if (!_currentInput.IsKeyMovement)
                    {
                        // Point click should be reliably
                        shouldSendReliably = true;
                    }
                    this.ClientWriteMovementInput3D(writer, inputState, _currentInput);
                    _isClientConfirmingTeleport = false;
                    _oldInput = _currentInput;
                    _currentInput = null;
                    return true;
                }
            }
            return false;
        }

        public bool WriteServerState(long writeTimestamp, NetDataWriter writer, out bool shouldSendReliably)
        {
            shouldSendReliably = false;
            if (_isTeleporting)
            {
                shouldSendReliably = true;
                if (_stillMoveAfterTeleport)
                    MovementState |= MovementState.IsTeleport;
                else
                    MovementState = MovementState.IsTeleport;
            }
            else
            {
                MovementState &= ~MovementState.IsTeleport;
            }
            // Sync transform from server to all clients (include owner client)
            this.ServerWriteSyncTransform3D(_movementForceAppliers, writer);
            _isTeleporting = false;
            _stillMoveAfterTeleport = false;
            return true;
        }

        public void ReadClientStateAtServer(long peerTimestamp, NetDataReader reader)
        {
            switch (movementSecure)
            {
                case MovementSecure.NotSecure:
                    ReadSyncTransformAtServer(peerTimestamp, reader);
                    break;
                case MovementSecure.ServerAuthoritative:
                    ReadMovementInputAtServer(peerTimestamp, reader);
                    break;
            }
        }

        public void ReadServerStateAtClient(long peerTimestamp, NetDataReader reader)
        {
            if (IsServer)
            {
                // Don't read and apply transform, because it was done at server
                return;
            }
            reader.ClientReadSyncTransformMessage3D(out MovementState movementState, out ExtraMovementState extraMovementState, out Vector3 position, out float yAngle, out List<EntityMovementForceApplier> movementForceAppliers);
            _movementForceAppliers.Clear();
            _movementForceAppliers.AddRange(movementForceAppliers);
            if (movementState.Has(MovementState.IsTeleport))
            {
                // Server requested to teleport
                OnTeleport(position, yAngle, movementState != MovementState.IsTeleport);
            }
            else if (_acceptedPositionTimestamp <= peerTimestamp)
            {
                // Prepare time
                long lagDeltaTime = Entity.Manager.Rtt;
                long deltaTime = lagDeltaTime + peerTimestamp - _acceptedPositionTimestamp;
                float unityDeltaTime = (float)deltaTime * s_timestampToUnityTimeMultiplier;
                if (Vector3.Distance(position, CacheTransform.position) >= snapThreshold)
                {
                    // Snap character to the position if character is too far from the position
                    if (movementSecure == MovementSecure.ServerAuthoritative || !IsOwnerClient)
                    {
                        CacheTransform.eulerAngles = new Vector3(0, yAngle, 0);
                        CacheNavMeshAgent.Warp(position);
                    }
                }
                else if (!IsOwnerClient)
                {
                    _targetYAngle = yAngle;
                    _yTurnSpeed = 1f / unityDeltaTime;
                    SetMovePaths(position);
                }
                if (movementState.HasDirectionMovement())
                {
                    _acceptedMovementStateBeforeStopped = movementState;
                    _acceptedExtraMovementStateBeforeStopped = extraMovementState;
                }
                _acceptedPositionTimestamp = peerTimestamp;
            }
        }

        public void ReadMovementInputAtServer(long peerTimestamp, NetDataReader reader)
        {
            if (IsOwnerClient)
            {
                // Don't read and apply inputs, because it was done (this is both owner client and server)
                return;
            }
            if (movementSecure == MovementSecure.NotSecure)
            {
                // Movement handling at client, so don't read movement inputs from client (but have to read transform)
                return;
            }
            reader.ReadMovementInputMessage3D(out EntityMovementInputState inputState, out EntityMovementInput entityMovementInput);
            if (entityMovementInput.MovementState.Has(MovementState.IsTeleport))
            {
                // Teleport confirming from client
                _isServerWaitingTeleportConfirm = false;
            }
            if (_isServerWaitingTeleportConfirm)
            {
                // Waiting for teleport confirming
                return;
            }
            if (Mathf.Abs(peerTimestamp - BaseGameNetworkManager.Singleton.ServerTimestamp) > s_lagBuffer)
            {
                // Timestamp is a lot difference to server's timestamp, player might try to hack a game or packet may corrupted occurring, so skip it
                return;
            }
            if (!Entity.CanMove())
            {
                // It can't move, so don't move
                return;
            }
            if (_acceptedPositionTimestamp <= peerTimestamp)
            {
                // Prepare time
                long lagDeltaTime = Entity.Player.Rtt;
                long deltaTime = lagDeltaTime + peerTimestamp - _acceptedPositionTimestamp;
                float unityDeltaTime = (float)deltaTime * s_timestampToUnityTimeMultiplier;
                _tempExtraMovementState = entityMovementInput.ExtraMovementState;
                if (inputState.Has(EntityMovementInputState.PositionChanged))
                {
                    SetMovePaths(entityMovementInput.Position);
                }
                if (inputState.Has(EntityMovementInputState.RotationChanged))
                {
                    if (IsClient)
                    {
                        _targetYAngle = entityMovementInput.YAngle;
                        _yTurnSpeed = 1f / unityDeltaTime;
                    }
                    else
                    {
                        _yAngle = _targetYAngle = entityMovementInput.YAngle;
                        UpdateRotation();
                    }
                }
                if (inputState.Has(EntityMovementInputState.IsStopped))
                    StopMoveFunction();
                _acceptedPositionTimestamp = peerTimestamp;
            }
        }

        public void ReadSyncTransformAtServer(long peerTimestamp, NetDataReader reader)
        {
            if (IsOwnerClient)
            {
                // Don't read and apply transform, because it was done (this is both owner client and server)
                return;
            }
            if (movementSecure == MovementSecure.ServerAuthoritative)
            {
                // Movement handling at server, so don't read sync transform from client
                return;
            }
            reader.ServerReadSyncTransformMessage3D(out MovementState movementState, out ExtraMovementState extraMovementState, out Vector3 position, out float yAngle);
            if (movementState.Has(MovementState.IsTeleport))
            {
                // Teleport confirming from client
                _isServerWaitingTeleportConfirm = false;
            }
            if (_isServerWaitingTeleportConfirm)
            {
                // Waiting for teleport confirming
                return;
            }
            if (Mathf.Abs(peerTimestamp - BaseGameNetworkManager.Singleton.ServerTimestamp) > s_lagBuffer)
            {
                // Timestamp is a lot difference to server's timestamp, player might try to hack a game or packet may corrupted occurring, so skip it
                return;
            }
            if (_acceptedPositionTimestamp <= peerTimestamp)
            {
                // Prepare time
                long lagDeltaTime = Entity.Player.Rtt;
                long deltaTime = lagDeltaTime + peerTimestamp - _acceptedPositionTimestamp;
                float unityDeltaTime = (float)deltaTime * s_timestampToUnityTimeMultiplier;
                // Prepare movement state
                MovementState = movementState;
                ExtraMovementState = extraMovementState;
                if (!IsClient)
                {
                    Vector3 oldPos = CacheTransform.position;
                    Vector3 newPos = position;
                    // Calculate moveable distance
                    float horMoveSpd = Entity.GetMoveSpeed(MovementState, ExtraMovementState);
                    float horMoveableDist = (float)horMoveSpd * unityDeltaTime;
                    if (horMoveableDist < 0.001f)
                        horMoveableDist = 0.001f;
                    // Movement validating, if it is valid, set the position follow the client, if not set position to proper one and tell client to teleport
                    float clientHorMoveDist = Vector3.Distance(oldPos.GetXZ(), newPos.GetXZ());
                    if (clientHorMoveDist <= horMoveableDist + _lastServerValidateHorDistDiff)
                    {
                        // Allow to move to the position
                        CacheNavMeshAgent.Warp(position);
                        _lastServerValidateHorDistDiff = horMoveableDist - clientHorMoveDist;
                        // Update character rotation
                        _yAngle = _targetYAngle = yAngle;
                        UpdateRotation();
                    }
                    else
                    {
                        // Client moves too fast, adjust it
                        Vector3 dir = (newPos.GetXZ() - oldPos.GetXZ()).normalized;
                        Vector3 deltaMove = dir * Mathf.Min(clientHorMoveDist, horMoveableDist);
                        newPos = oldPos + deltaMove;
                        // And also adjust client's position
                        Teleport(newPos, Quaternion.Euler(0f, yAngle, 0f), true);
                        // Reset distance difference
                        _lastServerValidateHorDistDiff = 0f;
                    }
                }
                else
                {
                    // It's both server and client, translate position (it's a host so don't do speed hack validation)
                    if (Vector3.Distance(position, CacheTransform.position) > s_minDistanceToSimulateMovement)
                        SetMovePaths(position);
                    // Simulate character turning
                    _targetYAngle = yAngle;
                    _yTurnSpeed = 1f / unityDeltaTime;
                }
                _acceptedPositionTimestamp = peerTimestamp;
            }
        }

        protected virtual void OnTeleport(Vector3 position, float yAngle, bool stillMoveAfterTeleport)
        {
            _inputDirection = null;
            _moveByDestination = false;
            Vector3 beforeWarpDest = CacheNavMeshAgent.destination;
            CacheNavMeshAgent.Warp(position);
            if (!stillMoveAfterTeleport && CacheNavMeshAgent.isOnNavMesh)
                CacheNavMeshAgent.isStopped = true;
            if (stillMoveAfterTeleport && CacheNavMeshAgent.isOnNavMesh)
                CacheNavMeshAgent.SetDestination(beforeWarpDest);
            _yAngle = _targetYAngle = yAngle;
            UpdateRotation();
            if (IsServer && !IsOwnedByServer)
                _isServerWaitingTeleportConfirm = true;
            if (!IsServer && IsOwnerClient)
                _isClientConfirmingTeleport = true;
        }

        public bool CanPredictMovement()
        {
            return Entity.IsOwnerClient || (Entity.IsOwnerClientOrOwnedByServer && movementSecure == MovementSecure.NotSecure) || (Entity.IsServer && movementSecure == MovementSecure.ServerAuthoritative);
        }
    }
}
