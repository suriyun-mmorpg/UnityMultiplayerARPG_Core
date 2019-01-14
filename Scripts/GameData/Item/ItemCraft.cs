using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class ItemCraft
    {
        public Item craftingItem;
        public ItemAmount[] craftRequirements;
        public int requireGold;

        private Dictionary<Item, short> cacheCraftRequirements;
        public Dictionary<Item, short> CacheCraftRequirements
        {
            get
            {
                if (cacheCraftRequirements == null)
                    cacheCraftRequirements = GameDataHelpers.MakeItems(craftRequirements, new Dictionary<Item, short>());
                return cacheCraftRequirements;
            }
        }

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
            character.IncreaseItems(CharacterItem.Create(craftingItem));
            foreach (ItemAmount craftRequirement in craftRequirements)
            {
                if (craftRequirement.item != null && craftRequirement.amount > 0)
                    character.DecreaseItems(craftRequirement.item.DataId, craftRequirement.amount);
            }
            character.Gold -= requireGold;
        }
    }
}
