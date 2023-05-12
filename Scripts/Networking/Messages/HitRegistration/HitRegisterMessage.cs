using LiteNetLib.Utils;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct HitRegisterMessage : INetSerializable
    {
        public int RandomSeed { get; set; }
        public List<HitData> Hits { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(RandomSeed);
            writer.PutList(Hits);
        }

        public void Deserialize(NetDataReader reader)
        {
            RandomSeed = reader.GetPackedInt();
            Hits = reader.GetList<HitData>();
        }
    }
}
