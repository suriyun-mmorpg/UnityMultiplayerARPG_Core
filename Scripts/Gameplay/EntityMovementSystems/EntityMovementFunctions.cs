using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class EntityMovementFunctions
    {
        #region Generic Functions
        public static bool CanPredictMovement(this IEntityMovement movement)
        {
            return movement.Entity.IsOwnerClient || (movement.Entity.IsServer && movement.Entity.MovementSecure == MovementSecure.ServerAuthoritative);
        }
        #endregion

        #region 3D
        public static void ClientSendMovementInput3D(this IEntityMovement movement, InputState inputState, MovementState movementState, Vector3 position, Quaternion rotation)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(0, DeliveryMethod.ReliableSequenced, GameNetworkingConsts.MovementInput, (writer) =>
            {
                writer.PutPackedInt((int)inputState);
                writer.PutPackedInt((int)movementState);
                if (inputState.HasFlag(InputState.PositionChanged))
                    writer.PutVector3(position);
                if (inputState.HasFlag(InputState.RotationChanged))
                    writer.PutPackedInt(GetCompressedAngle(rotation.eulerAngles.y));
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ServerSendSyncTransform3D(this IEntityMovement movement)
        {
            if (!movement.Entity.IsServer)
                return;
            movement.Entity.ServerSendPacketToSubscribers(0, DeliveryMethod.Unreliable, GameNetworkingConsts.SyncTransform, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutVector3(movement.Entity.CacheTransform.position);
                writer.PutPackedInt(GetCompressedAngle(movement.Entity.CacheTransform.eulerAngles.y));
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ClientSendSyncTransform3D(this IEntityMovement movement)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(0, DeliveryMethod.Unreliable, GameNetworkingConsts.SyncTransform, (writer) =>
            {
                writer.PutVector3(movement.Entity.CacheTransform.position);
                writer.PutPackedInt(GetCompressedAngle(movement.Entity.CacheTransform.eulerAngles.y));
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ServerSendTeleport3D(this IEntityMovement movement, Vector3 position, Quaternion rotation)
        {
            if (!movement.Entity.IsServer)
                return;
            movement.Entity.ServerSendPacketToSubscribers(0, DeliveryMethod.ReliableUnordered, GameNetworkingConsts.Teleport, (writer) =>
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
            movement.Entity.ClientSendPacket(0, DeliveryMethod.ReliableUnordered, GameNetworkingConsts.StopMove, (writer) =>
            {
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ServerSendJump(this IEntityMovement movement)
        {
            if (!movement.Entity.IsServer)
                return;
            movement.Entity.ServerSendPacketToSubscribers(0, DeliveryMethod.ReliableUnordered, GameNetworkingConsts.Jump, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ClientSendJump(this IEntityMovement movement)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(0, DeliveryMethod.ReliableUnordered, GameNetworkingConsts.Jump, (writer) =>
            {
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ReadMovementInputMessage3D(this NetDataReader reader, out InputState inputState, out MovementState movementState, out Vector3 position, out float yAngle, out long timestamp)
        {
            inputState = (InputState)reader.GetPackedInt();
            movementState = (MovementState)reader.GetPackedInt();
            position = Vector3.zero;
            if (inputState.HasFlag(InputState.PositionChanged))
                position = reader.GetVector3();
            yAngle = 0f;
            if (inputState.HasFlag(InputState.RotationChanged))
                yAngle = GetDecompressedAngle(reader.GetPackedInt());
            timestamp = reader.GetPackedLong();
        }

        public static void ReadSyncTransformMessage3D(this NetDataReader reader, out Vector3 position, out float yAngle, out long timestamp)
        {
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
        public static void ClientSendMovementInput2D(this IEntityMovement movement, Vector2 position)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(0, DeliveryMethod.Unreliable, GameNetworkingConsts.MovementInput, (writer) =>
            {
                writer.PutVector2(position);
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ServerSendSyncTransform2D(this IEntityMovement movement)
        {
            if (!movement.Entity.IsServer)
                return;
            movement.Entity.ServerSendPacketToSubscribers(0, DeliveryMethod.Unreliable, GameNetworkingConsts.SyncTransform, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutVector2(movement.Entity.CacheTransform.position);
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ClientSendSyncTransform2D(this IEntityMovement movement)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(0, DeliveryMethod.Unreliable, GameNetworkingConsts.SyncTransform, (writer) =>
            {
                writer.PutVector2(movement.Entity.CacheTransform.position);
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ServerSendTeleport2D(this IEntityMovement movement, Vector2 position)
        {
            if (!movement.Entity.IsServer)
                return;
            movement.Entity.ServerSendPacketToSubscribers(0, DeliveryMethod.ReliableUnordered, GameNetworkingConsts.Teleport, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutVector2(position);
                writer.PutPackedLong(movement.Entity.Manager.ServerTimestamp);
            });
        }

        public static void ReadMovementInputMessage2D(this NetDataReader reader, out Vector2 position, out long timestamp)
        {
            position = reader.GetVector2();
            timestamp = reader.GetPackedLong();
        }

        public static void ReadSyncTransformMessage2D(this NetDataReader reader, out Vector2 position, out long timestamp)
        {
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
