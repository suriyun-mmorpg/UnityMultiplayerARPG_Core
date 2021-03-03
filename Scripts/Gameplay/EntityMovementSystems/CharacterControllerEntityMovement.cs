using LiteNetLib;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterControllerEntityMovement : BaseGameEntityComponent<BaseGameEntity>, IEntityMovementComponent
    {
        [Header("Movement AI")]
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;

        [Header("Movement Settings")]
        public float jumpHeight = 2f;
        public ApplyJumpForceMode applyJumpForceMode = ApplyJumpForceMode.ApplyImmediately;
        public float applyJumpForceFixedDuration;
        public float backwardMoveSpeedRate = 0.75f;
        public float gravity = 9.81f;
        public float maxFallVelocity = 40f;
        public float stickGroundForce = 9.6f;
        [Tooltip("Delay before character change from grounded state to airborne")]
        public float airborneDelay = 0.01f;
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
        public CharacterController CacheCharacterController { get; private set; }

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
        private float airborneElapsed;
        private bool isUnderWater;
        private bool isJumping;
        private bool applyingJumpForce;
        private float applyJumpForceCountDown;
        private Collider waterCollider;
        private float yRotation;
        private Vector3 platformMotion;
        private Transform groundedTransform;
        private Vector3 groundedLocalPosition;
        private Vector3 oldGroundedPosition;

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
        private float tempEntityMoveSpeed;
        private float tempCurrentMoveSpeed;
        private CollisionFlags collisionFlags;

        public override void EntityAwake()
        {
            physicFunctions = new PhysicFunctions(30);
            // Prepare animator component
            CacheAnimator = GetComponent<Animator>();
            // Prepare network transform component
            CacheNetTransform = gameObject.GetOrAddComponent<LiteNetLibTransform>();
            CacheNetTransform.onTeleport += OnTeleport;
            // Prepare character controller component
            CacheCharacterController = gameObject.GetOrAddComponent<CharacterController>();
            // Setup
            StopMove();
        }

        public override void EntityStart()
        {
            yRotation = CacheTransform.eulerAngles.y;
            tempCurrentPosition = CacheTransform.position;
            tempVerticalVelocity = 0;
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
            CacheCharacterController.enabled = true;
            tempVerticalVelocity = 0;
        }

        public override void ComponentOnDisable()
        {
            CacheNetTransform.enabled = false;
            CacheCharacterController.enabled = false;
        }

        public override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            CacheNetTransform.onTeleport -= OnTeleport;
        }

        protected void OnTeleport(Vector3 position, Quaternion rotation)
        {
            tempVerticalVelocity = 0;
            yRotation = rotation.eulerAngles.y;
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
                isJumping = CacheCharacterController.isGrounded && tempMovementState.HasFlag(MovementState.IsJump);
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
                    CallNetFunction(NetFuncKeyMovement, DeliveryMethod.Sequenced, FunctionReceivers.Server, new DirectionVector3(moveDirection), movementState);
                    break;
                case MovementSecure.NotSecure:
                    tempInputDirection = moveDirection;
                    tempMovementState = movementState;
                    if (tempInputDirection.sqrMagnitude > 0)
                        navPaths = null;
                    if (!isJumping && !applyingJumpForce)
                        isJumping = CacheCharacterController.isGrounded && tempMovementState.HasFlag(MovementState.IsJump);
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
                    CallNetFunction(NetFuncUpdateYRotation, DeliveryMethod.Sequenced, FunctionReceivers.Server, (short)rotation.eulerAngles.y);
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
            float moveSpeed = Entity.GetMoveSpeed();
            CacheNetTransform.interpolateMode = interpolateMode;
            if (interpolateMode == LiteNetLibTransform.InterpolateMode.FixedSpeed)
                CacheNetTransform.fixedInterpolateSpeed = moveSpeed;
            CacheNetTransform.extrapolateMode = extrapolateMode;
            if (extrapolateMode == LiteNetLibTransform.ExtrapolateMode.FixedSpeed)
                CacheNetTransform.fixedExtrapolateSpeed = moveSpeed * extrapolateSpeedRate;

            if ((Entity.MovementSecure == MovementSecure.ServerAuthoritative && !IsServer) ||
                (Entity.MovementSecure == MovementSecure.NotSecure && !IsOwnerClient))
                return;

            UpdateMovement(Time.deltaTime);
            tempMovementState = tempMoveDirection.sqrMagnitude > 0f ? tempMovementState : MovementState.None;
            if (isUnderWater)
                tempMovementState |= MovementState.IsUnderWater;
            if (CacheCharacterController.isGrounded || airborneElapsed < airborneDelay)
                tempMovementState |= MovementState.IsGrounded;
            Entity.SetMovement(tempMovementState);
        }

        private void WaterCheck()
        {
            if (waterCollider == null)
            {
                // Not in water
                isUnderWater = false;
                return;
            }
            float footToSurfaceDist = waterCollider.bounds.max.y - CacheCharacterController.bounds.min.y;
            float currentThreshold = footToSurfaceDist / (CacheCharacterController.bounds.max.y - CacheCharacterController.bounds.min.y);
            isUnderWater = currentThreshold >= underWaterThreshold;
        }

        private void UpdateMovement(float deltaTime)
        {
            tempMoveVelocity = Vector3.zero;
            tempMoveDirection = Vector3.zero;
            tempTargetDistance = -1f;
            WaterCheck();

            bool isGrounded = CacheCharacterController.isGrounded;

            // Update airborne elasped
            if (isGrounded)
                airborneElapsed = 0f;
            else
                airborneElapsed += deltaTime;

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

            // Calculate vertical velocity by gravity
            if (!isGrounded && !isUnderWater)
            {
                if (!useRootMotionForFall)
                    tempVerticalVelocity = Mathf.MoveTowards(tempVerticalVelocity, -maxFallVelocity, gravity * deltaTime);
                else
                    tempVerticalVelocity = 0f;
            }
            else
            {
                // Not falling set verical velocity to 0
                tempVerticalVelocity = 0f;
            }

            // Jumping 
            if (isGrounded && isJumping)
            {
                airborneElapsed = airborneDelay;
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
                    isGrounded = false;
                    applyingJumpForce = false;
                    if (!useRootMotionForJump)
                    {
                        tempVerticalVelocity = CalculateJumpVerticalSpeed();
                    }
                }
            }
            // Updating horizontal movement (WASD inputs)
            if (tempMoveDirection.sqrMagnitude > 0f)
            {
                // Calculate only horizontal move direction
                tempHorizontalMoveDirection = tempMoveDirection;
                tempHorizontalMoveDirection.y = 0;
                tempHorizontalMoveDirection.Normalize();

                // If character move backward
                if (Vector3.Angle(tempHorizontalMoveDirection, CacheTransform.forward) > 120)
                    tempCurrentMoveSpeed *= backwardMoveSpeedRate;

                if (HasNavPaths)
                {
                    // NOTE: `tempTargetPosition` and `tempCurrentPosition` were set above
                    tempSqrMagnitude = (tempTargetPosition - tempCurrentPosition).sqrMagnitude;
                    tempPredictPosition = tempCurrentPosition + (tempHorizontalMoveDirection * tempCurrentMoveSpeed * deltaTime);
                    tempPredictSqrMagnitude = (tempPredictPosition - tempCurrentPosition).sqrMagnitude;
                    // Check `tempSqrMagnitude` against the `tempPredictSqrMagnitude`
                    // if `tempPredictSqrMagnitude` is greater than `tempSqrMagnitude`,
                    // rigidbody will reaching target and character is moving pass it,
                    // so adjust move speed by distance and time (with physic formula: v=s/t)
                    if (tempPredictSqrMagnitude >= tempSqrMagnitude)
                        tempCurrentMoveSpeed *= tempTargetDistance / deltaTime / tempCurrentMoveSpeed;
                    tempMoveVelocity = tempHorizontalMoveDirection * tempCurrentMoveSpeed;
                }
                else
                {
                    // Move with wasd keys so it does not have to adjust speed
                    tempMoveVelocity = tempHorizontalMoveDirection * tempCurrentMoveSpeed;
                }
            }
            // Updating vertical movement (Fall, WASD inputs under water)
            if (isUnderWater)
            {
                tempCurrentMoveSpeed = tempEntityMoveSpeed;
                // Move up to surface while under water
                if (autoSwimToSurface || tempMoveDirection.y > 0)
                {
                    if (autoSwimToSurface)
                        tempMoveDirection.y = 1f;
                    tempTargetPosition = Vector3.up * (waterCollider.bounds.max.y - (CacheCharacterController.bounds.size.y * underWaterThreshold));
                    tempCurrentPosition = Vector3.up * CacheTransform.position.y;
                    tempTargetDistance = Vector3.Distance(tempTargetPosition, tempCurrentPosition);
                    tempSqrMagnitude = (tempTargetPosition - tempCurrentPosition).sqrMagnitude;
                    tempPredictPosition = tempCurrentPosition + (Vector3.up * tempMoveDirection.y * tempCurrentMoveSpeed * deltaTime);
                    tempPredictSqrMagnitude = (tempPredictPosition - tempCurrentPosition).sqrMagnitude;
                    // Check `tempSqrMagnitude` against the `tempPredictSqrMagnitude`
                    // if `tempPredictSqrMagnitude` is greater than `tempSqrMagnitude`,
                    // rigidbody will reaching target and character is moving pass it,
                    // so adjust move speed by distance and time (with physic formula: v=s/t)
                    if (tempPredictSqrMagnitude >= tempSqrMagnitude)
                        tempCurrentMoveSpeed *= tempTargetDistance / deltaTime / tempCurrentMoveSpeed;
                    // Swim up to surface
                    tempMoveVelocity.y = tempMoveDirection.y * tempCurrentMoveSpeed;
                }
                else
                {
                    // Dive down under water
                    tempMoveVelocity.y = tempMoveDirection.y * tempCurrentMoveSpeed;
                }
            }
            else
            {
                // Update velocity while not under water
                tempMoveVelocity.y = tempVerticalVelocity;
            }

            platformMotion = Vector3.zero;
            if (isGrounded && !isUnderWater)
            {
                // Apply platform motion
                if (groundedTransform != null && deltaTime > 0.0f)
                {
                    Vector3 newGroundedPosition = groundedTransform.TransformPoint(groundedLocalPosition);
                    platformMotion = (newGroundedPosition - oldGroundedPosition) / deltaTime;
                    oldGroundedPosition = newGroundedPosition;
                }
            }

            Vector3 stickGroundMove = isGrounded && !isUnderWater ? Vector3.down * stickGroundForce * Time.deltaTime : Vector3.zero;
            collisionFlags = CacheCharacterController.Move((tempMoveVelocity + platformMotion) * deltaTime + stickGroundMove);
            if ((collisionFlags & CollisionFlags.CollidedBelow) == CollisionFlags.CollidedBelow ||
                (collisionFlags & CollisionFlags.CollidedAbove) == CollisionFlags.CollidedAbove)
            {
                // Hit something below or above, falling in next frame
                tempVerticalVelocity = 0f;
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

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (CacheCharacterController.isGrounded)
            {
                RaycastHit raycastHit;
                if (Physics.SphereCast(transform.position + Vector3.up * 0.8f, 0.8f, Vector3.down, out raycastHit, 0.1f, -1, QueryTriggerInteraction.Ignore))
                {
                    groundedTransform = raycastHit.collider.transform;
                    oldGroundedPosition = raycastHit.point;
                    groundedLocalPosition = groundedTransform.InverseTransformPoint(oldGroundedPosition);
                }
            }
        }

        public void HandleSyncTransformAtClient(MessageHandlerData messageHandler)
        {
        }

        public void HandleTeleportAtClient(MessageHandlerData messageHandler)
        {
        }

        public void HandleKeyMovementAtServer(MessageHandlerData messageHandler)
        {
        }

        public void HandlePointClickMovementAtServer(MessageHandlerData messageHandler)
        {
        }

        public void HandleSetLookRotationAtServer(MessageHandlerData messageHandler)
        {
        }

        public void HandleSyncTransformAtServer(MessageHandlerData messageHandler)
        {
        }

        public void HandleTeleportAtServer(MessageHandlerData messageHandler)
        {
        }

        public void HandleStopMoveAtServer(MessageHandlerData messageHandler)
        {
        }
    }
}
