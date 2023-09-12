namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct Reward
    {
        public int exp;
        public int gold;
        public CurrencyAmount[] currencies;
    }
}
