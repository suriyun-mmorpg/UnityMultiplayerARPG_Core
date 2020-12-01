using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Currency", menuName = "Create GameData/Currency", order = -4992)]
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
