using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class EntityMovementFunctions
    {
        public const byte MOVEMENT_DATA_CHANNEL = 2;

        #region Generic Functions
        public static bool CanPredictMovement(this IEntityMovement movement)
        {
            return movement.Entity.IsOwnerClient || (movement.Entity.IsServer && movement.Entity.MovementSecure == MovementSecure.ServerAuthoritative);
        }

        public static ExtraMovementState ValidateExtraMovementState(this IEntityMovement movement, MovementState movementState, ExtraMovementState extraMovementState)
        {
            // Movement state can affect extra movement state
            if (movementState.Has(MovementState.IsUnderWater))
            {
                // Extra movement states always none while under water
                extraMovementState = ExtraMovementState.None;
            }
            else
            {
                switch (extraMovementState)
                {
                    case ExtraMovementState.IsSprinting:
                        if (!movement.Entity.CanSprint())
                            extraMovementState = ExtraMovementState.None;
                        else if (!movement.Entity.CanSideSprint && (movementState.Has(MovementState.Left) || movementState.Has(MovementState.Right)))
                            extraMovementState = ExtraMovementState.None;
                        else if (!movement.Entity.CanBackwardSprint && movementState.Has(MovementState.Backward))
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
        public static void ClientSendMovementInput3D(this IEntityMovement movement, InputState inputState, MovementState movementState, ExtraMovementState extraMovementState, Vector3 position, Quaternion rotation)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(MOVEMENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.MovementInput, (writer) =>
            {
                writer.Put((byte)inputState);
                writer.Put((byte)movementState);
                writer.Put((byte)extraMovementState);
                if (inputState.Has(InputState.PositionChanged))
                    writer.PutVector3(position);
                if (inputState.Has(InputState.RotationChanged))
                    writer.PutPackedInt(GetCompressedAngle(rotation.eulerAngles.y));
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ServerSendSyncTransform3D(this IEntityMovement movement)
        {
            if (!movement.Entity.IsServer)
                return;
            movement.Entity.ServerSendPacketToSubscribers(MOVEMENT_DATA_CHANNEL, DeliveryMethod.Unreliable, GameNetworkingConsts.SyncTransform, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.Put((byte)movement.MovementState);
                writer.Put((byte)movement.ExtraMovementState);
                writer.PutVector3(movement.Entity.CacheTransform.position);
                writer.PutPackedInt(GetCompressedAngle(movement.Entity.CacheTransform.eulerAngles.y));
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ClientSendSyncTransform3D(this IEntityMovement movement)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(MOVEMENT_DATA_CHANNEL, DeliveryMethod.Unreliable, GameNetworkingConsts.SyncTransform, (writer) =>
            {
                writer.Put((byte)movement.MovementState);
                writer.Put((byte)movement.ExtraMovementState);
                writer.PutVector3(movement.Entity.CacheTransform.position);
                writer.PutPackedInt(GetCompressedAngle(movement.Entity.CacheTransform.eulerAngles.y));
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ServerSendTeleport3D(this IEntityMovement movement, Vector3 position, Quaternion rotation)
        {
            if (!movement.Entity.IsServer)
                return;
            movement.Entity.ServerSendPacketToSubscribers(MOVEMENT_DATA_CHANNEL, DeliveryMethod.ReliableUnordered, GameNetworkingConsts.Teleport, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutVector3(position);
                writer.PutPackedInt(GetCompressedAngle(rotation.eulerAngles.y));
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ClientSendStopMove(this IEntityMovement movement)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(MOVEMENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.StopMove, (writer) =>
            {
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ServerSendJump(this IEntityMovement movement)
        {
            if (!movement.Entity.IsServer)
                return;
            movement.Entity.ServerSendPacketToSubscribers(MOVEMENT_DATA_CHANNEL, DeliveryMethod.ReliableUnordered, GameNetworkingConsts.Jump, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ClientSendJump(this IEntityMovement movement)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(MOVEMENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Jump, (writer) =>
            {
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ReadMovementInputMessage3D(this NetDataReader reader, out InputState inputState, out MovementState movementState, out ExtraMovementState extraMovementState, out Vector3 position, out float yAngle, out long timestamp)
        {
            inputState = (InputState)reader.GetByte();
            movementState = (MovementState)reader.GetByte();
            extraMovementState = (ExtraMovementState)reader.GetByte();
            position = Vector3.zero;
            if (inputState.Has(InputState.PositionChanged))
                position = reader.GetVector3();
            yAngle = 0f;
            if (inputState.Has(InputState.RotationChanged))
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

        public static void ReadTeleportMessage3D(this NetDataReader reader, out Vector3 position, out float yAngle, out long timestamp)
        {
            position = reader.GetVector3();
            yAngle = GetDecompressedAngle(reader.GetPackedInt());
            timestamp = reader.GetPackedLong();
        }

        public static void ReadStopMoveMessage(this NetDataReader reader, out long timestamp)
        {
            timestamp = reader.GetPackedLong();
        }

        public static void ReadJumpMessage(this NetDataReader reader, out long timestamp)
        {
            timestamp = reader.GetPackedLong();
        }
        #endregion

        #region 2D
        public static void ClientSendMovementInput2D(this IEntityMovement movement, MovementState movementState, ExtraMovementState extraMovementState, Vector2 position)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(MOVEMENT_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.MovementInput, (writer) =>
            {
                writer.Put((byte)movement.MovementState);
                writer.Put((byte)movement.ExtraMovementState);
                writer.PutVector2(position);
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ServerSendSyncTransform2D(this IEntityMovement movement)
        {
            if (!movement.Entity.IsServer)
                return;
            movement.Entity.ServerSendPacketToSubscribers(MOVEMENT_DATA_CHANNEL, DeliveryMethod.Unreliable, GameNetworkingConsts.SyncTransform, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.Put((byte)movement.MovementState);
                writer.Put((byte)movement.ExtraMovementState);
                writer.PutVector2(movement.Entity.CacheTransform.position);
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ClientSendSyncTransform2D(this IEntityMovement movement)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(MOVEMENT_DATA_CHANNEL, DeliveryMethod.Unreliable, GameNetworkingConsts.SyncTransform, (writer) =>
            {
                writer.Put((byte)movement.MovementState);
                writer.Put((byte)movement.ExtraMovementState);
                writer.PutVector2(movement.Entity.CacheTransform.position);
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ServerSendTeleport2D(this IEntityMovement movement, Vector2 position)
        {
            if (!movement.Entity.IsServer)
                return;
            movement.Entity.ServerSendPacketToSubscribers(MOVEMENT_DATA_CHANNEL, DeliveryMethod.ReliableUnordered, GameNetworkingConsts.Teleport, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutVector2(position);
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ReadMovementInputMessage2D(this NetDataReader reader, out MovementState movementState, out ExtraMovementState extraMovementState, out Vector2 position, out long timestamp)
        {
            movementState = (MovementState)reader.GetByte();
            extraMovementState = (ExtraMovementState)reader.GetByte();
            position = reader.GetVector2();
            timestamp = reader.GetPackedLong();
        }

        public static void ReadSyncTransformMessage2D(this NetDataReader reader, out MovementState movementState, out ExtraMovementState extraMovementState, out Vector2 position, out long timestamp)
        {
            movementState = (MovementState)reader.GetByte();
            extraMovementState = (ExtraMovementState)reader.GetByte();
            position = reader.GetVector2();
            timestamp = reader.GetPackedLong();
        }

        public static void ReadTeleportMessage2D(this NetDataReader reader, out Vector2 position, out long timestamp)
        {
            position = reader.GetVector2();
            timestamp = reader.GetPackedLong();
        }
        #endregion

        #region Helpers
        public static int GetCompressedAngle(float angle)
        {
            return Mathf.RoundToInt(angle * 1000);
        }

        public static float GetDecompressedAngle(float compressedAngle)
        {
            return compressedAngle * 0.001f;
        }
        #endregion
    }
}
