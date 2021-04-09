namespace MultiplayerARPG
{
    public class FactionDatabaseSection : BaseGameDataListSection<Faction>
    {
        public override int Order { get { return 15; } }

        public override string MenuTitle { get { return "Factions"; } }

        protected override string FieldName { get { return "factions"; } }
    }
}
