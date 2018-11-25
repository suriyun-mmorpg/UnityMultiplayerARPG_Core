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
            var time = Time.unscaledTime;
            RandomNextWanderTime(time);
            SetFindTargetTime(time);
            SetStartFollowTargetTime(time);
        }

        protected void Update()
        {
            UpdateActivity(Time.unscaledTime);
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
            ResumeMove(ObstacleAvoidanceType.MedQualityObstacleAvoidance);
            CacheNavMeshAgent.speed = gameplayRule.GetMoveSpeed(CacheMonsterCharacterEntity);
            CacheNavMeshAgent.SetDestination(destination);
            oldDestination = destination;
        }

        public void SetWanderDestination(float time, Vector3 destination)
        {
            setDestinationTime = time;
            isWandering = true;
            ResumeMove(ObstacleAvoidanceType.NoObstacleAvoidance);
            CacheNavMeshAgent.speed = monsterDatabase.wanderMoveSpeed;
            CacheNavMeshAgent.SetDestination(destination);
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
                if ((wanderDestination.HasValue && Vector3.Distance(currentPosition, wanderDestination.Value) > CacheNavMeshAgent.stoppingDistance)
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
            attackDistance -= CacheNavMeshAgent.stoppingDistance;
            if (Vector3.Distance(currentPosition, targetEntityPosition) <= attackDistance)
            {
                StopMove();
                SetStartFollowTargetTime(time);
                // Lookat target then do anything when it's in range
                var lookAtDirection = (targetEntityPosition - currentPosition).normalized;
                // slerp to the desired rotation over time
                if (lookAtDirection.magnitude > 0)
                {
                    var lookRotationEuler = Quaternion.LookRotation(lookAtDirection).eulerAngles;
                    lookRotationEuler.x = 0;
                    lookRotationEuler.z = 0;
                    CacheMonsterCharacterEntity.CacheTransform.rotation = Quaternion.RotateTowards(CacheMonsterCharacterEntity.CacheTransform.rotation, Quaternion.Euler(lookRotationEuler), CacheNavMeshAgent.angularSpeed * Time.deltaTime);
                }
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
            var randomZ = Random.Range(randomWanderAreaMin, randomWanderAreaMax) * (Random.value > 0.5f ? -1 : 1);
            var randomPosition = CacheMonsterCharacterEntity.summoner != null ? CacheMonsterCharacterEntity.summoner.CacheTransform.position : CacheMonsterCharacterEntity.spawnPosition;
            randomPosition += new Vector3(randomX, 0, randomZ);
            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(randomPosition, out navMeshHit, randomWanderAreaMax, 1))
            {
                CacheMonsterCharacterEntity.SetTargetEntity(null);
                SetWanderDestination(time, navMeshHit.position);
            }
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

        public void ResumeMove(ObstacleAvoidanceType obstacleAvoidanceType)
        {
            CacheNavMeshAgent.updatePosition = true;
            CacheNavMeshAgent.updateRotation = true;
            CacheNavMeshAgent.isStopped = false;
            CacheNavMeshAgent.obstacleAvoidanceType = obstacleAvoidanceType;
        }

        public void StopMove()
        {
            CacheNavMeshAgent.updatePosition = false;
            CacheNavMeshAgent.updateRotation = false;
            CacheNavMeshAgent.isStopped = true;
            CacheNavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }
    }
}
