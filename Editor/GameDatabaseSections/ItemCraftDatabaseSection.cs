namespace MultiplayerARPG
{
    public class ItemCraftDatabaseSection : BaseGameDataListSection<ItemCraftFormula>
    {
        public override int Order { get { return 4; } }

        public override string MenuTitle { get { return "Item Crafts"; } }

        protected override string FieldName { get { return "itemCraftFormulas"; } }
    }
}
