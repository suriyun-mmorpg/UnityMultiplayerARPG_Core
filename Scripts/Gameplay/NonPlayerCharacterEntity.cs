using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using LiteNetLibHighLevel;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(LiteNetLibTransform))]
public class NonPlayerCharacterEntity : CharacterEntity
{
    public const float RANDOM_WANDER_DURATION_MIN = 1f;
    public const float RANDOM_WANDER_DURATION_MAX = 5f;
    public const float RANDOM_WANDER_RADIUS = 5f;
    public const float AGGRESSIVE_FIND_TARGET_DELAY = 1f;

    #region Protected data
    protected NpcPrototype prototype;
    protected float wanderTime;
    protected float findTargetTime;
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

    protected virtual void Awake()
    {
        var gameInstance = GameInstance.Singleton;
        gameObject.tag = gameInstance.npcTag;
        gameObject.layer = gameInstance.npcLayer;
        RandomWanderTime();
        SetFindTargetTime();
    }

    protected override void Update()
    {
        base.Update();

        var gameInstance = GameInstance.Singleton;
        var targetEntity = GetTargetEntity();
        if (targetEntity != null)
        {
            // Lookat target then do anything when it's in range
            CacheNavMeshAgent.updateRotation = false;
            var currentPosition = CacheTransform.position;
            var targetPosition = targetEntity.CacheTransform.position;
            var moveDirection = (targetPosition - currentPosition).normalized;
            // slerp to the desired rotation over time
            CacheTransform.rotation = Quaternion.RotateTowards(CacheTransform.rotation, Quaternion.LookRotation(moveDirection), CacheNavMeshAgent.angularSpeed * Time.deltaTime);
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
            }
            else
            {
                // If it's aggressive character, finding attacking target
                if (prototype.characteristic == NpcCharacteristic.Aggressive &&
                    Time.realtimeSinceStartup >= findTargetTime)
                {
                    SetFindTargetTime();
                    var targetCharacter = GetTargetEntity<CharacterEntity>();
                    if (targetCharacter == null || targetCharacter.CurrentHp <= 0)
                    {
                        // Find nearby character by layer mask
                        var foundObjects = new List<Collider>(Physics.OverlapSphere(CacheTransform.position, prototype.visualRange, gameInstance.playerLayer.Mask | gameInstance.npcLayer.Mask));
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

    protected override CharacterAction GetCharacterAction(CharacterEntity characterEntity)
    {
        return CharacterAction.Attack;
    }

    protected override bool IsAlly(CharacterEntity characterEntity)
    {
        // If this character have been attacked by any character
        // It will tell another ally nearby to help
        if (characterEntity == null)
            return false;
        var npcEntity = characterEntity as NonPlayerCharacterEntity;
        if (npcEntity != null && npcEntity.prototype.allyId == prototype.allyId)
            return true;
        return false;
    }

    protected override bool IsEnemy(CharacterEntity characterEntity)
    {
        // TODO: Mercenary will be implemented later
        // So there are NonPlayerCharacterEntity that is enemy with this character
        if (characterEntity is PlayerCharacterEntity)
            return true;
        return false;
    }

    public void SetAttackTarget(CharacterEntity target)
    {
        if (target == null || target.CurrentHp <= 0)
            return;
        // Already have target so don't set target
        var oldTarget = GetTargetEntity<CharacterEntity>();
        if (oldTarget != null && oldTarget.CurrentHp > 0)
            return;
        // Set target to attack
        SetTargetEntity(target);
        // Set destination to target
        CacheNavMeshAgent.SetDestination(target.CacheTransform.position);
    }

    public override void ReceiveDamage(CharacterEntity attacker, Dictionary<DamageElement, DamageAmount> allDamageAttributes, CharacterBuff debuff)
    {
        base.ReceiveDamage(attacker, allDamageAttributes, debuff);
        var gameInstance = GameInstance.Singleton;
        var targetEntity = GetTargetEntity();
        if (CurrentHp > 0)
        {
            if (targetEntity == null)
            {
                SetAttackTarget(attacker);
                // If it's assist character call another character for assist
                if (prototype.characteristic == NpcCharacteristic.Assist)
                {
                    var foundObjects = new List<Collider>(Physics.OverlapSphere(CacheTransform.position, prototype.visualRange, gameInstance.npcLayer.Mask));
                    foreach (var foundObject in foundObjects)
                    {
                        var npcEntity = foundObject.GetComponent<NonPlayerCharacterEntity>();
                        if (npcEntity != null && IsAlly(npcEntity))
                            npcEntity.SetAttackTarget(attacker);
                    }
                }
            }
            else if (attacker != targetEntity && Random.value <= 0.5f)
            {
                // Random 50% to change target when receive damage from anyone
                SetAttackTarget(attacker);
            }
        }
    }
}
