using LiteNetLib.Utils;
using LiteNetLibManager;
using UnityEngine;
using UnityEngine.AI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshEntityMovement : BaseNetworkedGameEntityComponent<BaseGameEntity>, IEntityMovementComponent
    {
        protected static readonly long lagBuffer = System.TimeSpan.TicksPerMillisecond * 200;
        protected static readonly float lagBufferUnityTime = 0.2f;

        [Header("Movement Settings")]
        public ObstacleAvoidanceType obstacleAvoidanceWhileMoving = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        public ObstacleAvoidanceType obstacleAvoidanceWhileStationary = ObstacleAvoidanceType.NoObstacleAvoidance;

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

        protected float lastServerValidateTransformTime;
        protected float lastServerValidateTransformMoveSpeed;
        protected long acceptedPositionTimestamp;
        protected float yAngle;
        protected float? targetYAngle;
        protected float yTurnSpeed;
        protected EntityMovementInput oldInput;
        protected EntityMovementInput currentInput;
        protected ExtraMovementState tempExtraMovementState;
        protected Vector3? inputDirection;
        protected bool moveByDestination = false;
        protected bool isTeleporting;

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
            targetYAngle = null;
            yAngle = CacheTransform.eulerAngles.y;
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
            {
                inputDirection = null;
                return;
            }
            if (this.CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                inputDirection = moveDirection;
                moveByDestination = false;
                CacheNavMeshAgent.updatePosition = true;
                CacheNavMeshAgent.updateRotation = true;
                if (CacheNavMeshAgent.isOnNavMesh)
                    CacheNavMeshAgent.isStopped = true;
            }
        }

        public void PointClickMovement(Vector3 position)
        {
            if (!Entity.CanMove())
                return;
            if (this.CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                SetMovePaths(position);
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
                this.SetInputStop(currentInput);
            }
            StopMoveFunction();
        }

        private void StopMoveFunction()
        {
            inputDirection = null;
            moveByDestination = false;
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
                targetYAngle = rotation.eulerAngles.y;
            }
        }

        public Quaternion GetLookRotation()
        {
            return Quaternion.Euler(0f, CacheTransform.eulerAngles.y, 0f);
        }

        public void SetSmoothTurnSpeed(float turnDuration)
        {
            yTurnSpeed = turnDuration;
        }

        public float GetSmoothTurnSpeed()
        {
            return yTurnSpeed;
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (!IsServer)
            {
                Logging.LogWarning("NavMeshEntityMovement", "Teleport function shouldn't be called at client [" + name + "]");
                return;
            }
            isTeleporting = true;
            OnTeleport(position, rotation.eulerAngles.y);
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
            CacheNavMeshAgent.speed = Entity.GetMoveSpeed();
            float deltaTime = Time.deltaTime;
            bool isStationary = CacheNavMeshAgent.isStopped || CacheNavMeshAgent.remainingDistance <= CacheNavMeshAgent.stoppingDistance;
            CacheNavMeshAgent.obstacleAvoidanceType = isStationary ? obstacleAvoidanceWhileStationary : obstacleAvoidanceWhileMoving;
            if (IsOwnerClient || (IsServer && Entity.MovementSecure == MovementSecure.ServerAuthoritative))
            {
                if (inputDirection.HasValue)
                {
                    CacheNavMeshAgent.Move(inputDirection.Value * CacheNavMeshAgent.speed * deltaTime);
                    CacheNavMeshAgent.SetDestination(CacheTransform.position);
                    MovementState = MovementState.Forward | MovementState.IsGrounded;
                    // Turn character to destination
                    if (yTurnSpeed <= 0f)
                    {
                        targetYAngle = null;
                        yAngle = Quaternion.LookRotation(inputDirection.Value).eulerAngles.y;
                    }
                    else
                    {
                        targetYAngle = Quaternion.LookRotation(inputDirection.Value).eulerAngles.y;
                    }
                }
                else
                {
                    // Update movement state
                    MovementState = (CacheNavMeshAgent.velocity.sqrMagnitude > 0 ? MovementState.Forward : MovementState.None) | MovementState.IsGrounded;
                    // Use nav mesh agent to turn character
                    targetYAngle = null;
                }
                // Update extra movement state
                ExtraMovementState = this.ValidateExtraMovementState(MovementState, tempExtraMovementState);
                // Set current input
                currentInput = this.SetInputMovementState(currentInput, MovementState);
                currentInput = this.SetInputExtraMovementState(currentInput, ExtraMovementState);
                if (inputDirection.HasValue)
                {
                    currentInput = this.SetInputIsKeyMovement(currentInput, true);
                    currentInput = this.SetInputPosition(currentInput, CacheTransform.position);
                }
                else if (moveByDestination)
                {
                    currentInput = this.SetInputIsKeyMovement(currentInput, false);
                    currentInput = this.SetInputPosition(currentInput, CacheNavMeshAgent.destination);
                }
            }
            else
            {
                // Update movement state
                MovementState = (CacheNavMeshAgent.velocity.sqrMagnitude > 0 ? MovementState.Forward : MovementState.None) | MovementState.IsGrounded;
            }
            // Update rotating
            if (targetYAngle.HasValue)
            {
                CacheNavMeshAgent.updateRotation = false;
                yAngle = Mathf.LerpAngle(yAngle, targetYAngle.Value, yTurnSpeed * deltaTime);
                if (Mathf.Abs(yAngle - targetYAngle.Value) < 1f)
                {
                    CacheNavMeshAgent.updateRotation = true;
                    targetYAngle = null;
                }
            }
            if (!CacheNavMeshAgent.updateRotation)
                CacheTransform.eulerAngles = new Vector3(0f, yAngle, 0f);
            currentInput = this.SetInputRotation(currentInput, CacheTransform.rotation);
        }

        private void SetMovePaths(Vector3 position)
        {
            if (!Entity.CanMove())
                return;
            inputDirection = null;
            moveByDestination = true;
            CacheNavMeshAgent.updatePosition = true;
            CacheNavMeshAgent.updateRotation = true;
            if (CacheNavMeshAgent.isOnNavMesh)
            {
                CacheNavMeshAgent.isStopped = false;
                CacheNavMeshAgent.SetDestination(position);
            }
        }

        public bool WriteClientState(NetDataWriter writer, out bool shouldSendReliably)
        {
            shouldSendReliably = false;
            if (Entity.MovementSecure == MovementSecure.NotSecure && IsOwnerClient && !IsServer)
            {
                // Sync transform from owner client to server (except it's both owner client and server)
                this.ClientWriteSyncTransform3D(writer);
                return true;
            }
            if (Entity.MovementSecure == MovementSecure.ServerAuthoritative && IsOwnerClient && !IsServer)
            {
                EntityMovementInputState inputState;
                if (this.DifferInputEnoughToSend(oldInput, currentInput, out inputState))
                {
                    if (!currentInput.IsKeyMovement)
                    {
                        // Point click should be reliably
                        shouldSendReliably = true;
                    }
                    this.ClientWriteMovementInput3D(writer, inputState, currentInput.MovementState, currentInput.ExtraMovementState, currentInput.Position, currentInput.Rotation);
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
            this.ServerWriteSyncTransform3D(writer);
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
            Vector3 position;
            float yAngle;
            long timestamp;
            reader.ReadSyncTransformMessage3D(out movementState, out extraMovementState, out position, out yAngle, out timestamp);
            if (Mathf.Abs(timestamp - BaseGameNetworkManager.Singleton.ServerTimestamp) > lagBuffer)
            {
                // Timestamp is a lot difference to server's timestamp, player might try to hack a game or packet may corrupted occurring, so skip it
                return;
            }
            if (acceptedPositionTimestamp < timestamp)
            {
                // Snap character to the position if character is too far from the position
                if (movementState.Has(MovementState.IsTeleport))
                {
                    OnTeleport(position, yAngle);
                }
                else if (Vector3.Distance(position, CacheTransform.position) >= snapThreshold)
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
                    targetYAngle = yAngle;
                    yTurnSpeed = 1f / Time.fixedDeltaTime;
                    SetMovePaths(position);
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
            Vector3 position;
            float yAngle;
            long timestamp;
            reader.ReadMovementInputMessage3D(out inputState, out movementState, out extraMovementState, out position, out yAngle, out timestamp);
            if (Mathf.Abs(timestamp - BaseGameNetworkManager.Singleton.ServerTimestamp) > lagBuffer)
            {
                // Timestamp is a lot difference to server's timestamp, player might try to hack a game or packet may corrupted occurring, so skip it
                return;
            }
            if (acceptedPositionTimestamp < timestamp)
            {
                if (!inputState.Has(EntityMovementInputState.IsStopped))
                {
                    tempExtraMovementState = extraMovementState;
                    if (inputState.Has(EntityMovementInputState.PositionChanged))
                    {
                        SetMovePaths(position);
                    }
                    if (inputState.Has(EntityMovementInputState.RotationChanged))
                    {
                        if (IsClient)
                        {
                            targetYAngle = yAngle;
                            yTurnSpeed = 1f / Time.fixedDeltaTime;
                        }
                        else
                        {
                            CacheTransform.eulerAngles = new Vector3(0, yAngle, 0);
                            targetYAngle = null;
                        }
                    }
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
            Vector3 position;
            float yAngle;
            long timestamp;
            reader.ReadSyncTransformMessage3D(out movementState, out extraMovementState, out position, out yAngle, out timestamp);
            if (Mathf.Abs(timestamp - BaseGameNetworkManager.Singleton.ServerTimestamp) > lagBuffer)
            {
                // Timestamp is a lot difference to server's timestamp, player might try to hack a game or packet may corrupted occurring, so skip it
                return;
            }
            if (acceptedPositionTimestamp < timestamp)
            {
                CacheTransform.eulerAngles = new Vector3(0, yAngle, 0);
                MovementState = movementState;
                ExtraMovementState = extraMovementState;
                if (Vector3.Distance(position.GetXZ(), CacheTransform.position.GetXZ()) > 0.01f)
                {
                    if (!IsClient)
                    {
                        // If it's server only (not a host), set position follows the client immediately
                        float currentTime = Time.unscaledTime;
                        float t = currentTime - lastServerValidateTransformTime;
                        float v = Entity.GetMoveSpeed();
                        float s = (lastServerValidateTransformMoveSpeed * (t + lagBufferUnityTime)) + (v * t); // +`lagBufferUnityTime` as high ping buffer
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
                        lastServerValidateTransformTime = currentTime;
                        lastServerValidateTransformMoveSpeed = v;
                    }
                    else
                    {
                        // It's both server and client, translate position (it's a host so don't do speed hack validation)
                        SetMovePaths(position);
                    }
                }
                acceptedPositionTimestamp = timestamp;
            }
        }

        protected virtual void OnTeleport(Vector3 position, float yAngle)
        {
            inputDirection = null;
            moveByDestination = false;
            CacheNavMeshAgent.Warp(position);
            if (CacheNavMeshAgent.isOnNavMesh)
                CacheNavMeshAgent.isStopped = true;
            this.yAngle = yAngle;
            targetYAngle = null;
        }
    }
}
