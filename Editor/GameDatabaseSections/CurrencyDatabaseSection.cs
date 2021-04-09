namespace MultiplayerARPG
{
    public class CurrencyDatabaseSection : BaseGameDataListSection<Currency>
    {
        public override int Order { get { return 1; } }

        public override string MenuTitle { get { return "Currencies"; } }

        protected override string FieldName { get { return "currencies"; } }
    }
}
