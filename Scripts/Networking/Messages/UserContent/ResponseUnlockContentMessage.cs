using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ResponseUnlockContentMessage : INetSerializable
    {
        public UITextKeys message;
        public UnlockableContentType type;
        public int dataId;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            type = (UnlockableContentType)reader.GetByte();
            dataId = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
            writer.PutPackedInt(dataId);
        }
    }
}
