using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICraftItem : UISelectionEntry<ItemCraft>
    {
        public ItemCraft ItemCraft { get { return Data; } }

        [Header("Generic Info Format")]
        [Tooltip("Require Gold Format => {0} = {Amount}")]
        public string requireGoldFormat = "Require Gold: {0}";

        [Header("UI Elements")]
        public UICharacterItem uiCraftingItem;
        public UIItemAmounts uiRequireItemAmounts;
        public TextWrapper uiTextRequireGold;

        protected override void UpdateData()
        {
            if (uiCraftingItem != null)
            {
                if (ItemCraft.craftingItem == null)
                    uiCraftingItem.Hide();
                else
                {
                    uiCraftingItem.Show();
                    uiCraftingItem.Data = new CharacterItemTuple(CharacterItem.Create(ItemCraft.craftingItem), 1, string.Empty);
                }
            }

            if (uiRequireItemAmounts != null)
            {
                if (ItemCraft.craftingItem == null)
                    uiRequireItemAmounts.Hide();
                else
                {
                    uiRequireItemAmounts.Show();
                    uiRequireItemAmounts.Data = ItemCraft.CacheCraftRequirements;
                }
            }

            if (uiTextRequireGold != null)
            {
                if (ItemCraft.craftingItem == null)
                    uiTextRequireGold.text = string.Format(requireGoldFormat, 0.ToString("N0"));
                else
                    uiTextRequireGold.text = string.Format(requireGoldFormat, ItemCraft.requireGold.ToString("N0"));
            }
        }
    }
}
