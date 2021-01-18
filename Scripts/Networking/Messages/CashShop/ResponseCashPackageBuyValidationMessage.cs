using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct ResponseCashPackageBuyValidationMessage : INetSerializable
    {
        public UITextKeys error;
        public int dataId;
        public int cash;

        public void Deserialize(NetDataReader reader)
        {
            error = (UITextKeys)reader.GetPackedUShort();
            if (error == UITextKeys.NONE)
            {
                dataId = reader.GetInt();
                cash = reader.GetInt();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)error);
            if (error == UITextKeys.NONE)
            {
                writer.Put(dataId);
                writer.Put(cash);
            }
        }
    }
}
