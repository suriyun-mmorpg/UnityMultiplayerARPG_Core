using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface ICharacterMovement
    {
        float StoppingDistance { get; }
        void StopMove();
        void KeyMovement(Vector3 moveDirection, MovementState movementState);
        void PointClickMovement(Vector3 position);
        void UpdateYRotation(float yRotation);
        void Teleport(Vector3 position);
        void FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result);
    }
}
