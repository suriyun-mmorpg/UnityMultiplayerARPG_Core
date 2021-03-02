using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IGameEntity
    {
        BaseGameEntity Entity { get; }
        LiteNetLibIdentity Identity { get; }
        void PrepareRelatesData();
        EntityInfo GetInfo();
    }
}
