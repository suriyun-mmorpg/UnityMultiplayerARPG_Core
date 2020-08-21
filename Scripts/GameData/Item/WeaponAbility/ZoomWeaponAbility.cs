using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Zoom Weapon Ability", menuName = "Create GameData/Weapon Ability/Zoom Weapon Ability", order = -4890)]
    public class ZoomWeaponAbility : BaseWeaponAbility
    {
        const float ZOOM_SPEED = 1.25f;

        public float zoomingFov = 20f;
        [Range(0.1f, 1f)]
        public float rotationSpeedScaleWhileZooming = 0.5f;
        public bool disableRenderersOnZoom;
        public Sprite zoomCrosshair;

        [System.NonSerialized]
        private float currentZoomInterpTime;
        [System.NonSerialized]
        private float currentZoomFov;
        [System.NonSerialized]
        private IZoomWeaponAbilityController zoomWeaponAbilityController;
        [System.NonSerialized]
        private ShooterControllerViewMode? preActivateViewMode;

        // TODO: Add rotate scale player's config

        public override void Setup(BasePlayerCharacterController controller, CharacterItem weapon)
        {
            base.Setup(controller, weapon);
            zoomWeaponAbilityController = controller as IZoomWeaponAbilityController;
            zoomWeaponAbilityController.InitialZoomCrosshair();
        }

        public override void Desetup()
        {
            ForceDeactivated();
        }

        public override void ForceDeactivated()
        {
            if (preActivateViewMode.HasValue)
                zoomWeaponAbilityController.ViewMode = preActivateViewMode.Value;
            zoomWeaponAbilityController.RotationSpeedScale = 1f;
            zoomWeaponAbilityController.ShowZoomCrosshair = false;
            zoomWeaponAbilityController.HideCrosshair = false;
        }

        public override void OnPreActivate()
        {
            preActivateViewMode = zoomWeaponAbilityController.ViewMode;
            zoomWeaponAbilityController.ViewMode = ShooterControllerViewMode.Fps;
            zoomWeaponAbilityController.SetZoomCrosshairSprite(zoomCrosshair);
            zoomWeaponAbilityController.RotationSpeedScale = rotationSpeedScaleWhileZooming;
            currentZoomInterpTime = 0f;
            currentZoomFov = zoomWeaponAbilityController.CurrentCameraFov;
        }

        public override WeaponAbilityState UpdateActivation(WeaponAbilityState state, float deltaTime)
        {
            if (state == WeaponAbilityState.Deactivating)
            {
                currentZoomInterpTime += deltaTime * ZOOM_SPEED;
                zoomWeaponAbilityController.CurrentCameraFov = currentZoomFov = Mathf.Lerp(currentZoomFov, zoomWeaponAbilityController.CameraFov, currentZoomInterpTime);
                if (currentZoomInterpTime >= 1f)
                {
                    currentZoomInterpTime = 0;
                    state = WeaponAbilityState.Deactivated;
                }
            }
            else if (state == WeaponAbilityState.Activating)
            {
                currentZoomInterpTime += deltaTime * ZOOM_SPEED;
                zoomWeaponAbilityController.CurrentCameraFov = currentZoomFov = Mathf.Lerp(currentZoomFov, zoomingFov, currentZoomInterpTime);
                if (currentZoomInterpTime >= 1f)
                {
                    currentZoomInterpTime = 0;
                    state = WeaponAbilityState.Activated;
                }
            }

            bool isActive = state == WeaponAbilityState.Activated || state == WeaponAbilityState.Activating;
            zoomWeaponAbilityController.ShowZoomCrosshair = isActive;
            zoomWeaponAbilityController.HideCrosshair = isActive;

            if (!isActive)
            {
                BasePlayerCharacterController.OwningCharacter.ModelManager.SetIsHide(CharacterModelManager.HIDE_SETTER_CONTROLLER, false);
            }
            else
            {
                if (disableRenderersOnZoom)
                    BasePlayerCharacterController.OwningCharacter.ModelManager.SetIsHide(CharacterModelManager.HIDE_SETTER_CONTROLLER, true);
            }
            return state;
        }

        public override void OnPreDeactivate()
        {
            zoomWeaponAbilityController.ViewMode = preActivateViewMode.Value;
            zoomWeaponAbilityController.RotationSpeedScale = 1f;
            currentZoomInterpTime = 0f;
            currentZoomFov = zoomWeaponAbilityController.CurrentCameraFov;
        }
    }
}
