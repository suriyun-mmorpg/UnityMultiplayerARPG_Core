using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UICraftingQueueItem : UISelectionEntry<CraftingQueueItem>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Amount}")]
        public UILocaleKeySetting formatKeyAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Craft Duration}")]
        public UILocaleKeySetting formatKeyCraftDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CRAFT_DURATION);
        [Tooltip("Format => {0} = {Craft Remains Duration}")]
        public UILocaleKeySetting formatKeyCraftRemainsDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("UI Elements")]
        public TextWrapper uiTextAmount;
        public TextWrapper uiTextDuration;
        public TextWrapper uiTextRemainsDuration;
        public Image imageDurationGage;
        public UIItemCraft uiItemCraft;
        public InputFieldWrapper inputAmount;

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

            if (uiTextAmount != null)
            {
                uiTextAmount.text = string.Format(
                    LanguageManager.GetText(formatKeyAmount),
                    Data.amount.ToString("N0"));
            }

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

            if (uiItemCraft != null)
            {
                if (formula == null)
                {
                    uiItemCraft.Hide();
                }
                else
                {
                    uiItemCraft.Show();
                    uiItemCraft.Data = formula.ItemCraft;
                }
            }
        }

        public void OnClickChange()
        {
            short amount;
            if (inputAmount == null || !short.TryParse(inputAmount.text, out amount))
                amount = 1;
            // TODO: Implement this
        }

        public void OnClickCancel()
        {
            // TODO: Implement this
        }
    }
}
