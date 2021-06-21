namespace MultiplayerARPG
{
    public struct SimulatingHit
    {
        public int hitIndex { get; set; }
        public int triggerLength { get; private set; }

        public SimulatingHit(int triggerLength)
        {
            hitIndex = 0;
            this.triggerLength = triggerLength;
        }
    }
}
