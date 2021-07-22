using UnityEngine;

namespace MultiplayerARPG
{
    public interface IMoveableModel
    {
        void SetMoveAnimationSpeedMultiplier(float moveAnimationSpeedMultiplier);
        void SetMovementState(MovementState movementState, Vector2 direction2D);
    }
}
