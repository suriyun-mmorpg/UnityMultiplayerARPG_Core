using LiteNetLib.Utils;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct HitRegisterMessage : INetSerializable
    {
        public List<HitRegisterData> Hits { get; set; }
        public int RandomSeed { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutList(Hits);
            writer.PutPackedInt(RandomSeed);
        }

        public void Deserialize(NetDataReader reader)
        {
            Hits = reader.GetList<HitRegisterData>();
            RandomSeed = reader.GetPackedInt();
        }
    }
}
