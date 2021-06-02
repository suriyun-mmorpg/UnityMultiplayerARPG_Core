namespace MultiplayerARPG
{
    public abstract class BaseMonsterActivityComponent : BaseGameEntityComponent<BaseMonsterCharacterEntity>
    {
        public MonsterCharacter MonsterDatabase
        {
            get { return Entity.CharacterDatabase; }
        }
    }
}
