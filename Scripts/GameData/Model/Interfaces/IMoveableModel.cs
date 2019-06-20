namespace MultiplayerARPG
{
    public interface IMoveableModel
    {
        void UpdateMovementAnimation(MovementState movementState, float playMoveSpeedMultiplier = 1f);
    }
}
