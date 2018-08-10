using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "CashShopItem", menuName = "Create CashShop/CashShopItem")]
    public class CashShopItem : BaseGameData
    {
        public string externalIconUrl;
        public int sellPrice;
        public int receiveGold;
        public ItemAmount[] receiveItems;
    }
}
