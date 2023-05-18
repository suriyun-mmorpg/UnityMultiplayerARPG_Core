using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class RigidBodyEntityMovement2D : BaseNetworkedGameEntityComponent<BaseGameEntity>, IEntityMovementComponent
    {
        protected static readonly long s_lagBuffer = System.TimeSpan.TicksPerMillisecond * 200;
        protected static readonly float s_lagBufferUnityTime = 0.2f;

        [Header("Movement Settings")]
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;
        public MovementSecure movementSecure = MovementSecure.NotSecure;

        [Header("Networking Settings")]
        public float snapThreshold = 5.0f;

        public Rigidbody2D CacheRigidbody2D { get; private set; }

        public float StoppingDistance
        {
            get { return stoppingDistance; }
        }
        public MovementState MovementState { get; protected set; }
        public ExtraMovementState ExtraMovementState { get; protected set; }
        public DirectionVector2 Direction2D { get; set; }
        public float CurrentMoveSpeed { get; private set; }

        public Queue<Vector2> NavPaths { get; protected set; }
        public bool HasNavPaths
        {
            get { return NavPaths != null && NavPaths.Count > 0; }
        }

        protected float _lastServerValidateTransformTime;
        protected float _lastServerValidateTransformMoveSpeed;
        protected long _acceptedPositionTimestamp;
        protected Vector2? _clientTargetPosition;
        protected EntityMovementInput _oldInput;
        protected EntityMovementInput _currentInput;
        protected MovementState _tempMovementState;
        protected ExtraMovementState _tempExtraMovementState;
        protected Vector2 _inputDirection;
        protected Vector2 _moveDirection;
        protected float? _lagMoveSpeedRate;
        protected bool _isTeleporting;
        protected bool _isServerWaitingTeleportConfirm;
        protected bool _isClientConfirmingTeleport;

        public override void EntityAwake()
        {
            // Prepare rigidbody component
            CacheRigidbody2D = gameObject.GetOrAddComponent<Rigidbody2D>();
            // Disable unused component
            LiteNetLibTransform disablingComp = gameObject.GetComponent<LiteNetLibTransform>();
            if (disablingComp != null)
            {
                Logging.LogWarning(nameof(RigidBodyEntityMovement2D), "You can remove `LiteNetLibTransform` component from game entity, it's not being used anymore [" + name + "]");
                disablingComp.enabled = false;
            }
            // Setup
            CacheRigidbody2D.gravityScale = 0;
            StopMoveFunction();
        }

        public override void EntityStart()
        {
            _isClientConfirmingTeleport = true;
        }

        public override void ComponentOnEnable()
        {
            CacheRigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        public override void ComponentOnDisable()
        {
            CacheRigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        public override void OnSetOwnerClient(bool isOwnerClient)
        {
            base.OnSetOwnerClient(isOwnerClient);
            _clientTargetPosition = null;
            NavPaths = null;
        }

        public virtual void StopMove()
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
            NavPaths = null;
            _lagMoveSpeedRate = null;
        }

        public virtual void KeyMovement(Vector3 moveDirection, MovementState movementState)
        {
            if (!Entity.CanMove())
                return;
            if (CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                _inputDirection = moveDirection;
                _tempMovementState = movementState;
                if (_inputDirection.sqrMagnitude > 0)
                    NavPaths = null;
            }
        }

        public virtual void PointClickMovement(Vector3 position)
        {
            if (!Entity.CanMove())
                return;
            if (CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                SetMovePaths(position, true);
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

        public virtual void SetLookRotation(Quaternion rotation)
        {
            if (!Entity.CanMove() || !Entity.CanTurn())
                return;
            if (CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                if (!HasNavPaths)
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

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (!IsServer)
            {
                Logging.LogWarning(nameof(RigidBodyEntityMovement2D), "Teleport function shouldn't be called at client [" + name + "]");
                return;
            }
            _isTeleporting = true;
            OnTeleport(position);
        }

        public bool FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
        {
            result = fromPosition;
            return true;
        }

        public override void EntityUpdate()
        {
            UpdateMovement(Time.deltaTime);
            if (CanPredictMovement())
            {
                _tempMovementState = _moveDirection.sqrMagnitude > 0f ? _tempMovementState : MovementState.None;
                _tempMovementState |= MovementState.IsGrounded;
                // Update movement state
                MovementState = _tempMovementState;
                // Update extra movement state
                ExtraMovementState = this.ValidateExtraMovementState(MovementState, _tempExtraMovementState);
                // Set current input
                _currentInput = Entity.SetInputMovementState(_currentInput, MovementState);
                _currentInput = Entity.SetInputExtraMovementState(_currentInput, ExtraMovementState);
            }
            else
            {
                // Update movement state
                if (HasNavPaths && !MovementState.Has(MovementState.Forward))
                    MovementState |= MovementState.Forward;
            }
        }

        protected virtual void UpdateMovement(float deltaTime)
        {
            float tempSqrMagnitude;
            float tempPredictSqrMagnitude;
            float tempTargetDistance;
            float tempEntityMoveSpeed;
            float tempMaxMoveSpeed;
            Vector2 tempMoveVelocity;
            Vector2 tempCurrentPosition;
            Vector2 tempTargetPosition;
            Vector2 tempPredictPosition;

            tempCurrentPosition = CacheTransform.position;
            tempMoveVelocity = Vector3.zero;
            _moveDirection = Vector2.zero;
            tempTargetDistance = 0f;

            if (HasNavPaths)
            {
                // Set `tempTargetPosition` and `tempCurrentPosition`
                tempTargetPosition = NavPaths.Peek();
                _moveDirection = (tempTargetPosition - tempCurrentPosition).normalized;
                tempTargetDistance = Vector2.Distance(tempTargetPosition, tempCurrentPosition);
                if (!_tempMovementState.Has(MovementState.Forward))
                    _tempMovementState |= MovementState.Forward;
                if (tempTargetDistance < StoppingDistance)
                {
                    NavPaths.Dequeue();
                    if (!HasNavPaths)
                    {
                        StopMoveFunction();
                        _moveDirection = Vector2.zero;
                    }
                }
                else
                {
                    // Turn character to destination
                    Direction2D = _moveDirection;
                }
            }
            else if (_clientTargetPosition.HasValue)
            {
                tempTargetPosition = _clientTargetPosition.Value;
                _moveDirection = (tempTargetPosition - tempCurrentPosition).normalized;
                tempTargetDistance = Vector2.Distance(tempTargetPosition, tempCurrentPosition);
                if (tempTargetDistance < 0.001f)
                {
                    _clientTargetPosition = null;
                    StopMoveFunction();
                    _moveDirection = Vector2.zero;
                }
            }
            else if (_inputDirection.sqrMagnitude > 0f)
            {
                _moveDirection = _inputDirection.normalized;
                tempTargetPosition = tempCurrentPosition + _moveDirection;
            }
            else
            {
                tempTargetPosition = tempCurrentPosition;
            }

            if (!Entity.CanMove())
            {
                _moveDirection = Vector2.zero;
            }

            // Prepare movement speed
            tempEntityMoveSpeed = Entity.GetMoveSpeed();
            tempMaxMoveSpeed = tempEntityMoveSpeed;

            // Updating horizontal movement (WASD inputs)
            if (_moveDirection.sqrMagnitude > 0f)
            {
                // If character move backward
                CurrentMoveSpeed = CalculateCurrentMoveSpeed(tempMaxMoveSpeed, deltaTime);

                // NOTE: `tempTargetPosition` and `tempCurrentPosition` were set above
                tempSqrMagnitude = (tempTargetPosition - tempCurrentPosition).sqrMagnitude;
                tempPredictPosition = tempCurrentPosition + (_moveDirection * CurrentMoveSpeed * deltaTime);
                tempPredictSqrMagnitude = (tempPredictPosition - tempCurrentPosition).sqrMagnitude;
                if (HasNavPaths || _clientTargetPosition.HasValue)
                {
                    // Check `tempSqrMagnitude` against the `tempPredictSqrMagnitude`
                    // if `tempPredictSqrMagnitude` is greater than `tempSqrMagnitude`,
                    // rigidbody will reaching target and character is moving pass it,
                    // so adjust move speed by distance and time (with physic formula: v=s/t)
                    if (tempPredictSqrMagnitude >= tempSqrMagnitude && tempTargetDistance > 0f)
                        CurrentMoveSpeed *= tempTargetDistance / deltaTime / CurrentMoveSpeed;
                }
                tempMoveVelocity = _moveDirection * CurrentMoveSpeed;
                // Set inputs
                if (HasNavPaths)
                {
                    _currentInput = Entity.SetInputPosition(_currentInput, tempTargetPosition);
                    _currentInput = Entity.SetInputIsKeyMovement(_currentInput, false);
                }
                else
                {
                    _currentInput = Entity.SetInputPosition(_currentInput, tempPredictPosition);
                    _currentInput = Entity.SetInputIsKeyMovement(_currentInput, true);
                }
            }
            _currentInput = Entity.SetInputDirection2D(_currentInput, Direction2D);
            CacheRigidbody2D.velocity = tempMoveVelocity;
        }

        private float CalculateCurrentMoveSpeed(float maxMoveSpeed, float deltaTime)
        {
            // Adjust speed by rtt
            if (!IsServer && IsOwnerClient && movementSecure == MovementSecure.ServerAuthoritative)
            {
                float rtt = 0.001f * Entity.Manager.Rtt;
                float acc = 1f / rtt * deltaTime * 0.5f;
                if (!_lagMoveSpeedRate.HasValue)
                    _lagMoveSpeedRate = 0f;
                if (_lagMoveSpeedRate < 1f)
                    _lagMoveSpeedRate += acc;
                if (_lagMoveSpeedRate > 1f)
                    _lagMoveSpeedRate = 1f;
                return maxMoveSpeed * _lagMoveSpeedRate.Value;
            }
            // TODO: Adjust other's client move speed by rtt
            return maxMoveSpeed;
        }

        protected virtual void SetMovePaths(Vector2 position, bool useNavMesh)
        {
            // TODO: Implement nav mesh
            NavPaths = new Queue<Vector2>();
            NavPaths.Enqueue(position);
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
                this.ClientWriteSyncTransform2D(writer);
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
                    this.ClientWriteMovementInput2D(writer, inputState, _currentInput.MovementState, _currentInput.ExtraMovementState, _currentInput.Position, _currentInput.Direction2D);
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
            this.ServerWriteSyncTransform2D(writer);
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
            reader.ReadSyncTransformMessage2D(out MovementState movementState, out ExtraMovementState extraMovementState, out Vector2 position, out DirectionVector2 direction2D, out long timestamp);
            if (movementState.Has(MovementState.IsTeleport))
            {
                // Server requested to teleport
                OnTeleport(position);
            }
            else if (_acceptedPositionTimestamp <= timestamp)
            {
                if (Vector2.Distance(position, CacheTransform.position) >= snapThreshold)
                {
                    // Snap character to the position if character is too far from the position
                    if (movementSecure == MovementSecure.ServerAuthoritative || !IsOwnerClient)
                    {
                        Direction2D = direction2D;
                        CacheTransform.position = position;
                    }
                    MovementState = movementState;
                    ExtraMovementState = extraMovementState;
                }
                else if (!IsOwnerClient)
                {
                    Direction2D = direction2D;
                    _clientTargetPosition = position;
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
            reader.ReadMovementInputMessage2D(out EntityMovementInputState inputState, out MovementState movementState, out ExtraMovementState extraMovementState, out Vector2 position, out DirectionVector2 direction2D, out long timestamp);
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
                NavPaths = null;
                _tempMovementState = movementState;
                _tempExtraMovementState = extraMovementState;
                if (inputState.Has(EntityMovementInputState.PositionChanged))
                {
                    if (inputState.Has(EntityMovementInputState.IsKeyMovement))
                    {
                        _clientTargetPosition = position;
                    }
                    else
                    {
                        SetMovePaths(position, true);
                    }
                }
                Direction2D = direction2D;
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
            reader.ReadSyncTransformMessage2D(out MovementState movementState, out ExtraMovementState extraMovementState, out Vector2 position, out DirectionVector2 direction2D, out long timestamp);
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
                Direction2D = direction2D;
                MovementState = movementState;
                ExtraMovementState = extraMovementState;
                if (!IsClient)
                {
                    // If it's server only (not a host), set position follows the client immediately
                    float currentTime = Time.unscaledTime;
                    float t = currentTime - _lastServerValidateTransformTime;
                    float v = Entity.GetMoveSpeed();
                    float s = (_lastServerValidateTransformMoveSpeed * (t + s_lagBufferUnityTime)) + (v * t); // +`lagBufferUnityTime` as high ping buffer
                    if (s < 0.001f)
                        s = 0.001f;
                    Vector2 oldPos = CacheTransform.position.GetXY();
                    Vector2 newPos = position;
                    if (Vector2.Distance(oldPos, newPos) <= s)
                    {
                        // Allow to move to the position
                        CacheTransform.position = position;
                    }
                    else
                    {
                        // Client moves too fast, adjust it
                        Vector2 dir = (newPos - oldPos).normalized;
                        newPos = oldPos + (dir * s);
                        newPos.y = position.y;
                        CacheTransform.position = newPos;
                        // And also adjust client's position
                        Teleport(newPos, Quaternion.identity);
                    }
                    _lastServerValidateTransformTime = currentTime;
                    _lastServerValidateTransformMoveSpeed = v;
                }
                else
                {
                    // It's both server and client, translate position (it's a host so don't do speed hack validation)
                    if (Vector3.Distance(position, CacheTransform.position) > 0.01f)
                        SetMovePaths(position, false);
                }
                _acceptedPositionTimestamp = timestamp;
            }
        }

        protected virtual void OnTeleport(Vector2 position)
        {
            _clientTargetPosition = null;
            NavPaths = null;
            CacheTransform.position = position;
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
