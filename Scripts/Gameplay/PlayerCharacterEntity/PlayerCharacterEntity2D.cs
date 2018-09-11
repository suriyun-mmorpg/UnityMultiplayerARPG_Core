using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using LiteNetLibManager;
using LiteNetLib;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterModel2D))]
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
        protected SyncFieldByte currentDirectionType = new SyncFieldByte();
        #endregion

        #region Temp data
        protected Collider2D[] overlapColliders2D = new Collider2D[OVERLAP_COLLIDER_SIZE];
        protected Vector2 currentDirection = Vector2.down;
        protected Vector2 tempDirection;
        protected Vector2? currentDestination;
        protected DirectionType localDirectionType = DirectionType.Down;
        #endregion

        public Vector2 moveDirection { get; protected set; }
        private Vector2 lastMoveDirection;

        public override float StoppingDistance
        {
            get { return stoppingDistance; }
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

        public DirectionType CurrentDirectionType
        {
            get
            {
                if (IsOwnerClient && movementSecure == MovementSecure.NotSecure)
                    return localDirectionType;
                return (DirectionType)currentDirectionType.Value;
            }
        }

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
            (CharacterModel as CharacterModel2D).currentDirectionType = CurrentDirectionType;
            Profiler.EndSample();
        }

        protected override void EntityFixedUpdate()
        {
            base.EntityFixedUpdate();
            Profiler.BeginSample("PlayerCharacterEntity2D - FixedUpdate");
            if (!IsServer && !IsOwnerClient)
                return;

            if (currentDestination.HasValue)
            {
                var currentPosition = new Vector2(CacheTransform.position.x, CacheTransform.position.y);
                moveDirection = (currentDestination.Value - currentPosition).normalized;
                if (Vector3.Distance(currentDestination.Value, currentPosition) < StoppingDistance)
                    StopMove();
            }

            if (!IsDead())
            {
                var moveDirectionMagnitude = moveDirection.magnitude;
                if (!IsPlayingActionAnimation() && moveDirectionMagnitude != 0)
                {
                    if (moveDirectionMagnitude > 1)
                        moveDirection = moveDirection.normalized;
                    UpdateCurrentDirection(moveDirection);
                    CacheRigidbody2D.velocity = moveDirection * CacheMoveSpeed;
                }

                RpgNetworkEntity tempEntity;
                if (moveDirectionMagnitude == 0 && TryGetTargetEntity(out tempEntity))
                {
                    var targetDirection = (tempEntity.CacheTransform.position - CacheTransform.position).normalized;
                    if (targetDirection.magnitude != 0f)
                        UpdateCurrentDirection(targetDirection);
                }
            }
            Profiler.EndSample();
        }

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            currentDirectionType.sendOptions = SendOptions.Unreliable;
            currentDirectionType.forOwnerOnly = false;
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
            RegisterNetFunction("PointClickMovement", new LiteNetLibFunction<NetFieldVector3>((position) => NetFuncPointClickMovement(position)));
            RegisterNetFunction("KeyMovement", new LiteNetLibFunction<NetFieldSByte, NetFieldSByte>((horizontalInput, verticalInput) => NetFuncKeyMovement(horizontalInput, verticalInput)));
            RegisterNetFunction("StopMove", new LiteNetLibFunction(StopMove));
            RegisterNetFunction("SetTargetEntity", new LiteNetLibFunction<NetFieldPackedUInt>((objectId) => NetFuncSetTargetEntity(objectId)));
            RegisterNetFunction("SetCurrentDirectionType", new LiteNetLibFunction<NetFieldByte>((byteDirectionType) => NetFuncSetByteDirectionType(byteDirectionType)));
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
            var direction = new Vector3((float)horizontalInput / 100f, (float)verticalInput / 100f);
            if (direction.magnitude > 0)
                currentDestination = CacheTransform.position + direction;
            currentNpcDialog = null;
        }

        protected void NetFuncSetTargetEntity(uint objectId)
        {
            if (objectId == 0)
                SetTargetEntity(null);
            RpgNetworkEntity rpgNetworkEntity;
            if (!TryGetEntityByObjectId(objectId, out rpgNetworkEntity))
                return;
            SetTargetEntity(rpgNetworkEntity);
        }

        protected void NetFuncSetByteDirectionType(byte directionType)
        {
            currentDirectionType.Value = directionType;
        }

        public override void PointClickMovement(Vector3 position)
        {
            if (IsDead())
                return;
            switch (movementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    CallNetFunction("PointClickMovement", FunctionReceivers.Server, position);
                    break;
                case MovementSecure.NotSecure:
                    currentDestination = position;
                    break;
            }
        }

        public override void KeyMovement(Vector3 direction, bool isJump)
        {
            if (IsDead())
                return;
            switch (movementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    // Multiply with 100 and cast to sbyte to reduce packet size
                    // then it will be devided with 100 later on server side
                    CallNetFunction("KeyMovement", FunctionReceivers.Server, (sbyte)(direction.x * 100), (sbyte)(direction.y * 100));
                    break;
                case MovementSecure.NotSecure:
                    if (direction.magnitude > 0)
                        currentDestination = CacheTransform.position + direction;
                    break;
            }
        }

        public override void StopMove()
        {
            currentDestination = null;
            moveDirection = Vector3.zero;
            CacheRigidbody2D.velocity = Vector2.zero;
            if (IsOwnerClient && !IsServer)
                CallNetFunction("StopMove", FunctionReceivers.Server);
        }

        public override void SetTargetEntity(RpgNetworkEntity entity)
        {
            base.SetTargetEntity(entity);
            if (IsOwnerClient && !IsServer)
                CallNetFunction("SetTargetEntity", FunctionReceivers.Server, entity == null ? 0 : entity.ObjectId);
        }

        public override int OverlapObjects(Vector3 position, float distance, int layerMask)
        {
            return Physics2D.OverlapCircleNonAlloc(position, distance, overlapColliders2D, layerMask);
        }

        public override GameObject GetOverlapObject(int index)
        {
            return overlapColliders2D[index].gameObject;
        }

        public override bool IsPositionInFov(float fov, Vector3 position)
        {
            var halfFov = fov * 0.5f;
            var angle = Vector2.Angle((CacheTransform.position - position).normalized, currentDirection);
            // Angle in forward position is 180 so we use this value to determine that target is in hit fov or not
            return (angle < 180 + halfFov && angle > 180 - halfFov);
        }

        protected override void GetDamagePositionAndRotation(DamageType damageType, out Vector3 position, out Quaternion rotation)
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
            rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(currentDirection.y, currentDirection.x) * (180 / Mathf.PI)) + 90);
        }

        public void UpdateCurrentDirection(Vector2 direction)
        {
            currentDirection = direction;
            UpdateDirection(currentDirection);
        }

        public void UpdateDirection(Vector2 direction)
        {
            if (direction.magnitude > 0f)
            {
                var normalized = direction.normalized;
                if (Mathf.Abs(normalized.x) >= Mathf.Abs(normalized.y))
                {
                    if (normalized.x < 0) localDirectionType = DirectionType.Left;
                    if (normalized.x > 0) localDirectionType = DirectionType.Right;
                }
                else
                {
                    if (normalized.y < 0) localDirectionType = DirectionType.Down;
                    if (normalized.y > 0) localDirectionType = DirectionType.Up;
                }
            }
            if (IsServer && movementSecure == MovementSecure.ServerAuthoritative)
                currentDirectionType.Value = (byte)localDirectionType;
            if (IsOwnerClient && movementSecure == MovementSecure.NotSecure)
                CallNetFunction("SetCurrentDirectionType", FunctionReceivers.Server, (byte)localDirectionType);
        }
    }
}
