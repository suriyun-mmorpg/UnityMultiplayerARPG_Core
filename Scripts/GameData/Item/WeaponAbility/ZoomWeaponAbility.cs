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

        private float zoomInterpTime;
        private ShooterPlayerCharacterController shooterController;

        public float DefaultCameraFOV { get; protected set; }
        public Vector3 DefaultCameraOffset { get; protected set; }
        public float DefaultCameraZoomDistance { get; protected set; }
        public bool DefaultCameraEnableWallHitSpring { get; protected set; }

        public override void Setup(BasePlayerCharacterController controller, CharacterItem weapon)
        {
            base.Setup(controller, weapon);
            shooterController = controller as ShooterPlayerCharacterController;
            shooterController.zoomCrosshairImage.preserveAspect = true;
            shooterController.zoomCrosshairImage.raycastTarget = false;
        }

        public override void Desetup()
        {
            shooterController.CacheGameplayCameraControls.CacheCamera.fieldOfView = DefaultCameraFOV;
            shooterController.CacheGameplayCameraControls.targetOffset = DefaultCameraOffset;
            shooterController.CacheGameplayCameraControls.zoomDistance = DefaultCameraZoomDistance;
            shooterController.CacheGameplayCameraControls.enableWallHitSpring = DefaultCameraEnableWallHitSpring;
            if (shooterController.zoomCrosshairImage != null)
                shooterController.zoomCrosshairImage.gameObject.SetActive(false);
        }

        public override bool IsTurnToTargetWhileActivated()
        {
            return true;
        }

        public override void OnPreActivate()
        {
            DefaultCameraFOV = shooterController.CacheGameplayCameraControls.CacheCamera.fieldOfView;
            DefaultCameraOffset = shooterController.CacheGameplayCameraControls.targetOffset;
            DefaultCameraZoomDistance = shooterController.CacheGameplayCameraControls.zoomDistance;
            DefaultCameraEnableWallHitSpring = shooterController.CacheGameplayCameraControls.enableWallHitSpring;
        }

        public override WeaponAbilityState UpdateActivation(WeaponAbilityState state, float deltaTime)
        {
            if (state == WeaponAbilityState.Deactivating)
            {
                zoomInterpTime += deltaTime * ZOOM_SPEED;
                shooterController.CacheGameplayCameraControls.CacheCamera.fieldOfView = Mathf.Lerp(shooterController.CacheGameplayCameraControls.CacheCamera.fieldOfView, DefaultCameraFOV, zoomInterpTime);
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
            if (shooterController.crosshairRect != null &&
                shooterController.crosshairRect.gameObject.activeSelf && isActive)
            {
                // Hide crosshair when not active
                shooterController.crosshairRect.gameObject.SetActive(false);
            }

            if (shooterController.zoomCrosshairImage != null &&
                shooterController.zoomCrosshairImage.gameObject.activeSelf != isActive)
            {
                shooterController.zoomCrosshairImage.gameObject.SetActive(isActive);
                shooterController.zoomCrosshairImage.sprite = zoomCrosshair;
            }

            if (isActive)
            {
                Vector3 offset = shooterController.CacheGameplayCameraControls.targetOffset;
                offset.x = 0f;
                // Change offset
                shooterController.CacheGameplayCameraControls.targetOffset = offset;
            }
            else
            {
                shooterController.CacheGameplayCameraControls.targetOffset = DefaultCameraOffset;
            }

            if (isActive)
            {
                shooterController.CacheGameplayCameraControls.zoomDistance = 0;
            }
            else
            {
                shooterController.CacheGameplayCameraControls.zoomDistance = DefaultCameraZoomDistance;
            }

            if (isActive)
            {
                shooterController.CacheGameplayCameraControls.enableWallHitSpring = false;
            }
            else
            {
                shooterController.CacheGameplayCameraControls.enableWallHitSpring = DefaultCameraEnableWallHitSpring;
            }

            // Hidding character model while activate
            shooterController.PlayerCharacterEntity.ModelManager.SetHide(CharacterModelManager.HIDE_SETTER_CONTROLLER, isActive);

            return state;
        }

        public override void OnPreDeactivate()
        {
        }
    }
}
