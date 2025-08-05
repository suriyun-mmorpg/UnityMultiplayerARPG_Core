using Cysharp.Threading.Tasks;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MultiplayerARPG
{
    public partial class BuiltInSimpleEntityMovementFunctions3D
    {
        private static readonly int s_forceGroundedFramesAfterTeleport = 3;
        private static readonly float s_minDistanceToSimulateMovement = 0.01f;
        private static readonly float s_timestampToUnityTimeMultiplier = 0.001f;

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
        public float groundedVerticalVelocity = 0f;
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
        public EntityMovementForceApplierData dashingForceApplier;

        [Header("Root Motion Settings")]
        public bool alwaysUseRootMotion;
        public bool useRootMotionForMovement;
        public bool useRootMotionForAirMovement;
        public bool useRootMotionForJump;
        public bool useRootMotionForFall;
        public bool useRootMotionUnderWater;
        public bool useRootMotionClimbing;
        public float rootMotionGroundedVerticalVelocity = 0f;

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
        public LiteNetLibTransform NetworkedTransform { get; protected set; }
        public IBuiltInEntityMovement3D EntityMovement { get; private set; }
        public IEntityTeleportPreparer TeleportPreparer { get; private set; }
        public bool IsPreparingToTeleport { get { return TeleportPreparer != null && TeleportPreparer.IsPreparingToTeleport; } }

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
        public bool IsGrounded { get; private set; } = true;
        public bool IsAirborne { get; private set; } = false;
        public bool IsUnderWater { get; private set; } = false;
        public bool IsClimbing { get; private set; } = false;

        // Input codes
        private bool _isJumping;
        private bool _isDashing;
        private Vector3 _inputDirection;
        private MovementState _tempMovementState;
        private ExtraMovementState _tempExtraMovementState;

        // Teleportation
        private int _lastTeleportFrame;
        private MovementTeleportState _serverTeleportState;
        private MovementTeleportState _clientTeleportState;

        // State simulate codes
        private float? _lagMoveSpeedRate;
        private float _verticalVelocity;
        private Vector3 _velocityBeforeAirborne;
        private Collider _waterCollider;
        private byte _underWaterFrameCount;
        private Transform _groundedTransform;
        private Vector3 _previousPlatformPosition;
        private Vector3 _previousPosition;
        private Vector3 _previousMovement;
        private bool _previouslyGrounded = false;
        private bool _previouslyAirborne = false;
        private bool _simulatingKeyMovement = false;
        private ExtraMovementState _previouslyExtraMovementState;

        // Move simulate codes
        private float _pauseMovementCountDown;
        private Vector3 _moveDirection;

        // Force simulation
        private readonly List<EntityMovementForceApplier> _movementForceAppliers = new List<EntityMovementForceApplier>();
        private IEntityMovementForceUpdateListener[] _forceUpdateListeners;

        // Jump simulate codes
        private bool _applyingJumpForce;
        private float _applyJumpForceCountDown;
        private ExtraMovementState _extraMovementStateWhenJump;

        // Turn simulate codes
        private bool _lookRotationApplied;
        private float _yAngle;
        private float _targetYAngle;
        private float _yTurnSpeed;
        private float? _remoteTargetYAngle;

        // Interpolation Data
        protected SortedList<uint, System.ValueTuple<MovementState, ExtraMovementState>> _interpExtra = new SortedList<uint, System.ValueTuple<MovementState, ExtraMovementState>>();

        public BuiltInSimpleEntityMovementFunctions3D(BaseGameEntity entity, Animator animator, IBuiltInEntityMovement3D entityMovement)
        {
            Entity = entity;
            LadderComponent = entity.GetComponent<CharacterLadderComponent>();
            NetworkedTransform = entity.GetComponent<LiteNetLibTransform>();
            NetworkedTransform.syncByOwnerClient = true;
            NetworkedTransform.onWriteSyncBuffer += NetworkedTransform_onWriteSyncBuffer;
            NetworkedTransform.onReadInterpBuffer += NetworkedTransform_onReadInterpBuffer;
            NetworkedTransform.onInterpolate += NetworkedTransform_onInterpolate;
            TeleportPreparer = entity.GetComponent<IEntityTeleportPreparer>();
            Animator = animator;
            EntityMovement = entityMovement;
            _forceUpdateListeners = entity.GetComponents<IEntityMovementForceUpdateListener>();
            _yAngle = _targetYAngle = CacheTransform.eulerAngles.y;
            _lookRotationApplied = true;
        }

        public void EntityStart()
        {
            _clientTeleportState = MovementTeleportState.Responding;
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

        private void NetworkedTransform_onWriteSyncBuffer(NetDataWriter writer, uint tick)
        {
            writer.Put((byte)MovementState);
            writer.Put((byte)ExtraMovementState);
        }

        private void NetworkedTransform_onReadInterpBuffer(NetDataReader reader, uint tick)
        {
            _interpExtra[tick] = new System.ValueTuple<MovementState, ExtraMovementState>(
                (MovementState)reader.GetByte(),
                (ExtraMovementState)reader.GetByte());
            while (_interpExtra.Count > 30)
            {
                _interpExtra.RemoveAt(0);
            }
        }

        private void NetworkedTransform_onInterpolate(LiteNetLibTransform.TransformData interpFromData, LiteNetLibTransform.TransformData interpToData, float interpTime)
        {
            if (interpTime <= 0.75f)
            {
                if (_interpExtra.TryGetValue(interpFromData.Tick, out var states))
                {
                    MovementState = states.Item1;
                    ExtraMovementState = states.Item2;
                }
            }
            else
            {
                if (_interpExtra.TryGetValue(interpToData.Tick, out var states))
                {
                    MovementState = states.Item1;
                    ExtraMovementState = states.Item2;
                }
            }
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
            if (!CanSimulateMovement())
                return;
            // Always apply movement to owner client (it's client prediction for server auth movement)
            _inputDirection = moveDirection;
            _tempMovementState = movementState;
            if (_inputDirection.sqrMagnitude > 0)
                NavPaths = null;
            if (!_isJumping)
                _isJumping = _tempMovementState.Has(MovementState.IsJump);
            if (!_isDashing)
                _isDashing = _tempMovementState.Has(MovementState.IsDash);
        }

        public void PointClickMovement(Vector3 position)
        {
            if (!Entity.CanMove())
                return;
            if (!CanSimulateMovement())
                return;
            SetMovePaths(position, true);
        }

        public void SetExtraMovementState(ExtraMovementState extraMovementState)
        {
            if (!Entity.CanMove())
                return;
            if (!CanSimulateMovement())
                return;
            if (_isJumping)
                return;
            _tempExtraMovementState = extraMovementState;
        }

        public void SetLookRotation(Quaternion rotation, bool immediately)
        {
            if (!Entity.CanTurn())
                return;
            if (!CanSimulateMovement())
                return;
            _targetYAngle = rotation.eulerAngles.y;
            if (LadderComponent && LadderComponent.ClimbingLadder)
            {
                // Turn to the ladder
                _targetYAngle = Quaternion.LookRotation(-LadderComponent.ClimbingLadder.ForwardWithYAngleOffsets).eulerAngles.y;
            }
            _lookRotationApplied = false;
            if (immediately)
                TurnImmediately(_targetYAngle);
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

        public async void Teleport(Vector3 position, Quaternion rotation, bool stillMoveAfterTeleport)
        {
            if (!IsServer)
            {
                Logging.LogWarning(nameof(BuiltInEntityMovementFunctions3D), $"Teleport function shouldn't be called at client [{Entity.name}]");
                return;
            }
            await OnTeleport(position, rotation, stillMoveAfterTeleport);
        }

        public void ApplyForce(ApplyMovementForceMode mode, Vector3 direction, ApplyMovementForceSourceType sourceType, int sourceDataId, int sourceLevel, float force, float deceleration, float duration)
        {
            if (!IsServer)
                return;
            if (mode.IsReplaceMovement())
            {
                // Can have only one replace movement force applier, so remove stored ones
                _movementForceAppliers.RemoveReplaceMovementForces();
            }
            _movementForceAppliers.Add(new EntityMovementForceApplier()
                .Apply(mode, direction, sourceType, sourceDataId, sourceLevel, force, deceleration, duration));
        }

        public EntityMovementForceApplier FindForceByActionKey(ApplyMovementForceSourceType sourceType, int sourceDataId)
        {
            return _movementForceAppliers.FindBySource(sourceType, sourceDataId);
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
            bool isUnderWater = Entity.EntityTransform.position.y < TargetWaterSurfaceY(waterCollider) + 0.01f;
            if (isUnderWater)
            {
                if (_underWaterFrameCount < 3)
                    _underWaterFrameCount++;
                return _underWaterFrameCount >= 3;
            }
            else
            {
                _underWaterFrameCount = 0;
                return false;
            }
        }

        public float TargetWaterSurfaceY(Collider waterCollider)
        {
            Bounds movementBounds = EntityMovement.GetMovementBounds();
            float result = waterCollider.bounds.max.y - (underWaterThreshold * movementBounds.size.y);
            return result;
        }

        public void UpdateMovement(float deltaTime)
        {
            if (!CanSimulateMovement())
                return;

            _moveDirection = Vector3.zero;
            IsUnderWater = WaterCheck(_waterCollider);
            IsClimbing = LadderComponent && LadderComponent.ClimbingLadder;
            IsGrounded = EntityMovement.GroundCheck();
            IsAirborne = !IsGrounded && !IsUnderWater && !IsClimbing && EntityMovement.AirborneCheck();

            // Underwater state, movement state must be setup here to make it able to calculate move speed properly
            if (IsClimbing)
                _tempMovementState |= MovementState.IsClimbing;
            else if (IsUnderWater)
                _tempMovementState |= MovementState.IsUnderWater;

            if (IsAirborne || IsClimbing || IsUnderWater)
            {
                if (_tempExtraMovementState == ExtraMovementState.IsCrouching || _tempExtraMovementState == ExtraMovementState.IsCrawling)
                    _tempExtraMovementState = ExtraMovementState.None;
            }

            if (IsClimbing)
                UpdateClimbMovement(deltaTime);
            else
                UpdateGenericMovement(deltaTime);
        }

        protected void UpdateClimbMovement(float deltaTime)
        {
            if (IsPreparingToTeleport)
                return;

            Vector3 tempPredictPosition;
            Vector3 tempCurrentPosition = CacheTransform.position;
            // Prepare movement speed
            _tempExtraMovementState = Entity.ValidateExtraMovementState(_tempMovementState, _tempExtraMovementState);
            float tempEntityMoveSpeed = Entity.GetMoveSpeed(_tempMovementState, _tempExtraMovementState);
            float tempMaxMoveSpeed = tempEntityMoveSpeed;
            CurrentMoveSpeed = tempMaxMoveSpeed;

            float currentTime = Time.unscaledTime;
            Vector3 tempMoveVelocity;
            switch (LadderComponent.EnterExitState)
            {
                case EnterExitState.Enter:
                case EnterExitState.Exit:
                    // Enter or exit
                    Vector3 tempPosition;
                    if (LadderComponent.EnterOrExitDuration > 0f)
                        tempPosition = Vector3.Lerp(LadderComponent.EnterOrExitFromPosition, LadderComponent.EnterOrExitToPosition, currentTime - LadderComponent.EnterOrExitTime / LadderComponent.EnterOrExitDuration);
                    else
                        tempPosition = LadderComponent.EnterOrExitToPosition;
                    tempMoveVelocity = GetVelocityForMovePosition(tempCurrentPosition, tempPosition, deltaTime);
                    break;
                case EnterExitState.ConfirmAwaiting:
                    tempMoveVelocity = Vector3.zero;
                    break;
                default:
                    if (_tempMovementState.Has(MovementState.Up))
                        _moveDirection.y = 1f;
                    else if (_tempMovementState.Has(MovementState.Down))
                        _moveDirection.y = -1f;

                    if (Mathf.Approximately(_moveDirection.y, 0f))
                        return;

                    tempMoveVelocity = GetVelocityForMovePosition(tempCurrentPosition,
                        LadderComponent.ClimbingLadder.ClosestPointOnLadderSegment(tempCurrentPosition, EntityMovement.GetMovementBounds().extents.z, out float segmentState), deltaTime) +
                        LadderComponent.ClimbingLadder.Up * _moveDirection.y * CurrentMoveSpeed;

                    if (Mathf.Abs(segmentState) > 0.05f)
                    {
                        if (segmentState > 0 && _moveDirection.y > 0f)
                        {
                            // Exit (top)
                            //tempMoveVelocity = GetVelocityForMovePosition(tempCurrentPosition, LadderComponent.ClimbingLadder.topExitTransform.position, deltaTime);

                            LadderComponent.EnterExitState = EnterExitState.ConfirmAwaiting;
                            LadderComponent.CallCmdExitLadder(LadderEntranceType.Top);
                        }
                        // If we're lower than the ladder bottom point
                        else if (segmentState < 0 && _moveDirection.y < 0f)
                        {
                            // Exit (bottom)
                            //tempMoveVelocity = GetVelocityForMovePosition(tempCurrentPosition, LadderComponent.ClimbingLadder.bottomExitTransform.position, deltaTime);

                            LadderComponent.EnterExitState = EnterExitState.ConfirmAwaiting;
                            LadderComponent.CallCmdExitLadder(LadderEntranceType.Bottom);
                        }
                    }
                    break;
            }
            // Move
            tempPredictPosition = tempCurrentPosition + (tempMoveVelocity * deltaTime);
            _previousMovement = tempMoveVelocity * deltaTime;
            EntityMovement.Move(_tempMovementState, _tempExtraMovementState, _previousMovement, deltaTime);
        }

        protected void UpdateGenericMovement(float deltaTime)
        {
            if (IsPreparingToTeleport)
                return;

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
                _isDashing = false;
            }

            if (_isJumping && (_applyingJumpForce || !Entity.CanJump() || !Entity.AllowToJump()))
            {
                _isJumping = false;
            }

            if (_isDashing && (!Entity.CanDash() || !Entity.AllowToDash()))
            {
                _isDashing = false;
            }

            // Prepare movement speed
            _tempExtraMovementState = Entity.ValidateExtraMovementState(_tempMovementState, _tempExtraMovementState);
            float tempEntityMoveSpeed = _applyingJumpForce ? 0f : Entity.GetMoveSpeed(_tempMovementState, _tempExtraMovementState);
            float tempMaxMoveSpeed = tempEntityMoveSpeed;

            // Calculate vertical velocity by gravity
            if (!IsGrounded && !IsUnderWater)
            {
                if (!useRootMotionForFall && !forceUseRootMotion)
                {
                    float gravity = CalculateGravity();
                    float maxFallVelocity = CalculateMaxFallVelocity();
                    _verticalVelocity -= gravity * deltaTime;
                    if (_verticalVelocity < -maxFallVelocity)
                        _verticalVelocity = -maxFallVelocity;
                }
                else
                {
                    _verticalVelocity = rootMotionGroundedVerticalVelocity;
                }
            }
            else
            {
                // Not falling set verical velocity to 0
                _verticalVelocity = groundedVerticalVelocity;
            }

            // Jumping
            if (_pauseMovementCountDown <= 0f && _isJumping)
            {
                _extraMovementStateWhenJump = _tempExtraMovementState;
                if (CanSimulateMovement())
                    Entity.CallRpcPlayJumpAnimation();
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
                    IsGrounded = false;
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
            if (!IsAirborne)
                _velocityBeforeAirborne = Vector3.zero;

            // Dashing
            if (_pauseMovementCountDown <= 0f && IsGrounded && _isDashing)
            {
                // Can have only one replace movement force applier, so remove stored ones
                _movementForceAppliers.RemoveReplaceMovementForces();
                _movementForceAppliers.Add(new EntityMovementForceApplier().Apply(
                    ApplyMovementForceMode.Dash, CacheTransform.forward, ApplyMovementForceSourceType.None, 0, 0, dashingForceApplier));
            }

            // Apply Forces
            _forceUpdateListeners.OnPreUpdateForces(_movementForceAppliers);
            _movementForceAppliers.UpdateForces(deltaTime,
                Entity.GetMoveSpeed(MovementState.Forward, ExtraMovementState.None),
                out Vector3 forceMotion, out EntityMovementForceApplier replaceMovementForceApplier);
            _forceUpdateListeners.OnPostUpdateForces(_movementForceAppliers);

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
            if (_pauseMovementCountDown <= 0f && _moveDirection.sqrMagnitude > 0f && (!IsAirborne || !doNotChangeVelocityWhileAirborne || !IsOwnerClientOrOwnedByServer))
            {
                // Calculate only horizontal move direction
                tempHorizontalMoveDirection = _moveDirection;
                tempHorizontalMoveDirection.y = 0;
                tempHorizontalMoveDirection.Normalize();

                // If character move backward
                if (Vector3.Angle(tempHorizontalMoveDirection, CacheTransform.forward) > 120)
                    tempMaxMoveSpeed *= backwardMoveSpeedRate;
                CurrentMoveSpeed = tempMaxMoveSpeed;

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
            }

            // Pause movement updating
            if (IsOwnerClientOrOwnedByServer)
            {
                if (IsGrounded && _previouslyAirborne)
                {
                    _pauseMovementCountDown = landedPauseMovementDuration;
                }
                else if (IsGrounded && _previouslyExtraMovementState != ExtraMovementState.IsCrawling && _tempExtraMovementState == ExtraMovementState.IsCrawling)
                {
                    _pauseMovementCountDown = beforeCrawlingPauseMovementDuration;
                }
                else if (IsGrounded && _previouslyExtraMovementState == ExtraMovementState.IsCrawling && _tempExtraMovementState != ExtraMovementState.IsCrawling)
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
                    _tempMovementState &= ~(MovementState.Forward | MovementState.Backward | MovementState.Left | MovementState.Right);
                }
            }

            // Move by velocity before jump
            if (IsAirborne && doNotChangeVelocityWhileAirborne)
                tempMoveVelocity = _velocityBeforeAirborne;
            // Updating vertical movement (Fall, WASD inputs under water)
            if (IsUnderWater)
            {
                CurrentMoveSpeed = tempMaxMoveSpeed;

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
                }
            }
            else
            {
                // Update velocity while not under water
                tempMoveVelocity.y = _verticalVelocity;
            }

            // Don't applies velocity while using root motion
            if ((IsGrounded && (forceUseRootMotion || useRootMotionForMovement)) ||
                (IsAirborne && (forceUseRootMotion || useRootMotionForAirMovement)) ||
                (!IsGrounded && !IsAirborne && (forceUseRootMotion || useRootMotionForMovement)) ||
                (IsUnderWater && (forceUseRootMotion || useRootMotionUnderWater)))
            {
                tempMoveVelocity.x = 0;
                tempMoveVelocity.z = 0;
            }

            Vector3 platformMotion = Vector3.zero;
            if (!IsUnderWater)
            {
                // Apply platform motion
                if (_groundedTransform != null)
                {
                    platformMotion = (_groundedTransform.position - _previousPlatformPosition) / deltaTime;
                    _previousPlatformPosition = _groundedTransform.position;
                }
            }
            Vector3 snapToGroundMotion = EntityMovement.GetSnapToGroundMotion(tempMoveVelocity, platformMotion, forceMotion);
            _previousMovement = ((tempMoveVelocity + platformMotion + forceMotion) * deltaTime) + snapToGroundMotion;
            if (Entity.IsOwnerClientOrOwnedByServer && LadderComponent &&
                LadderComponent.TriggeredLadderEntry && !LadderComponent.ClimbingLadder &&
                LadderComponent.EnterExitState == EnterExitState.None &&
                LadderComponent.EnterExitState != EnterExitState.ConfirmAwaiting &&
                tempHorizontalMoveDirection.sqrMagnitude > 0f)
            {
                Vector3 dirToLadder = (LadderComponent.TriggeredLadderEntry.TipTransform.position.GetXZ() - Entity.EntityTransform.position.GetXZ()).normalized;
                float angle = Vector3.Angle(tempHorizontalMoveDirection, dirToLadder);
                if (angle < 15f)
                {
                    LadderComponent.EnterExitState = EnterExitState.ConfirmAwaiting;
                    LadderComponent.CallCmdEnterLadder();
                }
            }
            EntityMovement.Move(_tempMovementState, _tempExtraMovementState, _previousMovement, deltaTime);
        }

        public void UpdateRotation(float deltaTime)
        {
            if (!CanSimulateMovement())
                return;

            if (_yTurnSpeed <= 0f)
                _yAngle = _targetYAngle;
            else if (Mathf.Abs(_yAngle - _targetYAngle) > 1f)
                _yAngle = Mathf.LerpAngle(_yAngle, _targetYAngle, _yTurnSpeed * deltaTime);
            _lookRotationApplied = true;
            RotateY();
        }

        public void AfterMovementUpdate(float deltaTime)
        {
            if (CanSimulateMovement())
            {
                // Re-setup movement state here to make sure it is correct
                _tempMovementState = _moveDirection.sqrMagnitude > 0f ? _tempMovementState : MovementState.None;
                if (IsUnderWater)
                    _tempMovementState |= MovementState.IsUnderWater;
                if (IsGrounded || !IsAirborne || Time.frameCount - _lastTeleportFrame < s_forceGroundedFramesAfterTeleport)
                    _tempMovementState |= MovementState.IsGrounded;
                if (_isJumping)
                    _tempMovementState |= MovementState.IsJump;
                // Update movement state
                MovementState = _tempMovementState;
                // Update extra movement state
                ExtraMovementState = Entity.ValidateExtraMovementState(MovementState, _tempExtraMovementState);
                if (_isJumping || IsAirborne)
                    ExtraMovementState = _extraMovementStateWhenJump;
            }
            _previouslyGrounded = IsGrounded;
            _previouslyAirborne = IsAirborne;
            _previousPosition = CacheTransform.position;
            _previouslyExtraMovementState = ExtraMovementState;
            _isJumping = false;
        }

        public void FixSwimUpPosition(float deltaTime)
        {
            if (!CanSimulateMovement())
                return;

            if (!IsGrounded && IsUnderWater && _previousMovement.y > 0f)
            {
                Vector3 tempTargetPosition = Vector3.up * TargetWaterSurfaceY(_waterCollider);
                if (Mathf.Abs(CacheTransform.position.y - tempTargetPosition.y) < 0.05f)
                    EntityMovement.SetPosition(new Vector3(CacheTransform.position.x, tempTargetPosition.y, CacheTransform.position.z));
            }
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
            if (IsGrounded)
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
            if (_clientTeleportState.Has(MovementTeleportState.Responding))
            {
                shouldSendReliably = true;
                writer.Put((byte)_clientTeleportState);
                _clientTeleportState = MovementTeleportState.None;
                return true;
            }
            shouldSendReliably = false;
            return false;
        }

        public bool WriteServerState(long writeTimestamp, NetDataWriter writer, out bool shouldSendReliably)
        {
            if (_serverTeleportState.Has(MovementTeleportState.Requesting))
            {
                shouldSendReliably = true;
                writer.Put((byte)_serverTeleportState);
                writer.Put(Entity.EntityTransform.position.x);
                writer.Put(Entity.EntityTransform.position.y);
                writer.Put(Entity.EntityTransform.position.z);
                writer.PutPackedUShort(Mathf.FloatToHalf(Entity.EntityTransform.eulerAngles.y));
                _serverTeleportState = MovementTeleportState.WaitingForResponse;
                return true;
            }
            shouldSendReliably = false;
            return false;
        }

        public void ReadClientStateAtServer(long peerTimestamp, NetDataReader reader)
        {
            MovementTeleportState movementTeleportState = (MovementTeleportState)reader.GetByte();
            if (movementTeleportState.Has(MovementTeleportState.Responding))
            {
                _serverTeleportState = MovementTeleportState.None;
                return;
            }
        }

        public async void ReadServerStateAtClient(long peerTimestamp, NetDataReader reader)
        {
            MovementTeleportState movementTeleportState = (MovementTeleportState)reader.GetByte();
            if (movementTeleportState.Has(MovementTeleportState.Requesting))
            {
                Vector3 position = new Vector3(
                    reader.GetFloat(),
                    reader.GetFloat(),
                    reader.GetFloat());
                float rotation = Mathf.HalfToFloat(reader.GetPackedUShort());
                bool stillMoveAfterTeleport = movementTeleportState.Has(MovementTeleportState.StillMoveAfterTeleport);
                await OnTeleport(position, Quaternion.Euler(0f, rotation, 0f), stillMoveAfterTeleport);
                return;
            }
        }

        private async UniTask OnTeleport(Vector3 position, Quaternion rotation, bool stillMoveAfterTeleport)
        {
            _verticalVelocity = 0;
            if (!stillMoveAfterTeleport)
                NavPaths = null;

            // Prepare teleporation states
            if (IsServer && !IsOwnedByServer)
            {
                _serverTeleportState = MovementTeleportState.Requesting;
                if (stillMoveAfterTeleport)
                    _serverTeleportState |= MovementTeleportState.StillMoveAfterTeleport;
                _serverTeleportState |= MovementTeleportState.WaitingForResponse;
            }
            if (!IsServer && IsOwnerClient)
            {
                _clientTeleportState = MovementTeleportState.Responding;
            }
            if (TeleportPreparer != null)
                await TeleportPreparer.PrepareToTeleport(position, rotation);

            // Move character to target position
            _verticalVelocity = 0;
            if (!stillMoveAfterTeleport)
                NavPaths = null;
            _lastTeleportFrame = Time.frameCount;
            EntityMovement.SetPosition(position);
            CurrentGameManager.ShouldPhysicSyncTransforms = true;
            TurnImmediately(rotation.eulerAngles.y);
            _previousPosition = CacheTransform.position;
        }

        public bool CanSimulateMovement()
        {
            return Entity.IsOwnerClientOrOwnedByServer;
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

        public async UniTask WaitClientTeleportConfirm()
        {
            while (this != null && _serverTeleportState.Has(MovementTeleportState.WaitingForResponse))
            {
                await UniTask.Delay(1000);
            }
        }
    }
}
