using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct HitData : INetSerializable
    {
        public uint HitObjectId { get; set; }
        public byte HitBoxIndex { get; set; }
        public Vector3 HitPoint { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUInt(HitObjectId);
            writer.Put(HitBoxIndex);
            writer.PutVector3(HitPoint);
        }

        public void Deserialize(NetDataReader reader)
        {
            HitObjectId = reader.GetPackedUInt();
            HitBoxIndex = reader.GetByte();
            HitPoint = reader.GetVector3();
        }
    }
}
