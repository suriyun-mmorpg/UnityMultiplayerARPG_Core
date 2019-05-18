using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ItemCraft
    {
        [SerializeField]
        private Item craftingItem;
        [SerializeField]
        private ItemAmount[] craftRequirements;
        [SerializeField]
        private int requireGold;

        public Item CraftingItem { get { return craftingItem; } }
        private Dictionary<Item, short> cacheCraftRequirements;
        public Dictionary<Item, short> CraftRequirements
        {
            get
            {
                if (cacheCraftRequirements == null)
                    cacheCraftRequirements = GameDataHelpers.CombineItems(craftRequirements, new Dictionary<Item, short>());
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
            gameMessageType = GameMessage.Type.None;
            if (craftingItem == null)
            {
                gameMessageType = GameMessage.Type.InvalidItemData;
                return false;
            }
            if (character.Gold < requireGold)
            {
                gameMessageType = GameMessage.Type.NotEnoughGold;
                return false;
            }
            if (craftRequirements == null || craftRequirements.Length == 0)
                return true;
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
            if (character.IncreaseItems(CharacterItem.Create(craftingItem)))
            {
                // Reduce item when able to increase craft item
                foreach (ItemAmount craftRequirement in craftRequirements)
                {
                    if (craftRequirement.item != null && craftRequirement.amount > 0)
                        character.DecreaseItems(craftRequirement.item.DataId, craftRequirement.amount);
                }
                character.Gold -= requireGold;
            }
        }
    }
}
