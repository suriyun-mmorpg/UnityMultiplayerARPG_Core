using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICashShop : UIBase
    {
        public UICashShopItem uiCashShopItemPrefab;
        public Transform uiCashShopItemContainer;

        private UIList cacheList;
        public UIList CacheList
        {
            get
            {
                if (cacheList == null)
                {
                    cacheList = gameObject.AddComponent<UIList>();
                    cacheList.uiPrefab = uiCashShopItemPrefab.gameObject;
                    cacheList.uiContainer = uiCashShopItemContainer;
                }
                return cacheList;
            }
        }

        public override void Show()
        {
            base.Show();
            // Load cash shop item list
        }

        public void Buy(int dataId)
        {

        }
    }
}
