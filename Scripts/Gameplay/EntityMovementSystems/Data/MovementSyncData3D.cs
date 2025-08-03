using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG
{
    public struct MovementSyncData3D : INetSerializable
    {
        public uint Tick;
        public Vector3 Position;
        public float Rotation;
        public MovementState MovementState;
        public ExtraMovementState ExtraMovementState;

        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetPackedUInt();
            Position = new Vector3(
                reader.GetFloat(),
                reader.GetFloat(),
                reader.GetFloat());
            Rotation = reader.GetFloat();
            MovementState = (MovementState)reader.GetByte();
            ExtraMovementState = (ExtraMovementState)reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUInt(Tick);
            writer.Put(Position.x);
            writer.Put(Position.y);
            writer.Put(Position.z);
            writer.Put(Rotation);
            writer.Put((byte)MovementState);
            writer.Put((byte)ExtraMovementState);
        }
    }
}