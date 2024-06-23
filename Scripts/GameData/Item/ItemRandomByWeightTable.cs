using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.ITEM_RANDOM_BY_WEIGHT_TABLE_FILE, menuName = GameDataMenuConsts.ITEM_RANDOM_BY_WEIGHT_TABLE_MENU, order = GameDataMenuConsts.ITEM_RANDOM_BY_WEIGHT_TABLE_ORDER)]
    public class ItemRandomByWeightTable : ScriptableObject, IGameDataValidation
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

        public bool OnValidateGameData()
        {
            bool hasChanges = false;
#if UNITY_EDITOR
            Debug.Log($"[ItemRandomByWeightTable] Validaing {name}, amount {randomItems.Length}");
            for (int i = 0; i < randomItems.Length; ++i)
            {
                ItemRandomByWeight data = randomItems[i];
                if (data.OnValidateGameData())
                {
                    Debug.Log($"[ItemRandomByWeightTable] Validaing {name}, has changes at {i}");
                    randomItems[i] = data;
                    hasChanges = true;
                }
            }
            if (hasChanges)
                EditorUtility.SetDirty(this);
#endif
            return hasChanges;
        }

        public void RandomItem(System.Action<BaseItem, int> onRandomItem)
        {
            ItemRandomByWeight randomedItem = WeightedRandomizer.From(CacheRandomItems).TakeOne();
            if (randomedItem.Item == null || randomedItem.maxAmount <= 0)
                return;
            if (randomedItem.minAmount <= 0)
                onRandomItem.Invoke(randomedItem.Item, randomedItem.maxAmount);
            else
                onRandomItem.Invoke(randomedItem.Item, Random.Range(randomedItem.minAmount, randomedItem.maxAmount));
        }
    }
}
