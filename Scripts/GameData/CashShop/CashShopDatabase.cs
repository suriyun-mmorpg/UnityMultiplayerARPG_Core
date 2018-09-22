using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "CashShopDatabase", menuName = "Create CashShop/CashShopDatabase")]
    public class CashShopDatabase : ScriptableObject
    {
        public CashShopItem[] cashStopItems;
        public CashPackage[] cashPackages;
    }
}
