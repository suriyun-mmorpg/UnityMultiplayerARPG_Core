using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.ITEM_DROP_TABLE_FILE, menuName = GameDataMenuConsts.ITEM_DROP_TABLE_MENU, order = GameDataMenuConsts.ITEM_DROP_TABLE_ORDER)]
    public class ItemDropTable : ScriptableObject
    {
        [ArrayElementTitle("item")]
        public ItemDrop[] randomItems;
        [ArrayElementTitle("currency")]
        public CurrencyRandomAmount[] randomCurrencies;
    }
}
