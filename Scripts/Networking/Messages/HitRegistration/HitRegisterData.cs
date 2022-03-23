using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct HitRegisterData : INetSerializable
    {
        public AimPosition AimPosition { get; set; }
        public uint HitObjectId { get; set; }
        public byte HitBoxIndex { get; set; }
        public Vector3 HitPoint { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            AimPosition.Serialize(writer);
            writer.PutPackedUInt(HitObjectId);
            writer.Put(HitBoxIndex);
            writer.PutVector3(HitPoint);
        }

        public void Deserialize(NetDataReader reader)
        {
            AimPosition.Deserialize(reader);
            HitObjectId = reader.GetPackedUInt();
            HitBoxIndex = reader.GetByte();
            HitPoint = reader.GetVector3();
        }
    }
}
