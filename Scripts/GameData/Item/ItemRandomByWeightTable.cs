using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

        public void RandomItem(OnDropItemDelegate onRandomItem)
        {
            ItemRandomByWeight randomedItem = WeightedRandomizer.From(CacheRandomItems).TakeOne();
            if (randomedItem.item == null)
                return;
            onRandomItem.Invoke(randomedItem.item, randomedItem.GetRandomedLevel(), randomedItem.GetRandomedAmount());
        }

#if UNITY_EDITOR
        [ContextMenu("Set ammo drop amount to max stack")]
        public void SetAmmoDropAmountToMaxStack()
        {
            for (int i = 0; i < randomItems.Length; ++i)
            {
                ItemRandomByWeight randomItem = randomItems[i];
                if (randomItem.item == null)
                    continue;
                if (randomItem.item is IAmmoItem ammoItem)
                {
                    randomItem.minAmount = ammoItem.MaxStack;
                    randomItem.maxAmount = ammoItem.MaxStack;
                    randomItems[i] = randomItem;
                }
            }
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
