using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterModel2D))]
    [RequireComponent(typeof(MonsterActivityComponent2D))]
    public class MonsterCharacterEntity2D : BaseMonsterCharacterEntity
    {
        #region Sync data
        [SerializeField]
        protected SyncFieldByte currentDirectionType = new SyncFieldByte();
        #endregion

        #region Temp data
        protected Collider2D[] overlapColliders2D = new Collider2D[OVERLAP_COLLIDER_SIZE];
        protected Vector2 currentDirection = Vector2.down;
        protected Vector2 tempDirection;
        protected DirectionType localDirectionType = DirectionType.Down;
        #endregion
        
        private MonsterActivityComponent2D cacheMonsterActivityComponent;
        public MonsterActivityComponent2D CacheMonsterActivityComponent
        {
            get
            {
                if (cacheMonsterActivityComponent == null)
                    cacheMonsterActivityComponent = GetComponent<MonsterActivityComponent2D>();
                return cacheMonsterActivityComponent;
            }
        }

        public DirectionType CurrentDirectionType
        {
            get
            {
                if (IsOwnerClient)
                    return localDirectionType;
                return (DirectionType)currentDirectionType.Value;
            }
        }

        protected override void EntityUpdate()
        {
            base.EntityUpdate();
            (CharacterModel as CharacterModel2D).currentDirectionType = CurrentDirectionType;
        }

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            currentDirectionType.sendOptions = SendOptions.Sequenced;
            currentDirectionType.forOwnerOnly = false;
        }

        public override int OverlapObjects(Vector3 position, float distance, int layerMask)
        {
            return Physics2D.OverlapCircleNonAlloc(position, distance, overlapColliders2D, layerMask);
        }

        public override GameObject GetOverlapObject(int index)
        {
            return overlapColliders2D[index].gameObject;
        }

        public override bool IsPositionInFov(float fov, Vector3 position)
        {
            float halfFov = fov * 0.5f;
            float angle = Vector2.Angle((CacheTransform.position - position).normalized, currentDirection);
            // Angle in forward position is 180 so we use this value to determine that target is in hit fov or not
            return (angle < 180 + halfFov && angle > 180 - halfFov);
        }

        protected override void GetDamagePositionAndRotation(DamageType damageType, out Vector3 position, out Quaternion rotation)
        {
            position = CacheTransform.position;
            if (CharacterModel != null)
            {
                switch (damageType)
                {
                    case DamageType.Melee:
                        position = MeleeDamageTransform.position;
                        break;
                    case DamageType.Missile:
                        position = MissileDamageTransform.position;
                        break;
                }
            }
            rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(currentDirection.y, currentDirection.x) * (180 / Mathf.PI)) + 90);
        }

        public void UpdateCurrentDirection(Vector2 direction)
        {
            currentDirection = direction;
            UpdateDirection(currentDirection);
        }

        public void UpdateDirection(Vector2 direction)
        {
            if (direction.magnitude > 0f)
            {
                Vector2 normalized = direction.normalized;
                if (Mathf.Abs(normalized.x) >= Mathf.Abs(normalized.y))
                {
                    if (normalized.x < 0) localDirectionType = DirectionType.Left;
                    if (normalized.x > 0) localDirectionType = DirectionType.Right;
                }
                else
                {
                    if (normalized.y < 0) localDirectionType = DirectionType.Down;
                    if (normalized.y > 0) localDirectionType = DirectionType.Up;
                }
            }
            if (IsServer)
                currentDirectionType.Value = (byte)localDirectionType;
        }

        public override void StopMove()
        {
            CacheMonsterActivityComponent.StopMove();
        }

        public override Vector3 GetSummonPosition()
        {
            return CacheTransform.position + new Vector3(Random.Range(GameInstance.minSummonDistance, GameInstance.maxSummonDistance) * GenericUtils.GetNegativePositive(), Random.Range(GameInstance.minSummonDistance, GameInstance.maxSummonDistance) * GenericUtils.GetNegativePositive());
        }

        public override Quaternion GetSummonRotation()
        {
            return Quaternion.identity;
        }
    }
}
