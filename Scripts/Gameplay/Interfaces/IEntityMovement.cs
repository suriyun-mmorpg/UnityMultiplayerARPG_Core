using UnityEngine;

namespace MultiplayerARPG
{
    public interface IEntityMovement
    {
        bool IsGrounded { get; }
        bool IsJumping { get; }
        float StoppingDistance { get; }
        void StopMove();
        void KeyMovement(Vector3 moveDirection, MovementState movementState);
        void PointClickMovement(Vector3 position);
        void SetLookRotation(Vector3 eulerAngles);
        void Teleport(Vector3 position);
        void FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result);
    }
}
