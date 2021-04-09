namespace MultiplayerARPG
{
    public class PlayerCharacterDatabaseSection : BaseGameDataListSection<PlayerCharacter>
    {
        public override int Order { get { return 10; } }

        public override string MenuTitle { get { return "Player Characters\n(aka Character Class)"; } }

        protected override string FieldName { get { return "playerCharacters"; } }
    }
}
