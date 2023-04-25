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
            if (movementState.Has(MovementState.IsUnderWater))
            {
                // Extra movement states always none while under water
                extraMovementState = ExtraMovementState.None;
            }
            else if (!movement.Entity.CanMove())
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

        #region 3D
        public static void ClientWriteMovementInput3D(this IEntityMovement movement, NetDataWriter writer, EntityMovementInputState inputState, MovementState movementState, ExtraMovementState extraMovementState, Vector3 position, Quaternion rotation)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            writer.Put((byte)inputState);
            writer.Put((byte)movementState);
            if (!inputState.Has(EntityMovementInputState.IsStopped))
                writer.Put((byte)extraMovementState);
            if (inputState.Has(EntityMovementInputState.PositionChanged))
                writer.PutVector3(position);
            if (inputState.Has(EntityMovementInputState.RotationChanged))
                writer.PutPackedInt(GetCompressedAngle(rotation.eulerAngles.y));
            writer.PutPackedLong(BaseGameNetworkManager.Singleton.ServerTimestamp);
        }

        public static void ServerWriteSyncTransform3D(this IEntityMovement movement, NetDataWriter writer)
        {
            if (!movement.Entity.IsServer)
                return;
            writer.Put((byte)movement.MovementState);
            writer.Put((byte)movement.ExtraMovementState);
            writer.PutVector3(movement.Entity.EntityTransform.position);
            writer.PutPackedInt(GetCompressedAngle(movement.Entity.EntityTransform.eulerAngles.y));
            writer.PutPackedLong(BaseGameNetworkManager.Singleton.ServerTimestamp);
        }

        public static void ClientWriteSyncTransform3D(this IEntityMovement movement, NetDataWriter writer)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            writer.Put((byte)movement.MovementState);
            writer.Put((byte)movement.ExtraMovementState);
            writer.PutVector3(movement.Entity.EntityTransform.position);
            writer.PutPackedInt(GetCompressedAngle(movement.Entity.EntityTransform.eulerAngles.y));
            writer.PutPackedLong(BaseGameNetworkManager.Singleton.ServerTimestamp);
        }

        public static void ReadMovementInputMessage3D(this NetDataReader reader, out EntityMovementInputState inputState, out MovementState movementState, out ExtraMovementState extraMovementState, out Vector3 position, out float yAngle, out long timestamp)
        {
            inputState = (EntityMovementInputState)reader.GetByte();
            movementState = (MovementState)reader.GetByte();
            if (!inputState.Has(EntityMovementInputState.IsStopped))
                extraMovementState = (ExtraMovementState)reader.GetByte();
            else
                extraMovementState = ExtraMovementState.None;
            position = Vector3.zero;
            if (inputState.Has(EntityMovementInputState.PositionChanged))
                position = reader.GetVector3();
            yAngle = 0f;
            if (inputState.Has(EntityMovementInputState.RotationChanged))
                yAngle = GetDecompressedAngle(reader.GetPackedInt());
            timestamp = reader.GetPackedLong();
        }

        public static void ReadSyncTransformMessage3D(this NetDataReader reader, out MovementState movementState, out ExtraMovementState extraMovementState, out Vector3 position, out float yAngle, out long timestamp)
        {
            movementState = (MovementState)reader.GetByte();
            extraMovementState = (ExtraMovementState)reader.GetByte();
            position = reader.GetVector3();
            yAngle = GetDecompressedAngle(reader.GetPackedInt());
            timestamp = reader.GetPackedLong();
        }
        #endregion

        #region 2D
        public static void ClientWriteMovementInput2D(this IEntityMovement movement, NetDataWriter writer, EntityMovementInputState inputState, MovementState movementState, ExtraMovementState extraMovementState, Vector2 position, DirectionVector2 direction2D)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            writer.Put((byte)inputState);
            writer.Put((byte)movementState);
            if (!inputState.Has(EntityMovementInputState.IsStopped))
                writer.Put((byte)extraMovementState);
            if (inputState.Has(EntityMovementInputState.PositionChanged))
                writer.PutVector2(position);
            writer.Put(direction2D);
            writer.PutPackedLong(BaseGameNetworkManager.Singleton.ServerTimestamp);
        }

        public static void ServerWriteSyncTransform2D(this IEntityMovement movement, NetDataWriter writer)
        {
            if (!movement.Entity.IsServer)
                return;
            writer.Put((byte)movement.MovementState);
            writer.Put((byte)movement.ExtraMovementState);
            writer.PutVector2(movement.Entity.EntityTransform.position);
            writer.Put(movement.Direction2D);
            writer.PutPackedLong(BaseGameNetworkManager.Singleton.ServerTimestamp);
        }

        public static void ClientWriteSyncTransform2D(this IEntityMovement movement, NetDataWriter writer)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            writer.Put((byte)movement.MovementState);
            writer.Put((byte)movement.ExtraMovementState);
            writer.PutVector2(movement.Entity.EntityTransform.position);
            writer.Put(movement.Direction2D);
            writer.PutPackedLong(BaseGameNetworkManager.Singleton.ServerTimestamp);
        }

        public static void ReadMovementInputMessage2D(this NetDataReader reader, out EntityMovementInputState inputState, out MovementState movementState, out ExtraMovementState extraMovementState, out Vector2 position, out DirectionVector2 direction2D, out long timestamp)
        {
            inputState = (EntityMovementInputState)reader.GetByte();
            movementState = (MovementState)reader.GetByte();
            if (!inputState.Has(EntityMovementInputState.IsStopped))
                extraMovementState = (ExtraMovementState)reader.GetByte();
            else
                extraMovementState = ExtraMovementState.None;
            position = Vector3.zero;
            if (inputState.Has(EntityMovementInputState.PositionChanged))
                position = reader.GetVector2();
            direction2D = reader.Get<DirectionVector2>();
            timestamp = reader.GetPackedLong();
        }

        public static void ReadSyncTransformMessage2D(this NetDataReader reader, out MovementState movementState, out ExtraMovementState extraMovementState, out Vector2 position, out DirectionVector2 direction2D, out long timestamp)
        {
            movementState = (MovementState)reader.GetByte();
            extraMovementState = (ExtraMovementState)reader.GetByte();
            position = reader.GetVector2();
            direction2D = reader.Get<DirectionVector2>();
            timestamp = reader.GetPackedLong();
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
