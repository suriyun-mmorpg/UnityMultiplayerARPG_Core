namespace MultiplayerARPG
{
    public interface IGameEntityComponent
    {
        bool Enabled { get; set; }
        bool AlwaysUpdate { get; }
        void EntityAwake();
        void EntityStart();
        void EntityUpdate();
        void EntityLateUpdate();
        void EntityOnDestroy();
    }
}
