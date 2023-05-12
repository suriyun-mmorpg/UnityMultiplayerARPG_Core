using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct HitData : INetSerializable
    {
        public byte TriggerIndex { get; set; }
        public byte SpreadIndex { get; set; }
        public uint ObjectId { get; set; }
        public byte HitBoxIndex { get; set; }
        public Vector3 HitPoint { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(TriggerIndex);
            writer.Put(SpreadIndex);
            writer.PutPackedUInt(ObjectId);
            writer.Put(HitBoxIndex);
            writer.PutVector3(HitPoint);
        }

        public void Deserialize(NetDataReader reader)
        {
            TriggerIndex = reader.GetByte();
            SpreadIndex = reader.GetByte();
            ObjectId = reader.GetPackedUInt();
            HitBoxIndex = reader.GetByte();
            HitPoint = reader.GetVector3();
        }
    }
}
