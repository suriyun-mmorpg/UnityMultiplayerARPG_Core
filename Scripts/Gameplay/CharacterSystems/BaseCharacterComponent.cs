namespace MultiplayerARPG
{
    public abstract class BaseCharacterComponent : BaseGameComponent<BaseCharacterEntity>
    {
        public bool IsDead()
        {
            return CacheCharacterEntity.IsDead();
        }
    }
}
