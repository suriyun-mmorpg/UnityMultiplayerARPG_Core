namespace MultiplayerARPG
{
    public static class EntityMovementFunctions
    {
        #region Generic Functions
        public static ExtraMovementState ValidateExtraMovementState(this IEntityMovement movement, MovementState movementState, ExtraMovementState extraMovementState)
        {
            // Movement state can affect extra movement state
            if (movementState.Has(MovementState.IsUnderWater) ||
                movementState.Has(MovementState.IsClimbing))
            {
                // Extra movement states always none while under water
                extraMovementState = ExtraMovementState.None;
            }
            else if (!movement.Entity.CanMove() && extraMovementState == ExtraMovementState.IsSprinting)
            {
                // Character can't move, set extra movement state to none
                extraMovementState = ExtraMovementState.None;
            }
            else
            {
                switch (extraMovementState)
                {
                    case ExtraMovementState.IsSprinting:
                        if (!movementState.HasDirectionMovement())
                            extraMovementState = ExtraMovementState.None;
                        else if (!movement.Entity.CanSprint())
                            extraMovementState = ExtraMovementState.None;
                        else if (!movement.Entity.CanSideSprint && (movementState.Has(MovementState.Left) || movementState.Has(MovementState.Right)))
                            extraMovementState = ExtraMovementState.None;
                        else if (!movement.Entity.CanBackwardSprint && movementState.Has(MovementState.Backward))
                            extraMovementState = ExtraMovementState.None;
                        break;
                    case ExtraMovementState.IsWalking:
                        if (!movementState.HasDirectionMovement())
                            extraMovementState = ExtraMovementState.None;
                        else if (!movement.Entity.CanWalk())
                            extraMovementState = ExtraMovementState.None;
                        break;
                    case ExtraMovementState.IsCrouching:
                        if (!movement.Entity.CanCrouch())
                            extraMovementState = ExtraMovementState.None;
                        break;
                    case ExtraMovementState.IsCrawling:
                        if (!movement.Entity.CanCrawl())
                            extraMovementState = ExtraMovementState.None;
                        break;
                }
            }
            return extraMovementState;
        }
        #endregion
    }
}
