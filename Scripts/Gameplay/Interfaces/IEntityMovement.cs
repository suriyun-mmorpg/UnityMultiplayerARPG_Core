using UnityEngine;

namespace MultiplayerARPG
{
    public interface IEntityMovement
    {
        BaseGameEntity Entity { get; }
        bool IsGrounded { get; }
        bool IsJumping { get; }
        bool IsUnderWater { get; }
        float StoppingDistance { get; }
        void StopMove();
        void KeyMovement(Vector3 moveDirection, MovementState movementState);
        void PointClickMovement(Vector3 position);
        void SetLookRotation(Quaternion rotation);
        Quaternion GetLookRotation();
        void Teleport(Vector3 position);
        void FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result);
    }
}
