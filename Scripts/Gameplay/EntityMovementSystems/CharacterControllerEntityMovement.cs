using Cysharp.Threading.Tasks;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterController))]
    public partial class CharacterControllerEntityMovement : BaseNetworkedGameEntityComponent<BaseGameEntity>, IEntityMovementComponent, IBuiltInEntityMovement3D
    {
        /// <summary>
        /// Add some distant to avoid character falling under ground
        /// </summary>
        private const float ABOVE_GROUND_OFFSETS = 0.25f;

        [Header("Network Settings")]
        public MovementSecure movementSecure = MovementSecure.NotSecure;
        [Tooltip("If distance between current frame and previous frame is greater than this value, then it will determine that changes occurs and will sync transform later")]
        [Min(0.01f)]
        public float positionThreshold = 0.01f;
        [Tooltip("If angle between current frame and previous frame is greater than this value, then it will determine that changes occurs and will sync transform later")]
        [Min(0.01f)]
        public float eulerAnglesThreshold = 1f;
        [Tooltip("Keep alive ticks before it is stop syncing (after has no changes)")]
        public int keepAliveTicks = 10;
        [Tooltip("If distance between two interpolating positions more than this value, it will change position to target position immediately")]
        [Min(0.01f)]
        public float interpSnapThreshold = 2f;

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
        public float groundedVerticalVelocity = -2f;
        public float groundSnapDistance = 2f;
        public bool doNotChangeVelocityWhileAirborne;

        [Header("Pausing")]
        public float landedPauseMovementDuration = 0f;
        public float beforeCrawlingPauseMovementDuration = 0f;
        public float afterCrawlingPauseMovementDuration = 0f;

        [Header("Swimming")]
        [Range(0.1f, 1f)]
        public float underWaterThreshold = 0.75f;
        public bool autoSwimToSurface;

        [Header("Ground Checking")]
        public float groundCheckOffsets = 0.14f;
        public float groundCheckRadius = 0.28f;
#if UNITY_EDITOR
        public Color groundCheckGizmosColor = Color.blue;
#endif

        [Header("Airborne Checking")]
        public float airborneCheckOffsets = 0.14f;
        public float airborneCheckRadius = 0.28f;
#if UNITY_EDITOR
        public Color airborneCheckGizmosColor = Color.cyan;
#endif
        public float forceUngroundAfterJumpDuration = 0.1f;

        [Header("Crawl Checking")]
        public float crawlCheckOffsets = -0.14f;
        [Min(0)]
        public int crawlCheckRaycasts = 0;
        [Min(0f)]
        public float crawlCheckRadius = 1f;
#if UNITY_EDITOR
        public Color crawlCheckGizmosColor = Color.yellow;
#endif

        [Header("Dashing")]
        public EntityMovementForceApplierData dashingForceApplier = EntityMovementForceApplierData.CreateDefault();

        [Header("Root Motion Settings")]
        [FormerlySerializedAs("useRootMotionWhileNotMoving")]
        public bool alwaysUseRootMotion;
        public bool useRootMotionForMovement;
        public bool useRootMotionForAirMovement;
        public bool useRootMotionForJump;
        public bool useRootMotionForFall;
        public bool useRootMotionUnderWater;
        public bool useRootMotionClimbing;
        public float rootMotionGroundedVerticalVelocity = -2f;

        [Header("Networking Settings")]
        public float snapThreshold = 5.0f;

        protected Animator _cacheAnimator;
        public Animator CacheAnimator
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying && _cacheAnimator == null)
                {
                    _cacheAnimator = GetComponent<Animator>();
                    if (_cacheAnimator == null)
                        _cacheAnimator = GetComponentInChildren<Animator>();
                }
#endif
                return _cacheAnimator;
            }
            private set => _cacheAnimator = value;
        }
        protected CharacterController _cacheCharacterController;
        public CharacterController CacheCharacterController
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying && _cacheCharacterController == null)
                    _cacheCharacterController = GetComponent<CharacterController>();
#endif
                return _cacheCharacterController;
            }
            private set => _cacheCharacterController = value;
        }
        public BuiltInEntityMovementFunctions3D Functions { get; private set; }
        public MovementColliderAdjustment ColliderAdjustment { get; private set; }

        public float StoppingDistance { get { return Functions.StoppingDistance; } }
        public MovementState MovementState { get { return Functions.MovementState; } }
        public ExtraMovementState ExtraMovementState { get { return Functions.ExtraMovementState; } }
        public DirectionVector2 Direction2D { get { return Functions.Direction2D; } set { Functions.Direction2D = value; } }
        public float CurrentMoveSpeed { get { return Functions.CurrentMoveSpeed; } }
        public Queue<Vector3> NavPaths { get { return Functions.NavPaths; } }
        public bool HasNavPaths { get { return Functions.HasNavPaths; } }

        protected float _forceUngroundCountdown = 0f;
        protected int _allowToJumpOrDashCheckFrame = 0;
        protected bool _isAllowToJumpOrDash = true;

        public override void EntityAwake()
        {
            // Prepare animator component
            CacheAnimator = GetComponent<Animator>();
            if (CacheAnimator == null)
                CacheAnimator = GetComponentInChildren<Animator>();
            // Prepare character controller component
            CacheCharacterController = gameObject.GetOrAddComponent<CharacterController>();
            ColliderAdjustment = gameObject.GetComponent<MovementColliderAdjustment>();
            // Disable unused component
            LiteNetLibTransform disablingComp = gameObject.GetComponent<LiteNetLibTransform>();
            if (disablingComp != null)
            {
                Logging.LogWarning(nameof(CharacterControllerEntityMovement), "You can remove `LiteNetLibTransform` component from game entity, it's not being used anymore [" + name + "]");
                disablingComp.enabled = false;
            }
            Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
            if (rigidBody != null)
            {
                rigidBody.useGravity = false;
                rigidBody.isKinematic = true;
            }
            // Setup
            Functions = new BuiltInEntityMovementFunctions3D(Entity, CacheAnimator, this)
            {
                movementSecure = movementSecure,
                positionThreshold = positionThreshold,
                eulerAnglesThreshold = eulerAnglesThreshold,
                keepAliveTicks = keepAliveTicks,
                interpSnapThreshold = interpSnapThreshold,
                stoppingDistance = stoppingDistance,
                jumpHeight = jumpHeight,
                applyJumpForceMode = applyJumpForceMode,
                applyJumpForceFixedDuration = applyJumpForceFixedDuration,
                backwardMoveSpeedRate = backwardMoveSpeedRate,
                gravity = gravity,
                maxFallVelocity = maxFallVelocity,
                groundedVerticalVelocity = groundedVerticalVelocity,
                doNotChangeVelocityWhileAirborne = doNotChangeVelocityWhileAirborne,
                landedPauseMovementDuration = landedPauseMovementDuration,
                beforeCrawlingPauseMovementDuration = beforeCrawlingPauseMovementDuration,
                afterCrawlingPauseMovementDuration = afterCrawlingPauseMovementDuration,
                underWaterThreshold = underWaterThreshold,
                autoSwimToSurface = autoSwimToSurface,
                dashingForceApplier = dashingForceApplier,
                alwaysUseRootMotion = alwaysUseRootMotion,
                useRootMotionForMovement = useRootMotionForMovement,
                useRootMotionForAirMovement = useRootMotionForAirMovement,
                useRootMotionForJump = useRootMotionForJump,
                useRootMotionForFall = useRootMotionForFall,
                useRootMotionUnderWater = useRootMotionUnderWater,
                useRootMotionClimbing = useRootMotionClimbing,
                rootMotionGroundedVerticalVelocity = rootMotionGroundedVerticalVelocity,
                snapThreshold = snapThreshold,
            };
            Functions.StopMoveFunction();
        }

        public override void EntityStart()
        {
            Functions.EntityStart();
        }

        public override void ComponentOnEnable()
        {
            Functions.ComponentEnabled();
            CacheCharacterController.enabled = true;
        }

        public override void ComponentOnDisable()
        {
            CacheCharacterController.enabled = false;
        }

        public override void OnSetOwnerClient(bool isOwnerClient)
        {
            base.OnSetOwnerClient(isOwnerClient);
            Functions.OnSetOwnerClient(isOwnerClient);
        }

        public override void OnIdentityInitialize()
        {
            base.OnIdentityInitialize();
            Functions.OnIdentityInitialize();
        }

        public override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            Functions.EntityOnDestroy();
        }

        private void OnAnimatorMove()
        {
            Functions.OnAnimatorMove();
        }

        private void OnTriggerEnter(Collider other)
        {
            Functions.OnTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            Functions.OnTriggerExit(other);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Functions.OnControllerColliderHit(hit.point, hit.transform);
        }

        public override void EntityUpdate()
        {
#if UNITY_EDITOR
            Functions.movementSecure = movementSecure;
            Functions.positionThreshold = positionThreshold;
            Functions.eulerAnglesThreshold = eulerAnglesThreshold;
            Functions.keepAliveTicks = keepAliveTicks;
            Functions.interpSnapThreshold = interpSnapThreshold;
            Functions.stoppingDistance = stoppingDistance;
            Functions.jumpHeight = jumpHeight;
            Functions.applyJumpForceMode = applyJumpForceMode;
            Functions.applyJumpForceFixedDuration = applyJumpForceFixedDuration;
            Functions.backwardMoveSpeedRate = backwardMoveSpeedRate;
            Functions.gravity = gravity;
            Functions.maxFallVelocity = maxFallVelocity;
            Functions.doNotChangeVelocityWhileAirborne = doNotChangeVelocityWhileAirborne;
            Functions.landedPauseMovementDuration = landedPauseMovementDuration;
            Functions.beforeCrawlingPauseMovementDuration = beforeCrawlingPauseMovementDuration;
            Functions.afterCrawlingPauseMovementDuration = afterCrawlingPauseMovementDuration;
            Functions.underWaterThreshold = underWaterThreshold;
            Functions.autoSwimToSurface = autoSwimToSurface;
            Functions.alwaysUseRootMotion = alwaysUseRootMotion;
            Functions.dashingForceApplier = dashingForceApplier;
            Functions.useRootMotionForMovement = useRootMotionForMovement;
            Functions.useRootMotionForAirMovement = useRootMotionForAirMovement;
            Functions.useRootMotionForJump = useRootMotionForJump;
            Functions.useRootMotionForFall = useRootMotionForFall;
            Functions.useRootMotionUnderWater = useRootMotionUnderWater;
            Functions.snapThreshold = snapThreshold;
#endif
            float deltaTime = Time.deltaTime;
            Functions.EntityUpdate(deltaTime);
            if (_forceUngroundCountdown > 0f)
                _forceUngroundCountdown -= deltaTime;
        }

        public override void EntityLateUpdate()
        {
            float deltaTime = Time.deltaTime;
            Functions.FixSwimUpPosition(deltaTime);
        }

        public bool GroundCheck()
        {
            if (_forceUngroundCountdown > 0f)
                return false;
            return CacheCharacterController.isGrounded;
        }

        public bool AirborneCheck()
        {
            if (_forceUngroundCountdown > 0f)
                return true;
            return !Physics.CheckSphere(GetAirborneCheckCenter(), airborneCheckRadius, GameInstance.Singleton.GetGameEntityGroundDetectionLayerMask(), QueryTriggerInteraction.Ignore);
        }

        public virtual bool AllowToJump()
        {
            return AllowToJumpOrDash();
        }

        public virtual bool AllowToDash()
        {
            return AllowToJumpOrDash();
        }

        protected virtual bool AllowToJumpOrDash()
        {
            if (!AllowToStand())
                return false;
            if (Time.frameCount == _allowToJumpOrDashCheckFrame)
                return _isAllowToJumpOrDash;
            _allowToJumpOrDashCheckFrame = Time.frameCount;
            _isAllowToJumpOrDash = Physics.CheckSphere(GetGroundCheckCenter(), groundCheckRadius, GameInstance.Singleton.GetGameEntityGroundDetectionLayerMask(), QueryTriggerInteraction.Ignore);
            return _isAllowToJumpOrDash;
        }

        public bool AllowToCrouch()
        {
            if (ColliderAdjustment == null)
                return true;
            var settings = ColliderAdjustment.CrouchSettings;
            return this.AllowToChangePose(settings.height, settings.radius, GameInstance.Singleton.GetGameEntityGroundDetectionLayerMask());
        }

        public bool AllowToCrawl()
        {
            return this.AllowToCrawl(crawlCheckRaycasts, crawlCheckOffsets, crawlCheckRadius);
        }

        public bool AllowToStand()
        {
            if (ColliderAdjustment == null)
                return true;
            var settings = ColliderAdjustment.StandSettings;
            return this.AllowToChangePose(settings.height, settings.radius, GameInstance.Singleton.GetGameEntityGroundDetectionLayerMask());
        }

        private Vector3 AdjustCrawlMotion(MovementState movementState, ExtraMovementState extraMovementState, Vector3 motion)
        {
            return this.AdjustCrawlMotion(movementState, extraMovementState, motion, crawlCheckRaycasts, crawlCheckOffsets, crawlCheckRadius, GameInstance.Singleton.GetGameEntityGroundDetectionLayerMask());
        }

        protected Vector3 GetGroundCheckCenter()
        {
            return new Vector3(EntityTransform.position.x, EntityTransform.position.y - groundCheckOffsets, EntityTransform.position.z);
        }

        protected Vector3 GetAirborneCheckCenter()
        {
            return new Vector3(EntityTransform.position.x, EntityTransform.position.y - airborneCheckOffsets, EntityTransform.position.z);
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = groundCheckGizmosColor;
            Gizmos.DrawWireSphere(GetGroundCheckCenter(), groundCheckRadius);
            Gizmos.color = airborneCheckGizmosColor;
            Gizmos.DrawWireSphere(GetAirborneCheckCenter(), airborneCheckRadius);
            this.DrawCrawlCheckRaycasts(crawlCheckRaycasts, crawlCheckOffsets, crawlCheckRadius, crawlCheckGizmosColor);
            Gizmos.color = prevColor;
        }
#endif

        public void SetPosition(Vector3 position)
        {
            EntityTransform.position = position;
        }

        public Bounds GetMovementBounds()
        {
            return CacheCharacterController.bounds;
        }

        public void Move(MovementState movementState, ExtraMovementState extraMovementState, Vector3 motion, float deltaTime)
        {
            CacheCharacterController.Move(AdjustCrawlMotion(movementState, extraMovementState, motion));
        }

        public void RotateY(float yAngle)
        {
            EntityTransform.eulerAngles = new Vector3(0f, yAngle, 0f);
        }

        public void OnJumpForceApplied(float verticalVelocity)
        {
            _forceUngroundCountdown = forceUngroundAfterJumpDuration;
        }

        public bool WriteClientState(uint writeTick, NetDataWriter writer, out bool shouldSendReliably)
        {
            return Functions.WriteClientState(writeTick, writer, out shouldSendReliably);
        }

        public bool WriteServerState(uint writeTick, NetDataWriter writer, out bool shouldSendReliably)
        {
            return Functions.WriteServerState(writeTick, writer, out shouldSendReliably);
        }

        public void ReadClientStateAtServer(uint peerTick, NetDataReader reader)
        {
            Functions.ReadClientStateAtServer(peerTick, reader);
        }

        public void ReadServerStateAtClient(uint peerTick, NetDataReader reader)
        {
            Functions.ReadServerStateAtClient(peerTick, reader);
        }

        public void StopMove()
        {
            Functions.StopMove();
        }

        public void KeyMovement(Vector3 moveDirection, MovementState movementState)
        {
            Functions.KeyMovement(moveDirection, movementState);
        }

        public void PointClickMovement(Vector3 position)
        {
            Functions.PointClickMovement(position);
        }

        public void SetExtraMovementState(ExtraMovementState extraMovementState)
        {
            Functions.SetExtraMovementState(extraMovementState);
        }

        public void SetLookRotation(Quaternion rotation, bool immediately)
        {
            Functions.SetLookRotation(rotation, immediately);
        }

        public Quaternion GetLookRotation()
        {
            return Functions.GetLookRotation();
        }

        public void SetSmoothTurnSpeed(float speed)
        {
            Functions.SetSmoothTurnSpeed(speed);
        }

        public float GetSmoothTurnSpeed()
        {
            return Functions.GetSmoothTurnSpeed();
        }

        public void Teleport(Vector3 position, Quaternion rotation, bool stillMoveAfterTeleport)
        {
            Functions.Teleport(position, rotation, stillMoveAfterTeleport);
        }

        public bool FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
        {
            return this.FindGroundedPosition(fromPosition, groundCheckRadius, findDistance, GameInstance.Singleton.GetGameEntityGroundDetectionLayerMask(), ABOVE_GROUND_OFFSETS, out result);
        }

        public void ApplyForce(ApplyMovementForceMode mode, Vector3 direction, ApplyMovementForceSourceType sourceType, int sourceDataId, int sourceLevel, float force, float deceleration, float duration)
        {
            Functions.ApplyForce(mode, direction, sourceType, sourceDataId, sourceLevel, force, deceleration, duration);
        }

        public EntityMovementForceApplier FindForceByActionKey(ApplyMovementForceSourceType sourceType, int sourceDataId)
        {
            return Functions.FindForceByActionKey(sourceType, sourceDataId);
        }

        public void ClearAllForces()
        {
            Functions.ClearAllForces();
        }

        public Vector3 GetSnapToGroundMotion(Vector3 motion, Vector3 platformMotion, Vector3 forceMotion)
        {
            if (!Functions.IsUnderWater && Functions.IsGrounded && Physics.Raycast(EntityTransform.position, Vector3.down, out RaycastHit hit, groundSnapDistance) && hit.transform.gameObject.layer != PhysicLayers.Water)
            {
                return Vector3.down * (hit.distance - CacheCharacterController.skinWidth);
            }
            return Vector3.zero;
        }

        public UniTask WaitClientTeleportConfirm()
        {
            return Functions.WaitClientTeleportConfirm();
        }

        public bool IsWaitingClientTeleportConfirm()
        {
            return Functions.IsWaitingClientTeleportConfirm();
        }
    }
}
