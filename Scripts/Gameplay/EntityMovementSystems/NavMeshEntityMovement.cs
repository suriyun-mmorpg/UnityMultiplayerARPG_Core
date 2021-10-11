using LiteNetLibManager;
using UnityEngine;
using UnityEngine.AI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshEntityMovement : BaseNetworkedGameEntityComponent<BaseGameEntity>, IEntityMovementComponent
    {
        [Header("Movement Settings")]
        [Tooltip("If calculated paths +1 higher than this value, it will stop moving. If this is 0 it will not applies")]
        public byte maxPathsForKeyMovement = 1;
        public ObstacleAvoidanceType obstacleAvoidanceWhileMoving = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        public ObstacleAvoidanceType obstacleAvoidanceWhileStationary = ObstacleAvoidanceType.NoObstacleAvoidance;

        [Header("Networking Settings")]
        public float moveThreshold = 0.01f;
        public float snapThreshold = 5.0f;
        [Range(0.00825f, 0.1f)]
        public float clientSyncTransformInterval = 0.05f;
        [Range(0.00825f, 0.1f)]
        public float clientSendInputsInterval = 0.05f;
        [Range(0.00825f, 0.1f)]
        public float serverSyncTransformInterval = 0.05f;

        public NavMeshAgent CacheNavMeshAgent { get; private set; }
        public float StoppingDistance
        {
            get { return CacheNavMeshAgent.stoppingDistance; }
        }
        public MovementState MovementState { get; protected set; }
        public ExtraMovementState ExtraMovementState { get; protected set; }

        protected long acceptedPositionTimestamp;
        protected float lastServerSyncTransform;
        protected float lastClientSyncTransform;
        protected float lastClientSendInputs;
        protected float? targetYRotation;
        protected float yRotateLerpTime;
        protected float yRotateLerpDuration;
        protected EntityMovementInput oldInput;
        protected EntityMovementInput currentInput;
        protected ExtraMovementState tempExtraMovementState;

        public override void EntityAwake()
        {
            base.EntityAwake();
            // Prepare nav mesh agent component
            CacheNavMeshAgent = gameObject.GetOrAddComponent<NavMeshAgent>();
            // Disable unused component
            LiteNetLibTransform disablingComp = gameObject.GetComponent<LiteNetLibTransform>();
            if (disablingComp != null)
            {
                Logging.LogWarning("NavMeshEntityMovement", "You can remove `LiteNetLibTransform` component from game entity, it's not being used anymore [" + name + "]");
                disablingComp.enabled = false;
            }
            // Setup
            StopMoveFunction();
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
                return;
            if (this.CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                SetMovePaths(CacheTransform.position + moveDirection, true);
                currentInput = this.SetInputPosition(currentInput, CacheTransform.position + moveDirection);
                currentInput = this.SetInputIsKeyMovement(currentInput, true);
            }
        }

        public void PointClickMovement(Vector3 position)
        {
            if (!Entity.CanMove())
                return;
            if (this.CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                SetMovePaths(position, false);
                currentInput = this.SetInputPosition(currentInput, position);
                currentInput = this.SetInputIsKeyMovement(currentInput, false);
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

        public void StopMove()
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
            CacheNavMeshAgent.updatePosition = false;
            CacheNavMeshAgent.updateRotation = false;
            if (CacheNavMeshAgent.isOnNavMesh)
                CacheNavMeshAgent.isStopped = true;
        }

        public void SetLookRotation(Quaternion rotation)
        {
            if (!Entity.CanMove())
                return;
            if (this.CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                CacheTransform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
                currentInput = this.SetInputRotation(currentInput, CacheTransform.rotation);
            }
        }

        public Quaternion GetLookRotation()
        {
            return Quaternion.Euler(0f, CacheTransform.eulerAngles.y, 0f);
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (!IsServer)
            {
                Logging.LogWarning("NavMeshEntityMovement", "Teleport function shouldn't be called at client [" + name + "]");
                return;
            }
            this.ServerSendTeleport3D(position, rotation);
            CacheTransform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
            CacheNavMeshAgent.Warp(position);
        }

        public bool FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
        {
            result = fromPosition;
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(fromPosition, out navHit, findDistance, NavMesh.AllAreas))
            {
                result = navHit.position;
                return true;
            }
            return false;
        }

        public override void EntityUpdate()
        {
            float deltaTime = Time.deltaTime;
            if (targetYRotation.HasValue)
            {
                CacheNavMeshAgent.updateRotation = false;
                yRotateLerpTime += deltaTime;
                float lerpTimeRate = yRotateLerpTime / yRotateLerpDuration;
                Vector3 eulerAngles = CacheTransform.eulerAngles;
                eulerAngles.y = Mathf.LerpAngle(eulerAngles.y, targetYRotation.Value, lerpTimeRate);
                CacheTransform.eulerAngles = eulerAngles;
                if (lerpTimeRate >= 1f)
                {
                    CacheNavMeshAgent.updateRotation = true;
                    targetYRotation = null;
                }
            }
        }

        public override void EntityFixedUpdate()
        {
            bool isStationary = CacheNavMeshAgent.isStopped || CacheNavMeshAgent.remainingDistance <= CacheNavMeshAgent.stoppingDistance;
            CacheNavMeshAgent.obstacleAvoidanceType = isStationary ? obstacleAvoidanceWhileStationary : obstacleAvoidanceWhileMoving;
            if (IsOwnerClient || (IsServer && Entity.MovementSecure == MovementSecure.ServerAuthoritative))
            {
                // Update movement state
                MovementState = (CacheNavMeshAgent.velocity.sqrMagnitude > 0 ? MovementState.Forward : MovementState.None) | MovementState.IsGrounded;
                // Update extra movement state
                ExtraMovementState = this.ValidateExtraMovementState(MovementState, tempExtraMovementState);
            }
            SyncTransform();
        }

        private void SyncTransform()
        {
            float currentTime = Time.fixedTime;
            if (Entity.MovementSecure == MovementSecure.NotSecure && IsOwnerClient && !IsServer)
            {
                // Sync transform from owner client to server (except it's both owner client and server)
                if (currentTime - lastClientSyncTransform > clientSyncTransformInterval)
                {
                    this.ClientSendSyncTransform3D();
                    lastClientSyncTransform = currentTime;
                }
            }
            if (Entity.MovementSecure == MovementSecure.ServerAuthoritative && IsOwnerClient && !IsServer)
            {
                InputState inputState;
                if (currentTime - lastClientSendInputs > clientSendInputsInterval && this.DifferInputEnoughToSend(oldInput, currentInput, out inputState))
                {
                    currentInput = this.SetInputExtraMovementState(currentInput, tempExtraMovementState);
                    this.ClientSendMovementInput3D(inputState, currentInput.MovementState, currentInput.ExtraMovementState, currentInput.Position, currentInput.Rotation);
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
                    this.ServerSendSyncTransform3D();
                    lastServerSyncTransform = currentTime;
                }
            }
        }

        private void SetMovePaths(Vector3 position, bool useKeyMovement)
        {
            SetMovePaths(position, Entity.GetMoveSpeed(), useKeyMovement);
        }

        private void SetMovePaths(Vector3 position, float moveSpeed, bool useKeyMovement)
        {
            if (!Entity.CanMove())
                return;
            CacheNavMeshAgent.updatePosition = true;
            CacheNavMeshAgent.updateRotation = true;
            CacheNavMeshAgent.speed = moveSpeed;
            if (CacheNavMeshAgent.isOnNavMesh)
            {
                CacheNavMeshAgent.isStopped = false;
                CacheNavMeshAgent.SetDestination(position);
                if (useKeyMovement && maxPathsForKeyMovement > 0 && CacheNavMeshAgent.path.corners.Length > maxPathsForKeyMovement + 1)
                    CacheNavMeshAgent.isStopped = true;
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
            Vector3 position;
            float yAngle;
            long timestamp;
            messageHandler.Reader.ReadSyncTransformMessage3D(out movementState, out extraMovementState, out position, out yAngle, out timestamp);
            if (acceptedPositionTimestamp < timestamp)
            {
                acceptedPositionTimestamp = timestamp;
                // Snap character to the position if character is too far from the position
                if (Vector3.Distance(position, CacheTransform.position) >= snapThreshold)
                {
                    if (Entity.MovementSecure == MovementSecure.ServerAuthoritative || !IsOwnerClient)
                    {
                        CacheTransform.eulerAngles = new Vector3(0, yAngle, 0);
                        CacheNavMeshAgent.Warp(position);
                    }
                    MovementState = movementState;
                    ExtraMovementState = extraMovementState;
                }
                else if (!IsOwnerClient)
                {
                    targetYRotation = yAngle;
                    yRotateLerpTime = 0;
                    yRotateLerpDuration = serverSyncTransformInterval;
                    if (Vector3.Distance(position.GetXZ(), CacheTransform.position.GetXZ()) > moveThreshold)
                    {
                        SetMovePaths(position, false);
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
            Vector3 position;
            float yAngle;
            long timestamp;
            messageHandler.Reader.ReadTeleportMessage3D(out position, out yAngle, out timestamp);
            if (acceptedPositionTimestamp < timestamp)
            {
                acceptedPositionTimestamp = timestamp;
                CacheTransform.eulerAngles = new Vector3(0, yAngle, 0);
                CacheNavMeshAgent.Warp(position);
            }
        }

        public void HandleJumpAtClient(MessageHandlerData messageHandler)
        {
            // There is no jump for navmesh
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
            InputState inputState;
            MovementState movementState;
            ExtraMovementState extraMovementState;
            Vector3 position;
            float yAngle;
            long timestamp;
            messageHandler.Reader.ReadMovementInputMessage3D(out inputState, out movementState, out extraMovementState, out position, out yAngle, out timestamp);
            if (acceptedPositionTimestamp < timestamp)
            {
                acceptedPositionTimestamp = timestamp;
                tempExtraMovementState = extraMovementState;
                if (inputState.HasFlag(InputState.PositionChanged))
                {
                    SetMovePaths(position, inputState.HasFlag(InputState.IsKeyMovement));
                }
                if (inputState.HasFlag(InputState.RotationChanged))
                {
                    if (IsClient)
                    {
                        targetYRotation = yAngle;
                        yRotateLerpTime = 0;
                        yRotateLerpDuration = clientSendInputsInterval;
                    }
                    else
                    {
                        CacheTransform.eulerAngles = new Vector3(0, yAngle, 0);
                    }
                }
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
            Vector3 position;
            float yAngle;
            long timestamp;
            messageHandler.Reader.ReadSyncTransformMessage3D(out movementState, out extraMovementState, out position, out yAngle, out timestamp);
            if (acceptedPositionTimestamp < timestamp)
            {
                acceptedPositionTimestamp = timestamp;
                CacheTransform.eulerAngles = new Vector3(0, yAngle, 0);
                if (Vector3.Distance(position.GetXZ(), CacheTransform.position.GetXZ()) > moveThreshold)
                {
                    if (!IsClient)
                    {
                        // If it's server only (not a host), set position follows the client immediately
                        CacheNavMeshAgent.Warp(position);
                    }
                    else
                    {
                        // It's both server and client, translate position
                        SetMovePaths(position, false);
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
            // There is no jump for navmesh
        }
    }
}
