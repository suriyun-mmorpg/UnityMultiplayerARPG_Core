using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using LiteNetLibManager;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(LiteNetLibTransform))]
public class MonsterCharacterEntity : BaseCharacterEntity
{
    public const float RANDOM_WANDER_DURATION_MIN = 2f;
    public const float RANDOM_WANDER_DURATION_MAX = 5f;
    public const float RANDOM_WANDER_AREA_MIN = 2f;
    public const float RANDOM_WANDER_AREA_MAX = 5f;
    public const float AGGRESSIVE_FIND_TARGET_DELAY = 2f;
    public const float SET_TARGET_DESTINATION_DELAY = 1f;
    public const float FOLLOW_TARGET_DURATION = 5f;

    #region Protected data
    protected float wanderTime;
    protected float findTargetTime;
    protected float setTargetDestinationTime;
    protected float startFollowTargetCountTime;
    protected float receivedDamageRecordsUpdateTime;
    protected float deadTime;
    protected Vector3? wanderDestination;
    protected Vector3 oldMovePosition;
    protected readonly Dictionary<BaseCharacterEntity, ReceivedDamageRecord> receivedDamageRecords = new Dictionary<BaseCharacterEntity, ReceivedDamageRecord>();
    #endregion

    #region Public data
    public Vector3 respawnPosition;
    #endregion

    #region Interface implementation
    public override string CharacterName
    {
        get { return MonsterDatabase == null ? "Unknow" : MonsterDatabase.title; }
        set { }
    }
    #endregion

    #region Fields/Cache components
    public MonsterCharacter MonsterDatabase
    {
        get { return database as MonsterCharacter; }
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

        if (MonsterDatabase == null || !IsServer)
            return;

        if (CurrentHp <= 0)
        {
            StopMove();
            SetTargetEntity(null);
            if (Time.realtimeSinceStartup - deadTime >= MonsterDatabase.deadHideDelay)
                isHidding.Value = true;
            if (Time.realtimeSinceStartup - deadTime >= MonsterDatabase.deadRespawnDelay)
                Respawn();
            return;
        }

        UpdateActivity();
    }

    protected virtual void StopMove()
    {
        CacheNavMeshAgent.isStopped = true;
        CacheNavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        wanderDestination = null;
    }

    protected virtual void UpdateActivity()
    {
        if (!IsServer || MonsterDatabase == null || CurrentHp <= 0)
            return;

        var gameInstance = GameInstance.Singleton;
        var currentPosition = CacheTransform.position;
        BaseCharacterEntity targetEntity;
        if (TryGetTargetEntity(out targetEntity))
        {
            if (targetEntity.CurrentHp <= 0)
            {
                StopMove();
                SetTargetEntity(null);
                return;
            }
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
                if (lookAtDirection.magnitude > 0)
                    CacheTransform.rotation = Quaternion.RotateTowards(CacheTransform.rotation, Quaternion.LookRotation(lookAtDirection), CacheNavMeshAgent.angularSpeed * Time.deltaTime);
                RequestAttack();
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
                    CacheNavMeshAgent.speed = MoveSpeed * gameInstance.moveSpeedMultiplier;
                    CacheNavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
                    CacheNavMeshAgent.SetDestination(targetPosition);
                    CacheNavMeshAgent.isStopped = false;
                    oldMovePosition = targetPosition;
                }
                if (Time.realtimeSinceStartup - startFollowTargetCountTime >= FOLLOW_TARGET_DURATION)
                {
                    StopMove();
                    SetTargetEntity(null);
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
                var randomPosition = respawnPosition + new Vector3(randomX, 0, randomZ);
                NavMeshHit navMeshHit;
                if (NavMesh.SamplePosition(randomPosition, out navMeshHit, RANDOM_WANDER_AREA_MAX, 1))
                {
                    wanderDestination = navMeshHit.position;
                    CacheNavMeshAgent.speed = MonsterDatabase.wanderMoveSpeed * gameInstance.moveSpeedMultiplier;
                    CacheNavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
                    CacheNavMeshAgent.SetDestination(wanderDestination.Value);
                    CacheNavMeshAgent.isStopped = false;
                }
            }
            else
            {
                // If it's aggressive character, finding attacking target
                if (MonsterDatabase.characteristic == MonsterCharacteristic.Aggressive &&
                    Time.realtimeSinceStartup >= findTargetTime)
                {
                    SetFindTargetTime();
                    BaseCharacterEntity targetCharacter;
                    // If no target enenmy or target enemy is dead
                    if (!TryGetTargetEntity(out targetCharacter) || targetCharacter.CurrentHp <= 0)
                    {
                        // Find nearby character by layer mask
                        var foundObjects = new List<Collider>(Physics.OverlapSphere(currentPosition, MonsterDatabase.visualRange, gameInstance.characterLayer.Mask));
                        foundObjects = foundObjects.OrderBy(a => System.Guid.NewGuid()).ToList();
                        foreach (var foundObject in foundObjects)
                        {
                            var characterEntity = foundObject.GetComponent<BaseCharacterEntity>();
                            if (characterEntity != null && IsEnemy(characterEntity))
                            {
                                startFollowTargetCountTime = Time.realtimeSinceStartup;
                                SetAttackTarget(characterEntity);
                                return;
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

    protected override bool CanReceiveDamageFrom(BaseCharacterEntity characterEntity)
    {
        return characterEntity != null && characterEntity is PlayerCharacterEntity;
    }

    protected override bool IsAlly(BaseCharacterEntity characterEntity)
    {
        if (characterEntity == null)
            return false;
        // If this character have been attacked by any character
        // It will tell another ally nearby to help
        var monsterCharacterEntity = characterEntity as MonsterCharacterEntity;
        if (monsterCharacterEntity != null && monsterCharacterEntity.MonsterDatabase.allyId == MonsterDatabase.allyId)
            return true;
        return false;
    }

    protected override bool IsEnemy(BaseCharacterEntity characterEntity)
    {
        return characterEntity != null && characterEntity is PlayerCharacterEntity;
    }

    public void SetAttackTarget(BaseCharacterEntity target)
    {
        if (target == null || target.CurrentHp <= 0)
            return;
        // Already have target so don't set target
        BaseCharacterEntity oldTarget;
        if (TryGetTargetEntity(out oldTarget) && oldTarget.CurrentHp > 0)
            return;
        // Set target to attack
        SetTargetEntity(target);
    }

    public override void ReceiveDamage(BaseCharacterEntity attacker, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff? debuff, int hitEffectsId)
    {
        // Damage calculations apply at server only
        if (!IsServer || CurrentHp <= 0)
            return;
        base.ReceiveDamage(attacker, allDamageAmounts, debuff, hitEffectsId);
        // If no attacker, skip next logics
        if (attacker == null || !IsEnemy(attacker))
            return;
        // If character isn't dead
        // If character is not dead, try to attack
        if (CurrentHp > 0)
        {
            var gameInstance = GameInstance.Singleton;
            // If no target enemy and current target is character, try to attack
            BaseCharacterEntity targetEntity;
            if (!TryGetTargetEntity(out targetEntity))
            {
                SetAttackTarget(attacker);
                // If it's assist character call another character for assist
                if (MonsterDatabase.characteristic == MonsterCharacteristic.Assist)
                {
                    var foundObjects = new List<Collider>(Physics.OverlapSphere(CacheTransform.position, MonsterDatabase.visualRange, gameInstance.characterLayer.Mask));
                    foreach (var foundObject in foundObjects)
                    {
                        var monsterCharacterEntity = foundObject.GetComponent<MonsterCharacterEntity>();
                        if (monsterCharacterEntity != null && IsAlly(monsterCharacterEntity))
                            monsterCharacterEntity.SetAttackTarget(attacker);
                    }
                }
            }
            else if (attacker != targetEntity && Random.value >= 0.5f)
            {
                // Random 50% to change target when receive damage from anyone
                SetAttackTarget(attacker);
            }
        }
    }

    public override void GetAttackingData(
        out int actionId, 
        out float damageDuration, 
        out float totalDuration,
        out DamageInfo damageInfo, 
        out Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
    {
        var gameInstance = GameInstance.Singleton;

        // Initialize attack animation
        actionId = -1;
        damageDuration = 0f;
        totalDuration = 0f;

        // Random attack animation
        var animArray = MonsterDatabase.attackAnimations;
        var animLength = animArray.Length;
        if (animLength > 0)
        {
            var anim = animArray[Random.Range(0, animLength)];
            // Assign animation data
            actionId = anim.Id;
            damageDuration = anim.TriggerDuration / AttackSpeed;
            totalDuration = (anim.ClipLength + anim.extraDuration) / AttackSpeed;
        }

        // Assign damage data
        damageInfo = MonsterDatabase.damageInfo;

        // Assign damage amounts
        allDamageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
        var damageElement = MonsterDatabase.damageAmount.damageElement;
        var damageAmount = MonsterDatabase.damageAmount.amount.GetAmount(Level);
        if (damageElement == null)
            damageElement = gameInstance.DefaultDamageElement;
        allDamageAmounts.Add(damageElement, damageAmount);
    }

    public override float GetAttackDistance()
    {
        return MonsterDatabase.damageInfo.GetDistance();
    }

    protected override void ReceivedDamage(BaseCharacterEntity attacker, CombatAmountType damageAmountType, int damage)
    {
        base.ReceivedDamage(attacker, damageAmountType, damage);
        // Add received damage entry
        if (attacker == null)
            return;
        var receivedDamageRecord = new ReceivedDamageRecord();
        receivedDamageRecord.totalReceivedDamage = damage;
        if (receivedDamageRecords.ContainsKey(attacker))
        {
            receivedDamageRecord = receivedDamageRecords[attacker];
            receivedDamageRecord.totalReceivedDamage += damage;
        }
        receivedDamageRecord.lastReceivedDamageTime = Time.realtimeSinceStartup;
        receivedDamageRecords[attacker] = receivedDamageRecord;
    }

    protected override void Killed(BaseCharacterEntity lastAttacker)
    {
        base.Killed(lastAttacker);
        deadTime = Time.realtimeSinceStartup;
        var maxHp = this.GetStats().hp;
        var randomedExp = Random.Range(MonsterDatabase.randomExpMin, MonsterDatabase.randomExpMax);
        var randomedGold = Random.Range(MonsterDatabase.randomGoldMin, MonsterDatabase.randomGoldMax);
        if (receivedDamageRecords.Count > 0)
        {
            var enemies = new List<BaseCharacterEntity>(receivedDamageRecords.Keys);
            foreach (var enemy in enemies)
            {
                var receivedDamageRecord = receivedDamageRecords[enemy];
                var rewardRate = receivedDamageRecord.totalReceivedDamage / maxHp;
                if (rewardRate > 1)
                    rewardRate = 1;
                enemy.IncreaseExp((int)(randomedExp * rewardRate));
                if (enemy is PlayerCharacterEntity)
                {
                    var enemyPlayer = enemy as PlayerCharacterEntity;
                    enemyPlayer.IncreaseGold((int)(randomedGold * rewardRate));
                }
            }
        }
        receivedDamageRecords.Clear();
        foreach (var randomItem in MonsterDatabase.randomItems)
        {
            if (Random.value <= randomItem.dropRate)
            {
                var item = randomItem.item;
                var amount = randomItem.amount;
                if (item != null && GameInstance.Items.ContainsKey(item.Id))
                {
                    var itemId = item.Id;
                    if (amount > item.maxStack)
                        amount = item.maxStack;
                    ItemDropEntity.DropItem(this, itemId, 1, amount);
                }
            }
        }
        var lastPlayer = lastAttacker as PlayerCharacterEntity;
        if (lastPlayer != null)
            lastPlayer.OnKillMonster(this);
    }

    protected override void Respawn()
    {
        if (!IsServer || CurrentHp > 0)
            return;
        base.Respawn();
        CacheTransform.position = respawnPosition;
        StopMove();
        RandomWanderTime();
        isHidding.Value = false;
    }
}

public struct ReceivedDamageRecord
{
    public float lastReceivedDamageTime;
    public int totalReceivedDamage;
}
