using LiteNetLib.Utils;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public struct MovementSyncData2D : INetSerializable
    {
        public uint Tick;
        public Vector2 Position;
        public HalfPrecision Rotation;
        public MovementState MovementState;
        public ExtraMovementState ExtraMovementState;

        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetPackedUInt();
            Position = new Vector2(
                reader.GetFloat(),
                reader.GetFloat());
            Rotation = reader.Get<HalfPrecision>();
            MovementState = (MovementState)reader.GetByte();
            ExtraMovementState = (ExtraMovementState)reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUInt(Tick);
            writer.Put(Position.x);
            writer.Put(Position.y);
            writer.Put(Rotation);
            writer.Put((byte)MovementState);
            writer.Put((byte)ExtraMovementState);
        }
    }
}