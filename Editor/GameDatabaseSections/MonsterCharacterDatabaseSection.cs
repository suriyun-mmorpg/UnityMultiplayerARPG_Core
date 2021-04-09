namespace MultiplayerARPG
{
    public class MonsterCharacterDatabaseSection : BaseGameDataListSection<MonsterCharacter>
    {
        public override int Order { get { return 11; } }

        public override string MenuTitle { get { return "Monster Characters"; } }

        protected override string FieldName { get { return "monsterCharacters"; } }
    }
}
