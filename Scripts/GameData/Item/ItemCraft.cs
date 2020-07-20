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
        [ArrayElementTitle("item", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        private ItemAmount[] craftRequirements;
        [SerializeField]
        private int requireGold;

        public BaseItem CraftingItem { get { return craftingItem; } }
        public short Amount { get { return amount; } }

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
        public int RequireGold { get { return requireGold; } }

        public bool CanCraft(IPlayerCharacterData character)
        {
            GameMessage.Type gameMessageType;
            return CanCraft(character, out gameMessageType);
        }

        public bool CanCraft(IPlayerCharacterData character, out GameMessage.Type gameMessageType)
        {
            // Mininmum amount is 1
            if (amount <= 0)
                amount = 1;
            gameMessageType = GameMessage.Type.None;
            if (craftingItem == null)
            {
                gameMessageType = GameMessage.Type.InvalidItemData;
                return false;
            }
            if (!GameInstance.Singleton.GameplayRule.CurrenciesEnoughToCraftItem(character, this))
            {
                gameMessageType = GameMessage.Type.NotEnoughGold;
                return false;
            }
            if (character.IncreasingItemsWillOverwhelming(craftingItem.DataId, amount))
            {
                gameMessageType = GameMessage.Type.CannotCarryAnymore;
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
                    gameMessageType = GameMessage.Type.NotEnoughItems;
                    return false;
                }
            }
            return true;
        }

        public void CraftItem(IPlayerCharacterData character)
        {
            // Mininmum amount is 1
            if (amount <= 0)
                amount = 1;
            if (character.IncreaseItems(CharacterItem.Create(craftingItem, 1, amount)))
            {
                // Reduce item when able to increase craft item
                foreach (ItemAmount craftRequirement in craftRequirements)
                {
                    if (craftRequirement.item != null && craftRequirement.amount > 0)
                        character.DecreaseItems(craftRequirement.item.DataId, craftRequirement.amount, GameInstance.Singleton.IsLimitInventorySlot);
                }
                character.FillEmptySlots();
                // Decrease required gold
                GameInstance.Singleton.GameplayRule.DecreaseCurrenciesWhenCraftItem(character, this);
            }
        }
    }
}
