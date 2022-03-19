namespace MultiplayerARPG
{
    public interface ILagCompensationManager
    {
        int MaxHistorySize { get; }
        bool AddHitBoxes(uint objectId, DamageableHitBox[] hitBoxes);
        bool RemoveHitBoxes(uint objectId);
        bool SimulateHitBoxes(long connectionId, long rewindTime, System.Action action);
        bool BeginSimlateHitBoxes(long connectionId, long rewindTime);
        bool SimulateHitBoxesByHalfRtt(long connectionId, System.Action action);
        bool BeginSimlateHitBoxesByHalfRtt(long connectionId);
        void EndSimulateHitBoxes();
    }
}
