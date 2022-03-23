using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct HitRegisterData : INetSerializable
    {
        public bool IsHit { get; set; }
        public Vector3 Origin { get; set; }
        public DirectionVector3 Direction { get; set; }
        public uint HitObjectId { get; set; }
        public byte HitBoxIndex { get; set; }
        public Vector3 HitPoint { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(IsHit);
            if (IsHit)
            {
                writer.PutVector3(Origin);
                Direction.Serialize(writer);
                writer.PutPackedUInt(HitObjectId);
                writer.Put(HitBoxIndex);
                writer.PutVector3(HitPoint);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            IsHit = reader.GetBool();
            if (IsHit)
            {
                Origin = reader.GetVector3();
                Direction.Deserialize(reader);
                HitObjectId = reader.GetPackedUInt();
                HitBoxIndex = reader.GetByte();
                HitPoint = reader.GetVector3();
            }
        }
    }
}
