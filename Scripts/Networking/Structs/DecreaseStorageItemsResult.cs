using System.Collections.Generic;

namespace MultiplayerARPG
{
    public struct DecreaseStorageItemsResult
    {
        public bool IsSuccess { get; set; }
        public Dictionary<int, short> DecreasedItems { get; set; }
    }
}
