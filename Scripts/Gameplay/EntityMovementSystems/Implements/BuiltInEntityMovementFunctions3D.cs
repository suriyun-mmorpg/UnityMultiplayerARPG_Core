using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MultiplayerARPG
{
    public partial class BuiltInEntityMovementFunctions3D
    {
        private static readonly RaycastHit[] s_findGroundRaycastHits = new RaycastHit[4];
        private static readonly long s_lagBuffer = System.TimeSpan.TicksPerMillisecond * 200;
        private static readonly int s_forceGroundedFramesAfterTeleport = 3;
        private static readonly float s_minDistanceToSimulateMovement = 0.01f;
        private static readonly float s_timestampToUnityTimeMultiplier = 0.001f;

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
        public float stickGroundForce = 9.6f;
        [Tooltip("Delay before character change from grounded state to airborne")]
        public float airborneDelay = 0.01f;
        public bool doNotChangeVelocityWhileAirborne;

        [Header("Pausing")]
        public float landedPauseMovementDuration = 0f;
        public float beforeCrawlingPauseMovementDuration = 0f;
        public float afterCrawlingPauseMovementDuration = 0f;

        [Header("Swimming")]
        [Range(0.1f, 1f)]
        public float underWaterThreshold = 0.75f;
        public bool autoSwimToSurface;

        [Header("Dashing")]
        public EntityMovementForceApplier dashingForceApplier;

        [Header("Root Motion Settings")]
        public bool alwaysUseRootMotion;
        public bool useRootMotionForMovement;
        public bool useRootMotionForAirMovement;
        public bool useRootMotionForJump;
        public bool useRootMotionForFall;
        public bool useRootMotionUnderWater;
        public bool useRootMotionClimbing;

        [Header("Networking Settings")]
        public float snapThreshold = 5.0f;

        public BaseGameEntity Entity { get; private set; }
        public CharacterLadderComponent LadderComponent { get; private set; }
        public bool IsServer { get { return Entity.IsServer; } }
        public bool IsClient { get { return Entity.IsClient; } }
        public bool IsOwnerClient { get { return Entity.IsOwnerClient; } }
        public bool IsOwnedByServer { get { return Entity.IsOwnedByServer; } }
        public bool IsOwnerClientOrOwnedByServer { get { return Entity.IsOwnerClientOrOwnedByServer; } }
        public GameInstance CurrentGameInstance { get { return Entity.CurrentGameInstance; } }
        public BaseGameplayRule CurrentGameplayRule { get { return Entity.CurrentGameplayRule; } }
        public BaseGameNetworkManager CurrentGameManager { get { return Entity.CurrentGameManager; } }
        public Transform CacheTransform { get { return Entity.EntityTransform; } }
        public Animator Animator { get; private set; }
        public IBuiltInEntityMovement3D EntityMovement { get; private set; }

        public float StoppingDistance
        {
            get { return stoppingDistance; }
        }
        public MovementState MovementState { get; private set; }
        public ExtraMovementState ExtraMovementState { get; private set; }
        public DirectionVector2 Direction2D { get { return Vector2.down; } set { } }
        public float CurrentMoveSpeed { get; private set; }
        public Queue<Vector3> NavPaths { get; private set; }
        public bool HasNavPaths
        {
            get { return NavPaths != null && NavPaths.Count > 0; }
        }

        // Input codes
        private bool _isJumping;
        private bool _isDashing;
        private Vector3 _inputDirection;
        private MovementState _tempMovementState;
        private ExtraMovementState _tempExtraMovementState;

        // Client state codes
        private EntityMovementInput _oldInput;
        private EntityMovementInput _currentInput;
        private bool _sendingJump;
        private bool _sendingDash;

        // State simulate codes
        private float? _lagMoveSpeedRate;
        private float _verticalVelocity;
        private Vector3 _velocityBeforeAirborne;
        private Collider _waterCollider;
        private Transform _groundedTransform;
        private Vector3 _previousPlatformPosition;
        private Vector3 _previousPosition;
        private Vector3 _previousMovement;
        private bool _isGrounded = true;
        private bool _isAirborne = false;
        private bool _isUnderWater = false;
        private bool _isClimbing = false;
        private bool _previouslyGrounded = false;
        private bool _previouslyAirborne = false;
        private bool _simulatingKeyMovement = false;
        private ExtraMovementState _previouslyExtraMovementState;

        // Move simulate codes
        private float _pauseMovementCountDown;
        private Vector3 _moveDirection;
        private readonly List<EntityMovementForceApplier> _movementForceAppliers = new List<EntityMovementForceApplier>();

        // Jump simulate codes
        private float _airborneElapsed;
        private bool _applyingJumpForce;
        private float _applyJumpForceCountDown;
        private ExtraMovementState _extraMovementStateWhenJump;

        // Turn simulate codes
        private bool _lookRotationApplied;
        private float _yAngle;
        private float _targetYAngle;
        private float _yTurnSpeed;
        private float? _remoteTargetYAngle;

        // Teleport codes
        private bool _isTeleporting;
        private bool _stillMoveAfterTeleport;
        private int _lastTeleportFrame;

        // Peers accept codes
        private bool _acceptedJump;
        private bool _acceptedDash;
        private long _acceptedPositionTimestamp;

        // Server validate codes
        private float _lastServerValidateHorDistDiff;
        private float _lastServerValidateVerDistDiff;
        private float _lastServerHorSpdBeforeAirborne;
        private bool _isServerWaitingTeleportConfirm;

        // Client confirm codes
        private bool _isClientConfirmingTeleport;

        public BuiltInEntityMovementFunctions3D(BaseGameEntity entity, Animator animator, IBuiltInEntityMovement3D entityMovement)
        {
            Entity = entity;
            LadderComponent = entity.GetComponent<CharacterLadderComponent>();
            Animator = animator;
            EntityMovement = entityMovement;
            _yAngle = _targetYAngle = CacheTransform.eulerAngles.y;
            _lookRotationApplied = true;
        }

        public void EntityStart()
        {
            _isClientConfirmingTeleport = true;
            _yAngle = CacheTransform.eulerAngles.y;
            _verticalVelocity = 0;
            _lastTeleportFrame = Time.frameCount;
            _previousPosition = CacheTransform.position;
        }

        public void ComponentEnabled()
        {
            _verticalVelocity = 0;
            _lastTeleportFrame = Time.frameCount;
            _previousPosition = CacheTransform.position;
        }

        public void OnSetOwnerClient(bool isOwnerClient)
        {
            NavPaths = null;
        }

        public void OnAnimatorMove()
        {
            if (!Animator)
                return;

            if (alwaysUseRootMotion || Entity.ShouldUseRootMotion)
            {
                // Always use root motion
                Animator.ApplyBuiltinRootMotion();
                return;
            }

            if (MovementState.Has(MovementState.IsGrounded) && useRootMotionForMovement)
                Animator.ApplyBuiltinRootMotion();
            if (!MovementState.Has(MovementState.IsGrounded) && useRootMotionForAirMovement)
                Animator.ApplyBuiltinRootMotion();
            if (MovementState.Has(MovementState.IsUnderWater) && useRootMotionUnderWater)
                Animator.ApplyBuiltinRootMotion();
            if (MovementState.Has(MovementState.IsClimbing) && useRootMotionClimbing)
                Animator.ApplyBuiltinRootMotion();
        }

        public void StopMove()
        {
            if (movementSecure == MovementSecure.ServerAuthoritative)
            {
                // Send movement input to server, then server will apply movement and sync transform to clients
                _currentInput = Entity.SetInputStop(_currentInput);
            }
            StopMoveFunction();
        }

        public void StopMoveFunction()
        {
            NavPaths = null;
            _lagMoveSpeedRate = null;
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
                    _isJumping = _isGrounded && _tempMovementState.Has(MovementState.IsJump);
                if (!_isDashing)
                    _isDashing = _isGrounded && _tempMovementState.Has(MovementState.IsDash);
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
            if (CanPredictMovement() && !_isJumping)
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
                if (LadderComponent && LadderComponent.ClimbingLadder)
                {
                    // Turn to the ladder
                    _targetYAngle = Quaternion.LookRotation(-LadderComponent.ClimbingLadder.ForwardWithYAngleOffsets).eulerAngles.y;
                }
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

        public void Teleport(Vector3 position, Quaternion rotation, bool stillMoveAfterTeleport)
        {
            if (!IsServer)
            {
                Logging.LogWarning(nameof(BuiltInEntityMovementFunctions3D), $"Teleport function shouldn't be called at client [{Entity.name}]");
                return;
            }
            _isTeleporting = true;
            _stillMoveAfterTeleport = stillMoveAfterTeleport;
            OnTeleport(position, rotation.eulerAngles.y, stillMoveAfterTeleport);
        }

        public bool FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
        {
            return PhysicUtils.FindGroundedPosition(fromPosition, s_findGroundRaycastHits, findDistance, GameInstance.Singleton.GetGameEntityGroundDetectionLayerMask(), out result, CacheTransform);
        }

        public void ApplyForce(Vector3 direction, ApplyMovementForceMode mode, float force, float deceleration, float duration)
        {
            if (!IsServer)
                return;
            if (mode.IsReplaceMovement())
            {
                // Can have only one replace movement force applier, so remove stored ones
                _movementForceAppliers.RemoveReplaceMovementForces();
            }
            _movementForceAppliers.Add(new EntityMovementForceApplier()
                .Apply(direction, mode, force, deceleration, duration));
        }

        public void ClearAllForces()
        {
            if (!IsServer)
                return;
            _movementForceAppliers.Clear();
        }

        /// <summary>
        /// Calculates the velocity required to move the character to the target position over a specific deltaTime.
        /// Useful for when you wish to work with positions rather than velocities in the UpdateVelocity callback 
        /// </summary>
        public Vector3 GetVelocityForMovePosition(Vector3 fromPosition, Vector3 toPosition, float deltaTime)
        {
            return GetVelocityFromMovement(toPosition - fromPosition, deltaTime);
        }

        public Vector3 GetVelocityFromMovement(Vector3 movement, float deltaTime)
        {
            if (deltaTime <= 0f)
                return Vector3.zero;

            return movement / deltaTime;
        }

        public bool WaterCheck(Collider waterCollider)
        {
            if (waterCollider == null)
            {
                // Not in water
                return false;
            }
            return Entity.EntityTransform.position.y < TargetWaterSurfaceY(waterCollider) + 0.01f;
        }

        public float TargetWaterSurfaceY(Collider waterCollider)
        {
            Bounds movementBounds = EntityMovement.GetBounds();
            float result = waterCollider.bounds.max.y - (underWaterThreshold * movementBounds.size.y);
            return result;
        }

        public void UpdateMovement(float deltaTime)
        {
            _moveDirection = Vector3.zero;
            _isUnderWater = WaterCheck(_waterCollider);
            _isClimbing = LadderComponent && LadderComponent.ClimbingLadder;
            _isGrounded = EntityMovement.GroundCheck();
            _isAirborne = !_isGrounded && !_isUnderWater && !_isClimbing && _airborneElapsed >= airborneDelay;

            // Update airborne elasped
            if (_isGrounded)
                _airborneElapsed = 0f;
            else
                _airborneElapsed += deltaTime;

            // Underwater state, movement state must be setup here to make it able to calculate move speed properly
            if (_isClimbing)
                _tempMovementState |= MovementState.IsClimbing;
            else if (_isUnderWater)
                _tempMovementState |= MovementState.IsUnderWater;

            if (_isClimbing)
                UpdateClimbMovement(deltaTime);
            else
                UpdateGenericMovement(deltaTime);

            _currentInput = Entity.SetInputYAngle(_currentInput, CacheTransform.eulerAngles.y);
        }

        protected void UpdateClimbMovement(float deltaTime)
        {
            if (!_tempMovementState.Has(MovementState.Up) &&
                !_tempMovementState.Has(MovementState.Down))
            {
                // No movement inputs
                return;
            }

            Vector3 tempPredictPosition;
            Vector3 tempCurrentPosition = CacheTransform.position;
            // Prepare movement speed
            _tempExtraMovementState = Entity.ValidateExtraMovementState(_tempMovementState, _tempExtraMovementState);
            float tempEntityMoveSpeed = Entity.GetMoveSpeed(_tempMovementState, _tempExtraMovementState);
            float tempMaxMoveSpeed = tempEntityMoveSpeed;
            CurrentMoveSpeed = CalculateCurrentMoveSpeed(tempMaxMoveSpeed, deltaTime);

            if (_tempMovementState.Has(MovementState.Up))
                _moveDirection.y = 1f;
            else if (_tempMovementState.Has(MovementState.Down))
                _moveDirection.y = -1f;

            if (Mathf.Approximately(_moveDirection.y, 0f))
                return;

            Vector3 tempMoveVelocity = GetVelocityForMovePosition(tempCurrentPosition,
                LadderComponent.ClimbingLadder.ClosestPointOnLadderSegment(tempCurrentPosition, EntityMovement.GetBounds().extents.z, out float segmentState), deltaTime) +
                LadderComponent.ClimbingLadder.Up * _moveDirection.y * CurrentMoveSpeed;

            if (Mathf.Abs(segmentState) > 0.05f)
            {
                if (segmentState > 0 && _moveDirection.y > 0f)
                {
                    // Exit (top)
                    tempMoveVelocity = GetVelocityForMovePosition(tempCurrentPosition, LadderComponent.ClimbingLadder.topExitTransform.position, deltaTime);
                    LadderComponent.CallCmdExitLadder(LadderEntranceType.Top);
                    LadderComponent.ClimbingLadder = null;
                }
                // If we're lower than the ladder bottom point
                else if (segmentState < 0 && _moveDirection.y < 0f)
                {
                    // Exit (bottom)
                    tempMoveVelocity = GetVelocityForMovePosition(tempCurrentPosition, LadderComponent.ClimbingLadder.bottomExitTransform.position, deltaTime);
                    LadderComponent.CallCmdExitLadder(LadderEntranceType.Bottom);
                    LadderComponent.ClimbingLadder = null;
                }
            }

            tempPredictPosition = tempCurrentPosition + (tempMoveVelocity * deltaTime);
            _currentInput = Entity.SetInputMovementState(_currentInput, _tempMovementState);
            _currentInput = Entity.SetInputPosition(_currentInput, tempPredictPosition);
            _currentInput = Entity.SetInputIsKeyMovement(_currentInput, true);
            _previousMovement = tempMoveVelocity * deltaTime;
            EntityMovement.Move(_previousMovement);
        }

        protected void UpdateGenericMovement(float deltaTime)
        {
            float tempSqrMagnitude;
            float tempPredictSqrMagnitude;
            Vector3 tempPredictPosition;
            float tempTargetDistance = 0f;
            Vector3 tempHorizontalMoveDirection = Vector3.zero;
            Vector3 tempMoveVelocity = Vector3.zero;
            Vector3 tempCurrentPosition = CacheTransform.position;
            Vector3 tempTargetPosition = tempCurrentPosition;
            bool forceUseRootMotion = alwaysUseRootMotion || Entity.ShouldUseRootMotion;

            if (HasNavPaths)
            {
                // Set `tempTargetPosition` and `tempCurrentPosition`
                tempTargetPosition = NavPaths.Peek();
                _moveDirection = (tempTargetPosition - tempCurrentPosition).normalized;
                tempTargetDistance = Vector3.Distance(tempTargetPosition.GetXZ(), tempCurrentPosition.GetXZ());
                float stoppingDistance = _simulatingKeyMovement ? s_minDistanceToSimulateMovement : StoppingDistance;
                bool shouldStop = tempTargetDistance < stoppingDistance;
                if (shouldStop)
                {
                    NavPaths.Dequeue();
                    if (!HasNavPaths)
                    {
                        StopMoveFunction();
                        _moveDirection = Vector3.zero;
                    }
                    else
                    {
                        if (!_simulatingKeyMovement && !_tempMovementState.Has(MovementState.Forward))
                            _tempMovementState |= MovementState.Forward;
                    }
                }
                else
                {
                    if (!_simulatingKeyMovement && !_tempMovementState.Has(MovementState.Forward))
                        _tempMovementState |= MovementState.Forward;
                }
            }
            else if (_inputDirection.sqrMagnitude > 0f)
            {
                _moveDirection = _inputDirection.normalized;
                tempTargetPosition = tempCurrentPosition + _moveDirection;
            }
            else
            {
                if (_previousMovement.sqrMagnitude <= 0f)
                    StopMove();
                ApplyRemoteTurnAngle();
            }

            if ((IsOwnerClientOrOwnedByServer || (HasNavPaths && !_simulatingKeyMovement)) && _lookRotationApplied && _moveDirection.sqrMagnitude > 0f)
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
            {
                _isJumping = false;
            }

            if (!Entity.CanDash())
            {
                _isDashing = false;
            }

            // Prepare movement speed
            _tempExtraMovementState = Entity.ValidateExtraMovementState(_tempMovementState, _tempExtraMovementState);
            float tempEntityMoveSpeed = _applyingJumpForce ? 0f : Entity.GetMoveSpeed(_tempMovementState, _tempExtraMovementState);
            float tempMaxMoveSpeed = tempEntityMoveSpeed;

            // Calculate vertical velocity by gravity
            if (!_isGrounded && !_isUnderWater)
            {
                if (!useRootMotionForFall && !forceUseRootMotion)
                {
                    _verticalVelocity -= CalculateGravity() * deltaTime;
                    _verticalVelocity = Mathf.Max(_verticalVelocity, -CalculateMaxFallVelocity());
                }
                else
                {
                    _verticalVelocity = 0f;
                }
            }
            else
            {
                // Not falling set verical velocity to 0
                _verticalVelocity = 0f;
            }

            // Jumping 
            if (_acceptedJump || (_pauseMovementCountDown <= 0f && _isGrounded && _isJumping))
            {
                _sendingJump = true;
                _extraMovementStateWhenJump = _tempExtraMovementState;
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
                _applyJumpForceCountDown -= deltaTime;
                if (_applyJumpForceCountDown <= 0f)
                {
                    _isGrounded = false;
                    _applyingJumpForce = false;
                    float jumpForceVerticalVelocity = CalculateJumpVerticalSpeed();
                    if (!useRootMotionForJump && !forceUseRootMotion)
                    {
                        _verticalVelocity = jumpForceVerticalVelocity;
                    }
                    EntityMovement.OnJumpForceApplied(jumpForceVerticalVelocity);
                    Entity.OnJumpForceApplied(jumpForceVerticalVelocity);
                }
            }

            // Updating horizontal movement (WASD inputs)
            if (!_isAirborne)
                _velocityBeforeAirborne = Vector3.zero;

            // Dashing
            if (_acceptedDash || (_pauseMovementCountDown <= 0f && _isGrounded && _isDashing))
            {
                _sendingDash = true;
                dashingForceApplier.Apply(CacheTransform.forward);
                dashingForceApplier.Mode = ApplyMovementForceMode.Dash;
                // Can have only one replace movement force applier, so remove stored ones
                _movementForceAppliers.RemoveReplaceMovementForces();
                _movementForceAppliers.Add(dashingForceApplier);
            }

            // Apply Forces
            _movementForceAppliers.UpdateForces(deltaTime,
                Entity.GetMoveSpeed(MovementState.Forward, ExtraMovementState.None),
                out Vector3 forceMotion, out EntityMovementForceApplier replaceMovementForceApplier);

            // Replace player's movement by this
            if (replaceMovementForceApplier != null)
            {
                // Still dashing to add dash to movement state
                if (replaceMovementForceApplier.Mode == ApplyMovementForceMode.Dash)
                    _tempMovementState |= MovementState.IsDash;
                // Force turn to dashed direction
                _moveDirection = replaceMovementForceApplier.Direction;
                _targetYAngle = Quaternion.LookRotation(_moveDirection).eulerAngles.y;
                // Change move speed to dash force
                tempMaxMoveSpeed = replaceMovementForceApplier.CurrentSpeed;
            }

            // Movement updating
            if (_pauseMovementCountDown <= 0f && _moveDirection.sqrMagnitude > 0f && (!_isAirborne || !doNotChangeVelocityWhileAirborne || !IsOwnerClientOrOwnedByServer))
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
                if (HasNavPaths)
                {
                    // Check `tempSqrMagnitude` against the `tempPredictSqrMagnitude`
                    // if `tempPredictSqrMagnitude` is greater than `tempSqrMagnitude`,
                    // rigidbody will reaching target and character is moving pass it,
                    // so adjust move speed by distance and time (with physic formula: v=s/t)
                    if (tempPredictSqrMagnitude >= tempSqrMagnitude && tempTargetDistance > 0f)
                        CurrentMoveSpeed *= tempTargetDistance / deltaTime / CurrentMoveSpeed;
                    if (CurrentMoveSpeed < 0.01f || tempTargetDistance <= 0f)
                        CurrentMoveSpeed = 0f;
                }
                tempMoveVelocity = tempHorizontalMoveDirection * CurrentMoveSpeed;
                _velocityBeforeAirborne = tempMoveVelocity;
                // Set inputs
                _currentInput = Entity.SetInputMovementState(_currentInput, _tempMovementState);
                if (HasNavPaths)
                {
                    _currentInput = Entity.SetInputPosition(_currentInput, tempTargetPosition);
                    _currentInput = Entity.SetInputIsKeyMovement(_currentInput, false);
                }
                else
                {
                    _currentInput = Entity.SetInputPosition(_currentInput, tempPredictPosition);
                    _currentInput = Entity.SetInputIsKeyMovement(_currentInput, true);
                }
            }

            // Pause movement updating
            if (IsOwnerClientOrOwnedByServer)
            {
                if (_isGrounded && _previouslyAirborne)
                {
                    _pauseMovementCountDown = landedPauseMovementDuration;
                }
                else if (_isGrounded && _previouslyExtraMovementState != ExtraMovementState.IsCrawling && _tempExtraMovementState == ExtraMovementState.IsCrawling)
                {
                    _pauseMovementCountDown = beforeCrawlingPauseMovementDuration;
                }
                else if (_isGrounded && _previouslyExtraMovementState == ExtraMovementState.IsCrawling && _tempExtraMovementState != ExtraMovementState.IsCrawling)
                {
                    _pauseMovementCountDown = afterCrawlingPauseMovementDuration;
                }
                else
                {
                    if (_pauseMovementCountDown > 0f)
                        _pauseMovementCountDown -= deltaTime;
                }
                if (_pauseMovementCountDown > 0f)
                {
                    // Remove movement from movestate while pausing movement
                    _tempMovementState ^= MovementState.Forward | MovementState.Backward | MovementState.Left | MovementState.Right;
                }
            }

            // Move by velocity before jump
            if ((_isAirborne || _airborneElapsed > 0f) && doNotChangeVelocityWhileAirborne)
                tempMoveVelocity = _velocityBeforeAirborne;
            // Updating vertical movement (Fall, WASD inputs under water)
            if (_isUnderWater)
            {
                CurrentMoveSpeed = CalculateCurrentMoveSpeed(tempMaxMoveSpeed, deltaTime);

                // Move up to surface while under water
                if (autoSwimToSurface || _tempMovementState.Has(MovementState.Up) || _tempMovementState.Has(MovementState.Down) || Mathf.Abs(_moveDirection.y) > 0)
                {
                    if (autoSwimToSurface || _tempMovementState.Has(MovementState.Up))
                        _moveDirection.y = 1f;
                    else if (_tempMovementState.Has(MovementState.Down))
                        _moveDirection.y = -1f;
                    tempTargetPosition = Vector3.up * TargetWaterSurfaceY(_waterCollider);
                    tempCurrentPosition = Vector3.up * CacheTransform.position.y;
                    tempTargetDistance = Vector3.Distance(tempTargetPosition, tempCurrentPosition);
                    tempSqrMagnitude = (tempTargetPosition - tempCurrentPosition).sqrMagnitude;
                    tempPredictPosition = tempCurrentPosition + (Vector3.up * _moveDirection.y * CurrentMoveSpeed * deltaTime);
                    if (_moveDirection.y > 0f)
                    {
                        tempPredictSqrMagnitude = (tempPredictPosition - tempCurrentPosition).sqrMagnitude;
                        // Check `tempSqrMagnitude` against the `tempPredictSqrMagnitude`
                        // if `tempPredictSqrMagnitude` is greater than `tempSqrMagnitude`,
                        // rigidbody will reaching target and character is moving pass it,
                        // so adjust move speed by distance and time (with physic formula: v=s/t)
                        if (tempPredictSqrMagnitude >= tempSqrMagnitude && tempTargetDistance > 0f)
                            CurrentMoveSpeed *= tempTargetDistance / deltaTime / CurrentMoveSpeed;
                        if (CurrentMoveSpeed < 0.01f || tempTargetDistance <= 0f)
                        {
                            CurrentMoveSpeed = 0f;
                            // Force set move direction y to 0 to prevent swim move animation playing
                            if (autoSwimToSurface || _tempMovementState.Has(MovementState.Up))
                                _moveDirection.y = 0f;
                        }
                    }
                    tempMoveVelocity.y = _moveDirection.y * CurrentMoveSpeed;
                    if (!HasNavPaths)
                        _currentInput = Entity.SetInputYPosition(_currentInput, tempPredictPosition.y);
                }
            }
            else
            {
                // Update velocity while not under water
                tempMoveVelocity.y = _verticalVelocity;
            }

            // Don't applies velocity while using root motion
            if ((_isGrounded && (forceUseRootMotion || useRootMotionForMovement)) ||
                (_isAirborne && (forceUseRootMotion || useRootMotionForAirMovement)) ||
                (!_isGrounded && !_isAirborne && (forceUseRootMotion || useRootMotionForMovement)) ||
                (_isUnderWater && (forceUseRootMotion || useRootMotionUnderWater)))
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
            Vector3 stickGroundMotion = _isGrounded && !_isUnderWater && platformMotion.y <= 0f ? (Vector3.down * stickGroundForce) : Vector3.zero;
            _previousMovement = (tempMoveVelocity + platformMotion + stickGroundMotion + forceMotion) * deltaTime;
            if (LadderComponent && LadderComponent.TriggeredLadderEntry && !LadderComponent.ClimbingLadder && tempHorizontalMoveDirection.sqrMagnitude > 0f)
            {
                Vector3 dirToLadder = (LadderComponent.TriggeredLadderEntry.TipTransform.position.GetXZ() - Entity.EntityTransform.position.GetXZ()).normalized;
                float angle = Vector3.Angle(tempHorizontalMoveDirection, dirToLadder);
                if (angle < 15f)
                {
                    LadderComponent.CallCmdEnterLadder();
                    _previousMovement = GetVelocityForMovePosition(tempCurrentPosition,
                        LadderComponent.TriggeredLadderEntry.ladder.ClosestPointOnLadderSegment(tempCurrentPosition, EntityMovement.GetBounds().extents.z, out _),
                        deltaTime) * deltaTime;
                }
            }
            EntityMovement.Move(_previousMovement);
        }

        public void UpdateRotation(float deltaTime)
        {
            if (_yTurnSpeed <= 0f)
                _yAngle = _targetYAngle;
            else if (Mathf.Abs(_yAngle - _targetYAngle) > 1f)
                _yAngle = Mathf.LerpAngle(_yAngle, _targetYAngle, _yTurnSpeed * deltaTime);
            _lookRotationApplied = true;
            RotateY();
        }

        public void AfterMovementUpdate(float deltaTime)
        {
            if (CanPredictMovement())
            {
                // Re-setup movement state here to make sure it is correct
                _tempMovementState = _moveDirection.sqrMagnitude > 0f ? _tempMovementState : MovementState.None;
                if (_isUnderWater)
                    _tempMovementState |= MovementState.IsUnderWater;
                if (_isGrounded || _airborneElapsed < airborneDelay || Time.frameCount - _lastTeleportFrame < s_forceGroundedFramesAfterTeleport)
                    _tempMovementState |= MovementState.IsGrounded;
                if (_isJumping)
                    _tempMovementState |= MovementState.IsJump;
                // Update movement state
                MovementState = _tempMovementState;
                // Update extra movement state
                ExtraMovementState = Entity.ValidateExtraMovementState(MovementState, _tempExtraMovementState);
                if (_isJumping || _isAirborne)
                    ExtraMovementState = _extraMovementStateWhenJump;
            }
            _previouslyGrounded = _isGrounded;
            _previouslyAirborne = _isAirborne;
            _previousPosition = CacheTransform.position;
            _previouslyExtraMovementState = ExtraMovementState;
            _isJumping = false;
            _acceptedJump = false;
            _isDashing = false;
            _acceptedDash = false;
        }

        public void FixSwimUpPosition(float deltaTime)
        {
            if (!CanPredictMovement())
                return;

            if (!_isGrounded && _isUnderWater && _previousMovement.y > 0f)
            {
                Vector3 tempTargetPosition = Vector3.up * TargetWaterSurfaceY(_waterCollider);
                if (Mathf.Abs(CacheTransform.position.y - tempTargetPosition.y) < 0.05f)
                    CacheTransform.position = new Vector3(CacheTransform.position.x, tempTargetPosition.y, CacheTransform.position.z);
            }
        }

        private float CalculateCurrentMoveSpeed(float maxMoveSpeed, float deltaTime)
        {
            // Adjust speed by rtt
            if (!IsServer && IsOwnerClient && movementSecure == MovementSecure.ServerAuthoritative)
            {
                float rtt = s_timestampToUnityTimeMultiplier * Entity.Manager.Rtt;
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

        private void RotateY()
        {
            EntityMovement.RotateY(_yAngle);
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

        private float CalculateMaxFallVelocity()
        {
            return maxFallVelocity * Entity.GetGravityRate();
        }

        private float CalculateGravity()
        {
            return gravity * Entity.GetGravityRate();
        }

        private float CalculateJumpVerticalSpeed()
        {
            // From the jump height and gravity we deduce the upwards speed 
            // for the character to reach at the apex.
            return Mathf.Sqrt(2f * (jumpHeight + Entity.GetJumpHeight()) * CalculateGravity());
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == PhysicLayers.Water)
            {
                // Enter water
                _waterCollider = other;
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == PhysicLayers.Water)
            {
                // Exit water
                _waterCollider = null;
            }
        }

        public void OnControllerColliderHit(Vector3 hitPoint, Transform hitTransform)
        {
            if (_isGrounded)
            {
                if (CacheTransform.position.y >= hitPoint.y)
                {
                    _groundedTransform = hitTransform;
                    _previousPlatformPosition = _groundedTransform.position;
                    return;
                }
            }
            _groundedTransform = null;
        }

        public bool WriteClientState(long writeTimestamp, NetDataWriter writer, out bool shouldSendReliably)
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
                if (_sendingDash)
                {
                    shouldSendReliably = true;
                    MovementState |= MovementState.IsDash;
                }
                else
                {
                    MovementState &= ~MovementState.IsDash;
                }
                if (_isClientConfirmingTeleport)
                {
                    shouldSendReliably = true;
                    MovementState |= MovementState.IsTeleport;
                }
                Entity.ClientWriteSyncTransform3D(writer);
                _sendingJump = false;
                _sendingDash = false;
                _isClientConfirmingTeleport = false;
                return true;
            }
            if (movementSecure == MovementSecure.ServerAuthoritative && IsOwnerClient && !IsServer)
            {
                _currentInput = Entity.SetInputExtraMovementState(_currentInput, _tempExtraMovementState);
                if (_sendingJump)
                {
                    shouldSendReliably = true;
                    _currentInput = Entity.SetInputJump(_currentInput);
                }
                else
                {
                    _currentInput = Entity.ClearInputJump(_currentInput);
                }
                if (_sendingDash)
                {
                    shouldSendReliably = true;
                    _currentInput = Entity.SetInputDash(_currentInput);
                }
                else
                {
                    _currentInput = Entity.ClearInputDash(_currentInput);
                }
                if (_isClientConfirmingTeleport)
                {
                    shouldSendReliably = true;
                    _currentInput.MovementState |= MovementState.IsTeleport;
                }
                if (Entity.DifferInputEnoughToSend(_oldInput, _currentInput, out EntityMovementInputState inputState))
                {
                    if (!_currentInput.IsKeyMovement)
                    {
                        // Point click should be reliably
                        shouldSendReliably = true;
                    }
                    if (inputState.Has(EntityMovementInputState.IsStopped))
                    {
                        // Stop should be reliably
                        shouldSendReliably = true;
                    }
                    Entity.ClientWriteMovementInput3D(writer, inputState, _currentInput);
                    _sendingJump = false;
                    _sendingDash = false;
                    _isClientConfirmingTeleport = false;
                    _oldInput = _currentInput;
                    _currentInput = null;
                    return true;
                }
            }
            return false;
        }

        public bool WriteServerState(long writeTimestamp, NetDataWriter writer, out bool shouldSendReliably)
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
            if (_sendingDash)
            {
                shouldSendReliably = true;
                MovementState |= MovementState.IsDash;
            }
            else
            {
                MovementState &= ~MovementState.IsDash;
            }
            if (_isTeleporting)
            {
                shouldSendReliably = true;
                if (_stillMoveAfterTeleport)
                    MovementState |= MovementState.IsTeleport;
                else
                    MovementState = MovementState.IsTeleport;
            }
            else
            {
                MovementState &= ~MovementState.IsTeleport;
            }
            Entity.ServerWriteSyncTransform3D(_movementForceAppliers, writer);
            _sendingJump = false;
            _sendingDash = false;
            _isTeleporting = false;
            _stillMoveAfterTeleport = false;
            return true;
        }

        public void ReadClientStateAtServer(long peerTimestamp, NetDataReader reader)
        {
            switch (movementSecure)
            {
                case MovementSecure.NotSecure:
                    ReadSyncTransformAtServer(peerTimestamp, reader);
                    break;
                case MovementSecure.ServerAuthoritative:
                    ReadMovementInputAtServer(peerTimestamp, reader);
                    break;
            }
        }

        public void ReadServerStateAtClient(long peerTimestamp, NetDataReader reader)
        {
            if (IsServer)
            {
                // Don't read and apply transform, because it was done at server
                return;
            }
            reader.ClientReadSyncTransformMessage3D(out MovementState movementState, out ExtraMovementState extraMovementState, out Vector3 position, out float yAngle, out List<EntityMovementForceApplier> movementForceAppliers);
            _movementForceAppliers.Clear();
            _movementForceAppliers.AddRange(movementForceAppliers);
            if (movementState.Has(MovementState.IsTeleport))
            {
                // Server requested to teleport
                OnTeleport(position, yAngle, movementState != MovementState.IsTeleport);
            }
            else if (_acceptedPositionTimestamp <= peerTimestamp)
            {
                // Prepare time
                long lagDeltaTime = Entity.Manager.Rtt;
                long deltaTime = lagDeltaTime + peerTimestamp - _acceptedPositionTimestamp;
                float unityDeltaTime = (float)deltaTime * s_timestampToUnityTimeMultiplier;
                if (Vector3.Distance(position, CacheTransform.position) >= snapThreshold)
                {
                    // Snap character to the position if character is too far from the position
                    if (movementSecure == MovementSecure.ServerAuthoritative || !IsOwnerClient)
                    {
                        CacheTransform.position = position;
                        CurrentGameManager.ShouldPhysicSyncTransforms = true;
                        RemoteTurnSimulation(true, yAngle, unityDeltaTime);
                    }
                    MovementState = _tempMovementState = movementState;
                    ExtraMovementState = _tempExtraMovementState = extraMovementState;
                }
                else if (!IsOwnerClient)
                {
                    RemoteTurnSimulation(true, yAngle, unityDeltaTime);
                    _simulatingKeyMovement = true;
                    if (Vector3.Distance(position.GetXZ(), CacheTransform.position.GetXZ()) > s_minDistanceToSimulateMovement)
                        SetMovePaths(position, false);
                    else
                        NavPaths = null;
                    MovementState = _tempMovementState = movementState;
                    ExtraMovementState = _tempExtraMovementState = extraMovementState;
                }
                _acceptedPositionTimestamp = peerTimestamp;
            }
            if (!IsOwnerClient && movementState.Has(MovementState.IsJump))
            {
                _acceptedJump = true;
            }
            if (!IsOwnerClient && movementState.Has(MovementState.IsDash))
            {
                _acceptedDash = true;
                TurnImmediately(yAngle);
            }
        }

        public void ReadMovementInputAtServer(long peerTimestamp, NetDataReader reader)
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
            reader.ReadMovementInputMessage3D(out EntityMovementInputState inputState, out EntityMovementInput entityMovementInput);
            if (entityMovementInput.MovementState.Has(MovementState.IsTeleport))
            {
                // Teleport confirming from client
                _isServerWaitingTeleportConfirm = false;
            }
            if (_isServerWaitingTeleportConfirm)
            {
                // Waiting for teleport confirming
                return;
            }
            if (Mathf.Abs(peerTimestamp - BaseGameNetworkManager.Singleton.ServerTimestamp) > s_lagBuffer)
            {
                // Timestamp is a lot difference to server's timestamp, player might try to hack a game or packet may corrupted occurring, so skip it
                return;
            }
            if (!Entity.CanMove())
            {
                // It can't move, so don't move
                return;
            }
            if (_acceptedPositionTimestamp <= peerTimestamp)
            {
                // Prepare time
                long lagDeltaTime = Entity.Player.Rtt;
                long deltaTime = lagDeltaTime + peerTimestamp - _acceptedPositionTimestamp;
                float unityDeltaTime = (float)deltaTime * s_timestampToUnityTimeMultiplier;
                _tempMovementState = entityMovementInput.MovementState;
                _tempExtraMovementState = entityMovementInput.ExtraMovementState;
                _simulatingKeyMovement = inputState.Has(EntityMovementInputState.IsKeyMovement);
                if (inputState.Has(EntityMovementInputState.PositionChanged))
                {
                    if (!UseRootMotion() || entityMovementInput.MovementState.HasDirectionMovement())
                        SetMovePaths(entityMovementInput.Position, !_simulatingKeyMovement);
                }
                if (inputState.Has(EntityMovementInputState.RotationChanged))
                {
                    RemoteTurnSimulation(_simulatingKeyMovement, entityMovementInput.YAngle, unityDeltaTime);
                }
                if (entityMovementInput.MovementState.Has(MovementState.IsJump))
                {
                    _acceptedJump = true;
                }
                if (entityMovementInput.MovementState.Has(MovementState.IsDash))
                {
                    _acceptedDash = true;
                    if (_remoteTargetYAngle.HasValue)
                        TurnImmediately(_remoteTargetYAngle.Value);
                    else
                        TurnImmediately(_targetYAngle);
                }
                if (inputState.Has(EntityMovementInputState.IsStopped))
                {
                    StopMoveFunction();
                }
                _acceptedPositionTimestamp = peerTimestamp;
            }
        }

        public float GetHorizontalMoveSpeed(MovementState movementState, ExtraMovementState extraMovementState)
        {
            if ((!movementState.Has(MovementState.IsGrounded) || movementState.Has(MovementState.IsJump)) && doNotChangeVelocityWhileAirborne)
                return _lastServerHorSpdBeforeAirborne;
            _lastServerHorSpdBeforeAirborne = Entity.GetMoveSpeed(movementState, extraMovementState);
            return _lastServerHorSpdBeforeAirborne;
        }

        public float GetVericalMoveSpeed(bool falling)
        {
            return falling ? CalculateMaxFallVelocity() : CalculateJumpVerticalSpeed();
        }

        public void ReadSyncTransformAtServer(long peerTimestamp, NetDataReader reader)
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
            reader.ServerReadSyncTransformMessage3D(out MovementState movementState, out ExtraMovementState extraMovementState, out Vector3 position, out float yAngle);
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
            if (Mathf.Abs(peerTimestamp - BaseGameNetworkManager.Singleton.ServerTimestamp) > s_lagBuffer)
            {
                // Timestamp is a lot difference to server's timestamp, player might try to hack a game or packet may corrupted occurring, so skip it
                return;
            }
            if (_acceptedPositionTimestamp <= peerTimestamp)
            {
                // Prepare time
                long lagDeltaTime = Entity.Player.Rtt;
                long deltaTime = lagDeltaTime + peerTimestamp - _acceptedPositionTimestamp;
                float unityDeltaTime = (float)deltaTime * 0.001f;
                // Prepare movement state
                MovementState = _tempMovementState = movementState;
                ExtraMovementState = _tempExtraMovementState = extraMovementState;
                // Skip simulation because while playing root motion animation it will also move charcter by root motion at server too, can determined that it is simulated
                if (!IsClient)
                {
                    // If it is not a client, don't have to simulate movement, just set the position (but still simulate gravity)
                    Vector3 oldPos = CacheTransform.position;
                    Vector3 newPos = position;
                    bool falling = newPos.y < oldPos.y;
                    // Calculate moveable distance
                    float horMoveSpd = GetHorizontalMoveSpeed(movementState, extraMovementState);
                    float horMoveableDist = (float)horMoveSpd * unityDeltaTime;
                    if (horMoveableDist < 0.001f)
                        horMoveableDist = 0.001f;
                    // Calculate jump/fall distance
                    float verMoveSpd = GetVericalMoveSpeed(falling);
                    float verMoveableDist = (float)verMoveSpd * unityDeltaTime;
                    if (verMoveableDist < 0.001f)
                        verMoveableDist = 0.001f;
                    // Set current move speed, in-case someone may use it for UIs
                    CurrentMoveSpeed = horMoveSpd;
                    // Movement validating, if it is valid, set the position follow the client, if not set position to proper one and tell client to teleport
                    float clientHorMoveDist = Vector3.Distance(oldPos.GetXZ(), newPos.GetXZ());
                    float clientVerMoveDist = Mathf.Abs(newPos.y - oldPos.y);
                    bool skipValidation = UseRootMotion();
                    if (Entity.SkipMovementValidation)
                        skipValidation = true;
                    if (skipValidation || ((clientHorMoveDist <= 0.001f || clientHorMoveDist <= horMoveableDist + _lastServerValidateHorDistDiff) && (clientVerMoveDist <= 0.001f || clientVerMoveDist <= verMoveableDist + _lastServerValidateVerDistDiff)))
                    {
                        // Allow to move to the position
                        CacheTransform.position = newPos;
                        CurrentGameManager.ShouldPhysicSyncTransforms = true;
                        // Update character rotation
                        _lastServerValidateHorDistDiff = horMoveableDist - clientHorMoveDist;
                        _lastServerValidateVerDistDiff = verMoveableDist - clientVerMoveDist;
                        RemoteTurnSimulation(true, yAngle, unityDeltaTime);
                    }
                    else
                    {
                        // Client moves too fast, adjust it
                        Vector3 dir = (newPos.GetXZ() - oldPos.GetXZ()).normalized;
                        Vector3 deltaMove = (dir * Mathf.Min(clientHorMoveDist, horMoveableDist)) + ((falling ? Vector3.down : Vector3.up) * Mathf.Min(clientVerMoveDist, verMoveableDist));
                        newPos = oldPos + deltaMove;
                        // And also adjust client's position
                        Teleport(newPos, Quaternion.Euler(0f, _yAngle, 0f), true);
                        // Reset distance difference
                        _lastServerValidateHorDistDiff = 0f;
                        _lastServerValidateVerDistDiff = 0f;
                    }
                }
                else
                {
                    // It's both server and client, simulate movement (it's a host so don't do speed hack validation)
                    if (Vector3.Distance(position, CacheTransform.position) > s_minDistanceToSimulateMovement)
                    {
                        _simulatingKeyMovement = true;
                        SetMovePaths(position, false);
                    }
                    RemoteTurnSimulation(true, yAngle, unityDeltaTime);
                }
                if (movementState.Has(MovementState.IsJump))
                {
                    _acceptedJump = true;
                }
                if (movementState.Has(MovementState.IsDash))
                {
                    _acceptedDash = true;
                    TurnImmediately(yAngle);
                }
                _acceptedPositionTimestamp = peerTimestamp;
            }
        }

        private void OnTeleport(Vector3 position, float yAngle, bool stillMoveAfterTeleport)
        {
            if (!stillMoveAfterTeleport)
                NavPaths = null;
            _airborneElapsed = 0;
            _verticalVelocity = 0;
            CacheTransform.position = position;
            CurrentGameManager.ShouldPhysicSyncTransforms = true;
            TurnImmediately(yAngle);
            if (IsServer && !IsOwnedByServer)
                _isServerWaitingTeleportConfirm = true;
            if (!IsServer && IsOwnerClient)
                _isClientConfirmingTeleport = true;
            _lastTeleportFrame = Time.frameCount;
            _previousPosition = CacheTransform.position;
        }

        public bool CanPredictMovement()
        {
            return Entity.IsOwnerClient || (Entity.IsOwnerClientOrOwnedByServer && movementSecure == MovementSecure.NotSecure) || (Entity.IsServer && movementSecure == MovementSecure.ServerAuthoritative);
        }

        public bool UseRootMotion()
        {
            return alwaysUseRootMotion || useRootMotionForMovement || useRootMotionForAirMovement || useRootMotionForJump || useRootMotionForFall || useRootMotionUnderWater || useRootMotionClimbing;
        }

        public void RemoteTurnSimulation(bool isKeyMovement, float yAngle, float deltaTime)
        {
            if (UseRootMotion())
            {
                if (isKeyMovement)
                {
                    // Turn to target immediately
                    TurnImmediately(yAngle);
                }
                else
                {
                    // Turn later after moved
                    _remoteTargetYAngle = yAngle;
                }
                return;
            }

            if (!IsClient)
            {
                // Turn to target immediately
                TurnImmediately(yAngle);
                return;
            }
            // Will turn smoothly later
            _targetYAngle = yAngle;
            _yTurnSpeed = 1f / deltaTime;
        }

        public void TurnImmediately(float yAngle)
        {
            _yAngle = _targetYAngle = yAngle;
            RotateY();
        }

        public void ApplyRemoteTurnAngle()
        {
            if (_remoteTargetYAngle.HasValue)
            {
                _targetYAngle = _remoteTargetYAngle.Value;
                _remoteTargetYAngle = null;
            }
        }
    }
}