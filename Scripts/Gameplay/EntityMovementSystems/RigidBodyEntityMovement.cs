using LiteNetLib.Utils;
using LiteNetLibManager;
using StandardAssets.Characters.Physics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(OpenCharacterController))]
    public class RigidBodyEntityMovement : BaseNetworkedGameEntityComponent<BaseGameEntity>, IEntityMovementComponent
    {
        protected static readonly RaycastHit[] s_findGroundRaycastHits = new RaycastHit[4];
        protected static readonly long s_lagBuffer = System.TimeSpan.TicksPerMillisecond * 200;
        protected static readonly float s_lagBufferUnityTime = 0.2f;

        [Header("Movement AI")]
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;
        public MovementSecure movementSecure = MovementSecure.NotSecure;

        [Header("Movement Settings")]
        public float jumpHeight = 2f;
        public ApplyJumpForceMode applyJumpForceMode = ApplyJumpForceMode.ApplyImmediately;
        public float applyJumpForceFixedDuration;
        public float backwardMoveSpeedRate = 0.75f;
        public float gravity = 9.81f;
        public float maxFallVelocity = 40f;
        public LayerMask platformLayerMask = 1;
        [Tooltip("Delay before character change from grounded state to airborne")]
        public float airborneDelay = 0.01f;
        public bool doNotChangeVelocityWhileAirborne;
        public float landedPauseMovementDuration = 0f;
        public float beforeCrawlingPauseMovementDuration = 0f;
        public float afterCrawlingPauseMovementDuration = 0f;
        [Range(0.1f, 1f)]
        public float underWaterThreshold = 0.75f;
        public bool autoSwimToSurface;

        [Header("Root Motion Settings")]
        public bool useRootMotionForMovement;
        public bool useRootMotionForAirMovement;
        public bool useRootMotionForJump;
        public bool useRootMotionForFall;
        public bool useRootMotionWhileNotMoving;
        public bool useRootMotionUnderWater;

        [Header("Networking Settings")]
        public float snapThreshold = 5.0f;

        public Animator CacheAnimator { get; private set; }
        public Rigidbody CacheRigidbody { get; private set; }
        public CapsuleCollider CacheCapsuleCollider { get; private set; }
        public OpenCharacterController CacheOpenCharacterController { get; private set; }
        public float StoppingDistance
        {
            get { return stoppingDistance; }
        }
        public MovementState MovementState { get; protected set; }
        public ExtraMovementState ExtraMovementState { get; protected set; }
        public DirectionVector2 Direction2D { get { return Vector2.down; } set { } }
        public float CurrentMoveSpeed { get; private set; }

        public Queue<Vector3> NavPaths { get; private set; }
        public bool HasNavPaths
        {
            get { return NavPaths != null && NavPaths.Count > 0; }
        }

        // Movement codes
        protected float _airborneElapsed;
        protected bool _isUnderWater;
        protected bool _isJumping;
        protected bool _applyingJumpForce;
        protected float _applyJumpForceCountDown;
        protected Collider _waterCollider;
        protected Transform _groundedTransform;
        protected Vector3 _previousPlatformPosition;
        protected long _acceptedPositionTimestamp;
        protected Vector3? _clientTargetPosition;
        protected float _yAngle;
        protected float _targetYAngle;
        protected float _yTurnSpeed;
        protected bool _lookRotationApplied;
        protected bool _acceptedJump;
        protected bool _sendingJump;
        protected float _lastServerValidateTransformTime;
        protected float _lastServerValidateTransformMoveSpeed;
        protected EntityMovementInput _oldInput;
        protected EntityMovementInput _currentInput;
        protected MovementState _tempMovementState;
        protected ExtraMovementState _tempExtraMovementState;
        protected Vector3 _inputDirection;
        protected Vector3 _moveDirection;
        protected float _verticalVelocity;
        protected float? _lagMoveSpeedRate;
        protected Vector3 _velocityBeforeAirborne;
        protected CollisionFlags _collisionFlags;
        protected float _pauseMovementCountDown;
        protected bool _previouslyGrounded;
        protected bool _previouslyAirborne;
        protected ExtraMovementState _previouslyExtraMovementState;
        protected bool _isTeleporting;
        protected bool _isServerWaitingTeleportConfirm;
        protected bool _isClientConfirmingTeleport;

        public override void EntityAwake()
        {
            // Prepare animator component
            CacheAnimator = GetComponent<Animator>();
            // Prepare rigidbody component
            CacheRigidbody = gameObject.GetOrAddComponent<Rigidbody>();
            // Prepare collider component
            CacheCapsuleCollider = gameObject.GetOrAddComponent<CapsuleCollider>();
            // Prepare open character controller
            float radius = CacheCapsuleCollider.radius;
            float height = CacheCapsuleCollider.height;
            Vector3 center = CacheCapsuleCollider.center;
            CacheOpenCharacterController = gameObject.GetOrAddComponent<OpenCharacterController>((comp) =>
            {
                comp.SetRadiusHeightAndCenter(radius, height, center, true, true);
            });
            CacheOpenCharacterController.collision += OnCharacterControllerCollision;
            // Disable unused component
            LiteNetLibTransform disablingComp = gameObject.GetComponent<LiteNetLibTransform>();
            if (disablingComp != null)
            {
                Logging.LogWarning("RigidBodyEntityMovement", "You can remove `LiteNetLibTransform` component from game entity, it's not being used anymore [" + name + "]");
                disablingComp.enabled = false;
            }
            // Setup
            _yAngle = _targetYAngle = CacheTransform.eulerAngles.y;
            _lookRotationApplied = true;
            StopMoveFunction();
        }

        public override void EntityStart()
        {
            _yAngle = CacheTransform.eulerAngles.y;
            CacheOpenCharacterController.SetPosition(CacheTransform.position, true);
            _verticalVelocity = 0;
        }

        public override void ComponentOnEnable()
        {
            CacheOpenCharacterController.enabled = true;
            CacheOpenCharacterController.SetPosition(CacheTransform.position, true);
            _verticalVelocity = 0;
        }

        public override void ComponentOnDisable()
        {
            CacheOpenCharacterController.enabled = false;
        }

        public override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            CacheOpenCharacterController.collision -= OnCharacterControllerCollision;
        }

        public override void OnSetOwnerClient(bool isOwnerClient)
        {
            base.OnSetOwnerClient(isOwnerClient);
            _clientTargetPosition = null;
            NavPaths = null;
        }

        private void OnAnimatorMove()
        {
            if (!CacheAnimator)
                return;

            if (useRootMotionWhileNotMoving &&
                !MovementState.Has(MovementState.Forward) &&
                !MovementState.Has(MovementState.Backward) &&
                !MovementState.Has(MovementState.Left) &&
                !MovementState.Has(MovementState.Right) &&
                !MovementState.Has(MovementState.IsJump))
            {
                // No movement, apply root motion position / rotation
                CacheAnimator.ApplyBuiltinRootMotion();
                return;
            }

            if (MovementState.Has(MovementState.IsGrounded) && useRootMotionForMovement)
                CacheAnimator.ApplyBuiltinRootMotion();
            if (!MovementState.Has(MovementState.IsGrounded) && useRootMotionForAirMovement)
                CacheAnimator.ApplyBuiltinRootMotion();
            if (MovementState.Has(MovementState.IsUnderWater) && useRootMotionUnderWater)
                CacheAnimator.ApplyBuiltinRootMotion();
        }

        public void StopMove()
        {
            if (movementSecure == MovementSecure.ServerAuthoritative)
            {
                // Send movement input to server, then server will apply movement and sync transform to clients
                this.SetInputStop(_currentInput);
            }
            StopMoveFunction();
        }

        private void StopMoveFunction()
        {
            NavPaths = null;
        }

        public void KeyMovement(Vector3 moveDirection, MovementState movementState)
        {
            if (!Entity.CanMove())
                return;
            if (CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                _inputDirection = moveDirection;
                _tempMovementState = movementState;
                if (_inputDirection.sqrMagnitude > 0)
                    NavPaths = null;
                if (!_isJumping && !_applyingJumpForce)
                    _isJumping = CacheOpenCharacterController.isGrounded && _tempMovementState.Has(MovementState.IsJump);
            }
        }

        public void PointClickMovement(Vector3 position)
        {
            if (!Entity.CanMove())
                return;
            if (CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                SetMovePaths(position, true);
            }
        }

        public void SetExtraMovementState(ExtraMovementState extraMovementState)
        {
            if (!Entity.CanMove())
                return;
            if (CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                _tempExtraMovementState = extraMovementState;
            }
        }

        public void SetLookRotation(Quaternion rotation)
        {
            if (!Entity.CanMove() || !Entity.CanTurn())
                return;
            if (CanPredictMovement())
            {
                // Always apply movement to owner client (it's client prediction for server auth movement)
                _targetYAngle = rotation.eulerAngles.y;
                _lookRotationApplied = false;
            }
        }

        public Quaternion GetLookRotation()
        {
            return Quaternion.Euler(0f, _yAngle, 0f);
        }

        public void SetSmoothTurnSpeed(float turnDuration)
        {
            _yTurnSpeed = turnDuration;
        }

        public float GetSmoothTurnSpeed()
        {
            return _yTurnSpeed;
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (!IsServer)
            {
                Logging.LogWarning("RigidBodyEntityMovement", "Teleport function shouldn't be called at client [" + name + "]");
                return;
            }
            _isTeleporting = true;
            OnTeleport(position, rotation.eulerAngles.y);
        }

        public bool FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
        {
            return PhysicUtils.FindGroundedPosition(fromPosition, s_findGroundRaycastHits, findDistance, GameInstance.Singleton.GetGameEntityGroundDetectionLayerMask(), out result, CacheTransform);
        }

        public override void EntityUpdate()
        {
            UpdateMovement(Time.deltaTime);
            if (CanPredictMovement())
            {
                // Update movement state
                _tempMovementState = _moveDirection.sqrMagnitude > 0f ? _tempMovementState : MovementState.None;
                if (_isUnderWater)
                    _tempMovementState |= MovementState.IsUnderWater;
                if (CacheOpenCharacterController.isGrounded || _airborneElapsed < airborneDelay)
                    _tempMovementState |= MovementState.IsGrounded;
                // Update movement state
                MovementState = _tempMovementState;
                // Update extra movement state
                ExtraMovementState = this.ValidateExtraMovementState(MovementState, _tempExtraMovementState);
            }
            else
            {
                // Update movement state
                if (HasNavPaths && !MovementState.Has(MovementState.Forward))
                    MovementState |= MovementState.Forward;
            }
        }

        private void WaterCheck()
        {
            if (_waterCollider == null)
            {
                // Not in water
                _isUnderWater = false;
                return;
            }
            float footToSurfaceDist = _waterCollider.bounds.max.y - CacheCapsuleCollider.bounds.min.y;
            float currentThreshold = footToSurfaceDist / (CacheCapsuleCollider.bounds.max.y - CacheCapsuleCollider.bounds.min.y);
            _isUnderWater = currentThreshold >= underWaterThreshold;
        }

        private void UpdateMovement(float deltaTime)
        {
            float tempSqrMagnitude;
            float tempPredictSqrMagnitude;
            float tempTargetDistance;
            float tempEntityMoveSpeed;
            float tempMaxMoveSpeed;
            Vector3 tempHorizontalMoveDirection;
            Vector3 tempMoveVelocity;
            Vector3 tempCurrentPosition;
            Vector3 tempTargetPosition;
            Vector3 tempPredictPosition;

            tempCurrentPosition = CacheTransform.position;
            tempMoveVelocity = Vector3.zero;
            _moveDirection = Vector3.zero;
            tempTargetDistance = 0f;
            WaterCheck();

            bool isGrounded = CacheOpenCharacterController.isGrounded;
            bool isAirborne = !isGrounded && !_isUnderWater && _airborneElapsed >= airborneDelay;

            // Update airborne elasped
            if (CacheOpenCharacterController.isGrounded)
                _airborneElapsed = 0f;
            else
                _airborneElapsed += deltaTime;

            if (HasNavPaths)
            {
                // Set `tempTargetPosition` and `tempCurrentPosition`
                tempTargetPosition = NavPaths.Peek();
                _moveDirection = (tempTargetPosition - tempCurrentPosition).normalized;
                tempTargetDistance = Vector3.Distance(tempTargetPosition.GetXZ(), tempCurrentPosition.GetXZ());
                if (!_tempMovementState.Has(MovementState.Forward))
                    _tempMovementState |= MovementState.Forward;
                if (tempTargetDistance < StoppingDistance)
                {
                    NavPaths.Dequeue();
                    if (!HasNavPaths)
                    {
                        StopMoveFunction();
                        _moveDirection = Vector3.zero;
                    }
                }
            }
            else if (_clientTargetPosition.HasValue)
            {
                tempTargetPosition = _clientTargetPosition.Value;
                _moveDirection = (tempTargetPosition - tempCurrentPosition).normalized;
                tempTargetDistance = Vector3.Distance(tempTargetPosition, tempCurrentPosition);
                if (tempTargetDistance < 0.01f)
                {
                    _clientTargetPosition = null;
                    StopMoveFunction();
                    _moveDirection = Vector3.zero;
                }
            }
            else if (_inputDirection.sqrMagnitude > 0f)
            {
                _moveDirection = _inputDirection.normalized;
                tempTargetPosition = tempCurrentPosition + _moveDirection;
            }
            else
            {
                tempTargetPosition = tempCurrentPosition;
            }
            if (IsOwnerClientOrOwnedByServer && _lookRotationApplied && _moveDirection.sqrMagnitude > 0f)
            {
                // Turn character by move direction
                if (Entity.CanTurn())
                    _targetYAngle = Quaternion.LookRotation(_moveDirection).eulerAngles.y;
            }

            if (!Entity.CanMove())
            {
                _moveDirection = Vector3.zero;
                _isJumping = false;
                _applyingJumpForce = false;
            }

            if (!Entity.CanJump())
                _isJumping = false;

            // Prepare movement speed
            tempEntityMoveSpeed = _applyingJumpForce ? 0f : Entity.GetMoveSpeed();
            tempMaxMoveSpeed = tempEntityMoveSpeed;

            // Calculate vertical velocity by gravity
            if (!isGrounded && !_isUnderWater)
            {
                if (!useRootMotionForFall)
                    _verticalVelocity = Mathf.MoveTowards(_verticalVelocity, -maxFallVelocity, gravity * deltaTime);
                else
                    _verticalVelocity = 0f;
            }
            else
            {
                // Not falling set verical velocity to 0
                _verticalVelocity = 0f;
            }

            // Jumping 
            if (_acceptedJump || (_pauseMovementCountDown <= 0f && isGrounded && _isJumping && !CacheOpenCharacterController.startedSlide))
            {
                _sendingJump = true;
                _airborneElapsed = airborneDelay;
                Entity.PlayJumpAnimation();
                _applyingJumpForce = true;
                _applyJumpForceCountDown = 0f;
                switch (applyJumpForceMode)
                {
                    case ApplyJumpForceMode.ApplyAfterFixedDuration:
                        _applyJumpForceCountDown = applyJumpForceFixedDuration;
                        break;
                    case ApplyJumpForceMode.ApplyAfterJumpDuration:
                        if (Entity.Model is IJumppableModel jumppableModel)
                            _applyJumpForceCountDown = jumppableModel.GetJumpAnimationDuration();
                        break;
                }
            }

            if (_applyingJumpForce)
            {
                _applyJumpForceCountDown -= Time.deltaTime;
                if (_applyJumpForceCountDown <= 0f)
                {
                    _applyingJumpForce = false;
                    if (!useRootMotionForJump)
                        _verticalVelocity = CalculateJumpVerticalSpeed();
                }
            }

            // Updating horizontal movement (WASD inputs)
            if (!isAirborne)
            {
                _velocityBeforeAirborne = Vector3.zero;
            }
            if (_pauseMovementCountDown <= 0f && _moveDirection.sqrMagnitude > 0f && (!isAirborne || !doNotChangeVelocityWhileAirborne || !IsOwnerClientOrOwnedByServer))
            {
                // Calculate only horizontal move direction
                tempHorizontalMoveDirection = _moveDirection;
                tempHorizontalMoveDirection.y = 0;
                tempHorizontalMoveDirection.Normalize();

                // If character move backward
                if (Vector3.Angle(tempHorizontalMoveDirection, CacheTransform.forward) > 120)
                    tempMaxMoveSpeed *= backwardMoveSpeedRate;
                CurrentMoveSpeed = CalculateCurrentMoveSpeed(tempMaxMoveSpeed, deltaTime);

                // NOTE: `tempTargetPosition` and `tempCurrentPosition` were set above
                tempSqrMagnitude = (tempTargetPosition - tempCurrentPosition).sqrMagnitude;
                tempPredictPosition = tempCurrentPosition + (tempHorizontalMoveDirection * CurrentMoveSpeed * deltaTime);
                tempPredictSqrMagnitude = (tempPredictPosition - tempCurrentPosition).sqrMagnitude;
                if (HasNavPaths || _clientTargetPosition.HasValue)
                {
                    // Check `tempSqrMagnitude` against the `tempPredictSqrMagnitude`
                    // if `tempPredictSqrMagnitude` is greater than `tempSqrMagnitude`,
                    // rigidbody will reaching target and character is moving pass it,
                    // so adjust move speed by distance and time (with physic formula: v=s/t)
                    if (tempPredictSqrMagnitude >= tempSqrMagnitude && tempTargetDistance > 0f)
                        CurrentMoveSpeed *= tempTargetDistance / deltaTime / CurrentMoveSpeed;
                }
                tempMoveVelocity = tempHorizontalMoveDirection * CurrentMoveSpeed;
                _velocityBeforeAirborne = tempMoveVelocity;
                // Set inputs
                _currentInput = this.SetInputMovementState(_currentInput, _tempMovementState);
                if (HasNavPaths)
                {
                    _currentInput = this.SetInputPosition(_currentInput, tempTargetPosition);
                    _currentInput = this.SetInputIsKeyMovement(_currentInput, false);
                }
                else
                {
                    _currentInput = this.SetInputPosition(_currentInput, tempPredictPosition);
                    _currentInput = this.SetInputIsKeyMovement(_currentInput, true);
                }
            }
            if (IsOwnerClientOrOwnedByServer)
            {
                if (isGrounded && _previouslyAirborne)
                {
                    _pauseMovementCountDown = landedPauseMovementDuration;
                }
                else if (isGrounded && _previouslyExtraMovementState != ExtraMovementState.IsCrawling && _tempExtraMovementState == ExtraMovementState.IsCrawling)
                {
                    _pauseMovementCountDown = beforeCrawlingPauseMovementDuration;
                }
                else if (isGrounded && _previouslyExtraMovementState == ExtraMovementState.IsCrawling && _tempExtraMovementState != ExtraMovementState.IsCrawling)
                {
                    _pauseMovementCountDown = afterCrawlingPauseMovementDuration;
                }
                else if (isAirborne && doNotChangeVelocityWhileAirborne)
                {
                    tempMoveVelocity = _velocityBeforeAirborne;
                }
                else
                {
                    if (_pauseMovementCountDown > 0f)
                        _pauseMovementCountDown -= deltaTime;
                }
                if (_pauseMovementCountDown > 0f)
                {
                    // Remove movement from movestate while pausing movement
                    _tempMovementState ^= MovementState.Forward | MovementState.Backward | MovementState.Right | MovementState.Right;
                }
            }
            // Updating vertical movement (Fall, WASD inputs under water)
            if (_isUnderWater)
            {
                CurrentMoveSpeed = CalculateCurrentMoveSpeed(tempMaxMoveSpeed, deltaTime);

                // Move up to surface while under water
                if (autoSwimToSurface || Mathf.Abs(_moveDirection.y) > 0)
                {
                    if (autoSwimToSurface)
                        _moveDirection.y = 1f;
                    tempTargetPosition = Vector3.up * (_waterCollider.bounds.max.y - (CacheCapsuleCollider.bounds.size.y * underWaterThreshold));
                    tempCurrentPosition = Vector3.up * CacheTransform.position.y;
                    tempTargetDistance = Vector3.Distance(tempTargetPosition, tempCurrentPosition);
                    tempSqrMagnitude = (tempTargetPosition - tempCurrentPosition).sqrMagnitude;
                    tempPredictPosition = tempCurrentPosition + (Vector3.up * _moveDirection.y * CurrentMoveSpeed * deltaTime);
                    tempPredictSqrMagnitude = (tempPredictPosition - tempCurrentPosition).sqrMagnitude;
                    // Check `tempSqrMagnitude` against the `tempPredictSqrMagnitude`
                    // if `tempPredictSqrMagnitude` is greater than `tempSqrMagnitude`,
                    // rigidbody will reaching target and character is moving pass it,
                    // so adjust move speed by distance and time (with physic formula: v=s/t)
                    if (tempPredictSqrMagnitude >= tempSqrMagnitude && tempTargetDistance > 0f)
                        CurrentMoveSpeed *= tempTargetDistance / deltaTime / CurrentMoveSpeed;
                    // Swim up to surface
                    if (CurrentMoveSpeed < 0.01f)
                        _moveDirection.y = 0f;
                    tempMoveVelocity.y = _moveDirection.y * CurrentMoveSpeed;
                    if (!HasNavPaths)
                        _currentInput = this.SetInputYPosition(_currentInput, tempPredictPosition.y);
                }
            }
            else
            {
                // Update velocity while not under water
                tempMoveVelocity.y = _verticalVelocity;
            }

            // Don't applies velocity while using root motion
            if ((isGrounded && useRootMotionForMovement) ||
                (isAirborne && useRootMotionForAirMovement) ||
                (!isGrounded && !isAirborne && useRootMotionForMovement) ||
                (_isUnderWater && useRootMotionUnderWater))
            {
                tempMoveVelocity.x = 0;
                tempMoveVelocity.z = 0;
            }

            Vector3 platformMotion = Vector3.zero;
            if (!_isUnderWater)
            {
                // Apply platform motion
                if (_groundedTransform != null)
                {
                    platformMotion = (_groundedTransform.position - _previousPlatformPosition) / deltaTime;
                    _previousPlatformPosition = _groundedTransform.position;
                }
            }
            _collisionFlags = CacheOpenCharacterController.Move((tempMoveVelocity + platformMotion) * deltaTime);

            if (_yTurnSpeed <= 0f)
                _yAngle = _targetYAngle;
            else if (Mathf.Abs(_yAngle - _targetYAngle) > 1f)
                _yAngle = Mathf.LerpAngle(_yAngle, _targetYAngle, _yTurnSpeed * deltaTime);
            UpdateRotation();
            _lookRotationApplied = true;
            _currentInput = this.SetInputRotation(_currentInput, CacheTransform.rotation);
            _isJumping = false;
            _acceptedJump = false;
            _previouslyGrounded = isGrounded;
            _previouslyAirborne = isAirborne;
            _previouslyExtraMovementState = _tempExtraMovementState;
        }

        private float CalculateCurrentMoveSpeed(float maxMoveSpeed, float deltaTime)
        {
            // Adjust speed by rtt
            if (!IsServer && IsOwnerClient && movementSecure == MovementSecure.ServerAuthoritative)
            {
                float rtt = 0.001f * Entity.Manager.Rtt;
                float acc = 1f / rtt * deltaTime * 0.5f;
                if (!_lagMoveSpeedRate.HasValue)
                    _lagMoveSpeedRate = 0f;
                if (_lagMoveSpeedRate < 1f)
                    _lagMoveSpeedRate += acc;
                if (_lagMoveSpeedRate > 1f)
                    _lagMoveSpeedRate = 1f;
                return maxMoveSpeed * _lagMoveSpeedRate.Value;
            }
            // TODO: Adjust other's client move speed by rtt
            return maxMoveSpeed;
        }

        private void UpdateRotation()
        {
            CacheTransform.eulerAngles = new Vector3(0f, _yAngle, 0f);
        }

        private void SetMovePaths(Vector3 position, bool useNavMesh)
        {
            if (useNavMesh)
            {
                NavMeshPath navPath = new NavMeshPath();
                if (NavMesh.SamplePosition(position, out NavMeshHit navHit, 5f, NavMesh.AllAreas) &&
                    NavMesh.CalculatePath(CacheTransform.position, navHit.position, NavMesh.AllAreas, navPath))
                {
                    NavPaths = new Queue<Vector3>(navPath.corners);
                    // Dequeue first path it's not require for future movement
                    NavPaths.Dequeue();
                }
            }
            else
            {
                // If not use nav mesh, just move to position by direction
                NavPaths = new Queue<Vector3>();
                NavPaths.Enqueue(position);
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
                _waterCollider = other;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == PhysicLayers.Water)
            {
                // Exit water
                _waterCollider = null;
            }
        }

        private void OnCharacterControllerCollision(OpenCharacterController.CollisionInfo hit)
        {
            if (CacheOpenCharacterController.isGrounded)
            {
                if (CacheTransform.position.y >= hit.point.y)
                {
                    _groundedTransform = hit.collider.transform;
                    _previousPlatformPosition = _groundedTransform.position;
                    return;
                }
            }
            _groundedTransform = null;
        }

        public bool WriteClientState(NetDataWriter writer, out bool shouldSendReliably)
        {
            shouldSendReliably = false;
            if (movementSecure == MovementSecure.NotSecure && IsOwnerClient && !IsServer)
            {
                // Sync transform from owner client to server (except it's both owner client and server)
                if (_sendingJump)
                {
                    shouldSendReliably = true;
                    MovementState |= MovementState.IsJump;
                }
                else
                {
                    MovementState &= ~MovementState.IsJump;
                }
                if (_isClientConfirmingTeleport)
                {
                    shouldSendReliably = true;
                    MovementState |= MovementState.IsTeleport;
                }
                this.ClientWriteSyncTransform3D(writer);
                _sendingJump = false;
                _isClientConfirmingTeleport = false;
                return true;
            }
            if (movementSecure == MovementSecure.ServerAuthoritative && IsOwnerClient && !IsServer)
            {
                _currentInput = this.SetInputExtraMovementState(_currentInput, _tempExtraMovementState);
                if (this.DifferInputEnoughToSend(_oldInput, _currentInput, out EntityMovementInputState inputState) || _sendingJump || _isClientConfirmingTeleport)
                {
                    if (_sendingJump)
                    {
                        shouldSendReliably = true;
                        _currentInput = this.SetInputJump(_currentInput);
                    }
                    else
                    {
                        _currentInput = this.ClearInputJump(_currentInput);
                    }
                    if (!_currentInput.IsKeyMovement)
                    {
                        // Point click should be reliably
                        shouldSendReliably = true;
                    }
                    if (_isClientConfirmingTeleport)
                    {
                        shouldSendReliably = true;
                        _currentInput.MovementState |= MovementState.IsTeleport;
                    }
                    this.ClientWriteMovementInput3D(writer, inputState, _currentInput.MovementState, _currentInput.ExtraMovementState, _currentInput.Position, _currentInput.Rotation);
                    _sendingJump = false;
                    _isClientConfirmingTeleport = false;
                    _oldInput = _currentInput;
                    _currentInput = null;
                    return true;
                }
            }
            return false;
        }

        public bool WriteServerState(NetDataWriter writer, out bool shouldSendReliably)
        {
            shouldSendReliably = false;
            // Sync transform from server to all clients (include owner client)
            if (_sendingJump)
            {
                shouldSendReliably = true;
                MovementState |= MovementState.IsJump;
            }
            else
            {
                MovementState &= ~MovementState.IsJump;
            }
            if (_isTeleporting)
            {
                shouldSendReliably = true;
                MovementState |= MovementState.IsTeleport;
            }
            else
            {
                MovementState &= ~MovementState.IsTeleport;
            }
            this.ServerWriteSyncTransform3D(writer);
            _sendingJump = false;
            _isTeleporting = false;
            return true;
        }

        public void ReadClientStateAtServer(NetDataReader reader)
        {
            switch (movementSecure)
            {
                case MovementSecure.NotSecure:
                    ReadSyncTransformAtServer(reader);
                    break;
                case MovementSecure.ServerAuthoritative:
                    ReadMovementInputAtServer(reader);
                    break;
            }
        }

        public void ReadServerStateAtClient(NetDataReader reader)
        {
            if (IsServer)
            {
                // Don't read and apply transform, because it was done at server
                return;
            }
            reader.ReadSyncTransformMessage3D(out MovementState movementState, out ExtraMovementState extraMovementState, out Vector3 position, out float yAngle, out long timestamp);
            if (movementState.Has(MovementState.IsTeleport))
            {
                // Server requested to teleport
                OnTeleport(position, yAngle);
            }
            else if (_acceptedPositionTimestamp <= timestamp)
            {
                if (Vector3.Distance(position, CacheTransform.position) >= snapThreshold)
                {
                    // Snap character to the position if character is too far from the position
                    if (movementSecure == MovementSecure.ServerAuthoritative || !IsOwnerClient)
                    {
                        _yAngle = _targetYAngle = yAngle;
                        UpdateRotation();
                        CacheOpenCharacterController.SetPosition(position, false);
                    }
                    MovementState = movementState;
                    ExtraMovementState = extraMovementState;
                }
                else if (!IsOwnerClient)
                {
                    _targetYAngle = yAngle;
                    _yTurnSpeed = 1f / Time.fixedDeltaTime;
                    if (Vector3.Distance(position.GetXZ(), CacheTransform.position.GetXZ()) > 0.01f)
                        _clientTargetPosition = position;
                    else
                        _clientTargetPosition = null;
                    MovementState = movementState;
                    ExtraMovementState = extraMovementState;
                }
                _acceptedPositionTimestamp = timestamp;
            }
            if (!IsOwnerClient && movementState.Has(MovementState.IsJump))
                _acceptedJump = true;
        }

        public void ReadMovementInputAtServer(NetDataReader reader)
        {
            if (IsOwnerClient)
            {
                // Don't read and apply inputs, because it was done (this is both owner client and server)
                return;
            }
            if (movementSecure == MovementSecure.NotSecure)
            {
                // Movement handling at client, so don't read movement inputs from client (but have to read transform)
                return;
            }
            if (!Entity.CanMove())
                return;
            reader.ReadMovementInputMessage3D(out EntityMovementInputState inputState, out MovementState movementState, out ExtraMovementState extraMovementState, out Vector3 position, out float yAngle, out long timestamp);
            if (movementState.Has(MovementState.IsTeleport))
            {
                // Teleport confirming from client
                _isServerWaitingTeleportConfirm = false;
            }
            if (_isServerWaitingTeleportConfirm)
            {
                // Waiting for teleport confirming
                return;
            }
            if (Mathf.Abs(timestamp - BaseGameNetworkManager.Singleton.ServerTimestamp) > s_lagBuffer)
            {
                // Timestamp is a lot difference to server's timestamp, player might try to hack a game or packet may corrupted occurring, so skip it
                return;
            }
            if (_acceptedPositionTimestamp <= timestamp)
            {
                if (!inputState.Has(EntityMovementInputState.IsStopped))
                {
                    _tempMovementState = movementState;
                    _tempExtraMovementState = extraMovementState;
                    _clientTargetPosition = null;
                    if (inputState.Has(EntityMovementInputState.PositionChanged))
                    {
                        if (inputState.Has(EntityMovementInputState.IsKeyMovement))
                        {
                            NavPaths = null;
                            _clientTargetPosition = position;
                        }
                        else
                        {
                            _clientTargetPosition = null;
                            SetMovePaths(position, true);
                        }
                    }
                    if (inputState.Has(EntityMovementInputState.RotationChanged))
                    {
                        if (IsClient)
                        {
                            _targetYAngle = yAngle;
                            _yTurnSpeed = 1f / Time.fixedDeltaTime;
                        }
                        else
                        {
                            _yAngle = _targetYAngle = yAngle;
                            UpdateRotation();
                        }
                    }
                    if (movementState.Has(MovementState.IsJump))
                        _acceptedJump = true;
                }
                else
                {
                    StopMoveFunction();
                }
                _acceptedPositionTimestamp = timestamp;
            }
        }

        public void ReadSyncTransformAtServer(NetDataReader reader)
        {
            if (IsOwnerClient)
            {
                // Don't read and apply transform, because it was done (this is both owner client and server)
                return;
            }
            if (movementSecure == MovementSecure.ServerAuthoritative)
            {
                // Movement handling at server, so don't read sync transform from client
                return;
            }
            reader.ReadSyncTransformMessage3D(out MovementState movementState, out ExtraMovementState extraMovementState, out Vector3 position, out float yAngle, out long timestamp);
            if (movementState.Has(MovementState.IsTeleport))
            {
                // Teleport confirming from client
                _isServerWaitingTeleportConfirm = false;
            }
            if (_isServerWaitingTeleportConfirm)
            {
                // Waiting for teleport confirming
                return;
            }
            if (Mathf.Abs(timestamp - BaseGameNetworkManager.Singleton.ServerTimestamp) > s_lagBuffer)
            {
                // Timestamp is a lot difference to server's timestamp, player might try to hack a game or packet may corrupted occurring, so skip it
                return;
            }
            if (_acceptedPositionTimestamp <= timestamp)
            {
                if (IsClient)
                {
                    _targetYAngle = yAngle;
                    _yTurnSpeed = 1f / Time.fixedDeltaTime;
                }
                else
                {
                    _yAngle = _targetYAngle = yAngle;
                    UpdateRotation();
                }
                MovementState = movementState;
                ExtraMovementState = extraMovementState;
                if (!IsClient)
                {
                    // If it's server only (not a host), set position follows the client immediately
                    float currentTime = Time.unscaledTime;
                    float t = currentTime - _lastServerValidateTransformTime;
                    float v = Entity.GetMoveSpeed();
                    float s = (_lastServerValidateTransformMoveSpeed * (t + s_lagBufferUnityTime)) + (v * t); // +`lagBufferUnityTime` as high ping buffer
                    if (s < 0.001f)
                        s = 0.001f;
                    Vector3 oldPos = CacheTransform.position;
                    Vector3 newPos = position;
                    float dist = Vector3.Distance(oldPos, newPos);
                    if (dist <= s)
                    {
                        // Allow to move to the position
                        CacheTransform.position = position;
                    }
                    else
                    {
                        // Client moves too fast, adjust it
                        Vector3 dir = (newPos - oldPos).normalized;
                        newPos = oldPos + (dir * s);
                        CacheTransform.position = newPos;
                        // And also adjust client's position
                        Teleport(newPos, Quaternion.Euler(0f, _yAngle, 0f));
                    }
                    _lastServerValidateTransformTime = currentTime;
                    _lastServerValidateTransformMoveSpeed = v;
                }
                else
                {
                    // It's both server and client, translate position (it's a host so don't do speed hack validation)
                    if (Vector3.Distance(position, CacheTransform.position) > 0.01f)
                        SetMovePaths(position, false);
                }
                _acceptedPositionTimestamp = timestamp;
            }
            if (movementState.Has(MovementState.IsJump))
                _acceptedJump = true;
        }

        protected virtual void OnTeleport(Vector3 position, float yAngle)
        {
            _airborneElapsed = 0;
            _verticalVelocity = 0;
            _clientTargetPosition = null;
            NavPaths = null;
            CacheOpenCharacterController.SetPosition(position, false);
            _yAngle = _targetYAngle = yAngle;
            UpdateRotation();
            if (IsServer && !IsOwnedByServer)
                _isServerWaitingTeleportConfirm = true;
            if (!IsServer && IsOwnerClient)
                _isClientConfirmingTeleport = true;
        }

        public bool CanPredictMovement()
        {
            return Entity.IsOwnerClient || (Entity.IsOwnerClientOrOwnedByServer && movementSecure == MovementSecure.NotSecure) || (Entity.IsServer && movementSecure == MovementSecure.ServerAuthoritative);
        }

#if UNITY_EDITOR
        [ContextMenu("Applies Collider Settings To Controller")]
        public void AppliesColliderSettingsToController()
        {
            CapsuleCollider collider = gameObject.GetOrAddComponent<CapsuleCollider>();
            float radius = collider.radius;
            float height = collider.height;
            Vector3 center = collider.center;
            // Prepare open character controller
            OpenCharacterController controller = gameObject.GetOrAddComponent<OpenCharacterController>();
            controller.SetRadiusHeightAndCenter(radius, height, center, true, true);
        }
#endif
    }
}
