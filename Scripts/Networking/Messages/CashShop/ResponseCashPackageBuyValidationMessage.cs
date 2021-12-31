using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseCashPackageBuyValidationMessage : INetSerializable
    {
        public UITextKeys message;
        public int dataId;
        public int cash;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            if (message == UITextKeys.NONE)
            {
                dataId = reader.GetPackedInt();
                cash = reader.GetPackedInt();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            if (message == UITextKeys.NONE)
            {
                writer.PutPackedInt(dataId);
                writer.PutPackedInt(cash);
            }
        }
    }
}
