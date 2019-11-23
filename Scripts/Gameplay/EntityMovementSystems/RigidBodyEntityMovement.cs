using LiteNetLib;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(LiteNetLibTransform))]
    public class RigidBodyEntityMovement : BaseEntityMovement
    {
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

        [Header("Root Motion Settings")]
        public bool useRootMotionForMovement;
        public bool useRootMotionForAirMovement;
        public bool useRootMotionForJump;
        public bool useRootMotionForFall;

        [Header("Network Settings")]
        public MovementSecure movementSecure;

        protected MovementState tempMovementState = MovementState.None;
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

        private Animator cacheAnimator;
        public Animator CacheAnimator
        {
            get
            {
                if (cacheAnimator == null)
                    cacheAnimator = GetComponent<Animator>();
                if (cacheAnimator == null)
                    cacheAnimator = gameObject.AddComponent<Animator>();
                return cacheAnimator;
            }
        }

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

        private Rigidbody cacheRigidbody;
        public Rigidbody CacheRigidbody
        {
            get
            {
                if (cacheRigidbody == null)
                    cacheRigidbody = GetComponent<Rigidbody>();
                if (cacheRigidbody == null)
                    cacheRigidbody = gameObject.AddComponent<Rigidbody>();
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
                if (cacheCapsuleCollider == null)
                    cacheCapsuleCollider = gameObject.AddComponent<CapsuleCollider>();
                return cacheCapsuleCollider;
            }
        }

        public override float StoppingDistance
        {
            get { return stoppingDistance; }
        }

        public Queue<Vector3> navPaths { get; protected set; }
        public bool HasNavPaths
        {
            get { return navPaths != null && navPaths.Count > 0; }
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

        protected virtual void Awake()
        {
            CacheRigidbody.useGravity = false;
            StopMove();
        }

        protected virtual void OnEnable()
        {
            CacheNetTransform.enabled = true;
            CacheRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        protected virtual void OnDisable()
        {
            CacheNetTransform.enabled = false;
            CacheRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }

        protected void OnAnimatorMove()
        {
            if (!MovementState.HasFlag(MovementState.Forward) &&
                !MovementState.HasFlag(MovementState.Backward) &&
                !MovementState.HasFlag(MovementState.Left) &&
                !MovementState.HasFlag(MovementState.Right) &&
                !MovementState.HasFlag(MovementState.IsJump))
            {
                // No movement, apply root motion position / rotation
                CacheAnimator.ApplyBuiltinRootMotion();
                return;
            }

            if (MovementState.HasFlag(MovementState.IsGrounded) && useRootMotionForMovement)
                cacheAnimator.ApplyBuiltinRootMotion();
            if (!MovementState.HasFlag(MovementState.IsGrounded) && useRootMotionForAirMovement)
                cacheAnimator.ApplyBuiltinRootMotion();
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
            entity.RegisterNetFunction(NetFuncTriggerJump);
            entity.RegisterNetFunction<Vector3>(NetFuncPointClickMovement);
            entity.RegisterNetFunction<sbyte, sbyte, byte>(NetFuncKeyMovement);
            entity.RegisterNetFunction<short>(NetFuncUpdateYRotation);
            entity.RegisterNetFunction(StopMove);
            entity.RegisterNetFunction<byte>(NetFuncSetMovement);
            entity.RegisterNetFunction<byte>(NetFuncSetExtraMovement);
        }

        protected void NetFuncKeyMovement(sbyte horizontalInput, sbyte verticalInput, byte movementState)
        {
            if (!CacheEntity.CanMove())
                return;
            // Devide inputs to float value
            tempInputDirection = new Vector3((float)horizontalInput / 100f, 0, (float)verticalInput / 100f);
            tempMovementState = (MovementState)movementState;
            if (!IsJumping)
                IsJumping = IsGrounded && tempMovementState.HasFlag(MovementState.IsJump);
        }

        protected void NetFuncPointClickMovement(Vector3 position)
        {
            if (!CacheEntity.CanMove())
                return;
            tempMovementState = MovementState.Forward;
            SetMovePaths(position, true);
        }

        protected void NetFuncUpdateYRotation(short yRotation)
        {
            if (!CacheEntity.CanMove())
                return;
            if (!HasNavPaths)
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

        protected void NetFuncTriggerJump()
        {
            if (!CacheEntity.CanMove())
                return;
            // Not play jump animation on owner client when running in not secure mode
            if (movementSecure == MovementSecure.NotSecure && IsOwnerClient && !IsServer)
                return;
            // Play jump animation on non owner clients
            if (CacheEntity.Model is IJumppableModel)
                (CacheEntity.Model as IJumppableModel).PlayJumpAnimation();
        }

        public void RequestTriggerJump()
        {
            if (!CacheEntity.CanMove())
                return;
            // Play jump animation immediately on owner client, if not running in server
            if (IsOwnerClient && !IsServer && CacheEntity.Model is IJumppableModel)
                (CacheEntity.Model as IJumppableModel).PlayJumpAnimation();
            // Play jump animation on other clients
            CacheEntity.CallNetFunction(NetFuncTriggerJump, FunctionReceivers.All);
        }

        public override void StopMove()
        {
            navPaths = null;
            CacheRigidbody.velocity = new Vector3(0, CacheRigidbody.velocity.y, 0);
            if (IsOwnerClient && !IsServer)
                CacheEntity.CallNetFunction(StopMove, FunctionReceivers.Server);
        }

        public override void KeyMovement(Vector3 moveDirection, MovementState movementState)
        {
            if (!CacheEntity.CanMove())
                return;

            if (useNavMeshForKeyMovement && moveDirection.magnitude > 0.5f)
            {
                PointClickMovement(CacheTransform.position + moveDirection);
                return;
            }

            switch (movementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    // Multiply with 100 and cast to sbyte to reduce packet size
                    // then it will be devided with 100 later on server side
                    CacheEntity.CallNetFunction(NetFuncKeyMovement, FunctionReceivers.Server, (sbyte)(moveDirection.x * 100), (sbyte)(moveDirection.z * 100), (byte)movementState);
                    break;
                case MovementSecure.NotSecure:
                    tempInputDirection = moveDirection;
                    tempMovementState = movementState;
                    if (!IsJumping)
                        IsJumping = IsGrounded && movementState.HasFlag(MovementState.IsJump);
                    break;
            }
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
                    tempMovementState = MovementState.Forward;
                    SetMovePaths(position, true);
                    break;
            }
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
                    if (!HasNavPaths)
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
            // TODO: implement this
            result = fromPosition;
        }

        private void FixedUpdate()
        {
            if (movementSecure == MovementSecure.ServerAuthoritative && !IsServer)
            {
                if (CacheRigidbody.useGravity)
                    CacheRigidbody.useGravity = false;
                return;
            }

            if (movementSecure == MovementSecure.NotSecure && !IsOwnerClient)
            {
                if (CacheRigidbody.useGravity)
                    CacheRigidbody.useGravity = false;
                return;
            }

            // Turn Use Gravity when this is allowed to update
            if (!useRootMotionForFall && !CacheRigidbody.useGravity)
                CacheRigidbody.useGravity = true;
            if (useRootMotionForFall && CacheRigidbody.useGravity)
                CacheRigidbody.useGravity = false;

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
                SetMovementState(MovementState.None);
            }
            else
            {
                // Send movement state which received from owning client
                SetMovementState(tempMovementState);
            }
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

            if (!CacheEntity.CanMove())
            {
                tempMoveDirection = Vector3.zero;
                IsJumping = false;
            }

            if (tempMoveDirection.magnitude > 0f)
            {
                tempMoveDirection = tempMoveDirection.normalized;

                // always move along the camera forward as it is the direction that it being aimed at
                tempMoveDirection = Vector3.ProjectOnPlane(tempMoveDirection, groundContactNormal).normalized;

                float currentTargetSpeed = CacheEntity.GetMoveSpeed();
                // If character move backward
                if (Vector3.Angle(tempMoveDirection, CacheTransform.forward) > 120)
                    currentTargetSpeed *= backwardMoveSpeedRate;

                tempMoveDirection *= currentTargetSpeed;
                if (IsGrounded && !useRootMotionForMovement)
                    CacheRigidbody.velocity = tempMoveDirection;
                else if (!IsGrounded && !useRootMotionForAirMovement)
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
                    if (!useRootMotionForJump)
                    {
                        CacheRigidbody.drag = 0f;
                        CacheRigidbody.velocity = new Vector3(CacheRigidbody.velocity.x, 0f, CacheRigidbody.velocity.z);
                        CacheRigidbody.AddForce(new Vector3(0f, CalculateJumpVerticalSpeed(), 0f), ForceMode.Impulse);
                    }
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

        protected void SetMovePaths(Vector3 position, bool useNavMesh)
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
