using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct EnhancerRemoval
    {
        [SerializeField]
        private bool returnEnhancerItem;
        public bool ReturnEnhancerItem { get { return returnEnhancerItem; } }

        [SerializeField]
        [ArrayElementTitle("item")]
        private ItemAmount[] requireItems;
        public ItemAmount[] RequireItems { get { return requireItems; } }

        [SerializeField]
        [ArrayElementTitle("currency")]
        private CurrencyAmount[] requireCurrencies;
        public CurrencyAmount[] RequireCurrencies { get { return requireCurrencies; } }

        [SerializeField]
        private int requireGold;
        public int RequireGold { get { return requireGold; } }

        public EnhancerRemoval(
            bool returnEnhancerItem,
            ItemAmount[] requireItems,
            CurrencyAmount[] requireCurrencies,
            int requireGold)
        {
            this.returnEnhancerItem = returnEnhancerItem;
            this.requireItems = requireItems;
            this.requireCurrencies = requireCurrencies;
            this.requireGold = requireGold;
        }

        public bool CanRemove(IPlayerCharacterData character)
        {
            return CanRemove(character, out _);
        }

        public bool CanRemove(IPlayerCharacterData character, out UITextKeys gameMessage)
        {
            gameMessage = UITextKeys.NONE;
            if (!GameInstance.Singleton.GameplayRule.CurrenciesEnoughToRemoveEnhancer(character))
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_CURRENCY_AMOUNTS;
                return false;
            }
            if (requireItems == null || requireItems.Length == 0)
                return true;
            // Count required items
            foreach (ItemAmount requireItem in requireItems)
            {
                if (requireItem.item != null && character.CountNonEquipItems(requireItem.item.DataId) < requireItem.amount)
                {
                    gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_ITEMS;
                    return false;
                }
            }
            return true;
        }
    }
}
