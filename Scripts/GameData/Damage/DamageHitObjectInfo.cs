using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct DamageHitObjectInfo : INetSerializable
    {
        public uint ObjectId { get; set; }
        public int HitBoxIndex { get; set; }

        public override bool Equals(object obj)
        {
            return ObjectId == ((DamageHitObjectInfo)obj).ObjectId;
        }

        public override int GetHashCode()
        {
            return ObjectId.GetHashCode();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUInt(ObjectId);
            writer.PutPackedInt(HitBoxIndex);
        }

        public void Deserialize(NetDataReader reader)
        {
            ObjectId = reader.GetPackedUInt();
            HitBoxIndex = reader.GetPackedInt();
        }
    }
}
