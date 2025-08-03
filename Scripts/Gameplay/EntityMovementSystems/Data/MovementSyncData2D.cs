using LiteNetLib.Utils;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public struct MovementSyncData2D : INetSerializable
    {
        public uint Tick;
        public Vector2 Position;
        public MovementState MovementState;
        public ExtraMovementState ExtraMovementState;
        public DirectionVector2 Direction2D;

        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetPackedUInt();
            Position = new Vector2(
                reader.GetFloat(),
                reader.GetFloat());
            MovementState = (MovementState)reader.GetByte();
            ExtraMovementState = (ExtraMovementState)reader.GetByte();
            Direction2D = reader.Get<DirectionVector2>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUInt(Tick);
            writer.Put(Position.x);
            writer.Put(Position.y);
            writer.Put((byte)MovementState);
            writer.Put((byte)ExtraMovementState);
            writer.Put(Direction2D);
        }
    }
}