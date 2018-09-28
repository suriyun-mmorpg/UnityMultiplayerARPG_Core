using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Cash Shop Database", menuName = "Create CashShop/Cash Shop Database")]
    public class CashShopDatabase : ScriptableObject
    {
        public CashShopItem[] cashStopItems;
        public CashPackage[] cashPackages;
    }
}
