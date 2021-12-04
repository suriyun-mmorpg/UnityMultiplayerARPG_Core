using LiteNetLibManager;
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
        public float moveThreshold = 0.01f;
        public float snapThreshold = 5.0f;
        [Range(0.00825f, 0.1f)]
        public float clientSyncTransformInterval = 0.05f;
        [Range(0.00825f, 0.1f)]
        public float clientSendInputsInterval = 0.05f;
        [Range(0.00825f, 0.1f)]
        public float serverSyncTransformInterval = 0.05f;

        public Rigidbody2D CacheRigidbody2D { get; private set; }

        public float StoppingDistance
        {
            get { return stoppingDistance; }
        }
        public MovementState MovementState { get; protected set; }
        public ExtraMovementState ExtraMovementState { get; protected set; }

        protected Vector2? currentDestination;
        protected Vector2 tempMoveDirection;
        protected Vector2 tempCurrentPosition;
        protected Vector2 tempPredictPosition;
        protected float tempSqrMagnitude;
        protected float tempPredictSqrMagnitude;
        protected float tempTargetDistance;
        protected float tempCurrentMoveSpeed;
        protected Quaternion lookRotation;
        protected long acceptedPositionTimestamp;
        protected float lastServerSyncTransform;
        protected float lastClientSyncTransform;
        protected float lastClientSendInputs;
        protected EntityMovementInput oldInput;
        protected EntityMovementInput currentInput;
        protected ExtraMovementState tempExtraMovementState;

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

        public virtual void StopMove()
        {
            if (Entity.MovementSecure == MovementSecure.ServerAuthoritative)
            {
                // Send movement input to server, then server will apply movement and sync transform to clients
                this.ClientSendStopMove();
            }
            StopMoveFunction();
        }

        private void StopMoveFunction()
        {
            currentDestination = null;
            CacheRigidbody2D.velocity = Vector2.zero;
        }

        public virtual void KeyMovement(Vector3 moveDirection, MovementState movementState)
        {
            if (moveDirection.sqrMagnitude <= 0.25f)
                return;
            PointClickMovement(CacheTransform.position + moveDirection);
        }

        public virtual void PointClickMovement(Vector3 position)
        {
            if (!Entity.CanMove())
                return;
            if (this.CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                currentDestination = position;
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
            lookRotation = rotation;
            Entity.SetDirection2D(lookRotation * Vector3.forward);
        }

        public Quaternion GetLookRotation()
        {
            return lookRotation;
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (!IsServer)
            {
                Logging.LogWarning("RigidBodyEntityMovement2D", "Teleport function shouldn't be called at client [" + name + "]");
                return;
            }
            this.ServerSendTeleport3D(position, rotation);
            OnTeleport(position);
        }

        public bool FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
        {
            result = fromPosition;
            return true;
        }

        public override void EntityFixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;
            tempMoveDirection = Vector2.zero;
            tempTargetDistance = 0f;
            if (currentDestination.HasValue)
            {
                currentInput = this.SetInputPosition(currentInput, currentDestination.Value);
                tempCurrentPosition = new Vector2(CacheTransform.position.x, CacheTransform.position.y);
                tempMoveDirection = (currentDestination.Value - tempCurrentPosition).normalized;
                tempTargetDistance = Vector2.Distance(currentDestination.Value, tempCurrentPosition);
                if (tempTargetDistance < StoppingDistance)
                {
                    StopMoveFunction();
                    tempMoveDirection = Vector2.zero;
                }
            }
            if (Entity.CanMove())
            {
                if (tempMoveDirection.sqrMagnitude > 0f)
                {
                    tempCurrentMoveSpeed = Entity.GetMoveSpeed();
                    tempSqrMagnitude = (currentDestination.Value - tempCurrentPosition).sqrMagnitude;
                    tempPredictPosition = tempCurrentPosition + (tempMoveDirection * tempCurrentMoveSpeed * deltaTime);
                    tempPredictSqrMagnitude = (tempPredictPosition - tempCurrentPosition).sqrMagnitude;
                    // Check `tempSqrMagnitude` against the `tempPredictSqrMagnitude`
                    // if `tempPredictSqrMagnitude` is greater than `tempSqrMagnitude`,
                    // rigidbody will reaching target and character is moving pass it,
                    // so adjust move speed by distance and time (with physic formula: v=s/t)
                    if (tempPredictSqrMagnitude >= tempSqrMagnitude)
                        tempCurrentMoveSpeed *= tempTargetDistance / deltaTime / tempCurrentMoveSpeed;
                    Entity.SetDirection2D(tempMoveDirection);
                    CacheRigidbody2D.velocity = tempMoveDirection * tempCurrentMoveSpeed;
                }
                else
                {
                    // Stop movement
                    CacheRigidbody2D.velocity = new Vector2(0, 0);
                }
            }
            if (IsOwnerClient || (IsServer && Entity.MovementSecure == MovementSecure.ServerAuthoritative))
            {
                // Update movement state
                MovementState = (CacheRigidbody2D.velocity.sqrMagnitude > 0 ? MovementState.Forward : MovementState.None) | MovementState.IsGrounded;
                // Update extra movement state
                ExtraMovementState = this.ValidateExtraMovementState(MovementState, tempExtraMovementState);
            }
            else
            {
                // Update movement state
                MovementState = (CacheRigidbody2D.velocity.sqrMagnitude > 0 ? MovementState.Forward : MovementState.None) | MovementState.IsGrounded;
            }
            SyncTransform();
        }

        protected void SyncTransform()
        {
            float currentTime = Time.fixedTime;
            if (Entity.MovementSecure == MovementSecure.NotSecure && IsOwnerClient && !IsServer)
            {
                // Sync transform from owner client to server (except it's both owner client and server)
                if (currentTime - lastClientSyncTransform > clientSyncTransformInterval)
                {
                    this.ClientSendSyncTransform2D();
                    lastClientSyncTransform = currentTime;
                }
            }
            if (Entity.MovementSecure == MovementSecure.ServerAuthoritative && IsOwnerClient && !IsServer)
            {
                InputState inputState;
                if (currentTime - lastClientSendInputs > clientSendInputsInterval && this.DifferInputEnoughToSend(oldInput, currentInput, out inputState))
                {
                    currentInput = this.SetInputMovementState(currentInput, MovementState);
                    currentInput = this.SetInputExtraMovementState(currentInput, tempExtraMovementState);
                    this.ClientSendMovementInput2D(currentInput.MovementState, currentInput.ExtraMovementState, currentInput.Position);
                    oldInput = currentInput;
                    currentInput = null;
                    lastClientSendInputs = currentTime;
                }
            }
            if (IsServer)
            {
                // Sync transform from server to all clients (include owner client)
                if (currentTime - lastServerSyncTransform > serverSyncTransformInterval)
                {
                    this.ServerSendSyncTransform2D();
                    lastServerSyncTransform = currentTime;
                }
            }
        }

        public void HandleSyncTransformAtClient(MessageHandlerData messageHandler)
        {
            if (IsServer)
            {
                // Don't read and apply transform, because it was done at server
                return;
            }
            MovementState movementState;
            ExtraMovementState extraMovementState;
            Vector2 position;
            long timestamp;
            messageHandler.Reader.ReadSyncTransformMessage2D(out movementState, out extraMovementState, out position, out timestamp);
            if (acceptedPositionTimestamp < timestamp)
            {
                acceptedPositionTimestamp = timestamp;
                // Snap character to the position if character is too far from the position
                if (Vector3.Distance(position, CacheTransform.position) >= snapThreshold)
                {
                    if (Entity.MovementSecure == MovementSecure.ServerAuthoritative || !IsOwnerClient)
                    {
                        CacheTransform.position = position;
                    }
                    MovementState = movementState;
                    ExtraMovementState = extraMovementState;
                }
                else if (!IsOwnerClient)
                {
                    if (Vector2.Distance(position, CacheTransform.position) > moveThreshold)
                    {
                        currentDestination = position;
                    }
                    MovementState = movementState;
                    ExtraMovementState = extraMovementState;
                }
            }
        }

        public void HandleTeleportAtClient(MessageHandlerData messageHandler)
        {
            if (IsServer)
            {
                // Don't read and apply transform, because it was done (this is both owner client and server)
                return;
            }
            Vector2 position;
            long timestamp;
            messageHandler.Reader.ReadTeleportMessage2D(out position, out timestamp);
            if (acceptedPositionTimestamp < timestamp)
            {
                acceptedPositionTimestamp = timestamp;
                OnTeleport(position);
            }
        }

        public void HandleJumpAtClient(MessageHandlerData messageHandler)
        {
            // There is no jump for 2D
        }

        public void HandleMovementInputAtServer(MessageHandlerData messageHandler)
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
            MovementState movementState;
            ExtraMovementState extraMovementState;
            Vector2 position;
            long timestamp;
            messageHandler.Reader.ReadMovementInputMessage2D(out movementState, out extraMovementState, out position, out timestamp);
            if (acceptedPositionTimestamp < timestamp)
            {
                acceptedPositionTimestamp = timestamp;
                tempExtraMovementState = extraMovementState;
                currentDestination = position;
            }
        }

        public void HandleSyncTransformAtServer(MessageHandlerData messageHandler)
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
            long timestamp;
            messageHandler.Reader.ReadSyncTransformMessage2D(out movementState, out extraMovementState, out position, out timestamp);
            if (acceptedPositionTimestamp < timestamp)
            {
                acceptedPositionTimestamp = timestamp;
                if (Vector2.Distance(position, CacheTransform.position) > moveThreshold)
                {
                    if (!IsClient)
                    {
                        // If it's server only (not a host), set position follows the client immediately
                        CacheTransform.position = position;
                    }
                    else
                    {
                        // It's both server and client, translate position
                        currentDestination = position;
                    }
                }
                MovementState = movementState;
                ExtraMovementState = extraMovementState;
            }
        }

        public void HandleStopMoveAtServer(MessageHandlerData messageHandler)
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
            long timestamp;
            messageHandler.Reader.ReadStopMoveMessage(out timestamp);
            if (acceptedPositionTimestamp < timestamp)
            {
                acceptedPositionTimestamp = timestamp;
                StopMoveFunction();
            }
        }

        public void HandleJumpAtServer(MessageHandlerData messageHandler)
        {
            // There is no jump for 2D
        }

        protected virtual void OnTeleport(Vector2 position)
        {
            currentDestination = null;
            CacheTransform.position = position;
        }
    }
}
