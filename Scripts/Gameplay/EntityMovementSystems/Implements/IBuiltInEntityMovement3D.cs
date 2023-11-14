using UnityEngine;

namespace MultiplayerARPG
{
    public partial interface IBuiltInEntityMovement3D
    {
        bool GroundCheck();
        void Move(Vector3 motion);
        void RotateY(float yAngle);
        void OnJumpForceApplied(float verticalVelocity);
        Bounds GetBounds();
    }
}
