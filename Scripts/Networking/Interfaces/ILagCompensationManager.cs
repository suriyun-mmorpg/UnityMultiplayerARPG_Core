using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface ILagCompensationManager
    {
        int MaxHistorySize { get; }
        bool AddHitBoxes(uint objectId, DamageableHitBox[] hitBoxes);
        bool RemoveHitBoxes(uint objectId);
        bool SimulateHitBoxes(long connectionId, System.Action action);
    }
}
