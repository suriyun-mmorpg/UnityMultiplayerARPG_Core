using LiteNetLibManager;
using StandardAssets.Characters.Physics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(OpenCharacterController))]
    [RequireComponent(typeof(LiteNetLibTransform))]
    public class RigidBodyEntityMovement : BaseEntityMovement
    {
        // Buffer to avoid character fall underground when teleport
        public const float GROUND_BUFFER = 0.16f;

        [Header("Movement AI")]
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;

        [Header("Movement Settings")]
        public float jumpHeight = 2f;
        public float backwardMoveSpeedRate = 0.75f;
        public float gravity = 9.81f;
        public float maxFallVelocity = 40f;
        [Range(0.1f, 1f)]
        public float underWaterThreshold = 0.75f;
        public bool autoSwimToSurface;

        [Header("Root Motion Settings")]
        public bool useRootMotionForMovement;
        public bool useRootMotionForAirMovement;
        public bool useRootMotionForJump;
        public bool useRootMotionForFall;

        public Animator CacheAnimator { get; private set; }
        public LiteNetLibTransform CacheNetTransform { get; private set; }
        public Rigidbody CacheRigidbody { get; private set; }
        public CapsuleCollider CacheCapsuleCollider { get; private set; }
        public OpenCharacterController CacheOpenCharacterController { get; private set; }

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
        private Vector3 tempPredictPosition;
        private float tempVerticalVelocity;
        private float tempSqrMagnitude;
        private float tempPredictSqrMagnitude;
        private float tempTargetDistance;
        private CollisionFlags collisionFlags;

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
            // Prepare open character controller
            CacheOpenCharacterController = GetComponent<OpenCharacterController>();
            if (CacheOpenCharacterController == null)
            {
                CacheOpenCharacterController = gameObject.AddComponent<OpenCharacterController>();
                CacheOpenCharacterController.InitRadiusHeightAndCenter(CacheCapsuleCollider.radius, CacheCapsuleCollider.height, CacheCapsuleCollider.center);
            }
            // Setup
            StopMove();
        }

        public override void EntityStart()
        {
            tempCurrentPosition = CacheTransform.position;
            tempCurrentPosition.y += GROUND_BUFFER;
            CacheTransform.position = tempCurrentPosition;
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
            CacheNetTransform.onTeleport += OnTeleport;
        }

        public override void ComponentOnDisable()
        {
            CacheNetTransform.enabled = false;
            CacheNetTransform.onTeleport -= OnTeleport;
        }

        protected void OnTeleport(Vector3 position, Quaternion rotation)
        {
            CacheOpenCharacterController.SetPosition(position, true);
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
            // Register Network functions
            CacheEntity.RegisterNetFunction(NetFuncTriggerJump);
            CacheEntity.RegisterNetFunction<Vector3>(NetFuncPointClickMovement);
            CacheEntity.RegisterNetFunction<DirectionVector3, MovementState>(NetFuncKeyMovement);
            CacheEntity.RegisterNetFunction<short>(NetFuncUpdateYRotation);
            CacheEntity.RegisterNetFunction(StopMove);
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
                isJumping = CacheOpenCharacterController.isGrounded && tempMovementState.HasFlag(MovementState.IsJump);
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
            if (IsOwnerClient && !IsServer)
                CacheEntity.CallNetFunction(StopMove, FunctionReceivers.Server);
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
                    CacheEntity.CallNetFunction(NetFuncKeyMovement, FunctionReceivers.Server, new DirectionVector3(moveDirection), movementState);
                    break;
                case MovementSecure.NotSecure:
                    tempInputDirection = moveDirection;
                    tempMovementState = movementState;
                    if (tempInputDirection.sqrMagnitude > 0)
                        navPaths = null;
                    if (!isJumping)
                        isJumping = CacheOpenCharacterController.isGrounded && tempMovementState.HasFlag(MovementState.IsJump);
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
            CacheNetTransform.Teleport(position + Vector3.up * GROUND_BUFFER, Quaternion.Euler(0, CacheEntity.MovementTransform.eulerAngles.y, 0));
        }

        public override void FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
        {
            // TODO: implement this
            result = fromPosition;
        }

        public override void EntityUpdate()
        {
            if ((CacheEntity.MovementSecure == MovementSecure.ServerAuthoritative && !IsServer) ||
                (CacheEntity.MovementSecure == MovementSecure.NotSecure && !IsOwnerClient))
                return;

            tempMoveDirection = Vector3.zero;
            tempTargetDistance = -1f;

            if (HasNavPaths)
            {
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
            UpdateMovement(Time.deltaTime);

            tempMovementState = tempMoveDirection.sqrMagnitude > 0f ? tempMovementState : MovementState.None;
            if (isUnderWater)
                tempMovementState |= MovementState.IsUnderWater;
            if (CacheOpenCharacterController.isGrounded)
                tempMovementState |= MovementState.IsGrounded;
            CacheEntity.SetMovement(tempMovementState);
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

        private void UpdateMovement(float deltaTime)
        {
            tempMoveVelocity = Vector3.zero;
            WaterCheck();

            // If move by WASD keys, set move direction to input direction
            if (tempInputDirection.sqrMagnitude > 0f)
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

            // Calculate vertical velocity by gravity
            if (!CacheOpenCharacterController.isGrounded)
                tempVerticalVelocity = Mathf.MoveTowards(tempVerticalVelocity, -maxFallVelocity, gravity * deltaTime);
            else
                tempVerticalVelocity = 0f;

            // Jumping 
            if (CacheOpenCharacterController.isGrounded && !CacheOpenCharacterController.startedSlide && isJumping)
            {
                RequestTriggerJump();
                if (!useRootMotionForJump)
                    tempVerticalVelocity = CalculateJumpVerticalSpeed();
            }

            if (tempMoveDirection.sqrMagnitude > 0f)
            {
                tempMoveDirection.Normalize();

                float currentTargetSpeed = CacheEntity.GetMoveSpeed();
                // If character move backward
                if (Vector3.Angle(tempMoveDirection, CacheTransform.forward) > 120)
                    currentTargetSpeed *= backwardMoveSpeedRate;

                if (HasNavPaths)
                {
                    tempHorizontalMoveDirection = tempMoveDirection;
                    tempHorizontalMoveDirection.y = 0;
                    tempSqrMagnitude = (tempTargetPosition - tempCurrentPosition).sqrMagnitude;
                    tempPredictPosition = tempCurrentPosition + (tempHorizontalMoveDirection * currentTargetSpeed * deltaTime);
                    tempPredictSqrMagnitude = (tempPredictPosition - tempCurrentPosition).sqrMagnitude;
                    // Check `tempSqrMagnitude` against the `tempPredictSqrMagnitude`
                    // if `tempPredictSqrMagnitude` is greater than `tempSqrMagnitude`,
                    // rigidbody will reaching target and character is moving pass it,
                    // so adjust move speed by distance and time (with physic formula: v=s/t)
                    if (tempPredictSqrMagnitude >= tempSqrMagnitude)
                        currentTargetSpeed *= tempTargetDistance / deltaTime / currentTargetSpeed;
                    tempMoveVelocity = tempMoveDirection * currentTargetSpeed;
                }
                else
                {
                    // Move with wasd keys so it does not have to adjust speed
                    tempMoveVelocity = tempMoveDirection * currentTargetSpeed;
                }

                if (!isUnderWater)
                {
                    // Update velocity while not under water
                    if ((CacheOpenCharacterController.isGrounded && !useRootMotionForMovement) ||
                        (!CacheOpenCharacterController.isGrounded && !useRootMotionForAirMovement))
                        tempMoveVelocity.y = tempVerticalVelocity;
                }
            }
            else
            {
                // Update fall
                if (!isUnderWater)
                    tempMoveVelocity.y = tempVerticalVelocity;
            }

            collisionFlags = CacheOpenCharacterController.Move(tempMoveVelocity * deltaTime);
            if ((collisionFlags & CollisionFlags.CollidedAbove) == CollisionFlags.CollidedAbove)
            {
                // Hit something above, falling in next frame
                tempVerticalVelocity = 0f;
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
            return Mathf.Sqrt(2f * jumpHeight * gravity);
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

#if UNITY_EDITOR
        [ContextMenu("Applies Collider Settings To Controller")]
        public void AppliesColliderSettingsToController()
        {
            CapsuleCollider collider = GetComponent<CapsuleCollider>();
            if (collider == null)
                collider = gameObject.AddComponent<CapsuleCollider>();
            // Prepare open character controller
            OpenCharacterController controller = GetComponent<OpenCharacterController>();
            if (controller == null)
                controller = gameObject.AddComponent<OpenCharacterController>();
            controller.InitRadiusHeightAndCenter(collider.radius, collider.height, collider.center);
        }
#endif
    }
}
