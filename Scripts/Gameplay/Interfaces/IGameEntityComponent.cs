namespace MultiplayerARPG
{
    public interface IGameEntityComponent
    {
        bool Enabled { get; set; }
        bool AlwaysUpdate { get; }
        void EntityAwake();
        void EntityStart();
        void EntityOnIdentityInitialize();
        void EntityUpdate();
        void EntityLateUpdate();
        void EntityOnDestroy();
        void Clean();
    }
}
