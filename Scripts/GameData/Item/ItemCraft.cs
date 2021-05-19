using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial struct ItemCraft
    {
        [SerializeField]
        private BaseItem craftingItem;
        [SerializeField]
        private short amount;
        [SerializeField]
        private int requireGold;
        [SerializeField]
        [ArrayElementTitle("item")]
        private ItemAmount[] craftRequirements;

        public BaseItem CraftingItem { get { return craftingItem; } }
        public short Amount { get { return (short)(amount > 0 ? amount : 1); } }
        public int RequireGold { get { return requireGold; } }

        [System.NonSerialized]
        private Dictionary<BaseItem, short> cacheCraftRequirements;
        public Dictionary<BaseItem, short> CacheCraftRequirements
        {
            get
            {
                if (cacheCraftRequirements == null)
                    cacheCraftRequirements = GameDataHelpers.CombineItems(craftRequirements, new Dictionary<BaseItem, short>());
                return cacheCraftRequirements;
            }
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
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD;
                return false;
            }
            if (character.IncreasingItemsWillOverwhelming(craftingItem.DataId, Amount))
            {
                gameMessage = UITextKeys.UI_ERROR_WILL_OVERWHELMING;
                return false;
            }
            if (craftRequirements == null || craftRequirements.Length == 0)
            {
                // No required items
                return true;
            }
            foreach (ItemAmount craftRequirement in craftRequirements)
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
                if (character is BasePlayerCharacterEntity)
                    GameInstance.ServerGameMessageHandlers.NotifyRewardItem((character as BasePlayerCharacterEntity).ConnectionId, craftingItem.DataId, Amount);
                // Reduce item when able to increase craft item
                foreach (ItemAmount craftRequirement in craftRequirements)
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
