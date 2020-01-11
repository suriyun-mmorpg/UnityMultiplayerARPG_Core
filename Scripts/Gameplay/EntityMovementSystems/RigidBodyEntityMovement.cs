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
        public float backwardMoveSpeedRate = 0.75f;
        public float groundCheckDistance = 0.1f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
        public float groundCheckDistanceWhileJump = 0.01f; // distance for checking if the controller is grounded while jumping
        public float stickToGroundHelperDistance = 0.5f; // distance for checking if the controller is grounded while moving on slopes
        [Range(0.1f, 1f)]
        public float underWaterThreshold = 0.75f;
        public bool autoSwimToSurface;
        public bool useNavMeshForKeyMovement;

        [Header("Root Motion Settings")]
        public bool useRootMotionForMovement;
        public bool useRootMotionForAirMovement;
        public bool useRootMotionForJump;
        public bool useRootMotionForFall;
        
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
        private Vector3 tempTargetPosition;
        private Vector3 tempCurrentPosition;
        private Vector3 groundContactNormal;
        private float tempTargetDistance;
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

            if (!CacheEntity.MovementState.HasFlag(MovementState.Forward) &&
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

        public override void EntityOnSetup()
        {
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
            // Register Network functions
            CacheEntity.RegisterNetFunction(NetFuncTriggerJump);
            CacheEntity.RegisterNetFunction<Vector3>(NetFuncPointClickMovement);
            CacheEntity.RegisterNetFunction<DirectionVector3, byte>(NetFuncKeyMovement);
            CacheEntity.RegisterNetFunction<short>(NetFuncUpdateYRotation);
            CacheEntity.RegisterNetFunction(StopMove);
        }

        protected void NetFuncKeyMovement(DirectionVector3 inputDirection, byte movementState)
        {
            if (!CacheEntity.CanMove())
                return;
            // Devide inputs to float value
            tempInputDirection = inputDirection;
            tempMovementState = (MovementState)movementState;
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

        protected void NetFuncTriggerJump()
        {
            if (!CacheEntity.CanMove())
                return;
            // Not play jump animation on owner client when running in not secure mode
            if (CacheEntity.MovementSecure == MovementSecure.NotSecure && IsOwnerClient && !IsServer)
                return;
            // Play jump animation on non owner clients
            if (CacheEntity.Model && CacheEntity.Model is IJumppableModel)
                (CacheEntity.Model as IJumppableModel).PlayJumpAnimation();
        }

        public void RequestTriggerJump()
        {
            if (!CacheEntity.CanMove())
                return;
            // Play jump animation immediately on owner client, if not running in server
            if (IsOwnerClient && !IsServer && CacheEntity.Model && CacheEntity.Model is IJumppableModel)
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

            switch (CacheEntity.MovementSecure)
            {
                case MovementSecure.ServerAuthoritative:
                    // Multiply with 100 and cast to sbyte to reduce packet size
                    // then it will be devided with 100 later on server side
                    CacheEntity.CallNetFunction(NetFuncKeyMovement, FunctionReceivers.Server, new DirectionVector3(moveDirection), (byte)movementState);
                    break;
                case MovementSecure.NotSecure:
                    tempInputDirection = moveDirection;
                    tempMovementState = movementState;
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
                    CacheEntity.CallNetFunction(NetFuncPointClickMovement, FunctionReceivers.Server, position);
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

            tempMovementState = CacheRigidbody.velocity.magnitude > 0 ? tempMovementState : MovementState.None;
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

        private void UpdateMovement()
        {
            WaterCheck();
            GroundCheck();

            // If move by WASD keys, set move direction to input direction
            if (tempInputDirection.magnitude > 0f)
                tempMoveDirection = tempInputDirection;

            if (!CacheEntity.CanMove())
            {
                tempMoveDirection = Vector3.zero;
                isJumping = false;
            }

            if (isUnderWater)
            {
                // Move up to surface while under water
                float footToSurfaceDist = waterCollider.bounds.max.y - CacheCapsuleCollider.bounds.min.y;
                float currentThreshold = footToSurfaceDist / (CacheCapsuleCollider.bounds.max.y - CacheCapsuleCollider.bounds.min.y);
                if (autoSwimToSurface)
                {
                    if (currentThreshold > underWaterThreshold)
                        tempMoveDirection.y = 1f;
                    else
                        tempMoveDirection.y = 0f;
                }
                else
                {
                    if (tempMoveDirection.y > 0f && currentThreshold <= underWaterThreshold)
                        tempMoveDirection.y = 0f;
                }
            }

            if (tempMoveDirection.magnitude > 0f)
            {
                tempMoveDirection = tempMoveDirection.normalized;

                if (!isUnderWater)
                {
                    // always move along the camera forward as it is the direction that it being aimed at
                    tempMoveDirection = Vector3.ProjectOnPlane(tempMoveDirection, groundContactNormal).normalized;
                }

                float currentTargetSpeed = CacheEntity.GetMoveSpeed();
                // If character move backward
                if (Vector3.Angle(tempMoveDirection, CacheTransform.forward) > 120)
                    currentTargetSpeed *= backwardMoveSpeedRate;

                tempMoveDirection *= currentTargetSpeed;

                if (isUnderWater)
                {
                    // Update velocity while under water
                    CacheRigidbody.velocity = tempMoveDirection;
                }
                else
                {
                    // Update velocity while not under water
                    if (isGrounded && !useRootMotionForMovement)
                        CacheRigidbody.velocity = tempMoveDirection;
                    else if (!isGrounded && !useRootMotionForAirMovement)
                        CacheRigidbody.velocity = new Vector3(tempMoveDirection.x, CacheRigidbody.velocity.y, tempMoveDirection.z);
                }
            }
            else
            {
                if (isUnderWater)
                    CacheRigidbody.velocity = Vector3.zero;
                else
                    CacheRigidbody.velocity = new Vector3(0f, CacheRigidbody.velocity.y, 0f);
            }

            if (isUnderWater)
            {
                CacheRigidbody.drag = 10f;
            }
            else if (isGrounded)
            {
                CacheRigidbody.drag = 5f;

                if (isJumping)
                {
                    RequestTriggerJump();
                    if (!useRootMotionForJump)
                    {
                        CacheRigidbody.drag = 0f;
                        CacheRigidbody.velocity = new Vector3(CacheRigidbody.velocity.x, 0f, CacheRigidbody.velocity.z);
                        CacheRigidbody.AddForce(new Vector3(0f, CalculateJumpVerticalSpeed(), 0f), ForceMode.Impulse);
                    }
                    applyingJump = true;
                    isGrounded = false;
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
