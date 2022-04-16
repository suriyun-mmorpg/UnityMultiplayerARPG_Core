using LiteNetLib.Utils;
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
        public float snapThreshold = 5.0f;

        public NavMeshAgent CacheNavMeshAgent { get; private set; }
        public float StoppingDistance
        {
            get { return CacheNavMeshAgent.stoppingDistance; }
        }
        public MovementState MovementState { get; protected set; }
        public ExtraMovementState ExtraMovementState { get; protected set; }
        public DirectionVector2 Direction2D { get { return Vector2.down; } set { } }

        protected float lastServerValidateTransformTime;
        protected float lastServerValidateTransformMoveSpeed;
        protected long acceptedPositionTimestamp;
        protected float? targetYRotation;
        protected float yRotateLerpTime;
        protected float yRotateLerpDuration;
        protected EntityMovementInput oldInput;
        protected EntityMovementInput currentInput;
        protected ExtraMovementState tempExtraMovementState;
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
                this.SetInputStop(currentInput);
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
            bool isStationary = CacheNavMeshAgent.isStopped || CacheNavMeshAgent.remainingDistance <= CacheNavMeshAgent.stoppingDistance;
            CacheNavMeshAgent.obstacleAvoidanceType = isStationary ? obstacleAvoidanceWhileStationary : obstacleAvoidanceWhileMoving;
            if (IsOwnerClient || (IsServer && Entity.MovementSecure == MovementSecure.ServerAuthoritative))
            {
                // Update movement state
                MovementState = (CacheNavMeshAgent.velocity.sqrMagnitude > 0 ? MovementState.Forward : MovementState.None) | MovementState.IsGrounded;
                // Update extra movement state
                ExtraMovementState = this.ValidateExtraMovementState(MovementState, tempExtraMovementState);
            }
            else
            {
                // Update movement state
                MovementState = (CacheNavMeshAgent.velocity.sqrMagnitude > 0 ? MovementState.Forward : MovementState.None) | MovementState.IsGrounded;
            }
        }

        private void SetMovePaths(Vector3 position, bool useKeyMovement)
        {
            if (!Entity.CanMove())
                return;
            CacheNavMeshAgent.updatePosition = true;
            CacheNavMeshAgent.updateRotation = true;
            if (CacheNavMeshAgent.isOnNavMesh)
            {
                CacheNavMeshAgent.isStopped = false;
                CacheNavMeshAgent.SetDestination(position);
                if (useKeyMovement && maxPathsForKeyMovement > 0 && CacheNavMeshAgent.path.corners.Length > maxPathsForKeyMovement + 1)
                    CacheNavMeshAgent.isStopped = true;
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
                    currentInput = this.SetInputExtraMovementState(currentInput, tempExtraMovementState);
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
                    targetYRotation = yAngle;
                    yRotateLerpTime = 0;
                    yRotateLerpDuration = Time.fixedDeltaTime;
                    SetMovePaths(position, false);
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
            if (acceptedPositionTimestamp < timestamp)
            {
                if (!inputState.Has(EntityMovementInputState.IsStopped))
                {
                    tempExtraMovementState = extraMovementState;
                    if (inputState.Has(EntityMovementInputState.PositionChanged))
                    {
                        SetMovePaths(position, inputState.Has(EntityMovementInputState.IsKeyMovement));
                    }
                    if (inputState.Has(EntityMovementInputState.RotationChanged))
                    {
                        if (IsClient)
                        {
                            targetYRotation = yAngle;
                            yRotateLerpTime = 0;
                            yRotateLerpDuration = Time.fixedDeltaTime;
                        }
                        else
                        {
                            CacheTransform.eulerAngles = new Vector3(0, yAngle, 0);
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
                        float s = (lastServerValidateTransformMoveSpeed * t) + (v * (t + 0.2f)); // +200ms as high ping buffer
                        if (s < 0.001f)
                            s = 0.001f;
                        Vector3 oldPos = CacheTransform.position.GetXZ();
                        Vector3 newPos = position.GetXZ();
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
                            newPos.y = position.y;
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
                        SetMovePaths(position, false);
                    }
                }
                acceptedPositionTimestamp = timestamp;
            }
        }

        protected virtual void OnTeleport(Vector3 position, float yAngle)
        {
            CacheNavMeshAgent.isStopped = true;
            CacheNavMeshAgent.Warp(position);
            CacheTransform.rotation = Quaternion.Euler(0f, yAngle, 0f);
        }
    }
}
