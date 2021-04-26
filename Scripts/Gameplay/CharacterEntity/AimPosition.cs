using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG
{
    public struct AimPosition : INetSerializable
    {
        public AimPositionType type;
        public Vector3 position;
        public DirectionVector3 direction;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
            if (type != AimPositionType.None)
                writer.PutVector3(position);
            if (type == AimPositionType.Direction)
                direction.Serialize(writer);
        }

        public void Deserialize(NetDataReader reader)
        {
            type = (AimPositionType)reader.GetByte();
            if (type != AimPositionType.None)
                position = reader.GetVector3();
            if (type == AimPositionType.Direction)
                direction.Deserialize(reader);
        }

        public static AimPosition CreatePosition(Vector3 position)
        {
            return new AimPosition()
            {
                type = AimPositionType.Position,
                position = position,
            };
        }

        public static AimPosition CreateDirection(Vector3 position, Vector3 direction)
        {
            return new AimPosition()
            {
                type = AimPositionType.Direction,
                position = position,
                direction = direction,
            };
        }
    }
}
