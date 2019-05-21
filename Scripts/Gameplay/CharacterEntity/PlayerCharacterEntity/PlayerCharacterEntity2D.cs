using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using LiteNetLibManager;
using LiteNetLib;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerCharacterEntity2D : BasePlayerCharacterEntity
    {
        #region Settings
        [Header("Movement AI")]
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;
        [Header("Network Settings")]
        public MovementSecure movementSecure;
        #endregion

        #region Sync data
        [SerializeField]
        protected SyncFieldVector2 currentDirection = new SyncFieldVector2();
        [SerializeField]
        protected SyncFieldByte currentDirectionType = new SyncFieldByte();
        #endregion

        #region Temp data
        protected Vector2 tempDirection;
        protected Vector2? currentDestination;
        protected Vector2 localDirection;
        #endregion

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

        private Rigidbody2D cacheRigidbody2D;
        public Rigidbody2D CacheRigidbody2D
        {
            get
            {
                if (cacheRigidbody2D == null)
                    cacheRigidbody2D = GetComponent<Rigidbody2D>();
                return cacheRigidbody2D;
            }
        }

        public Vector2 CurrentDirection
        {
            get
            {
                if (IsOwnerClient && movementSecure == MovementSecure.NotSecure)
                    return localDirection;
                return currentDirection.Value;
            }
        }

        protected DirectionType2D localDirectionType = DirectionType2D.Down;
        public DirectionType2D CurrentDirectionType
        {
            get
            {
                if (IsOwnerClient && movementSecure == MovementSecure.NotSecure)
                    return localDirectionType;
                return (DirectionType2D)currentDirectionType.Value;
            }
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

        protected override void EntityAwake()
        {
            base.EntityAwake();
            CacheRigidbody2D.gravityScale = 0;
            StopMove();
        }

        protected override void EntityUpdate()
        {
            base.EntityUpdate();
            Profiler.BeginSample("PlayerCharacterEntity2D - Update");
            if (IsDead())
            {
                StopMove();
                SetTargetEntity(null);
                return;
            }
            if (CharacterModel is ICharacterModel2D)
            {
                // Set current direction to character model 2D
                (CharacterModel as ICharacterModel2D).CurrentDirectionType = CurrentDirectionType;
            }
            Profiler.EndSample();
        }

        protected override void EntityFixedUpdate()
        {
            base.EntityFixedUpdate();
            Profiler.BeginSample("PlayerCharacterEntity2D - FixedUpdate");

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
                    CacheRigidbody2D.velocity = tempMoveDirection * CacheMoveSpeed;
                }
                else
                {
                    // Stop movement
                    CacheRigidbody2D.velocity = new Vector2(0, 0);
                }

                BaseGameEntity tempEntity;
                if (tempMoveDirectionMagnitude == 0f && TryGetTargetEntity(out tempEntity))
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
            Profiler.EndSample();
        }

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            currentDirectionType.deliveryMethod = DeliveryMethod.Sequenced;
            currentDirectionType.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
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
        }

        public override void OnSetup()
        {
            base.OnSetup();
            // Register Network functions
            RegisterNetFunction<Vector3>(NetFuncPointClickMovement);
            RegisterNetFunction<sbyte, sbyte>(NetFuncKeyMovement);
            RegisterNetFunction(StopMove);
            RegisterNetFunction<PackedUInt>(NetFuncSetTargetEntity);
            RegisterNetFunction<byte>(NetFuncSetMovementState);
            RegisterNetFunction<sbyte, sbyte>(NetFuncUpdateDirection);
        }

        protected void NetFuncPointClickMovement(Vector3 position)
        {
            if (IsDead())
                return;
            currentDestination = position;
            currentNpcDialog = null;
        }

        protected void NetFuncKeyMovement(sbyte horizontalInput, sbyte verticalInput)
        {
            if (IsDead())
                return;
            // Devide inputs to float value
            tempInputDirection = new Vector2((float)horizontalInput / 100f, (float)verticalInput / 100f);
            if (tempInputDirection.magnitude != 0)
                currentNpcDialog = null;
        }

        protected void NetFuncSetTargetEntity(PackedUInt objectId)
        {
            if (objectId == 0)
                SetTargetEntity(null);
            BaseGameEntity tempEntity;
            if (!TryGetEntityByObjectId(objectId, out tempEntity))
                return;
            SetTargetEntity(tempEntity);
        }

        protected void NetFuncSetMovementState(byte movementState)
        {
            if (!IsServer)
                return;

            MovementState = (MovementState)movementState;
        }

        protected void NetFuncUpdateDirection(sbyte x, sbyte y)
        {
            currentDirection.Value = new Vector2((float)x / 100f, (float)y / 100f);
            currentDirectionType.Value = (byte)GameplayUtils.GetDirectionTypeByVector2(currentDirection.Value);
        }

        public override void PointClickMovement(Vector3 position)
        {
            if (IsDead())
                return;

            switch (movementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    CallNetFunction(NetFuncPointClickMovement, FunctionReceivers.Server, position);
                    break;
                case MovementSecure.NotSecure:
                    currentDestination = position;
                    break;
            }
        }

        public override void KeyMovement(Vector3 direction, MovementState movementState)
        {
            if (IsDead())
                return;

            switch (movementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    // Multiply with 100 and cast to sbyte to reduce packet size
                    // then it will be devided with 100 later on server side
                    CallNetFunction(NetFuncKeyMovement, FunctionReceivers.Server, (sbyte)(direction.x * 100), (sbyte)(direction.y * 100));
                    break;
                case MovementSecure.NotSecure:
                    tempInputDirection = direction;
                    break;
            }
        }

        public override void UpdateYRotation(float yRotation)
        {
            // Do nothing, 2d characters will not rotates
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
                CallNetFunction(NetFuncSetMovementState, DeliveryMethod.Sequenced, FunctionReceivers.Server, (byte)state);
        }

        public override void StopMove()
        {
            currentDestination = null;
            tempMoveDirection = Vector3.zero;
            CacheRigidbody2D.velocity = Vector2.zero;
            if (IsOwnerClient && !IsServer)
                CallNetFunction(StopMove, FunctionReceivers.Server);
        }

        public override void SetTargetEntity(BaseGameEntity entity)
        {
            base.SetTargetEntity(entity);
            if (IsOwnerClient && !IsServer)
                CallNetFunction(NetFuncSetTargetEntity, FunctionReceivers.Server, new PackedUInt(entity == null ? 0 : entity.ObjectId));
        }

        public override bool IsPositionInFov(float fov, Vector3 position, Vector3 forward)
        {
            float halfFov = fov * 0.5f;
            Vector2 targetDir = (position - CacheTransform.position).normalized;
            float angle = Vector2.Angle(targetDir, CurrentDirection);
            // Angle in forward position is 180 so we use this value to determine that target is in hit fov or not
            return angle < halfFov;
        }

        protected override void GetDamagePositionAndRotation(DamageType damageType, bool isLeftHand, bool hasAimPosition, Vector3 aimPosition, Vector3 stagger, out Vector3 position, out Quaternion rotation)
        {
            position = CacheTransform.position;
            if (CharacterModel != null)
            {
                switch (damageType)
                {
                    case DamageType.Melee:
                        position = MeleeDamageTransform.position;
                        break;
                    case DamageType.Missile:
                        position = MissileDamageTransform.position;
                        break;
                }
            }
            rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(CurrentDirection.y, CurrentDirection.x) * (180 / Mathf.PI)) + 90);
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
                currentDirection.Value = localDirection;
                currentDirectionType.Value = (byte)localDirectionType;
            }
            if (IsOwnerClient && movementSecure == MovementSecure.NotSecure)
                CallNetFunction(NetFuncUpdateDirection, FunctionReceivers.Server, (sbyte)(localDirection.x * 100f), (sbyte)(localDirection.y * 100f));
        }
    }
}
