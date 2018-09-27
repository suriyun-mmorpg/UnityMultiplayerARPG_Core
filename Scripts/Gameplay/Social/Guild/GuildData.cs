using System.Collections.Generic;

namespace MultiplayerARPG
{
    public class GuildData : SocialGroupData
    {
        public string guildName { get; private set; }
        public string leaderName { get; private set; }
        public int level;
        public int exp;
        public int skillPoint;
        public string guildMessage;

        public GuildData(int id, string guildName, string leaderId, string leaderName)
            : base(id, leaderId)
        {
            this.guildName = guildName;
            this.leaderName = leaderName;
            level = 1;
            exp = 0;
            skillPoint = 0;
            guildMessage = string.Empty;
        }

        public GuildData(int id, string guildName, BasePlayerCharacterEntity leaderCharacterEntity)
            : this(id, guildName, leaderCharacterEntity.Id, leaderCharacterEntity.CharacterName)
        {
            AddMember(leaderCharacterEntity);
        }
    }
}
