using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "CashShopItem", menuName = "Create GameData/CashShopItem")]
    public class CashShopItem : BaseGameData
    {
        public string iconUrl;
        public int sellPrice;
        public int receiveGold;
        public ItemAmount[] receiveItems;
    }
    
    public class NetworkCashShopItem
    {
        public int dataId;
        public string title;
        public string description;
        public string iconUrl;
        public int sellPrice;

        public static NetworkCashShopItem MakeNetworkCashShopItem(CashShopItem cashShopItem)
        {
            var data = new NetworkCashShopItem();
            data.dataId = cashShopItem.DataId;
            data.title = cashShopItem.title;
            data.description = cashShopItem.description;
            data.iconUrl = cashShopItem.iconUrl;
            data.sellPrice = cashShopItem.sellPrice;
            return data;
        }
    }
}
