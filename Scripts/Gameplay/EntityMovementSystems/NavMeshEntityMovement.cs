using LiteNetLibManager;
using UnityEngine;
using UnityEngine.AI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(LiteNetLibTransform))]
    public class NavMeshEntityMovement : BaseEntityMovement
    {
        [Header("Movement Settings")]
        [Tooltip("If calculated paths +1 higher than this value, it will stop moving. If this is 0 it will not applies")]
        public byte maxPathsForKeyMovement = 1;

        [Header("Interpolate, Extrapolate Settings")]
        public LiteNetLibTransform.InterpolateMode interpolateMode = LiteNetLibTransform.InterpolateMode.FixedSpeed;
        public LiteNetLibTransform.ExtrapolateMode extrapolateMode = LiteNetLibTransform.ExtrapolateMode.None;
        [Range(0.01f, 1f)]
        public float extrapolateSpeedRate = 0.5f;

        public LiteNetLibTransform CacheNetTransform { get; private set; }
        public NavMeshAgent CacheNavMeshAgent { get; private set; }

        public override float StoppingDistance
        {
            get { return CacheNavMeshAgent.stoppingDistance; }
        }

        public override void EntityAwake()
        {
            base.EntityAwake();
            // Prepare network transform component
            CacheNetTransform = gameObject.GetOrAddComponent<LiteNetLibTransform>();
            // Prepare nav mesh agent component
            CacheNavMeshAgent = gameObject.GetOrAddComponent<NavMeshAgent>();
        }

        public override void EntityLateUpdate()
        {
            base.EntityLateUpdate();
            // Setup network components
            switch (CacheEntity.MovementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    CacheNetTransform.ownerClientCanSendTransform = false;
                    break;
                case MovementSecure.NotSecure:
                    CacheNetTransform.ownerClientCanSendTransform = true;
                    break;
            }
        }

        public override void ComponentOnEnable()
        {
            CacheNetTransform.enabled = true;
            CacheNavMeshAgent.enabled = true;
        }

        public override void ComponentOnDisable()
        {
            CacheNetTransform.enabled = false;
            CacheNavMeshAgent.enabled = false;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            // Register Network functions
            RegisterNetFunction<Vector3, bool>(NetFuncPointClickMovement);
            RegisterNetFunction<short>(NetFuncUpdateYRotation);
            RegisterNetFunction(StopMove);
        }

        protected void NetFuncPointClickMovement(Vector3 position, bool useKeyMovement)
        {
            if (!CacheEntity.CanMove())
                return;
            SetMovePaths(position, useKeyMovement);
        }

        protected void NetFuncUpdateYRotation(short yRotation)
        {
            if (!CacheEntity.CanMove())
                return;
            CacheTransform.eulerAngles = new Vector3(0, yRotation, 0);
        }

        public override void KeyMovement(Vector3 moveDirection, MovementState movementState)
        {
            if (!CacheEntity.CanMove())
                return;

            if (moveDirection.sqrMagnitude <= 0f)
                return;

            Vector3 position = CacheTransform.position + moveDirection;
            switch (CacheEntity.MovementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    CallNetFunction(NetFuncPointClickMovement, FunctionReceivers.Server, position, true);
                    break;
                case MovementSecure.NotSecure:
                    SetMovePaths(position, true);
                    break;
            }
        }

        public override void PointClickMovement(Vector3 position)
        {
            if (!CacheEntity.CanMove())
                return;

            switch (CacheEntity.MovementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    CallNetFunction(NetFuncPointClickMovement, FunctionReceivers.Server, position, false);
                    break;
                case MovementSecure.NotSecure:
                    SetMovePaths(position, false);
                    break;
            }
        }

        public override void StopMove()
        {
            CacheNavMeshAgent.updatePosition = false;
            CacheNavMeshAgent.updateRotation = false;
            CacheNavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            if (CacheNavMeshAgent.isOnNavMesh)
                CacheNavMeshAgent.isStopped = true;
        }

        public override void SetLookRotation(Quaternion rotation)
        {
            if (!CacheEntity.CanMove())
                return;

            Vector3 eulerAngles = rotation.eulerAngles;
            switch (CacheEntity.MovementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    // Cast to short to reduce packet size
                    CallNetFunction(NetFuncUpdateYRotation, FunctionReceivers.Server, (short)eulerAngles.y);
                    break;
                case MovementSecure.NotSecure:
                    eulerAngles.x = 0;
                    eulerAngles.z = 0;
                    CacheTransform.eulerAngles = eulerAngles;
                    break;
            }
        }

        public override Quaternion GetLookRotation()
        {
            return CacheTransform.rotation;
        }

        public override void Teleport(Vector3 position, Quaternion rotation)
        {
            CacheNetTransform.Teleport(position, rotation);
        }

        public override bool FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
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
            base.EntityUpdate();
            float moveSpeed = CacheEntity.GetMoveSpeed();
            CacheNetTransform.interpolateMode = interpolateMode;
            if (interpolateMode == LiteNetLibTransform.InterpolateMode.FixedSpeed)
                CacheNetTransform.fixedInterpolateSpeed = moveSpeed;
            CacheNetTransform.extrapolateMode = extrapolateMode;
            if (extrapolateMode == LiteNetLibTransform.ExtrapolateMode.FixedSpeed)
                CacheNetTransform.fixedExtrapolateSpeed = moveSpeed * extrapolateSpeedRate;
        }

        public override void EntityFixedUpdate()
        {
            if (CacheEntity.MovementSecure == MovementSecure.ServerAuthoritative && !IsServer)
                return;

            if (CacheEntity.MovementSecure == MovementSecure.NotSecure && !IsOwnerClient)
                return;

            CacheEntity.SetMovement((CacheNavMeshAgent.velocity.sqrMagnitude > 0 ? MovementState.Forward : MovementState.None) | MovementState.IsGrounded);
        }

        protected void SetMovePaths(Vector3 position, bool useKeyMovement)
        {
            SetMovePaths(position, CacheEntity.GetMoveSpeed(), useKeyMovement);
        }

        protected void SetMovePaths(Vector3 position, float moveSpeed, bool useKeyMovement)
        {
            if (!CacheEntity.CanMove())
                return;
            CacheNavMeshAgent.updatePosition = true;
            CacheNavMeshAgent.updateRotation = true;
            CacheNavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
            CacheNavMeshAgent.speed = moveSpeed;
            if (CacheNavMeshAgent.isOnNavMesh)
            {
                CacheNavMeshAgent.isStopped = false;
                CacheNavMeshAgent.SetDestination(position);
                if (useKeyMovement && maxPathsForKeyMovement > 0 && CacheNavMeshAgent.path.corners.Length > maxPathsForKeyMovement + 1)
                    CacheNavMeshAgent.isStopped = true;
            }
        }
    }
}
