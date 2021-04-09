namespace MultiplayerARPG
{
    public class ItemDatabaseSection : BaseGameDataListSection<BaseItem>
    {
        public override int Order { get { return 3; } }

        public override string MenuTitle { get { return "Items"; } }

        protected override string FieldName { get { return "items"; } }
    }
}
