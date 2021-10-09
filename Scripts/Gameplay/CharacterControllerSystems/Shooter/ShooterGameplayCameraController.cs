using System.Collections;
using UnityEngine;

namespace MultiplayerARPG
{
    public class ShooterGameplayCameraController : DefaultGameplayCameraController, IShooterGameplayCameraController, IAimAssistAvoidanceListener
    {
        public bool EnableAimAssist { get { return CameraControls.enableAimAssist; } set { CameraControls.enableAimAssist = value; } }
        public bool EnableAimAssistX { get { return CameraControls.enableAimAssistX; } set { CameraControls.enableAimAssistX = value; } }
        public bool EnableAimAssistY { get { return CameraControls.enableAimAssistY; } set { CameraControls.enableAimAssistY = value; } }
        public bool AimAssistCharacter { get { return aimAssistCharacter; } set { aimAssistCharacter = value; } }
        public bool AimAssistBuilding { get { return aimAssistBuilding; } set { aimAssistBuilding = value; } }
        public bool AimAssistHarvestable { get { return aimAssistHarvestable; } set { aimAssistHarvestable = value; } }
        public float AimAssistRadius { get { return CameraControls.aimAssistRadius; } set { CameraControls.aimAssistRadius = value; } }
        public float AimAssistXSpeed { get { return CameraControls.aimAssistXSpeed; } set { CameraControls.aimAssistXSpeed = value; } }
        public float AimAssistYSpeed { get { return CameraControls.aimAssistYSpeed; } set { CameraControls.aimAssistYSpeed = value; } }
        public float AimAssistMaxAngleFromFollowingTarget { get { return CameraControls.aimAssistMaxAngleFromFollowingTarget; } set { CameraControls.aimAssistMaxAngleFromFollowingTarget = value; } }
        public float RotationSpeedScale { get { return CameraControls.rotationSpeedScale; } set { CameraControls.rotationSpeedScale = value; } }

        public override void InitialCameraControls()
        {
            base.InitialCameraControls();
            CameraControls.AimAssistAvoidanceListener = this;
        }

        public void Recoil(float x, float y)
        {
            CameraControls.Recoil(x, y);
        }

        public virtual bool AvoidAimAssist(RaycastHit hitInfo)
        {
            IGameEntity entity = hitInfo.collider.GetComponent<IGameEntity>();
            if (entity != null && entity.Entity != null && entity.Entity != PlayerCharacterEntity)
            {
                DamageableEntity damageableEntity = entity.Entity as DamageableEntity;
                return damageableEntity == null || damageableEntity.IsDead() || !damageableEntity.CanReceiveDamageFrom(PlayerCharacterEntity.GetInfo());
            }
            return true;
        }

        protected virtual int GetAimAssistLayerMask()
        {
            int layerMask = 0;
            if (AimAssistCharacter)
                layerMask = layerMask | GameInstance.Singleton.characterLayer.Mask;
            if (AimAssistBuilding)
                layerMask = layerMask | GameInstance.Singleton.buildingLayer.Mask;
            if (AimAssistHarvestable)
                layerMask = layerMask | GameInstance.Singleton.harvestableLayer.Mask;
            return layerMask;
        }

        public override void Setup(BasePlayerCharacterEntity characterEntity)
        {
            base.Setup(characterEntity);
            if (characterEntity == null || CameraControls == null)
                return;
            CameraControls.startYRotation = characterEntity.CurrentRotation.y;
        }

        public override void Desetup(BasePlayerCharacterEntity characterEntity)
        {
            base.Desetup(characterEntity);
        }
    }
}
