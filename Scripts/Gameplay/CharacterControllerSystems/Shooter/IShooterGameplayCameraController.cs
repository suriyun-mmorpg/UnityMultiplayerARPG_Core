using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IShooterGameplayCameraController : IGameplayCameraController
    {
        bool EnableAimAssist { get; set; }
        bool EnableAimAssistX { get; set; }
        bool EnableAimAssistY { get; set; }
        bool AimAssistCharacter { get; set; }
        bool AimAssistBuilding { get; set; }
        bool AimAssistHarvestable { get; set; }
        float AimAssistRadius { get; set; }
        float AimAssistXSpeed { get; set; }
        float AimAssistYSpeed { get; set; }
        float AimAssistMaxAngleFromFollowingTarget { get; set; }
        float RotationSpeedScale { get; set; }
        void Recoil(float x, float y);
    }
}
