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
        /// <summary>
        /// Format => {0} = {Success Rate Label}, {1} = {Rate * 100}
        /// </summary>
        [Tooltip("Format => {0} = {Success Rate Label}, {1} = {Rate * 100}")]
        public string formatSuccessRate = "{0}: {1}%";
        /// <summary>
        /// Format => {0} = {Refining Level Label}, {1} = {Refining Level}
        /// </summary>
        [Tooltip("Format => {0} = {Refining Level Label}, {1} = {Refining Level}")]
        public string formatRefiningLevel = "{0}: +{1}";

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
                        currentAmount >= RefineLevel.RequireGold ? formatRequireGold : formatRequireGoldNotEnough,
                        LanguageManager.GetText(UILocaleKeys.UI_LABEL_REQUIRE_GOLD.ToString()),
                        currentAmount.ToString("N0"),
                        RefineLevel.RequireGold.ToString("N0"));
                }
            }

            if (uiTextSuccessRate != null)
            {
                if (!CanRefine)
                {
                    uiTextSuccessRate.text = string.Format(
                        formatSuccessRate,
                        LanguageManager.GetText(UILocaleKeys.UI_LABEL_REFINE_SUCCESS_RATE.ToString()),
                        "0.00");
                }
                else
                {
                    uiTextSuccessRate.text = string.Format(
                        formatSuccessRate,
                        LanguageManager.GetText(UILocaleKeys.UI_LABEL_REFINE_SUCCESS_RATE.ToString()),
                        (RefineLevel.SuccessRate * 100f).ToString("N2"));
                }
            }
            if (uiTextRefiningLevel != null)
            {
                if (!CanRefine)
                {
                    uiTextRefiningLevel.text = string.Format(
                        formatRefiningLevel,
                        LanguageManager.GetText(UILocaleKeys.UI_LABEL_REFINING_LEVEL.ToString()),
                        (Level - 1).ToString("N0"));
                }
                else
                {
                    uiTextRefiningLevel.text = string.Format(
                        formatRefiningLevel,
                        LanguageManager.GetText(UILocaleKeys.UI_LABEL_REFINING_LEVEL.ToString()),
                        Level.ToString("N0"));
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
