using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

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

        #region Temp data
        protected Collider2D[] overlapColliders2D = new Collider2D[OVERLAP_COLLIDER_SIZE];
        protected Vector2 currentDirection;
        protected Vector2? currentDestination;
        #endregion

        public Vector2 moveDirection { get; protected set; }

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
            Profiler.EndSample();
        }

        protected override void EntityFixedUpdate()
        {
            base.EntityFixedUpdate();
            Profiler.BeginSample("PlayerCharacterEntity2D - FixedUpdate");
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
                    
                    CacheRigidbody2D.velocity = moveDirection * CacheMoveSpeed;
                }
            }
            Profiler.EndSample();
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

        protected override int OverlapObjects(Vector3 position, float distance, int layerMask)
        {
            return Physics2D.OverlapCircleNonAlloc(position, distance, overlapColliders2D, layerMask);
        }

        protected override GameObject GetOverlapObject(int index)
        {
            return tempGameObject = overlapColliders2D[index].gameObject;
        }

        protected override bool IsPositionInAttackFov(float fov, Vector3 position)
        {
            var halfFov = fov * 0.5f;
            var angle = Vector2.Angle((CacheTransform.position - position).normalized, currentDirection);
            // Angle in forward position is 180 so we use this value to determine that target is in hit fov or not
            return (angle < 180 + halfFov && angle > 180 - halfFov);
        }
    }
}
