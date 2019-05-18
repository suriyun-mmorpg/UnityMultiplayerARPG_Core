using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICraftItem : UISelectionEntry<ItemCraft>
    {
        public ItemCraft ItemCraft { get { return Data; } }
        public Item CraftingItem { get { return ItemCraft.craftingItem; } }

        /// <summary>
        /// Format => {0} = {Required Gold Label}, {1} = {Current Amount}, {2} = {Target Amount}
        /// </summary>
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Required Gold Label}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public string formatRequireGold = "{0}: {1}/{2}";
        /// <summary>
        /// Format => {0} = {Required Gold Label}, {1} = {Current Amount}, {2} = {Target Amount}
        /// </summary>
        [Tooltip("Format => {0} = {Required Gold Label}, {1} = {Current Amount}, {2} = {Target Amount}")]
        public string formatRequireGoldNotEnough = "{0}: <color=red>{1}/{2}</color>";

        [Header("UI Elements")]
        public UICharacterItem uiCraftingItem;
        public UIItemAmounts uiRequireItemAmounts;
        public TextWrapper uiTextRequireGold;

        public CrafterType CrafterType { get; private set; }
        public uint BuildingObjectId { get; protected set; }

        public void SetupForCharacter(ItemCraft data)
        {
            CrafterType = CrafterType.Character;
            Data = data;
        }

        public void SetupForNpc(ItemCraft data)
        {
            CrafterType = CrafterType.Npc;
            Data = data;
        }

        public void SetupForWorkbench(uint objectId, ItemCraft data)
        {
            CrafterType = CrafterType.Workbench;
            BuildingObjectId = objectId;
            Data = data;
        }

        protected override void UpdateData()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (uiCraftingItem != null)
            {
                if (CraftingItem == null)
                {
                    // Hide if crafting item is null
                    uiCraftingItem.Hide();
                }
                else
                {
                    uiCraftingItem.Show();
                    uiCraftingItem.Data = new CharacterItemTuple(CharacterItem.Create(CraftingItem), 1, InventoryType.NonEquipItems);
                }
            }

            if (uiRequireItemAmounts != null)
            {
                if (CraftingItem == null)
                {
                    // Hide if crafting item is null
                    uiRequireItemAmounts.Hide();
                }
                else
                {
                    uiRequireItemAmounts.showAsRequirement = true;
                    uiRequireItemAmounts.Show();
                    uiRequireItemAmounts.Data = ItemCraft.CacheCraftRequirements;
                }
            }

            if (uiTextRequireGold != null)
            {
                if (CraftingItem == null)
                {
                    uiTextRequireGold.text = string.Format(
                        formatRequireGold,
                        LanguageManager.GetText(UILocaleKeys.UI_LABEL_REQUIRE_GOLD.ToString()),
                        "0",
                        "0");
                }
                else
                {
                    int currentAmount = 0;
                    if (owningCharacter != null)
                        currentAmount = owningCharacter.Gold;
                    uiTextRequireGold.text = string.Format(
                        currentAmount >= ItemCraft.requireGold ? formatRequireGold : formatRequireGoldNotEnough,
                        LanguageManager.GetText(UILocaleKeys.UI_LABEL_REQUIRE_GOLD.ToString()),
                        currentAmount.ToString("N0"),
                        ItemCraft.requireGold.ToString("N0"));
                }
            }
        }

        public void OnClickCraft()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null && CraftingItem != null)
            {
                if (CrafterType == CrafterType.Workbench)
                    owningCharacter.RequestCraftItemByWorkbench(BuildingObjectId, CraftingItem.DataId);
            }
        }
    }
}
