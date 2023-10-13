namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct Reward
    {
        public int exp;
        public int gold;
        public CurrencyAmount[] currencies;

        public bool NoExp()
        {
            return exp <= 0;
        }

        public bool NoGold()
        {
            return gold <= 0;
        }

        public bool NoCurrencies()
        {
            return currencies == null || currencies.Length == 0;
        }

        public bool NoRewards()
        {
            return NoExp() && NoGold() && NoCurrencies();
        }
    }
}
