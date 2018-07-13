using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "ItemRefineInfo", menuName = "Create GameData/ItemRefineInfo")]
    public class ItemRefine : BaseGameData
    {
        public ItemRefineLevel[] levels;
    }

    [System.Serializable]
    public class ItemRefineLevel
    {
        [Range(0.01f, 1f)]
        public float successRate;
        public ItemAmount[] requireItems;
        public int requireGold;
        public int refineFailDecreaseLevels;
        public bool refineFailDestroyItem;

        private Dictionary<Item, short> cacheRequireItems;
        public Dictionary<Item, short> CacheRequireItems
        {
            get
            {
                if (cacheRequireItems == null)
                    cacheRequireItems = GameDataHelpers.MakeItemAmountsDictionary(requireItems, new Dictionary<Item, short>());
                return cacheRequireItems;
            }
        }
        
        public bool CanRefine(IPlayerCharacterData character)
        {
            if ((requireItems == null || requireItems.Length == 0) && character.Gold >= requireGold)
                return true;
            foreach (var requireItem in requireItems)
            {
                if (requireItem.item != null && character.CountNonEquipItems(requireItem.item.DataId) < requireItem.amount)
                    return false;
            }
            return true;
        }

        public void RefineItem(IPlayerCharacterData character, int nonEquipIndex)
        {
            var refiningItem = character.NonEquipItems[nonEquipIndex];
            ++refiningItem.level;
            character.NonEquipItems[nonEquipIndex] = refiningItem;
            foreach (var requireItem in requireItems)
            {
                if (requireItem.item != null && requireItem.amount > 0)
                    character.DecreaseItems(requireItem.item.DataId, requireItem.amount);
            }
            character.Gold -= requireGold;
        }
    }
}
