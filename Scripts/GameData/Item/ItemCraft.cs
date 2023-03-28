using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct ItemCraft
    {
        public static readonly ItemCraft Empty = new ItemCraft();
        [SerializeField]
        private BaseItem craftingItem;
        public BaseItem CraftingItem { get { return craftingItem; } }

        [SerializeField]
        private int amount;
        public int Amount { get { return (amount > 0 ? amount : 1); } }

        [SerializeField]
        private int requireGold;
        public int RequireGold { get { return requireGold; } }

        [SerializeField]
        [FormerlySerializedAs("craftRequirements")]
        [ArrayElementTitle("item")]
        private ItemAmount[] requireItems;
        public ItemAmount[] RequireItems { get { return requireItems; } }

        [SerializeField]
        [ArrayElementTitle("currency")]
        private CurrencyAmount[] requireCurrencies;
        public CurrencyAmount[] RequireCurrencies { get { return requireCurrencies; } }

        public ItemCraft(
            BaseItem craftingItem,
            int amount,
            int requireGold,
            ItemAmount[] requireItems,
            CurrencyAmount[] requireCurrencies)
        {
            this.craftingItem = craftingItem;
            this.amount = amount;
            this.requireGold = requireGold;
            this.requireItems = requireItems;
            this.requireCurrencies = requireCurrencies;
        }

        public bool CanCraft(IPlayerCharacterData character)
        {
            return CanCraft(character, out _);
        }

        public bool CanCraft(IPlayerCharacterData character, out UITextKeys gameMessage)
        {
            gameMessage = UITextKeys.NONE;
            if (craftingItem == null)
            {
                gameMessage = UITextKeys.UI_ERROR_INVALID_ITEM_DATA;
                return false;
            }
            if (!GameInstance.Singleton.GameplayRule.CurrenciesEnoughToCraftItem(character, this))
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_CURRENCY_AMOUNTS;
                return false;
            }
            if (character.IncreasingItemsWillOverwhelming(craftingItem.DataId, Amount))
            {
                gameMessage = UITextKeys.UI_ERROR_WILL_OVERWHELMING;
                return false;
            }
            if (requireItems == null || requireItems.Length == 0)
            {
                // No required items
                return true;
            }
            foreach (ItemAmount craftRequirement in requireItems)
            {
                if (craftRequirement.item != null && character.CountNonEquipItems(craftRequirement.item.DataId) < craftRequirement.amount)
                {
                    gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_ITEMS;
                    return false;
                }
            }
            return true;
        }

        public void CraftItem(IPlayerCharacterData character)
        {
            if (character.IncreaseItems(CharacterItem.Create(craftingItem, 1, Amount)))
            {
                // Send notify reward item message to client
                if (character is BasePlayerCharacterEntity entity)
                    GameInstance.ServerGameMessageHandlers.NotifyRewardItem(entity.ConnectionId, RewardGivenType.Crafting, craftingItem.DataId, Amount);
                // Reduce item when able to increase craft item
                foreach (ItemAmount craftRequirement in requireItems)
                {
                    if (craftRequirement.item != null && craftRequirement.amount > 0)
                        character.DecreaseItems(craftRequirement.item.DataId, craftRequirement.amount);
                }
                character.FillEmptySlots();
                // Decrease required gold
                GameInstance.Singleton.GameplayRule.DecreaseCurrenciesWhenCraftItem(character, this);
            }
        }
    }
}
