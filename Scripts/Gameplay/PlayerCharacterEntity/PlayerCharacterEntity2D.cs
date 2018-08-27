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
        #endregion

        #region Sync data
        [SerializeField]
        protected SyncFieldByte currentDirectionType = new SyncFieldByte();
        #endregion

        #region Temp data
        protected Collider2D[] overlapColliders2D = new Collider2D[OVERLAP_COLLIDER_SIZE];
        protected Vector2 currentDirection;
        protected Vector2 dirtyDirection;
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
                if (IsOwnerClient)
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

                BaseCharacterEntity tempCharacterEntity;
                if (moveDirectionMagnitude == 0 && TryGetTargetEntity(out tempCharacterEntity))
                {
                    var targetDirection = (tempCharacterEntity.CacheTransform.position - CacheTransform.position).normalized;
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
        }

        public override void OnSetup()
        {
            base.OnSetup();
            // Setup network components
            CacheNetTransform.ownerClientCanSendTransform = true;
            CacheNetTransform.ownerClientNotInterpolate = false;
            // Register Network functions
            RegisterNetFunction("UpdateDirection", new LiteNetLibFunction<NetFieldSByte, NetFieldSByte>((x, y) => NetFuncUpdateDirection(x, y)));
        }

        public override void KeyMovement(Vector3 direction, bool isJump)
        {
            if (IsDead())
                return;
            moveDirection = direction;
            if (moveDirection.magnitude == 0)
                CacheRigidbody2D.velocity = Vector2.zero;
        }

        public override void PointClickMovement(Vector3 position)
        {
            if (IsDead())
                return;
            currentDestination = position;
        }

        public override void StopMove()
        {
            currentDestination = null;
            moveDirection = Vector3.zero;
            CacheRigidbody2D.velocity = Vector2.zero;
        }

        public override int OverlapObjects(Vector3 position, float distance, int layerMask)
        {
            return Physics2D.OverlapCircleNonAlloc(position, distance, overlapColliders2D, layerMask);
        }

        public override GameObject GetOverlapObject(int index)
        {
            return tempGameObject = overlapColliders2D[index].gameObject;
        }

        public override bool IsPositionInFov(float fov, Vector3 position)
        {
            var halfFov = fov * 0.5f;
            var angle = Vector2.Angle((CacheTransform.position - position).normalized, currentDirection);
            // Angle in forward position is 180 so we use this value to determine that target is in hit fov or not
            return (angle < 180 + halfFov && angle > 180 - halfFov);
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
            if (IsServer)
                currentDirectionType.Value = (byte)localDirectionType;
        }

        private void UpdateCurrentDirection(Vector2 direction)
        {
            currentDirection = direction;
            UpdateDirection(currentDirection);
            if (!currentDirection.Equals(dirtyDirection))
            {
                dirtyDirection = currentDirection;
                RequestUpdateDirection();
            }
        }

        private void NetFuncUpdateDirection(sbyte x, sbyte y)
        {
            tempDirection = new Vector2((float)x / 100, (float)y / 100);
            UpdateDirection(tempDirection);
            if (!IsOwnerClient)
                currentDirection = tempDirection;
        }
        
        public virtual void RequestUpdateDirection()
        {
            var x = (sbyte)(currentDirection.x * 100);
            var y = (sbyte)(currentDirection.y * 100);
            CallNetFunction("UpdateDirection", FunctionReceivers.Server, x, y);
        }
    }
}
