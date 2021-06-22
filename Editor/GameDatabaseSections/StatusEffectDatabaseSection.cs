namespace MultiplayerARPG
{
    public class StatusEffectDatabaseSection : BaseGameDataListSection<StatusEffect>
    {
        public override int Order { get { return 6; } }

        public override string MenuTitle { get { return "Status Effects"; } }

        protected override string FieldName { get { return "statusEffects"; } }
    }
}
