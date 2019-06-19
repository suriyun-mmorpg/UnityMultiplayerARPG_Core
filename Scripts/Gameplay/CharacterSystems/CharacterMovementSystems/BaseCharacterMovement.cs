using LiteNetLib;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseCharacterMovement : BaseCharacterComponent, ICharacterMovement
    {
        public virtual bool IsGrounded { get; protected set; }
        public virtual bool IsJumping { get; protected set; }
        public abstract float StoppingDistance { get; }
        public abstract void KeyMovement(Vector3 moveDirection, MovementState movementState);
        public abstract void PointClickMovement(Vector3 position);
        public abstract void StopMove();
        public abstract void SetLookRotation(Vector3 eulerAngles);
        public abstract void Teleport(Vector3 position);
        public abstract void FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result);
    }
}
