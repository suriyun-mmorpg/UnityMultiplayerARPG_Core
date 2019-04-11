using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterModel))]
    [RequireComponent(typeof(MonsterActivityComponent))]
    [RequireComponent(typeof(CapsuleCollider))]
    public partial class MonsterCharacterEntity : BaseMonsterCharacterEntity
    {
        public override bool IsGrounded
        {
            get { return true; }
            protected set { }
        }

        public override bool IsJumping
        {
            get { return false; }
            protected set { }
        }

        public override Vector3 CenterPosition
        {
            get { return CacheTransform.position + Vector3.up * (CacheCapsuleCollider.center.y + CacheTransform.localScale.y); }
        }

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
            this.spawnArea = spawnArea;
            CacheMonsterActivityComponent.SetSpawnArea(spawnPosition, out spawnPosition);
            this.spawnPosition = spawnPosition;
        }

        public override void StopMove()
        {
            CacheMonsterActivityComponent.StopMove();
        }
    }
}
