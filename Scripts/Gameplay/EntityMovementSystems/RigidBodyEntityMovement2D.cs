using LiteNetLib;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(LiteNetLibTransform))]
    public class RigidBodyEntityMovement2D : BaseEntityMovement
    {
        #region Settings
        [Header("Movement AI")]
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;
        #endregion
        
        public LiteNetLibTransform CacheNetTransform { get; private set; }
        public Rigidbody2D CacheRigidbody2D { get; private set; }

        public override float StoppingDistance
        {
            get { return stoppingDistance; }
        }

        protected Vector2? currentDestination;

        private float tempMoveDirectionMagnitude;
        private Vector2 tempInputDirection;
        private Vector2 tempMoveDirection;
        private Vector2 tempCurrentPosition;
        private Vector2 tempTargetDirection;
        private Quaternion lookRotation;

        public override void EntityAwake()
        {
            // Prepare network transform component
            CacheNetTransform = GetComponent<LiteNetLibTransform>();
            if (CacheNetTransform == null)
                CacheNetTransform = gameObject.AddComponent<LiteNetLibTransform>();
            // Prepare rigidbody component
            CacheRigidbody2D = GetComponent<Rigidbody2D>();
            if (CacheRigidbody2D == null)
                CacheRigidbody2D = gameObject.AddComponent<Rigidbody2D>();
            // Setup
            CacheRigidbody2D.gravityScale = 0;
            StopMove();
        }

        public override void EntityLateUpdate()
        {
            base.EntityLateUpdate();
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
        }

        public override void ComponentOnEnable()
        {
            CacheNetTransform.enabled = true;
            CacheRigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        public override void ComponentOnDisable()
        {
            CacheNetTransform.enabled = false;
            CacheRigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        public override void EntityOnSetup()
        {
            // Register Network functions
            CacheEntity.RegisterNetFunction<Vector3>(NetFuncPointClickMovement);
            CacheEntity.RegisterNetFunction<DirectionVector2>(NetFuncKeyMovement);
            CacheEntity.RegisterNetFunction(StopMove);
        }

        protected void NetFuncPointClickMovement(Vector3 position)
        {
            if (!CacheEntity.CanMove())
                return;
            currentDestination = position;
        }

        protected void NetFuncKeyMovement(DirectionVector2 direction)
        {
            if (!CacheEntity.CanMove())
                return;
            // Devide inputs to float value
            tempInputDirection = direction;
        }

        public override void StopMove()
        {
            currentDestination = null;
            CacheRigidbody2D.velocity = Vector2.zero;
            if (IsOwnerClient && !IsServer)
                CacheEntity.CallNetFunction(StopMove, FunctionReceivers.Server);
        }

        public override void KeyMovement(Vector3 moveDirection, MovementState movementState)
        {
            if (!CacheEntity.CanMove())
                return;

            switch (CacheEntity.MovementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    // Multiply with 100 and cast to sbyte to reduce packet size
                    // then it will be devided with 100 later on server side
                    CacheEntity.CallNetFunction(NetFuncKeyMovement, FunctionReceivers.Server, new DirectionVector2(moveDirection));
                    break;
                case MovementSecure.NotSecure:
                    tempInputDirection = moveDirection;
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
                    CacheEntity.CallNetFunction(NetFuncPointClickMovement, FunctionReceivers.Server, position);
                    break;
                case MovementSecure.NotSecure:
                    currentDestination = position;
                    break;
            }
        }

        public override void SetLookRotation(Quaternion rotation)
        {
            lookRotation = rotation;
            CacheEntity.SetDirection2D(lookRotation * Vector3.forward);
        }

        public override Quaternion GetLookRotation()
        {
            return lookRotation;
        }

        public override void Teleport(Vector3 position)
        {
            CacheNetTransform.Teleport(position, Quaternion.identity);
        }

        public override void FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
        {
            result = fromPosition;
        }

        public override void EntityFixedUpdate()
        {
            if (CacheEntity.MovementSecure == MovementSecure.ServerAuthoritative && !IsServer)
                return;

            if (CacheEntity.MovementSecure == MovementSecure.NotSecure && !IsOwnerClient)
                return;

            tempMoveDirection = Vector2.zero;

            if (currentDestination.HasValue)
            {
                tempCurrentPosition = new Vector2(CacheTransform.position.x, CacheTransform.position.y);
                tempMoveDirection = (currentDestination.Value - tempCurrentPosition).normalized;
                if (Vector2.Distance(currentDestination.Value, tempCurrentPosition) < StoppingDistance)
                    StopMove();
            }

            if (CacheEntity.CanMove())
            {
                // If move by WASD keys, set move direction to input direction
                if (tempInputDirection.magnitude > 0.5f)
                    tempMoveDirection = tempInputDirection;

                tempMoveDirectionMagnitude = tempMoveDirection.magnitude;
                if (tempMoveDirectionMagnitude > 0f)
                {
                    if (tempMoveDirectionMagnitude > 1)
                        tempMoveDirection = tempMoveDirection.normalized;

                    CacheEntity.SetDirection2D(tempMoveDirection);
                    CacheRigidbody2D.velocity = tempMoveDirection * CacheEntity.GetMoveSpeed();
                }
                else
                {
                    // Stop movement
                    CacheRigidbody2D.velocity = new Vector2(0, 0);
                }
            }

            CacheEntity.SetMovement((CacheRigidbody2D.velocity.sqrMagnitude > 0 ? MovementState.Forward : MovementState.None) | MovementState.IsGrounded);
        }
    }
}
