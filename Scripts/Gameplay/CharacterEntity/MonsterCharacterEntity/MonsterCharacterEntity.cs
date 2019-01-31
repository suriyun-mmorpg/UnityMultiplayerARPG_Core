using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterModel))]
    [RequireComponent(typeof(MonsterActivityComponent))]
    [RequireComponent(typeof(CapsuleCollider))]
    public partial class MonsterCharacterEntity : BaseMonsterCharacterEntity
    {
        private MonsterActivityComponent cacheMonsterActivityComponent;
        public MonsterActivityComponent CacheMonsterActivityComponent
        {
            get
            {
                if (cacheMonsterActivityComponent == null)
                    cacheMonsterActivityComponent = GetComponent<MonsterActivityComponent>();
                return cacheMonsterActivityComponent;
            }
        }

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

        public override void SetSpawnArea(MonsterSpawnArea spawnArea, Vector3 spawnPosition)
        {
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(spawnPosition, out navHit, spawnArea.randomRadius, -1))
            {
                this.spawnArea = spawnArea;
                this.spawnPosition = navHit.position;
                GetComponent<NavMeshAgent>().Warp(spawnPosition);
            }
        }

        public override void StopMove()
        {
            CacheMonsterActivityComponent.StopMove();
        }
    }
}
