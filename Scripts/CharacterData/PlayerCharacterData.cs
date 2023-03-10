using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public partial class PlayerCharacterData : INetSerializable
    {
        public void Deserialize(NetDataReader reader)
        {
            this.DeserializeCharacterData(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            this.SerializeCharacterData(writer);
        }
    }
}
