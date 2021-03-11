using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Cash Shop Item", menuName = "Create CashShop/Cash Shop Item", order = -3996)]
    public class CashShopItem : BaseGameData
    {
        [Header("Cash Shop Item Configs")]
        public string externalIconUrl;
        [FormerlySerializedAs("sellPrice")]
        [Tooltip("Required user's cash")]
        public int sellPriceCash;
        [Tooltip("Required is user's gold, not character's gold")]
        public int sellPriceGold;
        [Tooltip("Gold which character will receives")]
        public int receiveGold;
        [ArrayElementTitle("currency")]
        public CurrencyAmount[] receiveCurrencies;
        [ArrayElementTitle("item")]
        public ItemAmount[] receiveItems;
    }
}
