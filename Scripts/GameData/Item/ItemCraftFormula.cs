using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.ITEM_CRAFT_FORMULA_FILE, menuName = GameDataMenuConsts.ITEM_CRAFT_FORMULA_MENU, order = GameDataMenuConsts.ITEM_CRAFT_FORMULA_ORDER)]
    public partial class ItemCraftFormula : BaseGameData
    {
        [Category("Item Craft Formula Settings")]
        [SerializeField]
        private ItemCraft itemCraft = default;
        public ItemCraft ItemCraft
        {
            get { return itemCraft; }
        }

        [SerializeField]
        private float craftDuration = 0f;
        public float CraftDuration
        {
            get { return craftDuration; }
        }

        public HashSet<int> SourceIds { get; private set; } = new HashSet<int>();

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddItems(ItemCraft.CraftingItem);
            GameInstance.AddItems(ItemCraft.RequireItems);
            GameInstance.AddCurrencies(ItemCraft.RequireCurrencies);
        }
    }
}
