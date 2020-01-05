using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(LiteNetLibTransform))]
    public class NavMeshEntityMovement : BaseEntityMovement
    {
        public LiteNetLibTransform CacheNetTransform { get; private set; }
        public NavMeshAgent CacheNavMeshAgent { get; private set; }

        public sealed override bool IsGrounded
        {
            get { return true; }
            protected set { }
        }

        public sealed override bool IsJumping
        {
            get { return false; }
            protected set { }
        }

        public override float StoppingDistance
        {
            get { return CacheNavMeshAgent.stoppingDistance; }
        }

        public override void EntityAwake()
        {
            base.EntityAwake();
            // Prepare network transform component
            CacheNetTransform = GetComponent<LiteNetLibTransform>();
            if (CacheNetTransform == null)
                CacheNetTransform = gameObject.AddComponent<LiteNetLibTransform>();
            // Prepare nav mesh agent component
            CacheNavMeshAgent = GetComponent<NavMeshAgent>();
            if (CacheNavMeshAgent == null)
                CacheNavMeshAgent = gameObject.AddComponent<NavMeshAgent>();
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

        public override void EntityOnSetup()
        {
            // Setup network components
            switch (CacheEntity.MovementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    CacheNetTransform.ownerClientCanSendTransform = false;
                    CacheNetTransform.ownerClientNotInterpolate = false;
                    break;
                case MovementSecure.NotSecure:
                    CacheNetTransform.ownerClientCanSendTransform = true;
                    CacheNetTransform.ownerClientNotInterpolate = true;
                    break;
            }
            // Register Network functions
            CacheEntity.RegisterNetFunction<Vector3>(NetFuncPointClickMovement);
            CacheEntity.RegisterNetFunction<short>(NetFuncUpdateYRotation);
            CacheEntity.RegisterNetFunction(StopMove);
        }

        protected void NetFuncPointClickMovement(Vector3 position)
        {
            if (!CacheEntity.CanMove())
                return;
            SetMovePaths(position);
        }

        protected void NetFuncUpdateYRotation(short yRotation)
        {
            if (!CacheEntity.CanMove())
                return;
            CacheTransform.eulerAngles = new Vector3(0, (float)yRotation, 0);
        }

        public override void KeyMovement(Vector3 moveDirection, MovementState movementState)
        {
            if (moveDirection.magnitude > 0.5f)
                PointClickMovement(CacheTransform.position + moveDirection);
        }

        public override void PointClickMovement(Vector3 position)
        {
            if (!CacheEntity.CanMove())
                return;

            switch (CacheEntity.MovementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    CacheEntity.CallNetFunction(NetFuncPointClickMovement, FunctionReceivers.Server, position);
                    break;
                case MovementSecure.NotSecure:
                    SetMovePaths(position);
                    break;
            }
        }

        public override void StopMove()
        {
            CacheNavMeshAgent.updatePosition = false;
            CacheNavMeshAgent.updateRotation = false;
            CacheNavMeshAgent.isStopped = true;
            CacheNavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
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
                    CacheEntity.CallNetFunction(NetFuncUpdateYRotation, FunctionReceivers.Server, (short)eulerAngles.y);
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

        public override void Teleport(Vector3 position)
        {
            CacheNetTransform.Teleport(position, Quaternion.Euler(0, CacheEntity.MovementTransform.eulerAngles.y, 0));
        }

        public override void FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
        {
            result = fromPosition;
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(fromPosition, out navHit, findDistance, -1))
                result = navHit.position;
        }

        public override void EntityFixedUpdate()
        {
            if (CacheEntity.MovementSecure == MovementSecure.ServerAuthoritative && !IsServer)
                return;

            if (CacheEntity.MovementSecure == MovementSecure.NotSecure && !IsOwnerClient)
                return;

            CacheEntity.SetMovement((CacheNavMeshAgent.velocity.magnitude > 0 ? MovementState.Forward : MovementState.None) | MovementState.IsGrounded);
        }

        protected void SetMovePaths(Vector3 position)
        {
            SetMovePaths(position, CacheEntity.GetMoveSpeed());
        }

        protected void SetMovePaths(Vector3 position, float moveSpeed)
        {
            if (!CacheEntity.CanMove())
                return;
            CacheNavMeshAgent.updatePosition = true;
            CacheNavMeshAgent.updateRotation = true;
            CacheNavMeshAgent.isStopped = false;
            CacheNavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
            CacheNavMeshAgent.speed = moveSpeed;
            CacheNavMeshAgent.SetDestination(position);
        }
    }
}
