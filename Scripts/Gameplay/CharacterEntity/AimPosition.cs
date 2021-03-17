using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG
{
    public struct AimPosition : INetSerializable
    {
        public bool hasValue;
        public Vector3 value;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(hasValue);
            if (hasValue)
                writer.PutVector3(value);
        }

        public void Deserialize(NetDataReader reader)
        {
            hasValue = reader.GetBool();
            if (hasValue)
                value = reader.GetVector3();
        }
    }
}
