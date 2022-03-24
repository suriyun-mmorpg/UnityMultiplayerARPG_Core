namespace MultiplayerARPG
{
    public struct SimulatingHit
    {
        public int HitIndex { get; set; }
        public int TriggerLength { get; private set; }

        public SimulatingHit(int triggerLength)
        {
            HitIndex = 0;
            TriggerLength = triggerLength;
        }
    }
}
