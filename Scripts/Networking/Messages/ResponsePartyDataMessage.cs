using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class ResponsePartyDataMessage : ResponseSocialGroupDataMessage
    {
        public bool shareExp;
        public bool shareItem;

        public override void DeserializeData(NetDataReader reader)
        {
            shareExp = reader.GetBool();
            shareItem = reader.GetBool();
            base.DeserializeData(reader);
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(shareExp);
            writer.Put(shareItem);
            base.SerializeData(writer);
        }
    }
}
