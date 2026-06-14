using Insthync.UnityEditorUtils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class NpcDialogConfirmRequirement
    {
        public int gold = 0;
        [ArrayElementTitle("currency")]
        public CurrencyAmount[] currencyAmounts = new CurrencyAmount[0];
        [ArrayElementTitle("item")]
        public ItemAmount[] itemAmounts = new ItemAmount[0];

        public bool HasConfirmConditions()
        {
            return gold > 0 || (currencyAmounts != null && currencyAmounts.Length > 0) || (itemAmounts != null && itemAmounts.Length > 0);
        }
    }
}
