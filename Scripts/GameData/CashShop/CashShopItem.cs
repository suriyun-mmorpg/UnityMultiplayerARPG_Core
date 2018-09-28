using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Cash Shop Item", menuName = "Create CashShop/Cash Shop Item")]
    public class CashShopItem : BaseGameData
    {
        public string externalIconUrl;
        public int sellPrice;
        public int receiveGold;
        public ItemAmount[] receiveItems;
    }
}
