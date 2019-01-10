using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using LiteNetLibManager;
using UnityEngine.Profiling;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterModel))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public partial class PlayerCharacterEntity : BasePlayerCharacterEntity
    {
        #region Settings
        [Header("Movement AI")]
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;
        [Header("Movement Settings")]
        public float groundingDistance = 0.1f;
        public float jumpHeight = 2f;
        public float gravityRate = 1f;
        public float angularSpeed = 800f;
        [Header("Network Settings")]
        public MovementSecure movementSecure;
        #endregion

        public bool isJumping { get; protected set; }
        public bool isGrounded { get; protected set; }
        public Queue<Vector3> navPaths { get; protected set; }

        public bool HasNavPaths
        {
            get { return navPaths != null && navPaths.Count > 0; }
        }

        public override float StoppingDistance
        {
            get { return stoppingDistance; }
        }

        private Rigidbody cacheRigidbody;
        public Rigidbody CacheRigidbody
        {
            get
            {
                if (cacheRigidbody == null)
                    cacheRigidbody = GetComponent<Rigidbody>();
                return cacheRigidbody;
            }
        }

        private CapsuleCollider cacheCapsuleCollider;
        public CapsuleCollider CacheCapsuleCollider
        {
            get
            {
                if (cacheCapsuleCollider == null)
                    cacheCapsuleCollider = GetComponent<CapsuleCollider>();
                return cacheCapsuleCollider;
            }
        }

        // Optimize garbage collector
        private float tempMoveDirectionMagnitude;
        private Vector3 tempInputDirection;
        private Vector3 tempMoveDirection;
        private Vector3 tempTargetPosition;
        private Vector3 tempCurrentPosition;
        private Vector3 tempPreviousVelocity;
        private Vector3 tempTargetVelocity;
        private Vector3 tempTargetDirection;
        private Quaternion tempLookAtRotation;

        protected override void EntityAwake()
        {
            base.EntityAwake();
            CacheRigidbody.useGravity = false;
            StopMove();
        }

        protected override void EntityUpdate()
        {
            base.EntityUpdate();
            Profiler.BeginSample("PlayerCharacterEntity - Update");
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
            Profiler.BeginSample("PlayerCharacterEntity - FixedUpdate");

            if (movementSecure == MovementSecure.ServerAuthoritative && !IsServer)
                return;

            if (movementSecure == MovementSecure.NotSecure && !IsOwnerClient)
                return;

            tempMoveDirection = Vector3.zero;

            if (HasNavPaths)
            {
                tempTargetPosition = navPaths.Peek();
                tempTargetPosition.y = 0;
                tempCurrentPosition = CacheTransform.position;
                tempCurrentPosition.y = 0;
                tempMoveDirection = (tempTargetPosition - tempCurrentPosition).normalized;
                if (Vector3.Distance(tempTargetPosition, tempCurrentPosition) < StoppingDistance)
                {
                    navPaths.Dequeue();
                    if (!HasNavPaths)
                        StopMove();
                }
            }

            tempPreviousVelocity = CacheRigidbody.velocity;
            if (!IsDead())
            {
                // If move by WASD keys, set move direction to input direction
                if (tempInputDirection.magnitude != 0f)
                    tempMoveDirection = tempInputDirection;

                tempMoveDirectionMagnitude = tempMoveDirection.magnitude;
                if (!IsPlayingActionAnimation() && tempMoveDirectionMagnitude != 0f)
                {
                    if (tempMoveDirectionMagnitude > 1)
                        tempMoveDirection = tempMoveDirection.normalized;

                    tempTargetVelocity = tempMoveDirection * gameInstance.GameplayRule.GetMoveSpeed(this);

                    // Apply a force that attempts to reach our target velocity
                    Vector3 velocityChange = (tempTargetVelocity - tempPreviousVelocity);
                    velocityChange.x = Mathf.Clamp(velocityChange.x, -CacheMoveSpeed, CacheMoveSpeed);
                    velocityChange.y = 0;
                    velocityChange.z = Mathf.Clamp(velocityChange.z, -CacheMoveSpeed, CacheMoveSpeed);
                    CacheRigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
                    tempLookAtRotation = Quaternion.RotateTowards(CacheTransform.rotation, Quaternion.LookRotation(tempMoveDirection), angularSpeed * Time.fixedDeltaTime);
                    tempLookAtRotation.x = 0;
                    tempLookAtRotation.z = 0;
                    CacheTransform.rotation = tempLookAtRotation;
                }
                else
                {
                    // Stop movement
                    CacheRigidbody.velocity = new Vector3(0, CacheRigidbody.velocity.y, 0);
                }

                BaseGameEntity tempEntity;
                if (tempMoveDirectionMagnitude == 0f && TryGetTargetEntity(out tempEntity))
                {
                    tempTargetDirection = (tempEntity.CacheTransform.position - CacheTransform.position).normalized;
                    if (tempTargetDirection.magnitude != 0f)
                    {
                        tempLookAtRotation = Quaternion.RotateTowards(CacheTransform.rotation, Quaternion.LookRotation(tempTargetDirection), angularSpeed * Time.fixedDeltaTime);
                        tempLookAtRotation.x = 0f;
                        tempLookAtRotation.z = 0f;
                        CacheTransform.rotation = tempLookAtRotation;
                    }
                }
                // Jump
                if (isGrounded && isJumping)
                {
                    RequestTriggerJump();
                    CacheRigidbody.velocity = new Vector3(tempPreviousVelocity.x, CalculateJumpVerticalSpeed(), tempPreviousVelocity.z);
                    isJumping = false;
                }
            }

            if (Mathf.Abs(tempPreviousVelocity.y) > groundingDistance)
                isGrounded = false;

            // We apply gravity manually for more tuning control
            CacheRigidbody.AddForce(new Vector3(0, Physics.gravity.y * CacheRigidbody.mass * gravityRate, 0));
            Profiler.EndSample();
        }

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
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
            RegisterNetFunction(NetFuncTriggerJump);
            RegisterNetFunction<Vector3>(NetFuncPointClickMovement);
            RegisterNetFunction<sbyte, sbyte, bool>(NetFuncKeyMovement);
            RegisterNetFunction(StopMove);
            RegisterNetFunction<PackedUInt>(NetFuncSetTargetEntity);
        }

        protected void NetFuncPointClickMovement(Vector3 position)
        {
            if (IsDead())
                return;
            SetMovePaths(position, true);
            currentNpcDialog = null;
        }

        protected void NetFuncKeyMovement(sbyte horizontalInput, sbyte verticalInput, bool isJump)
        {
            if (IsDead())
                return;
            // Devide inputs to float value
            tempInputDirection = new Vector3((float)horizontalInput / 100f, 0, (float)verticalInput / 100f);
            if (tempInputDirection.magnitude != 0)
                currentNpcDialog = null;
            if (!isJumping)
                isJumping = isGrounded && isJump;
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

        protected virtual void NetFuncTriggerJump()
        {
            if (IsDead())
                return;
            if (movementSecure == MovementSecure.NotSecure && IsOwnerClient)
                return;
            // Play jump animation on non owner clients
            CharacterModel.PlayJumpAnimation();
        }

        public virtual void RequestTriggerJump()
        {
            if (IsDead())
                return;
            // Play jump animation immediately on owner client
            if (IsOwnerClient)
                CharacterModel.PlayJumpAnimation();
            // Only server will call for clients to trigger jump animation for secure entity
            if (IsServer)
                CallNetFunction(NetFuncTriggerJump, FunctionReceivers.All);
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
                    SetMovePaths(position, true);
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
                    CallNetFunction(NetFuncKeyMovement, FunctionReceivers.Server, (sbyte)(direction.x * 100), (sbyte)(direction.z * 100), isJump);
                    break;
                case MovementSecure.NotSecure:
                    tempInputDirection = direction;
                    if (!isJumping)
                        isJumping = isGrounded && isJump;
                    break;
            }
        }

        public override void StopMove()
        {
            navPaths = null;
            tempMoveDirection = Vector3.zero;
            CacheRigidbody.velocity = new Vector3(0, CacheRigidbody.velocity.y, 0);
            if (IsOwnerClient && !IsServer)
                CallNetFunction(StopMove, FunctionReceivers.Server);
        }

        public override void SetTargetEntity(BaseGameEntity entity)
        {
            if (IsOwnerClient && !IsServer && targetEntity != entity)
                CallNetFunction(NetFuncSetTargetEntity, FunctionReceivers.Server, new PackedUInt(entity == null ? 0 : entity.ObjectId));
            base.SetTargetEntity(entity);
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (!isGrounded && collision.impulse.y > 0)
                isGrounded = true;
        }

        protected virtual void OnCollisionStay(Collision collision)
        {
            if (!isGrounded && collision.impulse.y > 0)
                isGrounded = true;
        }

        protected virtual void SetMovePaths(Vector3 position, bool useNavMesh)
        {
            if (useNavMesh)
            {
                NavMeshPath navPath = new NavMeshPath();
                if (NavMesh.CalculatePath(CacheTransform.position, position, NavMesh.AllAreas, navPath))
                {
                    navPaths = new Queue<Vector3>(navPath.corners);
                    // Dequeue first path it's not require for future movement
                    navPaths.Dequeue();
                }
            }
            else
            {
                // If not use nav mesh, just move to position by direction
                navPaths = new Queue<Vector3>();
                navPaths.Enqueue(position);
            }
        }

        private float CalculateJumpVerticalSpeed()
        {
            // From the jump height and gravity we deduce the upwards speed 
            // for the character to reach at the apex.
            return Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y * gravityRate);
        }
    }
}
