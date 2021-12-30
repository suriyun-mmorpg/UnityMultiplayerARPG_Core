using LiteNetLib.Utils;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public struct ResponseOpenGachaMessage : INetSerializable
    {
        public UITextKeys message;
        public List<ItemAmount> rewardItems;

        public void Deserialize(NetDataReader reader)
        {
            message = (UITextKeys)reader.GetPackedUShort();
            rewardItems = reader.GetList<ItemAmount>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort((ushort)message);
            writer.PutList(rewardItems);
        }
    }
}
