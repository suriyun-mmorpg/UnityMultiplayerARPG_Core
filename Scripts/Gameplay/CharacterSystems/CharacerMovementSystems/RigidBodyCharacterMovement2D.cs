using LiteNetLib;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(LiteNetLibTransform))]
    public class RigidBodyCharacterMovement2D : BaseCharacterMovement2D
    {
        #region Settings
        [Header("Movement AI")]
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;
        [Header("Network Settings")]
        public MovementSecure movementSecure;
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

        #region Temp data
        protected Vector2 tempDirection;
        protected Vector2? currentDestination;
        #endregion

        protected Vector2 localDirection;
        public override Vector2 CurrentDirection
        {
            get
            {
                if (IsOwnerClient && movementSecure == MovementSecure.NotSecure)
                    return localDirection;
                return base.CurrentDirection;
            }
            set { base.CurrentDirection = value; }
        }

        protected DirectionType2D localDirectionType = DirectionType2D.Down;
        public override DirectionType2D CurrentDirectionType
        {
            get
            {
                if (IsOwnerClient && movementSecure == MovementSecure.NotSecure)
                    return localDirectionType;
                return base.CurrentDirectionType;
            }
            set { base.CurrentDirectionType = value; }
        }

        protected MovementState localMovementState = MovementState.None;
        public override MovementState MovementState
        {
            get
            {
                if (IsOwnerClient && movementSecure == MovementSecure.NotSecure)
                    return localMovementState;
                return base.MovementState;
            }
            set { base.MovementState = value; }
        }

        private float tempMoveDirectionMagnitude;
        private Vector2 tempInputDirection;
        private Vector2 tempMoveDirection;
        private Vector2 tempCurrentPosition;
        private Vector2 tempTargetDirection;

        protected virtual void Awake()
        {
            CacheRigidbody2D.gravityScale = 0;
            StopMove();
        }

        public override void EntityOnSetup(BaseCharacterEntity entity)
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
            entity.RegisterNetFunction<sbyte, sbyte>(NetFuncKeyMovement);
            entity.RegisterNetFunction(StopMove);
            entity.RegisterNetFunction<byte>(NetFuncSetMovementState);
            entity.RegisterNetFunction<sbyte, sbyte>(NetFuncUpdateDirection);
        }

        protected void NetFuncPointClickMovement(Vector3 position)
        {
            if (IsDead())
                return;
            currentDestination = position;
        }

        protected void NetFuncKeyMovement(sbyte horizontalInput, sbyte verticalInput)
        {
            if (IsDead())
                return;
            // Devide inputs to float value
            tempInputDirection = new Vector2((float)horizontalInput / 100f, (float)verticalInput / 100f);
        }

        protected void NetFuncSetMovementState(byte movementState)
        {
            if (!IsServer)
                return;

            MovementState = (MovementState)movementState;
        }

        protected void NetFuncUpdateDirection(sbyte x, sbyte y)
        {
            CurrentDirection = new Vector2((float)x / 100f, (float)y / 100f);
            CurrentDirectionType = GameplayUtils.GetDirectionTypeByVector2(CurrentDirection);
        }

        public override void StopMove()
        {
            currentDestination = null;
            tempMoveDirection = Vector3.zero;
            CacheRigidbody2D.velocity = Vector2.zero;
            if (IsOwnerClient && !IsServer)
                CacheCharacterEntity.CallNetFunction(StopMove, FunctionReceivers.Server);
        }

        public override void KeyMovement(Vector3 moveDirection, MovementState movementState)
        {
            if (IsDead())
                return;

            switch (movementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    // Multiply with 100 and cast to sbyte to reduce packet size
                    // then it will be devided with 100 later on server side
                    CacheCharacterEntity.CallNetFunction(NetFuncKeyMovement, FunctionReceivers.Server, (sbyte)(moveDirection.x * 100), (sbyte)(moveDirection.y * 100));
                    break;
                case MovementSecure.NotSecure:
                    tempInputDirection = moveDirection;
                    break;
            }
        }

        public override void PointClickMovement(Vector3 position)
        {
            if (IsDead())
                return;

            switch (movementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    CacheCharacterEntity.CallNetFunction(NetFuncPointClickMovement, FunctionReceivers.Server, position);
                    break;
                case MovementSecure.NotSecure:
                    currentDestination = position;
                    break;
            }
        }

        public override void SetLookRotation(Vector3 eulerAngles)
        {
            // Do nothing, 2d characters will not rotates
            UpdateCurrentDirection(Quaternion.Euler(eulerAngles) * Vector3.forward);
        }

        public override void Teleport(Vector3 position)
        {
            CacheNetTransform.Teleport(position, Quaternion.identity);
        }

        public override void FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
        {
            result = fromPosition;
        }

        protected virtual void FixedUpdate()
        {
            if (movementSecure == MovementSecure.ServerAuthoritative && !IsServer)
                return;

            if (movementSecure == MovementSecure.NotSecure && !IsOwnerClient)
                return;

            tempMoveDirection = Vector2.zero;

            if (currentDestination.HasValue)
            {
                tempCurrentPosition = new Vector2(CacheTransform.position.x, CacheTransform.position.y);
                tempMoveDirection = (currentDestination.Value - tempCurrentPosition).normalized;
                if (Vector2.Distance(currentDestination.Value, tempCurrentPosition) < StoppingDistance)
                    StopMove();
            }

            if (!IsDead())
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
                    CacheRigidbody2D.velocity = tempMoveDirection * gameplayRule.GetMoveSpeed(CacheCharacterEntity);
                }
                else
                {
                    // Stop movement
                    CacheRigidbody2D.velocity = new Vector2(0, 0);
                }

                BaseGameEntity tempEntity;
                if (tempMoveDirectionMagnitude == 0f && CacheCharacterEntity.TryGetTargetEntity(out tempEntity))
                {
                    tempTargetDirection = (tempEntity.CacheTransform.position - CacheTransform.position).normalized;
                    if (tempTargetDirection.magnitude != 0f)
                        UpdateCurrentDirection(tempTargetDirection);
                }
            }

            if (tempMoveDirection.Equals(Vector3.zero))
            {
                // No movement so state is none
                SetMovementState(MovementState.None);
            }
            else
            {
                // For 2d, just define that it is moving so can use any state
                SetMovementState(MovementState.Forward);
            }
        }

        public void SetMovementState(MovementState state)
        {
            if (IsGrounded)
                state |= MovementState.IsGrounded;

            // Set local movement state which will be used by owner client
            localMovementState = state;

            if (movementSecure == MovementSecure.ServerAuthoritative && IsServer)
                MovementState = state;

            if (movementSecure == MovementSecure.NotSecure && IsOwnerClient)
                CacheCharacterEntity.CallNetFunction(NetFuncSetMovementState, DeliveryMethod.Sequenced, FunctionReceivers.Server, (byte)state);
        }

        public void UpdateCurrentDirection(Vector2 direction)
        {
            if (direction.magnitude > 0f)
            {
                localDirection = direction;
                localDirectionType = GameplayUtils.GetDirectionTypeByVector2(direction);
            }
            if (IsServer && movementSecure == MovementSecure.ServerAuthoritative)
            {
                CurrentDirection = localDirection;
                CurrentDirectionType = localDirectionType;
            }
            if (IsOwnerClient && movementSecure == MovementSecure.NotSecure)
                CacheCharacterEntity.CallNetFunction(NetFuncUpdateDirection, FunctionReceivers.Server, (sbyte)(localDirection.x * 100f), (sbyte)(localDirection.y * 100f));
        }
    }
}
