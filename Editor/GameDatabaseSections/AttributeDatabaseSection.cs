namespace MultiplayerARPG
{
    public class AttributeDatabaseSection : BaseGameDataListSection<Attribute>
    {
        public override int Order { get { return 0; } }

        public override string MenuTitle { get { return "Attributes"; } }

        protected override string FieldName { get { return "attributes"; } }
    }
}
