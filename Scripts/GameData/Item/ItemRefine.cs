using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Item Refine Info", menuName = "Create GameData/Item Refine Info")]
    public partial class ItemRefine : BaseGameData
    {
        public Color titleColor = Color.white;
        [Tooltip("This is refine level, each level have difference success rate, required items, required gold")]
        public ItemRefineLevel[] levels;
        [Tooltip("This is repair prices, should order from high to low durability rate")]
        public ItemRepairPrice[] repairPrices;
    }

    [System.Serializable]
    public struct ItemRefineLevel
    {
        [Range(0.01f, 1f)]
        [SerializeField]
        private float successRate;
        [SerializeField]
        private ItemAmount[] requireItems;
        [SerializeField]
        private int requireGold;
        [Tooltip("How many levels it will be decreased if refining failed")]
        [SerializeField]
        private short refineFailDecreaseLevels;
        [Tooltip("It will be destroyed if this value is TRUE and refining failed")]
        [SerializeField]
        private bool refineFailDestroyItem;

        public float SuccessRate { get { return successRate; } }
        public ItemAmount[] RequireItemsArray { get { return requireItems; } }
        private Dictionary<Item, short> cacheRequireItems;
        public Dictionary<Item, short> RequireItems
        {
            get
            {
                if (cacheRequireItems == null)
                    cacheRequireItems = GameDataHelpers.CombineItems(requireItems, new Dictionary<Item, short>());
                return cacheRequireItems;
            }
        }
        public int RequireGold { get { return requireGold; } }
        public short RefineFailDecreaseLevels { get { return refineFailDecreaseLevels; } }
        public bool RefineFailDestroyItem { get { return refineFailDestroyItem; } }

        public bool CanRefine(IPlayerCharacterData character)
        {
            GameMessage.Type gameMessageType;
            return CanRefine(character, out gameMessageType);
        }

        public bool CanRefine(IPlayerCharacterData character, out GameMessage.Type gameMessageType)
        {
            gameMessageType = GameMessage.Type.None;
            if (character.Gold < requireGold)
            {
                gameMessageType = GameMessage.Type.NotEnoughGold;
                return false;
            }
            if (requireItems == null || requireItems.Length == 0)
                return true;
            // Count required items
            foreach (ItemAmount requireItem in requireItems)
            {
                if (requireItem.item != null && character.CountNonEquipItems(requireItem.item.DataId) < requireItem.amount)
                {
                    gameMessageType = GameMessage.Type.NotEnoughItems;
                    return false;
                }
            }
            return true;
        }
    }

    [System.Serializable]
    public struct ItemRepairPrice
    {
        [Range(0.01f, 1f)]
        [SerializeField]
        private float durabilityRate;
        [SerializeField]
        private int requireGold;

        public float DurabilityRate { get { return durabilityRate; } }
        public int RequireGold { get { return requireGold; } }

        public bool CanRepair(IPlayerCharacterData character)
        {
            GameMessage.Type gameMessageType;
            return CanRepair(character, out gameMessageType);
        }

        public bool CanRepair(IPlayerCharacterData character, out GameMessage.Type gameMessageType)
        {
            gameMessageType = GameMessage.Type.None;
            if (character.Gold < requireGold)
            {
                gameMessageType = GameMessage.Type.NotEnoughGold;
                return false;
            }
            return true;
        }
    }
}
