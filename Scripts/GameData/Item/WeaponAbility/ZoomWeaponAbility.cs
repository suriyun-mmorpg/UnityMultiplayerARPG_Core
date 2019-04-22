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

        public override void Setup(ShooterPlayerCharacterController controller)
        {
            base.Setup(controller);
            if (controller.zoomCrosshairImage != null)
            {
                controller.zoomCrosshairImage.preserveAspect = true;
                controller.zoomCrosshairImage.raycastTarget = false;
            }
        }

        public override void ForceDeactivated()
        {
            if (controllerCamera != null)
                controllerCamera.fieldOfView = playerCharacterController.DefaultGameplayCameraFOV;
            if (playerCharacterController.crosshairRect != null)
                playerCharacterController.crosshairRect.gameObject.SetActive(true);
            if (playerCharacterController.zoomCrosshairImage != null)
                playerCharacterController.zoomCrosshairImage.gameObject.SetActive(false);
        }

        public override WeaponAbilityState UpdateActivation(WeaponAbilityState state, float deltaTime)
        {
            if (state == WeaponAbilityState.Deactivating)
            {
                zoomInterpTime += deltaTime * ZOOM_SPEED;
                controllerCamera.fieldOfView = Mathf.Lerp(controllerCamera.fieldOfView, playerCharacterController.DefaultGameplayCameraFOV, zoomInterpTime);
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
            if (playerCharacterController.crosshairRect != null &&
                playerCharacterController.crosshairRect.gameObject.activeSelf != tempActiveState)
            {
                playerCharacterController.crosshairRect.gameObject.SetActive(tempActiveState);
            }

            tempActiveState = state == WeaponAbilityState.Activated || state == WeaponAbilityState.Activating;
            if (playerCharacterController.zoomCrosshairImage != null &&
                playerCharacterController.zoomCrosshairImage.gameObject.activeSelf != tempActiveState)
            {
                playerCharacterController.zoomCrosshairImage.gameObject.SetActive(tempActiveState);
                playerCharacterController.zoomCrosshairImage.sprite = zoomCrosshair;
            }

            return state;
        }
    }
}
