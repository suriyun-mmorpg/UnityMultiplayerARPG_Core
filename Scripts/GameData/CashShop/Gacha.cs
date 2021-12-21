using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Gacha", menuName = "Create CashShop/Gacha", order = -3994)]
    public class Gacha : BaseGameData
    {
        [SerializeField]
        private int singleModeOpenPrice = 10;
        public int SingleModeOpenPrice
        {
            get { return singleModeOpenPrice; }
        }

        [SerializeField]
        private int multipleModeOpenPrice = 100;
        public int MultipleModeOpenPrice
        {
            get { return multipleModeOpenPrice; }
        }

        [SerializeField]
        private int multipleModeOpenCount = 11;
        public int MultipleModeOpenCount
        {
            get { return multipleModeOpenCount; }
        }

        [SerializeField]
        private ItemRandomByWeight[] randomItems = new ItemRandomByWeight[0];
        public ItemRandomByWeight[] RandomItems
        {
            get { return randomItems; }
        }

        public List<ItemAmount> GetRandomedItems(int count)
        {
            List<ItemAmount> rewardItems = new List<ItemAmount>();
            Dictionary<ItemRandomByWeight, int> randomItems = new Dictionary<ItemRandomByWeight, int>();
            foreach (ItemRandomByWeight item in RandomItems)
            {
                if (item.item == null || item.randomWeight <= 0)
                    continue;
                randomItems[item] = item.randomWeight;
            }
            for (int i = 0; i < count; ++i)
            {
                ItemRandomByWeight randomedItem = WeightedRandomizer.From(randomItems).TakeOne();
                rewardItems.Add(new ItemAmount()
                {
                    item = randomedItem.item,
                    amount = randomedItem.amount,
                });
            }
            return rewardItems;
        }
    }
}
