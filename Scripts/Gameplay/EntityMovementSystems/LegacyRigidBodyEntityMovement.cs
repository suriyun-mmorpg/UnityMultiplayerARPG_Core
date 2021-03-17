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

        [Header("Root Motion Settings")]
        public bool useRootMotionForMovement;
        public bool useRootMotionForAirMovement;
        public bool useRootMotionForJump;
        public bool useRootMotionForFall;
        public bool useRootMotionWhileNotMoving;

        [Header("Networking Settings")]
        public float moveThreshold = 0.01f;
        public float snapThreshold = 5.0f;
        [Range(0.00825f, 0.1f)]
        public float clientSyncTransformInterval = 0.05f;
        [Range(0.00825f, 0.1f)]
        public float clientSendInputsInterval = 0.05f;
        [Range(0.00825f, 0.1f)]
        public float serverSyncTransformInterval = 0.05f;

        public Animator CacheAnimator { get; private set; }
        public Rigidbody CacheRigidbody { get; private set; }
        public CapsuleCollider CacheCapsuleCollider { get; private set; }

        public float StoppingDistance
        {
            get { return stoppingDistance; }
        }

        public Queue<Vector3> navPaths { get; private set; }
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
        private long acceptedPositionTimestamp;
        private long acceptedJumpTimestamp;
        private Vector3 acceptedPosition;
        private Vector3? clientTargetPosition;
        private float? targetYRotation;
        private float yRotateLerpTime;
        private float yRotateLerpDuration;
        private bool acceptedJump;
        private bool sendingJump;
        private float lastServerSyncTransform;
        private float lastClientSyncTransform;
        private float lastClientSendInputs;

        private EntityMovementInput oldInput;
        private EntityMovementInput currentInput;

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
            // Prepare rigidbody component
            CacheRigidbody = gameObject.GetOrAddComponent<Rigidbody>();
            // Prepare collider component
            CacheCapsuleCollider = gameObject.GetOrAddComponent<CapsuleCollider>();
            // Disable unused component
            LiteNetLibTransform disablingComp = gameObject.GetComponent<LiteNetLibTransform>();
            if (disablingComp != null)
            {
                Logging.LogWarning("RigidBodyEntityMovement", "You can remove `LiteNetLibTransform` component from game entity, it's not being used anymore [" + name + "]");
                disablingComp.enabled = false;
            }
            // Setup
            CacheRigidbody.useGravity = false;
            StopMoveFunction();
        }

        public override void EntityStart()
        {
            yRotation = CacheTransform.eulerAngles.y;
        }

        public override void ComponentOnEnable()
        {
            CacheRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        public override void ComponentOnDisable()
        {
            CacheRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }

        private void OnAnimatorMove()
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

        public void StopMove()
        {
            if (Entity.MovementSecure == MovementSecure.ServerAuthoritative)
            {
                // Send movement input to server, then server will apply movement and sync transform to clients
                this.ClientSendStopMove();
            }
            StopMoveFunction();
        }

        private void StopMoveFunction()
        {
            navPaths = null;
            CacheRigidbody.velocity = new Vector3(0, CacheRigidbody.velocity.y, 0);
        }

        public void KeyMovement(Vector3 moveDirection, MovementState movementState)
        {
            if (!Entity.CanMove())
                return;
            if (this.CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                tempInputDirection = moveDirection;
                tempMovementState = movementState;
                if (tempInputDirection.sqrMagnitude > 0)
                    navPaths = null;
                if (!isJumping && !applyingJumpForce)
                    isJumping = isGrounded && tempMovementState.HasFlag(MovementState.IsJump);
            }
        }

        public void PointClickMovement(Vector3 position)
        {
            if (!Entity.CanMove())
                return;
            if (this.CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                tempMovementState = MovementState.Forward;
                SetMovePaths(position, true);
            }
        }

        public void SetLookRotation(Quaternion rotation)
        {
            if (!Entity.CanMove())
                return;
            if (this.CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                if (!HasNavPaths)
                    yRotation = rotation.eulerAngles.y;
            }
        }

        public Quaternion GetLookRotation()
        {
            return Quaternion.Euler(0f, yRotation, 0f);
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (!IsServer)
            {
                Logging.LogWarning("LegacyRigidBodyEntityMovement", "Teleport function shouldn't be called at client [" + name + "]");
                return;
            }
            this.ServerSendTeleport3D(position, rotation);
            yRotation = rotation.eulerAngles.y;
            CacheTransform.position = position;
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

        public override void EntityFixedUpdate()
        {
            // Turn Use Gravity when this is allowed to update
            if (!useRootMotionForFall && CacheRigidbody.useGravity != !isUnderWater)
                CacheRigidbody.useGravity = !isUnderWater;
            if (useRootMotionForFall && CacheRigidbody.useGravity)
                CacheRigidbody.useGravity = false;

            UpdateMovement(Time.deltaTime);
            if (IsOwnerClient || (IsServer && Entity.MovementSecure == MovementSecure.ServerAuthoritative))
            {
                tempMovementState = tempMoveDirection.sqrMagnitude > 0f ? tempMovementState : MovementState.None;
                if (isUnderWater)
                    tempMovementState |= MovementState.IsUnderWater;
                if (isGrounded)
                    tempMovementState |= MovementState.IsGrounded;
                Entity.SetMovement(tempMovementState);
            }
            SyncTransform();
        }

        private void SyncTransform()
        {
            float currentTime = Time.fixedTime;
            if (Entity.MovementSecure == MovementSecure.NotSecure && IsOwnerClient && !IsServer)
            {
                // Sync transform from owner client to server (except it's both owner client and server)
                if (currentTime - lastClientSyncTransform > clientSyncTransformInterval)
                {
                    this.ClientSendSyncTransform3D();
                    if (sendingJump)
                    {
                        this.ClientSendJump();
                        sendingJump = false;
                    }
                    lastClientSyncTransform = currentTime;
                }
            }
            if (Entity.MovementSecure == MovementSecure.ServerAuthoritative && IsOwnerClient && !IsServer)
            {
                InputState inputState;
                if (currentTime - lastClientSendInputs > clientSendInputsInterval && this.DifferInputEnoughToSend(oldInput, currentInput, out inputState))
                {
                    this.ClientSendMovementInput3D(inputState, currentInput.MovementState, currentInput.Position, currentInput.Rotation);
                    oldInput = currentInput;
                    currentInput = null;
                    lastClientSendInputs = currentTime;
                }
            }
            if (IsServer)
            {
                // Sync transform from server to all clients (include owner client)
                if (currentTime - lastServerSyncTransform > serverSyncTransformInterval)
                {
                    this.ServerSendSyncTransform3D();
                    if (sendingJump)
                    {
                        this.ServerSendJump();
                        sendingJump = false;
                    }
                    lastServerSyncTransform = currentTime;
                }
            }
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
            tempCurrentPosition = CacheTransform.position;
            tempMoveDirection = Vector3.zero;
            tempTargetDistance = -1f;

            if (HasNavPaths)
            {
                // Set `tempTargetPosition` and `tempCurrentPosition`
                tempTargetPosition = navPaths.Peek();
                tempMoveDirection = (tempTargetPosition - tempCurrentPosition).normalized;
                tempTargetDistance = Vector3.Distance(tempTargetPosition.GetXZ(), tempCurrentPosition.GetXZ());
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
                    targetYRotation = null;
                }
            }
            else if (clientTargetPosition.HasValue)
            {
                tempTargetPosition = clientTargetPosition.Value;
                tempMoveDirection = (tempTargetPosition - tempCurrentPosition).normalized;
                tempTargetDistance = Vector3.Distance(tempTargetPosition.GetXZ(), tempCurrentPosition.GetXZ());
                if (tempTargetDistance < StoppingDistance)
                {
                    clientTargetPosition = null;
                    StopMove();
                    tempMoveDirection = Vector3.zero;
                }
            }
            else if (tempInputDirection.sqrMagnitude > 0f)
            {
                tempMoveDirection = tempInputDirection.normalized;
                tempTargetPosition = tempCurrentPosition + tempMoveDirection;
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
                // Set inputs
                currentInput = this.SetInputMovementState(currentInput, tempMovementState);
                if (HasNavPaths)
                    currentInput = this.SetInputPosition(currentInput, tempTargetPosition);
                else
                    currentInput = this.SetInputPosition(currentInput, tempPredictPosition);

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
                if (autoSwimToSurface || Mathf.Abs(tempMoveDirection.y) > 0)
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
                    if (!HasNavPaths)
                        currentInput = this.SetInputYPosition(currentInput, tempPredictPosition.y);
                }
            }
            else if (acceptedJump || isGrounded)
            {
                CacheRigidbody.drag = 5f;

                if (acceptedJump || isJumping)
                {
                    currentInput = this.SetInputJump(currentInput);
                    sendingJump = true;
                    Entity.PlayJumpAnimation();
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

            if (targetYRotation.HasValue)
            {
                yRotateLerpTime += deltaTime;
                float lerpTimeRate = yRotateLerpTime / yRotateLerpDuration;
                yRotation = Mathf.LerpAngle(yRotation, targetYRotation.Value, lerpTimeRate);
                if (lerpTimeRate >= 1f)
                    targetYRotation = null;
            }
            UpdateRotation();
            currentInput = this.SetInputRotation(currentInput, CacheTransform.rotation);
            isJumping = false;
            acceptedJump = false;
        }

        private void UpdateRotation()
        {
            CacheTransform.eulerAngles = new Vector3(0f, yRotation, 0f);
        }

        private void SetMovePaths(Vector3 position, bool useNavMesh)
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

        public void HandleSyncTransformAtClient(MessageHandlerData messageHandler)
        {
            if (IsServer)
            {
                // Don't read and apply transform, because it was done at server
                return;
            }
            Vector3 position;
            float yAngle;
            long timestamp;
            messageHandler.Reader.ReadSyncTransformMessage3D(out position, out yAngle, out timestamp);
            if (acceptedPositionTimestamp < timestamp)
            {
                acceptedPositionTimestamp = timestamp;
                // Snap character to the position if character is too far from the position
                if (Vector3.Distance(position, CacheTransform.position) >= snapThreshold)
                {
                    yRotation = yAngle;
                    CacheTransform.position = position;
                }
                else if (!IsOwnerClient)
                {
                    targetYRotation = yAngle;
                    yRotateLerpTime = 0;
                    yRotateLerpDuration = serverSyncTransformInterval;
                    if (Vector3.Distance(position.GetXZ(), acceptedPosition.GetXZ()) > moveThreshold)
                    {
                        acceptedPosition = position;
                        clientTargetPosition = position;
                    }
                }
            }
        }

        public void HandleTeleportAtClient(MessageHandlerData messageHandler)
        {
            if (IsServer)
            {
                // Don't read and apply transform, because it was done (this is both owner client and server)
                return;
            }
            Vector3 position;
            float yAngle;
            long timestamp;
            messageHandler.Reader.ReadTeleportMessage3D(out position, out yAngle, out timestamp);
            if (acceptedPositionTimestamp < timestamp)
            {
                acceptedPositionTimestamp = timestamp;
                yRotation = yAngle;
                CacheTransform.position = position;
            }
        }

        public void HandleJumpAtClient(MessageHandlerData messageHandler)
        {
            if (IsOwnerClient || IsServer)
            {
                // Don't read and apply transform, because it was done
                return;
            }
            long timestamp;
            messageHandler.Reader.ReadJumpMessage(out timestamp);
            if (acceptedJumpTimestamp < timestamp)
            {
                acceptedJumpTimestamp = timestamp;
                acceptedJump = true;
            }
        }

        public void HandleMovementInputAtServer(MessageHandlerData messageHandler)
        {
            if (IsOwnerClient)
            {
                // Don't read and apply inputs, because it was done (this is both owner client and server)
                return;
            }
            if (Entity.MovementSecure == MovementSecure.NotSecure)
            {
                // Movement handling at client, so don't read movement inputs from client (but have to read transform)
                return;
            }
            if (!Entity.CanMove())
                return;
            InputState inputState;
            MovementState movementState;
            Vector3 position;
            float yAngle;
            long timestamp;
            messageHandler.Reader.ReadMovementInputMessage3D(out inputState, out movementState, out position, out yAngle, out timestamp);
            if (acceptedPositionTimestamp < timestamp)
            {
                acceptedPositionTimestamp = timestamp;
                navPaths = null;
                tempMovementState = movementState;
                clientTargetPosition = null;
                if (inputState.HasFlag(InputState.PositionChanged))
                {
                    if (inputState.HasFlag(InputState.IsKeyMovement))
                    {
                        clientTargetPosition = position;
                    }
                    else
                    {
                        SetMovePaths(position, true);
                    }
                }
                if (inputState.HasFlag(InputState.RotationChanged))
                {
                    if (IsClient)
                    {
                        targetYRotation = yAngle;
                        yRotateLerpTime = 0;
                        yRotateLerpDuration = clientSendInputsInterval;
                    }
                    else
                    {
                        yRotation = yAngle;
                    }
                }
                acceptedJump = inputState.HasFlag(InputState.IsJump);
            }
        }

        public void HandleSyncTransformAtServer(MessageHandlerData messageHandler)
        {
            if (IsOwnerClient)
            {
                // Don't read and apply transform, because it was done (this is both owner client and server)
                return;
            }
            if (Entity.MovementSecure == MovementSecure.ServerAuthoritative)
            {
                // Movement handling at server, so don't read sync transform from client
                return;
            }
            Vector3 position;
            float yAngle;
            long timestamp;
            messageHandler.Reader.ReadSyncTransformMessage3D(out position, out yAngle, out timestamp);
            if (acceptedPositionTimestamp < timestamp)
            {
                acceptedPositionTimestamp = timestamp;
                yRotation = yAngle;
                if (Vector3.Distance(position.GetXZ(), acceptedPosition.GetXZ()) > moveThreshold)
                {
                    acceptedPosition = position;
                    if (!IsClient)
                    {
                        // If it's server only (not a host), set position follows the client immediately
                        CacheTransform.position = position;
                    }
                    else
                    {
                        // It's both server and client, translate position
                        SetMovePaths(position, false);
                    }
                }
            }
        }

        public void HandleStopMoveAtServer(MessageHandlerData messageHandler)
        {
            if (IsOwnerClient)
            {
                // Don't read and apply inputs, because it was done (this is both owner client and server)
                return;
            }
            if (Entity.MovementSecure == MovementSecure.NotSecure)
            {
                // Movement handling at client, so don't read movement inputs from client (but have to read transform)
                return;
            }
            long timestamp;
            messageHandler.Reader.ReadStopMoveMessage(out timestamp);
            if (acceptedPositionTimestamp < timestamp)
            {
                acceptedPositionTimestamp = timestamp;
                StopMoveFunction();
            }
        }

        public void HandleJumpAtServer(MessageHandlerData messageHandler)
        {
            if (IsOwnerClient)
            {
                // Don't read and apply transform, because it was done (this is both owner client and server)
                return;
            }
            if (Entity.MovementSecure == MovementSecure.ServerAuthoritative)
            {
                // Movement handling at server, so don't read sync transform from client
                return;
            }
            long timestamp;
            messageHandler.Reader.ReadJumpMessage(out timestamp);
            if (acceptedJumpTimestamp < timestamp)
            {
                acceptedJumpTimestamp = timestamp;
                acceptedJump = true;
            }
        }
    }
}
