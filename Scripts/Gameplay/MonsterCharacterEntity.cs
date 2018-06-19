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
    #region Activity System Data
    [HideInInspector, System.NonSerialized]
    public float wanderTime;
    [HideInInspector, System.NonSerialized]
    public float findTargetTime;
    [HideInInspector, System.NonSerialized]
    public float setDestinationTime;
    [HideInInspector, System.NonSerialized]
    public float startFollowTargetTime;
    [HideInInspector, System.NonSerialized]
    public float receivedDamageRecordsUpdateTime;
    [HideInInspector, System.NonSerialized]
    public float deadTime;
    [HideInInspector, System.NonSerialized]
    public Vector3? wanderDestination;
    [HideInInspector, System.NonSerialized]
    public Vector3 oldDestination;
    [HideInInspector, System.NonSerialized]
    public bool isWandering;
    #endregion
    public readonly Dictionary<BaseCharacterEntity, ReceivedDamageRecord> receivedDamageRecords = new Dictionary<BaseCharacterEntity, ReceivedDamageRecord>();

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
        var time = Time.unscaledTime;
        MonsterActivityComponent.RandomNextWanderTime(time, this, CacheTransform);
        MonsterActivityComponent.SetFindTargetTime(time, this);
        MonsterActivityComponent.SetStartFollowTargetTime(time, this);
    }

    public virtual void StopMove()
    {
        CacheNavMeshAgent.isStopped = true;
        CacheNavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        wanderDestination = null;
    }

    public override void OnSetup()
    {
        base.OnSetup();

        CacheNetTransform.ownerClientCanSendTransform = false;
    }

    public override bool CanReceiveDamageFrom(BaseCharacterEntity characterEntity)
    {
        return characterEntity != null && characterEntity is PlayerCharacterEntity;
    }

    public override bool IsAlly(BaseCharacterEntity characterEntity)
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

    public override bool IsEnemy(BaseCharacterEntity characterEntity)
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

    public override void ReceiveDamage(BaseCharacterEntity attacker, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, int hitEffectsId)
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
        out Item weapon,
        out int actionId, 
        out float triggerDuration, 
        out float totalDuration,
        out DamageInfo damageInfo, 
        out Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
    {
        var gameInstance = GameInstance.Singleton;

        // Initialize data
        weapon = null;
        actionId = -1;
        triggerDuration = 0f;
        totalDuration = 0f;

        // Random attack animation
        var animArray = MonsterDatabase.attackAnimations;
        var animLength = animArray.Length;
        if (animLength > 0)
        {
            var anim = animArray[Random.Range(0, animLength)];
            // Assign animation data
            actionId = anim.Id;
            GetActionAnimationDurations(anim, out triggerDuration, out totalDuration);
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

    public override void ReceivedDamage(BaseCharacterEntity attacker, CombatAmountType damageAmountType, int damage)
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
        receivedDamageRecord.lastReceivedDamageTime = Time.unscaledTime;
        receivedDamageRecords[attacker] = receivedDamageRecord;
    }

    public override void Killed(BaseCharacterEntity lastAttacker)
    {
        base.Killed(lastAttacker);
        deadTime = Time.unscaledTime;
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
                if (item != null && GameInstance.Items.ContainsKey(item.HashId))
                {
                    var itemDataId = item.HashId;
                    if (amount > item.maxStack)
                        amount = item.maxStack;
                    ItemDropEntity.DropItem(this, itemDataId, 1, amount);
                }
            }
        }
        var lastPlayer = lastAttacker as PlayerCharacterEntity;
        if (lastPlayer != null)
            lastPlayer.OnKillMonster(this);
    }

    public override void Respawn()
    {
        if (!IsServer || CurrentHp > 0)
            return;
        base.Respawn();
        StopMove();
        CacheNetTransform.Teleport(respawnPosition, CacheTransform.rotation);
        MonsterActivityComponent.RandomNextWanderTime(Time.unscaledTime, this, CacheTransform);
        isHidding.Value = false;
    }
}

public struct ReceivedDamageRecord
{
    public float lastReceivedDamageTime;
    public int totalReceivedDamage;
}
