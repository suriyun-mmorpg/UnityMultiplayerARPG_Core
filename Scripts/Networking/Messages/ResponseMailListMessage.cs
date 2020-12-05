using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class ResponseMailListMessage : INetSerializable
    {
        public bool onlyNewMails;
        public MailListEntry[] mails;

        public void Deserialize(NetDataReader reader)
        {
            onlyNewMails = reader.GetBool();
            mails = reader.GetArray<MailListEntry>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(onlyNewMails);
            writer.PutArray(mails);
        }
    }
}
