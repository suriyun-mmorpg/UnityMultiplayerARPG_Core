using UnityEngine;

namespace MultiplayerARPG
{
    public class EntityMovementInput
    {
        public bool IsKeyMovement { get; set; }
        public bool IsStopped { get; set; }
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
            input.IsStopped = false;
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
            input.IsStopped = false;
            input.MovementState = movementState;
            // Update extra movement state because some movement state can affect extra movement state
            input = SetInputExtraMovementState(entityMovement, input, input.ExtraMovementState);
            return input;
        }

        public static EntityMovementInput SetInputExtraMovementState(this IEntityMovementComponent entityMovement, EntityMovementInput input, ExtraMovementState extraMovementState)
        {
            if (input == null)
                input = entityMovement.InitInput();
            input.IsStopped = false;
            input.ExtraMovementState = entityMovement.ValidateExtraMovementState(input.MovementState, extraMovementState);
            return input;
        }

        public static EntityMovementInput SetInputPosition(this IEntityMovementComponent entityMovement, EntityMovementInput input, Vector3 position)
        {
            if (input == null)
                input = entityMovement.InitInput();
            input.IsStopped = false;
            input.Position = position;
            return input;
        }

        public static EntityMovementInput SetInputYPosition(this IEntityMovementComponent entityMovement, EntityMovementInput input, float yPosition)
        {
            if (input == null)
                input = entityMovement.InitInput();
            input.IsStopped = false;
            Vector3 position = input.Position;
            position.y = yPosition;
            input.Position = position;
            return input;
        }

        public static EntityMovementInput SetInputRotation(this IEntityMovementComponent entityMovement, EntityMovementInput input, Quaternion rotation)
        {
            if (input == null)
                input = entityMovement.InitInput();
            input.IsStopped = false;
            input.Rotation = rotation;
            return input;
        }

        public static EntityMovementInput SetInputDirection2D(this IEntityMovementComponent entityMovement, EntityMovementInput input, Vector2 direction2D)
        {
            if (input == null)
                input = entityMovement.InitInput();
            input.IsStopped = false;
            input.Direction2D = direction2D;
            return input;
        }

        public static EntityMovementInput SetInputJump(this IEntityMovementComponent entityMovement, EntityMovementInput input)
        {
            if (input == null)
                input = entityMovement.InitInput();
            input.IsStopped = false;
            input.MovementState |= MovementState.IsJump;
            return input;
        }

        public static EntityMovementInput ClearInputJump(this IEntityMovementComponent entityMovement, EntityMovementInput input)
        {
            if (input == null)
                input = entityMovement.InitInput();
            input.IsStopped = false;
            input.MovementState &= ~MovementState.IsJump;
            return input;
        }

        public static EntityMovementInput SetInputStop(this IEntityMovementComponent entityMovement, EntityMovementInput input)
        {
            if (input == null)
                input = entityMovement.InitInput();
            input.IsStopped = true;
            return input;
        }

        public static bool DifferInputEnoughToSend(this IEntityMovementComponent entityMovement, EntityMovementInput oldInput, EntityMovementInput newInput, out EntityMovementInputState state)
        {
            state = EntityMovementInputState.None;
            if (newInput == null)
                return false;
            if (oldInput == null)
            {
                state = EntityMovementInputState.PositionChanged | EntityMovementInputState.RotationChanged;
                if (newInput.IsStopped)
                    state |= EntityMovementInputState.IsStopped;
                if (newInput.IsKeyMovement)
                    state |= EntityMovementInputState.IsKeyMovement;
                if (newInput.MovementState.Has(MovementState.IsJump))
                    state |= EntityMovementInputState.IsJump;
                return true;
            }
            // TODO: Send delta changes
            if (newInput.IsStopped)
                state |= EntityMovementInputState.IsStopped;
            if (newInput.IsKeyMovement)
                state |= EntityMovementInputState.IsKeyMovement;
            if (Vector3.Distance(newInput.Position, oldInput.Position) > 0.01f)
                state |= EntityMovementInputState.PositionChanged;
            if (Quaternion.Angle(newInput.Rotation, oldInput.Rotation) > 1f)
                state |= EntityMovementInputState.RotationChanged;
            if (newInput.MovementState.Has(MovementState.IsJump))
                state |= EntityMovementInputState.IsJump;
            return state != EntityMovementInputState.None;
        }
    }
}
