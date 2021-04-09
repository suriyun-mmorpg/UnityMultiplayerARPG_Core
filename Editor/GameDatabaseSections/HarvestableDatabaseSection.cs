namespace MultiplayerARPG
{
    public class HarvestableDatabaseSection : BaseGameDataListSection<Harvestable>
    {
        public override int Order { get { return 12; } }

        public override string MenuTitle { get { return "Harvestables"; } }

        protected override string FieldName { get { return "harvestables"; } }
    }
}
