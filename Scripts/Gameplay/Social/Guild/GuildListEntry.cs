using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct GuildListEntry : INetSerializable
    {
        public int Id { get; set; }
        public string GuildName { get; set; }
        public short Level { get; set; }
        public GuildListFieldOptions FieldOptions { get; set; }
        public string GuildMessage { get; set; }
        public string GuildMessage2 { get; set; }
        public int Score { get; set; }
        public string Options { get; set; }
        public bool AutoAcceptRequests { get; set; }
        public int Rank { get; set; }
        public int CurrentMembers { get; set; }
        public int MaxMembers { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(Id);
            writer.Put(GuildName);
            writer.PutPackedShort(Level);
            writer.PutPackedInt((int)FieldOptions);
            if (FieldOptions.HasFlag(GuildListFieldOptions.All) || FieldOptions.HasFlag(GuildListFieldOptions.GuildMessage))
                writer.Put(GuildMessage);
            if (FieldOptions.HasFlag(GuildListFieldOptions.All) || FieldOptions.HasFlag(GuildListFieldOptions.GuildMessage2))
                writer.Put(GuildMessage2);
            if (FieldOptions.HasFlag(GuildListFieldOptions.All) || FieldOptions.HasFlag(GuildListFieldOptions.Score))
                writer.PutPackedInt(Score);
            if (FieldOptions.HasFlag(GuildListFieldOptions.All) || FieldOptions.HasFlag(GuildListFieldOptions.Options))
                writer.Put(Options);
            if (FieldOptions.HasFlag(GuildListFieldOptions.All) || FieldOptions.HasFlag(GuildListFieldOptions.AutoAcceptRequests))
                writer.Put(AutoAcceptRequests);
            if (FieldOptions.HasFlag(GuildListFieldOptions.All) || FieldOptions.HasFlag(GuildListFieldOptions.Rank))
                writer.PutPackedInt(Rank);
            if (FieldOptions.HasFlag(GuildListFieldOptions.All) || FieldOptions.HasFlag(GuildListFieldOptions.CurrentMembers))
                writer.PutPackedInt(CurrentMembers);
            if (FieldOptions.HasFlag(GuildListFieldOptions.All) || FieldOptions.HasFlag(GuildListFieldOptions.MaxMembers))
                writer.PutPackedInt(MaxMembers);
        }

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetPackedInt();
            GuildName = reader.GetString();
            Level = reader.GetPackedShort();
            FieldOptions = (GuildListFieldOptions)reader.GetPackedInt();
            if (FieldOptions.HasFlag(GuildListFieldOptions.All) || FieldOptions.HasFlag(GuildListFieldOptions.GuildMessage))
                GuildMessage = reader.GetString();
            if (FieldOptions.HasFlag(GuildListFieldOptions.All) || FieldOptions.HasFlag(GuildListFieldOptions.GuildMessage2))
                GuildMessage2 = reader.GetString();
            if (FieldOptions.HasFlag(GuildListFieldOptions.All) || FieldOptions.HasFlag(GuildListFieldOptions.Score))
                Score = reader.GetPackedInt();
            if (FieldOptions.HasFlag(GuildListFieldOptions.All) || FieldOptions.HasFlag(GuildListFieldOptions.Options))
                Options = reader.GetString();
            if (FieldOptions.HasFlag(GuildListFieldOptions.All) || FieldOptions.HasFlag(GuildListFieldOptions.AutoAcceptRequests))
                AutoAcceptRequests = reader.GetBool();
            if (FieldOptions.HasFlag(GuildListFieldOptions.All) || FieldOptions.HasFlag(GuildListFieldOptions.Rank))
                Rank = reader.GetPackedInt();
            if (FieldOptions.HasFlag(GuildListFieldOptions.All) || FieldOptions.HasFlag(GuildListFieldOptions.CurrentMembers))
                CurrentMembers = reader.GetPackedInt();
            if (FieldOptions.HasFlag(GuildListFieldOptions.All) || FieldOptions.HasFlag(GuildListFieldOptions.MaxMembers))
                MaxMembers = reader.GetPackedInt();
        }
    }
}
