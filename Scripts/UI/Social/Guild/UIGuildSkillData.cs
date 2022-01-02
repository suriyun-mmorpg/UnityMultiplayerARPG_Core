namespace MultiplayerARPG
{
    public struct UIGuildSkillData
    {
        public GuildSkill guildSkill;
        public short targetLevel;
        public UIGuildSkillData(GuildSkill guildSkill, short targetLevel)
        {
            this.guildSkill = guildSkill;
            this.targetLevel = targetLevel;
        }
    }
}
