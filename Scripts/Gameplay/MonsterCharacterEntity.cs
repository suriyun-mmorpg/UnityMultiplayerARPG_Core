using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using LiteNetLibHighLevel;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(LiteNetLibTransform))]
public class MonsterCharacterEntity : CharacterEntity
{
    public const float RANDOM_WANDER_DURATION_MIN = 2f;
    public const float RANDOM_WANDER_DURATION_MAX = 5f;
    public const float RANDOM_WANDER_AREA_MIN = 2f;
    public const float RANDOM_WANDER_AREA_MAX = 5f;
    public const float AGGRESSIVE_FIND_TARGET_DELAY = 1f;
    public const float SET_TARGET_DESTINATION_DELAY = 1f;
    public const float FOLLOW_TARGET_DURATION = 5f;

    #region Protected data
    protected MonsterCharacterDatabase database;
    protected float wanderTime;
    protected float findTargetTime;
    protected float setTargetDestinationTime;
    protected float startFollowTargetCountTime;
    protected Vector3? wanderDestination;
    protected Vector3 oldMovePosition;
    #endregion

    #region Cache components
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

    private LiteNetLibTransform cacheNetTransform;
    public LiteNetLibTransform CacheNetTransform
    {
        get
        {
            if (cacheNetTransform == null)
                cacheNetTransform = GetComponent<LiteNetLibTransform>();
            return cacheNetTransform;
        }
    }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        var gameInstance = GameInstance.Singleton;
        gameObject.tag = gameInstance.monsterTag;
        RandomWanderTime();
        SetFindTargetTime();
    }

    protected override void Update()
    {
        base.Update();

        if (CurrentHp <= 0)
        {
            ClearDestination();
            return;
        }

        UpdateActivity();
    }

    protected virtual void ClearDestination()
    {
        SetTargetEntity(null);
        CacheNavMeshAgent.isStopped = true;
        wanderDestination = null;
    }

    protected virtual void UpdateActivity()
    {
        if (!IsServer || database == null || CurrentHp <= 0)
            return;

        var gameInstance = GameInstance.Singleton;
        var currentPosition = CacheTransform.position;
        CharacterEntity targetEntity;
        if (TryGetTargetEntity(out targetEntity))
        {
            // If it has target then go to target
            var targetPosition = targetEntity.CacheTransform.position;
            var attackDistance = GetAttackDistance();
            attackDistance -= attackDistance * 0.1f;
            attackDistance -= CacheNavMeshAgent.stoppingDistance;
            attackDistance += targetEntity.CacheCapsuleCollider.radius;
            if (Vector3.Distance(currentPosition, targetPosition) <= attackDistance)
            {
                startFollowTargetCountTime = Time.realtimeSinceStartup;
                // Lookat target then do anything when it's in range
                CacheNavMeshAgent.updateRotation = false;
                CacheNavMeshAgent.isStopped = true;
                var lookAtDirection = (targetPosition - currentPosition).normalized;
                // slerp to the desired rotation over time
                CacheTransform.rotation = Quaternion.RotateTowards(CacheTransform.rotation, Quaternion.LookRotation(lookAtDirection), CacheNavMeshAgent.angularSpeed * Time.deltaTime);
                Attack();
                // TODO: Random to use skills
            }
            else
            {
                // Following target
                CacheNavMeshAgent.updateRotation = true;
                if (oldMovePosition != targetPosition &&
                    Time.realtimeSinceStartup - setTargetDestinationTime >= SET_TARGET_DESTINATION_DELAY)
                {
                    setTargetDestinationTime = Time.realtimeSinceStartup;
                    CacheNavMeshAgent.speed = this.GetStatsWithBuffs().moveSpeed;
                    CacheNavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
                    CacheNavMeshAgent.SetDestination(targetPosition);
                    CacheNavMeshAgent.isStopped = false;
                    oldMovePosition = targetPosition;
                }
                if (Time.realtimeSinceStartup - startFollowTargetCountTime >= FOLLOW_TARGET_DURATION)
                {
                    ClearDestination();
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
                || oldMovePosition != currentPosition)
            {
                RandomWanderTime();
                oldMovePosition = currentPosition;
            }
            // Wandering when it's time
            if (Time.realtimeSinceStartup >= wanderTime)
            {
                // If stopped then random
                var randomX = Random.Range(RANDOM_WANDER_AREA_MIN, RANDOM_WANDER_AREA_MAX) * (Random.value > 0.5f ? -1 : 1);
                var randomZ = Random.Range(RANDOM_WANDER_AREA_MIN, RANDOM_WANDER_AREA_MAX) * (Random.value > 0.5f ? -1 : 1);
                var randomPosition = currentPosition + new Vector3(randomX, 0, randomZ);
                NavMeshHit navMeshHit;
                if (NavMesh.SamplePosition(randomPosition, out navMeshHit, RANDOM_WANDER_AREA_MAX, 1))
                {
                    wanderDestination = navMeshHit.position;
                    CacheNavMeshAgent.speed = database.wanderMoveSpeed;
                    CacheNavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
                    CacheNavMeshAgent.SetDestination(wanderDestination.Value);
                    CacheNavMeshAgent.isStopped = false;
                }
            }
            else
            {
                // If it's aggressive character, finding attacking target
                if (database.characteristic == MonsterCharacteristic.Aggressive &&
                    Time.realtimeSinceStartup >= findTargetTime)
                {
                    SetFindTargetTime();
                    CharacterEntity targetCharacter;
                    // If no target enenmy or target enemy is dead
                    if (!TryGetTargetEntity(out targetCharacter) || targetCharacter.CurrentHp <= 0)
                    {
                        // Find nearby character by layer mask
                        var foundObjects = new List<Collider>(Physics.OverlapSphere(currentPosition, database.visualRange, gameInstance.characterLayer.Mask));
                        foundObjects = foundObjects.OrderBy(a => System.Guid.NewGuid()).ToList();
                        foreach (var foundObject in foundObjects)
                        {
                            var characterEntity = foundObject.GetComponent<CharacterEntity>();
                            if (characterEntity != null && IsEnemy(characterEntity))
                            {
                                startFollowTargetCountTime = Time.realtimeSinceStartup;
                                SetAttackTarget(characterEntity);
                            }
                        }
                    }
                }
            }
        }
    }

    public override void OnSetup()
    {
        base.OnSetup();

        CacheNetTransform.ownerClientCanSendTransform = false;
    }

    protected void RandomWanderTime()
    {
        wanderTime = Time.realtimeSinceStartup + Random.Range(RANDOM_WANDER_DURATION_MIN, RANDOM_WANDER_DURATION_MAX);
    }

    protected void SetFindTargetTime()
    {
        findTargetTime = Time.realtimeSinceStartup + AGGRESSIVE_FIND_TARGET_DELAY;
    }

    protected override bool IsAlly(CharacterEntity characterEntity)
    {
        if (characterEntity == null)
            return false;
        // If this character have been attacked by any character
        // It will tell another ally nearby to help
        var monsterCharacterEntity = characterEntity as MonsterCharacterEntity;
        if (monsterCharacterEntity != null && monsterCharacterEntity.database.allyId == database.allyId)
            return true;
        return false;
    }

    protected override bool IsEnemy(CharacterEntity characterEntity)
    {
        return true;
    }

    public void SetAttackTarget(CharacterEntity target)
    {
        if (target == null || target.CurrentHp <= 0)
            return;
        // Already have target so don't set target
        CharacterEntity oldTarget;
        if (TryGetTargetEntity(out oldTarget) && oldTarget.CurrentHp > 0)
            return;
        // Set target to attack
        SetTargetEntity(target);
    }

    public override void ReceiveDamage(CharacterEntity attacker, Dictionary<DamageElement, DamageAmount> allDamageAttributes, CharacterBuff debuff)
    {
        // Damage calculations apply at server only
        if (!IsServer)
            return;
        base.ReceiveDamage(attacker, allDamageAttributes, debuff);
        // If no attacker, skip next logics
        if (attacker == null)
            return;
        // If character isn't dead
        if (CurrentHp > 0)
        {
            var gameInstance = GameInstance.Singleton;
            // If no target enemy and current target is character, try to attack
            CharacterEntity targetEntity;
            if (!TryGetTargetEntity(out targetEntity))
            {
                SetAttackTarget(attacker);
                // If it's assist character call another character for assist
                if (database.characteristic == MonsterCharacteristic.Assist)
                {
                    var foundObjects = new List<Collider>(Physics.OverlapSphere(CacheTransform.position, database.visualRange, gameInstance.characterLayer.Mask));
                    foreach (var foundObject in foundObjects)
                    {
                        var monsterCharacterEntity = foundObject.GetComponent<MonsterCharacterEntity>();
                        if (monsterCharacterEntity != null && IsAlly(monsterCharacterEntity))
                            monsterCharacterEntity.SetAttackTarget(attacker);
                    }
                }
            }
            else if (attacker != targetEntity && Random.Range(0, 1) == 1)
            {
                // Random 50% to change target when receive damage from anyone
                SetAttackTarget(attacker);
            }
        }
    }

    protected override void OnDatabaseIdChange(string databaseId)
    {
        base.OnDatabaseIdChange(databaseId);
        GameInstance.MonsterCharacterDatabases.TryGetValue(databaseId, out database);
    }

    public override void GetAttackData(
        float inflictRate, 
        Dictionary<DamageElement, DamageAmount> additionalDamageAttributes, 
        out int actionId, 
        out float damageDuration, 
        out float totalDuration, 
        out Dictionary<DamageElement, DamageAmount> allDamageAttributes, 
        out DamageInfo damageInfo)
    {
        var gameInstance = GameInstance.Singleton;

        // Initialize attack animation
        actionId = -1;
        damageDuration = 0f;
        totalDuration = 0f;

        // Random attack animation
        var animArray = database.attackAnimations;
        var animLength = animArray.Length;
        if (animLength > 0)
        {
            var anim = animArray[Random.Range(0, animLength - 1)];
            // Assign animation data
            actionId = anim.actionId;
            damageDuration = anim.triggerDuration;
            totalDuration = anim.totalDuration;
        }

        // Assign damage attributes
        allDamageAttributes = new Dictionary<DamageElement, DamageAmount>();
        var damageElement = database.damageElement;
        var damageAmount = database.damageAmount;
        if (damageElement == null)
            damageElement = gameInstance.DefaultDamageElement;
        allDamageAttributes.Add(damageElement, damageAmount * inflictRate);
        allDamageAttributes = GameDataHelpers.CombineDamageAttributesDictionary(allDamageAttributes, additionalDamageAttributes);
        // Assign damage data
        damageInfo = database.damageInfo;
    }

    public override float GetAttackDistance()
    {
        return database.damageInfo.GetDistance();
    }
}
