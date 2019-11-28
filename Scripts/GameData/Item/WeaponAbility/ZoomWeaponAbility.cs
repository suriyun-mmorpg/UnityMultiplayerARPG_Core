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
        public bool disableRenderersOnZoom;
        public Sprite zoomCrosshair;

        [System.NonSerialized]
        private float zoomInterpTime;
        [System.NonSerialized]
        private ShooterPlayerCharacterController shooterController;
        [System.NonSerialized]
        private ShooterPlayerCharacterController.ControllerViewMode preActivateViewMode;

        public override void Setup(BasePlayerCharacterController controller, CharacterItem weapon)
        {
            base.Setup(controller, weapon);
            shooterController = controller as ShooterPlayerCharacterController;
            shooterController.zoomCrosshairImage.preserveAspect = true;
            shooterController.zoomCrosshairImage.raycastTarget = false;
        }

        public override void Desetup()
        {
            if (shooterController.zoomCrosshairImage != null)
                shooterController.zoomCrosshairImage.gameObject.SetActive(false);
        }

        public override void OnPreActivate()
        {
            preActivateViewMode = shooterController.ViewMode;
            shooterController.ViewMode = ShooterPlayerCharacterController.ControllerViewMode.Fps;
            shooterController.SetZoomCrosshairSprite(zoomCrosshair);
        }

        public override WeaponAbilityState UpdateActivation(WeaponAbilityState state, float deltaTime)
        {
            if (state == WeaponAbilityState.Deactivating)
            {
                zoomInterpTime += deltaTime * ZOOM_SPEED;
                shooterController.CacheGameplayCameraControls.CacheCamera.fieldOfView = Mathf.Lerp(shooterController.CacheGameplayCameraControls.CacheCamera.fieldOfView, shooterController.CameraFov, zoomInterpTime);
                if (zoomInterpTime >= 1f)
                {
                    zoomInterpTime = 0;
                    state = WeaponAbilityState.Deactivated;
                }
            }
            else if (state == WeaponAbilityState.Activating)
            {
                zoomInterpTime += deltaTime * ZOOM_SPEED;
                shooterController.CacheGameplayCameraControls.CacheCamera.fieldOfView = Mathf.Lerp(shooterController.CacheGameplayCameraControls.CacheCamera.fieldOfView, zoomingFov, zoomInterpTime);
                if (zoomInterpTime >= 1f)
                {
                    zoomInterpTime = 0;
                    state = WeaponAbilityState.Activated;
                }
            }

            bool isActive = state == WeaponAbilityState.Activated || state == WeaponAbilityState.Activating;
            shooterController.SetActiveZoomCrosshair(isActive);
            shooterController.SetActiveCrosshair(!isActive && !shooterController.CurrentCrosshairSetting.hidden);

            if (!isActive)
                shooterController.ViewMode = preActivateViewMode;

            // Hidding character model while activate
            shooterController.PlayerCharacterEntity.ModelManager.SetHide(CharacterModelManager.HIDE_SETTER_CONTROLLER, isActive);

            return state;
        }

        public override void OnPreDeactivate()
        {
            // Do Nothing
        }
    }
}
