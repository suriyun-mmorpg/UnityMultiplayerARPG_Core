using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICraftItem : UISelectionEntry<ItemCraft>
    {
        public ItemCraft ItemCraft { get { return Data; } }
        public Item CraftingItem { get { return ItemCraft.craftingItem; } }

        [Header("Generic Info Format")]
        [Tooltip("Require Gold Format => {0} = {Current Amount}, {1} = {Target Amount}, {2} = {Required Gold Label}")]
        public string requireGoldFormat = "{2}: {0}/{1}";
        [Tooltip("Require Gold Format => {0} = {Current Amount}, {1} = {Target Amount}, {2} = {Required Gold Label}")]
        public string requireGoldNotEnoughFormat = "{2}: <color=red>{0}/{1}</color>";

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
                    uiCraftingItem.Hide();
                else
                {
                    uiCraftingItem.Show();
                    uiCraftingItem.Data = new CharacterItemTuple(CharacterItem.Create(CraftingItem), 1, InventoryType.NonEquipItems);
                }
            }

            if (uiRequireItemAmounts != null)
            {
                if (CraftingItem == null)
                    uiRequireItemAmounts.Hide();
                else
                {
                    uiRequireItemAmounts.Show();
                    uiRequireItemAmounts.Data = ItemCraft.CacheCraftRequirements;
                }
            }

            if (uiTextRequireGold != null)
            {
                if (CraftingItem == null)
                    uiTextRequireGold.text = string.Format(requireGoldFormat, "0", "0", LanguageManager.GetText(UILocaleKeys.UI_REQUIRE_GOLD.ToString()));
                else
                {
                    int currentAmount = 0;
                    if (owningCharacter != null)
                        currentAmount = owningCharacter.Gold;
                    uiTextRequireGold.text = string.Format(
                        currentAmount >= ItemCraft.requireGold ? requireGoldFormat : requireGoldNotEnoughFormat,
                        currentAmount.ToString("N0"), ItemCraft.requireGold.ToString("N0"),
                        LanguageManager.GetText(UILocaleKeys.UI_REQUIRE_GOLD.ToString()));
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
