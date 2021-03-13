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
        public static EntityMovementInput SetIsKeyMovement(this EntityMovementInput input, bool isKeyMovement)
        {
            if (input == null)
                input = new EntityMovementInput();
            input.IsKeyMovement = isKeyMovement;
            return input;
        }

        public static EntityMovementInput SetMovementState(this EntityMovementInput input, MovementState movementState)
        {
            if (input == null)
                input = new EntityMovementInput();
            bool isJump = input.MovementState.HasFlag(MovementState.IsJump);
            input.MovementState = movementState;
            if (isJump)
                input = input.SetJump();
            return input;
        }

        public static EntityMovementInput SetPosition(this EntityMovementInput input, Vector3 position)
        {
            if (input == null)
                input = new EntityMovementInput();
            input.Position = position;
            return input;
        }

        public static EntityMovementInput SetYPosition(this EntityMovementInput input, float yPosition)
        {
            if (input == null)
                input = new EntityMovementInput();
            Vector3 position = input.Position;
            position.y = yPosition;
            input.Position = position;
            return input;
        }

        public static EntityMovementInput SetRotation(this EntityMovementInput input, Quaternion rotation)
        {
            if (input == null)
                input = new EntityMovementInput();
            input.Rotation = rotation;
            return input;
        }

        public static EntityMovementInput SetJump(this EntityMovementInput input)
        {
            if (input == null)
                input = new EntityMovementInput();
            input.MovementState = input.MovementState | MovementState.IsJump;
            return input;
        }

        public static bool DifferInputEnoughToSend(this IEntityMovement entityMovement, EntityMovementInput oldInput, EntityMovementInput newInput)
        {
            if (entityMovement == null || newInput == null)
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
