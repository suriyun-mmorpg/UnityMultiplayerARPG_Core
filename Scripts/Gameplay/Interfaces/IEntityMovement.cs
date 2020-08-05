using UnityEngine;

namespace MultiplayerARPG
{
    public interface IEntityMovement : IGameEntity
    {
        float StoppingDistance { get; }
        void StopMove();
        void KeyMovement(Vector3 moveDirection, MovementState movementState);
        void PointClickMovement(Vector3 position);
        void SetLookRotation(Quaternion rotation);
        Quaternion GetLookRotation();
        void Teleport(Vector3 position);
        bool FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result);
    }
}
