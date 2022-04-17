using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.ITEM_REFINE_FILE, menuName = GameDataMenuConsts.ITEM_REFINE_MENU, order = GameDataMenuConsts.ITEM_REFINE_ORDER)]
    public partial class ItemRefine : BaseGameData
    {
        [Category("Item Refine Settings")]
        [SerializeField]
        private Color titleColor = Color.clear;
        public Color TitleColor { get { return titleColor; } }

        [SerializeField]
        [Tooltip("This is refine level, each level have difference success rate, required items, required gold")]
        private ItemRefineLevel[] levels = new ItemRefineLevel[0];
        public ItemRefineLevel[] Levels { get { return levels; } }

        [SerializeField]
        [Tooltip("This is repair prices, should order from high to low durability rate")]
        private ItemRepairPrice[] repairPrices = new ItemRepairPrice[0];
        public ItemRepairPrice[] RepairPrices { get { return repairPrices; } }
    }

    [System.Serializable]
    public partial struct ItemRefineLevel
    {
        [Range(0.01f, 1f)]
        [SerializeField]
        private float successRate;
        public float SuccessRate { get { return successRate; } }

        [SerializeField]
        [ArrayElementTitle("item")]
        private ItemAmount[] requireItems;
        public ItemAmount[] RequireItems { get { return requireItems; } }

        [SerializeField]
        private int requireGold;
        public int RequireGold { get { return requireGold; } }

        [Tooltip("How many levels it will be decreased if refining failed")]
        [SerializeField]
        private short refineFailDecreaseLevels;
        public short RefineFailDecreaseLevels { get { return refineFailDecreaseLevels; } }

        [Tooltip("It will be destroyed if this value is TRUE and refining failed")]
        [SerializeField]
        private bool refineFailDestroyItem;
        public bool RefineFailDestroyItem { get { return refineFailDestroyItem; } }

        [System.NonSerialized]
        private Dictionary<BaseItem, short> cacheRequireItems;
        public Dictionary<BaseItem, short> CacheRequireItems
        {
            get
            {
                if (cacheRequireItems == null)
                    cacheRequireItems = GameDataHelpers.CombineItems(requireItems, new Dictionary<BaseItem, short>());
                return cacheRequireItems;
            }
        }

        public ItemRefineLevel(
            float successRate,
            ItemAmount[] requireItems,
            int requireGold,
            short refineFailDecreaseLevels,
            bool refineFailDestroyItem)
        {
            this.successRate = successRate;
            this.requireItems = requireItems;
            this.requireGold = requireGold;
            this.refineFailDecreaseLevels = refineFailDecreaseLevels;
            this.refineFailDestroyItem = refineFailDestroyItem;
            cacheRequireItems = null;
        }

        public bool CanRefine(IPlayerCharacterData character)
        {
            return CanRefine(character, out _);
        }

        public bool CanRefine(IPlayerCharacterData character, out UITextKeys gameMessage)
        {
            gameMessage = UITextKeys.NONE;
            if (!GameInstance.Singleton.GameplayRule.CurrenciesEnoughToRefineItem(character, this))
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD;
                return false;
            }
            if (requireItems == null || requireItems.Length == 0)
                return true;
            // Count required items
            foreach (ItemAmount requireItem in requireItems)
            {
                if (requireItem.item != null && character.CountNonEquipItems(requireItem.item.DataId) < requireItem.amount)
                {
                    gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_ITEMS;
                    return false;
                }
            }
            return true;
        }
    }

    [System.Serializable]
    public partial struct ItemRepairPrice
    {
        [Range(0.01f, 1f)]
        [SerializeField]
        private float durabilityRate;
        public float DurabilityRate { get { return durabilityRate; } }

        [SerializeField]
        private int requireGold;
        public int RequireGold { get { return requireGold; } }

        public ItemRepairPrice(float durabilityRate, int requireGold)
        {
            this.durabilityRate = durabilityRate;
            this.requireGold = requireGold;
        }

        public bool CanRepair(IPlayerCharacterData character)
        {
            return CanRepair(character, out _);
        }

        public bool CanRepair(IPlayerCharacterData character, out UITextKeys gameMessage)
        {
            gameMessage = UITextKeys.NONE;
            if (!GameInstance.Singleton.GameplayRule.CurrenciesEnoughToRepairItem(character, this))
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD;
                return false;
            }
            return true;
        }
    }
}
