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
                    cacheCraftRequirements = GameDataHelpers.MakeItemAmountsDictionary(craftRequirements, new Dictionary<Item, short>());
                return cacheCraftRequirements;
            }
        }

        public bool CanCraft(IPlayerCharacterData character)
        {
            if (craftingItem == null)
                return false;
            if ((craftRequirements == null || craftRequirements.Length == 0) && character.Gold >= requireGold)
                return true;
            foreach (var craftRequirement in craftRequirements)
            {
                if (craftRequirement.item != null && character.CountNonEquipItems(craftRequirement.item.DataId) < craftRequirement.amount)
                    return false;
            }
            return true;
        }
        
        public void CraftItem(IPlayerCharacterData character)
        {
            character.IncreaseItems(craftingItem.DataId, 1, 1);
            foreach (var craftRequirement in craftRequirements)
            {
                if (craftRequirement.item != null && craftRequirement.amount > 0)
                    character.DecreaseItems(craftRequirement.item.DataId, craftRequirement.amount);
            }
            character.Gold -= requireGold;
        }
    }
}
