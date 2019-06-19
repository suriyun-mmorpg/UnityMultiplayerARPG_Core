using LiteNetLib;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseCharacterMovement : BaseCharacterComponent, ICharacterMovement
    {
        private SyncFieldByte movementState = new SyncFieldByte();

        public virtual MovementState MovementState
        {
            get { return (MovementState)movementState.Value; }
            set { movementState.Value = (byte)value; }
        }

        public virtual bool IsGrounded { get; protected set; }
        public virtual bool IsJumping { get; protected set; }
        public abstract float StoppingDistance { get; }
        public abstract void KeyMovement(Vector3 moveDirection, MovementState movementState);
        public abstract void PointClickMovement(Vector3 position);
        public abstract void StopMove();
        public abstract void SetLookRotation(Vector3 eulerAngles);
        public abstract void Teleport(Vector3 position);
        public abstract void FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result);

        public override void EntityOnSetup(BaseCharacterEntity entity)
        {
            base.EntityOnSetup(entity);
            movementState.deliveryMethod = DeliveryMethod.Sequenced;
            movementState.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            entity.RegisterSyncField(movementState);
        }

        public virtual bool IsPositionInFov(float fov, Vector3 position, Vector3 forward)
        {
            float halfFov = fov * 0.5f;
            // This is unsigned angle, so angle found from this function is 0 - 180
            // if position forward from character this value will be 180
            // so just find for angle > 180 - halfFov
            Vector3 targetDir = (position - CacheTransform.position).normalized;
            targetDir.y = 0;
            forward.y = 0;
            targetDir.Normalize();
            forward.Normalize();
            return Vector3.Angle(targetDir, forward) < halfFov;
        }

        public virtual void GetDamagePositionAndRotation(DamageType damageType, bool isLeftHand, bool hasAimPosition, Vector3 aimPosition, Vector3 stagger, out Vector3 position, out Quaternion rotation)
        {
            position = CacheTransform.position;
            switch (damageType)
            {
                case DamageType.Melee:
                    position = CacheCharacterEntity.MeleeDamageTransform.position;
                    break;
                case DamageType.Missile:
                    Transform tempMissileDamageTransform = null;
                    if ((tempMissileDamageTransform = CacheCharacterEntity.CharacterModel.GetRightHandMissileDamageTransform()) != null && !isLeftHand)
                    {
                        // Use position from right hand weapon missile damage transform
                        position = tempMissileDamageTransform.position;
                    }
                    else if ((tempMissileDamageTransform = CacheCharacterEntity.CharacterModel.GetLeftHandMissileDamageTransform()) != null && isLeftHand)
                    {
                        // Use position from left hand weapon missile damage transform
                        position = tempMissileDamageTransform.position;
                    }
                    else
                    {
                        // Use position from default missile damage transform
                        position = CacheCharacterEntity.MissileDamageTransform.position;
                    }
                    break;
            }
            Quaternion forwardRotation = Quaternion.LookRotation(CacheTransform.forward);
            Vector3 forwardStagger = forwardRotation * stagger;
            rotation = Quaternion.LookRotation(CacheTransform.forward + forwardStagger);
            if (hasAimPosition)
            {
                forwardRotation = Quaternion.LookRotation(aimPosition - position);
                forwardStagger = forwardRotation * stagger;
                rotation = Quaternion.LookRotation(aimPosition + forwardStagger - position);
            }
        }
    }
}
