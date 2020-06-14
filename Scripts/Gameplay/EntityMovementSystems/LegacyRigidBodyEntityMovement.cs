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
    public class LegacyRigidBodyEntityMovement : BaseEntityMovement
    {
        [Header("Movement AI")]
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;

        [Header("Movement Settings")]
        public float jumpHeight = 2f;
        public float backwardMoveSpeedRate = 0.75f;
        public float groundCheckDistance = 0.1f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
        public float groundCheckDistanceWhileJump = 0.01f; // distance for checking if the controller is grounded while jumping
        public float stickToGroundHelperDistance = 0.5f; // distance for checking if the controller is grounded while moving on slopes
        [Range(0.1f, 1f)]
        public float underWaterThreshold = 0.75f;
        public bool autoSwimToSurface;

        [Header("Root Motion Settings")]
        public bool useRootMotionForMovement;
        public bool useRootMotionForAirMovement;
        public bool useRootMotionForJump;
        public bool useRootMotionForFall;
        public bool useRootMotionWhileNotMoving;

        public Animator CacheAnimator { get; private set; }
        public LiteNetLibTransform CacheNetTransform { get; private set; }
        public Rigidbody CacheRigidbody { get; private set; }
        public CapsuleCollider CacheCapsuleCollider { get; private set; }

        public override float StoppingDistance
        {
            get { return stoppingDistance; }
        }

        public Queue<Vector3> navPaths { get; protected set; }
        public bool HasNavPaths
        {
            get { return navPaths != null && navPaths.Count > 0; }
        }

        // Movement codes
        private bool isGrounded;
        private bool isUnderWater;
        private bool isJumping;
        private Collider waterCollider;

        // Optimize garbage collector
        private MovementState tempMovementState;
        private Vector3 tempInputDirection;
        private Vector3 tempMoveDirection;
        private Vector3 tempHorizontalMoveDirection;
        private Vector3 tempMoveVelocity;
        private Vector3 tempTargetPosition;
        private Vector3 tempCurrentPosition;
        private Vector3 groundContactNormal;
        private Vector3 tempPredictPosition;
        private float tempSqrMagnitude;
        private float tempPredictSqrMagnitude;
        private float tempTargetDistance;
        private float tempEntityMoveSpeed;
        private float tempCurrentMoveSpeed;
        private bool previouslyGrounded;
        private bool applyingJump;

        public override void EntityAwake()
        {
            // Prepare animator component
            CacheAnimator = GetComponent<Animator>();
            // Prepare network transform component
            CacheNetTransform = GetComponent<LiteNetLibTransform>();
            if (CacheNetTransform == null)
                CacheNetTransform = gameObject.AddComponent<LiteNetLibTransform>();
            // Prepare rigidbody component
            CacheRigidbody = GetComponent<Rigidbody>();
            if (CacheRigidbody == null)
                CacheRigidbody = gameObject.AddComponent<Rigidbody>();
            // Prepare collider component
            CacheCapsuleCollider = GetComponent<CapsuleCollider>();
            if (CacheCapsuleCollider == null)
                CacheCapsuleCollider = gameObject.AddComponent<CapsuleCollider>();
            // Setup
            CacheRigidbody.useGravity = false;
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
            CacheRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        public override void ComponentOnDisable()
        {
            CacheNetTransform.enabled = false;
            CacheRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }

        protected void OnAnimatorMove()
        {
            if (!CacheAnimator)
                return;

            if (useRootMotionWhileNotMoving &&
                !CacheEntity.MovementState.HasFlag(MovementState.Forward) &&
                !CacheEntity.MovementState.HasFlag(MovementState.Backward) &&
                !CacheEntity.MovementState.HasFlag(MovementState.Left) &&
                !CacheEntity.MovementState.HasFlag(MovementState.Right) &&
                !CacheEntity.MovementState.HasFlag(MovementState.IsJump))
            {
                // No movement, apply root motion position / rotation
                CacheAnimator.ApplyBuiltinRootMotion();
                return;
            }

            if (CacheEntity.MovementState.HasFlag(MovementState.IsGrounded) && useRootMotionForMovement)
                CacheAnimator.ApplyBuiltinRootMotion();
            if (!CacheEntity.MovementState.HasFlag(MovementState.IsGrounded) && useRootMotionForAirMovement)
                CacheAnimator.ApplyBuiltinRootMotion();
        }

        public override void OnSetup()
        {
            base.OnSetup();
            // Register Network functions
            RegisterNetFunction<Vector3>(NetFuncPointClickMovement);
            RegisterNetFunction<DirectionVector3, MovementState>(NetFuncKeyMovement);
            RegisterNetFunction<short>(NetFuncUpdateYRotation);
            RegisterNetFunction(StopMove);
        }

        protected void NetFuncKeyMovement(DirectionVector3 inputDirection, MovementState movementState)
        {
            if (!CacheEntity.CanMove())
                return;
            tempInputDirection = inputDirection;
            tempMovementState = movementState;
            if (tempInputDirection.sqrMagnitude > 0)
                navPaths = null;
            if (!isJumping)
                isJumping = isGrounded && tempMovementState.HasFlag(MovementState.IsJump);
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

        public override void StopMove()
        {
            navPaths = null;
            CacheRigidbody.velocity = new Vector3(0, CacheRigidbody.velocity.y, 0);
            if (IsOwnerClient && !IsServer)
                CallNetFunction(StopMove, FunctionReceivers.Server);
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
                    CallNetFunction(NetFuncKeyMovement, FunctionReceivers.Server, new DirectionVector3(moveDirection), movementState);
                    break;
                case MovementSecure.NotSecure:
                    tempInputDirection = moveDirection;
                    tempMovementState = movementState;
                    if (tempInputDirection.sqrMagnitude > 0)
                        navPaths = null;
                    if (!isJumping)
                        isJumping = isGrounded && tempMovementState.HasFlag(MovementState.IsJump);
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
                    CallNetFunction(NetFuncPointClickMovement, FunctionReceivers.Server, position);
                    break;
                case MovementSecure.NotSecure:
                    tempMovementState = MovementState.Forward;
                    SetMovePaths(position, true);
                    break;
            }
        }

        public override void SetLookRotation(Quaternion rotation)
        {
            if (!CacheEntity.CanMove())
                return;

            Vector3 eulerAngles = rotation.eulerAngles;
            switch (CacheEntity.MovementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    // Cast to short to reduce packet size
                    CallNetFunction(NetFuncUpdateYRotation, FunctionReceivers.Server, (short)eulerAngles.y);
                    break;
                case MovementSecure.NotSecure:
                    eulerAngles.x = 0;
                    eulerAngles.z = 0;
                    if (!HasNavPaths)
                        CacheTransform.eulerAngles = eulerAngles;
                    break;
            }
        }

        public override Quaternion GetLookRotation()
        {
            return CacheTransform.rotation;
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

        public override void EntityFixedUpdate()
        {
            if (CacheEntity.MovementSecure == MovementSecure.ServerAuthoritative && !IsServer)
            {
                if (CacheRigidbody.useGravity)
                    CacheRigidbody.useGravity = false;
                return;
            }

            if (CacheEntity.MovementSecure == MovementSecure.NotSecure && !IsOwnerClient)
            {
                if (CacheRigidbody.useGravity)
                    CacheRigidbody.useGravity = false;
                return;
            }

            // Turn Use Gravity when this is allowed to update
            if (!useRootMotionForFall && CacheRigidbody.useGravity != !isUnderWater)
                CacheRigidbody.useGravity = !isUnderWater;
            if (useRootMotionForFall && CacheRigidbody.useGravity)
                CacheRigidbody.useGravity = false;

            UpdateMovement(Time.deltaTime);

            tempMovementState = tempMoveDirection.sqrMagnitude > 0f ? tempMovementState : MovementState.None;
            if (isUnderWater)
                tempMovementState |= MovementState.IsUnderWater;
            if (isGrounded)
                tempMovementState |= MovementState.IsGrounded;
            CacheEntity.SetMovement(tempMovementState);
        }

        private int GetGroundDetectionLayerMask()
        {
            int layerMask = CurrentGameInstance.characterLayer.Mask | CurrentGameInstance.itemDropLayer.Mask;
            return ~layerMask;
        }

        private void StickToGroundHelper()
        {
            float maxDistance = CacheCapsuleCollider.bounds.extents.y + stickToGroundHelperDistance;
            // BoxCast to find ground to stick the character
            RaycastHit hitInfo;
            if (Physics.BoxCast(CacheCapsuleCollider.bounds.center + Vector3.up * CacheCapsuleCollider.bounds.extents.y,
                CacheCapsuleCollider.bounds.extents,
                Vector3.down, out hitInfo,
                CacheTransform.rotation, maxDistance,
                GetGroundDetectionLayerMask(),
                QueryTriggerInteraction.Ignore))
            {
                if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
                    CacheRigidbody.velocity = Vector3.ProjectOnPlane(CacheRigidbody.velocity, hitInfo.normal);
            }
        }

        private void WaterCheck()
        {
            if (waterCollider == null)
            {
                // Not in water
                isUnderWater = false;
                return;
            }
            float footToSurfaceDist = waterCollider.bounds.max.y - CacheCapsuleCollider.bounds.min.y;
            float currentThreshold = footToSurfaceDist / (CacheCapsuleCollider.bounds.max.y - CacheCapsuleCollider.bounds.min.y);
            isUnderWater = currentThreshold >= underWaterThreshold;
        }

        private void GroundCheck()
        {
            previouslyGrounded = isGrounded;
            if (isUnderWater)
                applyingJump = false;
            float maxDistance = CacheCapsuleCollider.bounds.extents.y + (applyingJump ? groundCheckDistanceWhileJump : groundCheckDistance);
            // BoxCast to find ground
            RaycastHit hitInfo;
            if (Physics.BoxCast(CacheCapsuleCollider.bounds.center + Vector3.up * CacheCapsuleCollider.bounds.extents.y,
                CacheCapsuleCollider.bounds.extents,
                Vector3.down, out hitInfo,
                CacheTransform.rotation, maxDistance,
                GetGroundDetectionLayerMask(),
                QueryTriggerInteraction.Ignore))
            {
                isGrounded = true;
                groundContactNormal = hitInfo.normal;
            }
            else
            {
                isGrounded = false;
                groundContactNormal = Vector3.up;
            }

            if (!previouslyGrounded && isGrounded && applyingJump)
            {
                applyingJump = false;
            }
        }

        private void UpdateMovement(float deltaTime)
        {
            WaterCheck();
            GroundCheck();
            tempMoveDirection = Vector3.zero;
            tempTargetDistance = -1f;

            if (HasNavPaths)
            {
                // Set `tempTargetPosition` and `tempCurrentPosition`
                tempTargetPosition = navPaths.Peek();
                tempCurrentPosition = CacheTransform.position;
                tempTargetPosition.y = 0;
                tempCurrentPosition.y = 0;
                tempMoveDirection = tempTargetPosition - tempCurrentPosition;
                tempMoveDirection.Normalize();
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

            // If move by WASD keys, set move direction to input direction
            if (tempInputDirection.sqrMagnitude > 0f)
            {
                tempMoveDirection = tempInputDirection;
                tempMoveDirection.Normalize();
            }

            if (!CacheEntity.CanMove())
            {
                tempMoveDirection = Vector3.zero;
                isJumping = false;
            }

            tempEntityMoveSpeed = CacheEntity.GetMoveSpeed();
            tempCurrentMoveSpeed = tempEntityMoveSpeed;
            // Updating horizontal movement (WASD inputs)
            if (tempMoveDirection.sqrMagnitude > 0f)
            {
                // Calculate only horizontal move direction
                tempHorizontalMoveDirection = tempMoveDirection;
                tempHorizontalMoveDirection.y = 0;
                tempHorizontalMoveDirection.Normalize();

                if (!isUnderWater)
                {
                    // always move along the camera forward as it is the direction that it being aimed at
                    tempMoveDirection = Vector3.ProjectOnPlane(tempMoveDirection, groundContactNormal);
                }

                // If character move backward
                if (Vector3.Angle(tempMoveDirection, CacheTransform.forward) > 120)
                    tempCurrentMoveSpeed *= backwardMoveSpeedRate;

                if (HasNavPaths)
                {
                    tempSqrMagnitude = (tempTargetPosition - tempCurrentPosition).sqrMagnitude;
                    tempPredictPosition = tempCurrentPosition + (tempHorizontalMoveDirection * tempCurrentMoveSpeed * deltaTime);
                    tempPredictSqrMagnitude = (tempPredictPosition - tempCurrentPosition).sqrMagnitude;
                    // Check `tempSqrMagnitude` against the `tempPredictSqrMagnitude`
                    // if `tempPredictSqrMagnitude` is greater than `tempSqrMagnitude`,
                    // rigidbody will reaching target and character is moving pass it,
                    // so adjust move speed by distance and time (with physic formula: v=s/t)
                    if (tempPredictSqrMagnitude >= tempSqrMagnitude)
                        tempCurrentMoveSpeed *= tempTargetDistance / deltaTime / tempCurrentMoveSpeed;
                    tempMoveVelocity = tempMoveDirection * tempCurrentMoveSpeed;
                }
                else
                {
                    // Move with wasd keys so it does not have to adjust speed
                    tempMoveVelocity = tempMoveDirection * tempCurrentMoveSpeed;
                }

                if (isUnderWater)
                {
                    // Update velocity while under water
                    CacheRigidbody.velocity = tempMoveVelocity;
                }
                else
                {
                    // Update velocity while not under water
                    if (isGrounded && !useRootMotionForMovement)
                    {
                        // Character is not falling, so don't applies gravity
                        CacheRigidbody.velocity = tempMoveVelocity;
                    }
                    else if (!isGrounded && !useRootMotionForAirMovement)
                    {
                        // Character is falling, so applies gravity
                        CacheRigidbody.velocity = new Vector3(tempMoveVelocity.x, CacheRigidbody.velocity.y, tempMoveVelocity.z);
                    }
                }
            }
            else
            {
                if (isUnderWater)
                {
                    // No gravity applies underwater
                    CacheRigidbody.velocity = Vector3.zero;
                }
                else
                {
                    // Applies gravity
                    CacheRigidbody.velocity = new Vector3(0f, CacheRigidbody.velocity.y, 0f);
                }
            }

            if (isUnderWater)
            {
                CacheRigidbody.drag = 10f;
                tempCurrentMoveSpeed = tempEntityMoveSpeed;
                // Move up to surface while under water
                if (autoSwimToSurface)
                {
                    tempTargetPosition = Vector3.up * (waterCollider.bounds.max.y - (CacheCapsuleCollider.bounds.size.y * underWaterThreshold));
                    tempCurrentPosition = Vector3.up * CacheTransform.position.y;
                    tempTargetDistance = Vector3.Distance(tempTargetPosition, tempCurrentPosition);
                    tempSqrMagnitude = (tempTargetPosition - tempCurrentPosition).sqrMagnitude;
                    tempPredictPosition = tempCurrentPosition + (Vector3.up * tempCurrentMoveSpeed * deltaTime);
                    tempPredictSqrMagnitude = (tempPredictPosition - tempCurrentPosition).sqrMagnitude;
                    // Check `tempSqrMagnitude` against the `tempPredictSqrMagnitude`
                    // if `tempPredictSqrMagnitude` is greater than `tempSqrMagnitude`,
                    // rigidbody will reaching target and character is moving pass it,
                    // so adjust move speed by distance and time (with physic formula: v=s/t)
                    if (tempPredictSqrMagnitude >= tempSqrMagnitude)
                        tempCurrentMoveSpeed *= tempTargetDistance / deltaTime / tempCurrentMoveSpeed;
                    // Swim up to surface
                    CacheRigidbody.velocity = new Vector3(CacheRigidbody.velocity.x, tempCurrentMoveSpeed, CacheRigidbody.velocity.z);
                }
            }
            else if (isGrounded)
            {
                CacheRigidbody.drag = 5f;

                if (isJumping)
                {
                    CacheEntity.TriggerJump();
                    if (!useRootMotionForJump)
                    {
                        CacheRigidbody.drag = 0f;
                        CacheRigidbody.velocity = new Vector3(CacheRigidbody.velocity.x, 0f, CacheRigidbody.velocity.z);
                        CacheRigidbody.AddForce(new Vector3(0f, CalculateJumpVerticalSpeed(), 0f), ForceMode.Impulse);
                    }
                    applyingJump = true;
                }

                if (!applyingJump &&
                    Mathf.Abs(tempMoveDirection.x) < float.Epsilon &&
                    Mathf.Abs(tempMoveDirection.z) < float.Epsilon &&
                    CacheRigidbody.velocity.sqrMagnitude < 1f)
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
            isJumping = false;
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

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicLayers.Water)
            {
                // Enter water
                waterCollider = other;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == PhysicLayers.Water)
            {
                // Exit water
                waterCollider = null;
            }
        }
    }
}
