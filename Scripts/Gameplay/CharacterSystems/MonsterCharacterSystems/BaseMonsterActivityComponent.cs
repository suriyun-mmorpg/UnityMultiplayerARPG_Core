namespace MultiplayerARPG
{
    public abstract class BaseMonsterActivityComponent : BaseGameEntityComponent<BaseMonsterCharacterEntity>
    {
        public MonsterCharacter CharacterDatabase
        {
            get { return Entity.CharacterDatabase; }
        }
    }
}
