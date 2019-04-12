using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Profiling;
using LiteNetLibManager;
using LiteNetLib;

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
        public float jumpHeight = 2f;
        public float gravityRate = 1f;
        public float backwardMoveSpeedRate = 0.75f;
        public float groundCheckDistance = 0.1f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
        public float groundCheckDistanceWhileJump = 0.01f;
        public float stickToGroundHelperDistance = 0.5f; // stops the character
        [Tooltip("set it to 0.1 or more if you get stuck in wall")]
        public float shellOffset = 0f;
        public bool useNavMeshForKeyMovement;
        [Header("Network Settings")]
        public MovementSecure movementSecure;
        #endregion

        public Queue<Vector3> navPaths { get; protected set; }

        public bool HasNavPaths
        {
            get { return navPaths != null && navPaths.Count > 0; }
        }

        public override float StoppingDistance
        {
            get { return stoppingDistance; }
        }

        protected MovementFlag tempMovementState = MovementFlag.None;
        protected MovementFlag localMovementState = MovementFlag.None;
        public override MovementFlag MovementState
        {
            get
            {
                if (IsOwnerClient && movementSecure == MovementSecure.NotSecure)
                    return localMovementState;
                return base.MovementState;
            }
            set { base.MovementState = value; }
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
        private Vector3 tempInputDirection;
        private Vector3 tempMoveDirection;
        private Vector3 tempTargetPosition;
        private Vector3 tempCurrentPosition;
        private Vector3 groundContactNormal;
        private float tempTargetDistance;
        private bool previouslyGrounded;
        private bool applyingJump;

        protected override void EntityAwake()
        {
            base.EntityAwake();
            CacheRigidbody.useGravity = true;
            CacheRigidbody.freezeRotation = true;
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
            tempTargetDistance = -1f;

            if (HasNavPaths)
            {
                tempTargetPosition = navPaths.Peek();
                tempTargetPosition.y = 0;
                tempCurrentPosition = CacheTransform.position;
                tempCurrentPosition.y = 0;
                tempMoveDirection = (tempTargetPosition - tempCurrentPosition).normalized;
                tempTargetDistance = Vector3.Distance(tempTargetPosition, tempCurrentPosition);
                if (tempTargetDistance < StoppingDistance)
                {
                    navPaths.Dequeue();
                    if (!HasNavPaths)
                        StopMove();
                }
                else
                {
                    // Turn character to destination
                    CacheTransform.rotation = Quaternion.LookRotation(tempMoveDirection);
                }
            }

            UpdateMovement();

            if (tempMoveDirection.Equals(Vector3.zero))
            {
                // No movement so state is none
                SetMovementState(MovementFlag.None);
            }
            else
            {
                // Send movement state which received from owning client
                SetMovementState(tempMovementState);
            }
            Profiler.EndSample();
        }

        private int GetGroundDetectionLayerMask()
        {
            int layerMask = gameInstance.characterLayer.Mask | gameInstance.itemDropLayer.Mask;
            return ~layerMask;
        }

        private void StickToGroundHelper()
        {
            float radius = CacheCapsuleCollider.radius * (1.0f - shellOffset);
            radius = radius * transform.localScale.z;
            float maxDistance = ((CacheCapsuleCollider.height / 2f) - CacheCapsuleCollider.radius) + stickToGroundHelperDistance;
            maxDistance = maxDistance * transform.localScale.y;
            float centerY = CacheCapsuleCollider.center.y;
            centerY = centerY * transform.localScale.y;
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position + Vector3.up * centerY, radius, Vector3.down, out hitInfo,
                                   maxDistance, GetGroundDetectionLayerMask(), QueryTriggerInteraction.Ignore))
            {
                if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
                {
                    CacheRigidbody.velocity = Vector3.ProjectOnPlane(CacheRigidbody.velocity, hitInfo.normal);
                }
            }
        }

        private void GroundCheck()
        {
            previouslyGrounded = IsGrounded;
            float radius = CacheCapsuleCollider.radius * (1.0f - shellOffset);
            radius = radius * transform.localScale.z;
            float maxDistance = ((CacheCapsuleCollider.height / 2f) - CacheCapsuleCollider.radius);
            if (applyingJump)
                maxDistance += groundCheckDistanceWhileJump;
            else
                maxDistance += groundCheckDistance;
            maxDistance = maxDistance * transform.localScale.y;
            float centerY = CacheCapsuleCollider.center.y;
            centerY = centerY * transform.localScale.y;
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position + Vector3.up * centerY, radius, Vector3.down, out hitInfo,
                                   maxDistance, GetGroundDetectionLayerMask(), QueryTriggerInteraction.Ignore))
            {
                IsGrounded = true;
                groundContactNormal = hitInfo.normal;
            }
            else
            {
                IsGrounded = false;
                groundContactNormal = Vector3.up;
            }
            if (!previouslyGrounded && IsGrounded && applyingJump)
            {
                applyingJump = false;
            }
        }

        private void UpdateMovement()
        {
            GroundCheck();

            // If move by WASD keys, set move direction to input direction
            if (tempInputDirection.magnitude > 0f)
                tempMoveDirection = tempInputDirection;

            if (IsDead())
            {
                tempMoveDirection = Vector3.zero;
                IsJumping = false;
            }

            if (tempMoveDirection.magnitude > 0f)
            {
                tempMoveDirection = tempMoveDirection.normalized;

                // always move along the camera forward as it is the direction that it being aimed at
                tempMoveDirection = Vector3.ProjectOnPlane(tempMoveDirection, groundContactNormal).normalized;

                float currentTargetSpeed = gameInstance.GameplayRule.GetMoveSpeed(this);
                // If character move backward
                if (Vector3.Angle(tempMoveDirection, CacheTransform.forward) > 120)
                    currentTargetSpeed *= backwardMoveSpeedRate;

                tempMoveDirection *= currentTargetSpeed;
                if (IsGrounded)
                    CacheRigidbody.velocity = tempMoveDirection;
                else
                    CacheRigidbody.velocity = new Vector3(tempMoveDirection.x, CacheRigidbody.velocity.y, tempMoveDirection.z);
            }
            else
            {
                CacheRigidbody.velocity = new Vector3(0f, CacheRigidbody.velocity.y, 0f);
            }

            if (IsGrounded)
            {
                CacheRigidbody.drag = 5f;

                if (IsJumping)
                {
                    RequestTriggerJump();
                    CacheRigidbody.drag = 0f;
                    CacheRigidbody.velocity = new Vector3(CacheRigidbody.velocity.x, 0f, CacheRigidbody.velocity.z);
                    CacheRigidbody.AddForce(new Vector3(0f, CalculateJumpVerticalSpeed(), 0f), ForceMode.Impulse);
                    applyingJump = true;
                    IsGrounded = false;
                }

                if (!applyingJump &&
                    Mathf.Abs(tempMoveDirection.x) < float.Epsilon &&
                    Mathf.Abs(tempMoveDirection.z) < float.Epsilon &&
                    CacheRigidbody.velocity.magnitude < 1f)
                {
                    CacheRigidbody.Sleep();
                }
            }
            else
            {
                CacheRigidbody.drag = 0f;
                if (previouslyGrounded && !applyingJump)
                {
                    StickToGroundHelper();
                }
            }
            IsJumping = false;
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
            RegisterNetFunction<sbyte, sbyte, byte>(NetFuncKeyMovement);
            RegisterNetFunction<short>(NetFuncUpdateYRotation);
            RegisterNetFunction(StopMove);
            RegisterNetFunction<PackedUInt>(NetFuncSetTargetEntity);
            RegisterNetFunction<byte>(NetFuncSetMovementState);
        }

        protected void NetFuncPointClickMovement(Vector3 position)
        {
            if (IsDead())
                return;
            SetMovePaths(position, true);
            currentNpcDialog = null;
        }

        protected void NetFuncKeyMovement(sbyte horizontalInput, sbyte verticalInput, byte movementState)
        {
            if (IsDead())
                return;
            // Devide inputs to float value
            tempInputDirection = new Vector3((float)horizontalInput / 100f, 0, (float)verticalInput / 100f);
            if (tempInputDirection.magnitude != 0)
                currentNpcDialog = null;
            tempMovementState = (MovementFlag)movementState;
            if (!IsJumping)
                IsJumping = IsGrounded && tempMovementState.HasFlag(MovementFlag.IsJump);
        }

        protected void NetFuncUpdateYRotation(short yRotation)
        {
            if (IsDead())
                return;
            if (!HasNavPaths)
                CacheTransform.rotation = Quaternion.Euler(0, (float)yRotation, 0);
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

            MovementState = (MovementFlag)movementState;
        }

        protected virtual void NetFuncTriggerJump()
        {
            if (IsDead())
                return;
            // Not play jump animation on owner client when running in not secure mode
            if (movementSecure == MovementSecure.NotSecure && IsOwnerClient && !IsServer)
                return;
            // Play jump animation on non owner clients
            CharacterModel.PlayJumpAnimation();
        }

        public virtual void RequestTriggerJump()
        {
            if (IsDead())
                return;
            // Play jump animation immediately on owner client, if not running in server
            if (IsOwnerClient && !IsServer)
                CharacterModel.PlayJumpAnimation();
            // Play jump animation on other clients
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

        public override void KeyMovement(Vector3 direction, MovementFlag movementState)
        {
            if (IsDead())
                return;
            if (useNavMeshForKeyMovement)
            {
                PointClickMovement(CacheTransform.position + tempInputDirection);
                return;
            }
            switch (movementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    // Multiply with 100 and cast to sbyte to reduce packet size
                    // then it will be devided with 100 later on server side
                    CallNetFunction(NetFuncKeyMovement, FunctionReceivers.Server, (sbyte)(direction.x * 100), (sbyte)(direction.z * 100), (byte)movementState);
                    break;
                case MovementSecure.NotSecure:
                    tempInputDirection = direction;
                    tempMovementState = movementState;
                    if (!IsJumping)
                        IsJumping = IsGrounded && movementState.HasFlag(MovementFlag.IsJump);
                    break;
            }
        }

        public override void UpdateYRotation(float yRotation)
        {
            if (IsDead())
                return;
            switch (movementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    // Multiply with 100 and cast to short to reduce packet size
                    // then it will be devided with 100 later on server side
                    CallNetFunction(NetFuncUpdateYRotation, FunctionReceivers.Server, (short)yRotation);
                    break;
                case MovementSecure.NotSecure:
                    if (!HasNavPaths)
                        CacheTransform.rotation = Quaternion.Euler(0, yRotation, 0);
                    break;
            }
        }

        public void SetMovementState(MovementFlag state)
        {
            if (IsGrounded)
                state |= MovementFlag.IsGrounded;

            // Set local movement state which will be used by owner client
            localMovementState = state;

            if (movementSecure == MovementSecure.ServerAuthoritative && IsServer)
                MovementState = state;

            if (movementSecure == MovementSecure.NotSecure && IsOwnerClient)
                CallNetFunction(NetFuncSetMovementState, DeliveryMethod.Sequenced, FunctionReceivers.Server, (byte)state);
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

        protected virtual void SetMovePaths(Vector3 position, bool useNavMesh)
        {
            if (useNavMesh)
            {
                NavMeshPath navPath = new NavMeshPath();
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(position, out navHit, 5f, NavMesh.AllAreas) &&
                    NavMesh.CalculatePath(CacheTransform.position, navHit.position, NavMesh.AllAreas, navPath))
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
            return Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y);
        }
    }
}
