using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class EntityMovementFunctions
    {
        public static void ClientSendKeyMovement(this IEntityMovement movement, Vector3 moveDirection, MovementState movementState)
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

        public static void ClientSendPointClickMovement(this IEntityMovement movement, Vector3 position)
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

        public static void ClientSendSetLookRotation(this IEntityMovement movement, Quaternion rotation)
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

        public static void ServerSendSyncTransform3D(this IEntityMovement movement, bool jump)
        {
            if (!movement.Entity.IsServer)
                return;
            movement.Entity.ServerSendPacketToSubscribers(DeliveryMethod.Unreliable, GameNetworkingConsts.SyncTransform, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutVector3(movement.Entity.CacheTransform.position);
                writer.PutPackedInt(GetCompressedAngle(movement.Entity.CacheTransform.eulerAngles.y));
                writer.Put(jump);
                writer.PutPackedLong(movement.Entity.Manager.ServerUnixTime);
            });
        }

        public static void ClientSendSyncTransform3D(this IEntityMovement movement, bool jump)
        {
            if (!movement.Entity.IsOwnerClient)
                return;
            movement.Entity.ClientSendPacket(DeliveryMethod.Unreliable, GameNetworkingConsts.SyncTransform, (writer) =>
            {
                writer.PutPackedUInt(movement.Entity.ObjectId);
                writer.PutVector3(movement.Entity.CacheTransform.position);
                writer.PutPackedInt(GetCompressedAngle(movement.Entity.CacheTransform.eulerAngles.y));
                writer.Put(jump);
                writer.PutPackedLong(movement.Entity.Manager.ServerUnixTime);
            });
        }

        public static void ServerSendTeleport3D(this IEntityMovement movement, Vector3 position, Quaternion rotation)
        {
            if (!movement.Entity.IsServer)
                return;
            movement.Entity.ServerSendPacketToSubscribers(DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Teleport, (writer) =>
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
            movement.Entity.ClientSendPacket(DeliveryMethod.ReliableOrdered, GameNetworkingConsts.StopMove, (writer) =>
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

        public static void ReadSetLookRotationMessage3D(this NetDataReader reader, out float yAngle, out long timestamp)
        {
            yAngle = GetDecompressedAngle(reader.GetPackedInt());
            timestamp = reader.GetPackedLong();
        }

        public static void ReadSyncTransformMessage3D(this NetDataReader reader, out Vector3 position, out float yAngle, out bool jump, out long timestamp)
        {
            position = reader.GetVector3();
            yAngle = GetDecompressedAngle(reader.GetPackedInt());
            jump = reader.GetBool();
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

        public static int GetCompressedAngle(float angle)
        {
            return Mathf.RoundToInt(angle * 1000);
        }

        public static float GetDecompressedAngle(float compressedAngle)
        {
            return compressedAngle * 0.001f;
        }
    }
}
