using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UICraftingItem : UISelectionEntry<CraftingItem>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Title}")]
        public UILocaleKeySetting formatKeyTitle = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Craft Duration}")]
        public UILocaleKeySetting formatKeyCraftDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CRAFT_DURATION);
        [Tooltip("Format => {0} = {Craft Remains Duration}")]
        public UILocaleKeySetting formatKeyCraftRemainsDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("UI Elements")]
        public TextWrapper uiTextTitle;
        public Image imageIcon;
        public TextWrapper uiTextDuration;
        public TextWrapper uiTextRemainsDuration;
        public Image imageDurationGage;
        public UIItemCraft uiCraftItem;

        protected float craftRemainsDuration;

        protected override void OnDisable()
        {
            base.OnDisable();
            craftRemainsDuration = 0f;
        }

        protected override void Update()
        {
            base.Update();

            if (craftRemainsDuration <= 0f)
            {
                craftRemainsDuration = Data.craftRemainsDuration;
                if (craftRemainsDuration <= 1f)
                    craftRemainsDuration = 0f;
            }

            if (craftRemainsDuration > 0f)
            {
                craftRemainsDuration -= Time.deltaTime;
                if (craftRemainsDuration <= 0f)
                    craftRemainsDuration = 0f;
            }
            else
                craftRemainsDuration = 0f;

            // Update UIs
            float craftDuration = 0;

            ItemCraftFormula formula;
            if (GameInstance.ItemCraftFormulas.TryGetValue(Data.dataId, out formula))
                craftDuration = formula.CraftDuration;

            if (uiTextDuration != null)
            {
                uiTextDuration.text = string.Format(
                    LanguageManager.GetText(formatKeyCraftDuration),
                    craftDuration.ToString("N0"));
            }

            if (uiTextRemainsDuration != null)
            {
                uiTextRemainsDuration.SetGameObjectActive(craftRemainsDuration > 0);
                uiTextRemainsDuration.text = string.Format(
                    LanguageManager.GetText(formatKeyCraftRemainsDuration),
                    craftRemainsDuration.ToString("N0"));
            }

            if (imageDurationGage != null)
                imageDurationGage.fillAmount = craftDuration <= 0 ? 0 : craftRemainsDuration / craftDuration;
        }

        protected override void UpdateData()
        {
            ItemCraftFormula formula;
            GameInstance.ItemCraftFormulas.TryGetValue(Data.dataId, out formula);

            // Update remains duration
            craftRemainsDuration = Data.craftRemainsDuration;

            if (uiTextTitle != null)
            {
                uiTextTitle.text = string.Format(
                    LanguageManager.GetText(formatKeyTitle),
                    formula == null ? LanguageManager.GetUnknowTitle() : formula.Title);
            }

            if (imageIcon != null)
            {
                Sprite iconSprite = formula == null ? null : formula.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
            }

            if (uiCraftItem != null)
            {
                if (formula == null)
                {
                    uiCraftItem.Hide();
                }
                else
                {
                    uiCraftItem.Show();
                    uiCraftItem.Data = formula.ItemCraft;
                }
            }
        }
    }
}
