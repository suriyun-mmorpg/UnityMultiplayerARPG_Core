namespace MultiplayerARPG
{
    [System.Serializable]
    public struct UnlockRequirement
    {
        public bool isLocked;
        public int progression;
        public long softCurrency;
        public long hardCurrency;
        public int userHandleLevel;
    }
}