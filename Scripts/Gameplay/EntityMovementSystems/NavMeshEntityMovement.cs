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

        protected Vector3 latestDestination;
        
        protected MovementState localMovementState = MovementState.None;
        public MovementState MovementState
        {
            get
            {
                if (IsOwnerClient && movementSecure == MovementSecure.NotSecure)
                    return localMovementState;
                return CacheEntity.MovementState;
            }
            set { CacheEntity.MovementState = value; }
        }
        protected MovementState extraMovementState = MovementState.None;

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

        protected virtual void OnEnable()
        {
            CacheNetTransform.enabled = true;
            CacheNavMeshAgent.enabled = true;
        }

        protected virtual void OnDisable()
        {
            CacheNetTransform.enabled = false;
            CacheNavMeshAgent.enabled = false;
        }

        public override void EntityOnSetup(BaseGameEntity entity)
        {
            base.EntityOnSetup(entity);
            if (entity is BaseMonsterCharacterEntity)
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
            entity.RegisterNetFunction<Vector3>(NetFuncPointClickMovement);
            entity.RegisterNetFunction<short>(NetFuncUpdateYRotation);
            entity.RegisterNetFunction(StopMove);
            entity.RegisterNetFunction<byte>(NetFuncSetMovement);
            entity.RegisterNetFunction<byte>(NetFuncSetExtraMovement);
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
            MovementState = (MovementState)movementState;
        }

        protected void NetFuncSetExtraMovement(byte movementState)
        {
            extraMovementState = (MovementState)movementState;
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

        public override void SetExtraMovement(MovementState movementState)
        {
            switch (movementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    CacheEntity.CallNetFunction(NetFuncSetExtraMovement, FunctionReceivers.Server, (byte)movementState);
                    break;
                case MovementSecure.NotSecure:
                    extraMovementState = movementState;
                    break;
            }
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

        private void FixedUpdate()
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
            {
                if (state.HasFlag(MovementState.Forward) && extraMovementState.HasFlag(MovementState.IsSprinting))
                    state |= MovementState.IsSprinting;
                state |= MovementState.IsGrounded;
            }

            // Set local movement state which will be used by owner client
            localMovementState = state;

            if (movementSecure == MovementSecure.ServerAuthoritative && IsServer)
                MovementState = state;

            if (movementSecure == MovementSecure.NotSecure && IsOwnerClient)
                CacheEntity.CallNetFunction(NetFuncSetMovement, DeliveryMethod.Sequenced, FunctionReceivers.Server, (byte)state);
        }

        protected void SetMovePaths(Vector3 position)
        {
            SetMovePaths(position, CacheEntity.GetMoveSpeed());
        }
        
        protected void SetMovePaths(Vector3 position, float moveSpeed)
        {
            if (!CacheEntity.CanMove() || position.Equals(latestDestination))
                return;
            latestDestination = position;
            CacheNavMeshAgent.updatePosition = true;
            CacheNavMeshAgent.updateRotation = true;
            CacheNavMeshAgent.isStopped = false;
            CacheNavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
            CacheNavMeshAgent.speed = moveSpeed;
            CacheNavMeshAgent.SetDestination(position);
        }
    }
}
