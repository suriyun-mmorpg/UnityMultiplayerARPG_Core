using UnityEngine;

namespace MultiplayerARPG
{
    public interface IEntityMovement
    {
        bool enabled { get; set; }
        bool IsGrounded { get; }
        bool IsJumping { get; }
        float StoppingDistance { get; }
        void StopMove();
        void KeyMovement(Vector3 moveDirection, MovementState movementState);
        void PointClickMovement(Vector3 position);
        void SetExtraMovement(ExtraMovementState extraMovementState);
        void SetLookRotation(Quaternion rotation);
        Quaternion GetLookRotation();
        void Teleport(Vector3 position);
        void FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result);
    }
}
