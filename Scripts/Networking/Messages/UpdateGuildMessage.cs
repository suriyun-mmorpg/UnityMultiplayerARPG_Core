using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct UpdateGuildMessage : INetSerializable
    {
        public enum UpdateType : byte
        {
            Create,
            ChangeLeader,
            SetGuildMessage,
            SetGuildMessage2,
            SetGuildRole,
            SetGuildMemberRole,
            SetSkillLevel,
            LevelExpSkillPoint,
            Terminate,
            SetGold,
            SetScore,
            SetOptions,
            SetAutoAcceptRequests,
            SetRank,
            UpdateStorage,
            Member,
        }
        public UpdateType type;
        public int id;
        public string guildName;
        public string guildMessage;
        public string characterId;
        public byte guildRole;
        public string roleName;
        public bool canInvite;
        public bool canKick;
        public byte shareExpPercentage;
        public short level;
        public int exp;
        public short skillPoint;
        public int gold;
        public int score;
        public string options;
        public bool autoAcceptRequests;
        public int rank;
        public int dataId;

        public void Deserialize(NetDataReader reader)
        {
            type = (UpdateType)reader.GetByte();
            id = reader.GetInt();
            switch (type)
            {
                case UpdateType.Create:
                    guildName = reader.GetString();
                    characterId = reader.GetString();
                    break;
                case UpdateType.ChangeLeader:
                    characterId = reader.GetString();
                    break;
                case UpdateType.SetGuildMessage:
                case UpdateType.SetGuildMessage2:
                    guildMessage = reader.GetString();
                    break;
                case UpdateType.SetGuildRole:
                    guildRole = reader.GetByte();
                    roleName = reader.GetString();
                    canInvite = reader.GetBool();
                    canKick = reader.GetBool();
                    shareExpPercentage = reader.GetByte();
                    break;
                case UpdateType.SetGuildMemberRole:
                    characterId = reader.GetString();
                    guildRole = reader.GetByte();
                    break;
                case UpdateType.SetSkillLevel:
                    dataId = reader.GetInt();
                    level = reader.GetShort();
                    break;
                case UpdateType.SetGold:
                    gold = reader.GetInt();
                    break;
                case UpdateType.SetScore:
                    score = reader.GetInt();
                    break;
                case UpdateType.SetOptions:
                    options = reader.GetString();
                    break;
                case UpdateType.SetAutoAcceptRequests:
                    autoAcceptRequests = reader.GetBool();
                    break;
                case UpdateType.SetRank:
                    rank = reader.GetInt();
                    break;
                case UpdateType.LevelExpSkillPoint:
                    level = reader.GetShort();
                    exp = reader.GetInt();
                    skillPoint = reader.GetShort();
                    break;
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
            writer.Put(id);
            switch (type)
            {
                case UpdateType.Create:
                    writer.Put(guildName);
                    writer.Put(characterId);
                    break;
                case UpdateType.ChangeLeader:
                    writer.Put(characterId);
                    break;
                case UpdateType.SetGuildMessage:
                case UpdateType.SetGuildMessage2:
                    writer.Put(guildMessage);
                    break;
                case UpdateType.SetGuildRole:
                    writer.Put(guildRole);
                    writer.Put(roleName);
                    writer.Put(canInvite);
                    writer.Put(canKick);
                    writer.Put(shareExpPercentage);
                    break;
                case UpdateType.SetGuildMemberRole:
                    writer.Put(characterId);
                    writer.Put(guildRole);
                    break;
                case UpdateType.SetSkillLevel:
                    writer.Put(dataId);
                    writer.Put(level);
                    break;
                case UpdateType.SetGold:
                    writer.Put(gold);
                    break;
                case UpdateType.SetScore:
                    writer.Put(score);
                    break;
                case UpdateType.SetOptions:
                    writer.Put(options);
                    break;
                case UpdateType.SetAutoAcceptRequests:
                    writer.Put(autoAcceptRequests);
                    break;
                case UpdateType.SetRank:
                    writer.Put(rank);
                    break;
                case UpdateType.LevelExpSkillPoint:
                    writer.Put(level);
                    writer.Put(exp);
                    writer.Put(skillPoint);
                    break;
            }
        }
    }
}
