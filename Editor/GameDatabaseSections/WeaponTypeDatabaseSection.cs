namespace MultiplayerARPG
{
    public class WeaponTypeDatabaseSection : BaseGameDataListSection<WeaponType>
    {
        public override int Order { get { return 6; } }

        public override string MenuTitle { get { return "Weapon Types"; } }

        protected override string FieldName { get { return "weaponTypes"; } }
    }
}
