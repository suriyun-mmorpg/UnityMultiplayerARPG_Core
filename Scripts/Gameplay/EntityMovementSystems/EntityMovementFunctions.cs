using System.Collections.Generic;
using LiteNetLib.Utils;
using UnityEngine;

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

        #region Movement Input Serialization (3D)
        public static void ClientWriteMovementInput3D(this IEntityMovement movement, NetDataWriter writer, EntityMovementInputState inputState, EntityMovementInput movementInput)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            writer.Put((byte)inputState);
            writer.PutPackedUInt((uint)movementInput.MovementState);
            if (!inputState.Has(EntityMovementInputState.IsStopped))
                writer.Put((byte)movementInput.ExtraMovementState);
            if (inputState.Has(EntityMovementInputState.PositionChanged))
                writer.PutVector3(movementInput.Position);
            if (inputState.Has(EntityMovementInputState.RotationChanged))
                writer.PutPackedInt(GetCompressedAngle(movementInput.YAngle));
        }

        public static void ReadMovementInputMessage3D(this NetDataReader reader, out EntityMovementInputState inputState, out EntityMovementInput entityMovementInput)
        {
            entityMovementInput = new EntityMovementInput();
            inputState = (EntityMovementInputState)reader.GetByte();
            entityMovementInput.MovementState = (MovementState)reader.GetPackedUInt();
            if (!inputState.Has(EntityMovementInputState.IsStopped))
                entityMovementInput.ExtraMovementState = (ExtraMovementState)reader.GetByte();
            else
                entityMovementInput.ExtraMovementState = ExtraMovementState.None;
            if (inputState.Has(EntityMovementInputState.PositionChanged))
                entityMovementInput.Position = reader.GetVector3();
            if (inputState.Has(EntityMovementInputState.RotationChanged))
                entityMovementInput.YAngle = GetDecompressedAngle(reader.GetPackedInt());
        }
        #endregion

        #region Movement Input Serialization (2D)
        public static void ClientWriteMovementInput2D(this IEntityMovement movement, NetDataWriter writer, EntityMovementInputState inputState, EntityMovementInput movementInput)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            writer.Put((byte)inputState);
            writer.PutPackedUInt((uint)movementInput.MovementState);
            if (!inputState.Has(EntityMovementInputState.IsStopped))
                writer.Put((byte)movementInput.ExtraMovementState);
            if (inputState.Has(EntityMovementInputState.PositionChanged))
                writer.PutVector2(movementInput.Position);
            writer.Put(movementInput.Direction2D);
        }

        public static void ReadMovementInputMessage2D(this NetDataReader reader, out EntityMovementInputState inputState, out EntityMovementInput entityMovementInput)
        {
            entityMovementInput = new EntityMovementInput();
            inputState = (EntityMovementInputState)reader.GetByte();
            entityMovementInput.MovementState = (MovementState)reader.GetPackedUInt();
            if (!inputState.Has(EntityMovementInputState.IsStopped))
                entityMovementInput.ExtraMovementState = (ExtraMovementState)reader.GetByte();
            else
                entityMovementInput.ExtraMovementState = ExtraMovementState.None;
            if (inputState.Has(EntityMovementInputState.PositionChanged))
                entityMovementInput.Position = reader.GetVector2();
            entityMovementInput.Direction2D = reader.Get<DirectionVector2>();
        }
        #endregion

        #region Sync Transform Serialization (3D)
        public static void ServerWriteSyncTransform3D(this IEntityMovement movement, List<EntityMovementForceApplier> movementForceAppliers, NetDataWriter writer)
        {
            if (!movement.Entity.IsServer)
                return;
            writer.PutPackedUInt((uint)movement.MovementState);
            writer.Put((byte)movement.ExtraMovementState);
            writer.PutVector3(movement.Entity.EntityTransform.position);
            writer.PutPackedInt(GetCompressedAngle(movement.Entity.EntityTransform.eulerAngles.y));
            writer.PutList(movementForceAppliers);
        }

        public static void ClientWriteSyncTransform3D(this IEntityMovement movement, NetDataWriter writer)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            writer.PutPackedUInt((uint)movement.MovementState);
            writer.Put((byte)movement.ExtraMovementState);
            writer.PutVector3(movement.Entity.EntityTransform.position);
            writer.PutPackedInt(GetCompressedAngle(movement.Entity.EntityTransform.eulerAngles.y));
        }

        public static void ServerReadSyncTransformMessage3D(this NetDataReader reader, out MovementState movementState, out ExtraMovementState extraMovementState, out Vector3 position, out float yAngle)
        {
            movementState = (MovementState)reader.GetPackedUInt();
            extraMovementState = (ExtraMovementState)reader.GetByte();
            position = reader.GetVector3();
            yAngle = GetDecompressedAngle(reader.GetPackedInt());
        }

        public static void ClientReadSyncTransformMessage3D(this NetDataReader reader, out MovementState movementState, out ExtraMovementState extraMovementState, out Vector3 position, out float yAngle, out List<EntityMovementForceApplier> movementForceAppliers)
        {
            movementState = (MovementState)reader.GetPackedUInt();
            extraMovementState = (ExtraMovementState)reader.GetByte();
            position = reader.GetVector3();
            yAngle = GetDecompressedAngle(reader.GetPackedInt());
            movementForceAppliers = reader.GetList<EntityMovementForceApplier>();
        }
        #endregion

        #region Sync Transform Serialization (2D)
        public static void ServerWriteSyncTransform2D(this IEntityMovement movement, List<EntityMovementForceApplier> movementForceAppliers, NetDataWriter writer)
        {
            if (!movement.Entity.IsServer)
                return;
            writer.PutPackedUInt((uint)movement.MovementState);
            writer.Put((byte)movement.ExtraMovementState);
            writer.PutVector2(movement.Entity.EntityTransform.position);
            writer.Put(movement.Direction2D);
            writer.PutList(movementForceAppliers);
        }

        public static void ClientWriteSyncTransform2D(this IEntityMovement movement, NetDataWriter writer)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            writer.PutPackedUInt((uint)movement.MovementState);
            writer.Put((byte)movement.ExtraMovementState);
            writer.PutVector2(movement.Entity.EntityTransform.position);
            writer.Put(movement.Direction2D);
        }

        public static void ServerReadSyncTransformMessage2D(this NetDataReader reader, out MovementState movementState, out ExtraMovementState extraMovementState, out Vector2 position, out DirectionVector2 direction2D)
        {
            movementState = (MovementState)reader.GetPackedUInt();
            extraMovementState = (ExtraMovementState)reader.GetByte();
            position = reader.GetVector2();
            direction2D = reader.Get<DirectionVector2>();
        }

        public static void ClientReadSyncTransformMessage2D(this NetDataReader reader, out MovementState movementState, out ExtraMovementState extraMovementState, out Vector2 position, out DirectionVector2 direction2D, out List<EntityMovementForceApplier> movementForceAppliers)
        {
            movementState = (MovementState)reader.GetPackedUInt();
            extraMovementState = (ExtraMovementState)reader.GetByte();
            position = reader.GetVector2();
            direction2D = reader.Get<DirectionVector2>();
            movementForceAppliers = reader.GetList<EntityMovementForceApplier>();
        }
        #endregion

        #region Helpers
        public static int GetCompressedAngle(float angle)
        {
            return Mathf.RoundToInt(angle * 1000);
        }

        public static float GetDecompressedAngle(int compressedAngle)
        {
            return (float)compressedAngle * 0.001f;
        }
        #endregion
    }
}
