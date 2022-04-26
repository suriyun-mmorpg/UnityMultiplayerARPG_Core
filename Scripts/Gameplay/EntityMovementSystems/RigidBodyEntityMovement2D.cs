using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class RigidBodyEntityMovement2D : BaseNetworkedGameEntityComponent<BaseGameEntity>, IEntityMovementComponent
    {
        [Header("Movement Settings")]
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;

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

        public Queue<Vector2> NavPaths { get; protected set; }
        public bool HasNavPaths
        {
            get { return NavPaths != null && NavPaths.Count > 0; }
        }

        protected float lastServerValidateTransformTime;
        protected float lastServerValidateTransformMoveSpeed;
        protected long acceptedPositionTimestamp;
        protected Vector2? clientTargetPosition;
        protected EntityMovementInput oldInput;
        protected EntityMovementInput currentInput;
        protected MovementState tempMovementState;
        protected ExtraMovementState tempExtraMovementState;
        protected Vector2 inputDirection;
        protected Vector2 moveDirection;
        protected float? lagMoveSpeedRate;
        protected bool isTeleporting;

        public override void EntityAwake()
        {
            // Prepare rigidbody component
            CacheRigidbody2D = gameObject.GetOrAddComponent<Rigidbody2D>();
            // Disable unused component
            LiteNetLibTransform disablingComp = gameObject.GetComponent<LiteNetLibTransform>();
            if (disablingComp != null)
            {
                Logging.LogWarning("NavMeshEntityMovement", "You can remove `LiteNetLibTransform` component from game entity, it's not being used anymore [" + name + "]");
                disablingComp.enabled = false;
            }
            // Setup
            CacheRigidbody2D.gravityScale = 0;
            StopMoveFunction();
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
            clientTargetPosition = null;
            NavPaths = null;
        }

        public virtual void StopMove()
        {
            if (Entity.MovementSecure == MovementSecure.ServerAuthoritative)
            {
                // Send movement input to server, then server will apply movement and sync transform to clients
                this.SetInputStop(currentInput);
            }
            StopMoveFunction();
        }

        private void StopMoveFunction()
        {
            NavPaths = null;
            lagMoveSpeedRate = null;
        }

        public virtual void KeyMovement(Vector3 moveDirection, MovementState movementState)
        {
            if (!Entity.CanMove())
                return;
            if (this.CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                inputDirection = moveDirection;
                tempMovementState = movementState;
                if (inputDirection.sqrMagnitude > 0)
                    NavPaths = null;
            }
        }

        public virtual void PointClickMovement(Vector3 position)
        {
            if (!Entity.CanMove())
                return;
            if (this.CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                SetMovePaths(position, true);
            }
        }

        public void SetExtraMovementState(ExtraMovementState extraMovementState)
        {
            if (!Entity.CanMove())
                return;
            if (this.CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                tempExtraMovementState = extraMovementState;
            }
        }

        public virtual void SetLookRotation(Quaternion rotation)
        {
            if (this.CanPredictMovement())
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

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (!IsServer)
            {
                Logging.LogWarning("RigidBodyEntityMovement2D", "Teleport function shouldn't be called at client [" + name + "]");
                return;
            }
            isTeleporting = true;
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
            if (IsOwnerClient || (IsServer && Entity.MovementSecure == MovementSecure.ServerAuthoritative))
            {
                tempMovementState = moveDirection.sqrMagnitude > 0f ? tempMovementState : MovementState.None;
                tempMovementState |= MovementState.IsGrounded;
                // Update movement state
                MovementState = tempMovementState;
                // Update extra movement state
                ExtraMovementState = this.ValidateExtraMovementState(MovementState, tempExtraMovementState);
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
            float tempCurrentMoveSpeed;
            float tempMaxMoveSpeed;
            Vector2 tempMoveVelocity;
            Vector2 tempCurrentPosition;
            Vector2 tempTargetPosition;
            Vector2 tempPredictPosition;

            tempCurrentPosition = CacheTransform.position;
            tempMoveVelocity = Vector3.zero;
            moveDirection = Vector2.zero;
            tempTargetDistance = 0f;

            if (HasNavPaths)
            {
                // Set `tempTargetPosition` and `tempCurrentPosition`
                tempTargetPosition = NavPaths.Peek();
                moveDirection = (tempTargetPosition - tempCurrentPosition).normalized;
                tempTargetDistance = Vector2.Distance(tempTargetPosition, tempCurrentPosition);
                if (!tempMovementState.Has(MovementState.Forward))
                    tempMovementState |= MovementState.Forward;
                if (tempTargetDistance < StoppingDistance)
                {
                    NavPaths.Dequeue();
                    if (!HasNavPaths)
                    {
                        StopMoveFunction();
                        moveDirection = Vector2.zero;
                    }
                }
                else
                {
                    // Turn character to destination
                    Direction2D = moveDirection;
                }
            }
            else if (clientTargetPosition.HasValue)
            {
                tempTargetPosition = clientTargetPosition.Value;
                moveDirection = (tempTargetPosition - tempCurrentPosition).normalized;
                tempTargetDistance = Vector2.Distance(tempTargetPosition, tempCurrentPosition);
                if (tempTargetDistance < 0.001f)
                {
                    clientTargetPosition = null;
                    StopMoveFunction();
                    moveDirection = Vector2.zero;
                }
            }
            else if (inputDirection.sqrMagnitude > 0f)
            {
                moveDirection = inputDirection.normalized;
                tempTargetPosition = tempCurrentPosition + moveDirection;
            }
            else
            {
                tempTargetPosition = tempCurrentPosition;
            }

            if (!Entity.CanMove())
            {
                moveDirection = Vector2.zero;
            }

            // Prepare movement speed
            tempEntityMoveSpeed = Entity.GetMoveSpeed();
            tempMaxMoveSpeed = tempEntityMoveSpeed;

            // Updating horizontal movement (WASD inputs)
            if (moveDirection.sqrMagnitude > 0f)
            {
                // If character move backward
                tempCurrentMoveSpeed = CalculateCurrentMoveSpeed(tempMaxMoveSpeed, deltaTime);

                // NOTE: `tempTargetPosition` and `tempCurrentPosition` were set above
                tempSqrMagnitude = (tempTargetPosition - tempCurrentPosition).sqrMagnitude;
                tempPredictPosition = tempCurrentPosition + (moveDirection * tempCurrentMoveSpeed * deltaTime);
                tempPredictSqrMagnitude = (tempPredictPosition - tempCurrentPosition).sqrMagnitude;
                if (HasNavPaths || clientTargetPosition.HasValue)
                {
                    // Check `tempSqrMagnitude` against the `tempPredictSqrMagnitude`
                    // if `tempPredictSqrMagnitude` is greater than `tempSqrMagnitude`,
                    // rigidbody will reaching target and character is moving pass it,
                    // so adjust move speed by distance and time (with physic formula: v=s/t)
                    if (tempPredictSqrMagnitude >= tempSqrMagnitude)
                        tempCurrentMoveSpeed *= tempTargetDistance / deltaTime / tempCurrentMoveSpeed;
                }
                tempMoveVelocity = moveDirection * tempCurrentMoveSpeed;
                // Set inputs
                currentInput = this.SetInputMovementState2D(currentInput, tempMovementState);
                if (HasNavPaths)
                {
                    currentInput = this.SetInputPosition(currentInput, tempTargetPosition);
                    currentInput = this.SetInputIsKeyMovement(currentInput, false);
                }
                else
                {
                    currentInput = this.SetInputPosition(currentInput, tempPredictPosition);
                    currentInput = this.SetInputIsKeyMovement(currentInput, true);
                }
            }
            currentInput = this.SetInputDirection2D(currentInput, Direction2D);
            CacheRigidbody2D.velocity = tempMoveVelocity;
        }

        private float CalculateCurrentMoveSpeed(float maxMoveSpeed, float deltaTime)
        {
            // Adjust speed by rtt
            if (!IsServer && IsOwnerClient && Entity.MovementSecure == MovementSecure.ServerAuthoritative)
            {
                float rtt = 0.001f * Entity.Manager.Rtt;
                float acc = 1f / rtt * deltaTime * 0.5f;
                if (!lagMoveSpeedRate.HasValue)
                    lagMoveSpeedRate = 0f;
                if (lagMoveSpeedRate < 1f)
                    lagMoveSpeedRate += acc;
                if (lagMoveSpeedRate > 1f)
                    lagMoveSpeedRate = 1f;
                return maxMoveSpeed * lagMoveSpeedRate.Value;
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
            if (Entity.MovementSecure == MovementSecure.NotSecure && IsOwnerClient && !IsServer)
            {
                // Sync transform from owner client to server (except it's both owner client and server)
                this.ClientWriteSyncTransform2D(writer);
                return true;
            }
            if (Entity.MovementSecure == MovementSecure.ServerAuthoritative && IsOwnerClient && !IsServer)
            {
                EntityMovementInputState inputState;
                if (this.DifferInputEnoughToSend(oldInput, currentInput, out inputState))
                {
                    currentInput = this.SetInputExtraMovementState(currentInput, tempExtraMovementState);
                    this.ClientWriteMovementInput2D(writer, inputState, currentInput.MovementState, currentInput.ExtraMovementState, currentInput.Position, currentInput.Direction2D);
                    oldInput = currentInput;
                    currentInput = null;
                    return true;
                }
            }
            return false;
        }

        public bool WriteServerState(NetDataWriter writer, out bool shouldSendReliably)
        {
            shouldSendReliably = false;
            if (isTeleporting)
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
            isTeleporting = false;
            return true;
        }

        public void ReadClientStateAtServer(NetDataReader reader)
        {
            switch (Entity.MovementSecure)
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
            MovementState movementState;
            ExtraMovementState extraMovementState;
            Vector2 position;
            DirectionVector2 direction2D;
            long timestamp;
            reader.ReadSyncTransformMessage2D(out movementState, out extraMovementState, out position, out direction2D, out timestamp);
            if (Mathf.Abs(timestamp - BaseGameNetworkManager.Singleton.ServerTimestamp) > System.TimeSpan.TicksPerMillisecond)
            {
                // Timestamp is a lot difference to server's timestamp, player might try to hack a game or packet may corrupted occurring, so skip it
                return;
            }
            if (acceptedPositionTimestamp < timestamp)
            {
                // Snap character to the position if character is too far from the position
                if (movementState.Has(MovementState.IsTeleport))
                {
                    OnTeleport(position);
                }
                else if (Vector2.Distance(position, CacheTransform.position) >= snapThreshold)
                {
                    if (Entity.MovementSecure == MovementSecure.ServerAuthoritative || !IsOwnerClient)
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
                    clientTargetPosition = position;
                    MovementState = movementState;
                    ExtraMovementState = extraMovementState;
                }
                acceptedPositionTimestamp = timestamp;
            }
        }

        public void ReadMovementInputAtServer(NetDataReader reader)
        {
            if (IsOwnerClient)
            {
                // Don't read and apply inputs, because it was done (this is both owner client and server)
                return;
            }
            if (Entity.MovementSecure == MovementSecure.NotSecure)
            {
                // Movement handling at client, so don't read movement inputs from client (but have to read transform)
                return;
            }
            if (!Entity.CanMove())
                return;
            EntityMovementInputState inputState;
            MovementState movementState;
            ExtraMovementState extraMovementState;
            Vector2 position;
            DirectionVector2 direction2D;
            long timestamp;
            reader.ReadMovementInputMessage2D(out inputState, out movementState, out extraMovementState, out position, out direction2D, out timestamp);
            if (Mathf.Abs(timestamp - BaseGameNetworkManager.Singleton.ServerTimestamp) > System.TimeSpan.TicksPerMillisecond)
            {
                // Timestamp is a lot difference to server's timestamp, player might try to hack a game or packet may corrupted occurring, so skip it
                return;
            }
            if (acceptedPositionTimestamp < timestamp)
            {
                if (!inputState.Has(EntityMovementInputState.IsStopped))
                {
                    NavPaths = null;
                    tempMovementState = movementState;
                    tempExtraMovementState = extraMovementState;
                    if (inputState.Has(EntityMovementInputState.PositionChanged))
                    {
                        if (inputState.Has(EntityMovementInputState.IsKeyMovement))
                        {
                            clientTargetPosition = position;
                        }
                        else
                        {
                            SetMovePaths(position, true);
                        }
                    }
                    Direction2D = direction2D;
                }
                else
                {
                    StopMoveFunction();
                }
                acceptedPositionTimestamp = timestamp;
            }
        }

        public void ReadSyncTransformAtServer(NetDataReader reader)
        {
            if (IsOwnerClient)
            {
                // Don't read and apply transform, because it was done (this is both owner client and server)
                return;
            }
            if (Entity.MovementSecure == MovementSecure.ServerAuthoritative)
            {
                // Movement handling at server, so don't read sync transform from client
                return;
            }
            MovementState movementState;
            ExtraMovementState extraMovementState;
            Vector2 position;
            DirectionVector2 direction2D;
            long timestamp;
            reader.ReadSyncTransformMessage2D(out movementState, out extraMovementState, out position, out direction2D, out timestamp);
            if (Mathf.Abs(timestamp - BaseGameNetworkManager.Singleton.ServerTimestamp) > System.TimeSpan.TicksPerMillisecond)
            {
                // Timestamp is a lot difference to server's timestamp, player might try to hack a game or packet may corrupted occurring, so skip it
                return;
            }
            if (acceptedPositionTimestamp < timestamp)
            {
                Direction2D = direction2D;
                MovementState = movementState;
                ExtraMovementState = extraMovementState;
                if (!IsClient)
                {
                    // If it's server only (not a host), set position follows the client immediately
                    float currentTime = Time.unscaledTime;
                    float t = currentTime - lastServerValidateTransformTime;
                    float v = Entity.GetMoveSpeed();
                    float s = (lastServerValidateTransformMoveSpeed * t) + (v * (t + 0.2f)); // +200ms as high ping buffer
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
                    lastServerValidateTransformTime = currentTime;
                    lastServerValidateTransformMoveSpeed = v;
                }
                else
                {
                    // It's both server and client, translate position (it's a host so don't do speed hack validation)
                    if (Vector3.Distance(position, CacheTransform.position) > 0.01f)
                        SetMovePaths(position, false);
                }
                acceptedPositionTimestamp = timestamp;
            }
        }

        protected virtual void OnTeleport(Vector2 position)
        {
            clientTargetPosition = null;
            NavPaths = null;
            CacheTransform.position = position;
        }
    }
}
