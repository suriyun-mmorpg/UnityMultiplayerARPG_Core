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
        private float _currentZoomInterpTime;
        [System.NonSerialized]
        private float _currentZoomFov;
        [System.NonSerialized]
        private IZoomWeaponAbilityController _zoomWeaponAbilityController;
        [System.NonSerialized]
        private ShooterControllerViewMode? _preActivateViewMode;

        public override bool ShouldDeactivateWhenReload { get { return true; } }

        public float CameraRotationSpeedScale
        {
            get { return CameraRotationSpeedScaleSetting.GetCameraRotationSpeedScaleByKey(cameraRotationSpeedScaleSaveKey, cameraRotationSpeedScaleWhileZooming); }
        }

        public override void Setup(BasePlayerCharacterController controller, CharacterItem weapon)
        {
            base.Setup(controller, weapon);
            _zoomWeaponAbilityController = controller as IZoomWeaponAbilityController;
            _zoomWeaponAbilityController.InitialZoomCrosshair();
        }

        public override void Desetup()
        {
            ForceDeactivated();
        }

        public override void ForceDeactivated()
        {
            if (_preActivateViewMode.HasValue)
                _zoomWeaponAbilityController.ViewMode = _preActivateViewMode.Value;
            _zoomWeaponAbilityController.ShowZoomCrosshair = false;
            _zoomWeaponAbilityController.HideCrosshair = false;
            _zoomWeaponAbilityController.UpdateCameraSettings();
            if (disableRenderersOnZoom)
                GameInstance.PlayingCharacterEntity.ModelManager.SetIsHide(CharacterModelManager.HIDE_SETTER_CONTROLLER, false);
        }

        public override void OnPreActivate()
        {
            _preActivateViewMode = _zoomWeaponAbilityController.ViewMode;
            if (zoomCrosshair)
            {
                _zoomWeaponAbilityController.ViewMode = ShooterControllerViewMode.Fps;
                _zoomWeaponAbilityController.SetZoomCrosshairSprite(zoomCrosshair);
            }
            _currentZoomInterpTime = 0f;
            _currentZoomFov = _zoomWeaponAbilityController.CurrentCameraFov;
        }

        public override WeaponAbilityState UpdateActivation(WeaponAbilityState state, float deltaTime)
        {
            switch (state)
            {
                case WeaponAbilityState.Deactivated:
                    return state;
                case WeaponAbilityState.Activated:
                    _zoomWeaponAbilityController.CameraRotationSpeedScale = CameraRotationSpeedScale;
                    return state;
                case WeaponAbilityState.Deactivating:
                    _currentZoomInterpTime += deltaTime * ZOOM_SPEED;
                    _zoomWeaponAbilityController.CurrentCameraFov = _currentZoomFov = Mathf.Lerp(_currentZoomFov, _zoomWeaponAbilityController.CameraFov, _currentZoomInterpTime);
                    if (_currentZoomInterpTime >= 1f)
                    {
                        _currentZoomInterpTime = 0;
                        state = WeaponAbilityState.Deactivated;
                    }
                    break;
                case WeaponAbilityState.Activating:
                    _currentZoomInterpTime += deltaTime * ZOOM_SPEED;
                    _zoomWeaponAbilityController.CurrentCameraFov = _currentZoomFov = Mathf.Lerp(_currentZoomFov, zoomingFov, _currentZoomInterpTime);
                    _zoomWeaponAbilityController.CameraRotationSpeedScale = CameraRotationSpeedScale;
                    if (_currentZoomInterpTime >= 1f)
                    {
                        _currentZoomInterpTime = 0;
                        state = WeaponAbilityState.Activated;
                    }
                    break;
            }

            bool isActive = state == WeaponAbilityState.Activated || state == WeaponAbilityState.Activating;
            _zoomWeaponAbilityController.ShowZoomCrosshair = zoomCrosshair && isActive;
            _zoomWeaponAbilityController.HideCrosshair = zoomCrosshair && isActive;

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
            _zoomWeaponAbilityController.ViewMode = _preActivateViewMode.Value;
            _currentZoomInterpTime = 0f;
            _currentZoomFov = _zoomWeaponAbilityController.CurrentCameraFov;
        }
    }
}
