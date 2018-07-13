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

        public bool CanCraft(ICharacterData character)
        {
            if (craftingItem == null)
                return false;
            if (craftRequirements == null || craftRequirements.Length == 0)
                return true;
            foreach (var craftRequirement in craftRequirements)
            {
                if (craftRequirement.item != null && character.CountNonEquipItems(craftRequirement.item.DataId) < craftRequirement.amount)
                    return false;
            }
            return true;
        }

        public void CraftItem(ICharacterData character)
        {
            foreach (var craftRequirement in craftRequirements)
            {
                if (craftRequirement.item != null && craftRequirement.amount > 0)
                    character.DecreaseItems(craftRequirement.item.DataId, craftRequirement.amount);
            }
            character.IncreaseItems(craftingItem.DataId, 1, 1);
        }
    }
}
