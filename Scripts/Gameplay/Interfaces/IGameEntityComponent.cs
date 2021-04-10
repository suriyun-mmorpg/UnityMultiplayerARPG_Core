namespace MultiplayerARPG
{
    public interface IGameEntityComponent
    {
        bool Enabled { get; set; }
        void EntityAwake();
        void EntityStart();
        void EntityUpdate();
        void EntityLateUpdate();
        void EntityFixedUpdate();
        void EntityOnDestroy();
    }
}
