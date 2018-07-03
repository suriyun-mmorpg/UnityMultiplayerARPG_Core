using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(LiteNetLibTransform))]
    public partial class PlayerCharacterEntity : BaseCharacterEntity, IPlayerCharacterData
    {
        [HideInInspector]
        public WarpPortalEntity warpingPortal;
        public BasePlayerCharacterController controllerPrefab;

        #region Settings
        [Header("Movement AI")]
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;
        [Header("Movement Settings")]
        public float groundingDistance = 0.1f;
        public float jumpHeight = 2f;
        public float gravityRate = 1f;
        public float angularSpeed = 120f;
        #endregion

        #region Protected data
        public Queue<Vector3> navPaths { get; protected set; }
        public Vector3 moveDirection { get; protected set; }
        public bool isJumping { get; protected set; }
        public bool isGrounded { get; protected set; }
        public NpcDialog currentNpcDialog { get; protected set; }

        public bool HasNavPaths
        {
            get { return navPaths != null && navPaths.Count > 0; }
        }
        #endregion

        #region Cache components
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

        private LiteNetLibTransform cacheNetTransform;
        public LiteNetLibTransform CacheNetTransform
        {
            get
            {
                if (cacheNetTransform == null)
                    cacheNetTransform = GetComponent<LiteNetLibTransform>();
                return cacheNetTransform;
            }
        }
        #endregion

        protected override void Awake()
        {
            base.Awake();
            CacheRigidbody.useGravity = false;
            gameObject.tag = gameInstance.playerTag;
            StopMove();
        }

        protected override void Start()
        {
            base.Start();
            if (IsOwnerClient)
            {
                if (BasePlayerCharacterController.Singleton == null)
                {
                    var controller = Instantiate(controllerPrefab);
                    controller.CharacterEntity = this;
                }
            }
        }

        protected override void Update()
        {
            base.Update();

            if (IsDead())
            {
                StopMove();
                SetTargetEntity(null);
                return;
            }
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

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!IsServer && !IsOwnerClient)
                return;

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

                BaseCharacterEntity tempCharacterEntity;
                if (moveDirectionMagnitude == 0 && TryGetTargetEntity(out tempCharacterEntity))
                {
                    var targetDirection = (tempCharacterEntity.CacheTransform.position - CacheTransform.position).normalized;
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
                    CacheRigidbody.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
                    isJumping = false;
                }
            }

            if (Mathf.Abs(velocity.y) > groundingDistance)
                isGrounded = false;

            // We apply gravity manually for more tuning control
            CacheRigidbody.AddForce(new Vector3(0, Physics.gravity.y * CacheRigidbody.mass * gravityRate, 0));
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (!IsServer && !IsOwnerClient)
                return;

            if (navPaths != null)
            {
                if (navPaths.Count > 0)
                {
                    var target = navPaths.Peek();
                    target = new Vector3(target.x, 0, target.z);
                    var currentPosition = CacheTransform.position;
                    currentPosition = new Vector3(currentPosition.x, 0, currentPosition.z);
                    moveDirection = (target - currentPosition).normalized;
                    if (Vector3.Distance(target, currentPosition) < stoppingDistance)
                        navPaths.Dequeue();
                }
                else
                    StopMove();
            }
        }

        public virtual void StopMove()
        {
            navPaths = null;
            moveDirection = Vector3.zero;
            CacheRigidbody.velocity = new Vector3(0, CacheRigidbody.velocity.y, 0);
        }

        private float CalculateJumpVerticalSpeed()
        {
            // From the jump height and gravity we deduce the upwards speed 
            // for the character to reach at the apex.
            return Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y * gravityRate);
        }

        public override void Respawn()
        {
            if (!IsServer || !IsDead())
                return;
            base.Respawn();
            var manager = Manager as BaseGameNetworkManager;
            if (manager != null)
                manager.WarpCharacter(this, RespawnMapName, RespawnPosition);
        }

        public override bool CanReceiveDamageFrom(BaseCharacterEntity characterEntity)
        {
            // TODO: May implement this for party/guild battle purposes
            return characterEntity != null && characterEntity is MonsterCharacterEntity;
        }

        public override bool IsAlly(BaseCharacterEntity characterEntity)
        {
            // TODO: May implement this for party/guild battle purposes
            return false;
        }

        public override bool IsEnemy(BaseCharacterEntity characterEntity)
        {
            // TODO: May implement this for party/guild battle purposes
            return characterEntity != null && characterEntity is MonsterCharacterEntity;
        }
        
        protected void SetMovePaths(Vector3 position)
        {
            var navPath = new NavMeshPath();
            if (NavMesh.CalculatePath(CacheTransform.position, position, NavMesh.AllAreas, navPath))
            {
                navPaths = new Queue<Vector3>(navPath.corners);
                // Dequeue first path it's not require for future movement
                navPaths.Dequeue();
            }
        }

        public void KeyMovement(Vector3 direction, bool isJump)
        {
            if (IsDead())
                return;
            moveDirection = direction;
            if (moveDirection.magnitude == 0 && isGrounded)
                CacheRigidbody.velocity = new Vector3(0, CacheRigidbody.velocity.y, 0);
            if (!isJumping)
                isJumping = isGrounded && isJump;
        }

        public void PointClickMovement(Vector3 position)
        {
            if (IsDead())
                return;
            SetMovePaths(position);
        }

        public override void Killed(BaseCharacterEntity lastAttacker)
        {
            base.Killed(lastAttacker);
            currentNpcDialog = null;
        }

        public virtual void IncreaseGold(int gold)
        {
            if (!IsServer)
                return;
            Gold += gold;
        }

        public virtual void OnKillMonster(MonsterCharacterEntity monsterCharacterEntity)
        {
            if (!IsServer || monsterCharacterEntity == null)
                return;

            for (var i = 0; i < Quests.Count; ++i)
            {
                var quest = Quests[i];
                if (quest.AddKillMonster(monsterCharacterEntity, 1))
                    quests[i] = quest;
            }
        }
    }
}
