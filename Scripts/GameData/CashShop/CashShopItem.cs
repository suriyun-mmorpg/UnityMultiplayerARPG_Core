using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Cash Shop Item", menuName = "Create CashShop/Cash Shop Item", order = -3996)]
    public class CashShopItem : BaseGameData
    {
        [Header("Cash Shop Item Configs")]
        public string externalIconUrl;
        public int sellPrice;
        public int receiveGold;
        [ArrayElementTitle("item", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public ItemAmount[] receiveItems;
    }
}
