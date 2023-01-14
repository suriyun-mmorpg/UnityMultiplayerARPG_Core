using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.CURRENCY_FILE, menuName = GameDataMenuConsts.CURRENCY_MENU, order = GameDataMenuConsts.CURRENCY_ORDER)]
    public partial class Currency : BaseGameData
    {
    }

    [System.Serializable]
    public struct CurrencyAmount
    {
        public Currency currency;
        public int amount;
    }

    [System.Serializable]
    public struct CurrencyRandomAmount
    {
        public Currency currency;
        public int minAmount;
        public int maxAmount;
    }
}
