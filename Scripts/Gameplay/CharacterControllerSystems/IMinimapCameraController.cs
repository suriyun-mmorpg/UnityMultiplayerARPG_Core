using UnityEngine;

namespace MultiplayerARPG
{
    public interface IMinimapCameraController
    {
        GameObject gameObject { get; }
        bool enabled { get; }
        Camera Camera { get; }
        Transform CameraTransform { get; }
        void Init(BasePlayerCharacterController controller);
        void Setup(BasePlayerCharacterEntity characterEntity);
        void Desetup(BasePlayerCharacterEntity characterEntity);
    }
}
