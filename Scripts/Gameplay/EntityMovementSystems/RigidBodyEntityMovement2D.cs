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

        private Rigidbody2D cacheRigidbody2D;
        public Rigidbody2D CacheRigidbody2D
        {
            get
            {
                if (cacheRigidbody2D == null)
                    cacheRigidbody2D = GetComponent<Rigidbody2D>();
                if (cacheRigidbody2D == null)
                    cacheRigidbody2D = gameObject.AddComponent<Rigidbody2D>();
                return cacheRigidbody2D;
            }
        }

        public override float StoppingDistance
        {
            get { return stoppingDistance; }
        }

        public override bool IsGrounded
        {
            get { return true; }
            protected set { }
        }

        public override bool IsJumping
        {
            get { return false; }
            protected set { }
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
            CacheRigidbody2D.gravityScale = 0;
            StopMove();
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
            CacheEntity.RegisterNetFunction<sbyte, sbyte>(NetFuncKeyMovement);
            CacheEntity.RegisterNetFunction(StopMove);
            CacheEntity.RegisterNetFunction<byte>(NetFuncSetMovement);
            CacheEntity.RegisterNetFunction<byte>(NetFuncSetExtraMovement);
            CacheEntity.RegisterNetFunction<DirectionVector2>(NetFuncUpdateDirection);
        }

        protected void NetFuncPointClickMovement(Vector3 position)
        {
            if (!CacheEntity.CanMove())
                return;
            currentDestination = position;
        }

        protected void NetFuncKeyMovement(sbyte horizontalInput, sbyte verticalInput)
        {
            if (!CacheEntity.CanMove())
                return;
            // Devide inputs to float value
            tempInputDirection = new Vector2((float)horizontalInput / 100f, (float)verticalInput / 100f);
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

        protected void NetFuncUpdateDirection(DirectionVector2 direction)
        {
            CacheEntity.CurrentDirection2D = direction;
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
                    CacheEntity.CallNetFunction(NetFuncKeyMovement, FunctionReceivers.Server, (sbyte)(moveDirection.x * 100), (sbyte)(moveDirection.y * 100));
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

        public override void SetExtraMovement(ExtraMovementState extraMovementState)
        {
            // Set local movement state which will be used by owner client
            CacheEntity.LocalExtraMovementState = extraMovementState;

            if (CacheEntity.MovementSecure == MovementSecure.ServerAuthoritative && IsServer)
                CacheEntity.ExtraMovementState = extraMovementState;

            if (CacheEntity.MovementSecure == MovementSecure.NotSecure && IsOwnerClient)
                CacheEntity.CallNetFunction(NetFuncSetExtraMovement, DeliveryMethod.Sequenced, FunctionReceivers.Server, (byte)extraMovementState);
        }

        public override void SetLookRotation(Quaternion rotation)
        {
            lookRotation = rotation;
            UpdateCurrentDirection(lookRotation * Vector3.forward);
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
                if (tempInputDirection.magnitude != 0f)
                    tempMoveDirection = tempInputDirection;

                tempMoveDirectionMagnitude = tempMoveDirection.magnitude;
                if (tempMoveDirectionMagnitude != 0f)
                {
                    if (tempMoveDirectionMagnitude > 1)
                        tempMoveDirection = tempMoveDirection.normalized;

                    UpdateCurrentDirection(tempMoveDirection);
                    CacheRigidbody2D.velocity = tempMoveDirection * CacheEntity.GetMoveSpeed();
                }
                else
                {
                    // Stop movement
                    CacheRigidbody2D.velocity = new Vector2(0, 0);
                }
            }

            SetMovementState(CacheRigidbody2D.velocity.magnitude > 0 ? MovementState.Forward : MovementState.None);
        }

        public void SetMovementState(MovementState state)
        {
            if (IsGrounded)
                state |= MovementState.IsGrounded;

            // Set local movement state which will be used by owner client
            CacheEntity.LocalMovementState = state;

            if (CacheEntity.MovementSecure == MovementSecure.ServerAuthoritative && IsServer)
                CacheEntity.MovementState = state;

            if (CacheEntity.MovementSecure == MovementSecure.NotSecure && IsOwnerClient)
                CacheEntity.CallNetFunction(NetFuncSetMovement, DeliveryMethod.Sequenced, FunctionReceivers.Server, (byte)state);
        }

        public void UpdateCurrentDirection(Vector2 direction)
        {
            if (direction.magnitude > 0f)
                CacheEntity.LocalDirection2D = direction;

            if (IsServer && CacheEntity.MovementSecure == MovementSecure.ServerAuthoritative)
                CacheEntity.CurrentDirection2D = CacheEntity.LocalDirection2D;

            if (IsOwnerClient && CacheEntity.MovementSecure == MovementSecure.NotSecure)
                CacheEntity.CallNetFunction(NetFuncUpdateDirection, FunctionReceivers.Server, new DirectionVector2(CacheEntity.LocalDirection2D));
        }
    }
}
