using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class MonsterActivityComponent : MonoBehaviour
{
    public const float RANDOM_WANDER_DURATION_MIN = 2f;
    public const float RANDOM_WANDER_DURATION_MAX = 5f;
    public const float RANDOM_WANDER_AREA_MIN = 2f;
    public const float RANDOM_WANDER_AREA_MAX = 5f;
    public const float AGGRESSIVE_FIND_TARGET_DELAY = 2f;
    public const float SET_TARGET_DESTINATION_DELAY = 1f;
    public const float FOLLOW_TARGET_DURATION = 5f;

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

    protected void Update()
    {
        var time = Time.unscaledTime;
        var gameInstance = GameInstance.Singleton;
        var gameplayRule = gameInstance != null ? gameInstance.GameplayRule : null;
        UpdateActivity(time, gameInstance, gameplayRule, CacheMonsterCharacterEntity, CacheMonsterCharacterEntity.CacheTransform, CacheMonsterCharacterEntity.CacheNavMeshAgent);
    }

    public static void RandomNextWanderTime(float time, MonsterCharacterEntity monsterCharacterEntity, Transform transform)
    {
        monsterCharacterEntity.wanderTime = time + Random.Range(RANDOM_WANDER_DURATION_MIN, RANDOM_WANDER_DURATION_MAX);
        monsterCharacterEntity.oldDestination = transform.position;
    }

    public static void SetFindTargetTime(float time, MonsterCharacterEntity monsterCharacterEntity)
    {
        monsterCharacterEntity.findTargetTime = time + AGGRESSIVE_FIND_TARGET_DELAY;
    }

    public static void SetStartFollowTargetTime(float time, MonsterCharacterEntity monsterCharacterEntity)
    {
        monsterCharacterEntity.startFollowTargetTime = time;
    }

    public static void SetDestination(float time, BaseGameplayRule gameplayRule, MonsterCharacterEntity monsterCharacterEntity, NavMeshAgent navMeshAgent, Vector3 targetPosition)
    {
        monsterCharacterEntity.setDestinationTime = time;
        monsterCharacterEntity.isWandering = false;
        navMeshAgent.speed = gameplayRule.GetMoveSpeed(monsterCharacterEntity);
        navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        navMeshAgent.SetDestination(targetPosition);
        navMeshAgent.isStopped = false;
        monsterCharacterEntity.oldDestination = targetPosition;
    }

    public static void SetWanderDestination(float time, BaseGameplayRule gameplayRule, MonsterCharacterEntity monsterCharacterEntity, NavMeshAgent navMeshAgent, Vector3 destination)
    {
        monsterCharacterEntity.setDestinationTime = time;
        monsterCharacterEntity.isWandering = true;
        monsterCharacterEntity.wanderDestination = destination;
        navMeshAgent.speed = gameplayRule.GetMoveSpeed(monsterCharacterEntity);
        navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        navMeshAgent.SetDestination(monsterCharacterEntity.wanderDestination.Value);
        navMeshAgent.isStopped = false;
    }

    protected static void UpdateActivity(float time, GameInstance gameInstance, BaseGameplayRule gameplayRule, MonsterCharacterEntity monsterCharacterEntity, Transform transform, NavMeshAgent navMeshAgent)
    {
        if (!monsterCharacterEntity.IsServer || monsterCharacterEntity.MonsterDatabase == null)
            return;

        var monsterDatabase = monsterCharacterEntity.MonsterDatabase;
        if (monsterCharacterEntity.CurrentHp <= 0)
        {
            monsterCharacterEntity.StopMove();
            monsterCharacterEntity.SetTargetEntity(null);
            if (time - monsterCharacterEntity.deadTime >= monsterDatabase.deadHideDelay)
                monsterCharacterEntity.isHidding.Value = true;
            if (time - monsterCharacterEntity.deadTime >= monsterDatabase.deadRespawnDelay)
                monsterCharacterEntity.Respawn();
            return;
        }

        var currentPosition = transform.position;
        BaseCharacterEntity targetEntity;
        if (monsterCharacterEntity.TryGetTargetEntity(out targetEntity))
        {
            if (targetEntity.CurrentHp <= 0)
            {
                monsterCharacterEntity.StopMove();
                monsterCharacterEntity.SetTargetEntity(null);
                return;
            }
            // If it has target then go to target
            var targetEntityPosition = targetEntity.CacheTransform.position;
            var attackDistance = monsterCharacterEntity.GetAttackDistance();
            attackDistance -= attackDistance * 0.1f;
            attackDistance -= navMeshAgent.stoppingDistance;
            attackDistance += targetEntity.CacheCapsuleCollider.radius;
            if (Vector3.Distance(currentPosition, targetEntityPosition) <= attackDistance)
            {
                SetStartFollowTargetTime(time, monsterCharacterEntity);
                // Lookat target then do anything when it's in range
                navMeshAgent.updateRotation = false;
                navMeshAgent.isStopped = true;
                var lookAtDirection = (targetEntityPosition - currentPosition).normalized;
                // slerp to the desired rotation over time
                if (lookAtDirection.magnitude > 0)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookAtDirection), navMeshAgent.angularSpeed * Time.deltaTime);
                monsterCharacterEntity.RequestAttack();
                // TODO: Random to use skills
            }
            else
            {
                // Following target
                navMeshAgent.updateRotation = true;
                if (monsterCharacterEntity.oldDestination != targetEntityPosition &&
                    time - monsterCharacterEntity.setDestinationTime >= SET_TARGET_DESTINATION_DELAY)
                    SetDestination(time, gameplayRule, monsterCharacterEntity, navMeshAgent, targetEntityPosition);
                // Stop following target
                if (time - monsterCharacterEntity.startFollowTargetTime >= FOLLOW_TARGET_DURATION)
                {
                    monsterCharacterEntity.StopMove();
                    monsterCharacterEntity.SetTargetEntity(null);
                    return;
                }
            }
        }
        else
        {
            // Update rotation while wandering
            navMeshAgent.updateRotation = true;
            // While character is moving then random next wander time
            // To let character stop movement some time before random next wander time
            if ((monsterCharacterEntity.wanderDestination.HasValue && Vector3.Distance(currentPosition, monsterCharacterEntity.wanderDestination.Value) > navMeshAgent.stoppingDistance)
                || monsterCharacterEntity.oldDestination != currentPosition)
                RandomNextWanderTime(time, monsterCharacterEntity, transform);
            // Wandering when it's time
            if (time >= monsterCharacterEntity.wanderTime)
            {
                // If stopped then random
                var randomX = Random.Range(RANDOM_WANDER_AREA_MIN, RANDOM_WANDER_AREA_MAX) * (Random.value > 0.5f ? -1 : 1);
                var randomZ = Random.Range(RANDOM_WANDER_AREA_MIN, RANDOM_WANDER_AREA_MAX) * (Random.value > 0.5f ? -1 : 1);
                var randomPosition = monsterCharacterEntity.respawnPosition + new Vector3(randomX, 0, randomZ);
                NavMeshHit navMeshHit;
                if (NavMesh.SamplePosition(randomPosition, out navMeshHit, RANDOM_WANDER_AREA_MAX, 1))
                    SetWanderDestination(time, gameplayRule, monsterCharacterEntity, navMeshAgent, navMeshHit.position);
            }
            else
            {
                // If it's aggressive character, finding attacking target
                if (monsterDatabase.characteristic == MonsterCharacteristic.Aggressive &&
                    time >= monsterCharacterEntity.findTargetTime)
                {
                    SetFindTargetTime(time, monsterCharacterEntity);
                    BaseCharacterEntity targetCharacter;
                    // If no target enenmy or target enemy is dead
                    if (!monsterCharacterEntity.TryGetTargetEntity(out targetCharacter) || targetCharacter.CurrentHp <= 0)
                    {
                        // Find nearby character by layer mask
                        var foundObjects = new List<Collider>(Physics.OverlapSphere(currentPosition, monsterDatabase.visualRange, gameInstance.characterLayer.Mask));
                        foundObjects = foundObjects.OrderBy(a => System.Guid.NewGuid()).ToList();
                        foreach (var foundObject in foundObjects)
                        {
                            var characterEntity = foundObject.GetComponent<BaseCharacterEntity>();
                            if (characterEntity != null && monsterCharacterEntity.IsEnemy(characterEntity))
                            {
                                SetStartFollowTargetTime(time, monsterCharacterEntity);
                                monsterCharacterEntity.SetAttackTarget(characterEntity);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
