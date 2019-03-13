using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICraftItem : UISelectionEntry<ItemCraft>
    {
        public ItemCraft ItemCraft { get { return Data; } }

        [Header("Generic Info Format")]
        [Tooltip("Require Gold Format => {0} = {Current Amount}, {1} = {Target Amount}")]
        public string requireGoldFormat = "Require Gold: {0}/{1}";
        [Tooltip("Require Gold Format => {0} = {Current Amount}, {1} = {Target Amount}")]
        public string requireGoldNotEnoughFormat = "Require Gold: <color=red>{0}/{1}</color>";

        [Header("UI Elements")]
        public UICharacterItem uiCraftingItem;
        public UIItemAmounts uiRequireItemAmounts;
        public TextWrapper uiTextRequireGold;

        protected override void UpdateData()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (uiCraftingItem != null)
            {
                if (ItemCraft.craftingItem == null)
                    uiCraftingItem.Hide();
                else
                {
                    uiCraftingItem.Show();
                    uiCraftingItem.Data = new CharacterItemTuple(CharacterItem.Create(ItemCraft.craftingItem), 1, InventoryType.NonEquipItems);
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
                    uiTextRequireGold.text = string.Format(requireGoldFormat, 0.ToString("N0"), 0.ToString("N0"));
                else
                {
                    int currentAmount = 0;
                    if (owningCharacter != null)
                        currentAmount = owningCharacter.Gold;
                    uiTextRequireGold.text = string.Format(
                        currentAmount >= ItemCraft.requireGold ? requireGoldFormat : requireGoldNotEnoughFormat,
                        currentAmount.ToString("N0"), ItemCraft.requireGold.ToString("N0"));
                }
            }
        }
    }
}
