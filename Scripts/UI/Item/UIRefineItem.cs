using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public partial class UIRefineItem : UIBaseOwningCharacterItem
    {
        public IEquipmentItem EquipmentItem { get { return CharacterItem != null ? CharacterItem.GetEquipmentItem() : null; } }
        public bool CanRefine { get; private set; }
        public bool ReachedMaxLevel { get; private set; }

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

            CanRefine = false;
            ReachedMaxLevel = false;
            ItemRefineLevel? refineLevel = null;
            if (!characterItem.IsEmptySlot())
            {
                GameMessage.Type gameMessageType = GameMessage.Type.CannotRefine;
                CanRefine = EquipmentItem != null && characterItem.GetItem().CanRefine(OwningCharacter, Level, out gameMessageType);
                if (CanRefine)
                {
                    refineLevel = EquipmentItem.ItemRefine.levels[Level - 1];
                }
                else
                {
                    switch (gameMessageType)
                    {
                        case GameMessage.Type.RefineItemReachedMaxLevel:
                            ReachedMaxLevel = true;
                            break;
                        case GameMessage.Type.NotEnoughGold:
                        case GameMessage.Type.NotEnoughItems:
                            refineLevel = EquipmentItem.ItemRefine.levels[Level - 1];
                            break;
                    }
                }
            }

            if (uiCharacterItem != null)
            {
                if (characterItem.IsEmptySlot())
                {
                    uiCharacterItem.Hide();
                }
                else
                {
                    uiCharacterItem.Setup(new UICharacterItemData(characterItem, InventoryType), OwningCharacter, IndexOfData);
                    uiCharacterItem.Show();
                }
            }


            if (uiRequireItemAmounts != null)
            {
                if (!refineLevel.HasValue || refineLevel.Value.CacheRequireItems.Count == 0)
                {
                    uiRequireItemAmounts.Hide();
                }
                else
                {
                    uiRequireItemAmounts.showAsRequirement = true;
                    uiRequireItemAmounts.Show();
                    uiRequireItemAmounts.Data = refineLevel.Value.CacheRequireItems;
                }
            }

            if (uiTextRequireGold != null)
            {
                if (!refineLevel.HasValue)
                {
                    uiTextRequireGold.text = string.Format(
                        LanguageManager.GetText(formatKeyRequireGold),
                        "0",
                        "0");
                }
                else
                {
                    uiTextRequireGold.text = string.Format(
                        OwningCharacter.Gold >= refineLevel.Value.RequireGold ?
                            LanguageManager.GetText(formatKeyRequireGold) :
                            LanguageManager.GetText(formatKeyRequireGoldNotEnough),
                        OwningCharacter.Gold.ToString("N0"),
                        refineLevel.Value.RequireGold.ToString("N0"));
                }
            }

            if (uiTextSuccessRate != null)
            {
                if (!refineLevel.HasValue)
                {
                    uiTextSuccessRate.text = string.Format(
                        LanguageManager.GetText(formatKeySuccessRate),
                        "0.00");
                }
                else
                {
                    uiTextSuccessRate.text = string.Format(
                        LanguageManager.GetText(formatKeySuccessRate),
                        (refineLevel.Value.SuccessRate * 100).ToString("N2"));
                }
            }

            if (uiTextRefiningLevel != null)
            {
                if (!refineLevel.HasValue)
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
            Data = new UIOwningCharacterItemData(InventoryType.NonEquipItems, -1);
        }

        public void OnClickRefine()
        {
            if (CharacterItem.IsEmptySlot())
                return;
            activated = true;
            activeItemId = CharacterItem.id;
            OwningCharacter.CallServerRefineItem(InventoryType, (short)IndexOfData);
        }
    }
}
