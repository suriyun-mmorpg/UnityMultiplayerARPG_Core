using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using LiteNetLibManager;
using UnityEngine.Profiling;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterModel))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public partial class PlayerCharacterEntity : BasePlayerCharacterEntity
    {
        #region Settings
        [Header("Movement AI")]
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;
        [Header("Movement Settings")]
        public float groundingDistance = 0.1f;
        public float jumpHeight = 2f;
        public float gravityRate = 1f;
        public float angularSpeed = 800f;
        #endregion

        public bool isJumping { get; protected set; }
        public bool isGrounded { get; protected set; }
        public Queue<Vector3> navPaths { get; protected set; }
        public Vector3 moveDirection { get; protected set; }

        public bool HasNavPaths
        {
            get { return navPaths != null && navPaths.Count > 0; }
        }

        public override float StoppingDistance
        {
            get { return stoppingDistance; }
        }
        
        private Rigidbody cacheRigidbody;
        public Rigidbody CacheRigidbody
        {
            get
            {
                if (cacheRigidbody == null)
                    cacheRigidbody = GetComponent<Rigidbody>();
                return cacheRigidbody;
            }
        }

        private CapsuleCollider cacheCapsuleCollider;
        public CapsuleCollider CacheCapsuleCollider
        {
            get
            {
                if (cacheCapsuleCollider == null)
                    cacheCapsuleCollider = GetComponent<CapsuleCollider>();
                return cacheCapsuleCollider;
            }
        }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            CacheRigidbody.useGravity = false;
            StopMove();
        }

        protected override void EntityUpdate()
        {
            base.EntityUpdate();
            Profiler.BeginSample("PlayerCharacterEntity - Update");
            if (IsDead())
            {
                StopMove();
                SetTargetEntity(null);
                return;
            }
            Profiler.EndSample();
        }

        protected override void EntityFixedUpdate()
        {
            base.EntityFixedUpdate();
            Profiler.BeginSample("PlayerCharacterEntity - FixedUpdate");
            if (!IsServer && !IsOwnerClient)
                return;

            if (HasNavPaths)
            {
                var targetPosition = navPaths.Peek();
                targetPosition.y = 0;
                var currentPosition = CacheTransform.position;
                currentPosition.y = 0;
                moveDirection = (targetPosition - currentPosition).normalized;
                if (Vector3.Distance(targetPosition, currentPosition) < StoppingDistance)
                {
                    navPaths.Dequeue();
                    if (!HasNavPaths)
                        StopMove();
                }
            }

            var velocity = CacheRigidbody.velocity;
            if (!IsDead())
            {
                var moveDirectionMagnitude = moveDirection.magnitude;
                if (!IsPlayingActionAnimation() && moveDirectionMagnitude != 0)
                {
                    if (moveDirectionMagnitude > 1)
                        moveDirection = moveDirection.normalized;

                    var targetVelocity = moveDirection * CacheMoveSpeed;

                    // Apply a force that attempts to reach our target velocity
                    Vector3 velocityChange = (targetVelocity - velocity);
                    velocityChange.x = Mathf.Clamp(velocityChange.x, -CacheMoveSpeed, CacheMoveSpeed);
                    velocityChange.y = 0;
                    velocityChange.z = Mathf.Clamp(velocityChange.z, -CacheMoveSpeed, CacheMoveSpeed);
                    CacheRigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
                    // Calculate rotation on client only, will send update to server later
                    CacheTransform.rotation = Quaternion.RotateTowards(CacheTransform.rotation, Quaternion.LookRotation(moveDirection), angularSpeed * Time.fixedDeltaTime);
                }

                RpgNetworkEntity tempEntity;
                if (moveDirectionMagnitude == 0 && TryGetTargetEntity(out tempEntity))
                {
                    var targetDirection = (tempEntity.CacheTransform.position - CacheTransform.position).normalized;
                    if (targetDirection.magnitude != 0f)
                    {
                        var fromRotation = CacheTransform.rotation.eulerAngles;
                        var lookAtRotation = Quaternion.LookRotation(targetDirection).eulerAngles;
                        lookAtRotation = new Vector3(fromRotation.x, lookAtRotation.y, fromRotation.z);
                        CacheTransform.rotation = Quaternion.RotateTowards(CacheTransform.rotation, Quaternion.Euler(lookAtRotation), angularSpeed * Time.fixedDeltaTime);
                    }
                }
                // Jump
                if (isGrounded && isJumping)
                {
                    RequestTriggerJump();
                    CacheRigidbody.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
                    isJumping = false;
                }
            }

            if (Mathf.Abs(velocity.y) > groundingDistance)
                isGrounded = false;

            // We apply gravity manually for more tuning control
            CacheRigidbody.AddForce(new Vector3(0, Physics.gravity.y * CacheRigidbody.mass * gravityRate, 0));
            Profiler.EndSample();
        }

        public override void OnSetup()
        {
            base.OnSetup();
            // Setup network components
            CacheNetTransform.ownerClientCanSendTransform = false;
            CacheNetTransform.ownerClientNotInterpolate = true;
            // Register Network functions
            RegisterNetFunction("TriggerJump", new LiteNetLibFunction(NetFuncTriggerJump));
            RegisterNetFunction("PointClickMovement", new LiteNetLibFunction<NetFieldVector3>((position) => NetFuncPointClickMovement(position)));
            RegisterNetFunction("KeyMovement", new LiteNetLibFunction<NetFieldVector3, NetFieldBool>((position, isJump) => NetFuncKeyMovement(position, isJump)));
            RegisterNetFunction("StopMove", new LiteNetLibFunction(StopMove));
            RegisterNetFunction("SetTargetEntity", new LiteNetLibFunction<NetFieldPackedUInt>((objectId) => NetFuncSetTargetEntity(objectId)));
        }

        protected void NetFuncPointClickMovement(Vector3 position)
        {
            if (IsDead())
                return;
            SetMovePaths(position, true);
            currentNpcDialog = null;
        }

        protected void NetFuncKeyMovement(Vector3 position, bool isJump)
        {
            if (IsDead())
                return;
            SetMovePaths(position, false);
            if (!isJumping)
                isJumping = isGrounded && isJump;
            currentNpcDialog = null;
        }

        protected void NetFuncSetTargetEntity(uint objectId)
        {
            if (objectId == 0)
                SetTargetEntity(null);
            RpgNetworkEntity rpgNetworkEntity;
            if (!TryGetEntityByObjectId(objectId, out rpgNetworkEntity))
                return;
            SetTargetEntity(rpgNetworkEntity);
        }

        protected virtual void NetFuncTriggerJump()
        {
            if (IsDead() || IsOwnerClient)
                return;
            // Play jump animation on non owner clients
            CharacterModel.PlayJumpAnimation();
        }

        public virtual void RequestTriggerJump()
        {
            if (IsDead())
                return;
            // Play jump animation immediately on owner client
            if (IsOwnerClient)
                CharacterModel.PlayJumpAnimation();
            // Only server will call for clients to trigger jump animation for secure entity
            if (IsServer)
                CallNetFunction("TriggerJump", FunctionReceivers.All);
        }

        public override void KeyMovement(Vector3 direction, bool isJump)
        {
            if (IsDead())
                return;
            if (direction.magnitude <= 0.025f && !isJump)
                return;
            var position = CacheTransform.position + direction;
            if (IsOwnerClient && !IsServer && CacheNetTransform.ownerClientNotInterpolate)
            {
                SetMovePaths(position, false);
                if (!isJumping)
                    isJumping = isGrounded && isJump;
            }
            CallNetFunction("KeyMovement", FunctionReceivers.Server, position, isJump);
        }

        public override void PointClickMovement(Vector3 position)
        {
            if (IsDead())
                return;
            if (IsOwnerClient && !IsServer && CacheNetTransform.ownerClientNotInterpolate)
                SetMovePaths(position, true);
            CallNetFunction("PointClickMovement", FunctionReceivers.Server, position);
        }

        public override void StopMove()
        {
            navPaths = null;
            moveDirection = Vector3.zero;
            CacheRigidbody.velocity = new Vector3(0, CacheRigidbody.velocity.y, 0);
            if (IsOwnerClient && !IsServer)
                CallNetFunction("StopMove", FunctionReceivers.Server);
        }

        public override void SetTargetEntity(RpgNetworkEntity entity)
        {
            base.SetTargetEntity(entity);
            if (IsOwnerClient && !IsServer)
                CallNetFunction("SetTargetEntity", FunctionReceivers.Server, entity == null ? 0 : entity.ObjectId);
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (!isGrounded && collision.impulse.y > 0)
                isGrounded = true;
        }

        protected virtual void OnCollisionStay(Collision collision)
        {
            if (!isGrounded && collision.impulse.y > 0)
                isGrounded = true;
        }
        
        protected virtual void SetMovePaths(Vector3 position, bool useNavMesh)
        {
            if (useNavMesh)
            {
                var navPath = new NavMeshPath();
                if (NavMesh.CalculatePath(CacheTransform.position, position, NavMesh.AllAreas, navPath))
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
            return Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y * gravityRate);
        }
    }
}
