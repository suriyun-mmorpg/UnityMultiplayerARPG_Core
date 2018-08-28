using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public override bool IsWandering()
        {
            return CacheMonsterActivityComponent.isWandering;
        }

        public override void StopMove()
        {
            CacheMonsterActivityComponent.StopMove();
        }
    }
}
