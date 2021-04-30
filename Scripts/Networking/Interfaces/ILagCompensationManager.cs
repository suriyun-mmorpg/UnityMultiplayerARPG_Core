using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface ILagCompensationManager
    {
        int MaxHistorySize { get; }
        bool AddHitBoxes(uint objectId, DamageableHitBox[] hitBoxes);
        bool RemoveHitBoxes(uint objectId);
        bool SimulateHitBoxes(long connectionId, long rewindTime, System.Action action);
        bool BeginSimlateHitBoxes(long connectionId, long rewindTime);
        bool SimulateHitBoxesByRtt(long connectionId, System.Action action);
        bool BeginSimlateHitBoxesByRtt(long connectionId);
        void EndSimulateHitBoxes();
    }
}
