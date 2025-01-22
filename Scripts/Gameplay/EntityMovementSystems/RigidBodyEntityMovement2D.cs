using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class RigidBodyEntityMovement2D : BaseNetworkedGameEntityComponent<BaseGameEntity>, IEntityMovementComponent
    {
        protected static readonly float s_minDistanceToSimulateMovement = 0.01f;
        protected static readonly float s_timestampToUnityTimeMultiplier = 0.001f;

        [Header("Movement Settings")]
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;
        public MovementSecure movementSecure = MovementSecure.NotSecure;

        [Header("Dashing")]
        public EntityMovementForceApplier dashingForceApplier;

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

        // Input codes
        protected bool _isDashing;
        protected Vector2 _inputDirection;
        protected MovementState _tempMovementState;
        protected ExtraMovementState _tempExtraMovementState;

        // Move simulate codes
        protected Vector2 _moveDirection;
        protected readonly List<EntityMovementForceApplier> _movementForceAppliers = new List<EntityMovementForceApplier>();

        // Teleport codes
        protected bool _isTeleporting;
        protected bool _stillMoveAfterTeleport;

        // Client state codes
        protected EntityMovementInput _oldInput;
        protected EntityMovementInput _currentInput;
        protected bool _sendingDash;

        // State simulate codes
        protected float? _lagMoveSpeedRate;
        protected bool _simulatingKeyMovement = false;

        // Peers accept codes
        protected bool _acceptedDash;
        protected long _acceptedPositionTimestamp;

        // Server validate codes
        protected float _lastServerValidateDistDiff;
        protected bool _isServerWaitingTeleportConfirm;

        // Client confirm codes
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
            NavPaths = null;
        }

        public virtual void StopMove()
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
                if (!_isDashing)
                    _isDashing = _tempMovementState.Has(MovementState.IsDash);
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

        public virtual void SetLookRotation(Quaternion rotation, bool immediately)
        {
            if (!Entity.CanMove() || !Entity.CanTurn())
                return;
            if (CanPredictMovement())
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

        public void Teleport(Vector3 position, Quaternion rotation, bool stillMoveAfterTeleport)
        {
            if (!IsServer)
            {
                Logging.LogWarning(nameof(RigidBodyEntityMovement2D), "Teleport function shouldn't be called at client [" + name + "]");
                return;
            }
            _isTeleporting = true;
            _stillMoveAfterTeleport = stillMoveAfterTeleport;
            OnTeleport(position, stillMoveAfterTeleport);
        }

        public bool FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
        {
            result = fromPosition;
            return true;
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

        public override void EntityUpdate()
        {
            UpdateMovement(Time.fixedDeltaTime);
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
            _isDashing = false;
            _acceptedDash = false;
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
                float stoppingDistance = _simulatingKeyMovement ? s_minDistanceToSimulateMovement : StoppingDistance;
                bool shouldStop = tempTargetDistance < stoppingDistance;
                if (shouldStop)
                {
                    NavPaths.Dequeue();
                    if (!HasNavPaths)
                    {
                        StopMoveFunction();
                        _moveDirection = Vector2.zero;
                    }
                    else
                    {
                        if (!_tempMovementState.Has(MovementState.Forward))
                            _tempMovementState |= MovementState.Forward;
                    }
                }
                else
                {
                    if (!_tempMovementState.Has(MovementState.Forward))
                        _tempMovementState |= MovementState.Forward;
                    // Turn character to destination
                    Direction2D = _moveDirection;
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
                StopMove();
            }

            if (!Entity.CanMove())
            {
                _moveDirection = Vector2.zero;
            }

            if (!Entity.CanDash())
            {
                _isDashing = false;
            }

            // Prepare movement speed
            tempEntityMoveSpeed = Entity.GetMoveSpeed();
            tempMaxMoveSpeed = tempEntityMoveSpeed;

            // Dashing
            if (_acceptedDash || _isDashing)
            {
                _sendingDash = true;
                dashingForceApplier.Apply(Direction2D);
                dashingForceApplier.Mode = ApplyMovementForceMode.Dash;
                // Can have only one replace movement force applier, so remove stored ones
                _movementForceAppliers.RemoveReplaceMovementForces();
                _movementForceAppliers.Add(dashingForceApplier);
            }

            // Apply Forces
            _movementForceAppliers.UpdateForces(Time.deltaTime,
                Entity.GetMoveSpeed(MovementState.Forward, ExtraMovementState.None),
                out Vector3 forceMotion, out EntityMovementForceApplier replaceMovementForceApplier);

            // Replace player's movement by this
            if (replaceMovementForceApplier != null)
            {
                // Still dashing to add dash to movement state
                if (replaceMovementForceApplier.Mode == ApplyMovementForceMode.Dash)
                    _tempMovementState |= MovementState.IsDash;
                // Force turn to dashed direction
                _moveDirection = replaceMovementForceApplier.Direction;
                Direction2D = _moveDirection;
                // Change move speed to dash force
                tempMaxMoveSpeed = replaceMovementForceApplier.CurrentSpeed;
            }

            // Updating horizontal movement (WASD inputs)
            if (_moveDirection.sqrMagnitude > 0f)
            {
                // If character move backward
                CurrentMoveSpeed = CalculateCurrentMoveSpeed(tempMaxMoveSpeed, deltaTime);

                // NOTE: `tempTargetPosition` and `tempCurrentPosition` were set above
                tempSqrMagnitude = (tempTargetPosition - tempCurrentPosition).sqrMagnitude;
                tempPredictPosition = tempCurrentPosition + (_moveDirection * CurrentMoveSpeed * deltaTime);
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
#if UNITY_6000_0_OR_NEWER
            CacheRigidbody2D.linearVelocity = tempMoveVelocity + new Vector2(forceMotion.x, forceMotion.y);
#else
            CacheRigidbody2D.velocity = tempMoveVelocity + new Vector2(forceMotion.x, forceMotion.y);
#endif
        }

        private float CalculateCurrentMoveSpeed(float maxMoveSpeed, float deltaTime)
        {
            // Adjust speed by rtt
            if (!IsServer && IsOwnerClient && movementSecure == MovementSecure.ServerAuthoritative)
            {
                float rtt = s_timestampToUnityTimeMultiplier * Entity.Manager.Rtt;
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

        public bool WriteClientState(long writeTimestamp, NetDataWriter writer, out bool shouldSendReliably)
        {
            shouldSendReliably = false;
            if (movementSecure == MovementSecure.NotSecure && IsOwnerClient && !IsServer)
            {
                // Sync transform from owner client to server (except it's both owner client and server)
                if (_sendingDash)
                {
                    shouldSendReliably = true;
                    MovementState |= MovementState.IsDash;
                }
                else
                {
                    MovementState &= ~MovementState.IsDash;
                }
                if (_isClientConfirmingTeleport)
                {
                    shouldSendReliably = true;
                    MovementState |= MovementState.IsTeleport;
                }
                this.ClientWriteSyncTransform2D(writer);
                _sendingDash = false;
                _isClientConfirmingTeleport = false;
                return true;
            }
            if (movementSecure == MovementSecure.ServerAuthoritative && IsOwnerClient && !IsServer)
            {
                _currentInput = Entity.SetInputExtraMovementState(_currentInput, _tempExtraMovementState);
                if (_sendingDash)
                {
                    shouldSendReliably = true;
                    _currentInput = Entity.SetInputDash(_currentInput);
                }
                else
                {
                    _currentInput = Entity.ClearInputDash(_currentInput);
                }
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
                    this.ClientWriteMovementInput2D(writer, inputState, _currentInput);
                    _sendingDash = false;
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
            if (_sendingDash)
            {
                shouldSendReliably = true;
                MovementState |= MovementState.IsDash;
            }
            else
            {
                MovementState &= ~MovementState.IsDash;
            }
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
            this.ServerWriteSyncTransform2D(_movementForceAppliers, writer);
            _sendingDash = false;
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
            reader.ClientReadSyncTransformMessage2D(out MovementState movementState, out ExtraMovementState extraMovementState, out Vector2 position, out DirectionVector2 direction2D, out List<EntityMovementForceApplier> movementForceAppliers);
            _movementForceAppliers.Clear();
            _movementForceAppliers.AddRange(movementForceAppliers);
            if (movementState.Has(MovementState.IsTeleport))
            {
                // Server requested to teleport
                OnTeleport(position, movementState != MovementState.IsTeleport);
            }
            else if (_acceptedPositionTimestamp <= peerTimestamp)
            {
                if (Vector2.Distance(position, CacheTransform.position) >= snapThreshold)
                {
                    // Snap character to the position if character is too far from the position
                    if (movementSecure == MovementSecure.ServerAuthoritative || !IsOwnerClient)
                    {
                        Direction2D = direction2D;
                        CacheTransform.position = position;
                        CurrentGameManager.ShouldPhysicSyncTransforms2D = true;
                    }
                    MovementState = movementState;
                    ExtraMovementState = extraMovementState;
                }
                else if (!IsOwnerClient)
                {
                    Direction2D = direction2D;
                    _simulatingKeyMovement = true;
                    if (Vector2.Distance(position, CacheTransform.position.GetXY()) > s_minDistanceToSimulateMovement)
                        SetMovePaths(position, false);
                    else
                        NavPaths = null;
                    MovementState = movementState;
                    ExtraMovementState = extraMovementState;
                }
                _acceptedPositionTimestamp = peerTimestamp;
            }
            if (!IsOwnerClient && movementState.Has(MovementState.IsDash))
            {
                _acceptedDash = true;
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
            reader.ReadMovementInputMessage2D(out EntityMovementInputState inputState, out EntityMovementInput entityMovementInput);
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
            if (!Entity.CanMove())
            {
                // It can't move, so don't move
                return;
            }
            if (_acceptedPositionTimestamp <= peerTimestamp)
            {
                NavPaths = null;
                _tempMovementState = entityMovementInput.MovementState;
                _tempExtraMovementState = entityMovementInput.ExtraMovementState;
                if (inputState.Has(EntityMovementInputState.PositionChanged))
                {
                    _simulatingKeyMovement = inputState.Has(EntityMovementInputState.IsKeyMovement);
                    SetMovePaths(entityMovementInput.Position, !_simulatingKeyMovement);
                }
                Direction2D = entityMovementInput.Direction2D;
                if (entityMovementInput.MovementState.Has(MovementState.IsDash))
                {
                    _acceptedDash = true;
                }
                if (inputState.Has(EntityMovementInputState.IsStopped))
                {
                    StopMoveFunction();
                }
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
            reader.ServerReadSyncTransformMessage2D(out MovementState movementState, out ExtraMovementState extraMovementState, out Vector2 position, out DirectionVector2 direction2D);
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
            if (_acceptedPositionTimestamp <= peerTimestamp)
            {
                // Prepare time
                long lagDeltaTime = Entity.Player.Rtt;
                long deltaTime = lagDeltaTime + peerTimestamp - _acceptedPositionTimestamp;
                float unityDeltaTime = (float)deltaTime * s_timestampToUnityTimeMultiplier;
                // Prepare movement state
                Direction2D = direction2D;
                MovementState = movementState;
                ExtraMovementState = extraMovementState;
                if (!IsClient)
                {
                    Vector3 oldPos = CacheTransform.position;
                    Vector3 newPos = position;
                    // Calculate moveable distance
                    float moveSpd = Entity.GetMoveSpeed(MovementState, ExtraMovementState);
                    float moveableDist = (float)moveSpd * unityDeltaTime;
                    if (moveableDist < 0.001f)
                        moveableDist = 0.001f;
                    // Movement validating, if it is valid, set the position follow the client, if not set position to proper one and tell client to teleport
                    float clientMoveDist = Vector3.Distance(oldPos.GetXY(), newPos.GetXY());
                    if (clientMoveDist <= 0.001f || clientMoveDist <= moveableDist + _lastServerValidateDistDiff)
                    {
                        // Allow to move to the position
                        CacheTransform.position = newPos;
                        CurrentGameManager.ShouldPhysicSyncTransforms2D = true;
                        _lastServerValidateDistDiff = moveableDist - clientMoveDist;
                    }
                    else
                    {
                        // Client moves too fast, adjust it
                        Vector3 dir = (newPos.GetXY() - oldPos.GetXY()).normalized;
                        Vector3 deltaMove = dir * Mathf.Min(clientMoveDist, moveableDist);
                        newPos = oldPos + deltaMove;
                        // And also adjust client's position
                        Teleport(newPos, Quaternion.identity, true);
                        // Reset distance difference
                        _lastServerValidateDistDiff = 0f;
                    }
                }
                else
                {
                    // It's both server and client, translate position (it's a host so don't do speed hack validation)
                    if (Vector3.Distance(position, CacheTransform.position) > s_minDistanceToSimulateMovement)
                        SetMovePaths(position, false);
                }
                if (movementState.Has(MovementState.IsDash))
                {
                    _acceptedDash = true;
                }
                _acceptedPositionTimestamp = peerTimestamp;
            }
        }

        protected virtual void OnTeleport(Vector2 position, bool stillMoveAfterTeleport)
        {
            if (!stillMoveAfterTeleport)
                NavPaths = null;
            CacheTransform.position = position;
            CurrentGameManager.ShouldPhysicSyncTransforms2D = true;
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
