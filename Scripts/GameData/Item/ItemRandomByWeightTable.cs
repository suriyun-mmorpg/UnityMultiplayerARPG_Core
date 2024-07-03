using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.ITEM_RANDOM_BY_WEIGHT_TABLE_FILE, menuName = GameDataMenuConsts.ITEM_RANDOM_BY_WEIGHT_TABLE_MENU, order = GameDataMenuConsts.ITEM_RANDOM_BY_WEIGHT_TABLE_ORDER)]
    public class ItemRandomByWeightTable : ScriptableObject
    {
        [Tooltip("Can set empty item as a chance to not drop any items")]
        [ArrayElementTitle("item")]
        public ItemRandomByWeight[] randomItems = new ItemRandomByWeight[0];

        [System.NonSerialized]
        private Dictionary<ItemRandomByWeight, int> _cacheRandomItems;
        public Dictionary<ItemRandomByWeight, int> CacheRandomItems
        {
            get
            {
                if (_cacheRandomItems == null)
                {
                    _cacheRandomItems = new Dictionary<ItemRandomByWeight, int>();
                    foreach (ItemRandomByWeight item in randomItems)
                    {
                        if (item.randomWeight <= 0)
                            continue;
                        _cacheRandomItems[item] = item.randomWeight;
                    }
                }
                return _cacheRandomItems;
            }
        }

        public void RandomItem(System.Action<BaseItem, int> onRandomItem)
        {
            ItemRandomByWeight randomedItem = WeightedRandomizer.From(CacheRandomItems).TakeOne();
            if (randomedItem.item == null || randomedItem.maxAmount <= 0)
                return;
            if (randomedItem.minAmount <= 0)
                onRandomItem.Invoke(randomedItem.item, randomedItem.maxAmount);
            else
                onRandomItem.Invoke(randomedItem.item, Random.Range(randomedItem.minAmount, randomedItem.maxAmount));
        }
    }
}
