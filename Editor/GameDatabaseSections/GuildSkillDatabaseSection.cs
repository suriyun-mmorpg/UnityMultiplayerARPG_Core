namespace MultiplayerARPG
{
    public class GuildSkillDatabaseSection : BaseGameDataListSection<GuildSkill>
    {
        public override int Order { get { return 9; } }

        public override string MenuTitle { get { return "Guild Skills"; } }

        protected override string FieldName { get { return "guildSkills"; } }
    }
}
