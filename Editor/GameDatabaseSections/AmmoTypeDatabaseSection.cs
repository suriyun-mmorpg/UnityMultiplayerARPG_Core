namespace MultiplayerARPG
{
    public class AmmoTypeDatabaseSection : BaseGameDataListSection<AmmoType>
    {
        public override int Order { get { return 7; } }

        public override string MenuTitle { get { return "Ammo Types"; } }

        protected override string FieldName { get { return "ammoTypes"; } }
    }
}
