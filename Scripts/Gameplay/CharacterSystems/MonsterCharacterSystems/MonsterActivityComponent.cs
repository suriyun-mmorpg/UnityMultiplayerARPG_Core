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
        
        protected readonly List<BaseCharacterEntity> enemies = new List<BaseCharacterEntity>();
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
        protected float lastSetDestinationTime;
        protected bool previousIsInSafeArea;

        public bool IsAggressiveWhileSummonerIdle()
        {
            return (isAggressiveWhileSummonerIdle || Entity.Characteristic == MonsterCharacteristic.Aggressive) && Entity.Characteristic != MonsterCharacteristic.NoHarm;
        }

        public override void EntityUpdate()
        {
            if (!Entity.IsServer || Entity.Identity.CountSubscribers() == 0 || CharacterDatabase == null)
                return;

            if (Entity.IsDead())
            {
                Entity.StopMove();
                Entity.SetTargetEntity(null);
                return;
            }

            float deltaTime = Time.unscaledDeltaTime;

            Vector3 currentPosition = Entity.MovementTransform.position;
            if (Entity.Summoner != null)
            {
                if (!UpdateAttackEnemy(deltaTime, currentPosition))
                {
                    if (startedAggressive)
                    {
                        aggressiveElasped += deltaTime;
                        // Find target when it's time
                        if ((enemies.Count > 0 || aggressiveElasped >= findEnemyDelay) &&
                            FindEnemy(currentPosition))
                        {
                            aggressiveElasped = 0f;
                            startedAggressive = false;
                        }
                    }

                    if (Vector3.Distance(currentPosition, Entity.Summoner.CacheTransform.position) > CurrentGameInstance.minFollowSummonerDistance)
                    {
                        // Follow summoner
                        FollowSummoner();
                        startedAggressive = IsAggressiveWhileSummonerIdle();
                    }
                    else
                    {
                        // Wandering when it's time
                        randomedWanderElasped += deltaTime;
                        if (randomedWanderElasped >= randomedWanderDelay)
                        {
                            randomedWanderElasped = 0f;
                            RandomWanderDestination();
                            startedAggressive = IsAggressiveWhileSummonerIdle();
                        }
                    }
                    startedFollowEnemy = false;
                }
            }
            else
            {
                if (Entity.IsInSafeArea)
                {
                    // If monster move into safe area, wander to another place
                    randomedWanderElasped += deltaTime;
                    if (!previousIsInSafeArea || randomedWanderElasped >= randomedWanderDelay)
                    {
                        randomedWanderElasped = 0f;
                        RandomWanderDestination();
                    }
                    previousIsInSafeArea = true;
                    startedFollowEnemy = false;
                    return;
                }
                previousIsInSafeArea = false;

                if (!UpdateAttackEnemy(deltaTime, currentPosition))
                {
                    if (startedAggressive)
                    {
                        aggressiveElasped += deltaTime;
                        // Find target when it's time
                        if ((enemies.Count > 0 || aggressiveElasped >= findEnemyDelay) &&
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
            if (!Entity.TryGetTargetEntity(out tempTargetEnemy) || Entity.Characteristic == MonsterCharacteristic.NoHarm)
            {
                // No target, stop attacking
                ClearActionState();
                return false;
            }

            if (tempTargetEnemy.Entity == Entity.Entity || tempTargetEnemy.IsHideOrDead() || !tempTargetEnemy.CanReceiveDamageFrom(Entity.GetInfo()))
            {
                // If target is dead or in safe area stop attacking
                Entity.SetTargetEntity(null);
                ClearActionState();
                return false;
            }

            // If it has target then go to target
            if (tempTargetEnemy != null && !Entity.IsPlayingActionAnimation() && !alreadySetActionState)
            {
                // Random action state to do next time
                if (CharacterDatabase.RandomSkill(Entity, out queueSkill, out queueSkillLevel) && queueSkill != null)
                {
                    // Cooling down
                    if (Entity.IndexOfSkillUsage(queueSkill.DataId, SkillUsageType.Skill) >= 0)
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
                SetWanderDestination(CacheTransform.position);
                // Lookat target then do something when it's in range
                Vector3 lookAtDirection = (targetPosition - currentPosition).normalized;
                if (lookAtDirection.sqrMagnitude > 0)
                {
                    if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                    {
                        Quaternion currentLookAtRotation = Entity.GetLookRotation();
                        Vector3 lookRotationEuler = Quaternion.LookRotation(lookAtDirection).eulerAngles;
                        lookRotationEuler.x = 0;
                        lookRotationEuler.z = 0;
                        currentLookAtRotation = Quaternion.RotateTowards(currentLookAtRotation, Quaternion.Euler(lookRotationEuler), turnToEnemySpeed * Time.deltaTime);
                        Entity.SetLookRotation(currentLookAtRotation);
                    }
                    else
                    {
                        // Update 2D direction
                        Entity.SetLookRotation(Quaternion.LookRotation(lookAtDirection));
                    }
                }

                Entity.AimPosition = Entity.GetAttackAimPosition(ref isLeftHandAttacking);
                if (Entity.IsPlayingActionAnimation())
                    return true;

                if (queueSkill != null && Entity.IndexOfSkillUsage(queueSkill.DataId, SkillUsageType.Skill) < 0)
                {
                    // Use skill when there is queue skill or randomed skill that can be used
                    Entity.UseSkill(queueSkill.DataId, false, 0, new AimPosition()
                    {
                        type = AimPositionType.Position,
                        position = tempTargetEnemy.OpponentAimTransform.position,
                    });
                }
                else
                {
                    // Attack when no queue skill
                    Entity.Attack(false);
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

                if (Entity.Summoner == null)
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
            float time = Time.unscaledTime;
            if (time - lastSetDestinationTime <= 0.1f)
                return;
            lastSetDestinationTime = time;
            Vector3 direction = (destination - Entity.MovementTransform.position).normalized;
            Vector3 position = destination - (direction * (distance - Entity.StoppingDistance));
            Entity.SetExtraMovementState(ExtraMovementState.None);
            Entity.PointClickMovement(position);
        }

        public void SetWanderDestination(Vector3 destination)
        {
            float time = Time.unscaledTime;
            if (time - lastSetDestinationTime <= 0.1f)
                return;
            lastSetDestinationTime = time;
            Entity.SetExtraMovementState(ExtraMovementState.IsWalking);
            Entity.PointClickMovement(destination);
        }

        public virtual void RandomWanderDestination()
        {
            Vector3 randomPosition;
            // Random position around summoner or around spawn point
            if (Entity.Summoner != null)
            {
                // Random position around summoner
                randomPosition = CurrentGameInstance.GameplayRule.GetSummonPosition(Entity.Summoner);
            }
            else
            {
                // Random position around spawn point
                Vector2 randomCircle = Random.insideUnitCircle * randomWanderDistance;
                if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                    randomPosition = Entity.SpawnPosition + new Vector3(randomCircle.x, randomCircle.y);
                else
                    randomPosition = Entity.SpawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
            }

            Entity.SetTargetEntity(null);
            SetWanderDestination(randomPosition);
            randomedWanderDelay = Random.Range(randomWanderDelayMin, randomWanderDelayMax);
        }

        public virtual void FollowSummoner()
        {
            Vector3 randomPosition;
            // Random position around summoner or around spawn point
            if (Entity.Summoner != null)
            {
                // Random position around summoner
                randomPosition = GameInstance.Singleton.GameplayRule.GetSummonPosition(Entity.Summoner);
            }
            else
            {
                // Random position around spawn point
                Vector2 randomCircle = Random.insideUnitCircle * randomWanderDistance;
                if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                    randomPosition = Entity.SpawnPosition + new Vector3(randomCircle.x, randomCircle.y);
                else
                    randomPosition = Entity.SpawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
            }

            Entity.SetTargetEntity(null);
            SetDestination(randomPosition, 0f);
        }

        /// <summary>
        /// Return `TRUE` if found enemy
        /// </summary>
        /// <param name="currentPosition"></param>
        /// <returns></returns>
        public virtual bool FindEnemy(Vector3 currentPosition)
        {
            // Aggressive monster or summoned monster will find target to attack
            if (Entity.Characteristic != MonsterCharacteristic.Aggressive &&
                Entity.Summoner == null)
                return false;

            IDamageableEntity targetEntity;
            if (!Entity.TryGetTargetEntity(out targetEntity) || targetEntity.Entity == Entity.Entity ||
                 targetEntity.IsDead() || !targetEntity.CanReceiveDamageFrom(Entity.GetInfo()))
            {
                BaseCharacterEntity enemy;
                for (int i = enemies.Count - 1; i >= 0; --i)
                {
                    enemy = enemies[i];
                    enemies.RemoveAt(i);
                    if (enemy != null && enemy.Entity != Entity.Entity && !enemy.IsDead() &&
                        enemy.CanReceiveDamageFrom(Entity.GetInfo()))
                    {
                        // Found target, attack it
                        Entity.SetAttackTarget(enemy);
                        return true;
                    }
                }

                // If no target enenmy or target enemy is dead, Find nearby character by layer mask
                enemies.Clear();
                if (Entity.IsSummoned)
                {
                    // Find enemy around summoner
                    enemies.AddRange(Entity.FindAliveCharacters<BaseCharacterEntity>(
                        Entity.Summoner.CacheTransform.position,
                        CharacterDatabase.SummonVisualRange,
                        false, /* Don't find an allies */
                        true,  /* Always find an enemies */
                        Entity.IsSummoned && IsAggressiveWhileSummonerIdle() /* Find enemy while summoned and aggresively */));
                }
                else
                {
                    enemies.AddRange(Entity.FindAliveCharacters<BaseCharacterEntity>(
                        CharacterDatabase.VisualRange,
                        false, /* Don't find an allies */
                        true,  /* Always find an enemies */
                        Entity.IsSummoned && IsAggressiveWhileSummonerIdle() /* Find enemy while summoned and aggresively */));
                }

                for (int i = enemies.Count - 1; i >= 0; --i)
                {
                    enemy = enemies[i];
                    enemies.RemoveAt(i);
                    if (enemy != null && enemy.Entity != Entity.Entity && !enemy.IsDead() &&
                        enemy.CanReceiveDamageFrom(Entity.GetInfo()))
                    {
                        // Found target, attack it
                        Entity.SetAttackTarget(enemy);
                        return true;
                    }
                }

                if (!isAttackBuilding)
                    return false;
                // Find building to attack
                List<BuildingEntity> buildingEntities = Entity.FindAliveDamageableEntities<BuildingEntity>(CharacterDatabase.VisualRange, CurrentGameInstance.buildingLayer.Mask);
                foreach (BuildingEntity buildingEntity in buildingEntities)
                {
                    // Attack target settings
                    if (buildingEntity == null || buildingEntity.Entity == Entity.Entity ||
                        buildingEntity.IsDead() || !buildingEntity.CanReceiveDamageFrom(Entity.GetInfo()))
                    {
                        // If building is null or cannot receive damage from monster, skip it
                        continue;
                    }
                    if (Entity.Summoner != null)
                    {
                        if (Entity.Summoner.Id.Equals(buildingEntity.CreatorId))
                        {
                            // If building was built by summoner, skip it
                            continue;
                        }
                    }
                    // Found target, attack it
                    Entity.SetAttackTarget(buildingEntity);
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
            return queueSkill != null ? queueSkill.GetApplyTransform(Entity, isLeftHandAttacking) :
                Entity.GetWeaponDamageInfo(null).GetDamageTransform(Entity, isLeftHandAttacking);
        }

        protected float GetAttackDistance()
        {
            return queueSkill != null && queueSkill.IsAttack ? queueSkill.GetCastDistance(Entity, queueSkillLevel, isLeftHandAttacking) :
                Entity.GetAttackDistance(isLeftHandAttacking);
        }

        protected virtual bool OverlappedEntity<T>(T entity, Vector3 measuringPosition, Vector3 targetPosition, float distance)
            where T : BaseGameEntity
        {
            if (Vector3.Distance(measuringPosition, targetPosition) <= distance)
                return true;
            // Target is far from controlling entity, try overlap the entity
            return Entity.FindPhysicFunctions.IsGameEntityInDistance(entity, measuringPosition, distance, false);
        }
    }
}
