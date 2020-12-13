using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestChangePartySettingMessage : INetSerializable
    {
        public string characterId;
        public bool shareExp;
        public bool shareItem;

        public void Deserialize(NetDataReader reader)
        {
            characterId = reader.GetString();
            shareExp = reader.GetBool();
            shareItem = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(characterId);
            writer.Put(shareExp);
            writer.Put(shareItem);
        }
    }
}