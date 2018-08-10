using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct CashShopItem
    {
        public string id;
        public string title;
        [TextArea]
        public string description;
        public Sprite icon;
        public string externalIconUrl;
        public int sellPrice;
        public int receiveGold;
        public ItemAmount[] receiveItems;

        public string Id { get { return id; } }
        public int DataId { get { return Id.GenerateHashId(); } }
    }
}
