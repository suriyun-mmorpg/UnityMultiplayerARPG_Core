namespace MultiplayerARPG
{
    public interface IShooterGameplayCameraController : IGameplayCameraController
    {
        bool EnableAimAssist { get; set; }
        bool EnableAimAssistX { get; set; }
        bool EnableAimAssistY { get; set; }
        bool AimAssistPlayer { get; set; }
        bool AimAssistMonster { get; set; }
        bool AimAssistBuilding { get; set; }
        bool AimAssistHarvestable { get; set; }
        float AimAssistRadius { get; set; }
        float AimAssistXSpeed { get; set; }
        float AimAssistYSpeed { get; set; }
        float AimAssistMaxAngleFromFollowingTarget { get; set; }
        float CameraRotationSpeedScale { get; set; }
        void Recoil(float x, float y);
    }
}
