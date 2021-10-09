using UnityEngine;

namespace MultiplayerARPG
{
    public interface IGameplayCameraController
    {
        GameObject GameObject { get; }
        Camera Camera { get; }
        Transform CameraTransform { get; }
        Transform FollowingEntityTransform { get; set; }
        Vector3 TargetOffset { get; set; }
        float MinZoomDistance { get; set; }
        float MaxZoomDistance { get; set; }
        float CurrentZoomDistance { get; set; }
        bool EnableWallHitSpring { get; set; }
        bool UpdateRotation { get; set; }
        bool UpdateRotationX { get; set; }
        bool UpdateRotationY { get; set; }
        bool UpdateZoom { get; set; }
        void Setup(BasePlayerCharacterEntity characterEntity);
        void Desetup(BasePlayerCharacterEntity characterEntity);
    }
}
