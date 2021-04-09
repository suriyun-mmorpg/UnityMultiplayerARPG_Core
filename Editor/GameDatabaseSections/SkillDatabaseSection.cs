namespace MultiplayerARPG
{
    public class SkillDatabaseSection : BaseGameDataListSection<BaseSkill>
    {
        public override int Order { get { return 8; } }

        public override string MenuTitle { get { return "Skills"; } }

        protected override string FieldName { get { return "skills"; } }
    }
}
