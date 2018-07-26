using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "CashShopItem", menuName = "Create GameData/CashShopItem")]
    public class CashShopItem : BaseGameData
    {
        public CashShopItemInfo info;
    }

    [System.Serializable]
    public struct CashShopItemInfo
    {
        [HideInInspector]
        public int networkDataId;
        public string externalIconUrl;
        public int sellPrice;
        public int receiveGold;
        public ItemAmount[] receiveItems;
    }
}
