using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiplayerARPG
{
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

        public float wanderTime { get; private set; }
        public float findTargetTime { get; private set; }
        public float setDestinationTime { get; private set; }
        public float startFollowTargetTime { get; private set; }
        public Vector3? wanderDestination { get; private set; }
        public Vector3 oldDestination { get; private set; }
        public bool isWandering { get; private set; }
        public bool isMovingOutFromWall { get; private set; }

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
            var time = Time.unscaledTime;
            UpdateActivity(time);
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
            CacheMonsterCharacterEntity.speed = gameplayRule.GetMoveSpeed(CacheMonsterCharacterEntity);
            CacheMonsterCharacterEntity.SetDestination(destination);
            CacheMonsterCharacterEntity.isStopped = false;
            oldDestination = destination;
        }

        public void SetWanderDestination(float time, Vector3 destination)
        {
            setDestinationTime = time;
            isWandering = true;
            CacheMonsterCharacterEntity.speed = monsterDatabase.wanderMoveSpeed;
            CacheMonsterCharacterEntity.SetDestination(destination);
            CacheMonsterCharacterEntity.isStopped = false;
            wanderDestination = destination;
        }

        protected void UpdateActivity(float time)
        {
            if (!CacheMonsterCharacterEntity.IsServer || monsterDatabase == null)
                return;
            
            if (CacheMonsterCharacterEntity.IsDead())
            {
                CacheMonsterCharacterEntity.StopMove();
                CacheMonsterCharacterEntity.SetTargetEntity(null);
                CacheMonsterCharacterEntity.DestroyAndRespawn();
                return;
            }

            var currentPosition = CacheMonsterCharacterEntity.CacheTransform.position;
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
                    RandomWanderTarget(time);
                    return;
                }
                UpdateAttackTarget(time, currentPosition, targetEntity);
            }
            else
            {
                // While character is moving then random next wander time
                // To let character stop movement some time before random next wander time
                if ((wanderDestination.HasValue && Vector3.Distance(currentPosition, wanderDestination.Value) > CacheMonsterCharacterEntity.stoppingDistance)
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
            // If it has target then go to target
            var targetEntityPosition = targetEntity.CacheTransform.position;
            var attackDistance = CacheMonsterCharacterEntity.GetAttackDistance();
            attackDistance -= attackDistance * 0.1f;
            attackDistance -= CacheMonsterCharacterEntity.stoppingDistance;
            if (Vector3.Distance(currentPosition, targetEntityPosition) <= attackDistance)
            {
                SetStartFollowTargetTime(time);
                // Lookat target then do anything when it's in range
                CacheMonsterCharacterEntity.isStopped = true;
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
                {
                    CacheMonsterCharacterEntity.StopMove();
                    CacheMonsterCharacterEntity.SetTargetEntity(null);
                }
            }
        }

        public void RandomWanderTarget(float time)
        {
            // If stopped then random
            var randomX = Random.Range(randomWanderAreaMin, randomWanderAreaMax) * (Random.value > 0.5f ? -1 : 1);
            var randomY = Random.Range(randomWanderAreaMin, randomWanderAreaMax) * (Random.value > 0.5f ? -1 : 1);
            var randomPosition = CacheMonsterCharacterEntity.spawnPosition + new Vector3(randomX, randomY);
            SetWanderDestination(time, randomPosition);
        }

        public void AggressiveFindTarget(float time, Vector3 currentPosition)
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
        }

        public void StopMove()
        {
            CacheMonsterCharacterEntity.StopMove();
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
            yield return new WaitForSeconds(Random.Range(randomWanderDelayMin, randomWanderDelayMax));
            if (Vector3.Distance(oldPosition, CacheMonsterCharacterEntity.CacheTransform.position) < CacheMonsterCharacterEntity.stoppingDistance)
                RandomWanderTarget(Time.unscaledTime);
            isMovingOutFromWall = false;
        }
    }
}
