namespace MultiplayerARPG
{
    public abstract class BaseCharacterComponent : BaseGameEntityComponent<BaseCharacterEntity>
    {
        public bool IsDead()
        {
            return CacheEntity.IsDead();
        }
    }
}
