using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using LiteNetLibHighLevel;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(LiteNetLibTransform))]
public class MonsterCharacterEntity : CharacterEntity
{
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
    }

    protected override void SetupModel(CharacterModel characterModel)
    {
        CacheCapsuleCollider.center = characterModel.center;
        CacheCapsuleCollider.radius = characterModel.radius;
        CacheCapsuleCollider.height = characterModel.height;
    }

    protected override Vector3 GetMovementVelocity()
    {
        return CacheNavMeshAgent.velocity;
    }
}
