using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class LegacyRigidBodyEntityMovement : BaseGameEntityComponent<BaseGameEntity>, IEntityMovementComponent
    {
        [Header("Movement AI")]
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;

        [Header("Movement Settings")]
        public float jumpHeight = 2f;
        public ApplyJumpForceMode applyJumpForceMode = ApplyJumpForceMode.ApplyImmediately;
        public float applyJumpForceFixedDuration;
        public float backwardMoveSpeedRate = 0.75f;
        public float groundCheckDistance = 0.1f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
        public float groundCheckDistanceWhileJump = 0.01f; // distance for checking if the controller is grounded while jumping
        public float stickToGroundHelperDistance = 0.5f; // distance for checking if the controller is grounded while moving on slopes
        [Range(0.1f, 1f)]
        public float underWaterThreshold = 0.75f;
        public bool autoSwimToSurface;

        [Header("Interpolate, Extrapolate Settings")]
        public LiteNetLibTransform.InterpolateMode interpolateMode = LiteNetLibTransform.InterpolateMode.FixedSpeed;
        public LiteNetLibTransform.ExtrapolateMode extrapolateMode = LiteNetLibTransform.ExtrapolateMode.None;
        [Range(0.01f, 1f)]
        public float extrapolateSpeedRate = 0.5f;

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

        public float StoppingDistance
        {
            get { return stoppingDistance; }
        }

        public Queue<Vector3> navPaths { get; protected set; }
        public bool HasNavPaths
        {
            get { return navPaths != null && navPaths.Count > 0; }
        }

        // Movement codes
        private PhysicFunctions physicFunctions;
        private bool isGrounded;
        private bool isUnderWater;
        private bool isJumping;
        private bool applyingJump;
        private bool applyingJumpForce;
        private float applyJumpForceCountDown;
        private Collider waterCollider;
        private float yRotation;

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

        public override void EntityAwake()
        {
            physicFunctions = new PhysicFunctions(30);
            // Prepare animator component
            CacheAnimator = GetComponent<Animator>();
            // Prepare network transform component
            CacheNetTransform = gameObject.GetOrAddComponent<LiteNetLibTransform>();
            // Prepare rigidbody component
            CacheRigidbody = gameObject.GetOrAddComponent<Rigidbody>();
            // Prepare collider component
            CacheCapsuleCollider = gameObject.GetOrAddComponent<CapsuleCollider>();
            // Setup
            CacheRigidbody.useGravity = false;
            StopMove();
        }

        public override void EntityStart()
        {
            yRotation = CacheTransform.eulerAngles.y;
        }

        public override void EntityLateUpdate()
        {
            base.EntityLateUpdate();
            // Setup network components
            switch (Entity.MovementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    CacheNetTransform.ownerClientCanSendTransform = false;
                    break;
                case MovementSecure.NotSecure:
                    CacheNetTransform.ownerClientCanSendTransform = true;
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
                !Entity.MovementState.HasFlag(MovementState.Forward) &&
                !Entity.MovementState.HasFlag(MovementState.Backward) &&
                !Entity.MovementState.HasFlag(MovementState.Left) &&
                !Entity.MovementState.HasFlag(MovementState.Right) &&
                !Entity.MovementState.HasFlag(MovementState.IsJump))
            {
                // No movement, apply root motion position / rotation
                CacheAnimator.ApplyBuiltinRootMotion();
                return;
            }

            if (Entity.MovementState.HasFlag(MovementState.IsGrounded) && useRootMotionForMovement)
                CacheAnimator.ApplyBuiltinRootMotion();
            if (!Entity.MovementState.HasFlag(MovementState.IsGrounded) && useRootMotionForAirMovement)
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
            if (!Entity.CanMove())
                return;
            tempInputDirection = inputDirection;
            tempMovementState = movementState;
            if (tempInputDirection.sqrMagnitude > 0)
                navPaths = null;
            if (!isJumping && !applyingJumpForce)
                isJumping = isGrounded && tempMovementState.HasFlag(MovementState.IsJump);
        }

        protected void NetFuncPointClickMovement(Vector3 position)
        {
            if (!Entity.CanMove())
                return;
            tempMovementState = MovementState.Forward;
            SetMovePaths(position, true);
        }

        protected void NetFuncUpdateYRotation(short yRotation)
        {
            if (!Entity.CanMove())
                return;
            if (!HasNavPaths)
            {
                this.yRotation = yRotation;
                UpdateRotation();
            }
        }

        public void StopMove()
        {
            navPaths = null;
            CacheRigidbody.velocity = new Vector3(0, CacheRigidbody.velocity.y, 0);
            if (IsOwnerClient && !IsServer)
                CallNetFunction(StopMove, FunctionReceivers.Server);
        }

        public void KeyMovement(Vector3 moveDirection, MovementState movementState)
        {
            if (!Entity.CanMove())
                return;

            switch (Entity.MovementSecure)
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
                    if (!isJumping && !applyingJumpForce)
                        isJumping = isGrounded && tempMovementState.HasFlag(MovementState.IsJump);
                    break;
            }
        }

        public void PointClickMovement(Vector3 position)
        {
            if (!Entity.CanMove())
                return;

            switch (Entity.MovementSecure)
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

        public void SetLookRotation(Quaternion rotation)
        {
            if (!Entity.CanMove())
                return;

            switch (Entity.MovementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    // Cast to short to reduce packet size
                    CallNetFunction(NetFuncUpdateYRotation, FunctionReceivers.Server, (short)rotation.eulerAngles.y);
                    break;
                case MovementSecure.NotSecure:
                    if (!HasNavPaths)
                        yRotation = rotation.eulerAngles.y;
                    break;
            }
        }

        public Quaternion GetLookRotation()
        {
            return Quaternion.Euler(0f, yRotation, 0f);
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            CacheNetTransform.Teleport(position, rotation);
        }

        public bool FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
        {
            result = fromPosition;
            float nearestDistance = float.MaxValue;
            bool foundGround = false;
            float tempDistance;
            int foundCount = physicFunctions.RaycastDown(fromPosition, GameInstance.Singleton.GetGameEntityGroundDetectionLayerMask(), findDistance, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < foundCount; ++i)
            {
                if (physicFunctions.GetRaycastTransform(i).root == CacheTransform.root)
                    continue;
                tempDistance = Vector3.Distance(fromPosition, physicFunctions.GetRaycastPoint(i));
                if (tempDistance < nearestDistance)
                {
                    result = physicFunctions.GetRaycastPoint(i);
                    nearestDistance = tempDistance;
                    foundGround = true;
                }
            }
            return foundGround;
        }

        public override void EntityUpdate()
        {
            base.EntityUpdate();
            float moveSpeed = Entity.GetMoveSpeed();
            CacheNetTransform.interpolateMode = interpolateMode;
            if (interpolateMode == LiteNetLibTransform.InterpolateMode.FixedSpeed)
                CacheNetTransform.fixedInterpolateSpeed = moveSpeed;
            CacheNetTransform.extrapolateMode = extrapolateMode;
            if (extrapolateMode == LiteNetLibTransform.ExtrapolateMode.FixedSpeed)
                CacheNetTransform.fixedExtrapolateSpeed = moveSpeed * extrapolateSpeedRate;
        }

        public override void EntityFixedUpdate()
        {
            if (Entity.MovementSecure == MovementSecure.ServerAuthoritative && !IsServer)
            {
                if (CacheRigidbody.useGravity)
                    CacheRigidbody.useGravity = false;
                return;
            }

            if (Entity.MovementSecure == MovementSecure.NotSecure && !IsOwnerClient)
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
            Entity.SetMovement(tempMovementState);
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
                    yRotation = Quaternion.LookRotation(tempMoveDirection).eulerAngles.y;
                }
            }

            // If move by WASD keys, set move direction to input direction
            if (tempInputDirection.sqrMagnitude > 0f)
            {
                tempMoveDirection = tempInputDirection;
                tempMoveDirection.Normalize();
            }

            if (!Entity.CanMove())
            {
                tempMoveDirection = Vector3.zero;
                isJumping = false;
                applyingJumpForce = false;
            }

            // Prepare movement speed
            tempEntityMoveSpeed = applyingJumpForce ? 0f : Entity.GetMoveSpeed();
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
                    Entity.CallAllPlayJumpAnimation();
                    applyingJumpForce = true;
                    applyJumpForceCountDown = 0f;
                    switch (applyJumpForceMode)
                    {
                        case ApplyJumpForceMode.ApplyAfterFixedDuration:
                            applyJumpForceCountDown = applyJumpForceFixedDuration;
                            break;
                        case ApplyJumpForceMode.ApplyAfterJumpDuration:
                            if (Entity.Model is IJumppableModel)
                                applyJumpForceCountDown = (Entity.Model as IJumppableModel).GetJumpAnimationDuration();
                            break;
                    }
                }

                if (applyingJumpForce)
                {
                    applyJumpForceCountDown -= Time.deltaTime;
                    if (applyJumpForceCountDown <= 0f)
                    {
                        applyingJumpForce = false;
                        if (!useRootMotionForJump)
                        {
                            CacheRigidbody.drag = 0f;
                            CacheRigidbody.velocity = new Vector3(CacheRigidbody.velocity.x, 0f, CacheRigidbody.velocity.z);
                            CacheRigidbody.AddForce(new Vector3(0f, CalculateJumpVerticalSpeed(), 0f), ForceMode.Impulse);
                        }
                        applyingJump = true;
                    }
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

            UpdateRotation();
            isJumping = false;
        }

        protected void UpdateRotation()
        {
            CacheTransform.eulerAngles = new Vector3(0f, yRotation, 0f);
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
