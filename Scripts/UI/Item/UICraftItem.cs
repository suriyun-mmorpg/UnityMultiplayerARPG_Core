using UnityEngine;

namespace MultiplayerARPG
{
    public class UICraftItem : UISelectionEntry<ItemCraft>
    {
        [Header("UI Elements")]
        public UICharacterItem uiCraftingItem;
        public UIItemAmounts uiRequireItemAmounts;

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
                    uiCraftingItem.Data = new CharacterItemLevelTuple(CharacterItem.Create(craftItemData.craftingItem), 1);
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
        }
    }
}
