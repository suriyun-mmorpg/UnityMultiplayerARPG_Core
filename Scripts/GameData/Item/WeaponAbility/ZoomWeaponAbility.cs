using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [NotPatchable]
    [CreateAssetMenu(fileName = GameDataMenuConsts.ZOOM_WEAPON_ABILITY_FILE, menuName = GameDataMenuConsts.ZOOM_WEAPON_ABILITY_MENU, order = GameDataMenuConsts.ZOOM_WEAPON_ABILITY_ORDER)]
    public class ZoomWeaponAbility : BaseWeaponAbility
    {
        const float ZOOM_SPEED = 1.25f;
        public static event System.Action OnActivateZoomAbility;
        public static event System.Action OnDeactivateZoomAbility;

        public float zoomingFov = 20f;
        [Range(0.1f, 1f)]
        [FormerlySerializedAs("rotationSpeedScaleWhileZooming")]
        public float cameraRotationSpeedScaleWhileZooming = 0.5f;
        public string cameraRotationSpeedScaleSaveKey = string.Empty;
        public Sprite zoomCrosshair;
        public bool hideCrosshairWhileZooming;
        public bool shouldDeactivateOnReload;

        public const string KEY = "ZOOM_WEAPON_ABILITY";
        public override string AbilityKey => KEY;

        [System.NonSerialized]
        private float _currentZoomInterpTime;
        [System.NonSerialized]
        private float _currentZoomFov;
        [System.NonSerialized]
        private IZoomWeaponAbilityController _zoomWeaponAbilityController;

        public override bool ShouldDeactivateOnReload { get => shouldDeactivateOnReload; }

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
            _zoomWeaponAbilityController.ShowZoomCrosshair = false;
            _zoomWeaponAbilityController.OverrideHideCrosshair.Remove(this);
            _zoomWeaponAbilityController.UpdateCameraSettings();
            OnDeactivateZoomAbility?.Invoke();
        }

        public override void OnPreActivate()
        {
            if (zoomCrosshair)
            {
                _zoomWeaponAbilityController.SetZoomCrosshairSprite(zoomCrosshair);
            }
            _currentZoomInterpTime = 0f;
            _currentZoomFov = _zoomWeaponAbilityController.AssignedCameraFov;
            _zoomWeaponAbilityController.OverrideCameraFov.Set(this, _currentZoomFov, 0);
            _zoomWeaponAbilityController.OverrideIsZoomAimming.Set(this, true, 0);
            OnActivateZoomAbility?.Invoke();
        }

        public override void OnPreDeactivate()
        {
            _currentZoomInterpTime = 0f;
            _zoomWeaponAbilityController.OverrideCameraFov.Remove(this);
            _zoomWeaponAbilityController.OverrideIsZoomAimming.Remove(this);
            _zoomWeaponAbilityController.OverrideCameraRotationSpeedScale.Remove(this);
            OnDeactivateZoomAbility?.Invoke();
        }

        public override WeaponAbilityState UpdateActivation(WeaponAbilityState state, bool isBlockController, float deltaTime)
        {
            switch (state)
            {
                case WeaponAbilityState.Deactivated:
                    return state;
                case WeaponAbilityState.Activated:
                    _zoomWeaponAbilityController.OverrideCameraRotationSpeedScale.Set(this, CameraRotationSpeedScale, 0);
                    if (isBlockController || GameInstance.PlayingCharacterEntity.MovementState.Has(MovementState.IsUnderWater))
                    {
                        OnPreDeactivate();
                        state = WeaponAbilityState.Deactivating;
                    }
                    return state;
                case WeaponAbilityState.Deactivating:
                    _currentZoomInterpTime += deltaTime * ZOOM_SPEED;
                    _currentZoomFov = Mathf.Lerp(_currentZoomFov, _zoomWeaponAbilityController.AssignedCameraFov, _currentZoomInterpTime);
                    _zoomWeaponAbilityController.OverrideCameraFov.Set(this, _currentZoomFov, 0);
                    if (_currentZoomInterpTime >= 1f)
                    {
                        // Zooming updated, change state to deactivated
                        _currentZoomInterpTime = 0;
                        state = WeaponAbilityState.Deactivated;
                    }
                    break;
                case WeaponAbilityState.Activating:
                    _currentZoomInterpTime += deltaTime * ZOOM_SPEED;
                    _currentZoomFov = Mathf.Lerp(_currentZoomFov, zoomingFov, _currentZoomInterpTime);
                    _zoomWeaponAbilityController.OverrideCameraFov.Set(this, _currentZoomFov, 0);
                    _zoomWeaponAbilityController.OverrideCameraRotationSpeedScale.Set(this, CameraRotationSpeedScale, 0);
                    if (_currentZoomInterpTime >= 1f)
                    {
                        // Zooming updated, change state to activated
                        _currentZoomInterpTime = 0;
                        state = WeaponAbilityState.Activated;
                    }
                    break;
            }

            // Update crosshair / view
            bool isActive = state == WeaponAbilityState.Activated || state == WeaponAbilityState.Activating;
            _zoomWeaponAbilityController.ShowZoomCrosshair = zoomCrosshair && isActive;
            _zoomWeaponAbilityController.OverrideHideCrosshair.Set(this, (hideCrosshairWhileZooming || zoomCrosshair) && isActive, 0);

            return state;
        }
    }
}
