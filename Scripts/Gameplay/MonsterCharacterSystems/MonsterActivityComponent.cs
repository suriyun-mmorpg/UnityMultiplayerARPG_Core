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
        [Tooltip("Min random delay for next wander")]
        public float randomWanderDelayMin = 2f;
        [Tooltip("Max random delay for next wander")]
        public float randomWanderDelayMax = 5f;
        [Tooltip("Min random distance around spawn position to wander")]
        public float randomWanderAreaMin = 2f;
        [Tooltip("Max random distance around spawn position to wander")]
        public float randomWanderAreaMax = 5f;
        [Tooltip("Delay before find enemy again")]
        public float aggressiveFindTargetDelay = 1f;
        [Tooltip("Delay before set following target position again")]
        public float setTargetDestinationDelay = 1f;
        [Tooltip("If following target time reached this value it will stop following target")]
        public float followTargetDuration = 5f;

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
            wanderTime = time + Random.Range(randomWanderDelayMin, randomWanderDelayMax);
            oldDestination = transform.position;
        }

        public void SetFindTargetTime(float time)
        {
            findTargetTime = time + aggressiveFindTargetDelay;
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
                if (CacheMonsterCharacterEntity.isInSafeArea || targetEntity.isInSafeArea)
                {
                    CacheMonsterCharacterEntity.StopMove();
                    CacheMonsterCharacterEntity.SetTargetEntity(null);
                    RandomWanderTarget(time, gameplayRule);
                    return;
                }
                UpdateAttackTarget(time, gameplayRule, currentPosition, targetEntity);
            }
            else
            {
                // Update rotation while wandering
                if (!CacheNavMeshAgent.updateRotation)
                    CacheNavMeshAgent.updateRotation = true;
                // While character is moving then random next wander time
                // To let character stop movement some time before random next wander time
                if ((wanderDestination.HasValue && Vector3.Distance(currentPosition, wanderDestination.Value) > CacheNavMeshAgent.stoppingDistance)
                    || oldDestination != currentPosition)
                    RandomNextWanderTime(time);
                // Wandering when it's time
                if (time >= wanderTime)
                    RandomWanderTarget(time, gameplayRule);
                else
                    AggressiveFindTarget(time, gameInstance, monsterDatabase, currentPosition);
            }
        }

        public void UpdateAttackTarget(float time, BaseGameplayRule gameplayRule, Vector3 currentPosition, BaseCharacterEntity targetEntity)
        {
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
                if (!CacheNavMeshAgent.updateRotation)
                    CacheNavMeshAgent.updateRotation = true;
                if (oldDestination != targetEntityPosition &&
                    time - setDestinationTime >= setTargetDestinationDelay)
                    SetDestination(time, gameplayRule, targetEntityPosition);
                // Stop following target
                if (time - startFollowTargetTime >= followTargetDuration)
                {
                    CacheMonsterCharacterEntity.StopMove();
                    CacheMonsterCharacterEntity.SetTargetEntity(null);
                }
            }
        }

        public void RandomWanderTarget(float time, BaseGameplayRule gameplayRule)
        {
            // If stopped then random
            var randomX = Random.Range(randomWanderAreaMin, randomWanderAreaMax) * (Random.value > 0.5f ? -1 : 1);
            var randomZ = Random.Range(randomWanderAreaMin, randomWanderAreaMax) * (Random.value > 0.5f ? -1 : 1);
            var randomPosition = CacheMonsterCharacterEntity.spawnPosition + new Vector3(randomX, 0, randomZ);
            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(randomPosition, out navMeshHit, randomWanderAreaMax, 1))
                SetWanderDestination(time, gameplayRule, navMeshHit.position);
        }

        public void AggressiveFindTarget(float time, GameInstance gameInstance, MonsterCharacter monsterDatabase, Vector3 currentPosition)
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
        }

        public void StopMove()
        {
            CacheNavMeshAgent.isStopped = true;
            CacheNavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }
    }
}
