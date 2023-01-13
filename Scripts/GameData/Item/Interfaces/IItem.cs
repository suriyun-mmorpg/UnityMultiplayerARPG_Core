using UnityEngine;

namespace MultiplayerARPG
{
    public partial interface IItem
    {
        /// <summary>
        /// Data Id, which will be used to store to game instance -> game data tables and game database tables
        /// </summary>
        int DataId { get; }
        /// <summary>
        /// Item type, still have to use it for backward compatibility reasons
        /// </summary>
        ItemType ItemType { get; }
        /// <summary>
        /// Drop model
        /// </summary>
        GameObject DropModel { get; }
        /// <summary>
        /// Sell price when sell it to NPC
        /// </summary>
        int SellPrice { get; }
        /// <summary>
        /// Weight
        /// </summary>
        float Weight { get; }
        /// <summary>
        /// Max stack
        /// </summary>
        int MaxStack { get; }
        /// <summary>
        /// Item refine data
        /// </summary>
        ItemRefine ItemRefine { get; }
        /// <summary>
        /// Max item level (can level-up by item refining system)
        /// </summary>
        int MaxLevel { get; }
        /// <summary>
        /// Lock duration before it is able to use
        /// </summary>
        float LockDuration { get; }
        /// <summary>
        /// Returning gold after this item was dismantled
        /// </summary>
        int DismantleReturnGold { get; }
        /// <summary>
        /// Returning items after this item was dismantled
        /// </summary>
        ItemAmount[] DismantleReturnItems { get; }
        /// <summary>
        /// Returning currencies after this item was dismantled
        /// </summary>
        CurrencyAmount[] DismantleReturnCurrencies { get; }
    }
}
