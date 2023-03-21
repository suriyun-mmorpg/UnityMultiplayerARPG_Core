using UnityEngine;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public partial struct Vec3 : INetSerializable
    {
        public static implicit operator Vec3(Vector3 value) { return new Vec3(value); }
        public static implicit operator Vector3(Vec3 value) { return new Vector3(value.x, value.y, value.z); }

        public Vec3(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(x);
            writer.Put(y);
            writer.Put(z);
        }

        public void Deserialize(NetDataReader reader)
        {
            x = reader.GetFloat();
            y = reader.GetFloat();
            z = reader.GetFloat();
        }
    }
}
