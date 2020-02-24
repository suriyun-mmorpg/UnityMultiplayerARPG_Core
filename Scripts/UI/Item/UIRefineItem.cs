using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public partial class UIRefineItem : BaseUICharacterItemByIndex
    {
        public IEquipmentItem EquipmentItem { get { return CharacterItem != null ? CharacterItem.GetEquipmentItem() : null; } }
        public bool CanRefine { get { return EquipmentItem != null && Level < EquipmentItem.MaxLevel; } }
        public ItemRefineLevel RefineLevel { get { return EquipmentItem.ItemRefine.levels[Level - 1]; } }
        
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRequireGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD);
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRequireGoldNotEnough = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD_NOT_ENOUGH);
        [Tooltip("Format => {0} = {Rate * 100}")]
        public UILocaleKeySetting formatKeySuccessRate = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REFINE_SUCCESS_RATE);
        [Tooltip("Format => {0} = {Refining Level}")]
        public UILocaleKeySetting formatKeyRefiningLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REFINING_LEVEL);

        [Header("UI Elements for UI Refine Item")]
        // TODO: This is deprecated
        [HideInInspector]
        public UICharacterItem uiRefiningItem;
        public UIItemAmounts uiRequireItemAmounts;
        public TextWrapper uiTextRequireGold;
        public TextWrapper uiTextSuccessRate;
        public TextWrapper uiTextRefiningLevel;

        protected bool activated;
        protected string activeItemId;

        protected override void Awake()
        {
            base.Awake();
            if (uiCharacterItem == null && uiRefiningItem != null)
                uiCharacterItem = uiRefiningItem;
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (uiCharacterItem == null && uiRefiningItem != null)
            {
                uiCharacterItem = uiRefiningItem;
                EditorUtility.SetDirty(this);
            }
#endif
        }

        public void OnUpdateCharacterItems()
        {
            if (!IsVisible())
                return;

            if (activated && (CharacterItem.IsEmptySlot() || !CharacterItem.id.Equals(activeItemId)))
            {
                // Item's ID is difference to active item ID, so the item may be destroyed
                // So clear data
                Data = new UICharacterItemByIndexData(InventoryType.NonEquipItems, -1);
                return;
            }

            if (uiCharacterItem != null)
            {
                if (CharacterItem.IsEmptySlot())
                {
                    uiCharacterItem.Hide();
                }
                else
                {
                    uiCharacterItem.Setup(new UICharacterItemData(CharacterItem, Level, InventoryType), base.OwningCharacter, IndexOfData);
                    uiCharacterItem.Show();
                }
            }

            if (uiRequireItemAmounts != null)
            {
                if (!CanRefine || RefineLevel.CacheRequireItems.Count == 0)
                {
                    uiRequireItemAmounts.Hide();
                }
                else
                {
                    uiRequireItemAmounts.showAsRequirement = true;
                    uiRequireItemAmounts.Show();
                    uiRequireItemAmounts.Data = RefineLevel.CacheRequireItems;
                }
            }

            if (uiTextRequireGold != null)
            {
                if (!CanRefine)
                {
                    uiTextRequireGold.text = string.Format(
                        LanguageManager.GetText(formatKeyRequireGold),
                        "0",
                        "0");
                }
                else
                {
                    int currentAmount = 0;
                    if (OwningCharacter != null)
                        currentAmount = OwningCharacter.Gold;
                    uiTextRequireGold.text = string.Format(
                        currentAmount >= RefineLevel.RequireGold ?
                            LanguageManager.GetText(formatKeyRequireGold) :
                            LanguageManager.GetText(formatKeyRequireGoldNotEnough),
                        currentAmount.ToString("N0"),
                        RefineLevel.RequireGold.ToString("N0"));
                }
            }

            if (uiTextSuccessRate != null)
            {
                if (!CanRefine)
                {
                    uiTextSuccessRate.text = string.Format(
                        LanguageManager.GetText(formatKeySuccessRate),
                        "0.00");
                }
                else
                {
                    uiTextSuccessRate.text = string.Format(
                        LanguageManager.GetText(formatKeySuccessRate),
                        (RefineLevel.SuccessRate * 100).ToString("N2"));
                }
            }

            if (uiTextRefiningLevel != null)
            {
                if (!CanRefine)
                {
                    uiTextRefiningLevel.text = string.Format(
                        LanguageManager.GetText(formatKeyRefiningLevel),
                        (Level - 1).ToString("N0"));
                }
                else
                {
                    uiTextRefiningLevel.text = string.Format(
                        LanguageManager.GetText(formatKeyRefiningLevel),
                        Level.ToString("N0"));
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
            Data = new UICharacterItemByIndexData(InventoryType.NonEquipItems, -1);
        }

        public void OnClickRefine()
        {
            if (CharacterItem.IsEmptySlot())
                return;
            activated = true;
            activeItemId = CharacterItem.id;
            OwningCharacter.RequestRefineItem(InventoryType, (short)IndexOfData);
        }
    }
}
