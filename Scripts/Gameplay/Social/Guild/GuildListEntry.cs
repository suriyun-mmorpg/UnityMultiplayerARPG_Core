using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct GuildListEntry : INetSerializable
    {
        public int Id { get; set; }
        public string GuildName { get; set; }
        public short Level { get; set; }
        public string GuildMessage { get; set; }
        public int Score { get; set; }
        public int OptionId1 { get; set; }
        public int OptionId2 { get; set; }
        public int OptionId3 { get; set; }
        public int OptionId4 { get; set; }
        public int OptionId5 { get; set; }
        public bool AutoAcceptRequests { get; set; }
        public int Rank { get; set; }
        public int CurrentMembers { get; set; }
        public int MaxMembers { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(Id);
            writer.Put(GuildName);
            writer.PutPackedShort(Level);
            writer.Put(GuildMessage);
            writer.PutPackedInt(Score);
            writer.PutPackedInt(OptionId1);
            writer.PutPackedInt(OptionId2);
            writer.PutPackedInt(OptionId3);
            writer.PutPackedInt(OptionId4);
            writer.PutPackedInt(OptionId5);
            writer.Put(AutoAcceptRequests);
            writer.PutPackedInt(Rank);
            writer.PutPackedInt(CurrentMembers);
            writer.PutPackedInt(MaxMembers);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetPackedInt();
            GuildName = reader.GetString();
            Level = reader.GetPackedShort();
            GuildMessage = reader.GetString();
            Score = reader.GetPackedInt();
            OptionId1 = reader.GetPackedInt();
            OptionId2 = reader.GetPackedInt();
            OptionId3 = reader.GetPackedInt();
            OptionId4 = reader.GetPackedInt();
            OptionId5 = reader.GetPackedInt();
            AutoAcceptRequests = reader.GetBool();
            Rank = reader.GetPackedInt();
            CurrentMembers = reader.GetPackedInt();
            MaxMembers = reader.GetPackedInt();
        }
    }
}
