using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Item Drop Table", menuName = "Create GameData/Item Drop Table", order = -4993)]
    public class ItemDropTable : ScriptableObject
    {
        [ArrayElementTitle("item")]
        public ItemDrop[] randomItems;
        [ArrayElementTitle("currency")]
        public CurrencyRandomAmount[] randomCurrencies;
    }
}
