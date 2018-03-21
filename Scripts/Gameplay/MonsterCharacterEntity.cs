using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(NavMeshAgent))]
public class MonsterCharacterEntity : CharacterEntity
{
    #region Cache components
    private CapsuleCollider cacheCapsuleCollider;
    public CapsuleCollider CacheCapsuleCollider
    {
        get
        {
            if (cacheCapsuleCollider == null)
                cacheCapsuleCollider = GetComponent<CapsuleCollider>();
            return cacheCapsuleCollider;
        }
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
    #endregion
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
