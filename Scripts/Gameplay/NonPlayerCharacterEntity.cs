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
    #region Protected data
    protected RpgNetworkEntity targetEntity;
    protected CharacterTargetType targetType;
    protected NpcPrototype prototype;
    protected float wanderTime;
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
    }

    protected override void Update()
    {
        base.Update();

        var gameInstance = GameInstance.Singleton;
        if (targetEntity != null)
        {
            // Lookat target then do anything when it's in range
        }
        else
        {
            // Wandering when it's time
            if (Time.realtimeSinceStartup >= wanderTime)
            {
                Vector3 randomPosition = CacheTransform.position + (Random.insideUnitSphere * prototype.randomWanderRadius);
                NavMeshHit navMeshHit;
                NavMesh.SamplePosition(randomPosition, out navMeshHit, prototype.randomWanderRadius, 1);
                CacheNavMeshAgent.SetDestination(navMeshHit.position);
                RandomWanderTime();
            }
            else
            {
                if (prototype.characteristic == NpcCharacteristic.Aggressive)
                {
                    // Find nearby character by layer mask
                    var foundObjects = new List<Collider>(Physics.OverlapSphere(CacheTransform.position, gameInstance.playerLayer.Mask | gameInstance.npcLayer.Mask));
                    foundObjects = foundObjects.OrderBy(a => System.Guid.NewGuid()).ToList();
                    foreach (var foundObject in foundObjects)
                    {
                        var characterEntity = foundObject.GetComponent<CharacterEntity>();
                        if (characterEntity != null && characterEntity.IsEnemy(this))
                        {
                            targetEntity = characterEntity;
                            CacheNavMeshAgent.SetDestination(targetEntity.CacheTransform.position);
                        }
                    }
                }
            }
        }
    }

    protected void RandomWanderTime()
    {
        wanderTime = Time.realtimeSinceStartup + Random.Range(prototype.randomWanderDurationMin, prototype.randomWanderDurationMax);
    }

    public override Vector3 GetMovementVelocity()
    {
        return CacheNavMeshAgent.velocity;
    }

    public override CharacterAction GetCharacterAction(CharacterEntity characterEntity)
    {
        return CharacterAction.Attack;
    }

    public override bool IsAlly(CharacterEntity characterEntity)
    {
        return false;
    }

    public override bool IsEnemy(CharacterEntity characterEntity)
    {
        return true;
    }

    public override void ReceiveDamage(CharacterEntity attacker, Dictionary<DamageElement, DamageAmount> allDamageAttributes, CharacterBuff debuff)
    {
        base.ReceiveDamage(attacker, allDamageAttributes, debuff);
        if (CurrentHp > 0)
        {
            // Random 50% to change target when receive damage from anyone
            if (targetEntity == null || Random.value <= 0.5f)
                targetEntity = attacker;
        }
    }
}
