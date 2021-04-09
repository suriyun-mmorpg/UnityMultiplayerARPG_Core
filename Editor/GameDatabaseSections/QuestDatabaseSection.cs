namespace MultiplayerARPG
{
    public class QDatabaseSection : BaseGameDataListSection<Quest>
    {
        public override int Order { get { return 14; } }

        public override string MenuTitle { get { return "Quests"; } }

        protected override string FieldName { get { return "quests"; } }
    }
}
