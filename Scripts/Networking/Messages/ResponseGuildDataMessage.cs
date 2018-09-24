using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class ResponseGuildDataMessage : ResponseSocialGroupDataMessage
    {
        public string guildName;
        public string leaderId;
        public string leaderName;
        public int level;
        public int exp;
        public int skillPoint;
        public string message;

        public override void DeserializeData(NetDataReader reader)
        {
            guildName = reader.GetString();
            leaderId = reader.GetString();
            leaderName = reader.GetString();
            level = reader.GetInt();
            exp = reader.GetInt();
            skillPoint = reader.GetInt();
            message = reader.GetString();
            base.DeserializeData(reader);
        }

        public override void SerializeData(NetDataWriter writer)
        {
            writer.Put(guildName);
            writer.Put(leaderId);
            writer.Put(leaderName);
            writer.Put(level);
            writer.Put(exp);
            writer.Put(skillPoint);
            writer.Put(message);
            base.SerializeData(writer);
        }
    }
}
