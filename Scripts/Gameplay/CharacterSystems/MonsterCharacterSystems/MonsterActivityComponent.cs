using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MultiplayerARPG
{
    public class MonsterActivityComponent : BaseMonsterActivityComponent
    {
        [Tooltip("Min random delay for next wander")]
        public float randomWanderDelayMin = 2f;
        [Tooltip("Max random delay for next wander")]
        public float randomWanderDelayMax = 5f;
        [Tooltip("Random distance around spawn position to wander")]
        public float randomWanderDistance = 2f;
        [Tooltip("Delay before find enemy again")]
        public float findEnemyDelay = 1f;
        [Tooltip("If following target time reached this value it will stop following target")]
        public float followTargetDuration = 5f;
        [Tooltip("Turn to enemy speed")]
        public float turnToEnemySpeed = 800f;

        protected bool startedAggressive;
        protected float aggressiveElasped;
        protected float randomedWanderElasped;
        protected float randomedWanderDelay;
        protected bool startedFollowEnemy;
        protected float startFollowEnemyElasped;
        protected Vector3 lastDestination;
        protected Vector3 lastPosition;
        protected BaseCharacterEntity tempTargetEnemy;
        protected BaseSkill queueSkill;
        protected short queueSkillLevel;

        public BaseMonsterCharacterEntity CacheMonsterCharacterEntity
        {
            get { return CacheEntity as BaseMonsterCharacterEntity; }
        }

        public MonsterCharacter MonsterDatabase
        {
            get { return CacheMonsterCharacterEntity.MonsterDatabase; }
        }

        protected void Update()
        {
            if (!IsServer || CacheEntity.Identity.CountSubscribers() == 0 || MonsterDatabase == null)
                return;

            if (CacheEntity.IsDead())
            {
                CacheEntity.StopMove();
                CacheEntity.SetTargetEntity(null);
                return;
            }

            float time = Time.unscaledTime;
            float deltaTime = Time.unscaledDeltaTime;

            Vector3 currentPosition = CacheEntity.CacheTransform.position;
            if (CacheMonsterCharacterEntity.Summoner != null)
            {
                if (!UpdateAttackEnemy(deltaTime, currentPosition))
                {
                    if (Vector3.Distance(currentPosition, CacheMonsterCharacterEntity.Summoner.CacheTransform.position) > gameInstance.minFollowSummonerDistance)
                    {
                        // Follow summoner
                        FollowSummoner();
                        startedFollowEnemy = false;
                    }
                    else
                    {
                        // Wandering when it's time
                        randomedWanderElasped += deltaTime;
                        if (randomedWanderElasped >= randomedWanderDelay)
                        {
                            randomedWanderElasped = 0f;
                            RandomWanderDestination();
                        }
                        startedFollowEnemy = false;
                    }
                }
            }
            else
            {
                if (CacheMonsterCharacterEntity.IsInSafeArea)
                {
                    // If monster move into safe area, wander to another place
                    RandomWanderDestination();
                    startedFollowEnemy = false;
                    return;
                }

                if (!UpdateAttackEnemy(deltaTime, currentPosition))
                {
                    if (startedAggressive)
                    {
                        aggressiveElasped += deltaTime;
                        // Find target when it's time
                        if (aggressiveElasped >= findEnemyDelay &&
                            FindEnemy(currentPosition))
                        {
                            aggressiveElasped = 0f;
                            startedAggressive = false;
                        }
                    }

                    // Wandering when it's time
                    randomedWanderElasped += deltaTime;
                    if (randomedWanderElasped >= randomedWanderDelay)
                    {
                        randomedWanderElasped = 0f;
                        RandomWanderDestination();
                        startedAggressive = true;
                    }
                    startedFollowEnemy = false;
                }
            }
        }

        /// <summary>
        /// Return `TRUE` if following / attacking enemy
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="currentPosition"></param>
        /// <returns></returns>
        private bool UpdateAttackEnemy(float deltaTime, Vector3 currentPosition)
        {
            if (!CacheEntity.TryGetTargetEntity(out tempTargetEnemy))
            {
                // No target, stop attacking
                return false;
            }

            if (tempTargetEnemy.IsDead() || tempTargetEnemy.IsInSafeArea)
            {
                // If target is dead or in safe area stop attacking
                CacheEntity.SetTargetEntity(null);
                return false;
            }

            // If it has target then go to target
            Vector3 targetPosition = tempTargetEnemy.CacheTransform.position;
            float attackDistance = CacheEntity.GetAttackDistance(false);
            attackDistance -= attackDistance * 0.1f;
            attackDistance -= CacheEntity.StoppingDistance;
            if (Vector3.Distance(currentPosition, targetPosition) <= attackDistance)
            {
                startedFollowEnemy = false;
                CacheEntity.StopMove();
                // Lookat target then do something when it's in range
                Vector3 lookAtDirection = (targetPosition - currentPosition).normalized;
                if (lookAtDirection.magnitude > 0)
                {
                    if (gameInstance.DimensionType == DimensionType.Dimension3D)
                    {
                        Quaternion currentLookAtRotation = CacheEntity.CacheTransform.rotation;
                        Vector3 lookRotationEuler = Quaternion.LookRotation(lookAtDirection).eulerAngles;
                        lookRotationEuler.x = 0;
                        lookRotationEuler.z = 0;
                        currentLookAtRotation = Quaternion.RotateTowards(currentLookAtRotation, Quaternion.Euler(lookRotationEuler), turnToEnemySpeed * Time.deltaTime);
                        CacheEntity.SetLookRotation(currentLookAtRotation.eulerAngles);
                    }
                    else
                    {
                        // Update 2D direction
                        CacheEntity.SetLookRotation(Quaternion.LookRotation(lookAtDirection).eulerAngles);
                    }
                }

                if (queueSkill != null || MonsterDatabase.RandomSkill(CacheMonsterCharacterEntity, out queueSkill, out queueSkillLevel))
                {
                    // Use skill when there is queue skill or randomed skill that can be used
                    CacheEntity.RequestUseSkill(queueSkill.DataId, false, tempTargetEnemy.OpponentAimTransform.position);
                    // Clear queue skill to random using skill later
                    queueSkill = null;
                }

                if (queueSkill == null)
                {
                    // Attack when no queue skill
                    CacheEntity.RequestAttack(false, tempTargetEnemy.OpponentAimTransform.position);
                }
            }
            else
            {
                if (!startedFollowEnemy)
                {
                    startFollowEnemyElasped = 0f;
                    startedFollowEnemy = true;
                }

                // Update destination if target's position changed
                if (!lastDestination.Equals(targetPosition))
                    SetDestination(targetPosition);

                if (CacheMonsterCharacterEntity.Summoner == null)
                {
                    startFollowEnemyElasped += deltaTime;
                    // Stop following target and move to position nearby spawn position when follow enemy too long
                    if (startFollowEnemyElasped >= followTargetDuration)
                        RandomWanderDestination();
                }
            }
            return true;
        }

        public void SetDestination(Vector3 destination)
        {
            CacheMonsterCharacterEntity.isWandering = false;
            CacheEntity.PointClickMovement(destination);
            lastDestination = destination;
        }

        public void SetWanderDestination(Vector3 destination)
        {
            CacheMonsterCharacterEntity.isWandering = true;
            CacheEntity.PointClickMovement(destination);
            lastDestination = destination;
        }

        public void RandomWanderDestination()
        {
            // Random position around spawn point
            Vector3 randomPosition;
            if (gameInstance.DimensionType == DimensionType.Dimension3D)
                randomPosition = CacheMonsterCharacterEntity.spawnPosition + new Vector3(Random.Range(-1f, 1f) * randomWanderDistance, 0, Random.Range(-1f, 1f) * randomWanderDistance);
            else
                randomPosition = CacheMonsterCharacterEntity.spawnPosition + new Vector3(Random.Range(-1f, 1f) * randomWanderDistance, Random.Range(-1f, 1f) * randomWanderDistance);
            // Random position around summoner
            if (CacheMonsterCharacterEntity.Summoner != null)
                randomPosition = CacheMonsterCharacterEntity.Summoner.GetSummonPosition();

            CacheEntity.SetTargetEntity(null);
            SetWanderDestination(randomPosition);
            randomedWanderDelay = Random.Range(randomWanderDelayMin, randomWanderDelayMax);
        }

        public void FollowSummoner()
        {
            // Random position around spawn point
            Vector3 randomPosition;
            if (gameInstance.DimensionType == DimensionType.Dimension3D)
                randomPosition = CacheMonsterCharacterEntity.spawnPosition + new Vector3(Random.Range(-1f, 1f) * randomWanderDistance, 0, Random.Range(-1f, 1f) * randomWanderDistance);
            else
                randomPosition = CacheMonsterCharacterEntity.spawnPosition + new Vector3(Random.Range(-1f, 1f) * randomWanderDistance, Random.Range(-1f, 1f) * randomWanderDistance);
            // Random position around summoner
            if (CacheMonsterCharacterEntity.Summoner != null)
                randomPosition = CacheMonsterCharacterEntity.Summoner.GetSummonPosition();

            CacheEntity.SetTargetEntity(null);
            SetDestination(randomPosition);
        }

        /// <summary>
        /// Return `TRUE` if found enemy
        /// </summary>
        /// <param name="currentPosition"></param>
        /// <returns></returns>
        public bool FindEnemy(Vector3 currentPosition)
        {
            // Aggressive monster or summoned monster will find target to attack
            if (MonsterDatabase.characteristic != MonsterCharacteristic.Aggressive &&
                CacheMonsterCharacterEntity.Summoner == null)
                return false;

            BaseCharacterEntity targetCharacter;
            if (!CacheEntity.TryGetTargetEntity(out targetCharacter) || targetCharacter.IsDead())
            {
                // If no target enenmy or target enemy is dead, Find nearby character by layer mask
                List<BaseCharacterEntity> characterEntities = CacheEntity.FindAliveCharacters<BaseCharacterEntity>(MonsterDatabase.visualRange, false, true, false);
                foreach (BaseCharacterEntity characterEntity in characterEntities)
                {
                    // Attack target settings
                    if (characterEntity == null || !characterEntity.CanReceiveDamageFrom(CacheEntity))
                    {
                        // If character is null or cannot receive damage from monster, skip it
                        continue;
                    }
                    if (CacheMonsterCharacterEntity.Summoner != null &&
                        CacheMonsterCharacterEntity.Summoner != characterEntity.GetTargetEntity())
                    {
                        // If character is not attacking summoner, skip it
                        continue;
                    }
                    if (!CacheEntity.IsEnemy(characterEntity))
                    {
                        // If character is not enemy, skip it
                        continue;
                    }
                    // Found target, attack it
                    CacheMonsterCharacterEntity.SetAttackTarget(characterEntity);
                    return true;
                }
            }

            return false;
        }
    }
}
