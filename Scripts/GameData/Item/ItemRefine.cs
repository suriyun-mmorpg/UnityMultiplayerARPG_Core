using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Item Refine Info", menuName = "Create GameData/Item Refine Info")]
    public partial class ItemRefine : BaseGameData
    {
        public ItemRefineLevel[] levels;
    }

    [System.Serializable]
    public class ItemRefineLevel
    {
        [Range(0.01f, 1f)]
        public float successRate = 1;
        public ItemAmount[] requireItems;
        public int requireGold;
        public short refineFailDecreaseLevels;
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
            if (character.Gold < requireGold)
                return false;
            if (requireItems == null || requireItems.Length == 0)
                return true;
            foreach (var requireItem in requireItems)
            {
                if (requireItem.item != null && character.CountNonEquipItems(requireItem.item.DataId) < requireItem.amount)
                    return false;
            }
            return true;
        }

        public bool RefineItem(IPlayerCharacterData character, int nonEquipIndex)
        {
            var isSuccess = false;
            var refiningItem = character.NonEquipItems[nonEquipIndex];
            if (Random.value <= successRate)
            {
                ++refiningItem.level;
                character.NonEquipItems[nonEquipIndex] = refiningItem;
                isSuccess = true;
            }
            else
            {
                if (refineFailDestroyItem)
                    character.NonEquipItems.RemoveAt(nonEquipIndex);
                else
                {
                    refiningItem.level -= refineFailDecreaseLevels;
                    if (refiningItem.level < 1)
                        refiningItem.level = 1;
                    character.NonEquipItems[nonEquipIndex] = refiningItem;
                }
            }
            foreach (var requireItem in requireItems)
            {
                if (requireItem.item != null && requireItem.amount > 0)
                    character.DecreaseItems(requireItem.item.DataId, requireItem.amount);
            }
            character.Gold -= requireGold;
            return isSuccess;
        }
    }
}
