using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;

namespace MultiplayerARPG
{
    public class MonsterCharacterEntity2D : BaseMonsterCharacterEntity
    {
        #region Sync data
        [SerializeField]
        protected SyncFieldByte currentDirectionType = new SyncFieldByte();
        #endregion

        #region Temp data
        protected Vector2 currentDirection = Vector2.down;
        protected Vector2 tempDirection;
        protected DirectionType2D localDirectionType = DirectionType2D.Down;
        #endregion

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

        public DirectionType2D CurrentDirectionType
        {
            get
            {
                if (IsOwnerClient)
                    return localDirectionType;
                return (DirectionType2D)currentDirectionType.Value;
            }
        }

        protected override void EntityUpdate()
        {
            base.EntityUpdate();
            if (CharacterModel is ICharacterModel2D)
            {
                // Set current direction to character model 2D
                (CharacterModel as ICharacterModel2D).CurrentDirectionType = CurrentDirectionType;
            }
        }

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            currentDirectionType.deliveryMethod = DeliveryMethod.Sequenced;
            currentDirectionType.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
        }

        public override bool IsPositionInFov(float fov, Vector3 position, Vector3 forward)
        {
            float halfFov = fov * 0.5f;
            Vector2 targetDir = (position - CacheTransform.position).normalized;
            float angle = Vector2.Angle(targetDir, currentDirection);
            // Angle in forward position is 180 so we use this value to determine that target is in hit fov or not
            return angle < halfFov;
        }

        protected override void GetDamagePositionAndRotation(DamageType damageType, bool isLeftHand, bool hasAimPosition, Vector3 aimPosition, Vector3 stagger, out Vector3 position, out Quaternion rotation)
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

        public override void StopMove()
        {
            CacheMonsterActivityComponent.StopMove();
        }

        public void UpdateCurrentDirection(Vector2 direction)
        {
            currentDirection = direction;
            localDirectionType = GameplayUtils.GetDirectionTypeByVector2(direction);
            if (IsServer)
                currentDirectionType.Value = (byte)localDirectionType;
        }
    }
}
