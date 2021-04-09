namespace MultiplayerARPG
{
    public class DamageElementDatabaseSection : BaseGameDataListSection<DamageElement>
    {
        public override int Order { get { return 2; } }

        public override string MenuTitle { get { return "Damage Elements"; } }

        protected override string FieldName { get { return "damageElements"; } }
    }
}
