namespace MultiplayerARPG
{
    [System.Serializable]
    public struct NpcDialogConfirmRequirement
    {
        public int gold;
        [ArrayElementTitle("currency")]
        public CurrencyAmount[] currencyAmounts;
        [ArrayElementTitle("item")]
        public ItemAmount[] itemAmounts;
    }
}
