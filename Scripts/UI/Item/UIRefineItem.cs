using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public partial class UIRefineItem : BaseUICharacterItemByIndex
    {
        public Item EquipmentItem { get { return CharacterItem != null ? CharacterItem.GetEquipmentItem() : null; } }
        public bool CanRefine { get { return EquipmentItem != null && Level < EquipmentItem.MaxLevel; } }
        public ItemRefineLevel RefineLevel { get { return EquipmentItem.itemRefineInfo.levels[Level - 1]; } }

        [Header("Format for UI Refine Item")]
        [Tooltip("Require Gold Format => {0} = {Current Amount}, {1} = {Target Amount}, {2} = {Required Gold Label}")]
        public string requireGoldFormat = "{2}: {0}/{1}";
        [Tooltip("Require Gold Format => {0} = {Current Amount}, {1} = {Target Amount}, {2} = {Required Gold Label}")]
        public string requireGoldNotEnoughFormat = "{2}: <color=red>{0}/{1}</color>";
        [Tooltip("Success Rate Format => {0} = {Rate}, {1} = {Success Rate Label}")]
        public string successRateFormat = "{1}: {0}%";
        [Tooltip("Refining Level Format => {0} = {Refining Level}, {1} = {Refining To Label}")]
        public string refiningLevelFormat = "{1}: +{0}";

        [Header("UI Elements for UI Refine Item")]
        // TODO: This is deprecated
        [HideInInspector]
        public UICharacterItem uiRefiningItem;
        public UIItemAmounts uiRequireItemAmounts;
        public TextWrapper uiTextRequireGold;
        public TextWrapper uiTextSuccessRate;
        public TextWrapper uiTextRefiningLevel;

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

            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;

            if (uiCharacterItem != null)
            {
                if (CharacterItem == null)
                {
                    // Hide if item is null
                    uiCharacterItem.Hide();
                }
                else
                {
                    uiCharacterItem.Setup(new CharacterItemTuple(CharacterItem, Level, InventoryType), OwningCharacter, IndexOfData);
                    uiCharacterItem.Show();
                }
            }

            if (uiRequireItemAmounts != null)
            {
                if (!CanRefine)
                {
                    // Hide if item is null
                    uiRequireItemAmounts.Hide();
                }
                else
                {
                    uiRequireItemAmounts.showAsRequirement = true;
                    uiRequireItemAmounts.Show();
                    uiRequireItemAmounts.Data = RefineLevel.RequireItems;
                }
            }

            if (uiTextRequireGold != null)
            {
                if (!CanRefine)
                {
                    uiTextRequireGold.text = string.Format(
                        requireGoldFormat,
                        "0",
                        "0",
                        LanguageManager.GetText(UILocaleKeys.UI_LABEL_REQUIRE_GOLD.ToString()));
                }
                else
                {
                    int currentAmount = 0;
                    if (owningCharacter != null)
                        currentAmount = owningCharacter.Gold;
                    uiTextRequireGold.text = string.Format(
                        currentAmount >= RefineLevel.RequireGold ? requireGoldFormat : requireGoldNotEnoughFormat,
                        currentAmount.ToString("N0"), RefineLevel.RequireGold.ToString("N0"),
                        LanguageManager.GetText(UILocaleKeys.UI_LABEL_REQUIRE_GOLD.ToString()));
                }
            }

            if (uiTextSuccessRate != null)
            {
                if (!CanRefine)
                {
                    uiTextSuccessRate.text = string.Format(
                        successRateFormat,
                        "0.00",
                        LanguageManager.GetText(UILocaleKeys.UI_LABEL_REFINE_SUCCESS_RATE.ToString()));
                }
                else
                {
                    uiTextSuccessRate.text = string.Format(
                        successRateFormat,
                        (RefineLevel.SuccessRate * 100f).ToString("N2"),
                        LanguageManager.GetText(UILocaleKeys.UI_LABEL_REFINE_SUCCESS_RATE.ToString()));
                }
            }
            if (uiTextRefiningLevel != null)
            {
                if (!CanRefine)
                {
                    uiTextRefiningLevel.text = string.Format(
                        refiningLevelFormat,
                        (Level - 1).ToString("N0"),
                        LanguageManager.GetText(UILocaleKeys.UI_LABEL_REFINING_LEVEL.ToString()));
                }
                else
                {
                    uiTextRefiningLevel.text = string.Format(
                        refiningLevelFormat,
                        Level.ToString("N0"),
                        LanguageManager.GetText(UILocaleKeys.UI_LABEL_REFINING_LEVEL.ToString()));
                }
            }
        }

        public override void Show()
        {
            base.Show();
            OnUpdateCharacterItems();
        }

        public override void Hide()
        {
            base.Hide();
            Data = new CharacterItemByIndexTuple(InventoryType.NonEquipItems, -1);
        }

        public void OnClickRefine()
        {
            if (IndexOfData < 0)
                return;
            OwningCharacter.RequestRefineItem((byte)InventoryType, (short)IndexOfData);
        }
    }
}
