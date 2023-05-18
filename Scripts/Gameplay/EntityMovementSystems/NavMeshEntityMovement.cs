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
        protected static readonly float s_lagBufferUnityTime = 0.2f;

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

        protected float _lastServerValidateTransformTime;
        protected float _lastServerValidateTransformMoveSpeed;
        protected long _acceptedPositionTimestamp;
        protected float _yAngle;
        protected float _targetYAngle;
        protected float _yTurnSpeed;
        protected bool _lookRotationApplied;
        protected EntityMovementInput _oldInput;
        protected EntityMovementInput _currentInput;
        protected ExtraMovementState _tempExtraMovementState;
        protected Vector3? _inputDirection;
        protected bool _moveByDestination;
        protected bool _isTeleporting;
        protected bool _isServerWaitingTeleportConfirm;
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
                Entity.SetInputStop(_currentInput);
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

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (!IsServer)
            {
                Logging.LogWarning(nameof(NavMeshEntityMovement), "Teleport function shouldn't be called at client [" + name + "]");
                return;
            }
            _isTeleporting = true;
            OnTeleport(position, rotation.eulerAngles.y);
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

        public override void EntityUpdate()
        {
            Profiler.BeginSample("NavMeshEntityMovement - Update");
            CacheNavMeshAgent.speed = Entity.GetMoveSpeed();
            float deltaTime = Time.deltaTime;
            bool isStationary = !CacheNavMeshAgent.isOnNavMesh || CacheNavMeshAgent.isStopped || CacheNavMeshAgent.remainingDistance <= CacheNavMeshAgent.stoppingDistance;
            CacheNavMeshAgent.obstacleAvoidanceType = isStationary ? obstacleAvoidanceWhileStationary : obstacleAvoidanceWhileMoving;
            if (CanPredictMovement())
            {
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
                    MovementState = (CacheNavMeshAgent.velocity.sqrMagnitude > 0f ? MovementState.Forward : MovementState.None) | MovementState.IsGrounded;
                    // Turn character to destination
                    if (_lookRotationApplied && Entity.CanTurn() && CacheNavMeshAgent.velocity.sqrMagnitude > 0f)
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
            // Update rotating
            if (_yTurnSpeed <= 0f)
                _yAngle = _targetYAngle;
            else if (Mathf.Abs(_yAngle - _targetYAngle) > 1f)
                _yAngle = Mathf.LerpAngle(_yAngle, _targetYAngle, _yTurnSpeed * deltaTime);
            UpdateRotation();
            _lookRotationApplied = true;
            _currentInput = Entity.SetInputRotation(_currentInput, CacheTransform.rotation);
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
                CacheNavMeshAgent.SetDestination(position);
            }
        }

        public bool WriteClientState(NetDataWriter writer, out bool shouldSendReliably)
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
                    this.ClientWriteMovementInput3D(writer, inputState, _currentInput.MovementState, _currentInput.ExtraMovementState, _currentInput.Position, _currentInput.Rotation);
                    _isClientConfirmingTeleport = false;
                    _oldInput = _currentInput;
                    _currentInput = null;
                    return true;
                }
            }
            return false;
        }

        public bool WriteServerState(NetDataWriter writer, out bool shouldSendReliably)
        {
            shouldSendReliably = false;
            if (_isTeleporting)
            {
                shouldSendReliably = true;
                MovementState |= MovementState.IsTeleport;
            }
            else
            {
                MovementState &= ~MovementState.IsTeleport;
            }
            // Sync transform from server to all clients (include owner client)
            this.ServerWriteSyncTransform3D(writer);
            _isTeleporting = false;
            return true;
        }

        public void ReadClientStateAtServer(NetDataReader reader)
        {
            switch (movementSecure)
            {
                case MovementSecure.NotSecure:
                    ReadSyncTransformAtServer(reader);
                    break;
                case MovementSecure.ServerAuthoritative:
                    ReadMovementInputAtServer(reader);
                    break;
            }
        }

        public void ReadServerStateAtClient(NetDataReader reader)
        {
            if (IsServer)
            {
                // Don't read and apply transform, because it was done at server
                return;
            }
            reader.ReadSyncTransformMessage3D(out MovementState movementState, out ExtraMovementState extraMovementState, out Vector3 position, out float yAngle, out long timestamp);
            if (movementState.Has(MovementState.IsTeleport))
            {
                // Server requested to teleport
                OnTeleport(position, yAngle);
            }
            else if (_acceptedPositionTimestamp <= timestamp)
            {
                if (Vector3.Distance(position, CacheTransform.position) >= snapThreshold)
                {
                    // Snap character to the position if character is too far from the position
                    if (movementSecure == MovementSecure.ServerAuthoritative || !IsOwnerClient)
                    {
                        CacheTransform.eulerAngles = new Vector3(0, yAngle, 0);
                        CacheNavMeshAgent.Warp(position);
                    }
                    MovementState = movementState;
                    ExtraMovementState = extraMovementState;
                }
                else if (!IsOwnerClient)
                {
                    _targetYAngle = yAngle;
                    _yTurnSpeed = 1f / Time.fixedDeltaTime;
                    SetMovePaths(position);
                    MovementState = movementState;
                    ExtraMovementState = extraMovementState;
                }
                _acceptedPositionTimestamp = timestamp;
            }
        }

        public void ReadMovementInputAtServer(NetDataReader reader)
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
            reader.ReadMovementInputMessage3D(out EntityMovementInputState inputState, out MovementState movementState, out ExtraMovementState extraMovementState, out Vector3 position, out float yAngle, out long timestamp);
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
            if (Mathf.Abs(timestamp - BaseGameNetworkManager.Singleton.ServerTimestamp) > s_lagBuffer)
            {
                // Timestamp is a lot difference to server's timestamp, player might try to hack a game or packet may corrupted occurring, so skip it
                return;
            }
            if (!Entity.CanMove())
            {
                // It can't move, so don't move
                return;
            }
            if (_acceptedPositionTimestamp <= timestamp)
            {
                _tempExtraMovementState = extraMovementState;
                if (inputState.Has(EntityMovementInputState.PositionChanged))
                {
                    SetMovePaths(position);
                }
                if (inputState.Has(EntityMovementInputState.RotationChanged))
                {
                    if (IsClient)
                    {
                        _targetYAngle = yAngle;
                        _yTurnSpeed = 1f / Time.fixedDeltaTime;
                    }
                    else
                    {
                        _yAngle = _targetYAngle = yAngle;
                        UpdateRotation();
                    }
                }
                if (inputState.Has(EntityMovementInputState.IsStopped))
                    StopMoveFunction();
                _acceptedPositionTimestamp = timestamp;
            }
        }

        public void ReadSyncTransformAtServer(NetDataReader reader)
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
            reader.ReadSyncTransformMessage3D(out MovementState movementState, out ExtraMovementState extraMovementState, out Vector3 position, out float yAngle, out long timestamp);
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
            if (Mathf.Abs(timestamp - BaseGameNetworkManager.Singleton.ServerTimestamp) > s_lagBuffer)
            {
                // Timestamp is a lot difference to server's timestamp, player might try to hack a game or packet may corrupted occurring, so skip it
                return;
            }
            if (_acceptedPositionTimestamp <= timestamp)
            {
                if (IsClient)
                {
                    _targetYAngle = yAngle;
                    _yTurnSpeed = 1f / Time.fixedDeltaTime;
                }
                else
                {
                    _yAngle = _targetYAngle = yAngle;
                    UpdateRotation();
                }
                MovementState = movementState;
                ExtraMovementState = extraMovementState;
                if (Vector3.Distance(position.GetXZ(), CacheTransform.position.GetXZ()) > 0.01f)
                {
                    if (!IsClient)
                    {
                        // If it's server only (not a host), set position follows the client immediately
                        float currentTime = Time.unscaledTime;
                        float t = currentTime - _lastServerValidateTransformTime;
                        float v = Entity.GetMoveSpeed();
                        float s = (_lastServerValidateTransformMoveSpeed * (t + s_lagBufferUnityTime)) + (v * t); // +`lagBufferUnityTime` as high ping buffer
                        if (s < 0.001f)
                            s = 0.001f;
                        Vector3 oldPos = CacheTransform.position;
                        Vector3 newPos = position;
                        if (Vector3.Distance(oldPos, newPos) <= s)
                        {
                            // Allow to move to the position
                            CacheNavMeshAgent.Warp(position);
                        }
                        else
                        {
                            // Client moves too fast, adjust it
                            Vector3 dir = (newPos - oldPos).normalized;
                            newPos = oldPos + (dir * s);
                            CacheNavMeshAgent.Warp(position);
                            // And also adjust client's position
                            Teleport(newPos, Quaternion.Euler(0f, yAngle, 0f));
                        }
                        _lastServerValidateTransformTime = currentTime;
                        _lastServerValidateTransformMoveSpeed = v;
                    }
                    else
                    {
                        // It's both server and client, translate position (it's a host so don't do speed hack validation)
                        SetMovePaths(position);
                    }
                }
                _acceptedPositionTimestamp = timestamp;
            }
        }

        protected virtual void OnTeleport(Vector3 position, float yAngle)
        {
            _inputDirection = null;
            _moveByDestination = false;
            CacheNavMeshAgent.Warp(position);
            if (CacheNavMeshAgent.isOnNavMesh)
                CacheNavMeshAgent.isStopped = true;
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
