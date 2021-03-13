using UnityEngine;

namespace MultiplayerARPG
{
    public class EntityMovementInput
    {
        public bool IsKeyMovement { get; set; }
        public MovementState MovementState { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
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

        public static EntityMovementInput SetIsKeyMovement(this IEntityMovementComponent entityMovement, EntityMovementInput input, bool isKeyMovement)
        {
            if (input == null)
                input = entityMovement.InitInput();
            input.IsKeyMovement = isKeyMovement;
            return input;
        }

        public static EntityMovementInput SetMovementState(this IEntityMovementComponent entityMovement, EntityMovementInput input, MovementState movementState)
        {
            if (input == null)
                input = entityMovement.InitInput();
            bool isJump = input.MovementState.HasFlag(MovementState.IsJump);
            input.MovementState = movementState;
            if (isJump)
                input = entityMovement.SetJump(input);
            return input;
        }

        public static EntityMovementInput SetPosition(this IEntityMovementComponent entityMovement, EntityMovementInput input, Vector3 position)
        {
            if (input == null)
                input = entityMovement.InitInput();
            input.Position = position;
            return input;
        }

        public static EntityMovementInput SetYPosition(this IEntityMovementComponent entityMovement, EntityMovementInput input, float yPosition)
        {
            if (input == null)
                input = entityMovement.InitInput();
            Vector3 position = input.Position;
            position.y = yPosition;
            input.Position = position;
            return input;
        }

        public static EntityMovementInput SetRotation(this IEntityMovementComponent entityMovement, EntityMovementInput input, Quaternion rotation)
        {
            if (input == null)
                input = entityMovement.InitInput();
            input.Rotation = rotation;
            return input;
        }

        public static EntityMovementInput SetJump(this IEntityMovementComponent entityMovement, EntityMovementInput input)
        {
            if (input == null)
                input = entityMovement.InitInput();
            input.MovementState = input.MovementState | MovementState.IsJump;
            return input;
        }

        public static bool DifferInputEnoughToSend(this IEntityMovementComponent entityMovement, EntityMovementInput oldInput, EntityMovementInput newInput)
        {
            if (newInput == null)
                return false;
            if (oldInput == null)
                return true;
            if (Vector3.Distance(newInput.Position, oldInput.Position) > entityMovement.StoppingDistance)
                return true;
            if (Quaternion.Angle(newInput.Rotation, oldInput.Rotation) > 1)
                return true;
            if (newInput.MovementState.HasFlag(MovementState.IsJump))
                return true;
            return false;
        }
    }
}
