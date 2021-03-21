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

        public static AimPosition Create(Vector3? vector3)
        {
            AimPosition aimPosition = new AimPosition();
            aimPosition.hasValue = vector3.HasValue;
            if (aimPosition.hasValue)
            {
                aimPosition.value = vector3.Value;
            }
            return aimPosition;
        }
    }
}
