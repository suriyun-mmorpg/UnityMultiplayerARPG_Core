using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct UnlockableContent : INetSerializable
    {
        public UnlockableContentType type;
        public int dataId;
        public int shardAmount;
        public bool unlocked;

        public void Deserialize(NetDataReader reader)
        {
            type = (UnlockableContentType)reader.GetByte();
            dataId = reader.GetPackedInt();
            shardAmount = reader.GetPackedInt();
            unlocked = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
            writer.PutPackedInt(dataId);
            writer.PutPackedInt(shardAmount);
            writer.Put(unlocked);
        }
    }
}
