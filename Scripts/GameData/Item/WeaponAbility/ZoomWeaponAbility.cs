using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.ZOOM_WEAPON_ABILITY_FILE, menuName = GameDataMenuConsts.ZOOM_WEAPON_ABILITY_MENU, order = GameDataMenuConsts.ZOOM_WEAPON_ABILITY_ORDER)]
    public class ZoomWeaponAbility : BaseWeaponAbility
    {
        const float ZOOM_SPEED = 1.25f;

        public float zoomingFov = 20f;
        [Range(0.1f, 1f)]
        [FormerlySerializedAs("rotationSpeedScaleWhileZooming")]
        public float cameraRotationSpeedScaleWhileZooming = 0.5f;
        public string cameraRotationSpeedScaleSaveKey = string.Empty;
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

        public override bool ShouldDeactivateWhenReload { get { return true; } }

        public float CameraRotationSpeedScale
        {
            get { return CameraRotationSpeedScaleSetting.GetCameraRotationSpeedScaleByKey(cameraRotationSpeedScaleSaveKey, cameraRotationSpeedScaleWhileZooming); }
        }

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
            zoomWeaponAbilityController.ShowZoomCrosshair = false;
            zoomWeaponAbilityController.HideCrosshair = false;
            zoomWeaponAbilityController.UpdateCameraSettings();
            if (disableRenderersOnZoom)
                GameInstance.PlayingCharacterEntity.ModelManager.SetIsHide(CharacterModelManager.HIDE_SETTER_CONTROLLER, false);
        }

        public override void OnPreActivate()
        {
            preActivateViewMode = zoomWeaponAbilityController.ViewMode;
            if (zoomCrosshair)
            {
                zoomWeaponAbilityController.ViewMode = ShooterControllerViewMode.Fps;
                zoomWeaponAbilityController.SetZoomCrosshairSprite(zoomCrosshair);
            }
            currentZoomInterpTime = 0f;
            currentZoomFov = zoomWeaponAbilityController.CurrentCameraFov;
        }

        public override WeaponAbilityState UpdateActivation(WeaponAbilityState state, float deltaTime)
        {
            switch (state)
            {
                case WeaponAbilityState.Deactivated:
                    return state;
                case WeaponAbilityState.Activated:
                    zoomWeaponAbilityController.CameraRotationSpeedScale = CameraRotationSpeedScale;
                    return state;
                case WeaponAbilityState.Deactivating:
                    currentZoomInterpTime += deltaTime * ZOOM_SPEED;
                    zoomWeaponAbilityController.CurrentCameraFov = currentZoomFov = Mathf.Lerp(currentZoomFov, zoomWeaponAbilityController.CameraFov, currentZoomInterpTime);
                    if (currentZoomInterpTime >= 1f)
                    {
                        currentZoomInterpTime = 0;
                        state = WeaponAbilityState.Deactivated;
                    }
                    break;
                case WeaponAbilityState.Activating:
                    currentZoomInterpTime += deltaTime * ZOOM_SPEED;
                    zoomWeaponAbilityController.CurrentCameraFov = currentZoomFov = Mathf.Lerp(currentZoomFov, zoomingFov, currentZoomInterpTime);
                    zoomWeaponAbilityController.CameraRotationSpeedScale = CameraRotationSpeedScale;
                    if (currentZoomInterpTime >= 1f)
                    {
                        currentZoomInterpTime = 0;
                        state = WeaponAbilityState.Activated;
                    }
                    break;
            }

            bool isActive = state == WeaponAbilityState.Activated || state == WeaponAbilityState.Activating;
            zoomWeaponAbilityController.ShowZoomCrosshair = zoomCrosshair && isActive;
            zoomWeaponAbilityController.HideCrosshair = zoomCrosshair && isActive;

            if (!isActive)
            {
                if (disableRenderersOnZoom)
                    GameInstance.PlayingCharacterEntity.ModelManager.SetIsHide(CharacterModelManager.HIDE_SETTER_CONTROLLER, false);
            }
            else
            {
                if (disableRenderersOnZoom)
                    GameInstance.PlayingCharacterEntity.ModelManager.SetIsHide(CharacterModelManager.HIDE_SETTER_CONTROLLER, true);
            }
            return state;
        }

        public override void OnPreDeactivate()
        {
            zoomWeaponAbilityController.ViewMode = preActivateViewMode.Value;
            currentZoomInterpTime = 0f;
            currentZoomFov = zoomWeaponAbilityController.CurrentCameraFov;
        }
    }
}
