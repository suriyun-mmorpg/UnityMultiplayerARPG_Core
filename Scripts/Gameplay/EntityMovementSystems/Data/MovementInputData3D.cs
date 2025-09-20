using LiteNetLib.Utils;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public struct MovementInputData3D : INetSerializable
    {
        public uint Tick;
        public bool IsStopped;
        public bool IsPointClick;
        public Vector3 Position;
        public MovementState MovementState;
        public ExtraMovementState ExtraMovementState;
        public DirectionVector3 MoveDirection;
        public DirectionVector3 LookDirection;

        public void Deserialize(NetDataReader reader)
        {
            Tick = reader.GetPackedUInt();
            IsStopped = reader.GetBool();
            if (IsStopped)
                return;
            IsPointClick = reader.GetBool();
            if (IsPointClick)
            {
                Position = new Vector3(
                    reader.GetFloat(),
                    reader.GetFloat(),
                    reader.GetFloat());
                MovementState = (MovementState)reader.GetByte();
                ExtraMovementState = (ExtraMovementState)reader.GetByte();
            }
            else
            {
                MovementState = (MovementState)reader.GetByte();
                ExtraMovementState = (ExtraMovementState)reader.GetByte();
                MoveDirection = reader.Get<DirectionVector3>();
                LookDirection = reader.Get<DirectionVector3>();
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
                writer.Put(Position.z);
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