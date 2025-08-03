using LiteNetLib.Utils;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public struct MovementInputData2D : INetSerializable
    {
        public uint Tick;
        public bool IsStopped;
        public bool IsPointClick;
        public Vector2 Position;
        public MovementState MovementState;
        public ExtraMovementState ExtraMovementState;
        public DirectionVector2 MoveDirection;
        public DirectionVector2 LookDirection;

        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetPackedUInt();
            IsStopped = reader.GetBool();
            if (IsStopped)
                return;
            IsPointClick = reader.GetBool();
            if (IsPointClick)
            {
                Position = new Vector2(
                    reader.GetFloat(),
                    reader.GetFloat());
                MovementState = (MovementState)reader.GetByte();
                ExtraMovementState = (ExtraMovementState)reader.GetByte();
            }
            else
            {
                MovementState = (MovementState)reader.GetByte();
                ExtraMovementState = (ExtraMovementState)reader.GetByte();
                MoveDirection = reader.Get<DirectionVector2>();
                LookDirection = reader.Get<DirectionVector2>();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUInt(Tick);
            writer.Put(IsStopped);
            if (IsStopped)
                return;
            writer.Put(IsPointClick);
            if (IsPointClick)
            {
                writer.Put(Position.x);
                writer.Put(Position.y);
                writer.Put((byte)MovementState);
                writer.Put((byte)ExtraMovementState);
            }
            else
            {
                writer.Put((byte)MovementState);
                writer.Put((byte)ExtraMovementState);
                writer.Put(MoveDirection);
                writer.Put(LookDirection);
            }
        }
    }
}