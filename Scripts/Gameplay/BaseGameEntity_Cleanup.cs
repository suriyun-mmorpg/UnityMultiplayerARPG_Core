using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseGameEntity
    {
        public virtual void Clean(bool isObjectDestroyed)
        {
            if (isObjectDestroyed)
            {
                ownerObjects?.DestroyAndNullify();
                ownerObjects = null;
                nonOwnerObjects?.DestroyAndNullify();
                nonOwnerObjects = null;
                model = null;
                cameraTargetTransform = null;
                fpsCameraTargetTransform = null;
                // Events
                onStart = null;
                onEnable = null;
                onDisable = null;
                onUpdate = null;
                onLateUpdate = null;
                onSetup = null;
                onSetupNetElements = null;
                onSetOwnerClient = null;
                onIsUpdateEntityComponentsChanged = null;
                onNetworkDestroy = null;
                onCanMoveValidated = null;
                onCanSprintValidated = null;
                onCanWalkValidated = null;
                onCanCrouchValidated = null;
                onCanCrawlValidated = null;
                onCanJumpValidated = null;
                onCanTurnValidated = null;
                onJumpForceApplied = null;
                // Move
                Movement = null;
            }
            ForceHide = false;
            // Mount
            LastMountActionTime = 0f;
            PassengingVehicleSeatIndex = 0;
            PassengingVehicleEntity = null;
            // Other
            OverrideCameraTargetTransform.Clear();
            MovementDisableState.Clear();
            _isTeleporting = false;
            _stillMoveAfterTeleport = false;
            _teleportingPosition = Vector3.zero;
            _teleportingRotation = Quaternion.identity;
            _wasUpdateEntityComponents = null;
        }
    }
}
