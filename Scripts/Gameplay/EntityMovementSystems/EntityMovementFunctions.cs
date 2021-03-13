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
        public static void ClientSendKeyMovement3D(this IEntityMovement movement, Vector3 moveDirection, MovementState movementState)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(DeliveryMethod.Unreliable, GameNetworkingConsts.KeyMovement, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                new DirectionVector3(moveDirection).Serialize(writer);
                writer.Put((byte)movementState);
                writer.PutPackedLong(movement.Entity.Manager.ServerUnixTime);
            });
        }

        public static void ClientSendPointClickMovement3D(this IEntityMovement movement, Vector3 position)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(DeliveryMethod.Unreliable, GameNetworkingConsts.PointClickMovement, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutVector3(position);
                writer.PutPackedLong(movement.Entity.Manager.ServerUnixTime);
            });
        }

        public static void ClientSendPointClickMovement3D_2(this IEntityMovement movement, bool isKeyMovement, MovementState movementState, Vector3 position, Quaternion rotation)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(DeliveryMethod.ReliableSequenced, GameNetworkingConsts.PointClickMovement, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.Put(isKeyMovement);
                writer.Put((byte)movementState);
                writer.PutVector3(position);
                writer.PutPackedInt(GetCompressedAngle(rotation.eulerAngles.y));
                writer.PutPackedLong(movement.Entity.Manager.ServerUnixTime);
            });
        }

        public static void ClientSendSetLookRotation3D(this IEntityMovement movement, Quaternion rotation)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(DeliveryMethod.Unreliable, GameNetworkingConsts.SetLookRotation, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutPackedInt(GetCompressedAngle(rotation.eulerAngles.y));
                writer.PutPackedLong(movement.Entity.Manager.ServerUnixTime);
            });
        }

        public static void ServerSendSyncTransform3D(this IEntityMovement movement)
        {
            if (!movement.Entity.IsServer)
                return;
            movement.Entity.ServerSendPacketToSubscribers(DeliveryMethod.Unreliable, GameNetworkingConsts.SyncTransform, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutVector3(movement.Entity.CacheTransform.position);
                writer.PutPackedInt(GetCompressedAngle(movement.Entity.CacheTransform.eulerAngles.y));
                writer.PutPackedLong(movement.Entity.Manager.ServerUnixTime);
            });
        }

        public static void ClientSendSyncTransform3D(this IEntityMovement movement)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(DeliveryMethod.Unreliable, GameNetworkingConsts.SyncTransform, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutVector3(movement.Entity.CacheTransform.position);
                writer.PutPackedInt(GetCompressedAngle(movement.Entity.CacheTransform.eulerAngles.y));
                writer.PutPackedLong(movement.Entity.Manager.ServerUnixTime);
            });
        }

        public static void ServerSendTeleport3D(this IEntityMovement movement, Vector3 position, Quaternion rotation)
        {
            if (!movement.Entity.IsServer)
                return;
            movement.Entity.ServerSendPacketToSubscribers(DeliveryMethod.ReliableUnordered, GameNetworkingConsts.Teleport, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutVector3(position);
                writer.PutPackedInt(GetCompressedAngle(rotation.eulerAngles.y));
                writer.PutPackedLong(movement.Entity.Manager.ServerUnixTime);
            });
        }

        public static void ClientSendStopMove(this IEntityMovement movement)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(DeliveryMethod.ReliableUnordered, GameNetworkingConsts.StopMove, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutPackedLong(movement.Entity.Manager.ServerUnixTime);
            });
        }

        public static void ServerSendJump(this IEntityMovement movement)
        {
            if (!movement.Entity.IsServer)
                return;
            movement.Entity.ServerSendPacketToSubscribers(DeliveryMethod.ReliableUnordered, GameNetworkingConsts.Jump, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutPackedLong(movement.Entity.Manager.ServerUnixTime);
            });
        }

        public static void ClientSendJump(this IEntityMovement movement)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(DeliveryMethod.ReliableUnordered, GameNetworkingConsts.Jump, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutPackedLong(movement.Entity.Manager.ServerUnixTime);
            });
        }

        public static void ReadKeyMovementMessage3D(this NetDataReader reader, out DirectionVector3 inputDirection, out MovementState movementState, out long timestamp)
        {
            inputDirection = new DirectionVector3();
            inputDirection.Deserialize(reader);
            movementState = (MovementState)reader.GetByte();
            timestamp = reader.GetPackedLong();
        }

        public static void ReadPointClickMovementMessage3D(this NetDataReader reader, out Vector3 position, out long timestamp)
        {
            position = reader.GetVector3();
            timestamp = reader.GetPackedLong();
        }

        public static void ReadPointClickMovementMessage3D_2(this NetDataReader reader, out bool isKeyMovement, out MovementState movementState, out Vector3 position, out float yAngle, out long timestamp)
        {
            isKeyMovement = reader.GetBool();
            movementState = (MovementState)reader.GetByte();
            position = reader.GetVector3();
            yAngle = GetDecompressedAngle(reader.GetPackedInt());
            timestamp = reader.GetPackedLong();
        }

        public static void ReadSetLookRotationMessage3D(this NetDataReader reader, out float yAngle, out long timestamp)
        {
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
        public static void ClientSendKeyMovement2D(this IEntityMovement movement, Vector2 moveDirection)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(DeliveryMethod.Unreliable, GameNetworkingConsts.KeyMovement, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                new DirectionVector2(moveDirection).Serialize(writer);
                writer.PutPackedLong(movement.Entity.Manager.ServerUnixTime);
            });
        }

        public static void ClientSendPointClickMovement2D(this IEntityMovement movement, Vector2 position)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(DeliveryMethod.Unreliable, GameNetworkingConsts.PointClickMovement, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutVector2(position);
                writer.PutPackedLong(movement.Entity.Manager.ServerUnixTime);
            });
        }

        public static void ServerSendSyncTransform2D(this IEntityMovement movement)
        {
            if (!movement.Entity.IsServer)
                return;
            movement.Entity.ServerSendPacketToSubscribers(DeliveryMethod.Unreliable, GameNetworkingConsts.SyncTransform, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutVector2(movement.Entity.CacheTransform.position);
                writer.PutPackedLong(movement.Entity.Manager.ServerUnixTime);
            });
        }

        public static void ClientSendSyncTransform2D(this IEntityMovement movement)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(DeliveryMethod.Unreliable, GameNetworkingConsts.SyncTransform, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutVector2(movement.Entity.CacheTransform.position);
                writer.PutPackedLong(movement.Entity.Manager.ServerUnixTime);
            });
        }

        public static void ServerSendTeleport2D(this IEntityMovement movement, Vector2 position)
        {
            if (!movement.Entity.IsServer)
                return;
            movement.Entity.ServerSendPacketToSubscribers(DeliveryMethod.ReliableUnordered, GameNetworkingConsts.Teleport, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutVector2(position);
                writer.PutPackedLong(movement.Entity.Manager.ServerUnixTime);
            });
        }

        public static void ReadKeyMovementMessage2D(this NetDataReader reader, out DirectionVector2 inputDirection, out long timestamp)
        {
            inputDirection = new DirectionVector2();
            inputDirection.Deserialize(reader);
            timestamp = reader.GetPackedLong();
        }

        public static void ReadPointClickMovementMessage2D(this NetDataReader reader, out Vector2 position, out long timestamp)
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
