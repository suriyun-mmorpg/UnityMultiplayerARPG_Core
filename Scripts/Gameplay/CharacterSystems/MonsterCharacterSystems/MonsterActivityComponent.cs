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
        public float aggressiveFindTargetDelay = 1f;
        [Tooltip("Delay before set following target position again")]
        public float setTargetDestinationDelay = 1f;
        [Tooltip("If following target time reached this value it will stop following target")]
        public float followTargetDuration = 5f;
        [Tooltip("Turn to enemy speed")]
        public float turnToEnemySpeed = 800f;

        public float wanderTime { get; private set; }
        public float findTargetTime { get; private set; }
        public float setDestinationTime { get; private set; }
        public float startFollowTargetTime { get; private set; }
        public Vector3? wanderDestination { get; private set; }
        public Vector3 oldDestination { get; private set; }

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

        protected void Awake()
        {
            float time = Time.unscaledTime;
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
            oldDestination = CacheEntity.CacheTransform.position;
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
            CacheMonsterCharacterEntity.isWandering = false;
            CacheEntity.PointClickMovement(destination);
            oldDestination = destination;
        }

        public void SetWanderDestination(float time, Vector3 destination)
        {
            setDestinationTime = time;
            CacheMonsterCharacterEntity.isWandering = true;
            CacheEntity.PointClickMovement(destination);
            wanderDestination = destination;
        }

        protected void UpdateActivity(float time)
        {
            if (!IsServer || CacheEntity.Identity.CountSubscribers() == 0 || MonsterDatabase == null)
                return;

            if (CacheEntity.IsDead())
            {
                CacheEntity.StopMove();
                CacheEntity.SetTargetEntity(null);
                return;
            }

            Vector3 currentPosition = CacheEntity.CacheTransform.position;

            if (CacheMonsterCharacterEntity.Summoner != null &&
                Vector3.Distance(currentPosition, CacheMonsterCharacterEntity.Summoner.CacheTransform.position) > gameInstance.minFollowSummonerDistance)
            {
                // Follow summoner with stat's move speed
                FollowSummoner(time);
                return;
            }

            if (CacheMonsterCharacterEntity.Summoner == null && CacheEntity.isInSafeArea)
            {
                // If monster move into safe area, wander to another place
                RandomWanderTarget(time);
                return;
            }

            BaseCharacterEntity targetEntity;
            if (CacheEntity.TryGetTargetEntity(out targetEntity))
            {
                if (targetEntity.IsDead() || targetEntity.isInSafeArea)
                {
                    // If target is dead or in safe area stop attacking
                    CacheEntity.SetTargetEntity(null);
                    return;
                }
                UpdateAttackTarget(time, currentPosition, targetEntity);
            }
            else
            {
                // Find target when it's time
                if (time >= findTargetTime)
                {
                    SetFindTargetTime(time);
                    AggressiveFindTarget(time, currentPosition);
                    return;
                }

                // Wandering when it's time
                if (time >= wanderTime)
                {
                    RandomNextWanderTime(time);
                    RandomWanderTarget(time);
                    return;
                }
            }
        }
        
        public void UpdateAttackTarget(float time, Vector3 currentPosition, BaseCharacterEntity targetEntity)
        {
            // If it has target then go to target
            Vector3 targetEntityPosition = targetEntity.CacheTransform.position;
            float attackDistance = CacheEntity.GetAttackDistance(false);
            attackDistance -= attackDistance * 0.1f;
            attackDistance -= CacheEntity.StoppingDistance;
            if (Vector3.Distance(currentPosition, targetEntityPosition) <= attackDistance)
            {
                CacheEntity.StopMove();
                SetStartFollowTargetTime(time);
                // Lookat target then do something when it's in range
                Vector3 lookAtDirection = (targetEntityPosition - currentPosition).normalized;
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
                    CacheEntity.RequestUseSkill(queueSkill.DataId, false, targetEntity.OpponentAimTransform.position);
                    // Clear queue skill to random using skill later
                    queueSkill = null;
                }

                if (queueSkill == null)
                {
                    // Attack when no queue skill
                    CacheEntity.RequestAttack(false, targetEntity.OpponentAimTransform.position);
                }
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
            Vector3 randomPosition;
            if (gameInstance.DimensionType == DimensionType.Dimension3D)
                randomPosition = CacheMonsterCharacterEntity.spawnPosition + new Vector3(Random.Range(-1f, 1f) * randomWanderDistance, 0, Random.Range(-1f, 1f) * randomWanderDistance);
            else
                randomPosition = CacheMonsterCharacterEntity.spawnPosition + new Vector3(Random.Range(-1f, 1f) * randomWanderDistance, Random.Range(-1f, 1f) * randomWanderDistance);
            if (CacheMonsterCharacterEntity.Summoner != null)
                randomPosition = CacheMonsterCharacterEntity.Summoner.GetSummonPosition();
            CacheEntity.SetTargetEntity(null);
            SetWanderDestination(time, randomPosition);
        }

        public void FollowSummoner(float time)
        {
            // If stopped then random
            Vector3 randomPosition;
            if (gameInstance.DimensionType == DimensionType.Dimension3D)
                randomPosition = CacheMonsterCharacterEntity.spawnPosition + new Vector3(Random.Range(-1f, 1f) * randomWanderDistance, 0, Random.Range(-1f, 1f) * randomWanderDistance);
            else
                randomPosition = CacheMonsterCharacterEntity.spawnPosition + new Vector3(Random.Range(-1f, 1f) * randomWanderDistance, Random.Range(-1f, 1f) * randomWanderDistance);
            if (CacheMonsterCharacterEntity.Summoner != null)
                randomPosition = CacheMonsterCharacterEntity.Summoner.GetSummonPosition();
            CacheEntity.SetTargetEntity(null);
            SetDestination(time, randomPosition);
        }

        public void AggressiveFindTarget(float time, Vector3 currentPosition)
        {
            // Aggressive monster or summoned monster will find target to attack
            if (MonsterDatabase.characteristic != MonsterCharacteristic.Aggressive &&
                CacheMonsterCharacterEntity.Summoner == null)
                return;

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
                    SetStartFollowTargetTime(time);
                    CacheMonsterCharacterEntity.SetAttackTarget(characterEntity);
                    break;
                }
            }
        }
    }
}
