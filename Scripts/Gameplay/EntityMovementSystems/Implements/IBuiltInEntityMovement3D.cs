using UnityEngine;

namespace MultiplayerARPG
{
    public partial interface IBuiltInEntityMovement3D
    {
        bool GroundCheck();
        void SetPosition(Vector3 position);
        void Move(Vector3 motion);
        void RotateY(float yAngle);
        void OnJumpForceApplied(float verticalVelocity);
        Bounds GetMovementBounds();
        Vector3 GetSnapToGroundMotion(Vector3 motion, Vector3 platformMotion, Vector3 forceMotion);
    }
}
