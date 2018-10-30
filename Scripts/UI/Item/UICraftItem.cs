using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICraftItem : UISelectionEntry<ItemCraft>
    {
        [Header("Generic Info Format")]
        [Tooltip("Require Gold Format => {0} = {Amount}")]
        public string requireGoldFormat = "Require Gold: {0}";

        [Header("UI Elements")]
        public UICharacterItem uiCraftingItem;
        public UIItemAmounts uiRequireItemAmounts;
        public TextWrapper uiTextRequireGold;

        protected override void UpdateData()
        {
            var craftItemData = Data;
            if (uiCraftingItem != null)
            {
                if (craftItemData.craftingItem == null)
                    uiCraftingItem.Hide();
                else
                {
                    uiCraftingItem.Show();
                    uiCraftingItem.Data = new CharacterItemTuple(CharacterItem.Create(craftItemData.craftingItem), 1, string.Empty);
                }
            }

            if (uiRequireItemAmounts != null)
            {
                if (craftItemData.craftingItem == null)
                    uiRequireItemAmounts.Hide();
                else
                {
                    uiRequireItemAmounts.Show();
                    uiRequireItemAmounts.Data = craftItemData.CacheCraftRequirements;
                }
            }

            if (uiTextRequireGold != null)
            {
                if (craftItemData.craftingItem == null)
                    uiTextRequireGold.text = string.Format(requireGoldFormat, 0.ToString("N0"));
                else
                    uiTextRequireGold.text = string.Format(requireGoldFormat, craftItemData.requireGold.ToString("N0"));
            }
        }
    }
}
