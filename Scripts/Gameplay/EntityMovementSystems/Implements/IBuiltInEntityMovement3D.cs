using UnityEngine;

namespace MultiplayerARPG
{
    public interface IBuiltInEntityMovement3D
    {
        bool GroundCheck();
        void Move(Vector3 motion);
        void RotateY(float yAngle);
        void OnJumpForceApplied();
        Bounds GetBounds();
    }
}
