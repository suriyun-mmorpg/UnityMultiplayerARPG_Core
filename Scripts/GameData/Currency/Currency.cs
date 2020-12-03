using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Currency", menuName = "Create GameData/Currency", order = -5999)]
    public class Currency : BaseGameData
    {
    }

    [System.Serializable]
    public struct CurrencyAmount
    {
        public Currency currency;
        public int amount;
    }
}
