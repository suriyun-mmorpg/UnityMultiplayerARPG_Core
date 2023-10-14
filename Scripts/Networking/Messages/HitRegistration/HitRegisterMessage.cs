using LiteNetLib.Utils;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct HitRegisterMessage : INetSerializable
    {
        public List<HitRegisterData> Hits { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutList(Hits);
        }

        public void Deserialize(NetDataReader reader)
        {
            Hits = reader.GetList<HitRegisterData>();
        }
    }
}
