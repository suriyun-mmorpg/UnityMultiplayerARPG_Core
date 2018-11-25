using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MonsterActivityComponent2D : MonoBehaviour
    {
        [Tooltip("Min random delay for next wander")]
        public float randomWanderDelayMin = 2f;
        [Tooltip("Max random delay for next wander")]
        public float randomWanderDelayMax = 5f;
        [Tooltip("Min random distance around spawn position to wander")]
        public float randomWanderAreaMin = 0.5f;
        [Tooltip("Max random distance around spawn position to wander")]
        public float randomWanderAreaMax = 2f;
        [Tooltip("Delay before find enemy again")]
        public float aggressiveFindTargetDelay = 1f;
        [Tooltip("Delay before set following target position again")]
        public float setTargetDestinationDelay = 1f;
        [Tooltip("If following target time reached this value it will stop following target")]
        public float followTargetDuration = 5f;
        [Header("Movement AI")]
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;
        public float speed = 1f;

        public float wanderTime { get; private set; }
        public float findTargetTime { get; private set; }
        public float setDestinationTime { get; private set; }
        public float startFollowTargetTime { get; private set; }
        public Vector3? wanderDestination { get; private set; }
        public Vector3 oldDestination { get; private set; }
        public bool isWandering { get; private set; }
        public bool isMovingOutFromWall { get; private set; }

        // AI Component
        protected bool isStopped;
        protected Vector2? currentDestination;
        protected Vector2 moveDirection;

        private Transform cacheTransform;
        public Transform CacheTransform
        {
            get
            {
                if (cacheTransform == null)
                    cacheTransform = GetComponent<Transform>();
                return cacheTransform;
            }
        }

        private MonsterCharacterEntity2D cacheMonsterCharacterEntity;
        public MonsterCharacterEntity2D CacheMonsterCharacterEntity
        {
            get
            {
                if (cacheMonsterCharacterEntity == null)
                    cacheMonsterCharacterEntity = GetComponent<MonsterCharacterEntity2D>();
                return cacheMonsterCharacterEntity;
            }
        }

        private Rigidbody2D cacheRigidbody2D;
        public Rigidbody2D CacheRigidbody2D
        {
            get
            {
                if (cacheRigidbody2D == null)
                    cacheRigidbody2D = GetComponent<Rigidbody2D>();
                return cacheRigidbody2D;
            }
        }

        public GameInstance gameInstance
        {
            get { return GameInstance.Singleton; }
        }

        public BaseGameplayRule gameplayRule
        {
            get { return gameInstance.GameplayRule; }
        }

        public MonsterCharacter monsterDatabase
        {
            get { return CacheMonsterCharacterEntity.MonsterDatabase; }
        }

        protected void Awake()
        {
            CacheRigidbody2D.gravityScale = 0;
            var time = Time.unscaledTime;
            RandomNextWanderTime(time);
            SetFindTargetTime(time);
            SetStartFollowTargetTime(time);
        }

        protected void Update()
        {
            UpdateActivity(Time.unscaledTime);
        }

        protected void FixedUpdate()
        {
            if (!CacheMonsterCharacterEntity.IsServer)
                return;

            if (isStopped)
            {
                CacheRigidbody2D.velocity = Vector2.zero;
                return;
            }

            if (currentDestination.HasValue)
            {
                var currentPosition = new Vector2(CacheTransform.position.x, CacheTransform.position.y);
                moveDirection = (currentDestination.Value - currentPosition).normalized;
                if (Vector3.Distance(currentDestination.Value, currentPosition) < stoppingDistance)
                    StopMove();
                else
                {
                    CacheMonsterCharacterEntity.UpdateCurrentDirection(moveDirection);
                    CacheRigidbody2D.velocity = moveDirection * speed;
                }
            }
        }

        public void SetDestination(Vector2 destination)
        {
            currentDestination = destination;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (isMovingOutFromWall)
                return;
            StartCoroutine(SimpleMoveOutFromWallRoutine());
        }

        IEnumerator SimpleMoveOutFromWallRoutine()
        {
            isMovingOutFromWall = true;
            var oldPosition = CacheMonsterCharacterEntity.CacheTransform.position;
            yield return new WaitForSeconds(0.5f);
            if (Vector3.Distance(oldPosition, CacheMonsterCharacterEntity.CacheTransform.position) < stoppingDistance)
                RandomWanderTarget(Time.unscaledTime);
            isMovingOutFromWall = false;
        }

        public void RandomNextWanderTime(float time)
        {
            wanderTime = time + Random.Range(randomWanderDelayMin, randomWanderDelayMax);
            oldDestination = CacheMonsterCharacterEntity.CacheTransform.position;
        }

        public void SetFindTargetTime(float time)
        {
            findTargetTime = time + aggressiveFindTargetDelay;
        }

        public void SetStartFollowTargetTime(float time)
        {
            startFollowTargetTime = time;
        }

        public void SetDestination(float time, Vector3 destination)
        {
            setDestinationTime = time;
            isWandering = false;
            ResumeMove();
            speed = gameplayRule.GetMoveSpeed(CacheMonsterCharacterEntity);
            SetDestination(destination);
            oldDestination = destination;
        }

        public void SetWanderDestination(float time, Vector3 destination)
        {
            setDestinationTime = time;
            isWandering = true;
            ResumeMove();
            speed = monsterDatabase.wanderMoveSpeed;
            SetDestination(destination);
            wanderDestination = destination;
        }

        protected void UpdateActivity(float time)
        {
            if (!CacheMonsterCharacterEntity.IsServer || monsterDatabase == null)
                return;

            if (CacheMonsterCharacterEntity.IsDead())
            {
                StopMove();
                CacheMonsterCharacterEntity.SetTargetEntity(null);
                return;
            }

            var currentPosition = CacheMonsterCharacterEntity.CacheTransform.position;
            BaseCharacterEntity targetEntity;
            if (CacheMonsterCharacterEntity.TryGetTargetEntity(out targetEntity))
            {
                if (targetEntity.IsDead())
                {
                    RandomWanderTarget(time);
                    return;
                }
                if (CacheMonsterCharacterEntity.isInSafeArea || targetEntity.isInSafeArea)
                {
                    RandomWanderTarget(time);
                    return;
                }
                UpdateAttackTarget(time, currentPosition, targetEntity);
            }
            else
            {
                // While character is moving then random next wander time
                // To let character stop movement some time before random next wander time
                if ((wanderDestination.HasValue && Vector3.Distance(currentPosition, wanderDestination.Value) > stoppingDistance)
                    || oldDestination != currentPosition)
                    RandomNextWanderTime(time);
                // Wandering when it's time
                if (time >= wanderTime)
                    RandomWanderTarget(time);
                else
                    AggressiveFindTarget(time, currentPosition);
            }
        }

        public void UpdateAttackTarget(float time, Vector3 currentPosition, BaseCharacterEntity targetEntity)
        {
            if (CacheMonsterCharacterEntity.summoner != null &&
                Vector3.Distance(currentPosition, CacheMonsterCharacterEntity.summoner.CacheTransform.position) > gameInstance.minFollowSummonerDistance)
            {
                RandomWanderTarget(time);
                return;
            }
            // If it has target then go to target
            var targetEntityPosition = targetEntity.CacheTransform.position;
            var attackDistance = CacheMonsterCharacterEntity.GetAttackDistance();
            attackDistance -= attackDistance * 0.1f;
            attackDistance -= stoppingDistance;
            if (Vector3.Distance(currentPosition, targetEntityPosition) <= attackDistance)
            {
                StopMove();
                SetStartFollowTargetTime(time);
                // Lookat target then do anything when it's in range
                var targetDirection = (targetEntity.CacheTransform.position - CacheMonsterCharacterEntity.CacheTransform.position).normalized;
                if (targetDirection.magnitude != 0f)
                    CacheMonsterCharacterEntity.UpdateCurrentDirection(targetDirection);
                CacheMonsterCharacterEntity.RequestAttack();
                // TODO: Random to use skills
            }
            else
            {
                // Following target
                if (oldDestination != targetEntityPosition &&
                    time - setDestinationTime >= setTargetDestinationDelay)
                    SetDestination(time, targetEntityPosition);
                // Stop following target
                if (time - startFollowTargetTime >= followTargetDuration)
                    RandomWanderTarget(time);
            }
        }

        public void RandomWanderTarget(float time)
        {
            // If stopped then random
            var randomX = Random.Range(randomWanderAreaMin, randomWanderAreaMax) * (Random.value > 0.5f ? -1 : 1);
            var randomY = Random.Range(randomWanderAreaMin, randomWanderAreaMax) * (Random.value > 0.5f ? -1 : 1);
            var randomPosition = CacheMonsterCharacterEntity.summoner != null ? CacheMonsterCharacterEntity.summoner.CacheTransform.position : CacheMonsterCharacterEntity.spawnPosition;
            randomPosition += new Vector3(randomX, randomY);
            CacheMonsterCharacterEntity.SetTargetEntity(null);
            SetWanderDestination(time, randomPosition);
        }

        public void AggressiveFindTarget(float time, Vector3 currentPosition)
        {
            // If it's aggressive character, finding attacking target
            if (monsterDatabase.characteristic != MonsterCharacteristic.Aggressive ||
                CacheMonsterCharacterEntity.summoner != null ||
                time < findTargetTime)
                return;

            SetFindTargetTime(time);
            BaseCharacterEntity targetCharacter;
            // If no target enenmy or target enemy is dead
            if (!CacheMonsterCharacterEntity.TryGetTargetEntity(out targetCharacter) || targetCharacter.IsDead())
            {
                // Find nearby character by layer mask
                var foundObjects = new List<Collider2D>(Physics2D.OverlapCircleAll(currentPosition, monsterDatabase.visualRange, gameInstance.characterLayer.Mask));
                foundObjects = foundObjects.OrderBy(a => System.Guid.NewGuid()).ToList();
                foreach (var foundObject in foundObjects)
                {
                    var characterEntity = foundObject.GetComponent<BaseCharacterEntity>();
                    // Attack target settings
                    if (characterEntity != null &&
                        CacheMonsterCharacterEntity.IsEnemy(characterEntity) &&
                        characterEntity.CanReceiveDamageFrom(CacheMonsterCharacterEntity))
                    {
                        SetStartFollowTargetTime(time);
                        CacheMonsterCharacterEntity.SetAttackTarget(characterEntity);
                        return;
                    }
                }
            }
        }

        public void ResumeMove()
        {
            isStopped = false;
        }

        public void StopMove()
        {
            isStopped = true;
            currentDestination = null;
            moveDirection = Vector3.zero;
            CacheRigidbody2D.velocity = Vector2.zero;
        }
    }
}
