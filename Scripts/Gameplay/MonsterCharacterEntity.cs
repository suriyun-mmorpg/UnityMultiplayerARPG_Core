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
    public const float RANDOM_WANDER_DURATION_MIN = 1f;
    public const float RANDOM_WANDER_DURATION_MAX = 5f;
    public const float RANDOM_WANDER_RADIUS = 5f;
    public const float AGGRESSIVE_FIND_TARGET_DELAY = 1f;

    #region Protected data
    protected MonsterCharacterDatabase prototype;
    protected float wanderTime;
    protected float findTargetTime;
    protected Vector3 oldFollowTargetPosition;
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
        UpdateActivity();
    }

    protected virtual void UpdateActivity()
    {
        if (!IsServer)
            return;
        var gameInstance = GameInstance.Singleton;
        CharacterEntity targetEntity;
        if (TryGetTargetEntity(out targetEntity))
        {
            // If it has target then go to target
            var currentPosition = CacheTransform.position;
            var targetPosition = targetEntity.CacheTransform.position;
            var attackDistance = EquipWeapons.GetAttackDistance() + targetEntity.CacheCapsuleCollider.radius;
            if (Vector3.Distance(currentPosition, targetPosition) <= attackDistance)
            {
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
                if (oldFollowTargetPosition != targetPosition)
                {
                    CacheNavMeshAgent.SetDestination(targetPosition);
                    CacheNavMeshAgent.isStopped = false;
                    oldFollowTargetPosition = targetPosition;
                }
            }
        }
        else
        {
            // Update rotation while wandering
            CacheNavMeshAgent.updateRotation = true;
            // While character is moving then random next wander time
            // To let character stop movement some time before random next wander time
            if (!CacheNavMeshAgent.isStopped)
                RandomWanderTime();
            // Wandering when it's time
            if (Time.realtimeSinceStartup >= wanderTime)
            {
                // If stopped then random
                Vector3 randomPosition = CacheTransform.position + (Random.insideUnitSphere * RANDOM_WANDER_RADIUS);
                NavMeshHit navMeshHit;
                NavMesh.SamplePosition(randomPosition, out navMeshHit, RANDOM_WANDER_RADIUS, 1);
                CacheNavMeshAgent.SetDestination(navMeshHit.position);
                CacheNavMeshAgent.isStopped = false;
            }
            else
            {
                // If it's aggressive character, finding attacking target
                if (prototype.characteristic == MonsterCharacteristic.Aggressive &&
                    Time.realtimeSinceStartup >= findTargetTime)
                {
                    SetFindTargetTime();
                    CharacterEntity targetCharacter;
                    // If no target enenmy or target enemy is dead
                    if (!TryGetTargetEntity(out targetCharacter) || targetCharacter.CurrentHp <= 0)
                    {
                        // Find nearby character by layer mask
                        var foundObjects = new List<Collider>(Physics.OverlapSphere(CacheTransform.position, prototype.visualRange, gameInstance.characterLayer.Mask));
                        foundObjects = foundObjects.OrderBy(a => System.Guid.NewGuid()).ToList();
                        foreach (var foundObject in foundObjects)
                        {
                            var characterEntity = foundObject.GetComponent<CharacterEntity>();
                            if (characterEntity != null && IsEnemy(characterEntity))
                                SetAttackTarget(characterEntity);
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

    protected override Vector3 GetMovementVelocity()
    {
        return CacheNavMeshAgent.velocity;
    }

    protected override bool IsAlly(CharacterEntity characterEntity)
    {
        if (characterEntity == null)
            return false;
        // If this character have been attacked by any character
        // It will tell another ally nearby to help
        var monsterCharacterEntity = characterEntity as MonsterCharacterEntity;
        if (monsterCharacterEntity != null && monsterCharacterEntity.prototype.allyId == prototype.allyId)
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
                if (prototype.characteristic == MonsterCharacteristic.Assist)
                {
                    var foundObjects = new List<Collider>(Physics.OverlapSphere(CacheTransform.position, prototype.visualRange, gameInstance.characterLayer.Mask));
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
}
