using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class MonsterActivityComponent : MonoBehaviour
    {
        public const float RANDOM_WANDER_DURATION_MIN = 2f;
        public const float RANDOM_WANDER_DURATION_MAX = 5f;
        public const float RANDOM_WANDER_AREA_MIN = 2f;
        public const float RANDOM_WANDER_AREA_MAX = 5f;
        public const float AGGRESSIVE_FIND_TARGET_DELAY = 2f;
        public const float SET_TARGET_DESTINATION_DELAY = 1f;
        public const float FOLLOW_TARGET_DURATION = 5f;
        
        public float wanderTime { get; private set; }
        public float findTargetTime { get; private set; }
        public float setDestinationTime { get; private set; }
        public float startFollowTargetTime { get; private set; }
        public Vector3? wanderDestination { get; private set; }
        public Vector3 oldDestination { get; private set; }
        public bool isWandering { get; private set; }

        private MonsterCharacterEntity cacheMonsterCharacterEntity;
        public MonsterCharacterEntity CacheMonsterCharacterEntity
        {
            get
            {
                if (cacheMonsterCharacterEntity == null)
                    cacheMonsterCharacterEntity = GetComponent<MonsterCharacterEntity>();
                return cacheMonsterCharacterEntity;
            }
        }
        
        private NavMeshAgent cacheNavMeshAgent;
        public NavMeshAgent CacheNavMeshAgent
        {
            get
            {
                if (cacheNavMeshAgent == null)
                    cacheNavMeshAgent = GetComponent<NavMeshAgent>();
                return cacheNavMeshAgent;
            }
        }

        protected void Awake()
        {
            var time = Time.unscaledTime;
            RandomNextWanderTime(time);
            SetFindTargetTime(time);
            SetStartFollowTargetTime(time);
        }

        protected void Update()
        {
            var time = Time.unscaledTime;
            var gameInstance = GameInstance.Singleton;
            var gameplayRule = gameInstance != null ? gameInstance.GameplayRule : null;
            UpdateActivity(time, gameInstance, gameplayRule);
        }

        public void RandomNextWanderTime(float time)
        {
            wanderTime = time + Random.Range(RANDOM_WANDER_DURATION_MIN, RANDOM_WANDER_DURATION_MAX);
            oldDestination = transform.position;
        }

        public void SetFindTargetTime(float time)
        {
            findTargetTime = time + AGGRESSIVE_FIND_TARGET_DELAY;
        }

        public void SetStartFollowTargetTime(float time)
        {
            startFollowTargetTime = time;
        }

        public void SetDestination(float time, BaseGameplayRule gameplayRule, Vector3 targetPosition)
        {
            setDestinationTime = time;
            isWandering = false;
            CacheNavMeshAgent.speed = gameplayRule.GetMoveSpeed(CacheMonsterCharacterEntity);
            CacheNavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
            CacheNavMeshAgent.SetDestination(targetPosition);
            CacheNavMeshAgent.isStopped = false;
            oldDestination = targetPosition;
        }

        public void SetWanderDestination(float time, BaseGameplayRule gameplayRule, Vector3 destination)
        {
            setDestinationTime = time;
            isWandering = true;
            wanderDestination = destination;
            CacheNavMeshAgent.speed = gameplayRule.GetMoveSpeed(CacheMonsterCharacterEntity);
            CacheNavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            CacheNavMeshAgent.SetDestination(wanderDestination.Value);
            CacheNavMeshAgent.isStopped = false;
        }

        protected void UpdateActivity(float time, GameInstance gameInstance, BaseGameplayRule gameplayRule)
        {
            if (!CacheMonsterCharacterEntity.IsServer || CacheMonsterCharacterEntity.MonsterDatabase == null)
                return;

            var monsterDatabase = CacheMonsterCharacterEntity.MonsterDatabase;
            if (CacheMonsterCharacterEntity.IsDead())
            {
                CacheMonsterCharacterEntity.StopMove();
                CacheMonsterCharacterEntity.SetTargetEntity(null);
                if (time - CacheMonsterCharacterEntity.DeadTime >= monsterDatabase.deadHideDelay)
                {
                    if (CacheMonsterCharacterEntity.spawnArea != null)
                        CacheMonsterCharacterEntity.spawnArea.Spawn(monsterDatabase.deadRespawnDelay - monsterDatabase.deadHideDelay);
                    CacheMonsterCharacterEntity.NetworkDestroy();
                }
                return;
            }

            var currentPosition = transform.position;
            BaseCharacterEntity targetEntity;
            if (CacheMonsterCharacterEntity.TryGetTargetEntity(out targetEntity))
            {
                if (targetEntity.IsDead())
                {
                    CacheMonsterCharacterEntity.StopMove();
                    CacheMonsterCharacterEntity.SetTargetEntity(null);
                    return;
                }
                // If it has target then go to target
                var targetEntityPosition = targetEntity.CacheTransform.position;
                var attackDistance = CacheMonsterCharacterEntity.GetAttackDistance();
                attackDistance -= attackDistance * 0.1f;
                attackDistance -= CacheNavMeshAgent.stoppingDistance;
                if (Vector3.Distance(currentPosition, targetEntityPosition) <= attackDistance)
                {
                    SetStartFollowTargetTime(time);
                    // Lookat target then do anything when it's in range
                    CacheNavMeshAgent.updateRotation = false;
                    CacheNavMeshAgent.isStopped = true;
                    var lookAtDirection = (targetEntityPosition - currentPosition).normalized;
                    // slerp to the desired rotation over time
                    if (lookAtDirection.magnitude > 0)
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookAtDirection), CacheNavMeshAgent.angularSpeed * Time.deltaTime);
                    CacheMonsterCharacterEntity.RequestAttack();
                    // TODO: Random to use skills
                }
                else
                {
                    // Following target
                    CacheNavMeshAgent.updateRotation = true;
                    if (oldDestination != targetEntityPosition &&
                        time - setDestinationTime >= SET_TARGET_DESTINATION_DELAY)
                        SetDestination(time, gameplayRule, targetEntityPosition);
                    // Stop following target
                    if (time - startFollowTargetTime >= FOLLOW_TARGET_DURATION)
                    {
                        CacheMonsterCharacterEntity.StopMove();
                        CacheMonsterCharacterEntity.SetTargetEntity(null);
                        return;
                    }
                }
            }
            else
            {
                // Update rotation while wandering
                CacheNavMeshAgent.updateRotation = true;
                // While character is moving then random next wander time
                // To let character stop movement some time before random next wander time
                if ((wanderDestination.HasValue && Vector3.Distance(currentPosition, wanderDestination.Value) > CacheNavMeshAgent.stoppingDistance)
                    || oldDestination != currentPosition)
                    RandomNextWanderTime(time);
                // Wandering when it's time
                if (time >= wanderTime)
                {
                    // If stopped then random
                    var randomX = Random.Range(RANDOM_WANDER_AREA_MIN, RANDOM_WANDER_AREA_MAX) * (Random.value > 0.5f ? -1 : 1);
                    var randomZ = Random.Range(RANDOM_WANDER_AREA_MIN, RANDOM_WANDER_AREA_MAX) * (Random.value > 0.5f ? -1 : 1);
                    var randomPosition = CacheMonsterCharacterEntity.spawnPosition + new Vector3(randomX, 0, randomZ);
                    NavMeshHit navMeshHit;
                    if (NavMesh.SamplePosition(randomPosition, out navMeshHit, RANDOM_WANDER_AREA_MAX, 1))
                        SetWanderDestination(time, gameplayRule, navMeshHit.position);
                }
                else
                {
                    // If it's aggressive character, finding attacking target
                    if (monsterDatabase.characteristic == MonsterCharacteristic.Aggressive &&
                        time >= findTargetTime)
                    {
                        SetFindTargetTime(time);
                        BaseCharacterEntity targetCharacter;
                        // If no target enenmy or target enemy is dead
                        if (!CacheMonsterCharacterEntity.TryGetTargetEntity(out targetCharacter) || targetCharacter.IsDead())
                        {
                            // Find nearby character by layer mask
                            var foundObjects = new List<Collider>(Physics.OverlapSphere(currentPosition, monsterDatabase.visualRange, gameInstance.characterLayer.Mask));
                            foundObjects = foundObjects.OrderBy(a => System.Guid.NewGuid()).ToList();
                            foreach (var foundObject in foundObjects)
                            {
                                var characterEntity = foundObject.GetComponent<BaseCharacterEntity>();
                                if (characterEntity != null && CacheMonsterCharacterEntity.IsEnemy(characterEntity))
                                {
                                    SetStartFollowTargetTime(time);
                                    CacheMonsterCharacterEntity.SetAttackTarget(characterEntity);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void StopMove()
        {
            CacheNavMeshAgent.isStopped = true;
            CacheNavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }
    }
}
