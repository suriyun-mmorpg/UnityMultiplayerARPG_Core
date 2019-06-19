using LiteNetLib;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseCharacterMovement2D : BaseCharacterMovement
    {
        private SyncFieldVector2 currentDirection = new SyncFieldVector2();
        private SyncFieldByte currentDirectionType = new SyncFieldByte();

        public virtual Vector2 CurrentDirection
        {
            get { return currentDirection.Value; }
            set { currentDirection.Value = value; }
        }

        public virtual DirectionType2D CurrentDirectionType
        {
            get { return (DirectionType2D)currentDirectionType.Value; }
            set { currentDirectionType.Value = (byte)value; }
        }

        protected virtual void Update()
        {
            if (CacheCharacterEntity.CharacterModel is ICharacterModel2D)
            {
                // Set current direction to character model 2D
                (CacheCharacterEntity.CharacterModel as ICharacterModel2D).CurrentDirectionType = CurrentDirectionType;
            }
        }

        public override void EntityOnSetup(BaseCharacterEntity entity)
        {
            base.EntityOnSetup(entity);
            currentDirection.deliveryMethod = DeliveryMethod.Sequenced;
            currentDirection.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            currentDirectionType.deliveryMethod = DeliveryMethod.Sequenced;
            currentDirectionType.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            entity.RegisterSyncField(currentDirection);
            entity.RegisterSyncField(currentDirectionType);
        }

        public sealed override bool IsPositionInFov(float fov, Vector3 position, Vector3 forward)
        {
            float halfFov = fov * 0.5f;
            Vector2 targetDir = (position - CacheTransform.position).normalized;
            float angle = Vector2.Angle(targetDir, CurrentDirection);
            // Angle in forward position is 180 so we use this value to determine that target is in hit fov or not
            return angle < halfFov;
        }

        public sealed override void GetDamagePositionAndRotation(DamageType damageType, bool isLeftHand, bool hasAimPosition, Vector3 aimPosition, Vector3 stagger, out Vector3 position, out Quaternion rotation)
        {
            position = CacheTransform.position;
            switch (damageType)
            {
                case DamageType.Melee:
                    position = CacheCharacterEntity.MeleeDamageTransform.position;
                    break;
                case DamageType.Missile:
                    position = CacheCharacterEntity.MissileDamageTransform.position;
                    break;
            }
            rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(CurrentDirection.y, CurrentDirection.x) * (180 / Mathf.PI)) + 90);
        }
    }
}
