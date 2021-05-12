using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIItemCraftFormula : UISelectionEntry<ItemCraftFormula>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Craft Duration}")]
        public UILocaleKeySetting formatKeyCraftDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CRAFT_DURATION);
        [Tooltip("Format => {0} = {Craft Remains Duration}")]
        public UILocaleKeySetting formatKeyCraftRemainsDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("UI Elements")]
        public TextWrapper uiTextDuration;
        public UIItemCraft uiItemCraft;
        public InputFieldWrapper inputAmount;

        public UIItemCraftFormulas Manager { get; set; }

        protected override void UpdateData()
        {
            if (uiTextDuration != null)
            {
                uiTextDuration.text = string.Format(
                    LanguageManager.GetText(formatKeyCraftDuration),
                    Data.CraftDuration.ToString("N0"));
            }

            if (uiItemCraft != null)
            {
                if (Data == null)
                {
                    uiItemCraft.Hide();
                }
                else
                {
                    uiItemCraft.Show();
                    uiItemCraft.Data = Data.ItemCraft;
                }
            }
        }

        public void OnClickAppend()
        {
            short amount;
            if (inputAmount == null || !short.TryParse(inputAmount.text, out amount))
                amount = 1;
            GameInstance.PlayingCharacterEntity.CallServerAppendCraftingQueueItem(Manager.Source.ObjectId, Data.DataId, amount);
        }
    }
}
