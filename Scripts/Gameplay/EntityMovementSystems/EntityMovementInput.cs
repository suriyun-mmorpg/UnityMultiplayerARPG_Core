using UnityEngine;

namespace MultiplayerARPG
{
    public class EntityMovementInput
    {
        public bool IsKeyMovement { get; set; }
        public MovementState MovementState { get; set; }
        public ExtraMovementState ExtraMovementState { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector2 Direction2D { get; set; }
    }

    public static class EntityMovementInputExtension
    {
        public static EntityMovementInput InitInput(this IEntityMovementComponent entityMovement)
        {
            return new EntityMovementInput()
            {
                Position = entityMovement.Entity.CacheTransform.position,
                Rotation = entityMovement.Entity.CacheTransform.rotation,
            };
        }

        public static EntityMovementInput SetInputIsKeyMovement(this IEntityMovementComponent entityMovement, EntityMovementInput input, bool isKeyMovement)
        {
            if (input == null)
                input = entityMovement.InitInput();
            input.IsKeyMovement = isKeyMovement;
            return input;
        }

        public static EntityMovementInput SetInputMovementState(this IEntityMovementComponent entityMovement, EntityMovementInput input, MovementState movementState)
        {
            if (input == null)
                input = entityMovement.InitInput();
            bool isJump = input.MovementState.Has(MovementState.IsJump);
            input.MovementState = movementState;
            if (isJump)
                input = entityMovement.SetInputJump(input);
            // Update extra movement state because some movement state can affect extra movement state
            input = SetInputExtraMovementState(entityMovement, input, input.ExtraMovementState);
            return input;
        }

        public static EntityMovementInput SetInputMovementState2D(this IEntityMovementComponent entityMovement, EntityMovementInput input, MovementState movementState)
        {
            if (input == null)
                input = entityMovement.InitInput();
            input.MovementState = movementState;
            // Update extra movement state because some movement state can affect extra movement state
            input = SetInputExtraMovementState(entityMovement, input, input.ExtraMovementState);
            return input;
        }

        public static EntityMovementInput SetInputExtraMovementState(this IEntityMovementComponent entityMovement, EntityMovementInput input, ExtraMovementState extraMovementState)
        {
            if (input == null)
                input = entityMovement.InitInput();
            input.ExtraMovementState = entityMovement.ValidateExtraMovementState(input.MovementState, extraMovementState);
            return input;
        }

        public static EntityMovementInput SetInputPosition(this IEntityMovementComponent entityMovement, EntityMovementInput input, Vector3 position)
        {
            if (input == null)
                input = entityMovement.InitInput();
            input.Position = position;
            return input;
        }

        public static EntityMovementInput SetInputYPosition(this IEntityMovementComponent entityMovement, EntityMovementInput input, float yPosition)
        {
            if (input == null)
                input = entityMovement.InitInput();
            Vector3 position = input.Position;
            position.y = yPosition;
            input.Position = position;
            return input;
        }

        public static EntityMovementInput SetInputRotation(this IEntityMovementComponent entityMovement, EntityMovementInput input, Quaternion rotation)
        {
            if (input == null)
                input = entityMovement.InitInput();
            input.Rotation = rotation;
            return input;
        }

        public static EntityMovementInput SetInputDirection2D(this IEntityMovementComponent entityMovement, EntityMovementInput input, Vector2 direction2D)
        {
            if (input == null)
                input = entityMovement.InitInput();
            input.Direction2D = direction2D;
            return input;
        }

        public static EntityMovementInput SetInputJump(this IEntityMovementComponent entityMovement, EntityMovementInput input)
        {
            if (input == null)
                input = entityMovement.InitInput();
            input.MovementState = input.MovementState | MovementState.IsJump;
            return input;
        }

        public static bool DifferInputEnoughToSend(this IEntityMovementComponent entityMovement, EntityMovementInput oldInput, EntityMovementInput newInput, out InputState state)
        {
            state = InputState.None;
            if (newInput == null)
                return false;
            if (oldInput == null)
            {
                state = InputState.PositionChanged | InputState.RotationChanged;
                if (newInput.IsKeyMovement)
                    state |= InputState.IsKeyMovement;
                if (newInput.MovementState.Has(MovementState.IsJump))
                    state |= InputState.IsJump;
                return true;
            }
            // TODO: Send delta changes
            if (newInput.IsKeyMovement)
                state |= InputState.IsKeyMovement;
            if (Vector3.Distance(newInput.Position, oldInput.Position) > 0.01f)
                state |= InputState.PositionChanged;
            if (Quaternion.Angle(newInput.Rotation, oldInput.Rotation) > 1)
                state |= InputState.RotationChanged;
            if (newInput.MovementState.Has(MovementState.IsJump))
                state |= InputState.IsJump;
            return state != InputState.None;
        }
    }
}
