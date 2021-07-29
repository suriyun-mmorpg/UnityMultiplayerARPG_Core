using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Cash Shop Item", menuName = "Create CashShop/Cash Shop Item", order = -3996)]
    public class CashShopItem : BaseGameData
    {
        [Category("Cash Shop Item Settings")]
        public string externalIconUrl;
        [FormerlySerializedAs("sellPrice")]
        public int sellPriceCash;
        public int sellPriceGold;
        [Tooltip("Gold which character will receives")]
        public int receiveGold;
        [ArrayElementTitle("currency")]
        public CurrencyAmount[] receiveCurrencies;
        [ArrayElementTitle("item")]
        public ItemAmount[] receiveItems;
    }
}
