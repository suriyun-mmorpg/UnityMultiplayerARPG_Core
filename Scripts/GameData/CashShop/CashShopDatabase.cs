using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.CASH_SHOP_DATABASE_FILE, menuName = GameDataMenuConsts.CASH_SHOP_DATABASE_MENU, order = GameDataMenuConsts.CASH_SHOP_DATABASE_ORDER)]
    public class CashShopDatabase : ScriptableObject
    {
        public CashShopItem[] cashStopItems;
        public CashPackage[] cashPackages;
    }
}
