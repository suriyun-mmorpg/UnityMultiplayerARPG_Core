using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Zoom Weapon Ability", menuName = "Create GameData/Weapon Ability/Zoom Weapon Ability")]
    public class ZoomWeaponAbility : BaseWeaponAbility
    {
        const float ZOOM_SPEED = 1.25f;

        public float zoomingFov = 20f;
        public bool disableRenderersOnZoom;
        public Sprite zoomCrosshair;

        private float zoomInterpTime;
        private bool tempActiveState;
        private Camera controllerCamera;
        private ShooterPlayerCharacterController shooterController;

        public override void Setup(BasePlayerCharacterController controller)
        {
            base.Setup(controller);
            shooterController = controller as ShooterPlayerCharacterController;
            controllerCamera = controller.CacheGameplayCameraControls.CacheCamera;
            shooterController.zoomCrosshairImage.preserveAspect = true;
            shooterController.zoomCrosshairImage.raycastTarget = false;
        }

        public override void ForceDeactivated()
        {
            if (controllerCamera != null)
                controllerCamera.fieldOfView = shooterController.DefaultGameplayCameraFOV;
            if (shooterController.crosshairRect != null)
                shooterController.crosshairRect.gameObject.SetActive(true);
            if (shooterController.zoomCrosshairImage != null)
                shooterController.zoomCrosshairImage.gameObject.SetActive(false);
        }

        public override WeaponAbilityState UpdateActivation(WeaponAbilityState state, float deltaTime)
        {
            if (state == WeaponAbilityState.Deactivating)
            {
                zoomInterpTime += deltaTime * ZOOM_SPEED;
                controllerCamera.fieldOfView = Mathf.Lerp(controllerCamera.fieldOfView, shooterController.DefaultGameplayCameraFOV, zoomInterpTime);
                if (zoomInterpTime >= 1f)
                {
                    zoomInterpTime = 0;
                    state = WeaponAbilityState.Deactivated;
                }
            }
            else if (state == WeaponAbilityState.Activating)
            {
                zoomInterpTime += deltaTime * ZOOM_SPEED;
                controllerCamera.fieldOfView = Mathf.Lerp(controllerCamera.fieldOfView, zoomingFov, zoomInterpTime);
                if (zoomInterpTime >= 1f)
                {
                    zoomInterpTime = 0;
                    state = WeaponAbilityState.Activated;
                }
            }

            tempActiveState = state == WeaponAbilityState.Deactivated || state == WeaponAbilityState.Deactivating;
            if (shooterController.crosshairRect != null &&
                shooterController.crosshairRect.gameObject.activeSelf != tempActiveState)
            {
                shooterController.crosshairRect.gameObject.SetActive(tempActiveState);
            }

            tempActiveState = state == WeaponAbilityState.Activated || state == WeaponAbilityState.Activating;
            if (shooterController.zoomCrosshairImage != null &&
                shooterController.zoomCrosshairImage.gameObject.activeSelf != tempActiveState)
            {
                shooterController.zoomCrosshairImage.gameObject.SetActive(tempActiveState);
                shooterController.zoomCrosshairImage.sprite = zoomCrosshair;
            }

            return state;
        }
    }
}
