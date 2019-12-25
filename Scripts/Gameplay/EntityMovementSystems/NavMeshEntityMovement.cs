using LiteNetLib;
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
        [Header("Network Settings")]
        public MovementSecure movementSecure;

        private LiteNetLibTransform cacheNetTransform;
        public LiteNetLibTransform CacheNetTransform
        {
            get
            {
                if (cacheNetTransform == null)
                    cacheNetTransform = GetComponent<LiteNetLibTransform>();
                if (cacheNetTransform == null)
                    cacheNetTransform = gameObject.AddComponent<LiteNetLibTransform>();
                return cacheNetTransform;
            }
        }

        private NavMeshAgent cacheNavMeshAgent;
        public NavMeshAgent CacheNavMeshAgent
        {
            get
            {
                if (cacheNavMeshAgent == null)
                    cacheNavMeshAgent = GetComponent<NavMeshAgent>();
                if (cacheNavMeshAgent == null)
                    cacheNavMeshAgent = gameObject.AddComponent<NavMeshAgent>();
                return cacheNavMeshAgent;
            }
        }

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
            if (CacheEntity is BaseMonsterCharacterEntity)
            {
                // Monster always server authoritative
                movementSecure = MovementSecure.ServerAuthoritative;
            }
            // Setup network components
            switch (movementSecure)
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
            CacheEntity.RegisterNetFunction<byte>(NetFuncSetMovement);
            CacheEntity.RegisterNetFunction<byte>(NetFuncSetExtraMovement);
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

        protected void NetFuncSetMovement(byte movementState)
        {
            // Set data at server and sync to clients later
            CacheEntity.MovementState = (MovementState)movementState;
        }

        protected void NetFuncSetExtraMovement(byte extraMovementState)
        {
            // Set data at server and sync to clients later
            CacheEntity.ExtraMovementState = (ExtraMovementState)extraMovementState;
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

            switch (movementSecure)
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

        public override void SetExtraMovement(ExtraMovementState extraMovementState)
        {
            // Set local movement state which will be used by owner client
            CacheEntity.LocalExtraMovementState = extraMovementState;

            if (movementSecure == MovementSecure.ServerAuthoritative && IsServer)
                CacheEntity.ExtraMovementState = extraMovementState;

            if (movementSecure == MovementSecure.NotSecure && IsOwnerClient)
                CacheEntity.CallNetFunction(NetFuncSetExtraMovement, DeliveryMethod.Sequenced, FunctionReceivers.Server, (byte)extraMovementState);
        }

        public override void SetLookRotation(Vector3 eulerAngles)
        {
            if (!CacheEntity.CanMove())
                return;

            switch (movementSecure)
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
            if (movementSecure == MovementSecure.ServerAuthoritative && !IsServer)
                return;

            if (movementSecure == MovementSecure.NotSecure && !IsOwnerClient)
                return;

            SetMovementState(CacheNavMeshAgent.velocity.magnitude > 0 ? MovementState.Forward : MovementState.None);
        }

        public void SetMovementState(MovementState state)
        {
            if (IsGrounded)
                state |= MovementState.IsGrounded;

            // Set local movement state which will be used by owner client
            CacheEntity.LocalMovementState = state;

            if (movementSecure == MovementSecure.ServerAuthoritative && IsServer)
                CacheEntity.MovementState = state;

            if (movementSecure == MovementSecure.NotSecure && IsOwnerClient)
                CacheEntity.CallNetFunction(NetFuncSetMovement, DeliveryMethod.Sequenced, FunctionReceivers.Server, (byte)state);
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
