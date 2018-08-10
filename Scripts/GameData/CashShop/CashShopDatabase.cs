using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "CashPackageDatabase", menuName = "Create GameData/CashPackageDatabase")]
    public class CashShopDatabase : ScriptableObject
    {
        public CashShopItem[] cashStopItems;
        public CashPackage[] cashPackages;
    }
}
