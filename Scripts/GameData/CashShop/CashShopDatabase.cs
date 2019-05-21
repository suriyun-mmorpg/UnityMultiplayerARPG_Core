using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Cash Shop Database", menuName = "Create CashShop/Cash Shop Database", order = -3997)]
    public class CashShopDatabase : ScriptableObject
    {
        public CashShopItem[] cashStopItems;
        public CashPackage[] cashPackages;
    }
}
