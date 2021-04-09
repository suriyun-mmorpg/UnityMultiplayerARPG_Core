namespace MultiplayerARPG
{
    public class ArmorTypeDatabaseSection : BaseGameDataListSection<ArmorType>
    {
        public override int Order { get { return 5; } }

        public override string MenuTitle { get { return "Armor Types"; } }

        protected override string FieldName { get { return "armorTypes"; } }
    }
}
