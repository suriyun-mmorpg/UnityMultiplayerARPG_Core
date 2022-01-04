using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Gacha", menuName = "Create GameData/Gacha", order = -4876)]
    public class Gacha : BaseGameData
    {
        [Category("Gacha Settings")]
        [SerializeField]
        private string externalIconUrl = string.Empty;
        public string ExternalIconUrl { get { return externalIconUrl; } }

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

        [ArrayElementTitle("item")]
        [SerializeField]
        private ItemRandomByWeight[] randomItems = new ItemRandomByWeight[0];
        public ItemRandomByWeight[] RandomItems
        {
            get { return randomItems; }
        }

        public List<RewardedItem> GetRandomedItems(int count)
        {
            List<RewardedItem> rewardItems = new List<RewardedItem>();
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
                rewardItems.Add(new RewardedItem()
                {
                    item = randomedItem.item,
                    amount = randomedItem.amount,
                    randomSeed = (short)Random.Range(short.MinValue, short.MaxValue),
                });
            }
            return rewardItems;
        }
    }
}
