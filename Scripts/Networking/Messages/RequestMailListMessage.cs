using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class RequestMailListMessage : INetSerializable
    {
        public bool onlyNewMails;

        public void Deserialize(NetDataReader reader)
        {
            onlyNewMails = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(onlyNewMails);
        }
    }
}
