using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public partial class UIRepairItem : UIBaseOwningCharacterItem
    {
        public IEquipmentItem EquipmentItem { get { return CharacterItem != null ? CharacterItem.GetEquipmentItem() : null; } }
        public bool CanRepair { get; private set; }

        [Header("String Formats")]
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRequireGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD);
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRequireGoldNotEnough = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD_NOT_ENOUGH);
        [Tooltip("Format => {0} = {Current Durability}, {1} = {Max Durability}")]
        public UILocaleKeySetting formatKeyDurability = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_ITEM_DURABILITY);

        [Header("UI Elements for UI Repair Item")]
        public TextWrapper uiTextRequireGold;
        public TextWrapper uiTextDurability;

        protected bool activated;
        protected string activeItemId;

        public override void OnUpdateCharacterItems()
        {
            if (!IsVisible())
                return;

            // Store data to variable so it won't lookup for data from property again
            CharacterItem characterItem = CharacterItem;

            if (activated && (characterItem.IsEmptySlot() || !characterItem.id.Equals(activeItemId)))
            {
                // Item's ID is difference to active item ID, so the item may be destroyed
                // So clear data
                Data = new UIOwningCharacterItemData(InventoryType.NonEquipItems, -1);
                return;
            }

            CanRepair = false;
            float maxDurability = 0f;
            ItemRepairPrice itemRepairPrice = default(ItemRepairPrice);
            if (!characterItem.IsEmptySlot())
                CanRepair = EquipmentItem != null && characterItem.GetItem().CanRepair(GameInstance.PlayingCharacter, characterItem.durability, out maxDurability, out itemRepairPrice);

            if (uiCharacterItem != null)
            {
                if (characterItem.IsEmptySlot())
                {
                    uiCharacterItem.Hide();
                }
                else
                {
                    uiCharacterItem.Setup(new UICharacterItemData(characterItem, InventoryType), GameInstance.PlayingCharacter, IndexOfData);
                    uiCharacterItem.Show();
                }
            }

            if (uiTextRequireGold != null)
            {
                if (maxDurability <= 0)
                {
                    uiTextRequireGold.text = string.Format(
                        LanguageManager.GetText(formatKeyRequireGold),
                        "0",
                        "0");
                }
                else
                {
                    uiTextRequireGold.text = string.Format(
                        GameInstance.PlayingCharacter.Gold >= itemRepairPrice.RequireGold ?
                            LanguageManager.GetText(formatKeyRequireGold) :
                            LanguageManager.GetText(formatKeyRequireGoldNotEnough),
                        GameInstance.PlayingCharacter.Gold.ToString("N0"),
                        itemRepairPrice.RequireGold.ToString("N0"));
                }
            }

            if (uiTextDurability != null)
            {
                if (maxDurability <= 0)
                {
                    uiTextDurability.text = string.Format(
                        LanguageManager.GetText(formatKeyDurability),
                        "0",
                        "0");
                }
                else
                {
                    uiTextDurability.text = string.Format(
                        LanguageManager.GetText(formatKeyDurability),
                        characterItem.durability.ToString("N0"),
                        maxDurability.ToString("N0"));
                }
            }
        }

        public override void Show()
        {
            base.Show();
            activated = false;
            OnUpdateCharacterItems();
        }

        public override void Hide()
        {
            base.Hide();
            Data = new UIOwningCharacterItemData(InventoryType.NonEquipItems, -1);
        }

        public void OnClickRepair()
        {
            if (CharacterItem.IsEmptySlot())
                return;
            activated = true;
            activeItemId = CharacterItem.id;
            GameInstance.ClientInventoryHandlers.RequestRepairItem(new RequestRepairItemMessage()
            {
                inventoryType = InventoryType,
                index = (short)IndexOfData,
            }, ClientInventoryActions.ResponseRepairItem);
        }
    }
}
