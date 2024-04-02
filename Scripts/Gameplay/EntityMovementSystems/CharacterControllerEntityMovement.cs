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
        public float landedPauseMovementDuration = 0f;
        public float beforeCrawlingPauseMovementDuration = 0f;
        public float afterCrawlingPauseMovementDuration = 0f;
        [Range(0.1f, 1f)]
        public float underWaterThreshold = 0.75f;
        public bool autoSwimToSurface;

        [Header("Ground checking")]
        public float groundCheckYOffsets = 0.1f;
        public float forceUngroundAfterJumpDuration = 0.1f;
        public Color groundCheckGizmosColor = Color.blue;

        [Header("Root Motion Settings")]
        [FormerlySerializedAs("useRootMotionWhileNotMoving")]
        public bool alwaysUseRootMotion;
        public bool useRootMotionForMovement;
        public bool useRootMotionForAirMovement;
        public bool useRootMotionForJump;
        public bool useRootMotionForFall;
        public bool useRootMotionUnderWater;

        [Header("Networking Settings")]
        public float snapThreshold = 5.0f;

        protected Animator _cacheAnimator;
        public Animator CacheAnimator
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying && _cacheAnimator == null)
                    _cacheAnimator = GetComponent<Animator>();
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

        public float StoppingDistance { get { return Functions.StoppingDistance; } }
        public MovementState MovementState { get { return Functions.MovementState; } }
        public ExtraMovementState ExtraMovementState { get { return Functions.ExtraMovementState; } }
        public DirectionVector2 Direction2D { get { return Functions.Direction2D; } set { Functions.Direction2D = value; } }
        public float CurrentMoveSpeed { get { return Functions.CurrentMoveSpeed; } }
        public Queue<Vector3> NavPaths { get { return Functions.NavPaths; } }
        public bool HasNavPaths { get { return Functions.HasNavPaths; } }

        protected float _forceUngroundCountdown = 0f;

        public override void EntityAwake()
        {
            // Prepare animator component
            CacheAnimator = GetComponent<Animator>();
            // Prepare character controller component
            CacheCharacterController = gameObject.GetOrAddComponent<CharacterController>();
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
                stoppingDistance = stoppingDistance,
                movementSecure = movementSecure,
                jumpHeight = jumpHeight,
                applyJumpForceMode = applyJumpForceMode,
                applyJumpForceFixedDuration = applyJumpForceFixedDuration,
                backwardMoveSpeedRate = backwardMoveSpeedRate,
                gravity = gravity,
                maxFallVelocity = maxFallVelocity,
                stickGroundForce = stickGroundForce,
                airborneDelay = airborneDelay,
                doNotChangeVelocityWhileAirborne = doNotChangeVelocityWhileAirborne,
                landedPauseMovementDuration = landedPauseMovementDuration,
                beforeCrawlingPauseMovementDuration = beforeCrawlingPauseMovementDuration,
                afterCrawlingPauseMovementDuration = afterCrawlingPauseMovementDuration,
                underWaterThreshold = underWaterThreshold,
                autoSwimToSurface = autoSwimToSurface,
                alwaysUseRootMotion = alwaysUseRootMotion,
                useRootMotionForMovement = useRootMotionForMovement,
                useRootMotionForAirMovement = useRootMotionForAirMovement,
                useRootMotionForJump = useRootMotionForJump,
                useRootMotionForFall = useRootMotionForFall,
                useRootMotionUnderWater = useRootMotionUnderWater,
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
            Functions.stoppingDistance = stoppingDistance;
            Functions.movementSecure = movementSecure;
            Functions.jumpHeight = jumpHeight;
            Functions.applyJumpForceMode = applyJumpForceMode;
            Functions.applyJumpForceFixedDuration = applyJumpForceFixedDuration;
            Functions.backwardMoveSpeedRate = backwardMoveSpeedRate;
            Functions.gravity = gravity;
            Functions.maxFallVelocity = maxFallVelocity;
            Functions.stickGroundForce = stickGroundForce;
            Functions.airborneDelay = airborneDelay;
            Functions.doNotChangeVelocityWhileAirborne = doNotChangeVelocityWhileAirborne;
            Functions.landedPauseMovementDuration = landedPauseMovementDuration;
            Functions.beforeCrawlingPauseMovementDuration = beforeCrawlingPauseMovementDuration;
            Functions.afterCrawlingPauseMovementDuration = afterCrawlingPauseMovementDuration;
            Functions.underWaterThreshold = underWaterThreshold;
            Functions.autoSwimToSurface = autoSwimToSurface;
            Functions.alwaysUseRootMotion = alwaysUseRootMotion;
            Functions.useRootMotionForMovement = useRootMotionForMovement;
            Functions.useRootMotionForAirMovement = useRootMotionForAirMovement;
            Functions.useRootMotionForJump = useRootMotionForJump;
            Functions.useRootMotionForFall = useRootMotionForFall;
            Functions.useRootMotionUnderWater = useRootMotionUnderWater;
            Functions.snapThreshold = snapThreshold;
#endif
            float deltaTime = Time.deltaTime;
            Functions.UpdateMovement(deltaTime);
            Functions.AfterMovementUpdate(deltaTime);
            if (_forceUngroundCountdown > 0f)
                _forceUngroundCountdown -= deltaTime;
        }

        public override void EntityLateUpdate()
        {
            float deltaTime = Time.deltaTime;
            Functions.UpdateRotation(deltaTime);
            Functions.FixSwimUpPosition(deltaTime);
        }

        public bool GroundCheck()
        {
            if (_forceUngroundCountdown > 0f)
                return false;
            if (CacheCharacterController.isGrounded)
                return true;
            float radius = GetGroundCheckRadius();
            return Physics.CheckSphere(GetGroundCheckCenter(radius), radius, GameInstance.Singleton.GetGameEntityGroundDetectionLayerMask(), QueryTriggerInteraction.Ignore);
        }

        private Vector3 GetGroundCheckCenter(float radius)
        {
            return new Vector3(CacheTransform.position.x, CacheTransform.position.y + radius - groundCheckYOffsets, CacheTransform.position.z);
        }

        private float GetGroundCheckRadius()
        {
            if (CacheCharacterController == null || CacheTransform == null)
                return 0f;
            return CacheCharacterController.radius * Mathf.Max(Mathf.Max(CacheTransform.lossyScale.x, CacheTransform.lossyScale.y), CacheTransform.lossyScale.z);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = groundCheckGizmosColor;
            float radius = GetGroundCheckRadius();
            Gizmos.DrawWireSphere(GetGroundCheckCenter(radius), radius);
            Gizmos.color = prevColor;
        }
#endif

        public Bounds GetBounds()
        {
            return CacheCharacterController.bounds;
        }

        public void Move(Vector3 motion)
        {
            CacheCharacterController.Move(motion);
        }

        public void RotateY(float yAngle)
        {
            CacheTransform.eulerAngles = new Vector3(0f, yAngle, 0f);
        }

        public void OnJumpForceApplied(float verticalVelocity)
        {
            _forceUngroundCountdown = forceUngroundAfterJumpDuration;
        }

        public bool WriteClientState(long writeTimestamp, NetDataWriter writer, out bool shouldSendReliably)
        {
            return Functions.WriteClientState(writeTimestamp, writer, out shouldSendReliably);
        }

        public bool WriteServerState(long writeTimestamp, NetDataWriter writer, out bool shouldSendReliably)
        {
            return Functions.WriteServerState(writeTimestamp, writer, out shouldSendReliably);
        }

        public void ReadClientStateAtServer(long peerTimestamp, NetDataReader reader)
        {
            Functions.ReadClientStateAtServer(peerTimestamp, reader);
        }

        public void ReadServerStateAtClient(long peerTimestamp, NetDataReader reader)
        {
            Functions.ReadServerStateAtClient(peerTimestamp, reader);
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

        public void SetLookRotation(Quaternion rotation)
        {
            Functions.SetLookRotation(rotation);
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
            return Functions.FindGroundedPosition(fromPosition, findDistance, out result);
        }
    }
}
