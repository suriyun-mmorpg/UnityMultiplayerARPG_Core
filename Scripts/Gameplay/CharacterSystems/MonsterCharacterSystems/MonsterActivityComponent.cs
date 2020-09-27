using System.Collections.Generic;
using UnityEngine;

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
        [Tooltip("If this is TRUE, monster will attacks buildings")]
        public bool isAttackBuilding = false;
        [Tooltip("If this is TRUE, monster will attacks targets while its summoner still idle")]
        public bool isAggressiveWhileSummonerIdle = false;

        protected bool startedAggressive;
        protected float aggressiveElasped;
        protected float randomedWanderElasped;
        protected float randomedWanderDelay;
        protected bool startedFollowEnemy;
        protected float startFollowEnemyElasped;
        protected Vector3 lastPosition;
        protected IDamageableEntity tempTargetEnemy;
        protected BaseSkill queueSkill;
        protected short queueSkillLevel;
        protected bool alreadySetActionState;
        protected bool isLeftHandAttacking;

        public override void EntityUpdate()
        {
            if (!IsServer || CacheEntity.Identity.CountSubscribers() == 0 || MonsterDatabase == null)
                return;

            if (CacheEntity.IsDead())
            {
                CacheEntity.StopMove();
                CacheEntity.SetTargetEntity(null);
                return;
            }

            float deltaTime = Time.unscaledDeltaTime;

            Vector3 currentPosition = CacheEntity.MovementTransform.position;
            if (CacheEntity.Summoner != null)
            {
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

                    if (Vector3.Distance(currentPosition, CacheEntity.Summoner.CacheTransform.position) > CurrentGameInstance.minFollowSummonerDistance)
                    {
                        // Follow summoner
                        FollowSummoner();
                        startedAggressive = isAggressiveWhileSummonerIdle;
                    }
                    else
                    {
                        // Wandering when it's time
                        randomedWanderElasped += deltaTime;
                        if (randomedWanderElasped >= randomedWanderDelay)
                        {
                            randomedWanderElasped = 0f;
                            RandomWanderDestination();
                            startedAggressive = isAggressiveWhileSummonerIdle;
                        }
                    }
                    startedFollowEnemy = false;
                }
            }
            else
            {
                if (CacheEntity.IsInSafeArea)
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
                ClearActionState();
                return false;
            }

            if (tempTargetEnemy.Entity == CacheEntity.Entity || tempTargetEnemy.IsHideOrDead() || !tempTargetEnemy.CanReceiveDamageFrom(CacheEntity))
            {
                // If target is dead or in safe area stop attacking
                CacheEntity.SetTargetEntity(null);
                ClearActionState();
                return false;
            }

            // If it has target then go to target
            if (tempTargetEnemy != null && !CacheEntity.IsPlayingActionAnimation() && !alreadySetActionState)
            {
                // Random action state to do next time
                if (MonsterDatabase.RandomSkill(CacheEntity, out queueSkill, out queueSkillLevel) && queueSkill != null)
                {
                    // Cooling down
                    if (CacheEntity.IndexOfSkillUsage(queueSkill.DataId, SkillUsageType.Skill) >= 0)
                    {
                        queueSkill = null;
                        queueSkillLevel = 0;
                    }
                }
                isLeftHandAttacking = !isLeftHandAttacking;
                alreadySetActionState = true;
                return true;
            }

            Vector3 targetPosition = tempTargetEnemy.GetTransform().position;
            float attackDistance = GetAttackDistance();
            if (OverlappedEntity(tempTargetEnemy.Entity, GetDamageTransform().position, targetPosition, attackDistance))
            {
                startedFollowEnemy = false;
                CacheEntity.StopMove();
                // Lookat target then do something when it's in range
                Vector3 lookAtDirection = (targetPosition - currentPosition).normalized;
                if (lookAtDirection.sqrMagnitude > 0)
                {
                    if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                    {
                        Quaternion currentLookAtRotation = CacheEntity.GetLookRotation();
                        Vector3 lookRotationEuler = Quaternion.LookRotation(lookAtDirection).eulerAngles;
                        lookRotationEuler.x = 0;
                        lookRotationEuler.z = 0;
                        currentLookAtRotation = Quaternion.RotateTowards(currentLookAtRotation, Quaternion.Euler(lookRotationEuler), turnToEnemySpeed * Time.deltaTime);
                        CacheEntity.SetLookRotation(currentLookAtRotation);
                    }
                    else
                    {
                        // Update 2D direction
                        CacheEntity.SetLookRotation(Quaternion.LookRotation(lookAtDirection));
                    }
                }

                if (CacheEntity.IsPlayingActionAnimation())
                    return true;

                if (queueSkill != null && CacheEntity.IndexOfSkillUsage(queueSkill.DataId, SkillUsageType.Skill) < 0)
                {
                    // Use skill when there is queue skill or randomed skill that can be used
                    CacheEntity.CallServerUseSkill(queueSkill.DataId, false, tempTargetEnemy.OpponentAimTransform.position);
                }
                else
                {
                    // Attack when no queue skill
                    CacheEntity.CallServerAttack(false);
                }

                ClearActionState();
            }
            else
            {
                if (!startedFollowEnemy)
                {
                    startFollowEnemyElasped = 0f;
                    startedFollowEnemy = true;
                }

                // Update destination if target's position changed
                SetDestination(targetPosition, attackDistance);

                if (CacheEntity.Summoner == null)
                {
                    startFollowEnemyElasped += deltaTime;
                    // Stop following target and move to position nearby spawn position when follow enemy too long
                    if (startFollowEnemyElasped >= followTargetDuration)
                        RandomWanderDestination();
                }
            }
            return true;
        }

        public void SetDestination(Vector3 destination, float distance)
        {
            Vector3 direction = (destination - CacheEntity.MovementTransform.position).normalized;
            Vector3 position = destination - (direction * (distance - CacheEntity.StoppingDistance));
            CacheEntity.SetExtraMovement(ExtraMovementState.None);
            CacheEntity.PointClickMovement(position);
        }

        public void SetWanderDestination(Vector3 destination)
        {
            CacheEntity.SetExtraMovement(ExtraMovementState.IsWalking);
            CacheEntity.PointClickMovement(destination);
        }

        public void RandomWanderDestination()
        {
            // Random position around spawn point
            Vector3 randomPosition;
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                randomPosition = CacheEntity.SpawnPosition + new Vector3(Random.Range(-1f, 1f) * randomWanderDistance, 0, Random.Range(-1f, 1f) * randomWanderDistance);
            else
                randomPosition = CacheEntity.SpawnPosition + new Vector3(Random.Range(-1f, 1f) * randomWanderDistance, Random.Range(-1f, 1f) * randomWanderDistance);
            // Random position around summoner
            if (CacheEntity.Summoner != null)
                randomPosition = CacheEntity.Summoner.GetSummonPosition();

            CacheEntity.SetTargetEntity(null);
            SetWanderDestination(randomPosition);
            randomedWanderDelay = Random.Range(randomWanderDelayMin, randomWanderDelayMax);
        }

        public void FollowSummoner()
        {
            // Random position around spawn point
            Vector3 randomPosition;
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                randomPosition = CacheEntity.SpawnPosition + new Vector3(Random.Range(-1f, 1f) * randomWanderDistance, 0, Random.Range(-1f, 1f) * randomWanderDistance);
            else
                randomPosition = CacheEntity.SpawnPosition + new Vector3(Random.Range(-1f, 1f) * randomWanderDistance, Random.Range(-1f, 1f) * randomWanderDistance);
            // Random position around summoner
            if (CacheEntity.Summoner != null)
                randomPosition = CacheEntity.Summoner.GetSummonPosition();

            CacheEntity.SetTargetEntity(null);
            SetDestination(randomPosition, 0f);
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
                CacheEntity.Summoner == null)
                return false;

            IDamageableEntity targetEntity;
            if (!CacheEntity.TryGetTargetEntity(out targetEntity) || targetEntity.Entity == CacheEntity.Entity ||
                 targetEntity.IsDead() || !targetEntity.CanReceiveDamageFrom(CacheEntity))
            {
                // If no target enenmy or target enemy is dead, Find nearby character by layer mask
                List<BaseCharacterEntity> characterEntities = CacheEntity.FindAliveCharacters<BaseCharacterEntity>(
                    MonsterDatabase.visualRange,
                    false, /* Don't find an allies */
                    true,  /* Always find an enemies */
                    CacheEntity.IsSummoned && isAggressiveWhileSummonerIdle /* Find enemy while summoned and aggresively */);
                foreach (BaseCharacterEntity characterEntity in characterEntities)
                {
                    // Attack target settings
                    if (characterEntity == null || characterEntity.Entity == CacheEntity.Entity ||
                        characterEntity.IsDead() || !characterEntity.CanReceiveDamageFrom(CacheEntity))
                    {
                        // If character is null or cannot receive damage from monster, skip it
                        continue;
                    }
                    // Found target, attack it
                    CacheEntity.SetAttackTarget(characterEntity);
                    return true;
                }
                if (!isAttackBuilding)
                    return false;
                // Find building to attack
                List<BuildingEntity> buildingEntities = CacheEntity.FindAliveDamageableEntities<BuildingEntity>(MonsterDatabase.visualRange, CurrentGameInstance.buildingLayer.Mask);
                foreach (BuildingEntity buildingEntity in buildingEntities)
                {
                    // Attack target settings
                    if (buildingEntity == null || buildingEntity.Entity == CacheEntity.Entity ||
                        buildingEntity.IsDead() || !buildingEntity.CanReceiveDamageFrom(CacheEntity))
                    {
                        // If building is null or cannot receive damage from monster, skip it
                        continue;
                    }
                    if (CacheEntity.Summoner != null)
                    {
                        if (CacheEntity.Summoner.Id.Equals(buildingEntity.CreatorId))
                        {
                            // If building was built by summoner, skip it
                            continue;
                        }
                    }
                    // Found target, attack it
                    CacheEntity.SetAttackTarget(buildingEntity);
                    return true;
                }
            }

            return false;
        }

        protected virtual void ClearActionState()
        {
            queueSkill = null;
            isLeftHandAttacking = false;
            alreadySetActionState = false;
        }

        protected Transform GetDamageTransform()
        {
            return queueSkill != null ? queueSkill.GetApplyTransform(CacheEntity, isLeftHandAttacking) :
                CacheEntity.GetWeaponDamageInfo(ref isLeftHandAttacking).GetDamageTransform(CacheEntity, isLeftHandAttacking);
        }

        protected float GetAttackDistance()
        {
            return queueSkill != null && queueSkill.IsAttack() ? queueSkill.GetCastDistance(CacheEntity, queueSkillLevel, isLeftHandAttacking) :
                CacheEntity.GetAttackDistance(isLeftHandAttacking);
        }

        protected virtual bool OverlappedEntity<T>(T entity, Vector3 measuringPosition, Vector3 targetPosition, float distance)
            where T : BaseGameEntity
        {
            if (Vector3.Distance(measuringPosition, targetPosition) <= distance)
                return true;
            // Target is far from controlling entity, try overlap the entity
            return CacheEntity.FindPhysicFunctions.IsGameEntityInDistance(entity, measuringPosition, distance, false);
        }
    }
}
