using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IShooterWeaponController
    {
        ShooterControllerViewMode ViewMode { get; set; }
        float CameraZoomDistance { get; }
        float CameraMinZoomDistance { get; }
        float CameraMaxZoomDistance { get; }
        Vector3 CameraTargetOffsets { get; }
        float CameraFov { get; }
        float CameraNearClipPlane { get; }
        float CameraFarClipPlane { get; }
        CrosshairSetting CurrentCrosshairSetting { get; }
        float CurrentCameraFov { get; set; }
        float DefaultCameraRotationSpeedScale { get; }
        float CameraRotationSpeedScale { get; set; }
        bool HideCrosshair { get; set; }
        void UpdateCameraSettings();
    }
}
